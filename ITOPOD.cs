using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace fasterPace
{
    [HarmonyPatch]
    internal class ITOPOD
    {
        internal const float GenSpeed = (GeneralBuffs.GenSpeed * 2);
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "awardHighestLevelPP")]
        private static bool awardHighestLevelPP(ItopodPerkController __instance, int level)
        {
            if (level % 10 != 0)
                return false;

            int tier = (level - 1) / 100;
            int perkPoints = 10 * (tier + 1);

            if (level % 100 == 0)
                perkPoints *= 10;

            const long PROGRESS_PER_PERK = 100000 * (long)GenSpeed;

            // was: double bonus = __instance.totalPPBonus();
            double bonus = __instance.totalPPBonus(false);

            long add = (long)Math.Ceiling(perkPoints * (double)PROGRESS_PER_PERK * bonus);
            __instance.addProgress(add);

            if (add < 1L)
                add = 1L;

            character.adventureController.log.AddEvent($"You Reached floor {level} of the I.T.O.P.O.D for the first time!");
            character.adventureController.log.AddEvent(
                $"You've been awarded a one-time bonus of {character.display(add)} progress to your next Perk!");

            return false;
        }

        /// <summary>
    /// Buff guaranteed poop gain (progress threshold).
    /// </summary>
    [HarmonyPatch(typeof(ItopodPerkController), nameof(ItopodPerkController.poopThreshold))]
    internal static class Patch_ItopodPerkController_PoopThreshold_FP
    {
        private const double BASE_THRESHOLD = 9000d;
        private static bool _loggedOnce;

        [HarmonyPrefix]
        private static bool Prefix(ref long __result)
        {
            double gs = GeneralBuffs.GenSpeed;
            if (gs < 1d) gs = 1d;

            __result = (long)Math.Ceiling(BASE_THRESHOLD / gs);
            if (__result < 1L) __result = 1L;

            if (!_loggedOnce)
            {
                _loggedOnce = true;
                Debug.Log($"[FP] poopThreshold patched: base={BASE_THRESHOLD} gs={gs} -> {__result}");
            }

            return false; // skip vanilla (9000)
        }
    }

    /// <summary>
    /// Buff the random "poop on kill" roll by scaling the chance at the roll site.
    /// This avoids fighting other mods and avoids permanent effectPerLevel mutations.
    /// </summary>
    [HarmonyPatch(typeof(LootDrop), "itopodDrop")]
    internal static class Patch_LootDrop_ItopodDrop_PoopChance_FP
    {
        private static bool _loggedOnce;

        [HarmonyPrefix]
        private static void Prefix(Character ___character, ref float __state)
        {
            __state = -1f;

            var c = ___character;
            var itopodState = c?.adventure?.itopod;
            var perkCtrl = c?.adventureController?.itopod;
            if (itopodState == null || perkCtrl == null) return;

            if (itopodState.perkLevel[30] < 1L) return;
            if (perkCtrl.effectPerLevel == null || perkCtrl.effectPerLevel.Count <= 30) return;

            float baseChance = perkCtrl.effectPerLevel[30];
            __state = baseChance;

            float gs = 1f;
            float newChance = Mathf.Min(1f, baseChance * gs);

            perkCtrl.effectPerLevel[30] = newChance;

            if (!_loggedOnce)
            {
                _loggedOnce = true;
                Debug.Log($"[FP] poop chance patched (itopodDrop): base={baseChance} gs={gs} -> {newChance}");
            }
        }

        [HarmonyPostfix]
        private static void Postfix(Character ___character, float __state)
        {
            // restore original chance so we don't permanently mutate the controller
            if (__state < 0f) return;

            var c = ___character;
            var perkCtrl = c?.adventureController?.itopod;
            if (perkCtrl?.effectPerLevel == null || perkCtrl.effectPerLevel.Count <= 30) return;

            perkCtrl.effectPerLevel[30] = __state;
        }
            [HarmonyPatch(typeof(ItopodPerkController), "showTooltip")]
            internal static class Patch_ItopodPerkController_ShowTooltip_PoopText_FP
            {
                private static FieldInfo _cachedTextField;

                [HarmonyPostfix]
                private static void Postfix(ItopodPerkController __instance, int id)
                {
                    if (__instance == null) return;
                    if (id != 30) return; // poop perk

                    // Find the Text field once (first run), cache it
                    if (_cachedTextField == null)
                        _cachedTextField = FindFirstTextField(__instance);

                    if (_cachedTextField == null) return;

                    var txt = _cachedTextField.GetValue(__instance) as Text;
                    if (txt == null) return;

                    if (string.IsNullOrEmpty(txt.text)) return;

                    long t = 9000L;
                    try { t = __instance.poopThreshold(); } catch { t = 9000L; }

                    // Replace common representations
                    txt.text = txt.text
                        .Replace("9,000", t.ToString("N0"))
                        .Replace("9000", t.ToString());
                }

                private static FieldInfo FindFirstTextField(ItopodPerkController inst)
                {
                    try
                    {
                        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                        var t = inst.GetType();

                        // Prefer fields with "tool" or "tip" in name first (tooltipText etc.)
                        foreach (var f in t.GetFields(flags))
                        {
                            if (f.FieldType != typeof(Text)) continue;
                            string n = (f.Name ?? "").ToLowerInvariant();
                            if (n.Contains("tool") || n.Contains("tip") || n.Contains("message"))
                                return f;
                        }

                        // Fallback: first Text field
                        foreach (var f in t.GetFields(flags))
                        {
                            if (f.FieldType == typeof(Text))
                                return f;
                        }
                    }
                    catch { }

                    return null;
                }
            }
            [HarmonyPatch(typeof(ItopodPerkController), "Start")]
            internal static class Patch_ItopodPerkController_PoopChance
            {
                private static float _base = -1f;

                [HarmonyPostfix]
                private static void Postfix(ItopodPerkController __instance)
                {
                    if (__instance?.effectPerLevel == null) return;
                    if (__instance.effectPerLevel.Count <= 30) return;

                    if (_base < 0f) _base = __instance.effectPerLevel[30];

                    float gs = Mathf.Max(1f, GeneralBuffs.GenSpeed);
                    __instance.effectPerLevel[30] = Mathf.Min(1f, _base * gs);
                }
            }
            [HarmonyPatch]
            internal static class Patch_Jshep_ImprovedTowerDescription_FixNextPoopLine
            {
                private static MethodBase TargetMethod()
                {
                    var t = AccessTools.TypeByName("jshepler.ngu.mods.ImprovedTowerDescription");
                    if (t == null) return null;
                    return AccessTools.Method(t, "buildAltTooltip");
                }

                [HarmonyPostfix]
                private static void Postfix(ref string __result)
                {
                    if (string.IsNullOrEmpty(__result)) return;

                    var c = GetCharacter();
                    if (c == null) return;

                    long threshold = 9000;
                    try { threshold = c.adventureController.itopod.poopThreshold(); } catch { threshold = 9000; }

                    long progress = GetLongField(c.adventure.itopod, "poopProgress");
                    // If field name differs, try common alternates:
                    if (progress == long.MinValue) progress = GetLongField(c.adventure.itopod, "poopProg");
                    if (progress == long.MinValue) progress = GetLongField(c.adventure.itopod, "poopCounter");
                    if (progress == long.MinValue) return;

                    long killsToNext = threshold - progress;
                    if (killsToNext < 0) killsToNext = 0;

                    // Pull “Seconds per kill” from the tooltip itself so we match jshepler’s current calcs
                    double secondsPerKill = ExtractSecondsPerKill(__result);
                    string timeStr = "";
                    if (secondsPerKill > 0.000001)
                        timeStr = " in " + FormatDuration(secondsPerKill * killsToNext);

                    // Rewrite the single line
                    __result = ReplaceLine(__result, "Kills to next Poop:", "Kills to next Poop: " + killsToNext + timeStr);
                }

                private static Character GetCharacter()
                {
                    try
                    {
                        var f = AccessTools.Field(typeof(GeneralBuffs), "character");
                        return f != null ? (Character)f.GetValue(null) : null;
                    }
                    catch { return null; }
                }

                private static long GetLongField(object obj, string fieldName)
                {
                    try
                    {
                        if (obj == null) return long.MinValue;
                        var f = AccessTools.Field(obj.GetType(), fieldName);
                        if (f == null) return long.MinValue;
                        object v = f.GetValue(obj);
                        if (v is long l) return l;
                        if (v is int i) return i;
                    }
                    catch { }
                    return long.MinValue;
                }

                private static double ExtractSecondsPerKill(string tooltip)
                {
                    // Looks like: "Seconds per kill: 2.1s (optimal)"
                    // We'll parse the number before the 's'
                    try
                    {
                        var lines = tooltip.Split('\n');
                        foreach (var line in lines)
                        {
                            if (!line.StartsWith("Seconds per kill:", StringComparison.Ordinal)) continue;

                            int colon = line.IndexOf(':');
                            if (colon < 0) continue;
                            string rest = line.Substring(colon + 1).Trim();

                            // rest starts with number like "2.1s"
                            int sIdx = rest.IndexOf('s');
                            if (sIdx <= 0) continue;

                            string num = rest.Substring(0, sIdx).Trim();
                            double val;
                            if (double.TryParse(num, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out val))
                                return val;

                            // fallback for locales using comma
                            num = num.Replace(',', '.');
                            if (double.TryParse(num, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out val))
                                return val;
                        }
                    }
                    catch { }
                    return 0;
                }

                private static string ReplaceLine(string text, string startsWith, string newLine)
                {
                    var lines = text.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith(startsWith, StringComparison.Ordinal))
                        {
                            lines[i] = newLine;
                            break;
                        }
                    }
                    return string.Join("\n", lines);
                }

                private static string FormatDuration(double totalSeconds)
                {
                    double secs = totalSeconds;
                    if (secs < 0) secs = 0;

                    long s = (long)Math.Round(secs);
                    long days = s / 86400; s %= 86400;
                    long hours = s / 3600; s %= 3600;
                    long mins = s / 60; s %= 60;

                    if (days > 0) return string.Format("{0}:{1:00}:{2:00}:{3:00}", days, hours, mins, s);
                    if (hours > 0) return string.Format("{0}:{1:00}:{2:00}", hours, mins, s);
                    return string.Format("{0}:{1:00}", mins, s);
                }
            }
        }

        [HarmonyPatch(typeof(ItopodPerkController), "addPoopProgress")]
        internal static class Patch_ItopodPerkController_AddPoopProgress_FP
        {
            private const double BASE_THRESHOLD = 9000d;

            [HarmonyPrefix]
            private static bool Prefix(ItopodPerkController __instance, long amount, ref long __result)
            {
                var c = __instance?.character;
                if (c?.adventure?.itopod == null)
                {
                    __result = 0L;
                    return false;
                }

                // Your scaling rule (3x faster when GenSpeed=3)
                double gs = GeneralBuffs.GenSpeed;
                if (gs < 1d) gs = 1d;

                long threshold = (long)Math.Ceiling(BASE_THRESHOLD / gs);
                if (threshold < 1L) threshold = 1L;

                // Vanilla logic, but using our threshold instead of __instance.poopThreshold()
                c.adventure.itopod.poopProgress += amount;

                long gained = c.adventure.itopod.poopProgress / threshold;
                c.adventure.itopod.poopProgress = c.adventure.itopod.poopProgress % threshold;

                if (gained >= 1L)
                {
                    c.arbitrary.poop1Count += (int)gained;
                    c.adventureController.log.AddEvent("You gained " + gained + " Poop!");
                    __instance.updateText();
                    __result = gained;
                    return false;
                }

                __instance.updateText();
                __result = 0L;
                return false;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "progressGained")]
        private static bool progressGained(long itopodLevel, ItopodPerkController __instance, ref long __result)
        {
            float diffFactor = 1f;
            switch (character.settings.rebirthDifficulty)
            {
                case difficulty.normal: diffFactor = 1f; break;
                case difficulty.evil: diffFactor = 2f; break;
                case difficulty.sadistic: diffFactor = 1.9f; break;
            }

            float baseMult = GenSpeed * diffFactor;
            double pp = __instance.totalPPBonus();

            float result;

            if (character.settings.rebirthDifficulty == difficulty.normal)
            {
                result =
                    (200f * baseMult) +
                    ((float)itopodLevel * GenSpeed);
            }
            else if (character.settings.rebirthDifficulty == difficulty.evil)
            {
                result =
                    (700f * baseMult) +
                    ((float)itopodLevel * GenSpeed);
            }
            else // sadistic
            {
                result =
                    (2000f * baseMult) +
                    ((float)itopodLevel * GenSpeed) +
                    ((float)__instance.totalBasePPBonus() * baseMult * 2);
            }

            __result = (long)(result * (float)pp);
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "baseProgressGained")]
        private static bool baseProgressGained(long itopodLevel, ItopodPerkController __instance, ref long __result)
        {
            float diffFactor = 1f;
            switch (__instance.character.settings.rebirthDifficulty)
            {
                case difficulty.normal: diffFactor = 1f; break;
                case difficulty.evil: diffFactor = 2f; break;
                case difficulty.sadistic: diffFactor = 1.9f; break;
            }

            float baseMult = GenSpeed * diffFactor;
            float result;

            if (__instance.character.settings.rebirthDifficulty == difficulty.normal)
            {
                result =
                    (200f * baseMult) +
                    ((float)itopodLevel * GenSpeed);
            }
            else if (__instance.character.settings.rebirthDifficulty == difficulty.evil)
            {
                result =
                    (700f * baseMult) +
                    ((float)itopodLevel * GenSpeed);
            }
            else // sadistic
            {
                result =
                    (2000f * baseMult) +
                    ((float)itopodLevel * GenSpeed) +
                    ((float)__instance.totalBasePPBonus() * baseMult * 2);
            }

            __result = (long)result;
            return false;
        }
    }
}
