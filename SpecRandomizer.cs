using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace fasterPace
{
    internal static class SpecRandUtil
    {
        // Exclusions (dead stats)
        private static readonly HashSet<specType> Excluded = new HashSet<specType>
        {
            specType.AP,
            specType.EXP,
            specType.EnergySpeed,
            specType.MagicSpeed,
            specType.AdvTraining,
            specType.AdvTraining2
        };

        private static List<specType> _cachedPool;

        public static List<specType> GetAllPossibleSpecsFiltered()
        {
            if (_cachedPool != null) return _cachedPool;

            var pool = new HashSet<specType>();

            var c = GetCharacter();
            if (c?.itemInfo == null)
                return _cachedPool ??= new List<specType>();

            var ii = c.itemInfo;

            for (int i = 0; i < ii.type.Length; i++)
            {
                Add(pool, ii.specType1[i]);
                Add(pool, ii.specType2[i]);
                Add(pool, ii.specType3[i]);
            }

            _cachedPool = new List<specType>(pool);
            return _cachedPool;

            static void Add(HashSet<specType> set, specType t)
            {
                if (t == specType.None) return;
                if (Excluded.Contains(t)) return;
                set.Add(t);
            }
        }

        public static Character GetCharacter()
        {
            try
            {
                // NoNGUSoftcaps has: private static Character character;
                var f = AccessTools.Field(typeof(NoNGUSoftcaps), "character");
                return f?.GetValue(null) as Character;
            }
            catch { return null; }
        }

        // Only randomize equipment pieces, not boosts/cubes/etc.
        public static bool IsTargetPart(part p) =>
            p == part.Head || p == part.Chest || p == part.Legs ||
            p == part.Boots || p == part.Weapon || p == part.Accessory;

        public struct XorShift32
        {
            private uint _s;
            public XorShift32(uint seed) => _s = (seed == 0) ? 0xA341316Cu : seed;

            public uint Next()
            {
                uint x = _s;
                x ^= x << 13;
                x ^= x >> 17;
                x ^= x << 5;
                _s = x;
                return x;
            }

            public float NextFloat01()
            {
                return (Next() & 0x00FFFFFF) / 16777216f;
            }
        }
    }

    [HarmonyPatch]
    internal static class SpecRandomizer
    {
        private const bool NEW_SPECS_START_FILLED = true;

        // ---------- Critical persistence fix ----------
        // Game clones equipment via ItemNameDesc.makeDummy() and DOES NOT copy numSpec.
        // We patch it so our per-item seed survives save/load.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemNameDesc), "makeDummy")]
        private static void ItemNameDesc_makeDummy_Postfix(Equipment toCopy, ref Equipment __result)
        {
            if (__result == null || toCopy == null) return;
            __result.numSpec = toCopy.numSpec; // keep seed
        }

        private static void EnsureSeed(Equipment e)
        {
            // numSpec is persisted on Equipment; we use it as the seed.
            if (e.numSpec != 0) return;

            int s = UnityEngine.Random.Range(1, int.MaxValue);
            if (s == 0) s = 1;
            e.numSpec = s;
        }

        private static specType Pick(List<specType> pool, ref SpecRandUtil.XorShift32 rng, HashSet<specType> used)
        {
            if (pool == null || pool.Count == 0) return specType.None;

            // Try to avoid duplicates within the same item
            for (int i = 0; i < 20; i++)
            {
                var t = pool[(int)(rng.Next() % (uint)pool.Count)];
                if (t != specType.None && used.Add(t))
                    return t;
            }

            // Fallback allow duplicates
            return pool[(int)(rng.Next() % (uint)pool.Count)];
        }

        // Base scaling (safe, sublinear)
        private static float BaseCap(float capAtk, float capDef, ref SpecRandUtil.XorShift32 rng)
        {
            float sum = Mathf.Max(1f, capAtk + capDef);

            float baseVal = Mathf.Pow(sum, 0.9f);

            // small deterministic variance
            float r = 0.85f + (rng.NextFloat01() * 0.90f);
            baseVal *= r;

            float maxRel = sum * 0.6f;

            float result = Mathf.Clamp(baseVal, 1f, maxRel);
            return Mathf.Floor(result);
        }

        // Per-spec clamps you requested
        private static float CapForType(specType t, float capAtk, float capDef, ref SpecRandUtil.XorShift32 rng)
        {
            float v = BaseCap(capAtk, capDef, ref rng);

            // Respawn + Movement Cooldown must be small
            if (t == specType.Respawn || t == specType.Cooldown)
                return Mathf.Clamp(v, 1000f, 16000f);

            // Yggdrasil gain must stay 10m..20m
            if (t == specType.Yggdrasil)
                return Mathf.Clamp(v, 10_000_000f, 20_000_000f);

            return v;
        }

        private static void ApplyToOne(Equipment e)
        {
            if (e == null) return;
            if (e.id == 0) return;
            if (!SpecRandUtil.IsTargetPart(e.type)) return;

            var pool = SpecRandUtil.GetAllPossibleSpecsFiltered();
            if (pool.Count == 0) return;

            EnsureSeed(e);

            // Deterministic RNG per item instance
            var rng = new SpecRandUtil.XorShift32((uint)e.numSpec);

            // You want ALL items to have 3 specs.
            var used = new HashSet<specType>();
            var t1 = Pick(pool, ref rng, used);
            var t2 = Pick(pool, ref rng, used);
            var t3 = Pick(pool, ref rng, used);

            // Apply types (same seed => same types forever)
            e.spec1Type = t1;
            e.spec2Type = t2;
            e.spec3Type = t3;

            // Preserve existing progress ratio if caps exist
            float r1 = (e.spec1Cap > 0f) ? (e.spec1Cur / e.spec1Cap) : 1f;
            float r2 = (e.spec2Cap > 0f) ? (e.spec2Cur / e.spec2Cap) : 1f;
            float r3 = (e.spec3Cap > 0f) ? (e.spec3Cur / e.spec3Cap) : 1f;

            // Initialize caps if missing OR if they are insane for clamped types (optional safety)
            if (e.spec1Cap <= 0f) e.spec1Cap = CapForType(e.spec1Type, e.capAttack, e.capDefense, ref rng);
            if (e.spec2Cap <= 0f) e.spec2Cap = CapForType(e.spec2Type, e.capAttack, e.capDefense, ref rng);
            if (e.spec3Cap <= 0f) e.spec3Cap = CapForType(e.spec3Type, e.capAttack, e.capDefense, ref rng);

            // Enforce clamps even if caps were already set (prevents old broken items)
            e.spec1Cap = CapForType(e.spec1Type, e.capAttack, e.capDefense, ref rng);
            e.spec2Cap = CapForType(e.spec2Type, e.capAttack, e.capDefense, ref rng);
            e.spec3Cap = CapForType(e.spec3Type, e.capAttack, e.capDefense, ref rng);

            // Restore progress or start full
            if (e.spec1Cap > 0f) e.spec1Cur = NEW_SPECS_START_FILLED ? e.spec1Cap : (e.spec1Cap * Mathf.Clamp01(r1));
            if (e.spec2Cap > 0f) e.spec2Cur = NEW_SPECS_START_FILLED ? e.spec2Cap : (e.spec2Cap * Mathf.Clamp01(r2));
            if (e.spec3Cap > 0f) e.spec3Cur = NEW_SPECS_START_FILLED ? e.spec3Cap : (e.spec3Cap * Mathf.Clamp01(r3));
        }

        // New loot: apply immediately
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Equipment), MethodType.Constructor, new Type[]
        {
            typeof(part), typeof(int),
            typeof(float), typeof(float), typeof(float), typeof(float),
            typeof(specType), typeof(float), typeof(float),
            typeof(specType), typeof(float), typeof(float),
            typeof(specType), typeof(float), typeof(float),
            typeof(string), typeof(int)
        })]
        private static void EquipmentCtor_Postfix(Equipment __instance)
        {
            ApplyToOne(__instance);
        }

        // Persistence: re-apply after rebuild (idempotent because seed is stable)
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void UpdateItemStats_Postfix()
        {
            var c = SpecRandUtil.GetCharacter();
            if (c == null || c.inventory == null) return;

            void ApplyList(List<Equipment> list)
            {
                if (list == null) return;
                for (int i = 0; i < list.Count; i++)
                    ApplyToOne(list[i]);
            }

            ApplyList(c.inventory.inventory);
            ApplyList(c.inventory.accs);

            ApplyToOne(c.inventory.head);
            ApplyToOne(c.inventory.chest);
            ApplyToOne(c.inventory.legs);
            ApplyToOne(c.inventory.boots);
            ApplyToOne(c.inventory.weapon);
            ApplyToOne(c.inventory.trash);
        }
    }
}
