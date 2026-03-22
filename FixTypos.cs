using HarmonyLib;

namespace fasterPace
{
    [HarmonyPatch(typeof(Character), "Start")]
    internal static class Patch_ItemNames_1_39_AfterInit
    {
        [HarmonyPostfix]
        private static void Postfix(Character __instance)
        {
            var info = __instance?.itemInfo;
            var n = info?.itemName;
            if (n == null || n.Length <= 39) return;

            // Vanilla offenders in your ItemNameDesc.cs:
            // itemName[34]..[39] are "Special boost ..." (lowercase b).
            n[34] = "Special Boost 200";
            n[35] = "Special Boost 500";
            n[36] = "Special Boost 1000";
            n[37] = "Special Boost 2000";
            n[38] = "Special Boost 5000";
            n[39] = "Special Boost 10K";

            // itemName[26] is "Defense boost 10K" but the desc calls it Toughness.
            n[26] = "Toughness Boost 10K";
        }
    }
}
