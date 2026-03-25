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
        private static PlayerController _playerController;
        private static float _lastAppliedSpeed = -1f;

        private static Character C => _character;

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void CacheCharacter(AdventureController __instance)
        {
            _character = __instance?.character;
            CachePlayerController();
            ForceApplyNow(_character);
        }

        private static void CachePlayerController()
        {
            try
            {
                if (_playerController == null)
                    _playerController = Object.FindObjectOfType<PlayerController>();
            }
            catch { }
        }

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
        {
            try { return PlayerPrefs.GetInt(SaveScopedKey(c, key), 0) != 0; }
            catch { return false; }
        }

        private static void SetFlag(Character c, string key)
        {
            try
            {
                PlayerPrefs.SetInt(SaveScopedKey(c, key), 1);
                PlayerPrefs.Save();
            }
            catch { }
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

        private static void ForceApplyNow(Character c)
        {
            if (c == null) return;

            float spd = DesiredIdleAttackSpeed(c);
            _lastAppliedSpeed = spd;

            try
            {
                if (c.adventure != null)
                    c.adventure.attackSpeed = spd;
            }
            catch { }

            CachePlayerController();

            try
            {
                if (_playerController != null)
                    _playerController.moveTimer = spd;
            }
            catch
            {
                _playerController = null;
            }
        }

        private static void ApplyNowIfChanged(Character c)
        {
            if (c == null) return;

            float spd = DesiredIdleAttackSpeed(c);
            if (Mathf.Abs(_lastAppliedSpeed - spd) < 0.0001f)
                return;

            ForceApplyNow(c);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Adventure), "setFasterIdleAttack")]
        private static void Postfix_SetFasterIdleAttack(Adventure __instance)
        {
            var c = C;
            if (c == null) return;
            if (c.adventure != null && c.adventure != __instance) return;

            __instance.attackSpeed = DesiredIdleAttackSpeed(c);
            _lastAppliedSpeed = __instance.attackSpeed;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerController), nameof(PlayerController.usedMove))]
        private static void Postfix_PlayerController_UsedMove(PlayerController __instance)
        {
            var c = C;
            if (c == null || __instance == null) return;

            _playerController = __instance;
            __instance.moveTimer = DesiredIdleAttackSpeed(c);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Postfix_AddOfflineProgress(Character __instance)
        {
            if (__instance == null) return;
            if (C == null) _character = __instance;

            ForceApplyNow(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllItemListController), "checkforBonuses")]
        private static void Postfix_CheckForBonuses_ExtraCompletes(AllItemListController __instance)
        {
            var c = __instance?.character;
            var il = c?.inventory?.itemList;
            if (il == null) return;

            bool edgyNow = IsMaxxed(c, BOTH_EDGY_BOOTS_ID);
            bool greyNow = IsMaxxed(c, GREY_LIQUID_ID);

            bool showEdgyPopup = false;
            bool showGreyPopup = false;

            if (greyNow && !GetFlag(c, PP_GREY))
            {
                SetFlag(c, PP_GREY);
                showGreyPopup = true;
            }

            if (edgyNow && !GetFlag(c, PP_EDGY))
            {
                SetFlag(c, PP_EDGY);
                showEdgyPopup = true;
            }

            ApplyNowIfChanged(c);

            if (showGreyPopup)
            {
                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out Grey Liquid, congrats!\n\nIdle Attack speed is now set to 0.4.",
                    5f
                );
            }
            else if (showEdgyPopup)
            {
                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out Both Edgy Boots, congrats!\n\nGlobal Attack speed is now set to 0.6.",
                    5f
                );
            }
        }
    }
}