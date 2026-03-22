using HarmonyLib;
using System;
using System.Reflection;

namespace fasterPace
{
    internal static class ChallengeAPRewardHelper
    {
        // ─────────────────────────────────────────────
        // EDIT THESE
        // ─────────────────────────────────────────────
        internal const long EVIL_DIVISOR = 2;      // vanilla style (/5)
        internal const long SAD_DIVISOR = 3;      // vanilla style (/5)

        internal static bool ComputeExpectedAPReward(object controllerInstance, ref string __result)
        {
            if (controllerInstance == null)
            {
                __result = "0";
                return false;
            }

            var t = controllerInstance.GetType();

            Character c = GetFieldOrProp<Character>(controllerInstance, t, "character");
            if (c == null)
            {
                __result = "0";
                return false;
            }

            long baseAP = GetBaseAPReward(controllerInstance, t);

            // Mirror the branching style you posted.
            var diff = c.settings.rebirthDifficulty;

            if (diff == difficulty.normal)
            {
                __result = FormatAP(c, baseAP);
                return false;
            }

            if (diff == difficulty.evil)
            {
                __result = FormatAP(c, SafeDiv(baseAP, EVIL_DIVISOR));
                return false;
            }

            if (diff == difficulty.sadistic)
            {
                __result = FormatAP(c, SafeDiv(baseAP, SAD_DIVISOR));
                return false;
            }

            // Fallback: baseAPReward * (completions() + 1) if completions exists
            int comps = TryGetCompletions(controllerInstance, t);
            long mult = (long)(Math.Max(0, comps) + 1);

            __result = FormatAP(c, SafeMul(baseAP, mult));
            return false;
        }

        private static string FormatAP(Character c, long value)
        {
            // checkAPAdded might return long/int depending on build; handle both.
            object v = InvokeCheckAPAdded(c, value);
            if (v is long l) return l.ToString("###,##0");
            if (v is int i) return i.ToString("###,##0");
            if (v is float f) return ((long)f).ToString("###,##0");
            if (v is double d) return ((long)d).ToString("###,##0");
            return value.ToString("###,##0");
        }

        private static object InvokeCheckAPAdded(Character c, long value)
        {
            // Prefer direct method via reflection to avoid signature mismatches.
            // Character.checkAPAdded(...) exists in NGU; parameter is typically long/int.
            var m = AccessTools.Method(c.GetType(), "checkAPAdded");
            if (m == null) return value;

            var ps = m.GetParameters();
            try
            {
                if (ps.Length == 1)
                {
                    if (ps[0].ParameterType == typeof(long))
                        return m.Invoke(c, new object[] { value });

                    if (ps[0].ParameterType == typeof(int))
                        return m.Invoke(c, new object[] { (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, value)) });

                    if (ps[0].ParameterType == typeof(float))
                        return m.Invoke(c, new object[] { (float)value });

                    if (ps[0].ParameterType == typeof(double))
                        return m.Invoke(c, new object[] { (double)value });

                    // Fallback: try raw
                    return m.Invoke(c, new object[] { value });
                }
            }
            catch { /* ignore and fall through */ }

            return value;
        }

        private static int TryGetCompletions(object controllerInstance, Type t)
        {
            // Many challenge controllers have completions()
            var m = AccessTools.Method(t, "completions");
            if (m == null) return 0;

            try
            {
                object v = m.Invoke(controllerInstance, null);
                if (v is int i) return i;
                if (v is long l) return (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, l));
            }
            catch { }
            return 0;
        }

        private static long GetBaseAPReward(object controllerInstance, Type t)
        {
            // Most have baseAPReward field (int/long)
            var f = AccessTools.Field(t, "baseAPReward");
            if (f == null) return 0;

            try
            {
                object v = f.GetValue(controllerInstance);
                if (v is long l) return l;
                if (v is int i) return i;
            }
            catch { }
            return 0;
        }

        private static T GetFieldOrProp<T>(object instance, Type t, string name) where T : class
        {
            try
            {
                var f = AccessTools.Field(t, name);
                if (f != null) return f.GetValue(instance) as T;

                var p = AccessTools.Property(t, name);
                if (p != null) return p.GetValue(instance, null) as T;
            }
            catch { }
            return null;
        }

        private static long SafeDiv(long v, long d)
        {
            if (d <= 0) return v;
            return v / d;
        }

        private static long SafeMul(long a, long b)
        {
            try
            {
                checked { return a * b; }
            }
            catch
            {
                // clamp on overflow
                return (a >= 0 && b >= 0) ? long.MaxValue : long.MinValue;
            }
        }
    }

    // ─────────────────────────────────────────────
    // PATCHES: expectedAPReward() for every challenge you listed
    // ─────────────────────────────────────────────

    [HarmonyPatch(typeof(BlindChallengeController), nameof(BlindChallengeController.expectedAPReward))]
    internal static class Patch_BlindChallengeController_expectedAPReward
    {
        [HarmonyPrefix]
        private static bool Prefix(BlindChallengeController __instance, ref string __result)
            => ChallengeAPRewardHelper.ComputeExpectedAPReward(__instance, ref __result);
    }

    [HarmonyPatch(typeof(NoRebirthChallengeController), nameof(NoRebirthChallengeController.expectedAPReward))]
    internal static class Patch_NoRebirthChallengeController_expectedAPReward
    {
        [HarmonyPrefix]
        private static bool Prefix(NoRebirthChallengeController __instance, ref string __result)
            => ChallengeAPRewardHelper.ComputeExpectedAPReward(__instance, ref __result);
    }

    [HarmonyPatch(typeof(NoEquipmentChallengeController), nameof(NoEquipmentChallengeController.expectedAPReward))]
    internal static class Patch_NoEquipmentChallengeController_expectedAPReward
    {
        [HarmonyPrefix]
        private static bool Prefix(NoEquipmentChallengeController __instance, ref string __result)
            => ChallengeAPRewardHelper.ComputeExpectedAPReward(__instance, ref __result);
    }

    [HarmonyPatch(typeof(NoAugsChallengeController), nameof(NoAugsChallengeController.expectedAPReward))]
    internal static class Patch_NoAugsChallengeController_expectedAPReward
    {
        [HarmonyPrefix]
        private static bool Prefix(NoAugsChallengeController __instance, ref string __result)
            => ChallengeAPRewardHelper.ComputeExpectedAPReward(__instance, ref __result);
    }

    [HarmonyPatch(typeof(NGUChallengeController), nameof(NGUChallengeController.expectedAPReward))]
    internal static class Patch_NGUChallengeController_expectedAPReward
    {
        [HarmonyPrefix]
        private static bool Prefix(NGUChallengeController __instance, ref string __result)
            => ChallengeAPRewardHelper.ComputeExpectedAPReward(__instance, ref __result);
    }

    [HarmonyPatch(typeof(TrollChallengeController), nameof(TrollChallengeController.expectedAPReward))]
    internal static class Patch_TrollChallengeController_expectedAPReward
    {
        [HarmonyPrefix]
        private static bool Prefix(TrollChallengeController __instance, ref string __result)
            => ChallengeAPRewardHelper.ComputeExpectedAPReward(__instance, ref __result);
    }

    [HarmonyPatch(typeof(LevelChallenge10KController), nameof(LevelChallenge10KController.expectedAPReward))]
    internal static class Patch_LevelChallenge10KController_expectedAPReward
    {
        [HarmonyPrefix]
        private static bool Prefix(LevelChallenge10KController __instance, ref string __result)
            => ChallengeAPRewardHelper.ComputeExpectedAPReward(__instance, ref __result);
    }

    [HarmonyPatch(typeof(BasicChallengeController), nameof(BasicChallengeController.expectedAPReward))]
    internal static class Patch_BasicChallengeController_expectedAPReward
    {
        [HarmonyPrefix]
        private static bool Prefix(BasicChallengeController __instance, ref string __result)
            => ChallengeAPRewardHelper.ComputeExpectedAPReward(__instance, ref __result);
    }

    [HarmonyPatch(typeof(LaserSwordChallengeController), nameof(LaserSwordChallengeController.expectedAPReward))]
    internal static class Patch_LaserSwordChallengeController_expectedAPReward
    {
        [HarmonyPrefix]
        private static bool Prefix(LaserSwordChallengeController __instance, ref string __result)
            => ChallengeAPRewardHelper.ComputeExpectedAPReward(__instance, ref __result);
    }

    [HarmonyPatch(typeof(TimeMachineChallengeController), nameof(TimeMachineChallengeController.expectedAPReward))]
    internal static class Patch_TimeMachineChallengeController_expectedAPReward
    {
        [HarmonyPrefix]
        private static bool Prefix(TimeMachineChallengeController __instance, ref string __result)
            => ChallengeAPRewardHelper.ComputeExpectedAPReward(__instance, ref __result);
    }
}
