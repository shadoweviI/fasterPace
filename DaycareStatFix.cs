using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static fasterPace.DaycareSpeed;

namespace fasterPace
{
    [HarmonyPatch(typeof(StatsDisplay), "displayMisc")]
    internal static class Patch_StatsDisplay_DisplayMisc_DaycareBreakdowns_FP
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(StatsDisplay __instance)
        {
            if (__instance == null) return;
            if (__instance.statsBreakdown == null || __instance.statValue == null) return;

            string bdText = __instance.statsBreakdown.text ?? "";
            string svText = __instance.statValue.text ?? "";

            var bdLines = new List<string>(bdText.Split('\n'));
            var svLines = new List<string>(svText.Split('\n'));
            while (svLines.Count < bdLines.Count) svLines.Add("");

            bool changed = false;

            // Replace SPEED block
            changed |= ReplaceBlock(
                bdLines, svLines,
                "<b>Base Kitty Happiness (speed):</b>",
                "<b>Total Kitty Happiness (speed):</b>",
                BuildFpDaycareSpeedBreakdown
            );

            // Replace TIME block
            changed |= ReplaceBlock(
                bdLines, svLines,
                "<b>Base Kitty Happiness (time):</b>",
                "<b>Total Kitty Happiness (time):</b>",
                BuildFpDaycareTimeBreakdown
            );

            if (!changed) return;

            __instance.statsBreakdown.text = string.Join("\n", bdLines.ToArray());
            __instance.statValue.text = string.Join("\n", svLines.ToArray());
        }

        private delegate void BuildBlockDelegate(out List<string> bd, out List<string> sv);

        private static bool ReplaceBlock(
            List<string> bdLines,
            List<string> svLines,
            string baseNeedle,
            string totalNeedle,
            BuildBlockDelegate builder)
        {
            int baseLine = FindLineIndexContaining(bdLines, baseNeedle);
            int totalLine = FindLineIndexContaining(bdLines, totalNeedle);
            if (baseLine < 0 || totalLine < 0 || totalLine <= baseLine) return false;

            List<string> newBd, newSv;
            builder(out newBd, out newSv);

            int removeCount = (totalLine - baseLine) + 1;
            if (baseLine + removeCount > bdLines.Count) return false;
            if (baseLine + removeCount > svLines.Count) return false;

            bdLines.RemoveRange(baseLine, removeCount);
            svLines.RemoveRange(baseLine, removeCount);

            bdLines.InsertRange(baseLine, newBd);
            svLines.InsertRange(baseLine, newSv);

            return true;
        }

        private static int FindLineIndexContaining(List<string> lines, string needle)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var s = lines[i];
                if (!string.IsNullOrEmpty(s) && s.IndexOf(needle, StringComparison.Ordinal) >= 0)
                    return i;
            }
            return -1;
        }

        private static Character GetCharacter()
        {
            try
            {
                // GeneralBuffs has: private static Character character;
                var f = AccessTools.Field(typeof(GeneralBuffs), "character");
                return f != null ? (Character)f.GetValue(null) : null;
            }
            catch { return null; }
        }

        // ---------------------------
        // TIME breakdown 
        // ---------------------------
        private static void BuildFpDaycareTimeBreakdown(out List<string> bd, out List<string> sv)
        {
            var c = GetCharacter();

            bd = new List<string>();
            sv = new List<string>();

            bd.Add("<b>Base Kitty Happiness (time):</b> ");
            sv.Add("  100%");

            float totalMult = 1f;

            if (c == null)
            {
                bd.Add("<b>Total Kitty Happiness (time):</b> ");
                sv.Add("  100%");
                return;
            }

            // Blind (FP): 25% first completion, + (0.01 * EXP_AP) per completion, cap 45%
            int blind = 0;
            try { blind = c.allChallenges.blindChallenge.completions(); } catch { blind = 0; }
            if (blind < 0) blind = 0;

            float expAp = GeneralBuffs.EXP_AP;
            if (expAp <= 0f) expAp = 1f;

            float blindReduction = 0f;
            if (blind >= 1) blindReduction += 0.25f;
            blindReduction += blind * (0.01f * expAp);
            if (blindReduction > 0.45f) blindReduction = 0.45f;
            if (blindReduction < 0f) blindReduction = 0f;

            float blindMult = 1f - blindReduction;

            if (blind > 0)
            {
                totalMult *= blindMult;
                bd.Add("<b>Normal Blind Challenge:</b> ");
                sv.Add("x " + (blindMult * 100f).ToString("0.##") + "%");
            }

            // Perks 27/28: double via EXP_AP
            try
            {
                long p27 = c.adventure.itopod.perkLevel[27];
                long p28 = c.adventure.itopod.perkLevel[28];

                float perkMult = 1f;

                if (p27 > 0)
                {
                    float baseRed27 = (float)p27 * c.adventureController.itopod.effectPerLevel[27];
                    float red27 = Mathf.Clamp01(baseRed27 * expAp);
                    perkMult *= (1f - red27);
                }
                if (p28 > 0)
                {
                    float baseRed28 = (float)p28 * c.adventureController.itopod.effectPerLevel[28];
                    float red28 = Mathf.Clamp01(baseRed28 * expAp);
                    perkMult *= (1f - red28);
                }

                if (p27 > 0 || p28 > 0)
                {
                    totalMult *= perkMult;
                    bd.Add("<b>Perks Modifier:</b> ");
                    sv.Add("x " + (perkMult * 100f).ToString("0.##") + "%");
                }
            }
            catch { }

            // AP Purchase: BoostTimeMult(0.9f, 0.95f)
            bool hasAP = false;
            try { hasAP = c.arbitrary.hasDaycareSpeed; } catch { hasAP = false; }

            if (hasAP)
            {
                float gs = GeneralBuffs.GenSpeed*2;
                if (gs <= 0f) gs = 1f;

                float reduction = (1f - 0.9f) * gs; // 0.1 * gs
                if (reduction < 0f) reduction = 0f;
                if (reduction > 0.95f) reduction = 0.95f;
                if (reduction > 0.999f) reduction = 0.999f;

                float apMult = 1f - reduction;

                totalMult *= apMult;
                bd.Add("<b>AP Purchase:</b> ");
                sv.Add("x " + (apMult * 100f).ToString("0.##") + "%");
            }

            bd.Add("<b>Total Kitty Happiness (time):</b> ");
            sv.Add("  " + (totalMult * 100f).ToString("0.##") + "%");
        }

        // ---------------------------
        // SPEED breakdown (match your Patch_AllGoldDiggerController_TotalDaycareBonus_FP)
        // ---------------------------
        private static void BuildFpDaycareSpeedBreakdown(out List<string> bd, out List<string> sv)
        {
            var c = GetCharacter();

            bd = new List<string>();
            sv = new List<string>();

            bd.Add("<b>Base Kitty Happiness (speed):</b> ");
            sv.Add("  100%");

            float totalMult = 1f;

            if (c == null)
            {
                bd.Add("<b>Total Kitty Happiness (speed):</b> ");
                sv.Add("  100%");
                return;
            }

            // Your patch baseline: pre starts at GenSpeed
            float gs = GeneralBuffs.GenSpeed*2;
            if (gs < 1f) gs = 1f;

            totalMult *= gs;
            bd.Add("<b>FasterPace Base:</b> ");
            sv.Add("x " + (gs * 100f).ToString("0.##") + "%");

            // Helper: FP scale = 1 + (BG - 1) * factor
            Func<float, float, float> Scale = (bgMult, factor) =>
            {
                float fp = 1f + (bgMult - 1f) * factor;
                return fp < 1f ? 1f : fp;
            };

            // Digger (source-of-truth from game's overload) + active gate
            float diggerMult = 1f;
            try { diggerMult = c.allDiggers.totalDaycareBonus(0, skipCheck: true); } catch { diggerMult = 1f; }

            bool diggerActive = false;
            try
            {
                // Your patch uses index 9
                const int DaycareDiggerIndex = 9;
                if (c.diggers != null && c.diggers.diggers != null && c.diggers.diggers.Count > DaycareDiggerIndex)
                    diggerActive = c.diggers.diggers[DaycareDiggerIndex].active;
            }
            catch { diggerActive = false; }

            if (!diggerActive) diggerMult = 1f;
            if (diggerMult < 1f) diggerMult = 1f;

            if (diggerMult > 1f)
            {
                totalMult *= diggerMult;
                bd.Add("<b>Digger Modifier:</b> ");
                sv.Add("x " + (diggerMult * 100f).ToString("0.##") + "%");
            }

            // Equipment (scaled)
            float equipBg = 1f;
            try { equipBg = 1f + c.inventoryController.bonuses[specType.DaycareSpeed]; } catch { equipBg = 1f; }
            float equipFp = Scale(equipBg, Patch_AllGoldDiggerController_TotalDaycareBonus_FP.EquipFactor);
            if (equipFp > 1f)
            {
                totalMult *= equipFp;
                bd.Add("<b>Equipment Modifier:</b> ");
                sv.Add("x " + (equipFp * 100f).ToString("0.##") + "%");
            }

            // Evil blind (scaled)
            int evil = 0;
            try { evil = c.allChallenges.blindChallenge.evilCompletions(); } catch { evil = 0; }
            if (evil > 0)
            {
                float evilBg = 1f + evil * 0.02f;
                float evilFp = Scale(evilBg, Patch_AllGoldDiggerController_TotalDaycareBonus_FP.EvilBlindFactor);
                totalMult *= evilFp;
                bd.Add("<b>Evil Blind Challenge Modifier:</b> ");
                sv.Add("x " + (evilFp * 100f).ToString("0.##") + "%");
            }

            // Sadistic blind (scaled)
            int sad = 0;
            try { sad = c.allChallenges.blindChallenge.sadisticCompletions(); } catch { sad = 0; }
            if (sad > 0)
            {
                float sadBg = 1f + sad * 0.01f;
                float sadFp = Scale(sadBg, Patch_AllGoldDiggerController_TotalDaycareBonus_FP.SadBlindFactor);
                totalMult *= sadFp;
                bd.Add("<b>Sadistic Blind Challenge Modifier:</b> ");
                sv.Add("x " + (sadFp * 100f).ToString("0.##") + "%");
            }

            // Hacks (unscaled in your patch)
            float hackMult = 1f;
            try { hackMult = c.hacksController.totalDaycareSpeedBonus(); } catch { hackMult = 1f; }
            if (hackMult < 1f) hackMult = 1f;
            if (hackMult > 1f)
            {
                totalMult *= hackMult;
                bd.Add("<b>Hacks Modifier:</b> ");
                sv.Add("x " + (hackMult * 100f).ToString("0.##") + "%");
            }

            // Wish (scaled)
            float wishBg = 1f;
            try { wishBg = c.wishesController.totalDaycareSpeedBonus(); } catch { wishBg = 1f; }
            if (wishBg < 1f) wishBg = 1f;
            float wishFp = Scale(wishBg, Patch_AllGoldDiggerController_TotalDaycareBonus_FP.WishFactor);
            if (wishFp > 1f)
            {
                totalMult *= wishFp;
                bd.Add("<b>Wish Modifier:</b> ");
                sv.Add("x " + (wishFp * 100f).ToString("0.##") + "%");
            }

            // Card (unscaled in your patch)
            float cardMult = 1f;
            try { cardMult = c.cardsController.getBonus(cardBonus.dayCareSpeed); } catch { cardMult = 1f; }
            if (cardMult < 1f) cardMult = 1f;
            if (cardMult > 1f)
            {
                totalMult *= cardMult;
                bd.Add("<b>Card Modifier:</b> ");
                // vanilla uses cardBonusString; we’ll do a numeric percent for consistency
                sv.Add("x " + (cardMult * 100f).ToString("0.##") + "%");
            }

            // Fibonacci perk (scaled)
            bool fiboOn = false;
            try { fiboOn = c.adventure.itopod.perkLevel[94] >= 55L; } catch { fiboOn = false; }
            if (fiboOn)
            {
                float fiboBg = 1.05f;
                float fiboFp = Scale(fiboBg, Patch_AllGoldDiggerController_TotalDaycareBonus_FP.FiboFactor);
                totalMult *= fiboFp;
                bd.Add("<b>Fibonacci Perk Modifier:</b> ");
                sv.Add("x " + (fiboFp * 100f).ToString("0.##") + "%");
            }

            bd.Add("<b>Total Kitty Happiness (speed):</b> ");
            sv.Add("  " + (totalMult * 100f).ToString("0.##") + "%");
        }
    }
}
