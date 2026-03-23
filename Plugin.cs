using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using static LootLevelPatches;

namespace fasterPace
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("jshepler.ngu.mods", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony _harmony;
        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> EnableSteamAchievements;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);

            EnableSteamAchievements = Config.Bind(
                "Steam Achievements",
                "EnableSteamAchievements",
                false,
                "Steam achievements are disabled by default as this mod can be considered cheaty. You may enable them at your own accord, but it is recommended to leave them off."
            );

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            _harmony.PatchAll();
            TryPatchJshepler(_harmony);
            FibPerkUnlockOverride.Install(_harmony);
            NewVersionMonitor.Init(this, Config);
        }

        private void OnGUI()
        {
            NewVersionMonitor.DrawGUI();
        }

        internal static void LogInfo(string msg) => Log?.LogInfo(msg);
        internal static void LogWarning(string msg) => Log?.LogWarning(msg);
        internal static void LogError(string msg) => Log?.LogError(msg);

        private static void TryPatchJshepler(Harmony h)
        {
            try
            {
                var t = AccessTools.TypeByName("jshepler.ngu.mods.ZoneDropsTooltip");
                if (t == null)
                {
                    LogInfo("[fasterPace] jshepler ZoneDropsTooltip not present. Skipping integration.");
                    return;
                }

                var m = AccessTools.Method(t, "BuildDropTable", new[] { typeof(int) });
                if (m == null)
                {
                    LogInfo("[fasterPace] jshepler ZoneDropsTooltip.BuildDropTable(int) not found. Skipping integration.");
                    return;
                }

                var postfix = AccessTools.Method(
                    typeof(Patch_JsheplerDropTableTooltip_AddEarlyZoneSetDrops),
                    "Postfix"
                );

                if (postfix == null)
                {
                    LogWarning("[fasterPace] Could not find Postfix method for jshepler integration.");
                    return;
                }

                h.Patch(m, postfix: new HarmonyMethod(postfix));
                LogInfo("[fasterPace] Patched jshepler ZoneDropsTooltip.BuildDropTable");
            }
            catch (Exception ex)
            {
                LogWarning("[fasterPace] Failed to patch jshepler tooltip: " + ex);
            }
        }
    }
}