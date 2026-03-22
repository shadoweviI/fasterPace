using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static fasterPace.DaycareSpeed;


namespace fasterPace
{
    [HarmonyPatch(typeof(ItopodPerkController), nameof(ItopodPerkController.drop2))]
    internal static class Drop2_Perk125
    {
        private const int PERK_125 = 125;

        [HarmonyPrefix]
        private static bool Prefix(ItopodPerkController __instance, ref float __result)
        {
            var c = __instance?.character;
            var perks = c?.adventure?.itopod?.perkLevel;

            if (c == null || perks == null)
                return true;

            // keep vanilla gate
            if (c.settings.rebirthDifficulty < difficulty.evil)
            {
                __result = 1f;
                return false;
            }

            long p125 = (perks.Count > PERK_125) ? perks[PERK_125] : 0L;
            if (p125 >= 1L)
            {
                __result =  10f; // 150% bonus
                // Plugin.LogInfo($"[FP][Perk125] drop2 -> {__result} (p125={p125})");
                return false;
            }

            return true; // no perk => vanilla logic
        }
    }

    internal class PerkModifications
    {
        // ===============================
        // Perk 125: Welcome to Evil Difficulty
        // - Stat5 (ATK/DEF) from +200% -> +1000% (3x -> 11x)
        // - Aug speed +20% (same hook perk 144 uses: aug1())
        // - Tooltip text update
        // ===============================
        private const int PERK_125 = 125;
        private const int PERK_144 = 144;

        private static long PerkLevel(Character c, int perkId)
        {
            var perks = c?.adventure?.itopod?.perkLevel;
            if (perks == null) return 0;
            if (perkId < 0 || perkId >= perks.Count) return 0;
            return perks[perkId];
        }

        private static bool HasPerk125EvilPlus(Character c)
        {
            if (c == null) return false;
            if (c.settings.rebirthDifficulty < difficulty.evil) return false;
            return PerkLevel(c, PERK_125) >= 1;
        }

        // 1) ATK/DEF perk (stat5Bonus) -> 11x when perk 125 is bought in Evil+
        [HarmonyPatch(typeof(ItopodPerkController), nameof(ItopodPerkController.stat5Bonus))]
        internal static class Patch_ItopodPerkController_stat5Bonus_Perk125
        {
            [HarmonyPostfix]
            private static void Postfix(ItopodPerkController __instance, ref float __result)
            {
                var c = __instance?.character;
                if (c == null) return;

                if (HasPerk125EvilPlus(c))
                {
                    __result = 11f; // 1 + 10 = +1000%
                }
            }
        }

        // 2) Aug speed +20%
        // Perk 144 uses aug1(), so we do too.
        // IMPORTANT: Do NOT stack with perk 144 (sadistic aug perk).
        [HarmonyPatch(typeof(ItopodPerkController), nameof(ItopodPerkController.aug1))]
        internal static class Patch_ItopodPerkController_aug1_Perk125
        {
            [HarmonyPostfix]
            private static void Postfix(ItopodPerkController __instance, ref float __result)
            {
                var c = __instance?.character;
                if (c == null) return;
                
                long p144 = PerkLevel(c, PERK_144);
                long p125 = PerkLevel(c, PERK_125);

                // If perk 144 applies (sadistic), keep vanilla behavior and DON'T stack perk 125
                if (c.settings.rebirthDifficulty >= difficulty.sadistic && p144 >= 1)
                    __result = 1.2f;

                // Apply perk 125 on evil+
                if (c.settings.rebirthDifficulty >= difficulty.evil && p125 >= 1)
                {
                    __result = 1.2f;
                }
            }
        }



        // 3) Tooltip text for perk 125
        [HarmonyPatch(typeof(ItopodPerkController), "Start")]
        internal static class Patch_ItopodPerkText_125
        {
            [HarmonyPostfix]
            private static void Postfix(ItopodPerkController __instance)
            {
                if (__instance?.perkName == null || __instance.perkDesc == null) return;
                if (__instance.perkName.Count <= PERK_125 || __instance.perkDesc.Count <= PERK_125) return;

                __instance.perkName[PERK_125] = "Welcome to Evil Difficulty";
                __instance.perkDesc[PERK_125] =
                    "Evil Difficulty kicking your ass? There there, buddy.\n" +
                    "Take this perk and receive a +1000% buff to Attack/Defense,\n" +
                    "1000% Drop Chance bonus, and +20% Aug speed.\n" +
                    "It'll be okay.";
            }
        }
        private static readonly int[] PERK_ID = [229, 230];
        internal const long NEW_MAX_LEVEL = 60;
        [HarmonyPatch(typeof(ItopodPerkController), "Start")]
        internal static class Patch_ItopodPerkText_34
        {
            [HarmonyPostfix]
            private static void Postfix(ItopodPerkController __instance)
            {
                if (__instance?.perkName == null || __instance.perkDesc == null) return;

                const int id = 34;
                if (__instance.perkName.Count <= id || __instance.perkDesc.Count <= id) return;

                __instance.perkName[id] = "Bonus Titan EXP!";
                __instance.perkDesc[id] =
                    "You like getting EXP from Titans? Get 50% EXP bonus for all Titans!";
            }
            [HarmonyPostfix]
            private static void Postfix1(ItopodPerkController __instance)
            {
                if (__instance?.perkName == null || __instance.perkDesc == null) return;

                const int id = 23;
                if (__instance.perkName.Count <= id || __instance.perkDesc.Count <= id) return;

                __instance.perkName[id] = "Golden Showers";
                __instance.perkDesc[id] =
                    "This perk grants 10% multiplier to all gold drops in adventure!";
            }

            [HarmonyPostfix]
            private static void Postfix2(ItopodPerkController __instance)
            {
                if (__instance?.perkName == null || __instance.perkDesc == null) return;

                const int id = 12;
                if (__instance.perkName.Count <= id || __instance.perkDesc.Count <= id) return;

                __instance.perkName[id] = "Boosted Boosts I";
                __instance.perkDesc[id] =
                    "Yo dawg, I heard you like boosts, so this perk will boost the power of all applied boosts by 2% per level!";
            }
            [HarmonyPatch(typeof(ItopodPerkController), nameof(ItopodPerkController.showTooltip))]
            internal static class Patch_ItopodPerkController_ShowTooltip_DynamicMacguffinText
            {
                private const int PERK_ID = 68;

                private sealed class State
                {
                    public bool changed;
                    public string oldDesc;
                }

                [HarmonyPrefix]
                private static void Prefix(ItopodPerkController __instance, int id, ref State __state)
                {
                    __state = null;

                    if (__instance == null) return;
                    if (id != PERK_ID) return;

                    if (__instance.perkDesc == null || __instance.perkDesc.Count <= id) return;

                    var ld = __instance.character?.adventureController?.lootDrop;
                    if (ld == null) return;

                    int kpm = ld.killsPerMacguffin();
                    if (kpm < 1) kpm = 1;

                    __state = new State
                    {
                        changed = true,
                        oldDesc = __instance.perkDesc[id]
                    };

                    string kpmText = kpm.ToString("N0");
                    __instance.perkDesc[id] =
                        $"This will unlock MacGuffin drops in the ITOPOD! Every {kpmText} kills, you'll obtain a random MacGuffin!";
                }

                [HarmonyPostfix]
                private static void Postfix(ItopodPerkController __instance, int id, State __state)
                {
                    if (__state == null || !__state.changed) return;
                    if (__instance?.perkDesc == null) return;
                    if (__instance.perkDesc.Count <= id) return;

                    __instance.perkDesc[id] = __state.oldDesc;
                }
            }
            [HarmonyPatch(typeof(WishesController), "showWishTooltip")]
            internal static class Patch_WishesController_ShowWishTooltip_FP_DynamicDesc
            {
                private const int WISH20 = 20; // rebirth
                private const int WISH27 = 27; // daycare

                // Your chosen value (matches your minRebirthTime patch)
                private const int RebirthWishSecondsPerLevel = 15;

                private sealed class State
                {
                    public object props;      // <-- no WishProperties type
                    public string oldDesc;
                }

                [HarmonyPrefix]
                private static void Prefix(WishesController __instance, int id, ref State __state)
                {
                    __state = null;

                    if (__instance == null) return;
                    if (id != WISH20 && id != WISH27) return;

                    // properties is a List<something>; we don't care what "something" is
                    var propsList = __instance.properties;
                    if (propsList == null || propsList.Count <= id) return;

                    object props = propsList[id];
                    if (props == null) return;

                    var tr = Traverse.Create(props);

                    string oldDesc = tr.Field("WishDesc").GetValue<string>();
                    if (string.IsNullOrEmpty(oldDesc)) return;

                    string newDesc = oldDesc;

                    if (id == WISH20)
                    {
                        // Your actual rebirth time does: ceil(base / GenSpeed)
                        // => effective seconds/level is (15 / GenSpeed)
                        float gs = GeneralBuffs.GenSpeed;
                        if (gs <= 0f) gs = 0.0001f;

                        float effSec = RebirthWishSecondsPerLevel / gs;
                        string effStr = effSec.ToString("0.##", CultureInfo.InvariantCulture);

                        newDesc = ReplaceNumberBeforePhrase(oldDesc, "seconds per level", effStr);
                    }
                    else // WISH27
                    {
                        // Base wish rate from effectPerLevel (usually 0.01 = 1%/lvl)
                        float epl = tr.Field("effectPerLevel").GetValue<float>();
                        float basePct = epl * 100f;

                        // Match YOUR gameplay scaling (WishFactor). If you don’t have this field,
                        // fallback to GenSpeed so at least it’s not stuck at 1%.
                        float wf = 1f;
                        try { wf = DaycareSpeed.Patch_AllGoldDiggerController_TotalDaycareBonus_FP.WishFactor; }
                        catch { wf = GeneralBuffs.GenSpeed; }

                        if (wf <= 0f) wf = 1f;

                        float effPct = basePct * wf;
                        string pctStr = effPct.ToString("0.###", CultureInfo.InvariantCulture) + "%";

                        newDesc = ReplaceNumberBeforePhrase(oldDesc, "daycare speed per level", pctStr);

                        // fallback if wording differs
                        if (newDesc == oldDesc)
                            newDesc = ReplaceNumberBeforePhrase(oldDesc, "daycare speed", pctStr);
                    }

                    if (newDesc == oldDesc) return;

                    __state = new State { props = props, oldDesc = oldDesc };
                    tr.Field("WishDesc").SetValue(newDesc);
                }

                [HarmonyPostfix]
                private static void Postfix(WishesController __instance, int id, State __state)
                {
                    if (__state == null || __state.props == null) return;
                    Traverse.Create(__state.props).Field("WishDesc").SetValue(__state.oldDesc);
                }

                private static string ReplaceNumberBeforePhrase(string text, string phrase, string replacementToken)
                {
                    int idx = text.IndexOf(phrase);
                    if (idx < 0) return text;

                    int end = idx - 1;
                    while (end >= 0 && char.IsWhiteSpace(text[end])) end--;

                    int start = end;
                    while (start >= 0)
                    {
                        char ch = text[start];
                        if (char.IsDigit(ch) || ch == '.' || ch == ',' || ch == '%')
                            start--;
                        else
                            break;
                    }
                    start++;

                    if (start > end) return text;

                    return text.Substring(0, start) + replacementToken + text.Substring(end + 1);
                }
            }
            [HarmonyPostfix]
            private static void Postfix3(ItopodPerkController __instance)
            {
                if (__instance?.perkName == null || __instance.perkDesc == null) return;

                const int id = 27;
                if (__instance.perkName.Count <= id || __instance.perkDesc.Count <= id) return;

                __instance.perkName[id] = "Daycare Kitty's Blessing I";
                __instance.perkDesc[id] =
                    "Make the Daycare kitty even happier, and she'll grow your daycare items 2% faster per level! :)";
            }
            [HarmonyPostfix]
            private static void Postfix4(ItopodPerkController __instance)
            {
                if (__instance?.perkName == null || __instance.perkDesc == null) return;

                const int id = 28;
                if (__instance.perkName.Count <= id || __instance.perkDesc.Count <= id) return;

                __instance.perkName[id] = "Daycare Kitty's Blessing II";
                __instance.perkDesc[id] =
                    "Make the Daycare kitty even MORE happier, and she'll grow your daycare items an additional 2% faster per level! :) :)";
            }
            [HarmonyPatch(typeof(ItopodPerkController), nameof(ItopodPerkController.statEffect))]
            internal static class Patch_ItopodPerkController_statEffect_Perk23
            {
                // Match whatever your new design is
                private const int PERK_ID = 23;
                [HarmonyPrefix]
                private static bool Prefix(ItopodPerkController __instance, int id, int offset, ref float __result)
                {
                    if (__instance == null || id != PERK_ID) return true;

                    var c = __instance.character;
                    if (c?.adventure?.itopod?.perkLevel == null) return true;
                    if (id < 0 || id >= c.adventure.itopod.perkLevel.Count) return true;

                    // Vanilla formula: 1 + (perkLevel + offset) * effectPerLevel[id]
                    // Your wanted formula: 1 + (perkLevel + offset) * effectPerLevel[id] * GOLD_MULT_AT_LEVEL
                    long lvl = c.adventure.itopod.perkLevel[id] + offset;
                    __result = 1f + (float)lvl * __instance.effectPerLevel[id] * Perk34_AllAlways_6PerFill.GOLD_MULT_AT_LEVEL;
                    return false; // skip vanilla
                }
            }

            [HarmonyPatch(typeof(ItopodPerkController), nameof(ItopodPerkController.statEffect))]
            internal static class Patch_ItopodPerkController_statEffect_Perk27
            {
                // Match whatever your new design is
                private const int PERK_ID = 27;
                [HarmonyPrefix]
                private static bool Prefix(ItopodPerkController __instance, int id, int offset, ref float __result)
                {
                    if (__instance == null || id != PERK_ID) return true;

                    var c = __instance.character;
                    if (c?.adventure?.itopod?.perkLevel == null) return true;
                    if (id < 0 || id >= c.adventure.itopod.perkLevel.Count) return true;

                    // Vanilla formula: 1 + (perkLevel + offset) * effectPerLevel[id]
                    // Your wanted formula: 1 + (perkLevel + offset) * effectPerLevel[id] * GOLD_MULT_AT_LEVEL
                    long lvl = c.adventure.itopod.perkLevel[id] + offset;
                    __result = 1f + (float)lvl * __instance.effectPerLevel[id] * 2;
                    return false; // skip vanilla
                }
            }

            [HarmonyPatch(typeof(ItopodPerkController), nameof(ItopodPerkController.statEffect))]
            internal static class Patch_ItopodPerkController_statEffect_Perk28
            {
                // Match whatever your new design is
                private const int PERK_ID = 28;
                [HarmonyPrefix]
                private static bool Prefix(ItopodPerkController __instance, int id, int offset, ref float __result)
                {
                    if (__instance == null || id != PERK_ID) return true;

                    var c = __instance.character;
                    if (c?.adventure?.itopod?.perkLevel == null) return true;
                    if (id < 0 || id >= c.adventure.itopod.perkLevel.Count) return true;

                    // Vanilla formula: 1 + (perkLevel + offset) * effectPerLevel[id]
                    // Your wanted formula: 1 + (perkLevel + offset) * effectPerLevel[id] * GOLD_MULT_AT_LEVEL
                    long lvl = c.adventure.itopod.perkLevel[id] + offset;
                    __result = 1f + (float)lvl * __instance.effectPerLevel[id] * 2;
                    return false; // skip vanilla
                }
            }

            [HarmonyPatch(typeof(ArbitraryController), "displayTooltip")]
            internal static class Patch_ArbitraryController_DisplayTooltip_DaycareSpeedBoost
            {
                private const int DaycareSpeedBoostId = 32;

                [HarmonyPrefix]
                private static bool Prefix(ArbitraryController __instance)
                {
                    if (__instance == null || __instance.tooltip == null) return true;
                    if (__instance.id != DaycareSpeedBoostId) return true;

                    // Replace the hover tooltip text for Daycare Speed Boost
                    __instance.tooltip.showTooltip("If bought, items placed in the Daycare will gain levels 30% faster! :o");
                    return false; // skip vanilla displayTooltip (which shows __instance.tooltipMessage)
                }
            }

            [HarmonyPatch(typeof(ArbitraryController), "OnPointerEnter")]
            internal static class Patch_APShop_TooltipMessage_Hearts
            {
                [HarmonyPrefix]
                private static void Prefix(ArbitraryController __instance, PointerEventData eventData)
                {
                    if (__instance == null) return;

                    // Purple Heart (shop entry id 42 -> item 212)
                    if (__instance.id == 42)
                    {
                        __instance.tooltipMessage =
                            "<b>When this heart reaches level 100,</b>\n" +
                            "<b>MacGuffin time factor is multiplied by 20%!</b>\n" +
                            "<b>AND MacGuffin Threshold is reduced by 20% (if unlocked)!</b>";
                    }
                }
            }

            // ─────────────────────────────────────────────
            // Perk 34: gold drop bonus (separate multiplier)
            // ─────────────────────────────────────────────
            [HarmonyPatch(typeof(ItopodPerkController), "goldDrop1Bonus")]
            internal static class Patch_Itopod_TotalGoldDropBonus_Perk34
            {
                [HarmonyPrefix]
                private static bool Postfix(ItopodPerkController __instance, ref float __result)
                {
                    __result = 1f + (float)__instance.character.adventure.itopod.perkLevel[23] * __instance.effectPerLevel[23] * Perk34_AllAlways_6PerFill.GOLD_MULT_AT_LEVEL;
                    return false;
                }

            }


            [HarmonyPatch(typeof(BeastQuestPerkController), "Start")]
            internal static class Patch_Quirk13_Text
            {
                [HarmonyPostfix]
                private static void Postfix(BeastQuestPerkController __instance)
                {
                    if (__instance?.quirkName == null || __instance.quirkDesc == null) return;

                    const int id = 13;

                    if (__instance.quirkName.Count <= id || __instance.quirkDesc.Count <= id) return;

                    // Title (optional)
                    __instance.quirkName[id] = "The Beast's Fertilizer";

                    // Description (edit this)
                    __instance.quirkDesc[id] =
                        "The Beast will make you a mix tape you can play to your fruits as they grow, consisting of the beast wailing at them in yggdrasil-speak to grow faster. Each tier will take 20 seconds less to grow!";
                }
            }

            [HarmonyPatch(typeof(BeastQuestPerkController), "Start")]
            internal static class Patch_BeastQuestPerkController_Quirk92_YggYield_1PercentPerLevel
            {
                private const int QUIRK_ID = 92;
                private const float NEW_PER_LEVEL = 0.01f; // 1% per level

                [HarmonyPostfix]
                private static void Postfix(BeastQuestPerkController __instance)
                {
                    if (__instance?.effectPerLevel == null) return;
                    if (__instance.effectPerLevel.Count <= QUIRK_ID) return;

                    __instance.effectPerLevel[QUIRK_ID] = NEW_PER_LEVEL;

                    // Optional: update description text if you want the tooltip to reflect it
                    // if (__instance.quirkDesc != null && __instance.quirkDesc.Count > QUIRK_ID)
                    //     __instance.quirkDesc[QUIRK_ID] = __instance.quirkDesc[QUIRK_ID].Replace("0.1%", "1%");
                }
            }

            [HarmonyPatch(typeof(BeastQuestPerkController), "Start")]
            internal static class Patch_BeastQuestPerks_BeastedBoosts_Tuning
            {
                // IDs from your screenshots:
                private const int BOOSTS_I = 11; // Beasted Boosts I
                private const int BOOSTS_II = 53; // Beasted Boosts II (evil)
                private const int BOOSTS_III = 72; // Beasted Boosts III (evil)
                private const int BOOSTS_IV = 73; // Beasted Boosts IV (sadistic)

                private const long NEW_MAX_LEVEL = 60;

                private const float PER_LEVEL_EVIL = 0.02f;     // 2% per level
                private const float PER_LEVEL_SAD = 0.01f;     // 1% per level (sadistic)

                private static void Postfix(BeastQuestPerkController __instance)
                {
                    if (__instance == null) return;

                    // Make sure lists exist and are large enough
                    EnsureSize(__instance.maxLevel, BOOSTS_IV + 1, 0L);
                    EnsureSize(__instance.effectPerLevel, BOOSTS_IV + 1, 0f);

                    // Max levels
                    __instance.maxLevel[BOOSTS_I] = NEW_MAX_LEVEL;
                    __instance.maxLevel[BOOSTS_II] = NEW_MAX_LEVEL;
                    __instance.maxLevel[BOOSTS_III] = NEW_MAX_LEVEL;
                    __instance.maxLevel[BOOSTS_IV] = NEW_MAX_LEVEL;

                    // Per-level scaling (statEffect = 1 + level * effectPerLevel)
                    __instance.effectPerLevel[BOOSTS_I] = PER_LEVEL_EVIL;
                    __instance.effectPerLevel[BOOSTS_II] = PER_LEVEL_EVIL;
                    __instance.effectPerLevel[BOOSTS_III] = PER_LEVEL_EVIL;
                    __instance.effectPerLevel[BOOSTS_IV] = PER_LEVEL_SAD;

                    // If the menu is open, refresh so MAX/current/next update immediately.
                    __instance.refreshMenu();
                }

                private static void EnsureSize<T>(List<T> list, int size, T fill)
                {
                    if (list == null) return;
                    while (list.Count < size) list.Add(fill);
                }
            }

            [HarmonyPatch(typeof(ItopodPerkController), "Start")]
            internal static class Patch_ItopodPerk12_BoostedBoosts_Nerf
            {
                private const int PERK_ID = 12;
                private const float NEW_EFFECT_PER_LEVEL = 0.02f; // 2% per level
                private const int NEW_MAX_LEVEL = 60;

                [HarmonyPostfix]
                private static void Postfix(ItopodPerkController __instance)
                {
                    if (__instance == null) return;

                    // Ensure lists are large enough
                    __instance.effectPerLevel ??= new List<float>();
                    __instance.maxLevel ??= new List<long>();

                    while (__instance.effectPerLevel.Count <= PERK_ID)
                        __instance.effectPerLevel.Add(0f);

                    while (__instance.maxLevel.Count <= PERK_ID)
                        __instance.maxLevel.Add(0L);

                    // Apply changes
                    __instance.effectPerLevel[PERK_ID] = NEW_EFFECT_PER_LEVEL;
                    __instance.maxLevel[PERK_ID] = NEW_MAX_LEVEL;
                }
            }

            [HarmonyPatch(typeof(BeastQuestPerkController), "Start")]
            internal static class Patch_BeastQuestPerkController_QuirkText
            {
                [HarmonyPostfix]
                private static void Postfix(BeastQuestPerkController __instance)
                {
                    if (__instance?.quirkName == null || __instance.quirkDesc == null) return;

                    Apply(__instance, 11, "Beasted Boosts I",
                        "The Beast said they'll squirt another random fluid onto boosts applied to equipment, and that'll make them boostier! Gain 2% better boosts per level of this Quirk!");

                    Apply(__instance, 53, "Beasted Boosts II",
                        "The Beast just can't get enough of quirting random fluids onto boosts! Gain 2% better boosts per level of this quirk!");

                    Apply(__instance, 72, "Beasted Boosts III",
                        "The Beast REALLY can't get enough of quirting random fluids onto boosts! Gain 2% better boosts per level of this quirk!");

                    Apply(__instance, 73, "Beasted Boosts IV",
                        "The Beast has a problem. Gain 1% better boosts per level of this quirk!");
                }

                private static void Apply(BeastQuestPerkController inst, int id, string name, string desc)
                {
                    if (id < 0) return;
                    if (inst.quirkName.Count <= id || inst.quirkDesc.Count <= id) return;

                    inst.quirkName[id] = name;
                    inst.quirkDesc[id] = desc;
                }
            }

            [HarmonyPrefix]
            private static void Prefix(ItopodPerkController __instance)
            {
                if (__instance == null) return;

                __instance.maxLevel ??= new List<long>();

                int maxId = 0;
                for (int i = 0; i < PERK_ID.Length; i++)
                    if (PERK_ID[i] > maxId) maxId = PERK_ID[i];

                while (__instance.maxLevel.Count <= maxId)
                    __instance.maxLevel.Add(0L);

                for (int i = 0; i < PERK_ID.Length; i++)
                    __instance.maxLevel[PERK_ID[i]] = NEW_MAX_LEVEL;
            }
        }
    }
}
