using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace fasterPace
{
    /// <summary>
    /// 1) Swaps existing zone/titan drop items to different ids/levels (and can cap them).
    /// 2) Adds extra drops to bosses/titans only, with custom levels and caps.
    ///
    /// Works by:
    /// - Setting a DropContext while LootDrop.zoneXXDrop(enemy) runs
    /// - Rewriting args to ItemNameDesc.makeLevelledLoot / makeTitanLevelledLoot
    /// - Postfixing LootDrop.zoneXXDrop to add extra drops after vanilla logic
    /// </summary>
    [HarmonyPatch]
    internal static class LootDropOverrides
    {
        // --------------------------
        // Context (current zone call)
        // --------------------------
        private struct Ctx
        {
            public int Zone;              // 0..n from method name zoneXXDrop
            public enemyType EnemyType;   // enemy.enemyType
        }

        [ThreadStatic] private static Ctx _ctx;
        [ThreadStatic] private static bool _ctxActive;

        private static bool InZoneDrop => _ctxActive && _ctx.Zone >= 0;
        private static bool IsNormalEnemy => _ctxActive && _ctx.EnemyType == enemyType.normal;

        private static bool IsBossOrTitanEnemy
        {
            get
            {
                // "boss-only" in your wording means: NOT normal mobs.
                // This includes bosses, waldo variants, bigBoss (titans), etc.
                return _ctxActive && _ctx.EnemyType != enemyType.normal;
            }
        }

        // --------------------------
        // Your rules / data tables
        // --------------------------

        // Swap an existing drop (originalId) into a new item (newId) at a custom level,
        // optionally capped (skip if already have >= cap copies).
        private sealed class SwapRule
        {
            public int Zone;        // exact zone; -1 = any
            public bool TitanOnly;  // only on makeTitanLevelledLoot calls
            public bool BossOnly;   // only if enemyType != normal
            public int OriginalId;
            public int NewId;
            public int NewLevel;
        }

        private sealed class ExtraDropRule
        {
            public int Zone;           // exact zone; -1 = any
            public bool TitanOnly;     // only titans
            public bool BossOnly;      // only if enemyType != normal
            public int ItemId;
            public int Level;

            public float Chance;       // base chance 0..1
            public bool UseLootFactor; // multiply by loot factor
            public float MaxChance;    // 0 = no cap; else clamp final chance <= MaxChance
        }


        private static readonly List<SwapRule> Swaps = new List<SwapRule>()
        {
            new SwapRule { Zone = 6, TitanOnly = true, BossOnly = false, OriginalId = 53, NewId = 76, NewLevel = 0 },
            new SwapRule { Zone = 8, TitanOnly = true, BossOnly = false, OriginalId = 53, NewId = 76, NewLevel = 0 },
            new SwapRule { Zone = 8, TitanOnly = true, BossOnly = false, OriginalId = 92, NewId = 92, NewLevel = 0 },
            new SwapRule { Zone = 11, TitanOnly = true, BossOnly = false, OriginalId = 76, NewId = 76, NewLevel = 20 },
            new SwapRule { Zone = 11, TitanOnly = true, BossOnly = false, OriginalId = 197, NewId = 197, NewLevel = 0 },
            new SwapRule { Zone = 14, TitanOnly = true, BossOnly = false, OriginalId = 141, NewId = 141, NewLevel = 0 },
            new SwapRule { Zone = 16, TitanOnly = true, BossOnly = false, OriginalId = 76, NewId = 94, NewLevel = 0 },
            new SwapRule { Zone = 19, TitanOnly = true, BossOnly = false, OriginalId = 142, NewId = 142, NewLevel = 15 },
            new SwapRule { Zone = 21, TitanOnly = false, BossOnly = true, OriginalId = 142, NewId = 142, NewLevel = 5 },
            new SwapRule { Zone = 22, TitanOnly = false, BossOnly = true, OriginalId = 142, NewId = 142, NewLevel = 30 },
            new SwapRule { Zone = 23, TitanOnly = true, BossOnly = false, OriginalId = 294, NewId = 294, NewLevel = 5 },
            new SwapRule { Zone = 23, TitanOnly = true, BossOnly = false, OriginalId = 170, NewId = 170, NewLevel = 5 },
            new SwapRule { Zone = 24, TitanOnly = false, BossOnly = true, OriginalId = 142, NewId = 170, NewLevel = 0 },
            new SwapRule { Zone = 24, TitanOnly = false, BossOnly = true, OriginalId = 128, NewId = 169, NewLevel = 0 },
            new SwapRule { Zone = 25, TitanOnly = false, BossOnly = true, OriginalId = 142, NewId = 170, NewLevel = 3 },
            new SwapRule { Zone = 25, TitanOnly = false, BossOnly = true, OriginalId = 128, NewId = 169, NewLevel = 3 },
            new SwapRule { Zone = 26, TitanOnly = true, BossOnly = false, OriginalId = 170, NewId = 229, NewLevel = 0 },
            new SwapRule { Zone = 26, TitanOnly = true, BossOnly = false, OriginalId = 169, NewId = 230, NewLevel = 0 },
            new SwapRule { Zone = 26, TitanOnly = true, BossOnly = false, OriginalId = 343, NewId = 343, NewLevel = 0 },
            new SwapRule { Zone = 27, TitanOnly = false, BossOnly = true, OriginalId = 128, NewId = 229, NewLevel = 0 },
            new SwapRule { Zone = 27, TitanOnly = false, BossOnly = true, OriginalId = 142, NewId = 230, NewLevel = 0 },
            new SwapRule { Zone = 28, TitanOnly = false, BossOnly = true, OriginalId = 128, NewId = 229, NewLevel = 3 },
            new SwapRule { Zone = 28, TitanOnly = false, BossOnly = true, OriginalId = 142, NewId = 230, NewLevel = 3 },
            new SwapRule { Zone = 29, TitanOnly = false, BossOnly = true, OriginalId = 128, NewId = 229, NewLevel = 10 },
            new SwapRule { Zone = 29, TitanOnly = false, BossOnly = true, OriginalId = 142, NewId = 230, NewLevel = 10 },
            new SwapRule { Zone = 30, TitanOnly = true, BossOnly = false, OriginalId = 170, NewId = 229, NewLevel = 45 },
            new SwapRule { Zone = 30, TitanOnly = true, BossOnly = false, OriginalId = 169, NewId = 230, NewLevel = 45 },
            new SwapRule { Zone = 31, TitanOnly = false, BossOnly = true, OriginalId = 170, NewId = 229, NewLevel = 28 },
            new SwapRule { Zone = 31, TitanOnly = false, BossOnly = true, OriginalId = 169, NewId = 230, NewLevel = 28 },
            new SwapRule { Zone = 32, TitanOnly = false, BossOnly = true, OriginalId = 229, NewId = 295, NewLevel = 0 },
            new SwapRule { Zone = 32, TitanOnly = false, BossOnly = true, OriginalId = 230, NewId = 296, NewLevel = 0 },
            new SwapRule { Zone = 33, TitanOnly = false, BossOnly = true, OriginalId = 229, NewId = 295, NewLevel = 0 },
            new SwapRule { Zone = 33, TitanOnly = false, BossOnly = true, OriginalId = 230, NewId = 296, NewLevel = 0 },
            new SwapRule { Zone = 34, TitanOnly = true, BossOnly = false, OriginalId = 229, NewId = 295, NewLevel = 28 },
            new SwapRule { Zone = 34, TitanOnly = true, BossOnly = false, OriginalId = 230, NewId = 296, NewLevel = 28 },
            new SwapRule { Zone = 35, TitanOnly = false, BossOnly = true, OriginalId = 229, NewId = 295, NewLevel = 5 },
            new SwapRule { Zone = 35, TitanOnly = false, BossOnly = true, OriginalId = 230, NewId = 296, NewLevel = 5 },
            new SwapRule { Zone = 36, TitanOnly = false, BossOnly = true, OriginalId = 229, NewId = 295, NewLevel = 15 },
            new SwapRule { Zone = 36, TitanOnly = false, BossOnly = true, OriginalId = 230, NewId = 296, NewLevel = 15 },
            new SwapRule { Zone = 37, TitanOnly = false, BossOnly = true, OriginalId = 229, NewId = 295, NewLevel = 28 },
            new SwapRule { Zone = 37, TitanOnly = false, BossOnly = true, OriginalId = 230, NewId = 296, NewLevel = 28 },
            new SwapRule { Zone = 38, TitanOnly = true, BossOnly = false, OriginalId = 295, NewId = 388, NewLevel = 5 },
            new SwapRule { Zone = 38, TitanOnly = true, BossOnly = false, OriginalId = 296, NewId = 389, NewLevel = 5 },
            new SwapRule { Zone = 39, TitanOnly = false, BossOnly = true, OriginalId = 295, NewId = 388, NewLevel = 0 },
            new SwapRule { Zone = 39, TitanOnly = false, BossOnly = true, OriginalId = 296, NewId = 389, NewLevel = 0 },
            new SwapRule { Zone = 40, TitanOnly = false, BossOnly = true, OriginalId = 295, NewId = 388, NewLevel = 0 },
            new SwapRule { Zone = 40, TitanOnly = false, BossOnly = true, OriginalId = 296, NewId = 389, NewLevel = 0 },
            new SwapRule { Zone = 41, TitanOnly = false, BossOnly = true, OriginalId = 295, NewId = 388, NewLevel = 10 },
            new SwapRule { Zone = 41, TitanOnly = false, BossOnly = true, OriginalId = 296, NewId = 389, NewLevel = 10 },
            new SwapRule { Zone = 42, TitanOnly = true, BossOnly = false, OriginalId = 388, NewId = 430, NewLevel = 10 },
            new SwapRule { Zone = 42, TitanOnly = true, BossOnly = false, OriginalId = 389, NewId = 431, NewLevel = 10 },
            new SwapRule { Zone = 43, TitanOnly = false, BossOnly = true, OriginalId = 295, NewId = 430, NewLevel = 0 },
            new SwapRule { Zone = 43, TitanOnly = false, BossOnly = true, OriginalId = 296, NewId = 431, NewLevel = 0 },
        };

        private static readonly List<ExtraDropRule> Extras = new List<ExtraDropRule>()
        {
             new ExtraDropRule { Zone = 6, TitanOnly = true, BossOnly = false, ItemId = 67, Level = 0, Chance = 0.46416f, UseLootFactor = true, MaxChance = 1f },
             new ExtraDropRule { Zone = 8, TitanOnly = true, BossOnly = false, ItemId = 67, Level = 0, Chance = 0.46416f, UseLootFactor = true, MaxChance = 1f },
             new ExtraDropRule { Zone = 11, TitanOnly = true, BossOnly = false, ItemId = 67, Level = 20, Chance = 0.46416f, UseLootFactor = true, MaxChance = 1f },
             new ExtraDropRule { Zone = 11, TitanOnly = true, BossOnly = false, ItemId = 116, Level = 0, Chance = 0.46416f, UseLootFactor = true, MaxChance = 1f },
             new ExtraDropRule { Zone = 11, TitanOnly = true, BossOnly = false, ItemId = 117, Level = 0, Chance = 0.46416f, UseLootFactor = true, MaxChance = 1f },
             new ExtraDropRule { Zone = 14, TitanOnly = true, BossOnly = false, ItemId = 67, Level = 0, Chance = 0.46416f, UseLootFactor = true, MaxChance = 1f },
             new ExtraDropRule { Zone = 16, TitanOnly = true, BossOnly = false, ItemId = 128, Level = 0, Chance = 0.171f, UseLootFactor = true, MaxChance = 1f },
             new ExtraDropRule { Zone = 19, TitanOnly = true, BossOnly = false, ItemId = 128, Level = 10, Chance = 0.07937f, UseLootFactor = true, MaxChance = 1f },
             new ExtraDropRule { Zone = 23, TitanOnly = true, BossOnly = false, ItemId = 169, Level = 5, Chance = 0.00035f, UseLootFactor = true, MaxChance = 0.25f },
        };


        // --------------------------
        // Helper: count how many of an item the player currently has
        // (used for "cap amounts")
        // --------------------------
        private static int CountOwned(Character c, int itemId)
        {
            if (c?.inventory == null) return 0;

            int count = 0;

            void CountEq(Equipment e)
            {
                if (e != null && e.id == itemId) count++;
            }

            // Main inventory + accessories bags
            if (c.inventory.inventory != null)
                for (int i = 0; i < c.inventory.inventory.Count; i++) CountEq(c.inventory.inventory[i]);

            if (c.inventory.accs != null)
                for (int i = 0; i < c.inventory.accs.Count; i++) CountEq(c.inventory.accs[i]);

            // Equipped slots
            CountEq(c.inventory.head);
            CountEq(c.inventory.chest);
            CountEq(c.inventory.legs);
            CountEq(c.inventory.boots);
            CountEq(c.inventory.weapon);
            CountEq(c.inventory.trash);

            return count;
        }


        internal static class LootDropHelper
        {
            private static readonly Dictionary<enemyType, bool> _isTitanCache =
                new Dictionary<enemyType, bool>();

            internal static bool IsTitanEnemyType(enemyType t)
            {
                bool cached;
                if (_isTitanCache.TryGetValue(t, out cached))
                    return cached;

                bool isTitan = t.ToString().StartsWith("bigBoss", StringComparison.OrdinalIgnoreCase);
                _isTitanCache[t] = isTitan;
                return isTitan;
            }
        }



        // --------------------------
        //  A) Patch ALL zoneXXDrop methods automatically
        // --------------------------
        [HarmonyPatch(typeof(LootDrop))]
        private static class Patch_AllZoneDrops
        {
            private static readonly Regex ZoneRx = new(@"^zone(\d+)Drop$", RegexOptions.Compiled);

            private static IEnumerable<MethodBase> TargetMethods()
            {
                var t = typeof(LootDrop);
                var ms = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var m in ms)
                {
                    var name = m.Name ?? "";
                    var match = ZoneRx.Match(name);
                    if (!match.Success) continue;

                    // signature must be (Enemy)
                    var ps = m.GetParameters();
                    if (ps.Length == 1 && ps[0].ParameterType == typeof(Enemy))
                        yield return m;
                }
            }

            private static void Prefix(LootDrop __instance, Enemy enemy, MethodBase __originalMethod)
            {
                int zone = -1;
                var match = ZoneRx.Match(__originalMethod?.Name ?? "");
                if (match.Success) int.TryParse(match.Groups[1].Value, out zone);

                _ctx = new Ctx
                {
                    Zone = zone,
                    EnemyType = enemy != null ? enemy.enemyType : enemyType.normal
                };
                _ctxActive = true;
            }

            private static void Postfix(LootDrop __instance, Enemy enemy)
            {
                try
                {
                    RunExtras(__instance, enemy);
                }
                finally
                {
                    _ctxActive = false;
                }
            }
        }

        // --------------------------
        //  B) Swap existing drops by rewriting args before loot is created
        // --------------------------
        [HarmonyPatch(typeof(ItemNameDesc), nameof(ItemNameDesc.makeLevelledLoot))]
        private static class Patch_makeLevelledLoot_Swaps
        {
            private static void Prefix(ItemNameDesc __instance, ref int id, ref int lootlevel)
            {
                if (!InZoneDrop) return;
                if (__instance?.character == null) return;

                // Only apply swap rules for non-titan drops
                ApplySwapRules(__instance.character, titanCall: false, ref id, ref lootlevel);
            }
        }

        [HarmonyPatch(typeof(ItemNameDesc), nameof(ItemNameDesc.makeTitanLevelledLoot))]
        private static class Patch_makeTitanLevelledLoot_Swaps
        {
            private static void Prefix(ItemNameDesc __instance, ref int id, ref int lootlevel)
            {
                if (!InZoneDrop) return;
                if (__instance?.character == null) return;

                ApplySwapRules(__instance.character, titanCall: true, ref id, ref lootlevel);
            }
        }

        private static void ApplySwapRules(Character c, bool titanCall, ref int id, ref int level)
        {
            // Swaps ONLY replace what drops (id/level).
            // Chance/caps are handled only in Extras (where we control the roll).
            for (int i = 0; i < Swaps.Count; i++)
            {
                SwapRule r = Swaps[i];
                if (r == null) continue;

                if (r.TitanOnly != titanCall) continue;
                if (r.BossOnly && !IsBossOrTitanEnemy) continue;
                if (r.Zone != -1 && r.Zone != _ctx.Zone) continue;
                if (r.OriginalId != id) continue;

                id = r.NewId;
                level = r.NewLevel;
                level += LootLevelPatches.DEFAULT_MIN_LEVEL + LootLevelPatches.BonusLootLevelsOnly(c);
                if (level < 0) level = 0;
                return;
            }
        }


        // --------------------------
        //  C) Add extra drops after vanilla zone drops finish (boss/titan-only)
        // --------------------------

        internal static class ExtraDropLevelScope
        {
            [ThreadStatic] private static int _depth;
            public static bool Active => _depth > 0;

            public static void Enter() => _depth++;
            public static void Exit()
            {
                if (_depth > 0) _depth--;
            }
        }
        private static void RunExtras(LootDrop ld, Enemy enemy)
{
    if (ld == null || enemy == null) return;
    if (!IsBossOrTitanEnemy) return; // bosses/titans only

    Character c = ld.character;
    if (c == null || ld.itemInfo == null || ld.log == null) return;

    bool isTitan = LootDropHelper.IsTitanEnemyType(enemy.enemyType);

    float lootFactor = 1f;
    try
    {
        lootFactor = c.lootFactorRooted();
    }
    catch
    {
        try { lootFactor = c.lootFactor(); }
        catch { lootFactor = 1f; }
    }

    // Keep zone context for anything else that relies on it,
    // but suppress zone-floor clamping for our custom extra drops.
    ZoneDropScope.Enter();
    try
    {
        for (int i = 0; i < Extras.Count; i++)
        {
            ExtraDropRule r = Extras[i];
            if (r == null) continue;

            if (r.Zone != -1 && r.Zone != _ctx.Zone) continue;
            if (r.TitanOnly && !isTitan) continue;
            if (r.BossOnly && !IsBossOrTitanEnemy) continue;

            float chance = r.Chance;
            if (r.UseLootFactor) chance *= lootFactor;

            if (r.MaxChance > 0f)
                chance = Mathf.Min(chance, r.MaxChance);

            chance = Mathf.Clamp01(chance);

            if (UnityEngine.Random.value >= chance) continue;

            // Match swap-rule behavior: configured level + base loot floor + bonus-only levels
            int finalLevel = r.Level + LootLevelPatches.DEFAULT_MIN_LEVEL + LootLevelPatches.BonusLootLevelsOnly(c);
            if (finalLevel < 0) finalLevel = 0;

            string drop;
            ExtraDropLevelScope.Enter();
            try
            {
                drop = ld.itemInfo.makeLevelledLoot(r.ItemId, finalLevel);
            }
            finally
            {
                ExtraDropLevelScope.Exit();
            }

            if (string.IsNullOrEmpty(drop))
                continue;

            ld.log.AddEvent(enemy.name + " also dropped " + drop + ld.itemInfo.endRemark());
        }
    }
    finally
    {
        ZoneDropScope.Exit();
    }
}
    }
}
