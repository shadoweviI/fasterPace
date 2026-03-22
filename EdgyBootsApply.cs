using HarmonyLib;
using System.Reflection;

namespace fasterPace
{
    // Make the "Boots Set" unlock apply immediately (no restart)
    [HarmonyPatch(typeof(AllItemListController), "checkforBonuses")]
    internal static class Patch_EdgyBootsSet_ApplyImmediately
    {
        // Cache MethodInfo once (reflection is expensive if spammed)
        private static readonly MethodInfo MI_CreateEnemyTable =
            AccessTools.Method(typeof(AdventureController), "createEnemyTable");

        [HarmonyPostfix]
        private static void Postfix(AllItemListController __instance)
        {
            var c = __instance?.character;
            if (c?.inventory?.itemList == null) return;

            // If the set JUST became complete, force-apply the unlock now.
            // (Even if some other mod/prefix prevented your original block from running,
            // this ensures the completion is detected and applied.)
            if (!c.inventory.itemList.edgyBootsComplete && c.inventory.itemList.maxxedEdgyBoots())
            {
                c.inventory.itemList.edgyBootsComplete = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Boots Set, Congrats! You've unlocked a special drop in The Evilverse!",
                    5f
                );

                // IMPORTANT PART: rebuild any cached Adventure tables so the new drop is active immediately
                TryRebuildAdventureTables(c);

                c.refreshMenus();
            }
        }

        private static void TryRebuildAdventureTables(Character c)
        {
            try
            {
                // Most likely the unlock is tied to AdventureController-built tables.
                // Rebuilding is effectively what a restart would have done.
                var ac = c.adventureController;
                if (ac != null && MI_CreateEnemyTable != null)
                    MI_CreateEnemyTable.Invoke(ac, null);
            }
            catch
            {
                // Swallow: worst case, it behaves like before (needs zone reload/restart)
            }
        }
    }
}
