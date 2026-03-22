using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace fasterPace
{
    [HarmonyPatch(typeof(LootDrop))]
    internal static class Patch_ZoneItemsFromNormals
    {
        // Your rule:
        // Zones 1..16: normal enemies can also drop zone items (boss set) at reduced rate (cap 10%)
        private const int MaxZone = 16;
        private const float CapChance = 0.33f;
        private const float NormalEnemyScale = 0.33f; // "reduced rate" (10% of boss-y rate)

        // Item IDs bosses roll for each zone (pulled from LootDrop.zoneXDrop makeLevelledLoot calls)
        // Note: some early zones have no set-items rolled in their zone drop method => omitted.
        private static readonly Dictionary<int, float> ZoneCapChance = new()
        {
            { 1, 0.11f },
            { 2, 0.11f },
            { 3, 0.11f },
            { 4, 0.11f },
            { 5, 0.22f },
            { 7, 0.22f },
            { 9, 0.22f },
            { 10, 0.22f },
            { 12, 0.22f },
            { 13, 0.33f },
            { 15, 0.33f },
        };


        private static readonly Dictionary<int, int[]> ZoneToBossSetItems = new()
        {
            { 1,  new[] { 40, 41, 42, 43, 44, 45, 46, 77 } },
            { 2,  new[] { 47, 48, 49, 50, 51, 52, 53 } },
            { 3,  new[] { 54, 55, 56, 57, 58, 59, 60, 61 } },
            { 4,  new[] { 1, 14, 27 } },
            { 5,  new[] { 68, 69, 70, 71, 72, 73, 74 } },
            { 7,  new[] { 85, 86, 87, 88, 89, 90, 91 } },
            { 9,  new[] { 95, 96, 97, 98, 99, 100, 101 } },
            { 10, new[] { 103, 104, 105, 106, 107, 108, 109 } },
            { 12, new[] { 122, 123, 124, 125, 126 } },
            { 13, new[] { 130, 131, 132, 133, 134 } },
            { 15, new[] { 143, 144, 145, 146, 147 } },
        };

        // Boss “chance coefficient” (the multiplier you see in the boss drop block)
        // Example: zone1’s boss block uses 0.65f * lootFactor():contentReference[oaicite:1]{index=1}
        private static readonly Dictionary<int, float> ZoneBossCoeff = new()
        {
            { 1,  0.65f },
            { 2,  0.50f },
            { 3,  0.75f },
            { 4,  0.40f },
            { 5,  0.40f },
            { 7,  0.30f },
            { 9,  0.32f },
            { 10, 0.3f },
            { 12, 0.2f },
            { 13, 0.08f },
            { 15, 0.01f },
        };

        // Patch ALL zone drop methods with one shared postfix
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone1Drop))] private static void Z1(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 1);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone2Drop))] private static void Z2(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 2);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone3Drop))] private static void Z3(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 3);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone4Drop))] private static void Z4(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 4);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone5Drop))] private static void Z5(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 5);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone6Drop))] private static void Z6(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 6);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone7Drop))] private static void Z7(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 7);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone8Drop))] private static void Z8(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 8);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone9Drop))] private static void Z9(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 9);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone10Drop))] private static void Z10(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 10);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone11Drop))] private static void Z11(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 11);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone12Drop))] private static void Z12(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 12);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone13Drop))] private static void Z13(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 13);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone14Drop))] private static void Z14(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 14);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone15Drop))] private static void Z15(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 15);
        [HarmonyPostfix][HarmonyPatch(nameof(LootDrop.zone16Drop))] private static void Z16(LootDrop __instance, Enemy enemy) => TryExtra(__instance, enemy, 16);

        private static void TryExtra(LootDrop ld, Enemy enemy, int zone)
        {
            if (ld == null || enemy == null) return;
            if (zone < 1 || zone > MaxZone) return;

            // Only normal enemies
            if (enemy.enemyType != enemyType.normal) return;

            // Must have mapping for this zone
            if (!ZoneToBossSetItems.TryGetValue(zone, out var items) || items == null || items.Length == 0)
                return;

            // Compute chance: (boss coefficient * lootFactor) scaled down, capped at 10%
            float lootFactor = ld.character != null ? ld.character.lootFactor() : 0f;

            ZoneBossCoeff.TryGetValue(zone, out float coeff);
            ZoneCapChance.TryGetValue(zone, out float cap);
            if (cap <= 0f) cap = 0.10f; // fallback cap if you forget a zone

            float chance = Mathf.Min(cap, (coeff * lootFactor) * NormalEnemyScale);


            if (chance <= 0f) return;

            // Roll
            if (UnityEngine.Random.value >= chance) return;

            // Drop one of the boss set items at your global default level.
            // If your mod clamps in makeLevelledLoot anyway, keep this low (like 4).
            int id = items[UnityEngine.Random.Range(0, items.Length)];
            int level;
            ZoneDropScope.Enter();
            try
            {
                level = LootLevelPatches.GetZoneMinLevel(ld.character);
            }
            finally
            {
                ZoneDropScope.Exit();
            }
            ld.log.AddEvent(enemy.name + " also dropped " + ld.itemInfo.makeLevelledLoot(id, level) + ld.itemInfo.endRemark());
        }
    }
}
