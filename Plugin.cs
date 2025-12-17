using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace fasterPace
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource _logger;
        internal static void LogInfo(string text) => _logger.LogInfo(text);

        private void Awake()
        {
            _logger = base.Logger;
            _harmony.PatchAll();

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
