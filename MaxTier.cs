using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CardTagsPanel
    {
        [HarmonyPrefix, HarmonyPatch(typeof(CardsController), "updateTagText")]
        private static bool CardsController_updateTagText_prefix(int tagID, CardsController __instance)
        {
            if (!__instance.tagPanelShown)
                return false;

            var bonusType = (cardBonus)tagID;
            var bonusName = __instance.getShortBonusName(bonusType);
            var tier = __instance.generateCardTier(bonusType);

            __instance.tagUI[tagID - 1].tagText.text = $"  {bonusName} ({tier})";
            __instance.tagUI[tagID - 1].tagText.alignment = UnityEngine.TextAnchor.MiddleLeft;

            return false;
        }
    }
    }