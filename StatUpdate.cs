using HarmonyLib;
using UnityEngine;

namespace fasterPace
{
    [HarmonyPatch(typeof(AdventureController), "Start")]
    internal static class Patch_AdventureController_Start_FasterStatRefresh
    {
        private const float AdventureStatRefreshInterval = 0.05f;

        [HarmonyPostfix]
        private static void Postfix(AdventureController __instance)
        {
            if (__instance == null)
                return;

            __instance.CancelInvoke("updateAdventureStats");
            __instance.CancelInvoke("displayEnemyStats");

            __instance.InvokeRepeating("updateAdventureStats", 0f, AdventureStatRefreshInterval);
            __instance.InvokeRepeating("displayEnemyStats", 0f, AdventureStatRefreshInterval);
        }
    }
}