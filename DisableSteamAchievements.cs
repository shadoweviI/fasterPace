using HarmonyLib;
using System.Reflection;

namespace fasterPace
{
    internal static class SteamAchievementGate
    {
        internal static bool AchievementsEnabled => Plugin.EnableSteamAchievements != null && Plugin.EnableSteamAchievements.Value;

        internal static readonly FieldInfo FI_m_bStoreStats =
            AccessTools.Field(typeof(SteamManager), "m_bStoreStats");
    }

    // Block scanning/unlocking when disabled
    [HarmonyPatch(typeof(SteamManager), "checkAllAchievements")]
    internal static class Patch_SteamManager_CheckAllAchievements
    {
        [HarmonyPrefix]
        private static bool Prefix() => SteamAchievementGate.AchievementsEnabled;
    }

    [HarmonyPatch(typeof(SteamManager), "unlockAchievement")]
    internal static class Patch_SteamManager_UnlockAchievement
    {
        [HarmonyPrefix]
        private static bool Prefix() => SteamAchievementGate.AchievementsEnabled;
    }

    // Keep StoreStats from firing when disabled (optional but nice)
    [HarmonyPatch(typeof(SteamManager), "Update")]
    internal static class Patch_SteamManager_Update_StoreStatsGate
    {
        [HarmonyPostfix]
        private static void Postfix(SteamManager __instance)
        {
            if (SteamAchievementGate.AchievementsEnabled) return;
            if (__instance == null) return;

            try { SteamAchievementGate.FI_m_bStoreStats?.SetValue(__instance, false); }
            catch { }
        }
    }
}
