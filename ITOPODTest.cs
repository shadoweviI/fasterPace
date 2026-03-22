using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace fasterPace
{
    [HarmonyPatch(typeof(AdventureController), "createEnemyTable")]
    internal static class Patch_AdventureController_CreateEnemyTable_ITOPODEnemyEdits
    {
        // Change this to whatever you want ITOPOD enemies to become:
        private const enemyType TargetType = enemyType.bigBoss7V4;

        // Optional: only change if it currently is itopod (recommended)
        private const bool OnlyIfCurrentlyItopod = true;

        // Optional: example of changing AI too
        private const bool ChangeAIToo = false;
        private const AI TargetAI = AI.exploder;

        private static readonly FieldInfo F_Type =
            AccessTools.Field(typeof(Enemy), "enemyType");   // usually public, but reflection keeps it safe

        // AI field name can vary between decompiles, so try common names
        private static readonly FieldInfo F_AI =
            AccessTools.Field(typeof(Enemy), "AI")
            ?? AccessTools.Field(typeof(Enemy), "ai")
            ?? AccessTools.Field(typeof(Enemy), "enemyAI")
            ?? AccessTools.Field(typeof(Enemy), "aiType");

        private static readonly FieldInfo F_Name =
            AccessTools.Field(typeof(Enemy), "name");

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(AdventureController __instance)
        {
            var list = __instance?.itopodEnemyList;
            if (list == null || list.Count == 0) return;

            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e == null) continue;

                // read current type
                enemyType curType = e.enemyType;
                if (F_Type != null)
                {
                    try { curType = (enemyType)F_Type.GetValue(e); }
                    catch { curType = e.enemyType; }
                }

                if (OnlyIfCurrentlyItopod && curType != enemyType.itopod)
                    continue;

                // set type
                if (F_Type != null) F_Type.SetValue(e, TargetType);
                else e.enemyType = TargetType;

                // optional AI change
                if (ChangeAIToo && F_AI != null)
                    F_AI.SetValue(e, TargetAI);

                // optional: mark name so you can confirm in-game quickly
                if (F_Name != null)
                {
                    string n = (string)F_Name.GetValue(e);
                    if (!string.IsNullOrEmpty(n) && !n.Contains("[FP]"))
                        F_Name.SetValue(e, n + " [FP]");
                }
                else
                {
                    if (!string.IsNullOrEmpty(e.name) && !e.name.Contains("[FP]"))
                        e.name += " [FP]";
                }
            }
        }
    }
}
