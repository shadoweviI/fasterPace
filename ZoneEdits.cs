using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace fasterPace
{
    // Same hook point EnemyZoneHealth uses (confirmed working)
    [HarmonyPatch(typeof(AdventureController), "createEnemyTable")]
    internal static class Patch_AdventureController_CreateEnemyTable_EditEnemies
    {
        // One rule = edit one enemy entry.
        private sealed class EnemyEdit
        {
            public int ZoneIndex;          // matches enemyList index (same as EnemyZoneHealth comment)
            public int EnemyIndex;         // index inside that zone list
            public int? MatchSpriteId;     // optional extra safety check (only edit if spriteID matches)

            public string NewName;         // optional
            public enemyType? NewType;     // optional
            public AI? NewAI;              // optional
            public int? NewSpriteId;       // optional
        }

        private static readonly List<EnemyEdit> Edits = new List<EnemyEdit>
        {
            new EnemyEdit
            {
                ZoneIndex = 18,
                EnemyIndex = 6,
                MatchSpriteId = 123, 
                NewName = "An Army of Annoying Penguins (BOSS)",
                NewType = enemyType.boss,
                NewAI = AI.grower,
                NewSpriteId = 123
            },
            new EnemyEdit
            {
                ZoneIndex = 13,
                EnemyIndex = 8,
                MatchSpriteId = 100, 
                NewName = "ALL Robot Masters Again (BOSS)",
                NewType = enemyType.boss,
                NewAI = AI.grower,
                NewSpriteId = 99
            },
            new EnemyEdit
            {
                ZoneIndex = 21,
                EnemyIndex = 8,
                MatchSpriteId = 324,
                NewName = "EVIL CHAD (BOSS)",
                NewType = enemyType.boss,
                NewAI = AI.charger,
                NewSpriteId = 324
            },
        };

        // Reflection helpers so this works even if some fields aren’t public in your build.
        private static readonly FieldInfo F_Name = AccessTools.Field(typeof(Enemy), "name");
        private static readonly FieldInfo F_Type = AccessTools.Field(typeof(Enemy), "enemyType");
        private static readonly FieldInfo F_SpriteId = AccessTools.Field(typeof(Enemy), "spriteID");

        // AI field name varies across some decompiles; try a few common ones.
        private static readonly FieldInfo F_AI =
            AccessTools.Field(typeof(Enemy), "AI")
            ?? AccessTools.Field(typeof(Enemy), "ai")
            ?? AccessTools.Field(typeof(Enemy), "enemyAI")
            ?? AccessTools.Field(typeof(Enemy), "aiType");

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(AdventureController __instance)
        {
            var enemyList = __instance?.enemyList;
            if (enemyList == null || enemyList.Count == 0) return;

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

                // Optional safety check: spriteID match
                if (rule.MatchSpriteId.HasValue)
                {
                    int curSprite = GetSpriteId(e);
                    if (curSprite != rule.MatchSpriteId.Value) continue;
                }

                // Idempotence: if already renamed to this exact name, skip (prevents double-applying)
                if (!string.IsNullOrEmpty(rule.NewName))
                {
                    string curName = GetName(e);
                    if (!string.Equals(curName, rule.NewName, StringComparison.Ordinal))
                        SetName(e, rule.NewName);
                }

                if (rule.NewType.HasValue)
                    SetEnemyType(e, rule.NewType.Value);

                if (rule.NewAI.HasValue)
                    SetAI(e, rule.NewAI.Value);

                if (rule.NewSpriteId.HasValue)
                    SetSpriteId(e, rule.NewSpriteId.Value);
            }
        }

        private static string GetName(Enemy e)
            => (string)(F_Name?.GetValue(e) ?? e.name);

        private static void SetName(Enemy e, string v)
        {
            if (F_Name != null) F_Name.SetValue(e, v);
            else e.name = v;
        }

        private static void SetEnemyType(Enemy e, enemyType t)
        {
            if (F_Type != null) F_Type.SetValue(e, t);
            else e.enemyType = t;
        }

        private static void SetAI(Enemy e, AI ai)
        {
            if (F_AI != null) F_AI.SetValue(e, ai);
        }

        private static int GetSpriteId(Enemy e)
            => F_SpriteId != null ? (int)F_SpriteId.GetValue(e) : e.spriteID;

        private static void SetSpriteId(Enemy e, int id)
        {
            if (F_SpriteId != null) F_SpriteId.SetValue(e, id);
            else e.spriteID = id;
        }
    }
}
