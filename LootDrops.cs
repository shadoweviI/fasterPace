using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

[HarmonyPatch(typeof(ItemNameDesc))]
internal static class LootLevelPatches
{
    private const int FORCED_LEVEL = 4;

    // Only bump truly unlevelled *gear* drops; keep forced 100 items, boosts, etc. intact.
    private static void ForceLootLevel(Equipment eq)
    {
        if (eq == null) return;

        // Don’t touch anything that already has a level (incl. 100)
        if (eq.level != 0) return;

        // Don’t touch boosts/macguffins
        if (eq.type == part.MacGuffin) return;

        eq.level = FORCED_LEVEL;
    }

    // --- Patch genLoot overloads (optional but good coverage) ---
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ItemNameDesc.genLoot), new Type[] { typeof(int) })]
    private static void GenLoot_Int_Postfix(ref Equipment __result) => ForceLootLevel(__result);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ItemNameDesc.genLoot), new Type[] { typeof(int), typeof(bool) })]
    private static void GenLoot_IntBool_Postfix(ref Equipment __result) => ForceLootLevel(__result);

    // --- Transpiler injection used by makeLoot/makeTitanLoot overloads ---
    private static IEnumerable<CodeInstruction> InjectAfterNewEquipmentLocal(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);

        var forceMethod = AccessTools.Method(typeof(LootLevelPatches), nameof(ForceLootLevel));
        if (forceMethod == null) throw new Exception("ForceLootLevel method not found.");

        for (int i = 0; i < list.Count; i++)
        {
            yield return list[i];

            // Detect "stloc.*" that stores the result of "newobj Equipment::.ctor"
            if (list[i].opcode.IsStloc() &&
                i > 0 &&
                list[i - 1].opcode == OpCodes.Newobj &&
                list[i - 1].operand is ConstructorInfo ci &&
                ci.DeclaringType == typeof(Equipment))
            {
                // After the local is assigned, load it and call ForceLootLevel(local)
                var local = GetStlocOperand(list[i]);
                yield return new CodeInstruction(OpCodes.Ldloc, local);
                yield return new CodeInstruction(OpCodes.Call, forceMethod);
            }
        }
    }

    // makeLoot(int id)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemNameDesc.makeLoot), new Type[] { typeof(int) })]
    private static IEnumerable<CodeInstruction> MakeLoot_Transpiler(IEnumerable<CodeInstruction> instructions)
        => InjectAfterNewEquipmentLocal(instructions);

    // makeTitanLoot(int id)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemNameDesc.makeTitanLoot), new Type[] { typeof(int) })]
    private static IEnumerable<CodeInstruction> MakeTitanLoot_Transpiler(IEnumerable<CodeInstruction> instructions)
        => InjectAfterNewEquipmentLocal(instructions);

    // makeLoot(int id, int sid)  (this overload exists in your file)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemNameDesc.makeLoot), new Type[] { typeof(int), typeof(int) })]
    private static IEnumerable<CodeInstruction> MakeLoot_Slot_Transpiler(IEnumerable<CodeInstruction> instructions)
        => InjectAfterNewEquipmentLocal(instructions);

    // --- helpers ---
    private static object GetStlocOperand(CodeInstruction stloc)
    {
        // stloc.0/1/2/3 have no operand; use indexes
        if (stloc.opcode == OpCodes.Stloc_0) return 0;
        if (stloc.opcode == OpCodes.Stloc_1) return 1;
        if (stloc.opcode == OpCodes.Stloc_2) return 2;
        if (stloc.opcode == OpCodes.Stloc_3) return 3;

        // stloc / stloc.s carry operand (LocalBuilder or int)
        if (stloc.operand is LocalBuilder lb) return lb.LocalIndex;
        if (stloc.operand is int i) return i;

        throw new InvalidOperationException("Unsupported stloc form.");
    }
}

internal static class OpCodeExtensions
{
    public static bool IsStloc(this OpCode op) =>
        op == OpCodes.Stloc || op == OpCodes.Stloc_S ||
        op == OpCodes.Stloc_0 || op == OpCodes.Stloc_1 ||
        op == OpCodes.Stloc_2 || op == OpCodes.Stloc_3;
}
