using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace fasterPace
{
    [HarmonyPatch(typeof(NGUChallengeController), "complete")]
    internal static class Patch_NGUChallengeController_Complete_TextOnly
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int apCount = 0;

            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && code.operand is string s && s == " AP!")
                {
                    apCount++;

                    // NGU order from pasted method:
                    // 1-3 normal, 4-6 evil, 7-9 sadistic
                    if (apCount >= 7 && apCount <= 9)
                        code.operand = " AP! You also gained +5% NGU Effectiveness and +5% Cheaper Sad NGUs!";
                }

                yield return code;
            }
        }
    }

    [HarmonyPatch(typeof(TimeMachineChallengeController), "complete")]
    internal static class Patch_TimeMachineChallengeController_Complete_TextOnly
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int plainApCount = 0;

            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && code.operand is string s && s == " AP!")
                {
                    plainApCount++;

                    // #1 normal generic
                    // #2 normal final
                    // #3 evil final
                    // #4 sad first
                    // #5 sad fifth
                    // #6 sad generic
                    // #7 sad final
                    if (plainApCount == 4 || plainApCount == 5 || plainApCount == 6)
                        code.operand = " AP! You also gained +3% Global Digger Bonus!";
                    else if (plainApCount == 7)
                        code.operand = " AP! You also gained +10% Global Digger Bonus!";
                }

                yield return code;
            }
        }
    }

    [HarmonyPatch(typeof(LaserSwordChallengeController), "complete")]
    internal static class Patch_LaserSwordChallengeController_Complete_TextOnly
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int apCount = 0;

            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && code.operand is string s && s == " AP!")
                {
                    apCount++;

                    // 1-3 normal, 4-6 evil, 7-9 sadistic
                    if (apCount == 4 || apCount == 5)
                        code.operand = " AP, and 1% more Ygg yields!";
                    else if (apCount == 6)
                        code.operand = " AP, and 50% more adventure stats!";
                    else if (apCount == 7 || apCount == 8)
                        code.operand = " AP, and 0.5% tagged card effectiveness!";
                    else if (apCount == 9)
                        code.operand = " AP, and 25% mayo and card generation speed! Golly!";
                }

                yield return code;
            }
        }
    }

    [HarmonyPatch(typeof(NoAugsChallengeController), "complete")]
    internal static class Patch_NoAugsChallengeController_Complete_TextOnly
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int apBangCount = 0;
            int apCommaCount = 0;

            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && code.operand is string s)
                {
                    if (s == " AP!")
                    {
                        apBangCount++;

                        // 1 normal first
                        // 2 evil first
                        // 3 evil generic
                        // 4 evil final
                        // 5 sad first
                        if (apBangCount == 2 || apBangCount == 3)
                            code.operand = " AP, and +5% Augment leveling speed!";
                        else if (apBangCount == 4)
                            code.operand = " AP, and +25% to Total Augment leveling Speed!";
                        else if (apBangCount == 5)
                            code.operand = " AP, and +1% more permanent beard levels!";
                    }
                    else if (s == " AP, ")
                    {
                        apCommaCount++;

                        // 1 normal generic
                        // 2 normal final
                        // 3 sad generic
                        // 4 sad final
                        if (apCommaCount == 3)
                            code.operand = " AP, and +1% more permanent beard levels!";
                        else if (apCommaCount == 4)
                            code.operand = " AP, and no beards level softcap multiplied by 0.25!";
                    }
                }

                yield return code;
            }
        }
    }

    [HarmonyPatch(typeof(TrollChallengeController), "complete")]
    internal static class Patch_TrollChallengeController_Complete_TextOnly
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && code.operand is string s)
                {
                    if (s == " AP! You also gained +10% Card Generation speed!")
                        code.operand = " AP! You also gained +10% Mayo Generation speed!";
                    else if (s == " AP! you also unlocked way better Idle MacGuffins!")
                        code.operand = " AP! Also, MacGuffin Time Factor is now x2 instead of x1!";
                }

                yield return code;
            }
        }
    }

    [HarmonyPatch(typeof(TrollChallengeController), "complete")]
    internal static class Patch_TrollChallengeController_Complete_TextOnly_Extra
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int plainApCount = 0;

            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && code.operand is string s && s == " AP!")
                {
                    plainApCount++;

                    // evil #2, #4, #5, #6, #7, then sad #3, #4
                    if (plainApCount == 2)
                        code.operand = " AP! You also unlocked the Dual Wielding Wish!";
                    else if (plainApCount == 3)
                        code.operand = " AP! You also gained 25% hack speed!";
                    else if (plainApCount == 4)
                        code.operand = " AP! You also unlocked the Improved Dual Wielding Wish!";
                    else if (plainApCount == 5)
                        code.operand = " AP! You also unlocked a wish slot!";
                    else if (plainApCount == 6)
                        code.operand = " AP! Also, Beast mow takes x2 less damage instead of x3!";
                    else if (plainApCount == 7)
                        code.operand = " AP! Also, cooking affects Yggdrasil at 50% rate!";
                }

                yield return code;
            }
        }
    }

    [HarmonyPatch(typeof(LevelChallenge10KController), "complete")]
    internal static class Patch_LevelChallenge10KController_Complete_TextOnly
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int apCount = 0;

            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && code.operand is string s && s == " AP!")
                {
                    apCount++;

                    // 1-3 normal, 4-6 evil, 7-9 sadistic
                    if (apCount == 6)
                        code.operand = " AP! You also gained +10% Wandoos Bootup Speed!";
                    else if (apCount == 7 || apCount == 8)
                        code.operand = " AP! 80% Bonus to cooking timer AND bonus!";
                    else if (apCount == 9)
                        code.operand = " AP! 80% Bonus to cooking timer AND bonus! Also, you no longer have the 300% cooking cap!";
                }

                yield return code;
            }
        }
    }
}