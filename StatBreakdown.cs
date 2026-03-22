using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fasterPace
{
    internal class StatBreakdown
    {
        internal static class FasterPaceBreakdownUtil
        {
            internal static void InsertModifierLine(
                StatsDisplay __instance,
                string sectionKey,
                string totalLineKey,
                float percentMultiplier,
                string sentinel)
            {
                if (__instance == null) return;
                if (__instance.statsBreakdown == null || __instance.statValue == null) return;

                string bd = __instance.statsBreakdown.text ?? "";
                string sv = __instance.statValue.text ?? "";

                if (!bd.Contains(sectionKey) || !bd.Contains(totalLineKey))
                    return;

                if (bd.Contains(sentinel))
                    return;

                var bdLines = new List<string>(bd.Split('\n'));
                var svLines = new List<string>(sv.Split('\n'));

                int totalLine = bdLines.FindIndex(l => l.Contains(totalLineKey));
                if (totalLine < 0) return;

                while (svLines.Count < bdLines.Count)
                    svLines.Add("");

                bdLines.Insert(totalLine, $"<b>{sentinel}:</b> ");
                svLines.Insert(totalLine, "x " + percentMultiplier.ToString("0.##") + "%");

                __instance.statsBreakdown.text = string.Join("\n", bdLines);
                __instance.statValue.text = string.Join("\n", svLines);
            }
        }

        // =========================================================
        // ENERGY / MAGIC / RES3 BARS
        // GeneralBuffs scales these by GenSpeed
        // =========================================================

        [HarmonyPatch(typeof(StatsDisplay), "displayEnergy")]
        internal static class Patch_StatsDisplay_DisplayEnergy_FasterPace
        {
            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(StatsDisplay __instance)
            {
                FasterPaceBreakdownUtil.InsertModifierLine(
                    __instance,
                    "Base Energy Bars",
                    "Total Energy Bars",
                    GeneralBuffs.GenSpeed * 100f,
                    "Faster Pace Modifier (Energy Bars)");
            }
        }

        [HarmonyPatch(typeof(StatsDisplay), "displayMagic")]
        internal static class Patch_StatsDisplay_DisplayMagic_FasterPace
        {
            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(StatsDisplay __instance)
            {
                FasterPaceBreakdownUtil.InsertModifierLine(
                    __instance,
                    "Base Magic Bars",
                    "Total Magic Bars",
                    GeneralBuffs.GenSpeed * 100f,
                    "Faster Pace Modifier (Magic Bars)");
            }
        }

        [HarmonyPatch(typeof(StatsDisplay), "displayRes3")]
        internal static class Patch_StatsDisplay_DisplayRes3_FasterPace
        {
            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(StatsDisplay __instance)
            {
                if (__instance == null) return;
                if (__instance.statsBreakdown == null || __instance.statValue == null) return;

                string bd = __instance.statsBreakdown.text ?? "";
                string sv = __instance.statValue.text ?? "";

                const string sentinel = "Faster Pace Modifier (Res3 Bars)";
                if (bd.Contains(sentinel))
                    return;

                var bdLines = new List<string>(bd.Split('\n'));
                var svLines = new List<string>(sv.Split('\n'));

                while (svLines.Count < bdLines.Count)
                    svLines.Add("");

                int insertAt = -1;

                // Find the dynamic Res3 bars section by looking for:
                //   Base <something> Bars
                //   ...
                //   Total <same something> Bars
                for (int i = 0; i < bdLines.Count; i++)
                {
                    string line = bdLines[i];
                    if (string.IsNullOrEmpty(line)) continue;

                    if (!line.Contains("Base ") || !line.Contains(" Bars"))
                        continue;

                    string core = ExtractBarsCore(line, "Base ", " Bars");
                    if (string.IsNullOrEmpty(core))
                        continue;

                    string expectedTotalA = $"Total {core} Bars";
                    string expectedTotalB = $"Total {core} Bar";

                    int totalIdx = bdLines.FindIndex(i + 1, l =>
                        l.Contains(expectedTotalA) || l.Contains(expectedTotalB));

                    if (totalIdx >= 0)
                    {
                        insertAt = totalIdx;
                        break;
                    }
                }

                if (insertAt < 0)
                    return;

                bdLines.Insert(insertAt, $"<b>{sentinel}:</b> ");
                svLines.Insert(insertAt, "x " + (GeneralBuffs.GenSpeed * 100f).ToString("0.##") + "%");

                __instance.statsBreakdown.text = string.Join("\n", bdLines);
                __instance.statValue.text = string.Join("\n", svLines);
            }

            private static string ExtractBarsCore(string line, string prefix, string suffix)
            {
                int start = line.IndexOf(prefix);
                if (start < 0) return null;
                start += prefix.Length;

                int end = line.IndexOf(suffix, start);
                if (end < 0 || end <= start) return null;

                string core = line.Substring(start, end - start);

                core = core.Replace("<b>", "").Replace("</b>", "").Trim();
                return string.IsNullOrEmpty(core) ? null : core;
            }
        }



        // =========================================================
        // EXP
        // GeneralBuffs scales exp gain by GenSpeed * 2
        // =========================================================

        [HarmonyPatch(typeof(StatsDisplay), "displayEXPGain")]
        internal static class Patch_StatsDisplay_DisplayEXPGain_FasterPace
        {
            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(StatsDisplay __instance)
            {
                if (__instance == null) return;
                if (__instance.statsBreakdown == null || __instance.statValue == null) return;

                // EXP
                FasterPaceBreakdownUtil.InsertModifierLine(
                    __instance,
                    "Base EXP Gain",
                    "Total EXP Bonus",
                    GeneralBuffs.GenSpeed * 2f * 100f,
                    "Faster Pace Modifier (EXP)");

                FasterPaceBreakdownUtil.InsertModifierLine(
                    __instance,
                    "Base EXP",
                    "Total EXP Bonus",
                    GeneralBuffs.GenSpeed * 2f * 100f,
                    "Faster Pace Modifier (EXP)");

                // AP
                InsertAPModifierIntoExpApPpPage(__instance);
            }

            private static void InsertAPModifierIntoExpApPpPage(StatsDisplay __instance)
            {
                string bd = __instance.statsBreakdown.text ?? "";
                string sv = __instance.statValue.text ?? "";

                const string sentinel = "Faster Pace Modifier (AP)";
                if (bd.Contains(sentinel))
                    return;

                var bdLines = new List<string>(bd.Split('\n'));
                var svLines = new List<string>(sv.Split('\n'));

                while (svLines.Count < bdLines.Count)
                    svLines.Add("");

                int insertAt = -1;

                for (int i = 0; i < bdLines.Count; i++)
                {
                    string line = StripTags(bdLines[i]);
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.IndexOf("Total AP Bonus", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        insertAt = i;
                        break;
                    }
                }

                if (insertAt < 0)
                {
                    for (int i = 0; i < bdLines.Count; i++)
                    {
                        string line = StripTags(bdLines[i]);
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        if (line.IndexOf("Total AP", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            insertAt = i;
                            break;
                        }
                    }
                }

                if (insertAt < 0)
                    return;

                float pct = GeneralBuffs.EXP_AP * 2f * 100f;

                bdLines.Insert(insertAt, $"<b>{sentinel}:</b> ");
                svLines.Insert(insertAt, "x " + pct.ToString("0.##") + "%");

                __instance.statsBreakdown.text = string.Join("\n", bdLines);
                __instance.statValue.text = string.Join("\n", svLines);
            }

            private static string StripTags(string s)
            {
                if (string.IsNullOrEmpty(s)) return "";
                return s.Replace("<b>", "").Replace("</b>", "").Trim();
            }
        }

        // =========================================================
        // MISC PAGE
        // jshep uses this area for Boost Modifier / Mayo Generation.
        // Hack / Wish can also be inserted here IF their lines already exist.
        // =========================================================

        [HarmonyPatch(typeof(StatsDisplay), "displayMisc")]
        internal static class Patch_StatsDisplay_DisplayMisc_FasterPace
        {
            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(StatsDisplay __instance)
            {
                // Boosts
                FasterPaceBreakdownUtil.InsertModifierLine(
                    __instance,
                    "Base Boost Modifier",
                    "Total Boost Modifier",
                    GeneralBuffs.GenSpeed * 2f * 100f,
                    "Faster Pace Modifier (Boosts)");

                // Mayo
                FasterPaceBreakdownUtil.InsertModifierLine(
                    __instance,
                    "Base Mayo Generation Rate",
                    "Total Mayo Generation Rate",
                    GeneralBuffs.GenSpeed * 2f * 100f,
                    "Faster Pace Modifier (Mayo)");

                // Hack Speed - only if that section exists in this page
                FasterPaceBreakdownUtil.InsertModifierLine(
                    __instance,
                    "Base Hack Speed",
                    "Total Hack Speed",
                    GeneralBuffs.GenSpeed * 2f * 100f,
                    "Faster Pace Modifier (Hack Speed)");

                // Wish Speed - only if that section exists in this page
                FasterPaceBreakdownUtil.InsertModifierLine(
                    __instance,
                    "Base Wish Speed",
                    "Total Wish Speed",
                    GeneralBuffs.GenSpeed * 2f * 100f,
                    "Faster Pace Modifier (Wish Speed)");
            }
        }

        // =========================================================
        // MISC ADVENTURE PAGE
        // Keep your respawn line here; add Drop Chance here too.
        // =========================================================

        [HarmonyPatch(typeof(StatsDisplay), "displayMiscAdventure")]
        internal static class Patch_StatsDisplay_DisplayMiscAdventure_RespawnExtraLine
        {
            private const float RespawnDivisor = 2f;

            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(StatsDisplay __instance)
            {
                if (__instance == null) return;
                if (__instance.statsBreakdown == null || __instance.statValue == null) return;

                string bd = __instance.statsBreakdown.text ?? "";
                string sv = __instance.statValue.text ?? "";

                // Respawn
                if (bd.Contains("Base Respawn Rate") && bd.Contains("Total Respawn Rate") &&
                    !bd.Contains("Faster Pace Modifier (Respawn)"))
                {
                    var bdLines = new List<string>(bd.Split('\n'));
                    var svLines = new List<string>(sv.Split('\n'));

                    int totalLine = bdLines.FindIndex(l => l.Contains("Total Respawn Rate"));
                    if (totalLine >= 0)
                    {
                        while (svLines.Count < bdLines.Count)
                            svLines.Add("");

                        bdLines.Insert(totalLine, "<b>Faster Pace Modifier (Respawn):</b> ");
                        svLines.Insert(totalLine, "x " + (100f / RespawnDivisor).ToString("0.##") + "%");

                        __instance.statsBreakdown.text = string.Join("\n", bdLines);
                        __instance.statValue.text = string.Join("\n", svLines);
                    }
                }

                // Refresh strings after respawn insertion
                bd = __instance.statsBreakdown.text ?? "";

                // Drop Chance / Loot Factor
                FasterPaceBreakdownUtil.InsertModifierLine(
                    __instance,
                    "Base Drop Chance",
                    "Total Drop Chance",
                    GeneralBuffs.GenSpeed * 2f * 100f,
                    "Faster Pace Modifier (Drop Chance)");

                FasterPaceBreakdownUtil.InsertModifierLine(
                    __instance,
                    "Base Loot Factor",
                    "Total Loot Factor",
                    GeneralBuffs.GenSpeed * 2f * 100f,
                    "Faster Pace Modifier (Drop Chance)");
            }
        }
    }
}
