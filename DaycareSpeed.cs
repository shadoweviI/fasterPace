using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx.Logging;

namespace fasterPace
{
    internal class DaycareSpeed


    {
        [HarmonyPatch(typeof(InventoryController), "effectBonus")]
        internal static class Patch_InventoryController_EffectBonus_DaycareSpeed_EquipFactor
        {
            // method signature:
            // float effectBonus(float amount, specType type)

            [HarmonyPrefix]
            private static bool Prefix(float amount, specType type, ref float __result)
            {
                if (type != specType.DaycareSpeed)
                    return true; // vanilla for everything else

                // Vanilla display: amount / 100000f
                // Gameplay buff: multiplied by EquipFactor (== GenSpeed)
                __result = (amount / 100000f) * Patch_AllGoldDiggerController_TotalDaycareBonus_FP.EquipFactor;

                return false; 
            }
        }

        [HarmonyPatch(typeof(InventoryController), "updateBonuses")]
        internal static class Patch_InventoryController_UpdateBonuses_DaycareSpeed_DisplayEquipFactor
        {
            private static readonly FieldInfo BonusDisplayField =
                AccessTools.Field(typeof(InventoryController), "bonusDisplay");

            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(InventoryController __instance)
            {
                if (__instance == null) return;
                if (__instance.bonuses == null) return;
                if (__instance.display == null) return;
                if (BonusDisplayField == null) return;

                string bd = BonusDisplayField.GetValue(__instance) as string ?? "";

                const string tag = "<b>Daycare Speed:</b>";
                if (!bd.Contains(tag))
                    return;

                // This should match the gameplay scaling factor (EquipFactor = GenSpeed)
                float equipFactor = Patch_AllGoldDiggerController_TotalDaycareBonus_FP.EquipFactor; // or GeneralBuffs.EquipFactor if you exposed it

                // Use the already-computed bonus, just scale for display
                float baseBonus = 0f;
                if (__instance.bonuses.TryGetValue(specType.DaycareSpeed, out var v))
                    baseBonus = v;

                string replacementLine = $"{tag} {(baseBonus * equipFactor * 100f).ToString("#0.##")}%";

                // Replace only that one line, keeping alignment of the list intact
                var lines = new List<string>(bd.Split('\n'));
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains(tag))
                    {
                        lines[i] = replacementLine;
                        break;
                    }
                }

                string newBd = string.Join("\n", lines);

                // write back to the private field
                BonusDisplayField.SetValue(__instance, newBd);

                // re-render UI (since updateBonuses already rendered before our postfix ran)
                __instance.display.updateDisplay(newBd);
            }
        }

        [HarmonyPatch(typeof(AllDaycareController), "daycareTime")]
        internal static class Patch_AllDaycareController_DaycareTime_Item129
        {
            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)] // run after other postfixes too
            private static void Postfix(AllDaycareController __instance, Equipment item, ref float __result)
            {
                if (item == null) return;

                // My Yellow Heart <3
                if (item.id == 129)
                    __result *= 0.1f; // 10x faster
            }
        }


        [HarmonyPatch(typeof(AllDaycareController), "daycareTime")]
        internal static class Patch_AllDaycareController_DaycareTime_FasterPace
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)] // run before any other prefixes
            internal static bool Prefix(AllDaycareController __instance, Equipment item, ref float __result)
            {
                var c = __instance?.character;
                if (c == null || item == null || c.itemInfo?.daycareRate == null)
                    return true; // fallback to vanilla if something is missing

                float num = c.itemInfo.daycareRate[item.id];

                float gs = GeneralBuffs.GenSpeed;
                if (gs <= 0f) gs = 1f;

                float expAp = GeneralBuffs.EXP_AP;
                if (expAp <= 0f) expAp = 1f;

                float BoostTimeMult(float bgMult, float maxFinalReduction)
                {
                    float reduction = (1f - bgMult) * gs;
                    if (reduction < 0f) reduction = 0f;
                    if (reduction > maxFinalReduction) reduction = maxFinalReduction;
                    if (reduction > 0.999f) reduction = 0.999f;
                    return 1f - reduction;
                }

                // -------------------------
                // Blind: 25% first, 2% per (via EXP_AP)
                // Cap at 45% total reduction (mult floor = 0.55)
                // -------------------------
                int blind = 0;
                try { blind = c.allChallenges?.blindChallenge?.completions() ?? 0; } catch { blind = 0; }
                if (blind < 0) blind = 0;

                float blindReduction = 0f;
                if (blind >= 1) blindReduction += 0.25f;
                blindReduction += blind * (0.01f * expAp);

                if (blindReduction > 0.45f) blindReduction = 0.45f;
                if (blindReduction < 0f) blindReduction = 0f;

                num *= (1f - blindReduction);

                // -------------------------
                // Perks 27/28: double reduction via EXP_AP (apply directly; no GenSpeed boosting)
                // -------------------------
                try
                {
                    float bg27 = 1f - (float)c.adventure.itopod.perkLevel[27] * c.adventureController.itopod.effectPerLevel[27];
                    float red27 = (1f - bg27) * expAp;
                    if (red27 < 0f) red27 = 0f;
                    if (red27 > 0.999f) red27 = 0.999f;
                    num *= (1f - red27);

                    float bg28 = 1f - (float)c.adventure.itopod.perkLevel[28] * c.adventureController.itopod.effectPerLevel[28];
                    float red28 = (1f - bg28) * expAp;
                    if (red28 < 0f) red28 = 0f;
                    if (red28 > 0.999f) red28 = 0.999f;
                    num *= (1f - red28);
                }
                catch { }

                // -------------------------
                // Daycare Speed Boost: sheet-style (0.90 -> 0.70 at gs=3)
                // -------------------------
                if (c.arbitrary?.hasDaycareSpeed == true)
                    num *= BoostTimeMult(0.9f, 0.95f);

                if (num < 0.001f) num = 0.001f;

                __result = num;
                return false; // skip vanilla (we replaced it)
            }
        }




        [HarmonyPatch(typeof(AllGoldDiggerController), "totalDaycareBonus", new Type[] { })]
        internal static class Patch_AllGoldDiggerController_TotalDaycareBonus_FP
        {
            private const float GenSpeed = GeneralBuffs.GenSpeed;

            // Sheet-based scaling factors (speed side)
            internal const float EquipFactor = GenSpeed;
            internal const float WishFactor = GenSpeed;
            internal const float FiboFactor = GenSpeed;
            internal const float EvilBlindFactor = GenSpeed / 2f;
            internal const float SadBlindFactor = GenSpeed * 2f;

            // Known daycare digger index 
            private const int DaycareDiggerIndex = 9;

            private static float Scale(float bgMult, float factor)
            {
                // FP = 1 + (BG - 1) * factor
                float fp = 1f + (bgMult - 1f) * factor;
                return fp < 1f ? 1f : fp;
            }

            [HarmonyPrefix]
            private static bool Prefix(AllGoldDiggerController __instance, ref float __result)
            {
                var c = __instance?.character;
                if (c == null) return true;

                // -----------------------------
                // PRE-DIGGER speed (baseline)
                // -----------------------------
                float pre = GenSpeed * 2; // base ALWAYS 3x

                // Vanilla “1 -> 1” rows 
                float hackMult = 1f;
                float cardMult = 1f;
                try { hackMult = c.hacksController.totalDaycareSpeedBonus(); } catch { }
                try { cardMult = c.cardsController.getBonus(cardBonus.dayCareSpeed); } catch { }
                if (hackMult <= 0f) hackMult = 1f;
                if (cardMult <= 0f) cardMult = 1f;
                pre *= hackMult;
                pre *= cardMult;

                // Sheet-scaled rows
                float equipMult = 1f;
                float wishMult = 1f;
                float fiboMult = 1f;
                float evilMult = 1f;
                float sadMult = 1f;

                try { equipMult = 1f + c.inventoryController.bonuses[specType.DaycareSpeed]; } catch { equipMult = 1f; }
                try { wishMult = c.wishesController.totalDaycareSpeedBonus(); } catch { wishMult = 1f; }
                try { fiboMult = (c.adventure.itopod.perkLevel[94] >= 55) ? 1.05f : 1f; } catch { fiboMult = 1f; }
                try { evilMult = 1f + c.allChallenges.blindChallenge.evilCompletions() * 0.02f; } catch { evilMult = 1f; }
                try { sadMult = 1f + c.allChallenges.blindChallenge.sadisticCompletions() * 0.01f; } catch { sadMult = 1f; }

                pre *= Scale(equipMult, EquipFactor);
                pre *= Scale(wishMult, WishFactor);
                pre *= Scale(fiboMult, FiboFactor);
                pre *= Scale(evilMult, EvilBlindFactor);
                pre *= Scale(sadMult, SadBlindFactor);

                if (pre < 1f) pre = 1f;
                float diggerMult = 1f;
                try
                {
                    diggerMult = __instance.totalDaycareBonus(0, skipCheck: true);
                }
                catch
                {
                    diggerMult = 1f;
                }

                // Only apply digger if it is actually active (true "extra bonus")
                bool diggerActive = false;
                long diggerLevel = 0;
                try
                {
                    if (c.diggers?.diggers != null && c.diggers.diggers.Count > DaycareDiggerIndex)
                    {
                        diggerActive = c.diggers.diggers[DaycareDiggerIndex].active;
                        diggerLevel = Convert.ToInt64(c.diggers.diggers[DaycareDiggerIndex].curLevel);
                    }
                }
                catch { }

                if (!diggerActive) diggerMult = 1f;
                if (diggerMult < 1f) diggerMult = 1f;

                float final = pre * diggerMult;
                if (final < 1f) final = 1f;

                __result = final;

                return false;
            }
        }
    }
}
