using HarmonyLib;
using System.Collections.Generic;

namespace fasterPace
{
    [HarmonyPatch(typeof(AdventureController), "createEnemyTable")]
    internal static class Patch_AdventureController_CreateEnemyTable_ZoneHealthPerZone
    {
        // One rule = scale one whole zone list.
        private sealed class ZoneScaleRule
        {
            public int ZoneIndex;                  // enemyList index (same meaning as your existing comment)

            // If you want one knob, set AllMult and leave the per-stat at 1.
            public float AllMult = 1f;

            // Optional per-stat overrides (multiplied in addition to AllMult).
            public float AttackMult = 1f;
            public float DefenseMult = 1f;
            public float RegenMult = 1f;
            public float HpMult = 1f;

            // Optional: if first enemy's attack is already below this value, we assume scaling already happened.
            // This mirrors your current "already halved" check, but per-zone.
            public float? AlreadyAppliedIfFirstAttackBelow = null;
        }

        // Put your per-zone tuning here (examples include your current 27-29 @ 0.2)
        private static readonly List<ZoneScaleRule> Rules = new List<ZoneScaleRule>
        {
            new ZoneScaleRule { ZoneIndex = 27, AllMult = 0.2f, AlreadyAppliedIfFirstAttackBelow = 6e19f },
            new ZoneScaleRule { ZoneIndex = 28, AllMult = 0.2f, AlreadyAppliedIfFirstAttackBelow = 6e19f },
            new ZoneScaleRule { ZoneIndex = 29, AllMult = 0.2f, AlreadyAppliedIfFirstAttackBelow = 6e19f },
            new ZoneScaleRule { ZoneIndex = 31, AllMult = 0.1f, AlreadyAppliedIfFirstAttackBelow = 6e19f },

            // Example: zone 30 softer nerf, and HP nerfed more than atk/def/regen:
            // new ZoneScaleRule { ZoneIndex = 30, AllMult = 0.5f, HpMult = 0.75f },

            // Example: zone 31 only nerf regen (leave everything else unchanged):
            // new ZoneScaleRule { ZoneIndex = 31, RegenMult = 0.4f },
        };

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(AdventureController __instance)
        {
            var enemyList = __instance?.enemyList;
            if (enemyList == null || enemyList.Count == 0) return;

            for (int r = 0; r < Rules.Count; r++)
            {
                var rule = Rules[r];
                if (rule == null) continue;

                int z = rule.ZoneIndex;
                if (z < 0 || z >= enemyList.Count) continue;

                var zoneEnemies = enemyList[z];
                if (zoneEnemies == null || zoneEnemies.Count == 0) continue;

                // Optional "already applied" guard
                if (rule.AlreadyAppliedIfFirstAttackBelow.HasValue)
                {
                    var first = zoneEnemies[0];
                    if (first != null && first.attack < rule.AlreadyAppliedIfFirstAttackBelow.Value)
                        continue;
                }

                // Compute final multipliers
                float aMult = rule.AllMult * rule.AttackMult;
                float dMult = rule.AllMult * rule.DefenseMult;
                float rMult = rule.AllMult * rule.RegenMult;
                float hMult = rule.AllMult * rule.HpMult;

                // No-op rule: skip quickly
                if (aMult == 1f && dMult == 1f && rMult == 1f && hMult == 1f)
                    continue;

                for (int i = 0; i < zoneEnemies.Count; i++)
                {
                    var e = zoneEnemies[i];
                    if (e == null) continue;

                    if (aMult != 1f) e.attack *= aMult;
                    if (dMult != 1f) e.defense *= dMult;
                    if (rMult != 1f) e.regen *= rMult;

                    if (hMult != 1f)
                    {
                        e.maxHP *= hMult;
                        e.curHP *= hMult;
                    }
                }
            }
        }
    }
}
