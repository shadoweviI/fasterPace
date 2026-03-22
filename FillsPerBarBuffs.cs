using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;
#pragma warning disable Harmony003

namespace fasterPace
{
    // Put this in its own file/class to avoid collisions.

    [HarmonyPatch]
    internal static class Perk34_AllAlways_6PerFill
    {
        // ─────────────────────────────────────────────
        // Perk 34 settings
        // ─────────────────────────────────────────────
        private const int PERK_ID = 34;

        private const long NEW_MAX_LEVEL = 1;
        private const long NEW_BASE_COST = 100;

        // 6 levels per completed bar fill => +5 extra per vanilla +1
        // 6 levels per completed bar fill => +5 extra per vanilla +1
        private const int LEVELS_PER_FILL = (int)GeneralBuffs.GenSpeed * 2;

        private static Character _character;

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void CacheCharacter(AdventureController __instance)
        {
            _character = __instance?.character;
        }

        private static Character C => _character;

        private static bool HasT12HarderSet()
        {
            var maxxed = C?.inventory?.itemList?.itemMaxxed as IList<bool>;
            return maxxed != null
                && maxxed.Count > 479
                && maxxed[477]
                && maxxed[478]
                && maxxed[479];
        }

        private static long ExtraPerGain
        {
            get
            {
                long extra = LEVELS_PER_FILL - 1L; // base 6 fills => +5 extra

                if (HasT12HarderSet())
                    extra += 6L; // 6 fills -> 12 fills

                return extra;
            }
        }

        // Gold bonus behavior (leave as you want)
        internal const float GOLD_MULT_AT_LEVEL = 2f;

        [HarmonyPatch(typeof(Character), "addOfflineProgress")]
        internal static class Patch_Character_AddOfflineProgress_RefreshBars
        {
            [HarmonyPostfix]
            private static void Postfix(Character __instance)
            {
                __instance?.refreshMenus();
            }
        }


        // ─────────────────────────────────────────────
        // Perk 34: max level + base cost
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(ItopodPerkController), "Start")]
        internal static class Patch_ItopodPerk34_MaxLevelAndCost
        {
            [HarmonyPrefix]
            private static void Prefix(ItopodPerkController __instance)
            {
                if (__instance == null) return;

                __instance.maxLevel ??= new List<long>();
                __instance.cost ??= new List<long>();

                while (__instance.maxLevel.Count <= PERK_ID) __instance.maxLevel.Add(0L);
                while (__instance.cost.Count <= PERK_ID) __instance.cost.Add(0L);

                __instance.maxLevel[PERK_ID] = NEW_MAX_LEVEL;
                __instance.cost[PERK_ID] = NEW_BASE_COST;
            }
        }

        // ─────────────────────────────────────────────
        // Advanced Training: updateAdvancedTraining
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(AdvancedTrainingController), "updateAdvancedTraining")]
        internal static class Patch_AdvancedTraining_6PerFill
        {
            [HarmonyPrefix]
            private static void Prefix(AdvancedTrainingController __instance, ref long __state)
            {
                var ch = __instance?.character;
                if (ch == null) { __state = 0; return; }

                int id = __instance.id;
                if (id < 0 || id >= ch.advancedTraining.level.Length) { __state = 0; return; }

                __state = ch.advancedTraining.level[id];
            }

            [HarmonyPostfix]
            private static void Postfix(AdvancedTrainingController __instance, long __state)
            {
                var ch = __instance?.character;
                if (ch == null) return;

                int id = __instance.id;
                if (id < 0 || id >= ch.advancedTraining.level.Length) return;

                long after = ch.advancedTraining.level[id];
                long gained = after - __state;
                if (gained <= 0) return;

                long extra = ExtraPerGain * gained;
                ch.advancedTraining.level[id] += extra;
                ch.settings.rebirthLevels += extra;

                __instance.updateText();
            }
        }
        // ─────────────────────────────────────────────
        // Time Machine: speed + gold multi
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(TimeMachineController), "advanceSpeedProgress")]
        internal static class Patch_TimeMachine_Speed_6PerFill
        {
            [HarmonyPrefix]
            private static void Prefix(TimeMachineController __instance, ref long __state)
            {
                var ch = __instance?.character;
                __state = ch?.machine?.levelSpeed ?? 0L;
            }

            [HarmonyPostfix]
            private static void Postfix(TimeMachineController __instance, long __state)
            {
                var ch = __instance?.character;
                if (ch?.machine == null) return;

                long after = ch.machine.levelSpeed;
                long gained = after - __state;
                if (gained <= 0) return;

                long extra = ExtraPerGain * gained;
                ch.machine.levelSpeed += extra;
                ch.settings.rebirthLevels += extra;

                __instance.updateSpeedText();
            }
        }

        [HarmonyPatch(typeof(TimeMachineController), "advanceGoldMultiProgress")]
        internal static class Patch_TimeMachine_GoldMulti_6PerFill
        {
            [HarmonyPrefix]
            private static void Prefix(TimeMachineController __instance, ref long __state)
            {
                var ch = __instance?.character;
                __state = ch?.machine?.levelGoldMulti ?? 0L;
            }

            [HarmonyPostfix]
            private static void Postfix(TimeMachineController __instance, long __state)
            {
                var ch = __instance?.character;
                if (ch?.machine == null) return;

                long after = ch.machine.levelGoldMulti;
                long gained = after - __state;
                if (gained <= 0) return;

                long extra = ExtraPerGain * gained;
                ch.machine.levelGoldMulti += extra;
                ch.settings.rebirthLevels += extra;

                __instance.updateGoldMultiText();
            }
        }


        // ─────────────────────────────────────────────
        // Beards: advanceBeard
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(AllBeardsController), "advanceBeard")]
        internal static class Patch_Beards_6PerFill
        {
            [HarmonyPrefix]
            private static void Prefix(AllBeardsController __instance, int id, ref long __state)
            {
                var ch = __instance?.character;
                if (ch?.beards?.beards == null || id < 0 || id >= ch.beards.beards.Count)
                {
                    __state = 0;
                    return;
                }

                __state = ch.beards.beards[id].beardLevel;
            }

            [HarmonyPostfix]
            private static void Postfix(AllBeardsController __instance, int id, long __state)
            {
                var ch = __instance?.character;
                if (ch?.beards?.beards == null || id < 0 || id >= ch.beards.beards.Count) return;

                long after = ch.beards.beards[id].beardLevel;
                long gained = after - __state;
                if (gained <= 0) return;

                long extra = ExtraPerGain * gained;
                ch.beards.beards[id].beardLevel += extra;
                ch.settings.rebirthLevels += extra;
            }
        }

        // ─────────────────────────────────────────────
        // Augments: advanceAug + advanceUpgrade
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(AugmentController), "advanceAug")]
        internal static class Patch_Augment_AdvanceAug_6PerFill
        {
            [HarmonyPrefix]
            private static void Prefix(AugmentController __instance, ref long __state)
            {
                var ch = __instance?.character;
                if (ch?.augments?.augs == null) { __state = 0; return; }

                int id = __instance.id;
                if (id < 0 || id >= ch.augments.augs.Length || ch.augments.augs[id] == null)
                {
                    __state = 0;
                    return;
                }

                __state = ch.augments.augs[id].augLevel;
            }

            [HarmonyPostfix]
            private static void Postfix(AugmentController __instance, long __state)
            {
                var ch = __instance?.character;
                if (ch?.augments?.augs == null) return;

                int id = __instance.id;
                if (id < 0 || id >= ch.augments.augs.Length || ch.augments.augs[id] == null) return;

                long after = ch.augments.augs[id].augLevel;
                long gained = after - __state;
                if (gained <= 0) return;

                long extra = ExtraPerGain * gained;
                ch.augments.augs[id].augLevel += extra;
                ch.settings.rebirthLevels += extra;
            }
        }

        [HarmonyPatch(typeof(AugmentController), "advanceUpgrade")]
        internal static class Patch_Augment_AdvanceUpgrade_6PerFill
        {
            [HarmonyPrefix]
            private static void Prefix(AugmentController __instance, ref long __state)
            {
                var ch = __instance?.character;
                if (ch?.augments?.augs == null) { __state = 0; return; }

                int id = __instance.id;
                if (id < 0 || id >= ch.augments.augs.Length || ch.augments.augs[id] == null)
                {
                    __state = 0;
                    return;
                }

                __state = ch.augments.augs[id].upgradeLevel;
            }

            [HarmonyPostfix]
            private static void Postfix(AugmentController __instance, long __state)
            {
                var ch = __instance?.character;
                if (ch?.augments?.augs == null) return;

                int id = __instance.id;
                if (id < 0 || id >= ch.augments.augs.Length || ch.augments.augs[id] == null) return;

                long after = ch.augments.augs[id].upgradeLevel;
                long gained = after - __state;
                if (gained <= 0) return;

                long extra = ExtraPerGain * gained;
                ch.augments.augs[id].upgradeLevel += extra;
                ch.settings.rebirthLevels += extra;
            }
        }

        // ─────────────────────────────────────────────
        // Wandoos98: energy + magic progress
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(Wandoos98Controller), "advanceEnergyProgress")]
        internal static class Patch_Wandoos_Energy_6PerFill
        {
            [HarmonyPrefix]
            private static void Prefix(Wandoos98Controller __instance, ref long __state)
            {
                var ch = __instance?.character;
                __state = ch?.wandoos98?.energyLevel ?? 0L;
            }

            [HarmonyPostfix]
            private static void Postfix(Wandoos98Controller __instance, long __state)
            {
                var ch = __instance?.character;
                if (ch?.wandoos98 == null) return;

                long after = ch.wandoos98.energyLevel;
                long gained = after - __state;
                if (gained <= 0) return;

                long extra = ExtraPerGain * gained;
                ch.wandoos98.energyLevel += extra;
                ch.settings.rebirthLevels += extra;

                __instance.updateText();
                __instance.updateBars();
            }
        }

        [HarmonyPatch(typeof(Wandoos98Controller), "advanceMagicProgress")]
        internal static class Patch_Wandoos_Magic_6PerFill
        {
            [HarmonyPrefix]
            private static void Prefix(Wandoos98Controller __instance, ref long __state)
            {
                var ch = __instance?.character;
                __state = ch?.wandoos98?.magicLevel ?? 0L;
            }

            [HarmonyPostfix]
            private static void Postfix(Wandoos98Controller __instance, long __state)
            {
                var ch = __instance?.character;
                if (ch?.wandoos98 == null) return;

                long after = ch.wandoos98.magicLevel;
                long gained = after - __state;
                if (gained <= 0) return;

                long extra = ExtraPerGain * gained;
                ch.wandoos98.magicLevel += extra;
                ch.settings.rebirthLevels += extra;

                __instance.updateText();
                __instance.updateBars();
            }
        }

        // ─────────────────────────────────────────────
        // Blood Rituals: updateBloodMagic (ritual is List<Ritual>)
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(BloodMagicController), "updateBloodMagic")]
        internal static class Patch_BloodRitual_6PerFill
        {
            [HarmonyPrefix]
            private static void Prefix(BloodMagicController __instance, ref long __state)
            {
                var ch = __instance?.character;
                var rituals = ch?.bloodMagic?.ritual;
                if (rituals == null) { __state = 0; return; }

                int id = __instance.id;
                if (id < 0 || id >= rituals.Count) { __state = 0; return; }

                __state = rituals[id].level;
            }

            [HarmonyPostfix]
            private static void Postfix(BloodMagicController __instance, long __state)
            {
                var ch = __instance?.character;
                var rituals = ch?.bloodMagic?.ritual;
                if (rituals == null) return;

                int id = __instance.id;
                if (id < 0 || id >= rituals.Count) return;

                long after = rituals[id].level;
                long gained = after - __state;
                if (gained <= 0) return;

                long extraLevels = ExtraPerGain * gained;
                if (extraLevels <= 0) return;

                rituals[id].level += extraLevels;
                ch.settings.rebirthLevels += extraLevels;

                // Match vanilla per-level blood point gain
                double perLevelBlood = 0.0;
                try { perLevelBlood = __instance.bloodAdded(); } catch { }

                if (perLevelBlood > 0.0)
                    ch.bloodMagic.bloodPoints += perLevelBlood * (double)extraLevels;

                __instance.updateBloodMagicText();
            }
        }
        // ─────────────────────────────────────────────
        // OFFLINE: Wandoos
        // Character.wandoosOfflineProgress(int seconds)
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(Character), "wandoosOfflineProgress")]
        internal static class Patch_Offline_Wandoos_ExtraPerGain
        {
            private struct State { public long e0; public long m0; public bool ok; }

            [HarmonyPrefix]
            private static void Prefix(Character __instance, out State __state)
            {
                var w = __instance?.wandoos98;
                if (w == null)
                {
                    __state = default;
                    return;
                }

                __state = new State
                {
                    e0 = w.energyLevel,
                    m0 = w.magicLevel,
                    ok = true
                };
            }

            [HarmonyPostfix]
            private static void Postfix(Character __instance, State __state)
            {
                if (!__state.ok) return;
                if (ExtraPerGain <= 0) return;

                var w = __instance?.wandoos98;
                if (w == null) return;

                long eg = w.energyLevel - __state.e0;
                long mg = w.magicLevel - __state.m0;
                if (eg <= 0 && mg <= 0) return;

                if (eg > 0) w.energyLevel += ExtraPerGain * eg;
                if (mg > 0) w.magicLevel += ExtraPerGain * mg;

                long extra = ExtraPerGain * (Math.Max(0, eg) + Math.Max(0, mg));
                if (extra > 0) __instance.settings.rebirthLevels += extra;
            }
        }

        // ─────────────────────────────────────────────
        // OFFLINE: Time Machine
        // Character.timeMachineOfflineProgress(int seconds)
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(Character), "timeMachineOfflineProgress")]
        internal static class Patch_Offline_TimeMachine_ExtraPerGain
        {
            private struct State { public long s0; public long g0; public bool ok; }

            [HarmonyPrefix]
            private static void Prefix(Character __instance, out State __state)
            {
                var m = __instance?.machine;
                if (m == null)
                {
                    __state = default;
                    return;
                }

                __state = new State
                {
                    s0 = m.levelSpeed,
                    g0 = m.levelGoldMulti,
                    ok = true
                };
            }

            [HarmonyPostfix]
            private static void Postfix(Character __instance, State __state)
            {
                if (!__state.ok) return;
                if (ExtraPerGain <= 0) return;

                var m = __instance?.machine;
                if (m == null) return;

                long sg = m.levelSpeed - __state.s0;
                long gg = m.levelGoldMulti - __state.g0;
                if (sg <= 0 && gg <= 0) return;

                if (sg > 0) m.levelSpeed += ExtraPerGain * sg;
                if (gg > 0) m.levelGoldMulti += ExtraPerGain * gg;

                long extra = ExtraPerGain * (Math.Max(0, sg) + Math.Max(0, gg));
                if (extra > 0) __instance.settings.rebirthLevels += extra;
            }
        }

        // ─────────────────────────────────────────────
        // OFFLINE: Advanced Training
        // Character.advancedTrainingOfflineProgress(int seconds)
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(Character), "advancedTrainingOfflineProgress")]
        internal static class Patch_Offline_AdvancedTraining_ExtraPerGain
        {
            private struct State { public long[] before; public bool ok; }

            [HarmonyPrefix]
            private static void Prefix(Character __instance, out State __state)
            {
                var lv = __instance?.advancedTraining?.level;
                if (lv == null)
                {
                    __state = default;
                    return;
                }

                __state = new State
                {
                    before = (long[])lv.Clone(),
                    ok = true
                };
            }

            [HarmonyPostfix]
            private static void Postfix(Character __instance, State __state)
            {
                if (!__state.ok) return;
                if (ExtraPerGain <= 0) return;

                var lv = __instance?.advancedTraining?.level;
                if (lv == null) return;

                var before = __state.before;
                if (before == null) return;

                int n = Math.Min(lv.Length, before.Length);
                long totalExtra = 0L;

                for (int i = 0; i < n; i++)
                {
                    long gained = lv[i] - before[i];
                    if (gained <= 0) continue;

                    long extra = ExtraPerGain * gained;
                    lv[i] += extra;
                    totalExtra += extra;
                }

                if (totalExtra > 0) __instance.settings.rebirthLevels += totalExtra;
            }
        }

        // ─────────────────────────────────────────────
        // OFFLINE: Augments
        // Character.augmentOfflineProgress(int seconds)
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(Character), "augmentOfflineProgress")]
        internal static class Patch_Offline_Augments_ExtraPerGain
        {
            private struct State
            {
                public long[] aug0;
                public long[] upg0;
                public bool ok;
            }

            [HarmonyPrefix]
            private static void Prefix(Character __instance, out State __state)
            {
                var augs = __instance?.augments?.augs;
                if (augs == null)
                {
                    __state = default;
                    return;
                }

                int n = augs.Length;
                var aug0 = new long[n];
                var upg0 = new long[n];

                for (int i = 0; i < n; i++)
                {
                    var a = augs[i];
                    if (a == null) continue;

                    aug0[i] = a.augLevel;
                    upg0[i] = a.upgradeLevel;
                }

                __state = new State
                {
                    aug0 = aug0,
                    upg0 = upg0,
                    ok = true
                };
            }

            [HarmonyPostfix]
            private static void Postfix(Character __instance, State __state)
            {
                if (!__state.ok) return;
                if (ExtraPerGain <= 0) return;

                var augs = __instance?.augments?.augs;
                if (augs == null) return;

                var aug0 = __state.aug0;
                var upg0 = __state.upg0;
                if (aug0 == null || upg0 == null) return;

                int n = Math.Min(augs.Length, Math.Min(aug0.Length, upg0.Length));
                long totalExtra = 0L;

                for (int i = 0; i < n; i++)
                {
                    var a = augs[i];
                    if (a == null) continue;

                    long gAug = a.augLevel - aug0[i];
                    if (gAug > 0)
                    {
                        long extra = ExtraPerGain * gAug;
                        a.augLevel += extra;
                        totalExtra += extra;
                    }

                    long gUpg = a.upgradeLevel - upg0[i];
                    if (gUpg > 0)
                    {
                        long extra = ExtraPerGain * gUpg;
                        a.upgradeLevel += extra;
                        totalExtra += extra;
                    }
                }

                if (totalExtra > 0) __instance.settings.rebirthLevels += totalExtra;
            }
        }

        // ─────────────────────────────────────────────
        // OFFLINE: Blood Magic
        // Character.bloodMagicOfflineProgress(int seconds)
        // NOTE: adds extra ritual levels only.
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(Character), "bloodMagicOfflineProgress")]
        internal static class Patch_Offline_BloodMagic_ExtraPerGain
        {
            private struct State { public long[] before; public bool ok; }

            [HarmonyPrefix]
            private static void Prefix(Character __instance, out State __state)
            {
                var r = __instance?.bloodMagic?.ritual;
                if (r == null)
                {
                    __state = default;
                    return;
                }

                int n = r.Count;
                var before = new long[n];
                for (int i = 0; i < n; i++)
                    before[i] = r[i].level;

                __state = new State
                {
                    before = before,
                    ok = true
                };
            }

            [HarmonyPostfix]
            private static void Postfix(Character __instance, State __state)
            {
                if (!__state.ok) return;
                if (ExtraPerGain <= 0) return;

                var r = __instance?.bloodMagic?.ritual;
                if (r == null) return;

                var before = __state.before;
                if (before == null) return;

                int n = Math.Min(r.Count, before.Length);
                long totalExtra = 0L;

                for (int i = 0; i < n; i++)
                {
                    long gained = r[i].level - before[i];
                    if (gained <= 0) continue;

                    long extra = ExtraPerGain * gained;
                    r[i].level += extra;
                    totalExtra += extra;
                }

                if (totalExtra > 0) __instance.settings.rebirthLevels += totalExtra;
            }
        }

        // ─────────────────────────────────────────────
        // OFFLINE: Beards
        // Character.beardOfflineProgress(int energyBeardTime, int magicBeardTime)
        // ─────────────────────────────────────────────
        [HarmonyPatch(typeof(Character), "beardOfflineProgress")]
        internal static class Patch_Offline_Beards_ExtraPerGain
        {
            private struct State { public long[] before; public bool ok; }

            [HarmonyPrefix]
            private static void Prefix(Character __instance, out State __state)
            {
                var b = __instance?.beards?.beards;
                if (b == null)
                {
                    __state = default;
                    return;
                }

                int n = b.Count;
                var before = new long[n];
                for (int i = 0; i < n; i++)
                    before[i] = b[i].beardLevel;

                __state = new State
                {
                    before = before,
                    ok = true
                };
            }

            [HarmonyPostfix]
            private static void Postfix(Character __instance, State __state)
            {
                if (!__state.ok) return;
                if (ExtraPerGain <= 0) return;

                var b = __instance?.beards?.beards;
                if (b == null) return;

                var before = __state.before;
                if (before == null) return;

                int n = Math.Min(b.Count, before.Length);
                long totalExtra = 0L;

                for (int i = 0; i < n; i++)
                {
                    long gained = b[i].beardLevel - before[i];
                    if (gained <= 0) continue;

                    long extra = ExtraPerGain * gained;
                    b[i].beardLevel += extra;
                    totalExtra += extra;
                }

                if (totalExtra > 0) __instance.settings.rebirthLevels += totalExtra;
            }
        }

    }
}
