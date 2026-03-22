using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore;

namespace fasterPace
{
    internal class ChallengeTooltips
    {
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }

        [HarmonyPatch(typeof(NGUChallengeController), "showChallengeInfo")]
        internal static class Patch_NGUChallengeController_ShowChallengeInfo_Prefix
        {
            private static readonly FieldInfo FI_message =
                AccessTools.Field(typeof(ChallengeController), "message") // often declared on base
                ?? AccessTools.Field(typeof(NGUChallengeController), "message");

            private static readonly FieldInfo FI_maxCompletions =
                AccessTools.Field(typeof(ChallengeController), "maxCompletions")
                ?? AccessTools.Field(typeof(NGUChallengeController), "maxCompletions");

            [HarmonyPrefix]
            private static bool Prefix(NGUChallengeController __instance)
            {
                var c = __instance?.character;
                if (c == null || __instance.challengeInfo == null) return false;

                float effectiveLevel = NoNGUSoftcaps.EffNGULevel;
                if (effectiveLevel <= 0f) effectiveLevel = 1f;

                long required = (long)Mathf.Ceil(10000f / effectiveLevel);
                if (required < 1) required = 1;

                int maxCompletions = 0;
                if (FI_maxCompletions != null)
                {
                    try { maxCompletions = (int)FI_maxCompletions.GetValue(__instance); }
                    catch { maxCompletions = 0; }
                }

                string msg =
                    "<b>NGU Challenge</b>\n\n" +
                    "<b>Recommended Stats:</b> Everything that isn't NGU!\n\n" +
                    "<b>Description:</b> Those NGU's sure are powerful! Let's see how well you do without them.\n\n" +
                    "<b>Win condition:</b> Defeat " + c.bossController.getBossName(__instance.targetBoss()) +
                    " (Boss # " + (__instance.targetBoss() + 1) +
                    "). The boss # will increase by 10 for each completion.\n\n" +
                    "<b>Reward:</b>\n" + c.checkExpAdded(__instance.expectedEXP()).ToString("###,##0") + " EXP.\n" +
                    __instance.expectedAPReward() + " AP.\n" +
                    __instance.specialRewards() + "\n\n" +
                    "<b>Completions:</b> " + __instance.currentCompletions() + " / " + __instance.maxCompletions + "\n\n" +
                    "<b>Last Completion Time:</b> " + NumberOutput.timeOutput(c.challenges.nguChallenge.bestTime) + "\n\n" +
                    "<b>Restrictions:</b> NGU's provide absolutely no bonuses!\n\n" +
                    "<b>Challenge Unlock Condition:</b> Obtain >" + required.ToString("N0") +
                    " levels total through all your NGU's!";

                // Set private/protected 'message' if it exists (keeps vanilla behavior consistent)
                if (FI_message != null)
                {
                    try { FI_message.SetValue(__instance, msg); } catch { /* ignore */ }
                }

                __instance.challengeInfo.text = msg;
                return false; 
            }

            // stub 
            private static float EffectiveLevel(Character c) => 1f;
        }

        [HarmonyPatch(typeof(NoRebirthChallengeController), "specialRewards")]
        internal static class Patch_NoRebirthChallengeController_SpecialRewards
        {
            [HarmonyPostfix]
            private static void Postfix(NoRebirthChallengeController __instance, ref string __result)
            {
                var c = __instance?.character;
                if (c?.settings == null) return;

                int stepMins = (int)(TitanSpawnTimes.CompletionStepScaled / 60f);
                int flatMins = (int)(TitanSpawnTimes.FINAL_FLAT / 60f);
                int minFloorMins = (int)(TitanSpawnTimes.MinSpawnTimeScaled / 60f);

                if (c.settings.rebirthDifficulty == difficulty.normal)
                {
                    __result =
                        "First completion: +1 level to all dropped Titan loot.\n" +
                        $"Every completion: -{stepMins} minutes to Titan spawn time starting with Jake From Accounting and beyond.\n" +
                        $"Titan spawn times cannot go below {minFloorMins} minutes.";
                }
                else if (c.settings.rebirthDifficulty == difficulty.evil)
                {
                    __result =
                        $"Every completion: -{stepMins} minutes to Titan spawn time starting with Greasy Nerd and beyond. Removes -{flatMins} at 10 completions for Evil";
                }
                else if (c.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    __result =
                        $"Every completion: -{stepMins} minutes to Titan spawn time starting with IT HUNGERS and beyond. Removes -{flatMins} at 10 completions for Sadistic";
                }
            }
        }

        [HarmonyPatch(typeof(LaserSwordChallengeController))]
        internal static class Patch_LSCController_SpecialRewards
        {
            [HarmonyPrefix]
            [HarmonyPatch("specialRewards")]
            private static bool Prefix(LaserSwordChallengeController __instance, ref string __result)
            {
                var c = __instance?.character;
                if (c?.settings == null)
                    return true;

                if (c.settings.rebirthDifficulty == difficulty.normal)
                {
                    __result =
                        "Each completion: Augment levels for Milk and on will be raised to a higher power! Each completion will raise the bonus by 0.01 for milk, 0.02 for cannon, and so on down to Laser Sword.\nFirst Completion: Same bonus, but increases by 0.05!\nFinal Completion: Same bonus, but increases by 0.05!";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.evil)
                {
                    __result = "Each completion: 1% more Ygg yields! Final completion grants 50% Adventure Stats!";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    __result = "Each completion: 0.5% Tagged Card Effect! Final completion grants 25% Mayo & Card Speed gen!";
                    return false;
                }

                __result = "Additional special rewards will be added over time!";
                return false;
            }
        }

        [HarmonyPatch(typeof(BlindChallengeController))]
        internal static class Patch_BlindChallengeController_SpecialRewards
        {
            [HarmonyPrefix]
            [HarmonyPatch("specialRewards")]
            private static bool Prefix(BlindChallengeController __instance, ref string __result)
            {
                var c = __instance?.character;
                if (c?.settings == null)
                    return true; 

                if (c.settings.rebirthDifficulty == difficulty.normal)
                {
                    __result =
                        "Each completion: Items will take 2% less time to level up in the daycare!\n" +
                        "First Completion: Items will take a bonus 25% less time to level up in the daycare!\n" +
                        "Final Completion: An extra daycare slot is unlocked!";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.evil)
                {
                    __result = "Each completion: Daycare Speed increases by 3%!\n";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    __result = "Each completion: Daycare Speed increases by 3%!\n";
                    return false;
                }

                __result = "Additional special rewards will be added over time!";
                return false;
            }
        }

        [HarmonyPatch(typeof(TimeMachineChallengeController))]
        internal static class Patch_TMCController_SpecialRewards
        {
            [HarmonyPrefix]
            [HarmonyPatch("specialRewards")]
            private static bool Prefix(TimeMachineChallengeController __instance, ref string __result)
            {
                var c = __instance?.character;
                if (c?.settings == null)
                    return true;

                if (c.settings.rebirthDifficulty == difficulty.normal)
                {
                    __result =
                        "First Completion: +5% bonuses to all active diggers!\nEach completion: + 100% to your GPS!\n5th Completion: An extra digger slot!";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.evil)
                {
                    __result = "First Completion: +100% Gold Drop!\nEach completion: 10% TM Speed Bonus.";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    __result = "3% Global Digger Bonus! Final completion grants 10% Global Digger Bonus!";
                    return false;
                }

                __result = "Additional special rewards will be added over time!";
                return false;
            }
        }

        [HarmonyPatch(typeof(LevelChallenge10KController))]
        internal static class Patch_100LController_SpecialRewards
        {
            [HarmonyPrefix]
            [HarmonyPatch("specialRewards")]
            private static bool Prefix(LevelChallenge10KController __instance, ref string __result)
            {
                var c = __instance?.character;
                if (c?.settings == null)
                    return true;

                if (c.settings.rebirthDifficulty == difficulty.normal)
                {
                    __result =
                        "First Completion: Unlock Boost Transformation.Change any type of boost to a Power/ Toughness / Special Boost with Q / W / E + left click, at a cost of reducing the boost by one tier.\nEach completion: +20 % permanent Wandoos Speed.\nFinal completion: Removes Boost transformation cost!You also unlock an option in the settings menu to automatically transform dropped boosts!";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.evil)
                {
                    __result = "Each completion: +10% Wandoos Bootup Speed!";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    __result = "80% Bonus to cooking timer AND bonus! Final completion uncaps the 300% cooking hardcap.";
                    return false;
                }

                __result = "Additional special rewards will be added over time!";
                return false;
            }
        }

        [HarmonyPatch(typeof(NoAugsChallengeController))]
        internal static class Patch_NoAugsCController_SpecialRewards
        {
            [HarmonyPrefix]
            [HarmonyPatch("specialRewards")]
            private static bool Prefix(NoAugsChallengeController __instance, ref string __result)
            {
                var c = __instance?.character;
                if (c?.settings == null)
                    return true;

                if (c.settings.rebirthDifficulty == difficulty.normal)
                {
                    __result =
                        "First completion: +10 % Augment leveling speed.\nEvery completion: +25 % to Total Augment Power.\nFinal completion: Reduces augmentation and upgrade costs by 50 %.";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.evil)
                {
                    __result = "Each completion gives +5% Augmentation levelling Speed! Final Completion grants an extra 25% speed!";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    __result = "Beards permanent levels are 1% more effective per level. Final completion removes the power raised to permanent beard levels gain!";
                    return false;
                }

                __result = "Additional special rewards will be added over time!";
                return false;
            }
        }

        [HarmonyPatch(typeof(NGUChallengeController))]
        internal static class Patch_NoNGUCController_SpecialRewards
        {
            [HarmonyPrefix]
            [HarmonyPatch("specialRewards")]
            private static bool Prefix(NGUChallengeController __instance, ref string __result)
            {
                var c = __instance?.character;
                if (c?.settings == null)
                    return true;

                if (c.settings.rebirthDifficulty == difficulty.normal)
                {
                    __result =
                        "Each completion: 5% faster NGUs!";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.evil)
                {
                    __result = "Each completion: 20% faster Hacks!";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    __result = "Each completion: 5% cheaper Sadistic NGUs! And 5% NGU Level Effectiveness!";
                    return false;
                }

                __result = "Additional special rewards will be added over time!";
                return false;
            }
        }

        [HarmonyPatch(typeof(TrollChallengeController))]
        internal static class Patch_TrollCController_SpecialRewards
        {
            [HarmonyPrefix]
            [HarmonyPatch("specialRewards")]
            private static bool Prefix(TrollChallengeController __instance, ref string __result)
            {
                var c = __instance?.character;
                if (c?.settings == null)
                    return true;

                if (c.settings.rebirthDifficulty == difficulty.normal)
                {
                    __result =
                        "Completion 1: 3x speed boost to Magic NGU!\nCompletion 2: A new accessory slot! *VERY GOOD THING*\nCompletion 3: Yggdrasil fruits can be upgraded to a max tier of 24!\nCompletion 4: A new beard slot! *ALSO VERY GOOD*\nCompletion 5: A new fruit: The Fruit of Numbers!\nCompletion 6: A new Blood Magic Ritual!\nCompletion 7: The Golden Beard!";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.evil)
                {
                    __result = "Completion 1: An extra Accessory slot!\nCompletion 2: An extra MacGuffin slot!\nCompletion 3: An extra Daycare slot!\nCompletion 4: Unlock the Dual Wielding Wish!\nCompletion 5: +25% Hack Speed!\nCompletion 6: Unlock Improved Dual Wielding!\nCompletion 7: Unlock a Wish Slot!";
                    return false;
                }

                if (c.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    __result = "Completion 1: x3 Energy NGU Speed!\nCompletion 2: MacGuffin Time Factor Multiplier = 1 > 2\nCompletion 3: Beast Mode is now x2 less damage taken instead of 3!\nCompletion 4: Cooking affects Yggdrasil gain at 50% rate!\nCompletion 5:+10% Card Generation Speed\nCompletion 6: +10% Mayo Generation Speed!\nCompletion 7: Unlock an Accessory Slot! Yes, I am a bastard.";
                    return false;
                }

                __result = "Additional special rewards will be added over time!";
                return false;
            }
        }
    }
}
