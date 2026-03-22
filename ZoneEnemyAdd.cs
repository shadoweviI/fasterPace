using HarmonyLib;
using System;
using System.Collections.Generic;

namespace fasterPace
{
    [HarmonyPatch(typeof(AdventureController), "createEnemyTable")]
    internal static class Patch_AdventureController_CreateEnemyTable_AddEnemies
    {
        private sealed class EnemyAdd
        {
            public int ZoneIndex { get; }
            public string Name { get; }
            public int SpriteId { get; }

            public int? InsertAt { get; set; }
            public string UniqueName { get; set; }
            public int? UniqueSpriteId { get; set; }

            // Match vanilla ctor: Enemy(name, speed, power, toughness, regen, maxHP, type, ai, spriteId)
            public float Speed { get; set; } = 1f;
            public float Power { get; set; } = 1f;
            public float Toughness { get; set; } = 1f;
            public float HpRegen { get; set; } = 0f;
            public float MaxHP { get; set; } = 10f;

            public enemyType Type { get; set; } = enemyType.normal;
            public AI AI { get; set; } = AI.normal;

            public EnemyAdd(int zoneIndex, string name, int spriteId)
            {
                ZoneIndex = zoneIndex;
                Name = name ?? "";
                SpriteId = spriteId;
            }
        }

        private static readonly List<EnemyAdd> Adds = new List<EnemyAdd>
        {
            new EnemyAdd(1, "Gorden Ramsy", 302)
            {
                UniqueName = "Gorden Ramsy",
                UniqueSpriteId = 302,

                Speed = 1.3f,
                Power = 890000000f,
                Toughness = 890000000f,
                MaxHP = 89000000000f,
                HpRegen = 89000000f,

                Type = enemyType.boss,
                AI = AI.normal
            },
        };

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(AdventureController __instance)
        {
            var enemyList = __instance?.enemyList;
            if (enemyList == null || enemyList.Count == 0) return;

            for (int r = 0; r < Adds.Count; r++)
            {
                var rule = Adds[r];
                if (rule == null) continue;

                int z = rule.ZoneIndex;
                if (z < 0 || z >= enemyList.Count) continue;

                var zone = enemyList[z];
                if (zone == null) continue;

                // Compute dedupe values without assigning later (prevents Harmony003 warnings)
                string uniqueName = !string.IsNullOrEmpty(rule.UniqueName) ? rule.UniqueName : rule.Name;
                int uniqueSpriteId = rule.UniqueSpriteId.HasValue ? rule.UniqueSpriteId.Value : rule.SpriteId;
                // If you want "no sprite check" when UniqueSpriteId is null, do this instead:
                // int uniqueSpriteId = rule.UniqueSpriteId.HasValue ? rule.UniqueSpriteId.Value : -1;

                if (AlreadyExists(zone, uniqueName, uniqueSpriteId))
                    continue;

                var enemy = new Enemy(
                    rule.Name,
                    rule.Speed,
                    rule.Power,
                    rule.Toughness,
                    rule.HpRegen,
                    rule.MaxHP,
                    rule.Type,
                    rule.AI,
                    rule.SpriteId
                );

                if (rule.InsertAt.HasValue)
                {
                    int idx = rule.InsertAt.Value;
                    if (idx < 0) idx = 0;
                    if (idx > zone.Count) idx = zone.Count;
                    zone.Insert(idx, enemy);
                }
                else
                {
                    zone.Add(enemy);
                }
            }
        }

        private static bool AlreadyExists(List<Enemy> zone, string uniqueName, int uniqueSpriteId)
        {
            if (zone == null || zone.Count == 0) return false;
            if (string.IsNullOrEmpty(uniqueName)) return false;

            bool checkSprite = uniqueSpriteId >= 0;

            for (int i = 0; i < zone.Count; i++)
            {
                var e = zone[i];
                if (e == null) continue;

                if (!string.Equals(e.name, uniqueName, StringComparison.Ordinal))
                    continue;

                if (checkSprite && e.spriteID != uniqueSpriteId)
                    continue;

                return true;
            }

            return false;
        }
    }
}
