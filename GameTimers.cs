using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.TextCore;

namespace fasterPace
{
    [HarmonyPatch]
    internal class GameTimers
    {
        internal const float GenSpeed = GeneralBuffs.GenSpeed;
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "macGuffinBonusTimeFactor")]
        private static bool macGuffinBonusTimeFactor(ref float __result)
        {
            double t = character.rebirthTime.totalseconds * (double)GenSpeed;

            float num;

            if (t < 180.0)
                num = 0f;
            else if (t <= 1800.0)
                num = Mathf.Pow((float)(t / 1800.0), 2f);
            else if (t <= 86400.0)
                num = Mathf.Pow((float)(t / 1800.0), 1f);
            else
                num = 48f * Mathf.Pow((float)(t / 86400.0), 0.4f);

            bool sadisticBoost =
                character.settings.rebirthDifficulty >= difficulty.sadistic &&
                character.allChallenges.trollChallenge.sadisticCompletions() >= 2;

            float scale = sadisticBoost ? 2f : 1f;

            // +20% bonus if Purple Heart is complete
            if (character.inventory.itemList.purpleHeartComplete)
            {
                scale *= 1.2f;
            }

            if (character.inventory.itemList.netherComplete)
            {
                scale *= 2f;
            }

            num *= scale;

            float cap = 104.86f * scale;
            if (num > cap) num = cap;

            if (character.arbitrary.macGuffinBooster1Time.totalseconds > 0.0 || character.arbitrary.macGuffinBooster1InUse)
                num *= character.allArbitrary.potionModifier();

            __result = num;
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(PitController), "currentPitTime")]
        private static bool currentPitTime(ref float __result)
        {
            __result = 3600 * (character.pit.tossCount + 1) / GenSpeed;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TimerUp), "checkTriggers")]
        private static bool checkTriggers()
        {
            if (character.adventure.itopod.perkLevel[16] >= 1 && character.yggdrasil.fruits[6].maxTier >= 1 && character.rebirthTime.totalseconds >= (1800.0 / GeneralBuffs.GenSpeed) && !character.yggdrasil.permBonusOn)
            {
                character.yggdrasil.permBonusOn = true;
            }

            if (character.adventure.itopod.perkLevel[17] >= 1 && character.yggdrasil.fruits[8].maxTier >= 1 && character.rebirthTime.totalseconds >= (1800.0 / GeneralBuffs.GenSpeed) && !character.yggdrasil.permNumberBonusOn)
            {
                character.yggdrasil.permNumberBonusOn = true;
            }
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(FruitController), "tierThreshold")]
        private static bool tierThreshold(FruitController __instance, ref float __result)
        {
            float gs = GenSpeed;
            if (gs <= 0f) gs = 1f; // safety

            float baseTime = 3600f;
            float reduction = Mathf.Min(character.beastQuest.quirkLevel[13] * 60f, 1800f);

            __result = (baseTime - reduction) / gs;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "minimumWishTime")]
        private static bool minimumWishTime(WishesController __instance, ref float __result)
        {
            // Speed up the BASE time only
            float gs = GenSpeed;
            if (gs <= 0f) gs = 0.0001f;

            float num = 14400f / gs;

            // Do NOT scale reducers
            num -= character.adventureController.itopod.totalWishMinReduction();
            num -= character.beastQuestPerkController.totalWishMinReduction();

            if (num < 60f)
                num = 60f;

            __result = 1f / (num * 50f);
            return false;
        }

        [HarmonyPatch(typeof(HacksController), nameof(HacksController.endHackSpeed))]
        internal static class Patch_HacksController_EndHackSpeed
        {
            [HarmonyPrefix]
            private static bool Prefix(ref float __result)
            {
                __result = 1E-06f;
                return false; 
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BeastQuestController), "timerThreshold")]
        private static bool timerThreshold(BeastQuestController __instance, ref int __result)
        {
            int num = 28200;

            if (character.arbitrary.hasFasterQuests)
                num = (int)(num * 0.8f);

            if (character.inventory.itemList.fadComplete)
                num = (int)(num * 0.9f);

            // Never allow 0 or negative speed.
            float gs = GenSpeed;
            if (gs <= 0f) gs = 0.0001f;

            __result = Mathf.CeilToInt(num / gs); // 0.1 => 28200/0.1 = 282000
            if (__result < 1) __result = 1;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Rebirth), "minRebirthTime")]
        private static bool minRebirthTime(Rebirth __instance, ref int __result)
        {
            int num = 180;
            num -= character.wishes.wishes[20].level * 15;
            if (num < 120)
            {
                num = 120;
            }

            if (num > 180)
            {
                num = 180;
            }
            float gs = GenSpeed;
            if (gs <= 0f) gs = 0.0001f;

            __result = Mathf.CeilToInt(num / gs); 
            if (__result < 1) __result = 1;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Wandoos98Controller), "wandoosBootupTime")]
        private static bool wandoosBootupTime(ref float __result)
        {
            float num = 3600f;
            if (character.inventory.itemList.xlComplete)
            {
                num = 3240f;
            }

            float num2 = 1f - (float)character.allChallenges.level100Challenge.evilCompletions() * 0.1f;
            if (num2 < 0.5f)
            {
                num2 = 0.5f;
            }

            if (num2 > 1f)
            {
                num2 = 1f;
            }

            __result = num * num2 / GenSpeed;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllBeardsController), "timeFactor")]
        private static bool timeFactor(AllBeardsController __instance, ref double __result)
        {
            double t = character.rebirthTime.totalseconds * (double)GenSpeed;

            if (t < 3600.0)
            {
                __result = 0.0;
                return false;
            }

            double num = t / 10800.0 * 24.0 / (double)(24 - character.adventure.itopod.perkLevel[21]);

            if (num > 8.0)
                num = 8.0;

            __result = num;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "autoMergeTime")]
        private static bool autoMergeTime(ref float __result)
        {
            float num = 3600f;
            num *= 1f - (float)character.allChallenges.noEquipmentChallenge.completions() * 0.1f;
            if (character.arbitrary.improvedAutoBoostMerge)
            {
                num *= 0.5f;
            }

            __result = num / GenSpeed;
            return false;
        }

        [HarmonyPatch(typeof(RebirthPowerSpell), "Start")]
        internal static class Patch_IronPillCooldown
        {
            private const float DIVIDE_BY = GenSpeed;

            [HarmonyPostfix]
            private static void Postfix(RebirthPowerSpell __instance)
            {
                if (__instance == null) return;

                int cur = __instance.adventureSpellCooldown;
                if (cur <= 0) return;

                __instance.adventureSpellCooldown = Mathf.Max(1, Mathf.CeilToInt(cur / DIVIDE_BY));
            }
        }

        internal static class DailySaveAP
        {
            // 23 hours / 6 = 13800 seconds = 3h 50m
            public const double THRESHOLD = 82800.0 / (long)GenSpeed;
            public const double CAP = THRESHOLD + 1800.0; // mirrors the game's 84600 "cap" behavior
        }

        [HarmonyPatch(typeof(SaveButtonHover), "timeLeft")]
        internal static class Patch_SaveButtonHover_TimeLeft
        {
            [HarmonyPrefix]
            private static bool Prefix(SaveButtonHover __instance, ref string __result)
            {
                double t = __instance.character.settings.dailySaveRewardTime.totalseconds;
                if (t >= DailySaveAP.THRESHOLD) { __result = "READY"; return false; }
                __result = NumberOutput.timeOutput(DailySaveAP.THRESHOLD - t);
                return false;
            }
        }

        [HarmonyPatch(typeof(OpenFileDialog), "Update")]
        internal static class Patch_OpenFileDialog_Update_DailySaveTimer
        {
            [HarmonyPostfix]
            private static void Postfix(OpenFileDialog __instance)
            {
                // Only changes the timer’s “cap” so it behaves like vanilla (doesn’t grow forever).
                // Vanilla uses 84600; we use THRESHOLD + 1800.
                var ch = __instance.character;
                if (ch == null || !ch.mainMenu.doneInitialLoad) return;

                if (ch.settings.dailySaveRewardTime.totalseconds > DailySaveAP.CAP)
                    ch.settings.dailySaveRewardTime.setTime((float)DailySaveAP.CAP);
            }
        }

        [HarmonyPatch(typeof(OpenFileDialog), "Save")]
        internal static class Patch_OpenFileDialog_Save_Threshold
        {
            [HarmonyPrefix]
            private static void Prefix(OpenFileDialog __instance)
            {
                var ch = __instance.character;
                if (ch == null) return;

                if (ch.settings.dailySaveRewardTime.totalseconds >= DailySaveAP.THRESHOLD)
                {
                    // Let vanilla run; it will reset + award AP.
                    // We just “make ready earlier” by lowering the threshold checked elsewhere.
                }
            }
        }

        internal static class GuffTimers
        {
            // 6x faster timers
            private const double DIV = (long)GenSpeed;

            private const double VANILLA_READY = 82800.0;   // 23h
            private const double VANILLA_CAP = 84600.0;   // vanilla clamp

            private static double Scaled(double v) => v / DIV;

            private static int ScaleCooldownInt(int v)
            {
                if (v <= 1) return v;
                return Mathf.Max(1, Mathf.CeilToInt((float)(v / DIV)));
            }

            // ─────────────────────────────────────────────
            // 1) MacGuffin Blood Ritual cooldowns (Alpha/Beta)
            // ─────────────────────────────────────────────
            [HarmonyPatch(typeof(RebirthPowerSpell), "Start")]
            internal static class Patch_MacGuffinCooldowns_6xFaster
            {
                [HarmonyPostfix]
                private static void Postfix(RebirthPowerSpell __instance)
                {
                    if (__instance == null) return;

                    __instance.macguffin1Cooldown = ScaleCooldownInt(__instance.macguffin1Cooldown);
                    __instance.macguffin2Cooldown = ScaleCooldownInt(__instance.macguffin2Cooldown);
                }
            }

            // ─────────────────────────────────────────────
            // Helper: replace specific ldc.r8 constants in IL
            // ─────────────────────────────────────────────
            private static IEnumerable<CodeInstruction> ReplaceR8(
                IEnumerable<CodeInstruction> instructions,
                double fromA, double toA,
                double fromB, double toB)
            {
                foreach (var ci in instructions)
                {
                    if (ci.opcode == OpCodes.Ldc_R8 && ci.operand is double d)
                    {
                        if (Math.Abs(d - fromA) < 0.0000001) ci.operand = toA;
                        else if (Math.Abs(d - fromB) < 0.0000001) ci.operand = toB;
                    }
                    yield return ci;
                }
            }

            // ─────────────────────────────────────────────
            // 2) Daily Save AP timer: OpenFileDialog methods
            // ─────────────────────────────────────────────
            [HarmonyPatch]
            internal static class Patch_OpenFileDialog_DailySaveAP_6xFaster
            {
                private static IEnumerable<MethodBase> TargetMethods()
                {
                    var t = typeof(OpenFileDialog);

                    // These are the ones that usually embed 82800/84600
                    var names = new[]
                    {
                    "Update",
                    "Save",
                    "startSaveStandalone",
                    "dailySaveTimeLeft"
                };

                    foreach (var n in names)
                    {
                        var m = AccessTools.Method(t, n);
                        if (m != null) yield return m;
                    }
                }

                [HarmonyTranspiler]
                private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    double newReady = Scaled(VANILLA_READY);
                    double newCap = Scaled(VANILLA_CAP);

                    return ReplaceR8(instructions, VANILLA_READY, newReady, VANILLA_CAP, newCap);
                }
            }

            // ─────────────────────────────────────────────
            // 3) Save button hover: "READY" + time remaining
            // ─────────────────────────────────────────────
            [HarmonyPatch(typeof(SaveButtonHover), "timeLeft")]
            internal static class Patch_SaveButtonHover_TimeLeft_6xFaster
            {
                [HarmonyTranspiler]
                private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    double newReady = Scaled(VANILLA_READY);
                    // timeLeft typically only references 82800, but safe to also replace cap if present
                    double newCap = Scaled(VANILLA_CAP);

                    return ReplaceR8(instructions, VANILLA_READY, newReady, VANILLA_CAP, newCap);
                }
            }
        }

        [HarmonyPatch(typeof(Move69), nameof(Move69.Update))]
        internal static class Patch_Move69_Update
        {
            private static float Cooldown => 3600f / GenSpeed;

            [HarmonyPrefix]
            private static bool Prefix(
                Move69 __instance,
                ref float ___move69Timer)
            {
                if (!__instance.pc.moveCheck())
                {
                    __instance.button.interactable = false;
                    return false;
                }

                float num = Mathf.Max(__instance.pc.moveTimer, Cooldown - ___move69Timer);

                if (!__instance.character.adventure.move69Unlocked)
                {
                    __instance.buttonText.text = "Locked";
                    __instance.button.interactable = false;
                    return false;
                }

                __instance.border.color = UnityEngine.Color.clear;
                ___move69Timer += Time.deltaTime;

                if (__instance.character.adventure.autoattacking)
                {
                    __instance.buttonText.text = "Idle Mode";
                    __instance.button.interactable = false;
                    return false;
                }

                if (___move69Timer > Cooldown)
                {
                    __instance.button.interactable = true;
                    __instance.buttonText.text = "MOVE 69";
                    return false;
                }

                __instance.button.interactable = false;
                __instance.buttonText.text = num.ToString("###0") + " s";
                return false;
            }
        }

        [HarmonyPatch(typeof(Move69), nameof(Move69.OnPointerEnter))]
        internal static class Patch_Move69_OnPointerEnter
        {
            private static float Cooldown => 3600f / GenSpeed;

            [HarmonyPrefix]
            private static bool Prefix(Move69 __instance)
            {
                if (__instance.character.settings.rebirthDifficulty < difficulty.sadistic)
                {
                    __instance.tooltip.showTooltip(
                        "You are UNWORTHY of this move.",
                        Screen.width * 0.5f,
                        Screen.height * 0.5f);
                    return false;
                }

                if (!__instance.character.adventure.move69Unlocked)
                {
                    __instance.tooltip.showTooltip(
                        "You have not yet proven yourself worthy of this move. The answer lies with <b>Lemmiwinks</b>.",
                        Screen.width * 0.5f,
                        Screen.height * 0.5f);
                    return false;
                }

                __instance.tooltip.showTooltip(
                    "<b>MOVE 69</b>\n\nCooldown: " + Cooldown.ToString("#,##0") +
                    "  seconds.\nPerforms $#fRHe+7!!k_=;\nERROR CS0103 IN MOVE69.CS:91: THE END DOES NOT EXIST IN THE CURRENT CONTEXT",
                    Screen.width * 0.5f,
                    Screen.height * 0.4f);

                return false;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "autoBoostTime")]
        private static bool autoBoostTime(ref float __result)
        {
            float num = 3600f;
            num *= 1f - (float)character.allChallenges.noEquipmentChallenge.completions() * 0.1f;
            if (character.arbitrary.improvedAutoBoostMerge)
            {
                num *= 0.5f;
            }
            __result = num / GenSpeed;
            return false;
        }
    }
}
