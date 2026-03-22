using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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

        // Vanilla-ish numbers (you keep these unchanged)
        internal const float TIME_PER_COMPLETION = 900f; // 15 minutes (vanilla step)
        internal const float FINAL_FLAT = 300f;          // 5 minutes

        private const float Divider = GeneralBuffs.GenSpeed;

        private static Character character;
        private static int normalCompletions => character?.allChallenges?.noRebirthChallenge?.completions() ?? 0;
        private static int evilCompletions => character?.allChallenges?.noRebirthChallenge?.evilCompletions() ?? 0;
        private static int sadCompletions => character?.allChallenges?.noRebirthChallenge?.sadisticCompletions() ?? 0;

        private static readonly float[] baseSpawnTimes =
        {
            0f, 3600f, 3600f, 7200f, 7200f, 10800f, 12600f, 16200f, 18000f, 19800f,
            23400f, 25200f, 27000f, 27000f, 27000f
        };

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance) => character = __instance?.character;

        // Compute minimum automatically from boss1 base (so you never calculate it)
        internal static float MinSpawnTimeScaled
        {
            get
            {
                float min = baseSpawnTimes[1] / Divider; 

                if (evilCompletions >= 10) min -= FINAL_FLAT;
                if (sadCompletions >= 10) min -= FINAL_FLAT;

                // Don’t go negative
                if (min < 0f) min = 0f;
                return min;
            }
        }

        // Base time scaled by divider
        private static float BaseScaled(int bossIndex) => baseSpawnTimes[bossIndex] / Divider;

        internal static float CompletionStepScaled => TIME_PER_COMPLETION / Divider;
        private static float FinalFlatUnscaled => FINAL_FLAT;

        private static float SpawnTime(int bossIndex, int completionSum)
        {
            float time = BaseScaled(bossIndex);

            // No-rebirth reduces like vanilla, but also scaled by Divider so it matches the new time scale
            time -= CompletionStepScaled * completionSum;

            // Evil/Sad final: flat reduction (scaled to match the new time scale)
            if (evilCompletions >= 10) time -= FinalFlatUnscaled;
            if (sadCompletions >= 10) time -= FinalFlatUnscaled;

            return Math.Max(MinSpawnTimeScaled, time);
        }

        // Boss 1–6: normal only
        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss1SpawnTime")]
        private static bool b1(ref float __result) { __result = SpawnTime(1, normalCompletions); return false; }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss2SpawnTime")]
        private static bool b2(ref float __result) { __result = SpawnTime(2, normalCompletions); return false; }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss3SpawnTime")]
        private static bool b3(ref float __result) { __result = SpawnTime(3, normalCompletions); return false; }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss4SpawnTime")]
        private static bool b4(ref float __result) { __result = SpawnTime(4, normalCompletions); return false; }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss5SpawnTime")]
        private static bool b5(ref float __result) { __result = SpawnTime(5, normalCompletions); return false; }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss6SpawnTime")]
        private static bool b6(ref float __result) { __result = SpawnTime(6, normalCompletions); return false; }

        // Boss 7–9: normal + evil
        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss7SpawnTime")]
        private static bool b7(ref float __result) { __result = SpawnTime(7, normalCompletions + evilCompletions); return false; }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss8SpawnTime")]
        private static bool b8(ref float __result) { __result = SpawnTime(8, normalCompletions + evilCompletions); return false; }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss9SpawnTime")]
        private static bool b9(ref float __result) { __result = SpawnTime(9, normalCompletions + evilCompletions); return false; }

        // Boss 10–14: normal + evil + sad
        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss10SpawnTime")]
        private static bool b10(ref float __result) { __result = SpawnTime(10, normalCompletions + evilCompletions + sadCompletions); return false; }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss11SpawnTime")]
        private static bool b11(ref float __result) { __result = SpawnTime(11, normalCompletions + evilCompletions + sadCompletions); return false; }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss12SpawnTime")]
        private static bool b12(ref float __result) { __result = SpawnTime(12, normalCompletions + evilCompletions + sadCompletions); return false; }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss13SpawnTime")]
        private static bool b13(ref float __result) { __result = SpawnTime(13, normalCompletions + evilCompletions + sadCompletions); return false; }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "boss14SpawnTime")]
        private static bool b14(ref float __result) { __result = SpawnTime(14, normalCompletions + evilCompletions + sadCompletions); return false; }
    }


        [HarmonyPatch(typeof(AdventureController))]
        internal static class Titan1To8_Autokill5Kills
        {
            private const int NewKillReq = 5;

            // ------------------------------------------------------------
            // Titans 6-8: add kill-based unlock alongside the vanilla stat checks
            // ------------------------------------------------------------

            [HarmonyPrefix, HarmonyPatch("autokillTitan6V1Achieved")]
            private static bool Titan6V1(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[312].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 2.5E+09f && c.totalAdvDefense() >= 1.6E+09f && c.totalAdvHPRegen() >= 25000000f);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("autokillTitan6V2Achieved")]
            private static bool Titan6V2(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[313].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 2.5E+10f && c.totalAdvDefense() >= 1.6E+10f && c.totalAdvHPRegen() >= 250000000f);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("autokillTitan6V3Achieved")]
            private static bool Titan6V3(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[314].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 2.5E+11f && c.totalAdvDefense() >= 1.6E+11f && c.totalAdvHPRegen() >= 2.5E+09f);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("autokillTitan6V4Achieved")]
            private static bool Titan6V4(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[315].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 2.5E+12f && c.totalAdvDefense() >= 1.6E+12f && c.totalAdvHPRegen() >= 2.5E+10f);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("autokillTitan7V1Achieved")]
            private static bool Titan7V1(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[334].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 5E+14f && c.totalAdvDefense() >= 2.5E+14f && c.totalAdvHPRegen() >= 5E+12f);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("autokillTitan7V2Achieved")]
            private static bool Titan7V2(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[335].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 1E+16f && c.totalAdvDefense() >= 5E+15f && c.totalAdvHPRegen() >= 1E+14f);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("autokillTitan7V3Achieved")]
            private static bool Titan7V3(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[336].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 2E+17f && c.totalAdvDefense() >= 1E+17f && c.totalAdvHPRegen() >= 2E+15f);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("autokillTitan7V4Achieved")]
            private static bool Titan7V4(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[337].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 5E+18f && c.totalAdvDefense() >= 2.5E+18f && c.totalAdvHPRegen() >= 5E+16f);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("autokillTitan8V1Achieved")]
            private static bool Titan8V1(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[339].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 5E+18f && c.totalAdvDefense() >= 2.5E+18f && c.totalAdvHPRegen() >= 5E+16f);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("autokillTitan8V2Achieved")]
            private static bool Titan8V2(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[340].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 1E+20f && c.totalAdvDefense() >= 5E+19f && c.totalAdvHPRegen() >= 1E+18f);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("autokillTitan8V3Achieved")]
            private static bool Titan8V3(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[341].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 2E+21f && c.totalAdvDefense() >= 1E+21f && c.totalAdvHPRegen() >= 2E+19f);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("autokillTitan8V4Achieved")]
            private static bool Titan8V4(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[342].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 5E+22f && c.totalAdvDefense() >= 2.5E+22f && c.totalAdvHPRegen() >= 5E+20f);
                return false;
            }

            // ------------------------------------------------------------
            // Titans 1-5: no "Achieved" methods exist, so add an early-kill shortcut
            // inside manageFight(). If you have 5+ kills of that titan, autokill it.
            // ------------------------------------------------------------

            [HarmonyPrefix, HarmonyPatch("manageFight")]
            private static bool ManageFight_KillShortcut(AdventureController __instance)
            {
                var c = __instance.character;
                if (c == null) return true;

                // only act when autokill is enabled
                if (!c.settings.autoKillTitans) return true;

                // Titan 1 (enemy 302, zone 6)
                if (c.effectiveBossID() >= 58 &&
                    c.adventure.boss1Spawn.totalseconds >= (double)__instance.boss1SpawnTime() &&
                    c.bestiary.enemies[302].kills >= NewKillReq)
                {
                    if (__instance.zone == 6 && __instance.currentEnemy != null) __instance.wipeEnemy();
                    c.bestiaryController.addKills(302, 1);
                    c.adventure.boss1Spawn.reset();
                    __instance.lootDrop.zone6Drop(__instance.enemyList[6][0]);
                    return false;
                }

                // Titan 2 (enemy 303, zone 8)
                if (c.effectiveBossID() >= 66 &&
                    c.adventure.boss2Spawn.totalseconds >= (double)__instance.boss2SpawnTime() &&
                    c.bestiary.enemies[303].kills >= NewKillReq)
                {
                    if (__instance.zone == 8 && __instance.currentEnemy != null) __instance.wipeEnemy();
                    c.bestiaryController.addKills(303, 1);
                    c.adventure.boss2Spawn.reset();
                    __instance.lootDrop.zone8Drop(__instance.enemyList[8][0]);
                    return false;
                }

                // Titan 3 (enemy 304, zone 11) + keeps the vanilla unlock
                if (c.effectiveBossID() >= 82 &&
                    c.adventure.boss3Spawn.totalseconds >= (double)__instance.boss3SpawnTime() &&
                    c.bestiary.enemies[304].kills >= NewKillReq)
                {
                    if (__instance.zone == 11 && __instance.currentEnemy != null) __instance.wipeEnemy();
                    c.bestiaryController.addKills(304, 1);
                    c.adventure.boss3Spawn.reset();
                    __instance.lootDrop.zone11Drop(__instance.enemyList[11][0]);
                    c.challenges.noRebirthChallenge.unlocked = true;
                    return false;
                }

                // Titan 4 (enemy 305, zone 14)
                if (c.effectiveBossID() >= 100 &&
                    c.adventure.boss4Spawn.totalseconds >= (double)__instance.boss4SpawnTime() &&
                    c.bestiary.enemies[305].kills >= NewKillReq)
                {
                    if (__instance.zone == 14 && __instance.currentEnemy != null) __instance.wipeEnemy();
                    c.bestiaryController.addKills(305, 1);
                    c.adventure.boss4Spawn.reset();
                    __instance.lootDrop.zone14Drop(__instance.enemyList[14][0]);
                    return false;
                }

                // Titan 5 (real/final titan is enemy 310, zone 16)
                // Keep the "boss5Kills >= 3" gate so you don't skip the fake versions progression.
                if (c.effectiveBossID() >= 116 &&
                    c.adventure.boss5Spawn.totalseconds >= (double)__instance.boss5SpawnTime() &&
                    c.adventure.boss5Kills >= 3 &&
                    c.bestiary.enemies[310].kills >= NewKillReq)
                {
                    if (__instance.zone == 16 && __instance.currentEnemy != null) __instance.wipeEnemy();
                    c.bestiaryController.addKills(310, 1);
                    c.adventure.boss5Spawn.reset();
                    __instance.lootDrop.zone16Drop(__instance.enemyList[16][4]);
                    c.allAchievements.markAchievementAsComplete(145);
                    return false;
                }

                return true; // run vanilla manageFight for everything else
            }
        }

        [HarmonyPatch(typeof(AdventureController))]
        internal static class Titan9AutoKillThresholdPatch
        {
            private const int NewKillReq = 5;

            [HarmonyPrefix]
            [HarmonyPatch("autokillTitan9V1Achieved")]
            private static bool V1(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[344].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 1E+23f && c.totalAdvDefense() >= 5E+22f && c.totalAdvHPRegen() >= 1E+21f);
                return false; // skip original
            }

            [HarmonyPrefix]
            [HarmonyPatch("autokillTitan9V2Achieved")]
            private static bool V2(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[345].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 2E+24f && c.totalAdvDefense() >= 1E+24f && c.totalAdvHPRegen() >= 2E+22f);
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("autokillTitan9V3Achieved")]
            private static bool V3(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[346].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 4E+25f && c.totalAdvDefense() >= 2E+25f && c.totalAdvHPRegen() >= 4E+23f);
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("autokillTitan9V4Achieved")]
            private static bool V4(AdventureController __instance, ref bool __result)
            {
                var c = __instance.character;
                __result = c.bestiary.enemies[347].kills >= NewKillReq
                    || (c.totalAdvAttack() >= 7.5E+26f && c.totalAdvDefense() >= 3.7E+26f && c.totalAdvHPRegen() >= 7.5E+24f);
                return false;
            }
        }

    // 5 titan kills unlock autokill to OFFLINE progress for Titans 1-5.

    [HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
    internal static class Patch_Character_AdventureOfflineProgress_Titan1To5_UnlockByKills
    {
        private const int NewKillReq = 5;

        // Enemy IDs you used in manageFight
        private const int T1 = 302;
        private const int T2 = 303;
        private const int T3 = 304;
        private const int T4 = 305;
        private const int T5_FINAL = 310;

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);
            var cm = new CodeMatcher(codes, il);

            // Inject after bossID gate for each titan:
            // if (bestiary.enemies[ID].kills >= 5) goto successBody;
            InjectKillBypass(cm, il, bossIdGate: 58, enemyId: T1, extraGateBoss5Kills: false);
            InjectKillBypass(cm, il, bossIdGate: 66, enemyId: T2, extraGateBoss5Kills: false);
            InjectKillBypass(cm, il, bossIdGate: 82, enemyId: T3, extraGateBoss5Kills: false);
            InjectKillBypass(cm, il, bossIdGate: 100, enemyId: T4, extraGateBoss5Kills: false);

            // Titan 5: require boss5Kills >= 3 like your online shortcut
            InjectKillBypass(cm, il, bossIdGate: 116, enemyId: T5_FINAL, extraGateBoss5Kills: true);

            return cm.InstructionEnumeration();
        }

        private static void InjectKillBypass(CodeMatcher cm, ILGenerator il, int bossIdGate, int enemyId, bool extraGateBoss5Kills)
        {
            // We search for: call effectiveBossID; ldc.i4 <bossIdGate>; blt.s <failLabel>
            // and then insert our kill-check right after that bossID gate passes.

            var mEffectiveBossID = AccessTools.Method(typeof(Character), "effectiveBossID");
            var fBestiary = AccessTools.Field(typeof(Character), "bestiary");
            var fEnemies = AccessTools.Field(typeof(Bestiary), "enemies");
            var mListGetItem = AccessTools.Method(typeof(List<BestiaryInfo>), "get_Item");
            var fKills = AccessTools.Field(typeof(BestiaryInfo), "kills");

            // For Titan 5 extra gate: this.adventure.boss5Kills >= 3
            var fAdventure = AccessTools.Field(typeof(Character), "adventure");
            var fBoss5Kills = AccessTools.Field(fAdventure.FieldType, "boss5Kills"); // field lives on Adventure (type name varies per build)

            cm.Start();
            cm.MatchForward(false,
                new CodeMatch(ci => ci.opcode == OpCodes.Ldarg_0),
                new CodeMatch(ci => ci.opcode == OpCodes.Call && (ci.operand as MethodInfo) == mEffectiveBossID),
                new CodeMatch(ci => ci.opcode == OpCodes.Ldc_I4 && (int)ci.operand == bossIdGate),
                new CodeMatch(ci => ci.opcode == OpCodes.Blt || ci.opcode == OpCodes.Blt_S)
            );


            if (cm.IsInvalid)
                return; // if the pattern doesn't match in this build, just skip safely

            // Branch instruction is the bossID fail jump. The code immediately after this continues with stat checks.
            int bossGateBranchIndex = cm.Pos + 3;

            // We want to jump to the "success body" (the first instruction after ALL the stat checks for that titan).
            // In the compiled IL, that’s the first instruction after the LAST "blt/brfalse" check in the chain.
            //
            // Practical trick:
            //   - Start scanning forward for the first "addExp(" call in that titan block; it’s inside the success body.
            //   - Label that instruction as success and branch to it.
            //
            // This is robust across minor code shifts.

            var mAddExp = AccessTools.Method(typeof(Character), "addExp", new[] { typeof(long) });
            int scan = bossGateBranchIndex + 1;
            int successIndex = -1;

            // scan up to a reasonable distance so we don't accidentally hit later blocks
            for (int i = scan; i < Math.Min(scan + 200, cm.Length); i++)
            {
                var inst = cm.InstructionAt(i);
                if (inst.opcode == OpCodes.Call || inst.opcode == OpCodes.Callvirt)
                {
                    if (inst.operand as MethodInfo == mAddExp)
                    {
                        successIndex = i;
                        break;
                    }
                }
            }

            if (successIndex < 0)
                return;

            // Ensure the success instruction has a label we can branch to
            var successLabel = il.DefineLabel();
            var successInst = cm.InstructionAt(successIndex);
            successInst.labels ??= new List<Label>();
            successInst.labels.Add(successLabel);

            // Build injected code:
            // if (extraGateBoss5Kills) { if (this.adventure.boss5Kills < 3) goto continue; }
            // if (this.bestiary.enemies[enemyId].kills >= 5) goto successLabel;
            // continue: (falls through into vanilla stat checks)

            var injected = new List<CodeInstruction>();

            if (extraGateBoss5Kills && fAdventure != null && fBoss5Kills != null)
            {
                var continueLabel = il.DefineLabel();

                // if (this.adventure.boss5Kills < 3) goto continueLabel;
                injected.Add(new CodeInstruction(OpCodes.Ldarg_0));
                injected.Add(new CodeInstruction(OpCodes.Ldfld, fAdventure));
                injected.Add(new CodeInstruction(OpCodes.Ldfld, fBoss5Kills));
                injected.Add(new CodeInstruction(OpCodes.Ldc_I4_3));
                injected.Add(new CodeInstruction(OpCodes.Blt_S, continueLabel));

                // kill check
                injected.AddRange(BuildKillCheck(fBestiary, fEnemies, mListGetItem, fKills, enemyId, successLabel));

                // continue label (falls into vanilla stat checks)
                injected.Add(new CodeInstruction(OpCodes.Nop) { labels = new List<Label> { continueLabel } });
            }
            else
            {
                injected.AddRange(BuildKillCheck(fBestiary, fEnemies, mListGetItem, fKills, enemyId, successLabel));
            }

            // Insert right AFTER the bossID gate branch instruction
            cm.Advance(4); // position after the blt
            cm.Insert(injected);
        }

        private static IEnumerable<CodeInstruction> BuildKillCheck(
            FieldInfo fBestiary,
            FieldInfo fEnemies,
            MethodInfo mListGetItem,
            FieldInfo fKills,
            int enemyId,
            Label successLabel)
        {
            // if (this.bestiary.enemies[enemyId].kills >= NewKillReq) goto successLabel;

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, fBestiary);
            yield return new CodeInstruction(OpCodes.Ldfld, fEnemies);
            yield return new CodeInstruction(OpCodes.Ldc_I4, enemyId);
            yield return new CodeInstruction(OpCodes.Callvirt, mListGetItem);
            yield return new CodeInstruction(OpCodes.Ldfld, fKills);
            yield return new CodeInstruction(OpCodes.Ldc_I4, NewKillReq);
            yield return new CodeInstruction(OpCodes.Bge_S, successLabel);
        }
    }

    [HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
    internal static class Patch_Character_AdventureOfflineProgress_Titan5_UnlockByKills
    {
        private const int NewKillReq = 5;
        private const int WalderpEnemyId = 310;

        [ThreadStatic] private static int _oldBoss5Kills;
        [ThreadStatic] private static bool _changed;

        [HarmonyPrefix]
        private static void Prefix(Character __instance)
        {
            _changed = false;

            var adv = __instance?.adventure;
            var enemies = __instance?.bestiary?.enemies;
            if (adv == null || enemies == null) return;
            if (WalderpEnemyId < 0 || WalderpEnemyId >= enemies.Count) return;

            if (enemies[WalderpEnemyId].kills >= NewKillReq && adv.boss5Kills < 3)
            {
                _oldBoss5Kills = adv.boss5Kills;
                _changed = true;
                adv.boss5Kills = 3;
            }
        }

        [HarmonyPostfix]
        private static void Postfix(Character __instance)
        {
            if (!_changed) return;

            var adv = __instance?.adventure;
            if (adv == null) return;

            adv.boss5Kills = _oldBoss5Kills;
        }
    }
}

