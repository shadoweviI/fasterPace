using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace fasterPace
{
    // Vanilla: 1 + level/100  => level 100 = x2
    // New:     1 + level/(100/9) => level 100 = x10
    [HarmonyPatch(typeof(Equipment))]
    internal static class EquipmentLevelDivisor_x10
    {
        private const float NEW_DIVISOR = 100f / 9f; // 11.111111...

        private static readonly FieldInfo LevelField = AccessTools.Field(typeof(Equipment), "level");

        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // Patch the big ones by exact signature where possible
            yield return AccessTools.Method(typeof(Equipment), nameof(Equipment.tooltipText), new[] { typeof(int) });
            yield return AccessTools.Method(typeof(Equipment), "updateItem"); // signature varies a bit; name is stable
            yield return AccessTools.Method(typeof(Equipment), "mergeItem");
            yield return AccessTools.Method(typeof(Equipment), "levelUp");

            // Some builds have multiple overloads of boostEquip / maxEquipBoost
            foreach (var m in typeof(Equipment).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (m.Name == "boostEquip" || m.Name == "maxEquipBoost")
                    yield return m;
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Replace ONLY: (float)this.level / 100f
            // IL pattern: ldarg.0 ; ldfld level ; conv.r4 ; ldc.r4 100 ; div
            for (int i = 0; i < codes.Count - 4; i++)
            {
                if (codes[i].opcode != OpCodes.Ldarg_0) continue;
                if (codes[i + 1].opcode != OpCodes.Ldfld || !Equals(codes[i + 1].operand, LevelField)) continue;
                if (codes[i + 2].opcode != OpCodes.Conv_R4) continue;
                if (codes[i + 3].opcode != OpCodes.Ldc_R4 || !(codes[i + 3].operand is float f) || Math.Abs(f - 100f) > 0.0001f) continue;
                if (codes[i + 4].opcode != OpCodes.Div) continue;

                codes[i + 3].operand = NEW_DIVISOR;
            }

            return codes;
        }
    }
    [HarmonyPatch(typeof(Character), "addOfflineProgress")]
    internal static class EquipmentCurClampCleanup
    {
        private const string PP_KEY = "fasterPace.fixItemCurOverMax_v1";
        private const float NEW_DIVISOR = 100f / 9f; // must match your x10 divisor

        [HarmonyPostfix]
        private static void Postfix(Character __instance)
        {
            var c = __instance;
            if (c?.inventory?.inventory == null) return;

            // Run ONCE per save
            if (PlayerPrefs.GetInt(PP_KEY, 0) != 0) return;

            int fixedCount = 0;

            // inventory.inventory is List<Equipment> in this build
            var inv = c.inventory.inventory as IList<Equipment>;
            if (inv == null) return;

            for (int i = 0; i < inv.Count; i++)
            {
                var e = inv[i];
                if (e == null) continue;

                int lvl = e.level;
                float mult = 1f + (lvl <= 0 ? 0f : (lvl / NEW_DIVISOR));

                // Clamp current values down to the NEW max
                float maxAtk = Mathf.Floor(e.capAttack * mult);
                float maxDef = Mathf.Floor(e.capDefense * mult);
                float maxS1 = Mathf.Floor(e.spec1Cap * mult);
                float maxS2 = Mathf.Floor(e.spec2Cap * mult);
                float maxS3 = Mathf.Floor(e.spec3Cap * mult);

                bool changed = false;

                if (e.curAttack > maxAtk) { e.curAttack = maxAtk; changed = true; }
                if (e.curDefense > maxDef) { e.curDefense = maxDef; changed = true; }

                if (e.spec1Cur > maxS1) { e.spec1Cur = maxS1; changed = true; }
                if (e.spec2Cur > maxS2) { e.spec2Cur = maxS2; changed = true; }
                if (e.spec3Cur > maxS3) { e.spec3Cur = maxS3; changed = true; }

                if (changed) fixedCount++;
            }

            PlayerPrefs.SetInt(PP_KEY, 1);
            PlayerPrefs.Save();

            Plugin.LogInfo($"[FP] EquipmentCurClampCleanup ran once. Items clamped: {fixedCount}");
        }
    }
}
