using HarmonyLib;
using UnityEngine;

namespace fasterPace
{
    internal static partial class InvSetBonuses
    {
        private const int SlotsPerSet = 12;

        // NEW versioned keys so old broken flags do not block awards.
        private const string KEY_MIGRATION_PREFIX = "fasterPace.invset.migration.v2";

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

        private static int GetAppliedCount(Character c)
        {
            try { return PlayerPrefs.GetInt(SaveScopedKey(c, KEY_MIGRATION_PREFIX), 0); }
            catch { return 0; }
        }



        private static void SetAppliedCount(Character c, int value)
        {
            try
            {
                PlayerPrefs.SetInt(SaveScopedKey(c, KEY_MIGRATION_PREFIX), value);
                PlayerPrefs.Save();
            }
            catch { }
        }

        internal static void RefreshUI(Character c)
        {
            try { c?.inventoryController?.updateInvCount(); } catch { }
            try { c?.allArbitrary?.updateMenu(); } catch { }
            try { c?.refreshMenus(); } catch { }
        }

        internal static void GrantInvSpaces(Character c, int amount)
        {
            if (c == null || c.arbitrary == null || amount <= 0)
                return;

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
                RefreshAfterSetReward(c);
            }
        }

        private static void RefreshAfterSetReward(Character c)
        {
            try { c?.inventoryController?.updateInvCount(); } catch { }
            try { c?.inventoryController?.updateBonuses(); } catch { }
            try { c?.allArbitrary?.updateMenu(); } catch { }
            try { c?.refreshMenus(); } catch { }
        }

        internal static void GrantSetRewardNow(Character c)
        {
            GrantInvSpaces(c, SlotsPerSet);
            RefreshAfterSetReward(c);
        }

        private static int CountCompletedEligibleSets(Character c)
        {
            var il = c?.inventory?.itemList;
            if (il == null) return 0;

            int n = 0;
            if (il.trainingComplete) n++;
            if (il.sewersComplete) n++;
            if (il.forestComplete) n++;
            if (il.caveComplete) n++;
            if (il.HSBComplete) n++;
            // include only if intended:
            // if (il.tutorialCubeComplete) n++;

            return n;
        }

        // One-time backfill for already-complete saves.
        private static void SyncAlreadyCompletedSets(Character c)
        {
            if (c == null || c.inventory?.itemList == null || c.arbitrary == null)
                return;

            int completed = CountCompletedEligibleSets(c);
            int applied = GetAppliedCount(c);

            if (applied < 0) applied = 0;
            if (applied > completed) applied = completed;

            int missing = completed - applied;
            if (missing > 0)
            {
                GrantInvSpaces(c, missing * SlotsPerSet);
                SetAppliedCount(c, completed);
            }

            RefreshUI(c);
        }

        [HarmonyPatch(typeof(ImportExport), "loadData")]
        internal static class Patch_ImportExport_LoadData_SyncInvSetBonuses
        {
            [HarmonyPostfix]
            private static void Postfix(ImportExport __instance)
            {
                try
                {
                    SyncAlreadyCompletedSets(__instance?.character);
                }
                catch { }
            }
        }
    }
}