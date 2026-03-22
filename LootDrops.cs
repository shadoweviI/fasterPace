using fasterPace;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

internal static class ForestPendantOverride
{
    public const int ID = 53;

    public static int LevelForZone(Character c)
    {
        int z = c?.adventure?.zone ?? -1;
        return z switch
        {
            2 => LootLevelPatches.DEFAULT_MIN_LEVEL,
            3 => LootLevelPatches.DEFAULT_MIN_LEVEL + 5,
            4 => LootLevelPatches.DEFAULT_MIN_LEVEL + 10,
            5 => LootLevelPatches.DEFAULT_MIN_LEVEL + 15,
            _ => -1
        };
    }
}



internal static class ZoneDropScope
{
    [ThreadStatic] private static int _depth;
    public static bool Active => _depth > 0;

    public static void Enter() => _depth++;
    public static void Exit()
    {
        if (_depth > 0) _depth--;
    }
}

[HarmonyPatch(typeof(InventoryController), nameof(InventoryController.checkItemTransform))]
internal static class Patch_InventoryController_CheckItemTransform_Pending
{
    [HarmonyPostfix]
    private static void Postfix(int __result)
    {
        // If the game says "transform into X", remember X for the next loot creation
        if (__result > 0)
            LootLevelContext.SetPendingTransform(__result);
    }
}


[HarmonyPatch(typeof(InventoryController), nameof(InventoryController.swapItems))]
internal static class Patch_InventoryController_SwapItems_AscensionContext
{
    [HarmonyPrefix]
    private static void Prefix(InventoryController __instance, ref bool __state)
    {
        __state = false;

        var c = __instance?.character;
        var inv = c?.inventory?.inventory;
        if (inv == null) return;

        int item1 = c.inventory.item1;
        int item2 = c.inventory.item2;

        if (item1 < 0 || item2 < 0 || item1 == item2) return;
        if (item1 >= inv.Count || item2 >= inv.Count) return;

        // Only relevant when this swap is actually a merge/transform situation
        if (!__instance.mergeable(inv[item1], inv[item2])) return;

        int t1 = __instance.checkItemTransform(inv[item1]);
        int t2 = __instance.checkItemTransform(inv[item2]);

        if (t1 > 0) LootLevelContext.SetPendingTransform(t1);
        if (t2 > 0) LootLevelContext.SetPendingTransform(t2);

    }

    [HarmonyPostfix]
    private static void Postfix(bool __state)
    {
        if (__state)
            LootLevelContext.ExitAscension();
    }
}

internal static class LootLevelContext
{
    [ThreadStatic] private static int _ascendDepth;
    [ThreadStatic] private static HashSet<int> _pending;

    public static bool InAscension => _ascendDepth > 0;

    public static void SetPendingTransform(int newId)
    {
        if (newId <= 0) return;
        _pending ??= new HashSet<int>();
        _pending.Add(newId);
    }

    public static bool TryEnterForCreatedId(int createdId)
    {
        if (createdId <= 0) return false;
        if (_pending == null) return false;

        if (_pending.Remove(createdId))
        {
            _ascendDepth++;
            return true;
        }
        return false;
    }

    public static void ExitAscension()
    {
        if (_ascendDepth > 0) _ascendDepth--;
    }
}

[HarmonyPatch(typeof(LootDrop))]
internal static class Patch_LootDrop_ZoneScope
{
    private static void Enter() => ZoneDropScope.Enter();
    private static void Exit() => ZoneDropScope.Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone0Drop))] private static void Z0_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone0Drop))] private static void Z0_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone1Drop))] private static void Z1_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone1Drop))] private static void Z1_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone2Drop))] private static void Z2_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone2Drop))] private static void Z2_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone15Drop))] private static void Z15_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone15Drop))] private static void Z15_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone17Drop))] private static void Z17_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone17Drop))] private static void Z17_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone18Drop))] private static void Z18_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone18Drop))] private static void Z18_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone20Drop))] private static void Z20_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone20Drop))] private static void Z20_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone21Drop))] private static void Z21_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone21Drop))] private static void Z21_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone22Drop))] private static void Z22_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone22Drop))] private static void Z22_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone24Drop))] private static void Z24_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone24Drop))] private static void Z24_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone25Drop))] private static void Z25_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone25Drop))] private static void Z25_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone27Drop))] private static void Z27_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone27Drop))] private static void Z27_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone28Drop))] private static void Z28_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone28Drop))] private static void Z28_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone29Drop))] private static void Z29_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone29Drop))] private static void Z29_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone31Drop))] private static void Z31_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone31Drop))] private static void Z31_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone32Drop))] private static void Z32_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone32Drop))] private static void Z32_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone33Drop))] private static void Z33_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone33Drop))] private static void Z33_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone35Drop))] private static void Z35_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone35Drop))] private static void Z35_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone36Drop))] private static void Z36_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone36Drop))] private static void Z36_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone37Drop))] private static void Z37_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone37Drop))] private static void Z37_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone39Drop))] private static void Z39_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone39Drop))] private static void Z39_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone40Drop))] private static void Z40_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone40Drop))] private static void Z40_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone41Drop))] private static void Z41_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone41Drop))] private static void Z41_Post() => Exit();

    [HarmonyPrefix, HarmonyPatch(nameof(LootDrop.zone43Drop))] private static void Z43_Pre() => Enter();
    [HarmonyPostfix, HarmonyPatch(nameof(LootDrop.zone43Drop))] private static void Z43_Post() => Exit();
}

[HarmonyPatch(typeof(ItemNameDesc))]
internal static class LootLevelPatches
{


    internal const int DEFAULT_MIN_LEVEL = 5;



    internal static int GetZoneMinLevel(Character c)
    {
        int baseMin = DEFAULT_MIN_LEVEL;

        // Only apply special zone minimums during actual zone-drop methods
        if (ZoneDropScope.Active)
        {
            int z = (c != null && c.adventureController != null) ? c.adventureController.zone : -1;

            if (z == 0) baseMin = 45 + DEFAULT_MIN_LEVEL;
            else if (z == 1) baseMin = 20 + DEFAULT_MIN_LEVEL;
            else if (z == 2) baseMin = 6 + DEFAULT_MIN_LEVEL;
            else if (z == 15) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 17) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 18) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 20) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 21) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 22) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 24) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 25) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 27) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 28) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 29) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 31) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 32) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 33) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 35) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 36) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 37) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 39) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 40) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 41) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else if (z == 43) baseMin = 1 + DEFAULT_MIN_LEVEL;
            else baseMin = DEFAULT_MIN_LEVEL;
        }

        int bonus = 0; 

        // Keep whatever bonus logic you already had:
        if (c?.adventure?.itopod?.perkLevel != null && c.adventure.itopod.perkLevel.Count > 25)
        {
            if (c.adventure.itopod.perkLevel[25] > 0)
                bonus += 1;
        }

        if (c?.adventure?.itopod?.perkLevel != null && c.adventure.itopod.perkLevel.Count > 97)
        {
            if (c.adventure.itopod.perkLevel[94] >= 1597)
                bonus += 1;
        }

        if (c?.inventory?.itemList != null && c.inventory.itemList.gaudyComplete)
            bonus += 1; 

        return baseMin + bonus;
    }

    [HarmonyPatch(typeof(LootDrop), "titanLevelBonus")]
    internal static class Patch_LootDrop_TitanLevelBonus_AddOnlyBonus
    {
        [HarmonyPostfix]
        private static void Postfix(LootDrop __instance, ref int __result)
        {
            var c = __instance?.character;
            if (c == null) return;

            int bonus = 0;

            // EXACTLY the "bonus" part (no baseMin / no zone logic)
            if (c?.adventure?.itopod?.perkLevel != null && c.adventure.itopod.perkLevel.Count > 25)
            {
                if (c.adventure.itopod.perkLevel[25] > 0)
                    bonus += 1;
            }

            if (c?.adventure?.itopod?.perkLevel != null && c.adventure.itopod.perkLevel.Count > 94)
            {
                if (c.adventure.itopod.perkLevel[94] >= 1597)
                    bonus += 1;
            }

            if (c?.inventory?.itemList != null && c.inventory.itemList.gaudyComplete)
                bonus += 1;

            __result += bonus;

            if (__result < 0)
                __result = 0;
        }
    }

    internal static float DefaultMinLevelRuntime()
    {
        return DEFAULT_MIN_LEVEL;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(LootDrop), "macGuffinThreshold")]
    private static void LootDrop_macGuffinThreshold_Postfix(LootDrop __instance, ref long __result)
    {
        var c = __instance?.character;
        if (c == null) return;

        int baseMin = DEFAULT_MIN_LEVEL;
        int bonus = BonusLootLevelsOnly(c);

        float div = baseMin + bonus;
        if (div <= 1f) return;

        __result = (long)Mathf.Ceil(__result / div);
        if (__result < 1) __result = 1;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(LootDrop), "killsPerMacguffin")]
    private static void LootDrop_killsPerMacguffin_Postfix(LootDrop __instance, ref int __result)
    {
        var c = __instance?.character;
        if (c == null) return;

        // Reuse your existing logic
        int baseMin = LootLevelPatches.DEFAULT_MIN_LEVEL;
        int bonus = BonusLootLevelsOnly(c); // or GetZoneMinLevel(c) - baseMin

        int div = baseMin + bonus;
        if (div <= 1) return;

        __result = Mathf.CeilToInt(__result / (float)div);

        if (__result < 1)
            __result = 1;
    }

    [HarmonyPatch(typeof(ItemNameDesc), nameof(ItemNameDesc.bonusLootLevels))]
    internal static class Patch_BonusLootLevels_DisableChance
    {
        [HarmonyPrefix]
        private static bool Prefix(ref int __result)
        {
            __result = 0; 
            return false;   
        }
    }

    [HarmonyPatch(typeof(ItopodPerkController), nameof(ItopodPerkController.fibPerkUnlocks))]
    internal static class Patch_FibPerkUnlockText
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ci in instructions)
            {
                if (ci.opcode == OpCodes.Ldstr && ci.operand is string s)
                {
                    if (s == "\n<b>Level 144: </b>+5% Chance for +1 level on Loot!")
                    {
                        ci.operand = "\n<b>Level 144: </b>FIBONACCI KITTY ART";
                    }
                    else if (s == "\n<b>Level 1597: </b>FIBONACCI KITTY ART")
                    {
                        ci.operand = "\n<b>Level 1597: </b>+1 level on Loot!";
                    }
                    else if (s == "\n<b>Level 1597: </b>?????? (COSMETIC)")
                    {
                        ci.operand = "\n<b>Level 1597: </b>??????";
                    }
                }

                yield return ci;
            }
        }

        [HarmonyPatch(typeof(ItopodPerkController), nameof(ItopodPerkController.fibPerkUnlocks))]
        internal static class Patch_FibPerkUnlocks_RecolorUnlocked
        {
            private static readonly int[] FibLevels =
            {
            1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597
        };

            [HarmonyPostfix]
            private static void Postfix(ItopodPerkController __instance, ref string __result)
            {
                if (string.IsNullOrEmpty(__result) || __instance == null)
                    return;

                long cur = __instance.character.adventure.itopod.perkLevel[94];

                foreach (int lvl in FibLevels)
                {
                    if (cur < lvl)
                        continue;

                    // Color only the text AFTER "Level X: "
                    __result = Regex.Replace(
                        __result,
                        $@"(\n<b>Level {lvl}: </b>)([^\n<][^\n]*)",
                        "$1<color=green>$2</color>"
                    );
                }
            }
        }
    }

    internal static class FibPerkUnlockOverride
    {
        internal static void Install(Harmony harmony)
        {
            var original = AccessTools.Method(typeof(ItopodPerkController), nameof(ItopodPerkController.fibPerkUnlocks));
            if (original == null)
            {
                Debug.Log("[fasterPace] Could not find ItopodPerkController.fibPerkUnlocks");
                return;
            }

            // Remove only jshep's rewrite of the unlock list
            harmony.Unpatch(original, HarmonyPatchType.Postfix, "jshepler.ngu.mods");

            Debug.Log("[fasterPace] Removed jshep FibonacciPerks postfix from fibPerkUnlocks");
        }
    }

    // IMPORTANT: Fibonacci perk is ID 94 in vanilla (not 25).
    [HarmonyPatch(typeof(ItopodPerkController), nameof(ItopodPerkController.Start))]
    internal static class Patch_ItopodPerkText_Fibonacci
    {
        [HarmonyPostfix]
        private static void Postfix(ItopodPerkController __instance)
        {
            if (__instance?.perkName == null || __instance.perkDesc == null) return;

            const int id = 94; // Fibonacci perk
            if (__instance.perkName.Count <= id || __instance.perkDesc.Count <= id) return;

            __instance.perkName[id] = "The Loot Goblin's Blessing";
            __instance.perkDesc[id] =
                "Gain the blessings of a fat green loot goblin!\n" +
                "At level 144: unlock Fibonacci Kitty Art.\n" +
                "At level 1597: +1 level on ALL drops (stacks with Gaudy).";
        }
    }



    [HarmonyPatch(typeof(ItopodPerkController), "Start")]
    internal static class Patch_ItopodPerkText_25
    {
        [HarmonyPostfix]
        private static void Postfix(ItopodPerkController __instance)
        {
            if (__instance?.perkName == null || __instance.perkDesc == null) return;

            const int id = 25;
            if (__instance.perkName.Count <= id || __instance.perkDesc.Count <= id) return;

            __instance.perkName[id] = "The Loot Goblin's Blessing";
            __instance.perkDesc[id] =
                "Gain the blessings of a fat green loot goblin! This perk gives you +1 level on ALL drops! This stacks with the Gaudy Set Bonus.";
        }
    }

    [HarmonyPatch(typeof(ItopodPerkController), "Start")]
    internal static class Patch_ItopodPerk25_Tuning
    {
        private const int PERK_25 = 25;

        [HarmonyPostfix]
        private static void Postfix(ItopodPerkController __instance)
        {
            if (__instance == null) return;

            // safety checks
            if (__instance.cost == null || __instance.maxLevel == null || __instance.perkDifficultyReq == null) return;
            if (__instance.cost.Count <= PERK_25 || __instance.maxLevel.Count <= PERK_25 || __instance.perkDifficultyReq.Count <= PERK_25) return;

            __instance.cost[PERK_25] = 1000L;
            __instance.maxLevel[PERK_25] = 1L;
            __instance.perkDifficultyReq[PERK_25] = difficulty.evil;

            // Refresh if we are currently in the perks menu
            __instance.updateMenu();
        }
    }

    [HarmonyPatch(typeof(ItemNameDesc), nameof(ItemNameDesc.addLoot))]
    internal static class Patch_ForestPendant_LevelPerZone_AddLoot
    {
        [HarmonyPrefix]
        private static void Prefix(ItemNameDesc __instance, Equipment loot)
        {
            if (loot == null) return;
            if (loot.id != ForestPendantOverride.ID) return;

            int lvl = ForestPendantOverride.LevelForZone(__instance?.character);
            if (lvl > 0) loot.level = lvl;
        }
    }

    internal static class BonusAccessoryOverride
    {
        // Put as many as you want here (432, 433, 434, ...)
        public static readonly HashSet<int> IDs = new HashSet<int>
    {
        432, 433, 434, 435, 436, 437, 
        438, 439, 440, 441
    };

        public static int LevelForZone(Character c)
        {
            const int BASE = 5; // your desired baseline for bonus accessories

            // "bonus only" part from your existing logic (perk/gaudy/etc)
            int bonus = LootLevelPatches.GetZoneMinLevel(c) - LootLevelPatches.DEFAULT_MIN_LEVEL;
            if (bonus < 0) bonus = 0;

            return BASE + bonus;
        }


    }


    [HarmonyPatch(typeof(ItemNameDesc), nameof(ItemNameDesc.addLoot))]
    internal static class Patch_BonusAccessories_LevelPerZone_AddLoot
    {
        [HarmonyPrefix]
        private static void Prefix(ItemNameDesc __instance, Equipment loot)
        {
            if (loot == null) return;
            if (!BonusAccessoryOverride.IDs.Contains(loot.id)) return;

            // Prevent the old bug: ascension/transform inherits current zone level.
            // If this loot was created via transform, don't override.
            if (LootLevelContext.InAscension) return;

            int lvl = BonusAccessoryOverride.LevelForZone(__instance?.character);
            if (lvl > 0) loot.level = lvl;
        }
    }
    // Quest items (IDs 278–287) should NOT scale with zone level.
    // They get: DEFAULT_MIN_LEVEL + BonusLootLevelsOnly(character)
    internal static readonly HashSet<int> QuestItemIds = new HashSet<int>
    {
    278, 279, 280, 281, 282, 283, 284, 285, 286, 287
    };


    private static void ForceLootLevel(Equipment eq, Character character)
    {
        // item 506 special-case: always level 0
        if (eq.id == 506)
        {
            eq.level = 0;
            return;
        }

        if (eq == null) return;

        // ✅ Forest Pendant override
        if (eq.id == ForestPendantOverride.ID)
        {
            int lvl = ForestPendantOverride.LevelForZone(character);
            if (lvl > 0) eq.level = lvl;
            return; // skip min-level clamp
        }

        // Quest items: fixed floor + ONLY bonus levels (no zone scaling)
        if (QuestItemIds.Contains(eq.id))
        {
            int floor = DEFAULT_MIN_LEVEL + BonusLootLevelsOnly(character);
            if (eq.level < floor) eq.level = floor;
            return;
        }

        // Don’t touch macguffins
        if (eq.type == part.MacGuffin)
        {
            int floor = DEFAULT_MIN_LEVEL + BonusLootLevelsOnly(character);
            if (eq.level < floor) eq.level = floor;
            return;
        }

        // Don’t touch forced 100s
        if (eq.level == 100) return;

        // Boosts: always level 4 no matter what zone
        // Boosts: fixed base floor + ONLY bonus levels (no titan/zone scaling)
        if (eq.type == part.atkBoost || eq.type == part.defBoost || eq.type == part.specBoost)
        {
            int floor = DEFAULT_MIN_LEVEL + BonusLootLevelsOnly(character);
            if (eq.level < floor) eq.level = floor;
            return;
        }


        // Real gear: zone-based minimum
        int minLevel = GetZoneMinLevel(character);
        if (eq.level < minLevel)
            eq.level = minLevel;
    }




    // --- Patch genLoot overloads (optional but good coverage) ---

    [HarmonyPatch(nameof(ItemNameDesc.makeLoot))]

    public static int BonusLootLevelsOnly(Character c)
    {
        int bonus = 0;

        // same bonus rules you already use elsewhere
        if (c?.adventure?.itopod?.perkLevel != null && c.adventure.itopod.perkLevel.Count > 25)
            if (c.adventure.itopod.perkLevel[25] > 0)
                bonus += 1;

        if (c?.adventure?.itopod?.perkLevel != null && c.adventure.itopod.perkLevel.Count > 94)
            if (c.adventure.itopod.perkLevel[94] >= 1597)
                bonus += 1;

        if (c?.inventory?.itemList?.gaudyComplete == true)
            bonus += 1;

        return bonus;
    }

    private static bool MakeLoot_Prefix(ItemNameDesc __instance, ref string __result, int id)
    {

        bool enteredAscension = LootLevelContext.TryEnterForCreatedId(id);
        try
        {
            if (id == 0)
            {
                __result = "";
                return false;
            }

            __instance.character.allItemList.markItemAsDropped(id);

            Equipment equipment = new Equipment(
                __instance.type[id], __instance.bossRequired[id],
                __instance.curAttack[id], __instance.capAttack[id],
                __instance.curDefense[id], __instance.capDefense[id],
                __instance.specType1[id], __instance.curSpec1[id], __instance.capSpec1[id],
                __instance.specType2[id], __instance.curSpec2[id], __instance.capSpec2[id],
                __instance.specType3[id], __instance.curSpec3[id], __instance.capSpec3[id],
                __instance.path[id], id
            );

            if (equipment != null)
            {
                // (keep ALL your filter code unchanged)

                // Zone-based gear, but boosts always 4
                if (equipment.type == part.atkBoost ||
                    equipment.type == part.defBoost ||
                    equipment.type == part.specBoost)
                {
                    if (equipment.level < 4) equipment.level = 4;
                }
                else if (equipment.type != part.MacGuffin)
                {
                    int minLevel = GetZoneMinLevel(__instance.character);
                    if (equipment.level < minLevel) equipment.level = minLevel;
                }

                if (equipment.level > 0 && equipment.level < 100)
                    equipment.level += __instance.bonusLootLevels();

                if (__instance.character.settings.autoTransform >= 1 &&
                    __instance.character.settings.autoTransform <= 3 &&
                    equipment.isBoost())
                {
                    Equipment t = __instance.autoTransform(equipment, __instance.character.settings.autoTransform);
                    if (t.id != 0) equipment = t;
                }

                int slot = __instance.addLoot(equipment);
                __instance.ic.updateItem(slot);
            }

            __result = __instance.itemName[id];
            return false;
        }
        finally
        {
            if (enteredAscension)
                LootLevelContext.ExitAscension();
        }
    }



    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemNameDesc), nameof(ItemNameDesc.makeLevelledLoot))]
    private static void MakeLevelledLoot_ClampPrefix(ItemNameDesc __instance, int id, ref int lootlevel)
    {
        // item 506 special-case: always level 0 (even during ascension/transform)
        if (id == 506)
        {
            bool enteredAscension = LootLevelContext.TryEnterForCreatedId(id);
            try
            {
                lootlevel = 0;
                return;
            }
            finally
            {
                if (enteredAscension)
                    LootLevelContext.ExitAscension();
            }
        }
        if (LootLevelContext.TryEnterForCreatedId(id))
        {
            // We only need the flag active during this clamp; exit immediately after adjusting.
            try
            {
                // force global default during ascension
                if (lootlevel < DEFAULT_MIN_LEVEL) lootlevel = DEFAULT_MIN_LEVEL;
                return;
            }
            finally
            {
                LootLevelContext.ExitAscension();
            }
        }

        if (LootDropOverrides.ExtraDropLevelScope.Active)
        {
            int floor = DEFAULT_MIN_LEVEL + BonusLootLevelsOnly(__instance.character);
            if (lootlevel < floor)
                lootlevel = floor;
            return;
        }

        var t = __instance.type[id];

        // Leave macguffins alone
        if (t == part.MacGuffin)
        {
            int floor = DEFAULT_MIN_LEVEL + BonusLootLevelsOnly(__instance.character);
            if (lootlevel < floor) lootlevel = floor;
            return;
        }

        if (QuestItemIds.Contains(id))
        {
            int floor = DEFAULT_MIN_LEVEL + BonusLootLevelsOnly(__instance.character);
            if (lootlevel < floor) lootlevel = floor;
            return;
        }

        // Boosts: always 4, ignore zone
        if (t == part.atkBoost || t == part.defBoost || t == part.specBoost)
        {
            int floor = DEFAULT_MIN_LEVEL + BonusLootLevelsOnly(__instance.character);
            if (lootlevel < floor) lootlevel = floor;
            return;
        }


        // Gear: zone-based minimum
        int minLevel = GetZoneMinLevel(__instance.character);
        if (lootlevel < minLevel)
            lootlevel = minLevel;

    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ItemNameDesc.genLoot), new Type[] { typeof(int) })]
    private static void GenLoot_Int_Postfix(ItemNameDesc __instance, ref Equipment __result)
    => ForceLootLevel(__result, __instance?.character);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ItemNameDesc.genLoot), new Type[] { typeof(int), typeof(bool) })]
    private static void GenLoot_IntBool_Postfix(ItemNameDesc __instance, ref Equipment __result)
    => ForceLootLevel(__result, __instance?.character);

    // --- Transpiler injection used by makeLoot/makeTitanLoot overloads ---
    private static IEnumerable<CodeInstruction> InjectAfterNewEquipmentLocal(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);

        var forceMethod = AccessTools.Method(typeof(LootLevelPatches), nameof(ForceLootLevel));
        if (forceMethod == null) throw new Exception("ForceLootLevel method not found.");

        for (int i = 0; i < list.Count; i++)
        {
            yield return list[i];

            // Detect "stloc.*" that stores the result of "newobj Equipment::.ctor"
            if (list[i].opcode.IsStloc() &&
                i > 0 &&
                list[i - 1].opcode == OpCodes.Newobj &&
                list[i - 1].operand is ConstructorInfo ci &&
                ci.DeclaringType == typeof(Equipment))
            {
                // After the local is assigned, load it and call ForceLootLevel(local)
                var local = GetStlocOperand(list[i]);
                var characterField = AccessTools.Field(typeof(ItemNameDesc), "character");

                yield return new CodeInstruction(OpCodes.Ldloc, local);     // Equipment
                yield return new CodeInstruction(OpCodes.Ldarg_0);          // this
                yield return new CodeInstruction(OpCodes.Ldfld, characterField); // this.character
                yield return new CodeInstruction(OpCodes.Call, forceMethod);

            }
        }
    }

    // makeLoot(int id)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemNameDesc.makeLoot), new Type[] { typeof(int) })]
    private static IEnumerable<CodeInstruction> MakeLoot_Transpiler(IEnumerable<CodeInstruction> instructions)
        => InjectAfterNewEquipmentLocal(instructions);

    // makeTitanLoot(int id)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemNameDesc.makeTitanLoot), new Type[] { typeof(int) })]
    private static IEnumerable<CodeInstruction> MakeTitanLoot_Transpiler(IEnumerable<CodeInstruction> instructions)
        => InjectAfterNewEquipmentLocal(instructions);

    // makeLoot(int id, int sid)  (this overload exists in your file)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemNameDesc.makeLoot), new Type[] { typeof(int), typeof(int) })]
    private static IEnumerable<CodeInstruction> MakeLoot_Slot_Transpiler(IEnumerable<CodeInstruction> instructions)
        => InjectAfterNewEquipmentLocal(instructions);

    // --- helpers ---
    private static object GetStlocOperand(CodeInstruction stloc)
    {
        // stloc.0/1/2/3 have no operand; use indexes
        if (stloc.opcode == OpCodes.Stloc_0) return 0;
        if (stloc.opcode == OpCodes.Stloc_1) return 1;
        if (stloc.opcode == OpCodes.Stloc_2) return 2;
        if (stloc.opcode == OpCodes.Stloc_3) return 3;

        // stloc / stloc.s carry operand (LocalBuilder or int)
        if (stloc.operand is LocalBuilder lb) return lb.LocalIndex;
        if (stloc.operand is int i) return i;

        throw new InvalidOperationException("Unsupported stloc form.");
    }
}

internal static class OpCodeExtensions
{
    public static bool IsStloc(this OpCode op) =>
        op == OpCodes.Stloc || op == OpCodes.Stloc_S ||
        op == OpCodes.Stloc_0 || op == OpCodes.Stloc_1 ||
        op == OpCodes.Stloc_2 || op == OpCodes.Stloc_3;
}

[HarmonyPatch(typeof(LootDrop), "zone1Drop")]
internal static class Patch_Zone1Drop_AlwaysExtra
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var ci in instructions)
        {
            // Replace the literal 0.1f with 1.0f
            if (ci.opcode == OpCodes.Ldc_R4 && ci.operand is float f && f == 0.1f)
            {
                ci.operand = 1f; // 100% chance
            }

            yield return ci;
        }
    }
}



[HarmonyPatch]
internal static class ForestPendantPerZoneLevels
{
    private const int PENDANT_ID = 53;

    // ✅ Set these however you want:
    private const int ZONE2_PENDANT_LEVEL = 5;
    private const int ZONE3_PENDANT_LEVEL = 10;
    private const int ZONE4_PENDANT_LEVEL = 15;
    private const int ZONE5_PENDANT_LEVEL = 20;

    private static readonly MethodInfo MI_makeLoot_1 =
        AccessTools.Method(typeof(ItemNameDesc), nameof(ItemNameDesc.makeLoot), new[] { typeof(int) });

    private static readonly MethodInfo MI_makeLevelledLoot =
        AccessTools.Method(typeof(ItemNameDesc), nameof(ItemNameDesc.makeLevelledLoot), new[] { typeof(int), typeof(int) });

    // ---------- Zone 2: convert makeLoot(53) -> makeLevelledLoot(53, ZONE2_PENDANT_LEVEL)

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(LootDrop), nameof(LootDrop.zone2Drop))]
    private static IEnumerable<CodeInstruction> Zone2_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);

        for (int i = 0; i < list.Count; i++)
        {
            // Look for: ldc.i4 53 ; callvirt ItemNameDesc::makeLoot(int)
            if (IsLoadInt(list[i], PENDANT_ID) &&
                i + 1 < list.Count &&
                IsCallTo(list[i + 1], MI_makeLoot_1))
            {
                // After pushing id(53), push level, then call makeLevelledLoot instead
                list.Insert(i + 1, new CodeInstruction(OpCodes.Ldc_I4, ZONE2_PENDANT_LEVEL));
                list[i + 2].operand = MI_makeLevelledLoot; // swap method target
                i += 2;
            }
        }

        return list;
    }

    // ---------- Zones 3/4/5: replace lootlevel only when id == 53

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(LootDrop), nameof(LootDrop.zone3Drop))]
    private static IEnumerable<CodeInstruction> Zone3_Transpiler(IEnumerable<CodeInstruction> instructions)
        => ReplacePendantLevel(instructions, ZONE3_PENDANT_LEVEL);

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(LootDrop), nameof(LootDrop.zone4Drop))]
    private static IEnumerable<CodeInstruction> Zone4_Transpiler(IEnumerable<CodeInstruction> instructions)
        => ReplacePendantLevel(instructions, ZONE4_PENDANT_LEVEL);

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(LootDrop), nameof(LootDrop.zone5Drop))]
    private static IEnumerable<CodeInstruction> Zone5_Transpiler(IEnumerable<CodeInstruction> instructions)
        => ReplacePendantLevel(instructions, ZONE5_PENDANT_LEVEL);

    private static IEnumerable<CodeInstruction> ReplacePendantLevel(IEnumerable<CodeInstruction> instructions, int newLevel)
    {
        var list = new List<CodeInstruction>(instructions);

        for (int i = 0; i < list.Count - 2; i++)
        {
            // Pattern: ldc.i4 53 ; ldc.i4 <oldLevel> ; callvirt makeLevelledLoot(int,int)
            if (IsLoadInt(list[i], PENDANT_ID) &&
                IsLoadIntAny(list[i + 1]) &&
                IsCallTo(list[i + 2], MI_makeLevelledLoot))
            {
                // Replace the level load with your new level
                list[i + 1] = new CodeInstruction(OpCodes.Ldc_I4, newLevel);
                i += 2;
            }
        }

        return list;
    }

    // ---------- small IL helpers

    private static bool IsCallTo(CodeInstruction ci, MethodInfo mi)
        => (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt) && ci.operand is MethodInfo m && m == mi;

    private static bool IsLoadIntAny(CodeInstruction ci)
    {
        if (ci.opcode == OpCodes.Ldc_I4 || ci.opcode == OpCodes.Ldc_I4_S) return true;
        return ci.opcode == OpCodes.Ldc_I4_M1 ||
               ci.opcode == OpCodes.Ldc_I4_0 ||
               ci.opcode == OpCodes.Ldc_I4_1 ||
               ci.opcode == OpCodes.Ldc_I4_2 ||
               ci.opcode == OpCodes.Ldc_I4_3 ||
               ci.opcode == OpCodes.Ldc_I4_4 ||
               ci.opcode == OpCodes.Ldc_I4_5 ||
               ci.opcode == OpCodes.Ldc_I4_6 ||
               ci.opcode == OpCodes.Ldc_I4_7 ||
               ci.opcode == OpCodes.Ldc_I4_8;
    }

    private static bool IsLoadInt(CodeInstruction ci, int value)
    {
        if (ci.opcode == OpCodes.Ldc_I4 && ci.operand is int i && i == value) return true;
        if (ci.opcode == OpCodes.Ldc_I4_S && ci.operand is sbyte sb && sb == (sbyte)value) return true;

        // handle short forms
        return (value switch
        {
            -1 => ci.opcode == OpCodes.Ldc_I4_M1,
            0 => ci.opcode == OpCodes.Ldc_I4_0,
            1 => ci.opcode == OpCodes.Ldc_I4_1,
            2 => ci.opcode == OpCodes.Ldc_I4_2,
            3 => ci.opcode == OpCodes.Ldc_I4_3,
            4 => ci.opcode == OpCodes.Ldc_I4_4,
            5 => ci.opcode == OpCodes.Ldc_I4_5,
            6 => ci.opcode == OpCodes.Ldc_I4_6,
            7 => ci.opcode == OpCodes.Ldc_I4_7,
            8 => ci.opcode == OpCodes.Ldc_I4_8,
            _ => false
        });
    }
}


