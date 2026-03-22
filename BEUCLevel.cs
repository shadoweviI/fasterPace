using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace fasterPace
{
    // LootDrop.zone30Drop contains:
    // if (character.adventure.titan9SpecialReward) { ... makeTitanLevelledLoot(342, 0) ... }
    [HarmonyPatch(typeof(LootDrop), "zone30Drop")]
    internal static class Patch_Zone30_Titan9SpecialReward_UseBonusLootLevels
    {
        private const int SPECIAL_ID = 342;

        private static readonly MethodInfo MI_makeTitanLevelledLoot =
            AccessTools.Method(typeof(ItemNameDesc), nameof(ItemNameDesc.makeTitanLevelledLoot),
                new[] { typeof(int), typeof(int) });

        // level += DEFAULT_MIN_LEVEL + BonusLootLevelsOnly(c)
        private static int ComputeScaledLevel(int baseLevel, LootDrop ld)
        {
            var c = ld?.character;

            int level = baseLevel;
            level += LootLevelPatches.DEFAULT_MIN_LEVEL + LootLevelPatches.BonusLootLevelsOnly(c);

            if (level < 0) level = 0;
            return level;
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = new List<CodeInstruction>(instructions);

            // Find the exact call: makeTitanLevelledLoot(342, 0)
            for (int i = 0; i < list.Count - 2; i++)
            {
                if (IsLoadInt(list[i], SPECIAL_ID) &&
                    IsLoadInt(list[i + 1], 0) &&
                    IsCallTo(list[i + 2], MI_makeTitanLevelledLoot))
                {
                    // Stack around here is:
                    // ... itemInfo, 342, 0
                    // We want: ... itemInfo, 342, ComputeScaledLevel(0, this)
                    //
                    // Keep the 0, then push "this" and call helper; it consumes (0, this) and returns int.
                    list.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0)); // this (LootDrop)
                    list.Insert(i + 3, new CodeInstruction(
                        OpCodes.Call,
                        AccessTools.Method(typeof(Patch_Zone30_Titan9SpecialReward_UseBonusLootLevels),
                            nameof(ComputeScaledLevel),
                            new[] { typeof(int), typeof(LootDrop) })
                    ));

                    break; 
                }
            }

            return list;
        }

        private static bool IsCallTo(CodeInstruction ci, MethodInfo mi)
            => mi != null && (ci.opcode == OpCodes.Callvirt || ci.opcode == OpCodes.Call) &&
               ci.operand is MethodInfo m && m == mi;

        private static bool IsLoadInt(CodeInstruction ci, int value)
        {
            if (ci.opcode == OpCodes.Ldc_I4) return (int)ci.operand == value;
            if (ci.opcode == OpCodes.Ldc_I4_S) return (sbyte)ci.operand == value;
            return value switch
            {
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
            };
        }
    }
}