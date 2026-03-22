using HarmonyLib;
using System.Text.RegularExpressions;
using UnityEngine;

namespace fasterPace
{
    [HarmonyPatch]
    internal static class Patch_ShowTitanTimer_FixGRB_GCT_Display
    {
        // Run after jshelper so we override the final message shown
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ButtonShower), "showTitanTimer")]
        [HarmonyAfter(new[] { "jshelper.ngu.mods" })]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(ButtonShower __instance, ref string ___message)
        {
            if (__instance == null || string.IsNullOrEmpty(___message)) return;

            var ch = __instance.character;
            if (ch?.adventure == null || ch.adventureController == null) return;

            // Compute correct remaining times (clamped at 0)
            float rem1 = ch.adventureController.boss1SpawnTime() - (float)ch.adventure.boss1Spawn.totalseconds;
            float rem2 = ch.adventureController.boss2SpawnTime() - (float)ch.adventure.boss2Spawn.totalseconds;

            if (rem1 < 0f) rem1 = 0f;
            if (rem2 < 0f) rem2 = 0f;

            string t1 = NumberOutput.timeOutput(rem1);
            string t2 = NumberOutput.timeOutput(rem2);

            // Replace ONLY the time part on those two lines
            ___message = ReplaceLineTime(___message, 58, "GRB", t1);
            ___message = ReplaceLineTime(___message, 66, "GCT", t2);

            // Ensure tooltip updates (jshelper already shows it; we override final)
            __instance.tooltip.showTooltip(___message);
        }

        private static string ReplaceLineTime(string msg, int id, string name, string newTime)
        {
            // Replaces only the trailing time
            var pattern = $@"(?m)^(?<pre>{id}:\s*Time until\s*{Regex.Escape(name)}\s*Spawn:\s*)(?<time>\d+:\d\d(?::\d\d)?)\s*$";
            return Regex.Replace(msg, pattern, m => m.Groups["pre"].Value + newTime);
        }
    }
}
