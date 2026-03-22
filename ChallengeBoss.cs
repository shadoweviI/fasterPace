using HarmonyLib;

namespace fasterPace
{
    [HarmonyPatch(typeof(NGUChallengeController), nameof(NGUChallengeController.targetBoss))]
    internal static class Patch_NGUChallengeController_targetBoss
    {
        [HarmonyPrefix]
        private static bool Prefix(NGUChallengeController __instance, ref int __result)
        {
            var c = __instance.character;

            if (c.settings.rebirthDifficulty == difficulty.normal)
            {
                __result = 57 + __instance.completions() * 8;
                return false;
            }

            if (c.settings.rebirthDifficulty == difficulty.evil)
            {
                __result = 57 + __instance.evilCompletions() * 8;
                return false;
            }

            if (c.settings.rebirthDifficulty == difficulty.sadistic)
            {
                __result = 57 + __instance.sadisticCompletions() * 8;
                return false;
            }

            // fallback / future-proof
            __result = 57 + __instance.completions() * 8;
            return false;
        }
    }
}
