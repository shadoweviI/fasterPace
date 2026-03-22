using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace fasterPace
{
    internal static class Perk34BossExpUtil
    {
        public const int PerkIndex = 34;
        public const double PerkMult = 1.5;

        public static bool Perk34On(Character c)
        {
            try
            {
                var list = c?.adventure?.itopod?.perkLevel;
                return list != null && list.Count > PerkIndex && list[PerkIndex] > 0;
            }
            catch { return false; }
        }

        public static long ApplyPerk(long baseExp, Character c)
        {
            if (baseExp <= 0) return baseExp;
            if (!Perk34On(c)) return baseExp;
            return (long)Math.Ceiling(baseExp * PerkMult);
        }
    }

    // ------------------------------------------------------------
    // 1) Make bossXExp() include perk 34 bonus (Titan/Boss-only)
    // ------------------------------------------------------------
    [HarmonyPatch(typeof(AdventureController))]
    internal static class Patch_AdventureController_BossExp_Perk34
    {
        // Patch all boss exp methods you have in your build.
        // Add/remove names if your game version differs.
        private static readonly string[] BossExpMethods =
        {
            "boss1Exp","boss2Exp","boss3Exp","boss4Exp","boss5Exp","boss6Exp",
            "boss7Exp","boss8Exp","boss9Exp","boss10Exp","boss11Exp","boss12Exp"
        };

        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var name in BossExpMethods)
            {
                var m = AccessTools.Method(typeof(AdventureController), name);
                if (m != null) yield return m;
            }
        }

        [HarmonyPostfix]
        private static void Postfix(AdventureController __instance, ref long __result)
        {
            var c = __instance?.character;
            __result = Perk34BossExpUtil.ApplyPerk(__result, c);
        }
    }

    // ------------------------------------------------------------
    // 2) Neutralize the OLD LootDrop perk branch multiplier (prevents double-buff)
    //    We only touch zone*Drop methods that reference perk index 34,
    //    and only change 1.5f -> 1.0f constants.
    // ------------------------------------------------------------
    [HarmonyPatch(typeof(LootDrop))]
    internal static class Patch_LootDrop_RemoveOldPerk34_1p5
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            var t = typeof(LootDrop);
            foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (m == null || m.IsAbstract || m.ContainsGenericParameters) continue;
                if (m.Name.StartsWith("zone", StringComparison.Ordinal) &&
                    m.Name.EndsWith("Drop", StringComparison.Ordinal))
                    yield return m;
            }
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Only edit methods that mention perk index 34 (avoid touching other 1.5f uses)
            bool references34 = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if ((codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand is sbyte sb && sb == 34) ||
                    (codes[i].opcode == OpCodes.Ldc_I4 && codes[i].operand is int iv && iv == 34))
                {
                    references34 = true;
                    break;
                }
            }

            if (!references34) return codes;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].operand is float f)
                {
                    if (Math.Abs(f - 1.5f) < 0.00001f)
                        codes[i].operand = 1.0f;
                }
            }

            return codes;
        }
    }
}
