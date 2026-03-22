using HarmonyLib;
using UnityEngine;

namespace fasterPace
{
    internal static partial class InvSetBonuses
    {
        private const int SlotsPerSet = 12;

        private const string KEY_TRAINING = "fasterPace.invset.training";
        private const string KEY_SEWERS = "fasterPace.invset.sewers";
        private const string KEY_FOREST = "fasterPace.invset.forest";
        private const string KEY_CAVE = "fasterPace.invset.cave";
        private const string KEY_HSB = "fasterPace.invset.hsb";
        private const string KEY_TUTORIALCUBE = "fasterPace.invset.tutorialCube";

        private static string SaveScopedKey(Character c, string key)
        {
            string name = "unknown";
            string plat = "unknownPlat";

            try
            {
                if (c != null && !string.IsNullOrEmpty(c.playerName))
                    name = c.playerName;
            }
            catch { }

            try
            {
                if (c != null)
                    plat = c.platform.ToString();
            }
            catch { }

            return $"{key}.{plat}.{name}";
        }

        private static bool GetFlag(Character c, string key)
            => PlayerPrefs.GetInt(SaveScopedKey(c, key), 0) != 0;

        private static void SetFlag(Character c, string key)
        {
            PlayerPrefs.SetInt(SaveScopedKey(c, key), 1);
            PlayerPrefs.Save();
        }

        // Add slots safely (clamped to vanilla max if available)
        private static void GrantInvSpaces(Character c, int amount)
        {
            if (c == null || amount <= 0) return;

            long max = long.MaxValue;
            try
            {
                var rac = c.allArbitrary?.randomArbitraryController;
                if (rac != null) max = rac.maxSpaces();
            }
            catch { }

            long cur = c.arbitrary.inventorySpaces;
            long next = cur + amount;

            if (next > max) next = max;
            if (next < 0) next = 0;

            if (next != cur)
            {
                c.arbitrary.inventorySpaces = (int)next;
                c.inventoryController?.updateInvCount();
            }
        }

        private static void GrantIfCompleteAndNotAwarded(Character c, bool isComplete, string key)
        {
            if (c == null) return;
            if (!isComplete) return;
            if (GetFlag(c, key)) return;

            GrantInvSpaces(c, SlotsPerSet);
            SetFlag(c, key);
        }

        [HarmonyPatch(typeof(AllItemListController), nameof(AllItemListController.checkforBonuses))]
        internal static class Patch_AllItemListController_CheckForBonuses_EnsureInvSetSlots
        {
            [HarmonyPostfix]
            private static void Postfix(AllItemListController __instance)
            {
                try
                {
                    var c = __instance?.character;
                    var il = c?.inventory?.itemList;
                    if (c == null || il == null) return;

                    GrantIfCompleteAndNotAwarded(c, il.trainingComplete, KEY_TRAINING);
                    GrantIfCompleteAndNotAwarded(c, il.sewersComplete, KEY_SEWERS);
                    GrantIfCompleteAndNotAwarded(c, il.forestComplete, KEY_FOREST);
                    GrantIfCompleteAndNotAwarded(c, il.caveComplete, KEY_CAVE);
                    GrantIfCompleteAndNotAwarded(c, il.HSBComplete, KEY_HSB);

                    // Remove this line if tutorial cube should NOT grant slots.
                    GrantIfCompleteAndNotAwarded(c, il.tutorialCubeComplete, KEY_TUTORIALCUBE);
                }
                catch { }
            }
        }
    }
}