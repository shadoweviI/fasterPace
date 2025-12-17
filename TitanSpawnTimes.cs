using HarmonyLib;
using System;
using UnityEngine;

namespace fasterPace
{
    [HarmonyPatch]
    internal class TitanSpawnTimes
    {

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void afterSaveLoaded(Character __instance)
        {
            var controller = __instance.adventureController;

            var text = $"T1: {controller.boss1SpawnTime()}"
                + $"\nT2: {controller.boss2SpawnTime()}"
                + $"\nT3: {controller.boss3SpawnTime()}"
                + $"\nT4: {controller.boss4SpawnTime()}"
                + $"\nT5: {controller.boss5SpawnTime()}"
                + $"\nT6: {controller.boss6SpawnTime()}"
                + $"\nT7: {controller.boss7SpawnTime()}"
                + $"\nT8: {controller.boss8SpawnTime()}"
                + $"\nT9: {controller.boss9SpawnTime()}"
                + $"\nT10: {controller.boss10SpawnTime()}"
                + $"\nT11: {controller.boss11SpawnTime()}"
                + $"\nT12: {controller.boss12SpawnTime()}"
                + $"\nT13: {controller.boss13SpawnTime()}"
                + $"\nT14: {controller.boss14SpawnTime()}";

            Plugin.LogInfo($"titan spawn times:\n{text}");
        }

        const float MIN_SPAWN_TIME = 300f;
        const float TIME_PER_COMPLETION = 50f;

        private static Character character;
        private static int normalCompletions => character.allChallenges.noRebirthChallenge.completions();
        private static int evilCompletions => character.allChallenges.noRebirthChallenge.evilCompletions();
        private static int sadCompletions => character.allChallenges.noRebirthChallenge.sadisticCompletions();
        private static float[] baseSpawnTimes = [0, 300, 300, 400, 450, 500f, 550f, 600f, 700f, 800f, 900f, 1000f, 1100f, 1200f, 1300f];

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss1SpawnTime")]
        private static bool boss1SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[1] - TIME_PER_COMPLETION * normalCompletions);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss2SpawnTime")]
        private static bool boss2SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[2] - TIME_PER_COMPLETION * normalCompletions);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss3SpawnTime")]
        private static bool boss3SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[3] - TIME_PER_COMPLETION * normalCompletions);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss4SpawnTime")]
        private static bool boss4SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[4] - TIME_PER_COMPLETION * normalCompletions);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss5SpawnTime")]
        private static bool boss5SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[5] - TIME_PER_COMPLETION * normalCompletions);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss6SpawnTime")]
        private static bool boss6SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[6] - TIME_PER_COMPLETION * normalCompletions);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss7SpawnTime")]
        private static bool boss7SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[7] - TIME_PER_COMPLETION * (normalCompletions + evilCompletions));
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss8SpawnTime")]
        private static bool boss8SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[8] - TIME_PER_COMPLETION * (normalCompletions + evilCompletions));
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss9SpawnTime")]
        private static bool boss9SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[9] - TIME_PER_COMPLETION * (normalCompletions + evilCompletions));
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss10SpawnTime")]
        private static bool boss10SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[10] - TIME_PER_COMPLETION * (normalCompletions + evilCompletions + sadCompletions));
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss11SpawnTime")]
        private static bool boss11SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[11] - TIME_PER_COMPLETION * (normalCompletions + evilCompletions + sadCompletions));
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss12SpawnTime")]
        private static bool boss12SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[12] - TIME_PER_COMPLETION * (normalCompletions + evilCompletions + sadCompletions));
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss13SpawnTime")]
        private static bool boss13SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[13] - TIME_PER_COMPLETION * (normalCompletions + evilCompletions + sadCompletions));
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss14SpawnTime")]
        private static bool boss14SpawnTime(ref float __result)
        {
            __result = Math.Max(MIN_SPAWN_TIME, baseSpawnTimes[14] - TIME_PER_COMPLETION * (normalCompletions + evilCompletions + sadCompletions));
            return false;
        }
    }
}
