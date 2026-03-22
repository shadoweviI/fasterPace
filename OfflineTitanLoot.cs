using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace fasterPace
{
    // After offline progress runs (on login), grant the Titan item drops that would have happened
    // for any Titans autokilled offline, matching the correct Titan VERSION (v1-v4).
    //
    // Works by:
    //  - Snapshot bestiary Titan kills before Character.adventureOfflineProgress
    //  - Compute deltas after it finishes
    //  - For each delta, call LootDrop.zoneXXDrop(enemyTemplateForThatVersion)
    //  - Restore adventure.boss{N}Kills so we don't double-count from zone drops
    //  - Use character.lootState to keep RNG deterministic like normal loot
    [HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
    internal static class TitanOfflineLootDrops
    {
        private sealed class State
        {
            public Dictionary<int, long> bestiaryKillsBefore = new Dictionary<int, long>();
        }

        private struct TitanInfo
        {
            public int zone;
            public int enemyIndex;
            public int bossNum;

            public TitanInfo(int zone, int enemyIndex, int bossNum)
            {
                this.zone = zone;
                this.enemyIndex = enemyIndex;
                this.bossNum = bossNum;
            }
        }

        // bestiaryEnemyId -> (zone, enemyListIndex, bossNumberForKillCounterRestore)
        // Zone/boss mapping matches vanilla:
        //  zone 6->boss1, 8->boss2, 11->boss3, 14->boss4, 16->boss5,
        //  19->boss6, 23->boss7, 26->boss8, 30->boss9, 34->boss10,
        //  38->boss11, 42->boss12
        private static readonly Dictionary<int, TitanInfo> TitanMap = new Dictionary<int, TitanInfo>()
        {
            // Titans 1-5
            { 302, new TitanInfo( 6, 0,  1) }, // Titan 1
            { 303, new TitanInfo( 8, 0,  2) }, // Titan 2
            { 304, new TitanInfo(11, 0,  3) }, // Titan 3
            { 305, new TitanInfo(14, 0,  4) }, // Titan 4
            { 310, new TitanInfo(16, 0,  5) }, // Titan 5 (final/real one)

            // Titan 6 (THE BEAST) v1-v4 : enemyList[19][1..4]
            { 312, new TitanInfo(19, 1,  6) }, // v1
            { 313, new TitanInfo(19, 2,  6) }, // v2
            { 314, new TitanInfo(19, 3,  6) }, // v3
            { 315, new TitanInfo(19, 4,  6) }, // v4

            // Titan 7 v1-v4 : enemyList[23][1..4]
            { 334, new TitanInfo(23, 1,  7) }, // v1
            { 335, new TitanInfo(23, 2,  7) }, // v2
            { 336, new TitanInfo(23, 3,  7) }, // v3
            { 337, new TitanInfo(23, 4,  7) }, // v4

            // Titan 8 v1-v4 : enemyList[26][1..4]
            { 339, new TitanInfo(26, 1,  8) }, // v1
            { 340, new TitanInfo(26, 2,  8) }, // v2
            { 341, new TitanInfo(26, 3,  8) }, // v3
            { 342, new TitanInfo(26, 4,  8) }, // v4

            // Titan 9 v1-v4 : enemyList[30][1..4]
            { 344, new TitanInfo(30, 1,  9) }, // v1
            { 345, new TitanInfo(30, 2,  9) }, // v2
            { 346, new TitanInfo(30, 3,  9) }, // v3
            { 347, new TitanInfo(30, 4,  9) }, // v4

            // Titan 10 v1-v4 : enemyList[34][1..4]
            { 365, new TitanInfo(34, 1, 10) }, // v1
            { 366, new TitanInfo(34, 2, 10) }, // v2
            { 367, new TitanInfo(34, 3, 10) }, // v3
            { 368, new TitanInfo(34, 4, 10) }, // v4

            // Titan 11 v1-v4 : enemyList[38][0..3] (NOTE: index==version)
            { 369, new TitanInfo(38, 0, 11) }, // v1
            { 370, new TitanInfo(38, 1, 11) }, // v2
            { 371, new TitanInfo(38, 2, 11) }, // v3
            { 372, new TitanInfo(38, 3, 11) }, // v4

            // Titan 12 v1-v4 : enemyList[42][0..3] (NOTE: index==version)
            { 373, new TitanInfo(42, 0, 12) }, // v1
            { 374, new TitanInfo(42, 1, 12) }, // v2
            { 375, new TitanInfo(42, 2, 12) }, // v3
            { 376, new TitanInfo(42, 3, 12) }, // v4
        };

        private static MethodInfo ZoneDropMethod(int zone)
        {
            return AccessTools.Method(typeof(LootDrop), "zone" + zone + "Drop", new Type[] { typeof(Enemy) });
        }

        [HarmonyPrefix]
        private static void Prefix(Character __instance, ref State __state)
        {
            __state = new State();

            try
            {
                if (__instance == null || __instance.bestiary == null || __instance.bestiary.enemies == null)
                    return;

                foreach (var kv in TitanMap)
                {
                    int id = kv.Key;
                    if (id < 0 || id >= __instance.bestiary.enemies.Count) continue;

                    __state.bestiaryKillsBefore[id] = __instance.bestiary.enemies[id].kills;
                }
            }
            catch
            {
                // Never break offline progress
            }
        }

        [HarmonyPostfix]
        private static void Postfix(Character __instance, State __state)
        {
            try
            {
                if (__instance == null || __state == null || __state.bestiaryKillsBefore == null) return;

                var ac = __instance.adventureController;
                if (ac == null || ac.lootDrop == null || ac.enemyList == null) return;
                if (__instance.bestiary == null || __instance.bestiary.enemies == null) return;

                LootDrop ld = ac.lootDrop;

                foreach (var kv in TitanMap)
                {
                    int bestiaryId = kv.Key;
                    TitanInfo info = kv.Value;

                    if (bestiaryId < 0 || bestiaryId >= __instance.bestiary.enemies.Count) continue;

                    long before;
                    if (!__state.bestiaryKillsBefore.TryGetValue(bestiaryId, out before))
                        before = 0;

                    long after = __instance.bestiary.enemies[bestiaryId].kills;
                    long delta = after - before;

                    if (delta <= 0) continue;

                    // Enemy template for that zone/version
                    Enemy template = null;
                    try
                    {
                        var zoneList = ac.enemyList[info.zone];
                        if (zoneList != null && info.enemyIndex >= 0 && info.enemyIndex < zoneList.Count)
                            template = zoneList[info.enemyIndex];
                    }
                    catch { template = null; }

                    if (template == null) continue;

                    MethodInfo miDrop = ZoneDropMethod(info.zone);
                    if (miDrop == null) continue;
                    // Roll drops once per offline kill delta
                    for (long i = 0; i < delta; i++)
                    {
                        var globalState = UnityEngine.Random.state;
                        try
                        {
                            UnityEngine.Random.state = __instance.lootState;

                            miDrop.Invoke(ld, new object[] { template });

                            __instance.lootState = UnityEngine.Random.state;
                        }
                        catch
                        {
                            // ignore
                        }
                        finally
                        {
                            UnityEngine.Random.state = globalState;
                        }
                    }
                }
            }
            catch
            {
                // Never break login/offline
            }
        }
    }
}
