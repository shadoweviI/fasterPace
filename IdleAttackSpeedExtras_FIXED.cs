using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace fasterPace
{
    [HarmonyPatch]
    internal static class IdleAttackSpeedExtras
    {
        private const int BOTH_EDGY_BOOTS_ID = 220;
        private const int GREY_LIQUID_ID = 506;

        private const string PP_EDGY = "fasterPace.extraComplete.edgyBoots220";
        private const string PP_GREY = "fasterPace.extraComplete.greyLiquid506";

        private static Character _character;
        private static Character C => _character;

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void CacheCharacter(AdventureController __instance)
        {
            _character = __instance?.character;

            // Also force the correct speed on load/start immediately.
            if (_character != null)
                ApplyNow(_character);
        }

        private static bool GetFlag(string key) => PlayerPrefs.GetInt(key, 0) != 0;

        private static void SetFlag(string key)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }

        private static bool IsMaxxed(Character c, int itemId)
        {
            var maxxed = c?.inventory?.itemList?.itemMaxxed as IList<bool>;
            return maxxed != null && itemId >= 0 && itemId < maxxed.Count && maxxed[itemId];
        }

        private static float DesiredIdleAttackSpeed(Character c)
        {
            if (IsMaxxed(c, GREY_LIQUID_ID)) return 0.4f;
            if (IsMaxxed(c, BOTH_EDGY_BOOTS_ID)) return 0.6f;
            if (c?.inventory?.itemList?.redLiquidComplete ?? false) return 0.7f;
            return 0.8f;
        }

        /// <summary>
        /// Apply immediately to both the Adventure speed field and the live PlayerController timer.
        /// </summary>
        private static void ApplyNow(Character c)
        {
            if (c == null) return;

            float spd = DesiredIdleAttackSpeed(c);

            // Force vanilla to re-evaluate first.
            if (c.adventure != null)
            {
                c.adventure.setFasterIdleAttack();
                c.adventure.attackSpeed = spd;
            }

            // IMPORTANT:
            // The currently running combat timer can still be holding the old value.
            // Push the new speed into the live player controller immediately.
            try
            {
                var pc = UnityEngine.Object.FindObjectOfType<PlayerController>();
                if (pc != null)
                    pc.moveTimer = spd;
            }
            catch { }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Adventure), "setFasterIdleAttack")]
        private static void Postfix_SetFasterIdleAttack(Adventure __instance)
        {
            var c = C;
            if (c == null) return;
            if (c.adventure != null && c.adventure != __instance) return;

            __instance.attackSpeed = DesiredIdleAttackSpeed(c);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerController), nameof(PlayerController.usedMove))]
        private static void Postfix_PlayerController_UsedMove(PlayerController __instance)
        {
            var c = C;
            if (c == null || __instance == null) return;

            __instance.moveTimer = DesiredIdleAttackSpeed(c);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Postfix_AddOfflineProgress(Character __instance)
        {
            if (__instance == null) return;
            if (C == null) _character = __instance;

            ApplyNow(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllItemListController), "checkforBonuses")]
        private static void Postfix_CheckForBonuses_ExtraCompletes(AllItemListController __instance)
        {
            var c = __instance?.character;
            if (c?.inventory?.itemList == null) return;

            bool changed = false;

            // Grey Liquid first (highest priority)
            if (!GetFlag(PP_GREY) && IsMaxxed(c, GREY_LIQUID_ID))
            {
                SetFlag(PP_GREY);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out Grey Liquid, congrats!\n\nIdle Attack speed is now set to 0.4.",
                    5f
                );

                changed = true;
            }

            // Both Edgy Boots
            if (!GetFlag(PP_EDGY) && IsMaxxed(c, BOTH_EDGY_BOOTS_ID))
            {
                SetFlag(PP_EDGY);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out Both Edgy Boots, congrats!\n\nGlobal Attack speed is now set to 0.6.",
                    5f
                );

                changed = true;
            }

            // Apply after flags/tooltips so the highest-priority state wins cleanly.
            if (changed)
            {
                ApplyNow(c);
                c.refreshMenus();
            }
        }
    }
}