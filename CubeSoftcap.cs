using HarmonyLib;

[HarmonyPatch]
internal static class WesternCubeDivisor4
{
    private const int PERK_26 = 26;
    private const float TARGET_NUM = 25f;

    private static bool WesternComplete(Character c)
        => c?.inventory?.itemList != null && c.inventory.itemList.westernComplete;

    private static long PerkLevelSafe(Character c, int id)
    {
        try
        {
            var list = c?.adventure?.itopod?.perkLevel;
            if (list == null) return 0;
            if (id < 0 || id >= list.Count) return 0;
            return list[id];
        }
        catch { return 0; }
    }

    // Mirrors game's pattern:
    // float num = 100f; if(perk26>=1) num=50f; num /= wishesDivider;
    private static float GetVanillaNum(Character c)
    {
        float num = (PerkLevelSafe(c, PERK_26) >= 1L) ? 50f : 100f;

        try
        {
            var wishes = c?.wishesController;
            if (wishes != null)
                num /= wishes.totalBoostRatioDivider();
        }
        catch
        {
            // ignore
        }

        return num;
    }

    private struct CubeState
    {
        public float powerBefore;
        public float toughBefore;
        public float vanillaNum;
        public bool apply;
    }

    private static void ApplyScaledDelta(Character c, ref CubeState st)
    {
        if (!st.apply) return;
        if (st.vanillaNum <= 0f) return;

        float powerAfter = c.inventory.cubePower;
        float toughAfter = c.inventory.cubeToughness;

        float dP = powerAfter - st.powerBefore;
        float dT = toughAfter - st.toughBefore;

        // Only scale positive gains
        if (dP <= 0f && dT <= 0f) return;

        float scale = st.vanillaNum / TARGET_NUM;
        if (scale <= 1f) return;

        if (dP > 0f) c.inventory.cubePower = st.powerBefore + dP * scale;
        if (dT > 0f) c.inventory.cubeToughness = st.toughBefore + dT * scale;
    }

    // ─────────────────────────────────────────────
    // 1) Offline progress: scales cube gains done inside addOfflineProgress
    // ─────────────────────────────────────────────
    [HarmonyPatch(typeof(Character), "addOfflineProgress")]
    private static class Patch_Character_AddOfflineProgress_WesternCube
    {
        private static void Prefix(Character __instance, ref CubeState __state)
        {
            __state = default;

            if (__instance?.inventory == null) return;
            if (!WesternComplete(__instance)) return;

            __state.apply = true;
            __state.powerBefore = __instance.inventory.cubePower;
            __state.toughBefore = __instance.inventory.cubeToughness;
            __state.vanillaNum = GetVanillaNum(__instance);
        }

        private static void Postfix(Character __instance, ref CubeState __state)
        {
            if (__instance?.inventory == null) return;
            ApplyScaledDelta(__instance, ref __state);
        }
    }

    // ─────────────────────────────────────────────
    // 2) Boost -> cube conversion: InventoryController.infinityCubeBoost
    // ─────────────────────────────────────────────
    [HarmonyPatch(typeof(InventoryController), "infinityCubeBoost")]
    private static class Patch_InventoryController_InfinityCubeBoost_WesternCube
    {
        private static void Prefix(InventoryController __instance, ref CubeState __state)
        {
            __state = default;

            var c = __instance?.character;
            if (c?.inventory == null) return;
            if (!WesternComplete(c)) return;

            __state.apply = true;
            __state.powerBefore = c.inventory.cubePower;
            __state.toughBefore = c.inventory.cubeToughness;
            __state.vanillaNum = GetVanillaNum(c);
        }

        private static void Postfix(InventoryController __instance, ref CubeState __state)
        {
            var c = __instance?.character;
            if (c?.inventory == null) return;
            ApplyScaledDelta(c, ref __state);
        }
    }
}