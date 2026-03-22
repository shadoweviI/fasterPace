using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
#pragma warning disable 0649 // Optional rule fields are intentionally not always assigned
namespace fasterPace
{
    [HarmonyPatch(typeof(AdventureController), "createEnemyTable")]
    internal static class Patch_AdventureController_CreateEnemyTable_ModifyTable
    {
        // --------------------------
        // 1) RULES YOU EDIT
        // --------------------------

        // Remove rules (run FIRST)
        private sealed class EnemyRemove
        {
            public int ZoneIndex;
            public int? EnemyIndex;          // remove exact slot (fastest)

            public string MatchName;         // optional
            public enemyType? MatchType;     // optional
            public AI? MatchAI;              // optional
            public int? MatchSpriteId;       // optional
        }

        // Edit rules (run AFTER removals)
        private sealed class EnemyEdit
        {
            public int ZoneIndex;
            public int EnemyIndex;

            public string NewName;
            public enemyType? NewType;
            public AI? NewAI;
            public int? NewSpriteId;
        }

        // Example removals:
        private static readonly List<EnemyRemove> Removes = new List<EnemyRemove>
        {
            // Remove by index:
            // new EnemyRemove { ZoneIndex = 19, EnemyIndex = 8 },

            // Remove ONLY the paralyze copy (useful when names duplicate):
            new EnemyRemove
            {
                ZoneIndex = 18,
                MatchName = "THE ELUSIVE C.S (BOSS)",
                MatchAI = AI.paralyze
            },
        };

        // Example edits:
        private static readonly List<EnemyEdit> Edits = new List<EnemyEdit>
        {
            // After the removal above, indices might shift.
            // So either re-check indices, or avoid index-based edits on items after the removed one.

            new EnemyEdit
            {
                ZoneIndex = 18,
                EnemyIndex = 7,
                NewName = "THE ELUSIVE C.S (BOSS)",
                NewAI = AI.poison
            },
        };

        // --------------------------
        // 2) REFLECTION (robust)
        // --------------------------
        private static readonly FieldInfo F_Name = AccessTools.Field(typeof(Enemy), "name");
        private static readonly FieldInfo F_Type = AccessTools.Field(typeof(Enemy), "enemyType");
        private static readonly FieldInfo F_SpriteId = AccessTools.Field(typeof(Enemy), "spriteID");

        private static readonly FieldInfo F_AI =
            AccessTools.Field(typeof(Enemy), "AI")
            ?? AccessTools.Field(typeof(Enemy), "ai")
            ?? AccessTools.Field(typeof(Enemy), "enemyAI")
            ?? AccessTools.Field(typeof(Enemy), "aiType");

        // --------------------------
        // 3) THE PATCH
        // --------------------------
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(AdventureController __instance)
        {
            var enemyList = __instance?.enemyList;
            if (enemyList == null || enemyList.Count == 0) return;

            // A) REMOVALS FIRST
            ApplyRemovals(enemyList);

            // B) THEN EDITS
            ApplyEdits(enemyList);
        }

        private static void ApplyRemovals(List<List<Enemy>> enemyList)
        {
            for (int r = 0; r < Removes.Count; r++)
            {
                var rule = Removes[r];
                if (rule == null) continue;

                if (rule.ZoneIndex < 0 || rule.ZoneIndex >= enemyList.Count) continue;
                var zone = enemyList[rule.ZoneIndex];
                if (zone == null || zone.Count == 0) continue;

                // Remove a specific index
                if (rule.EnemyIndex.HasValue)
                {
                    int idx = rule.EnemyIndex.Value;
                    if (idx >= 0 && idx < zone.Count)
                        zone.RemoveAt(idx);
                    continue;
                }

                // Remove by match: iterate backwards
                for (int i = zone.Count - 1; i >= 0; i--)
                {
                    var e = zone[i];
                    if (e == null) continue;

                    if (!string.IsNullOrEmpty(rule.MatchName) && GetName(e) != rule.MatchName) continue;
                    if (rule.MatchType.HasValue && GetType(e) != rule.MatchType.Value) continue;
                    if (rule.MatchAI.HasValue && GetAI(e) != rule.MatchAI.Value) continue;
                    if (rule.MatchSpriteId.HasValue && GetSpriteId(e) != rule.MatchSpriteId.Value) continue;

                    zone.RemoveAt(i);
                }
            }
        }

        private static void ApplyEdits(List<List<Enemy>> enemyList)
        {
            for (int r = 0; r < Edits.Count; r++)
            {
                var rule = Edits[r];
                if (rule == null) continue;

                if (rule.ZoneIndex < 0 || rule.ZoneIndex >= enemyList.Count) continue;
                var zone = enemyList[rule.ZoneIndex];
                if (zone == null) continue;

                if (rule.EnemyIndex < 0 || rule.EnemyIndex >= zone.Count) continue;
                var e = zone[rule.EnemyIndex];
                if (e == null) continue;

                if (!string.IsNullOrEmpty(rule.NewName)) SetName(e, rule.NewName);
                if (rule.NewType.HasValue) SetType(e, rule.NewType.Value);
                if (rule.NewAI.HasValue) SetAI(e, rule.NewAI.Value);
                if (rule.NewSpriteId.HasValue) SetSpriteId(e, rule.NewSpriteId.Value);
            }
        }

        // --------------------------
        // 4) SMALL GET/SET HELPERS
        // --------------------------
        private static string GetName(Enemy e) => (string)(F_Name?.GetValue(e) ?? e.name);
        private static void SetName(Enemy e, string v) { if (F_Name != null) F_Name.SetValue(e, v); else e.name = v; }

        private static enemyType GetType(Enemy e) => (enemyType)(F_Type?.GetValue(e) ?? e.enemyType);
        private static void SetType(Enemy e, enemyType v) { if (F_Type != null) F_Type.SetValue(e, v); else e.enemyType = v; }

        private static AI GetAI(Enemy e) => F_AI != null ? (AI)F_AI.GetValue(e) : default(AI);
        private static void SetAI(Enemy e, AI v) { if (F_AI != null) F_AI.SetValue(e, v); }

        private static int GetSpriteId(Enemy e) => F_SpriteId != null ? (int)F_SpriteId.GetValue(e) : e.spriteID;
        private static void SetSpriteId(Enemy e, int v) { if (F_SpriteId != null) F_SpriteId.SetValue(e, v); else e.spriteID = v; }
    }
}
#pragma warning restore 0649
