using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace fasterPace
{
    /// <summary>
    /// Adds "set-style" completion text to:
    ///  - (220) BOTH Edgy Boots
    ///  - (506) Grey Liquid
    ///
    /// This is intentionally a *Postfix* on ItemListController.setBonusText so it works
    /// even if you already have a big Prefix that returns false (your rewritten file).
    /// </summary>
    [HarmonyPatch]
    internal static class ExtraSetBonusText_EdgyGrey
    {
        private static Character _character;

        // Cache Character (same pattern you already use elsewhere)
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void CacheCharacter(AdventureController __instance)
        {
            _character = __instance?.character;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItemListController), "setBonusText")]
        private static void ItemListController_setBonusText_Postfix(ItemListController __instance, ref string __result)
        {
            if (__instance == null) return;

            // Avoid double-inserting if setBonusText is called multiple times
            if (!string.IsNullOrEmpty(__result))
            {
                if (__instance.id == 220 && __result.Contains("Both Edgy Boots Bonus")) return;
                if (__instance.id == 506 && __result.Contains("Grey Liquid Bonus")) return;
            }

            // We want a "set-like" block, even though these aren't real sets in vanilla.
            if (__instance.id == 220)
            {
                __result = ( __result ?? "" ) + BuildSingleItemBonusBlock(
                    title: "Both Edgy Boots Bonus",
                    itemLine: "Item 220.",
                    bonusLine: "Global Attack speed is now set to <b>0.6</b> (faster than Red Liquid).",
                    isComplete: IsMaxxed(220)
                );
            }
            else if (__instance.id == 506)
            {
                __result = ( __result ?? "" ) + BuildSingleItemBonusBlock(
                    title: "Grey Liquid Bonus",
                    itemLine: "Item 506.",
                    bonusLine: "Global Attack speed is now set to <b>0.4</b> (overrides Both Edgy Boots / Red Liquid).",
                    isComplete: IsMaxxed(506)
                );
            }
        }

        private static bool IsMaxxed(int itemId)
        {
            var c = _character;
            var maxxed = c?.inventory?.itemList?.itemMaxxed; // List<bool> in the game
            return maxxed != null && itemId >= 0 && itemId < maxxed.Count && maxxed[itemId];
        }

        private static string BuildSingleItemBonusBlock(string title, string itemLine, string bonusLine, bool isComplete)
        {
            // Matches the style your existing setBonusText uses.
            // (Single-item “set”: completion just means this item is level 100 / maxxed.)
            string status = isComplete ? "\n<color=green><b>COMPLETE</b></color>" : "\n<color=red><b>NOT COMPLETE</b></color>";

            return
                "\n\n<b>" + title + ":</b>\n" +
                itemLine + "\n\n" +
                "<b>Completion Bonus (Item level 100):</b>\n" +
                bonusLine +
                status;
        }
    }
}
