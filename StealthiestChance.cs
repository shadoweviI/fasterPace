using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace fasterPace
{
    [HarmonyPatch(typeof(LootDrop), nameof(LootDrop.zone18Drop))]
    internal static class Patch_Zone18_StealthItem178_CapChance
    {
        // 0.001f = 0.1% cap. 0.005f = 0.5% (vanilla).
        private const float NEW_CAP = 0.03f;

        private static readonly MethodInfo MI_makeLevelledLoot =
            AccessTools.Method(typeof(ItemNameDesc), nameof(ItemNameDesc.makeLevelledLoot), new[] { typeof(int), typeof(int) });

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = new List<CodeInstruction>(instructions);

            // We target the specific drop: makeLevelledLoot(178, 5)
            for (int i = 0; i < list.Count - 2; i++)
            {
                if (IsLoadInt(list[i], 178) &&
                    IsLoadInt(list[i + 1], 5) &&
                    IsCallTo(list[i + 2], MI_makeLevelledLoot))
                {
                    // Search backwards for the cap literal 0.005f near this drop and replace it.
                    // (It’s the second argument to Mathf.Min(..., 0.005f) in the stealthComplete block.)
                    for (int j = i; j >= 0 && j >= i - 35; j--)
                    {
                        if (list[j].opcode == OpCodes.Ldc_R4 && list[j].operand is float f && f == 0.005f)
                        {
                            list[j].operand = NEW_CAP;
                            return list;
                        }
                    }
                }
            }

            return list;
        }

        private static bool IsLoadInt(CodeInstruction ci, int value)
        {
            if (ci.opcode == OpCodes.Ldc_I4) return (int)ci.operand == value;
            if (ci.opcode == OpCodes.Ldc_I4_S) return (sbyte)ci.operand == value;
            if (value == 0 && ci.opcode == OpCodes.Ldc_I4_0) return true;
            if (value == 1 && ci.opcode == OpCodes.Ldc_I4_1) return true;
            if (value == 2 && ci.opcode == OpCodes.Ldc_I4_2) return true;
            if (value == 3 && ci.opcode == OpCodes.Ldc_I4_3) return true;
            if (value == 4 && ci.opcode == OpCodes.Ldc_I4_4) return true;
            if (value == 5 && ci.opcode == OpCodes.Ldc_I4_5) return true;
            if (value == 6 && ci.opcode == OpCodes.Ldc_I4_6) return true;
            if (value == 7 && ci.opcode == OpCodes.Ldc_I4_7) return true;
            if (value == 8 && ci.opcode == OpCodes.Ldc_I4_8) return true;
            return false;
        }

        private static bool IsCallTo(CodeInstruction ci, MethodInfo mi)
            => mi != null && (ci.opcode == OpCodes.Callvirt || ci.opcode == OpCodes.Call) && ci.operand is MethodInfo m && m == mi;
    }
}
