using HarmonyLib;

namespace fasterPace
{
    [HarmonyPatch(typeof(AllNGUController), nameof(AllNGUController.nguChallengeUnlocked))]
    internal static class Patch_AllNGUController_NGUChallengeUnlocked_EffectiveLevel
    {
        [HarmonyPrefix]
        private static bool Prefix(AllNGUController __instance, ref bool __result)
        {
            var c = __instance?.character;
            if (c?.NGU == null)
            {
                __result = false;
                return false; 
            }

            float effectiveLevel = NoNGUSoftcaps.EffNGULevel;
            if (effectiveLevel <= 0f) effectiveLevel = 1f;

            long num = 0L;

            // Regular NGUs
            for (int i = 0; i < c.NGU.skills.Count; i++)
            {
                num += (long)(c.NGU.skills[i].level * effectiveLevel);
                if (num >= 10000L)
                {
                    __result = true;
                    return false;
                }
            }

            // Magic NGUs
            for (int j = 0; j < c.NGU.magicSkills.Count; j++)
            {
                num += (long)(c.NGU.magicSkills[j].level * effectiveLevel);
                if (num >= 10000L)
                {
                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false; 
        }

        // Stub 
        private static float GetEffectiveLevel(Character c)
        {
            return 1f;
        }
    }
}
