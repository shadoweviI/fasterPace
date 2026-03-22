using HarmonyLib;

namespace fasterPace
{
    [HarmonyPatch(typeof(AdventureController), "spawnEnemy")]
    internal static class Patch_Walderp_AlwaysRealAfterUnlock
    {
        // Vanilla uses boss5Kills >= 3 as the “real Walderp unlocked” gate (same as autokill logic).
        private const int REAL_UNLOCK_KILLS = 3;
        private const int ZONE_WALDERP = 16;
        private const int REAL_INDEX = 4; // enemyList[16][4] = real titan (id 310) in your project

        [HarmonyPrefix]
        private static bool Prefix(AdventureController __instance, int zone, ref Enemy __result)
        {
            if (zone != ZONE_WALDERP) return true;

            var c = __instance?.character;
            var adv = c?.adventure;
            var list = __instance?.enemyList;

            if (adv == null || list == null) return true;
            if (list.Count <= ZONE_WALDERP) return true;
            if (list[ZONE_WALDERP] == null || list[ZONE_WALDERP].Count <= REAL_INDEX) return true;

            // Only force real titan once the account/rebirth has unlocked it
            if (adv.boss5Kills < REAL_UNLOCK_KILLS) return true;

            // Keep internal state consistent so other waldo logic doesn’t keep thinking you’re mid-chain
            if (adv.waldoDefeats < REAL_INDEX) adv.waldoDefeats = REAL_INDEX;

            __result = list[ZONE_WALDERP][REAL_INDEX];
            return false; // skip vanilla spawn selection
        }
    }
}
