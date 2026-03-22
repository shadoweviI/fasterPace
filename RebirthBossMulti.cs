using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;

namespace fasterPace


{
    [HarmonyPatch]
    internal class RebirthBossMulti
    {
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }
        private static void RefreshRebirthUI()
        {
            if (character == null) return;

            // Try common member names across versions
            object rc =
                GetMember(character, "rebirthController") ??
                GetMember(character, "rebirthMenu") ??
                GetMember(character, "rebirth") ??
                GetMember(character, "rebirthUI") ??
                GetMember(character, "rebirthCtrl");

            if (rc == null) return;

            var t = rc.GetType();
            (t.GetMethod("updateMenu", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             ?? t.GetMethod("updateRebirthMenu", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             ?? t.GetMethod("updateText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            ?.Invoke(rc, null);
        }

        private static object GetMember(object obj, string name)
        {
            var type = obj.GetType();
            return (object)type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj, null)
                ?? type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
        }
        [HarmonyPrefix, HarmonyPatch(typeof(BossController), "advanceBoss")]
        private static bool advanceBoss(BossController __instance)
        {
            character.buttons.updateButtons();
            __instance.rewardExp();
            character.bossID++;
            __instance.bossTextScrollbar.value = 1f;
            if (character.bossID == 30)
            {
                character.timeMachineController.setBankedLevels();
            }

            __instance.inventoryController.updateBonuses();
            character.augmentsController.updateMenu();
            character.stats.bossesDefeated++;
            character.adventureController.constructDropdown();
            if (character.bossID > character.highestBoss && character.settings.rebirthDifficulty == difficulty.normal)
            {
                character.highestBoss = character.bossID;
                character.stats.highestBoss = character.bossID;
            }

            if (character.bossID > character.highestHardBoss && character.settings.rebirthDifficulty == difficulty.evil)
            {
                character.highestHardBoss = character.bossID;
            }

            if (character.bossID > character.highestSadisticBoss && character.settings.rebirthDifficulty == difficulty.sadistic)
            {
                character.highestSadisticBoss = character.bossID;
            }

            if (character.bossID > character.currentHighestBoss)
            {
                character.currentHighestBoss = character.bossID;
            }

            if (character.settings.rebirthDifficulty == difficulty.normal)
            {
                character.bossMulti *= 2f + (GeneralBuffs.GenSpeed * 0.02f);
                RefreshRebirthUI();
            }

            else if (character.settings.rebirthDifficulty == difficulty.evil)
            {
                character.bossMulti *= 1.5 + (GeneralBuffs.GenSpeed * 0.033333333334f);
                RefreshRebirthUI();
            }
            else if (character.settings.rebirthDifficulty == difficulty.sadistic)
            {
                character.bossMulti *= __instance.sadisticBossMultiplier();
                RefreshRebirthUI();
            }

            if (character.bossID >= 4)
            {
                character.settings.inventoryOn = true;
            }

            if (character.bossID == 37)
            {
                character.magic.unlockMagic();
            }

            if (character.bossID == 58)
            {
                character.challenges.unlocked = true;
            }

            if (character.bossID > 300)
            {
                __instance.bossName.text = "NONE (YOU KILLED THEM ALL)";
                __instance.bossDesc.text = "You've defeated every single boss! Rebirth to bring them back, or try a higher difficulty rebirth!";
                character.bossAttack = 69.0;
                character.bossDefense = 69.0;
                character.bossRegen = 420.0;
                character.bossCurHP = 420.0;
                character.bossMaxHP = 420.0;
                character.buttons.updateButtons();
                __instance.updateMenu();
                if (character.settings.rebirthDifficulty >= difficulty.sadistic)
                {
                    if (character.itemInfo.findIndexWithID(487) == -1)
                    {
                        character.itemInfo.makeLevelledLoot(487, 100);
                    }

                    character.tooltip.showTooltip("THE END NEARS.");
                }
            }
            else
            {
                character.bossAttack = __instance.boss.bossAttack[character.bossID];
                character.bossDefense = __instance.boss.bossDefense[character.bossID];
                character.bossRegen = __instance.boss.bossRegen[character.bossID];
                character.bossCurHP = __instance.boss.bossCurHP[character.bossID];
                character.bossMaxHP = __instance.boss.bossMaxHP[character.bossID];
                __instance.updateMenu();
                __instance.bossTextScrollbar.value = 1f;
                character.buttons.updateButtons();
            }
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BossController), "sadisticBossMultiplier")]
        private static bool sadisticBossMultiplier(BossController __instance, ref float __result)
        {
            __result = 1.2f + (GeneralBuffs.GenSpeed * 0.05f) + character.adventureController.itopod.sadisticBossMultiplierBonus() + character.beastQuestPerkController.sadisticBossMultiplierBonus() + character.wishesController.sadisticBossMultiplierBonus();
            return false;
        }

        [HarmonyPatch(typeof(Rebirth), nameof(Rebirth.checkSpeedrunSecret))]
        internal static class Patch_CheckSpeedrunSecret_GenSpeed
        {
            [HarmonyPrefix]
            private static bool Prefix(Rebirth __instance)
            {
                var c = __instance?.character;
                if (c == null) return false;

                double mult = (GeneralBuffs.GenSpeed <= 0 ? 1.0 : GeneralBuffs.GenSpeed);

                // Stricter requirement: less real time allowed
                double limit = 1800.0 / mult;

                if (c.bossID >= 37 && c.rebirthTime.totalseconds <= limit)
                {
                    c.settings.speedrunCount++;
                    if (c.settings.speedrunCount >= 3)
                    {
                        c.settings.speedrunCount = 3;
                        if (!c.settings.gotSpeedrunSecret)
                        {
                            c.settings.gotSpeedrunSecret = true;
                            c.addExp(200L);
                            c.energyPower += 1f;
                            __instance.tooltip.showTooltip(
                                "You completed the speedrun 'secret'! Here's 200 EXP and +1 Energy power for your troubles!",
                                5f
                            );
                        }
                    }
                }
                else
                {
                    c.settings.speedrunCount = 0;
                }

                return false; // skip original
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Rebirth), "calculateTimeMulti")]
        private static bool calculateTimeMulti()
        {
            double t = character.rebirthTime.totalseconds * (double)GeneralBuffs.GenSpeed;
            if (t < 60.0)
                character.timeMulti = t / 34359738368.0 / 3600.0;
            else if (t < 120.0)
                character.timeMulti = t / 33554432.0 / 3600.0;
            else if (t < 180.0)
                character.timeMulti = t / 518144.0 / 3600.0;
            else if (t < 240.0)
                character.timeMulti = t / 16192.0 / 3600.0;
            else if (t < 300.0)
                character.timeMulti = t / 2048.0 / 3600.0;
            else if (t < 420.0)
                character.timeMulti = t / 512.0 / 3600.0;
            else if (t < 600.0)
                character.timeMulti = t / 128.0 / 3600.0;
            else if (t < 720.0)
                character.timeMulti = t / 32.0 / 3600.0;
            else if (t < 900.0)
                character.timeMulti = t / 8.0 / 3600.0;
            else if (t < 1800.0)
                character.timeMulti = t / 4.0 / 3600.0;
            else if (t < 3600.0)
                character.timeMulti = t / 2.0 / 3600.0;
            else
                character.timeMulti = 1.0 + t / 172800.0;
            return false; 
        }


        internal static class TrainingGains
        {
            
            public const long BaseGain = (long)GeneralBuffs.GenSpeed*2;          
            public const long ItopodPerk15Gain = (long)GeneralBuffs.GenSpeed*2;
            public const long Quirk17Gain = (long)GeneralBuffs.GenSpeed*2;
            public const long Wish23Gain = (long)GeneralBuffs.GenSpeed*2;
        }

        [HarmonyPatch]
        internal static class Patch_DefenseTraining_LevelUp
        {
            static MethodBase TargetMethod()
                => AccessTools.Method(typeof(DefenseTraining), "levelUp"); 

            [HarmonyPrefix]
            static bool Prefix(DefenseTraining __instance)
            {
                // Read required instance fields
                var character = AccessTools.Field(typeof(DefenseTraining), "character").GetValue(__instance) as Character;
                int id = (int)AccessTools.Field(typeof(DefenseTraining), "id").GetValue(__instance);

                if (character == null) return true; 

                // Reset bar progress like vanilla
                character.training.defenseBarProgress[id] = 0f;

                // Cap guard like vanilla
                if (character.training.defenseTraining[id] >= 9223372036854775805L)
                {
                    character.training.defenseTraining[id] = long.MaxValue;
                    return false;
                }

                // Compute total gain this bar fill
                long gain = 0;

                gain += TrainingGains.BaseGain;

                if (character.adventure.itopod.perkLevel[15] >= 1)
                    gain += TrainingGains.ItopodPerk15Gain;

                if (character.beastQuest.quirkLevel[17] >= 1)
                    gain += TrainingGains.Quirk17Gain;

                if (character.wishes.wishes[23].level >= 1)
                    gain += TrainingGains.Wish23Gain;

                // Apply gain (and keep totals consistent)
                character.training.defenseTraining[id] += gain;
                character.training.totalDefenseLevels += gain;

                // Call updateText()
                AccessTools.Method(typeof(DefenseTraining), "updateText")
                    .Invoke(__instance, null);

                // Unlock logic: vanilla does == (id+1)*5000 and id != 5
                if (character.training.defenseTraining[id] == (id + 1) * 5000 && id != 5)
                {
                    AccessTools.Method(typeof(DefenseTraining), "unlockedText")
                        .Invoke(__instance, null);

                    AccessTools.Method(typeof(DefenseTraining), "checkButtons")
                        .Invoke(__instance, null);

                    // Auto-advance check
                    if (character.purchases.hasAutoAdvance)
                    {
                        var toggleObj = AccessTools.Field(typeof(DefenseTraining), "autoadvanceToggle")
                            .GetValue(__instance);

                        if (toggleObj is Toggle t && t.isOn)
                        {
                            AccessTools.Method(typeof(DefenseTraining), "autoAdvance")
                                .Invoke(__instance, null);
                        }
                    }
                }

                return false; // skip original
            }
        }

        [HarmonyPatch]
        internal static class Patch_OffenseTraining_LevelUp
        {
            static MethodBase TargetMethod()
                => AccessTools.Method(typeof(OffenseTraining), "levelUp"); // public in your snippet

            [HarmonyPrefix]
            static bool Prefix(OffenseTraining __instance)
            {
                var character = AccessTools.Field(typeof(OffenseTraining), "character").GetValue(__instance) as Character;
                int id = (int)AccessTools.Field(typeof(OffenseTraining), "id").GetValue(__instance);

                if (character == null) return true;

                character.training.attackBarProgress[id] = 0f;

                if (character.training.attackTraining[id] >= 9223372036854775805L)
                {
                    character.training.attackTraining[id] = long.MaxValue;
                    return false;
                }

                long gain = 0;

                gain += TrainingGains.BaseGain;

                if (character.adventure.itopod.perkLevel[15] >= 1)
                    gain += TrainingGains.ItopodPerk15Gain;

                if (character.beastQuest.quirkLevel[17] >= 1)
                    gain += TrainingGains.Quirk17Gain;

                if (character.wishes.wishes[23].level >= 1)
                    gain += TrainingGains.Wish23Gain;

                character.training.attackTraining[id] += gain;
                character.training.totalAttackLevels += gain;

                AccessTools.Method(typeof(OffenseTraining), "updateText")
                    .Invoke(__instance, null);

                if (character.training.attackTraining[id] == (id + 1) * 5000)
                {
                    AccessTools.Method(typeof(OffenseTraining), "unlockedText")
                        .Invoke(__instance, null);

                    AccessTools.Method(typeof(OffenseTraining), "checkButtons")
                        .Invoke(__instance, null);

                    if (character.purchases.hasAutoAdvance)
                    {
                        var toggleObj = AccessTools.Field(typeof(OffenseTraining), "autoAdvanceToggle")
                            .GetValue(__instance);

                        if (toggleObj is Toggle t && t.isOn)
                        {
                            AccessTools.Method(typeof(OffenseTraining), "autoAdvance")
                                .Invoke(__instance, null);
                        }
                    }
                }

                return false;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(RebirthPowerDisplay), "difficultyFactor")]
        private static bool RebirthPowerDisplay_difficultyFactor(RebirthPowerDisplay __instance, ref float __result)
        {
            switch (__instance.character.settings.rebirthDifficulty)
            {
                case difficulty.normal:
                    __result = 2.00f + (GeneralBuffs.GenSpeed * 0.02f);
                    return false;

                case difficulty.evil:
                    __result = 1.5f + (GeneralBuffs.GenSpeed * 0.033333333334f);
                    return false;

                case difficulty.sadistic:
                    __result = __instance.character.bossController.sadisticBossMultiplier();
                    return false;

                default:
                    __result = 1.2f;
                    return false;
            }
        }



    }

    internal static class RebirthBossMulti_DisplayOnly
    {
        private static readonly Regex RxFormula = new Regex(
            @"^(?<indent>\s*)(?<base>[0-9]+(?:[.,][0-9]+)?)\s*\^\s*(?<exp>-?[0-9,]+)\s*=\s*(?<rhs>.+)$",
            RegexOptions.Compiled
        );

        [HarmonyPatch(typeof(RebirthPowerDisplay), "Update")]
        internal static class Patch_RebirthPowerDisplay_Update_FixLastRebirthDisplayOnly
        {
            [HarmonyPostfix]
            private static void Postfix(RebirthPowerDisplay __instance)
            {
                var c = __instance?.character;
                var fmt = __instance?.format;
                var textComp = __instance?.rebirthInfoValues;
                if (c == null || fmt == null || textComp == null) return;

                // only on rebirth menu (your original check)
                if (c.menuID != 23) return;

                double old = c.oldBossMulti;
                if (!(old > 0.0) || double.IsNaN(old) || double.IsInfinity(old))
                    return; // if old is already bad, don’t try to “fix” display

                string text = textComp.text;
                if (string.IsNullOrEmpty(text)) return;

                var lines = text.Split('\n');
                if (lines.Length < 3) return;

                // find "last rebirth:" line, edit the NEXT line if it matches "<base> ^ <exp> = <rhs>"
                for (int i = 0; i < lines.Length - 1; i++)
                {
                    if (lines[i].IndexOf("last rebirth:", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    int formulaIdx = i + 1;
                    var m = RxFormula.Match(lines[formulaIdx]);
                    if (!m.Success) return;

                    string indent = m.Groups["indent"].Value;
                    string baseStrShown = m.Groups["base"].Value; // keep whatever the UI already shows
                    string rhsShown = m.Groups["rhs"].Value;      // keep RHS exactly as shown

                    // Parse the base shown (current run) as a fallback only
                    TryParseFlexible(baseStrShown, out double baseShown);
                    if (baseShown <= 1.0000001) baseShown = __instance.difficultyFactor();

                    // We infer a base for LAST rebirth using oldBossMulti + an exponent guess.
                    // Exponent guess: use current bosses defeated-ish as a neighborhood.
                    long expGuess = Math.Max(1L, (long)c.bossID - 1L);

                    // inferred base: exp( ln(old) / expGuess )
                    double baseInferred = Math.Exp(Math.Log(old) / expGuess);

                    // sanity: if inferred base is nonsense, fall back to shown/current base
                    if (double.IsNaN(baseInferred) || double.IsInfinity(baseInferred) || baseInferred <= 1.0000001)
                        baseInferred = baseShown;

                    // Now compute exponent using inferred base (this stops the “two less” when GenSpeed changes)
                    long exp = BestFitExponentFromDisplayed(old, baseInferred);

                    // Clamp just for display safety
                    if (exp < 0) exp = 0;
                    if (exp > 10_000_000L) exp = 10_000_000L;

                    // Display base: keep the UI’s base string (so formatting stays the same),
                    // but exponent is derived from baseInferred (stable across GenSpeed changes).
                    lines[formulaIdx] =
                        $"{indent}{baseStrShown} ^ {exp.ToString("N0", CultureInfo.CurrentCulture)} = {rhsShown}";

                    // Safety: never display negative/absurd exponents
                    if (exp < 0) exp = 0;
                    if (exp > 10_000_000L) exp = 10_000_000L; // display-only clamp, should never hit

                    // Replace ONLY exponent, keep base + rhs formatting intact
                    lines[formulaIdx] =
                        $"{indent}{baseStrShown} ^ {exp.ToString("N0", CultureInfo.CurrentCulture)} = {rhsShown}";

                    textComp.text = string.Join("\n", lines);
                    return;
                }
            }

            // Pick exponent whose base^exp is closest to 'old' in log-space.
            private static long BestFitExponentFromDisplayed(double old, double baseFactor)
            {
                double lnBase = Math.Log(baseFactor);
                double lnOld = Math.Log(old);

                double raw = lnOld / lnBase;

                // Start from nearest integer guess
                long guess = (long)Math.Round(raw, MidpointRounding.AwayFromZero);

                // Search small window to avoid off-by-one/two due to floating drift
                long best = Math.Max(0, guess);
                double bestErr = double.PositiveInfinity;

                for (long e = guess - 3; e <= guess + 3; e++)
                {
                    if (e < 0) continue;
                    double err = Math.Abs(lnOld - e * lnBase);
                    if (err < bestErr)
                    {
                        bestErr = err;
                        best = e;
                    }
                }

                return best;
            }

            private static bool TryParseFlexible(string s, out double value)
            {
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                    return true;

                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    return true;

                // fallback swap '.' <-> ',' if needed
                string swapped = s.Contains(",") && !s.Contains(".") ? s.Replace(',', '.') :
                                 s.Contains(".") && !s.Contains(",") ? s.Replace('.', ',') :
                                 s;

                return double.TryParse(swapped, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            }
        }
    }
}
