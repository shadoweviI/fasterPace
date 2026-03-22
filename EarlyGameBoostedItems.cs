using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace fasterPace
{

    // Drops selected items as "fully boosted at base" (atk/def + specs)
    // by setting cur = cap * levelMult for each stat/spec.
    [HarmonyPatch(typeof(ItemNameDesc), nameof(ItemNameDesc.addLoot))]
    internal static class Patch_ItemNameDesc_AddLoot_FullBoostSelected
    {
        // Transform/ascend path: places loot directly into a specific inventory slot (sid),
        // so addLoot(...) postfix may never run.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemNameDesc), nameof(ItemNameDesc.makeLoot), new Type[] { typeof(int), typeof(int) })]
        private static void Postfix_MakeLoot_Slot(ItemNameDesc __instance, int id, int sid)
        {
            try
            {
                if (__instance?.character?.inventory == null) return;
                if (!FullBoostOnDrop.Contains(id)) return; // transformed item id is what matters

                // character.inventory.inventory is the backing list/array used by the game
                var invField = AccessTools.Field(__instance.character.inventory.GetType(), "inventory");
                if (invField == null) return;

                var invObj = invField.GetValue(__instance.character.inventory);
                if (invObj is System.Collections.IList list)
                {
                    if (sid < 0 || sid >= list.Count) return;

                    var eq = list[sid] as Equipment;
                    if (eq == null) return;

                    // Same exclusions as your addLoot postfix
                    var t = eq.type;
                    if (t == part.MacGuffin) return;
                    if (t == part.atkBoost || t == part.defBoost || t == part.specBoost) return;

                    ApplyFullBoostAtBase(eq);
                }
            }
            catch { /* keep it silent like the rest of your mod */ }
        }

        // Put all the boss-table / titan IDs you want here
        private static readonly HashSet<int> FullBoostOnDrop = new HashSet<int>
        {
             // Zone 0
            62, 63 , 64, 65,

            // Zone 1
            40, 41, 42, 43, 44, 45, 46, 77,

            // Zone 2
            47, 48, 49, 50, 51, 52, 432,

            // Zone 3
            54, 55, 56, 57, 58, 59, 60, 61, 433,

            // Zone 4
            67, 434,

            // Zone 5
            68, 69, 70, 71, 72, 73, 74, 435,

            // Zone 6 (Titan 1)
            78, 79, 80, 81, 82, 83, 84,

            // Zone 7
            85, 86, 87, 88, 89, 90, 91, 436,

            // Zone 9
            95, 96, 97, 98, 99, 100, 101, 437,

            // Zone 10
            103, 104, 105, 106, 107, 108, 109, 438,
            
            // Zone 11 (Titan 3)
            111, 112, 113, 114, 115, 116, 117,

            // Zone 12
            122, 123, 124, 125, 126, 439,

            // Zone 13
            130, 131, 132, 133, 134, 440,

            // Zone 14 (Titan 4)
            136, 137, 138, 139, 140, 149,

            // Zone 15
            143, 144, 145, 146, 147, 441,

            // Misc
            128, 76, 94, 110, 127, 148, 118

        };

        [HarmonyPostfix]
        private static void Postfix(ItemNameDesc __instance, Equipment loot)
        {
            if (loot == null) return;
            if (!FullBoostOnDrop.Contains(loot.id)) return;

            // Don't touch macguffins or boost items
            var t = loot.type;
            if (t == part.MacGuffin) return;
            if (t == part.atkBoost || t == part.defBoost || t == part.specBoost) return;

            ApplyFullBoostAtBase(loot);
        }

        // -------------------- FULL BOOST (no console spam) --------------------

        private static class EqFields
        {
            private static bool _init;

            public static FieldInfo level;
            public static FieldInfo curAttack, capAttack, curDefense, capDefense;

            // Spec naming style B (your build): spec1Type/spec1Cur/spec1Cap etc.
            public static FieldInfo spec1Type, spec1Cur, spec1Cap;
            public static FieldInfo spec2Type, spec2Cur, spec2Cap;
            public static FieldInfo spec3Type, spec3Cur, spec3Cap;

            // (Optional) also cache style A if you ever run another build
            public static FieldInfo specType1, curSpec1, capSpec1;
            public static FieldInfo specType2, curSpec2, capSpec2;
            public static FieldInfo specType3, curSpec3, capSpec3;

            public static bool HasStyleB =>
                spec1Type != null && spec1Cur != null && spec1Cap != null;

            public static bool HasStyleA =>
                specType1 != null && curSpec1 != null && capSpec1 != null;

            public static void Init()
            {
                if (_init) return;
                _init = true;

                var t = typeof(Equipment);

                // NOTE: AccessTools.Field logs warnings when not found, but this runs only once now.
                level = AccessTools.Field(t, "level");

                curAttack = AccessTools.Field(t, "curAttack");
                capAttack = AccessTools.Field(t, "capAttack");
                curDefense = AccessTools.Field(t, "curDefense");
                capDefense = AccessTools.Field(t, "capDefense");

                // Style B
                spec1Type = AccessTools.Field(t, "spec1Type");
                spec1Cur = AccessTools.Field(t, "spec1Cur");
                spec1Cap = AccessTools.Field(t, "spec1Cap");

                spec2Type = AccessTools.Field(t, "spec2Type");
                spec2Cur = AccessTools.Field(t, "spec2Cur");
                spec2Cap = AccessTools.Field(t, "spec2Cap");

                spec3Type = AccessTools.Field(t, "spec3Type");
                spec3Cur = AccessTools.Field(t, "spec3Cur");
                spec3Cap = AccessTools.Field(t, "spec3Cap");

                // Style A (fallback)
                specType1 = AccessTools.Field(t, "specType1");
                curSpec1 = AccessTools.Field(t, "curSpec1");
                capSpec1 = AccessTools.Field(t, "capSpec1");

                specType2 = AccessTools.Field(t, "specType2");
                curSpec2 = AccessTools.Field(t, "curSpec2");
                capSpec2 = AccessTools.Field(t, "capSpec2");

                specType3 = AccessTools.Field(t, "specType3");
                curSpec3 = AccessTools.Field(t, "curSpec3");
                capSpec3 = AccessTools.Field(t, "capSpec3");
            }
        }

        private static void ApplyFullBoostAtBase(Equipment eq)
        {
            if (eq == null) return;

            EqFields.Init();

            float level = eq.level;
            if (EqFields.level != null)
            {
                object v = EqFields.level.GetValue(eq);
                if (v is float ff) level = ff;
                else if (v is int ii) level = ii;
            }

            float lvlMult = 1f + (level / 100f);

            // Attack/Defense
            ApplyCurCap(eq, EqFields.curAttack, EqFields.capAttack, lvlMult);
            ApplyCurCap(eq, EqFields.curDefense, EqFields.capDefense, lvlMult);

            // Specs: use whichever naming style exists in *this* build
            if (EqFields.HasStyleB)
            {
                ApplySpec(eq, EqFields.spec1Type, EqFields.spec1Cur, EqFields.spec1Cap, lvlMult);
                ApplySpec(eq, EqFields.spec2Type, EqFields.spec2Cur, EqFields.spec2Cap, lvlMult);
                ApplySpec(eq, EqFields.spec3Type, EqFields.spec3Cur, EqFields.spec3Cap, lvlMult);
            }
            else if (EqFields.HasStyleA)
            {
                ApplySpec(eq, EqFields.specType1, EqFields.curSpec1, EqFields.capSpec1, lvlMult);
                ApplySpec(eq, EqFields.specType2, EqFields.curSpec2, EqFields.capSpec2, lvlMult);
                ApplySpec(eq, EqFields.specType3, EqFields.curSpec3, EqFields.capSpec3, lvlMult);
            }
        }

        private static void ApplyCurCap(Equipment eq, FieldInfo curF, FieldInfo capF, float lvlMult)
        {
            if (eq == null || curF == null || capF == null) return;
            if (lvlMult <= 0f) return;

            object capObj = capF.GetValue(eq);
            float cap = 0f;
            if (capObj is float ff) cap = ff;
            else if (capObj is int ii) cap = ii;

            if (cap <= 0f) return;

            float baseVal = cap / lvlMult;           // <-- key change (was cap * lvlMult)
            if (baseVal < 0f) baseVal = 0f;
            if (baseVal > cap) baseVal = cap;

            if (curF.FieldType == typeof(int))
                curF.SetValue(eq, Mathf.FloorToInt(baseVal));
            else
                curF.SetValue(eq, baseVal);
        }

        private static void ApplySpec(Equipment eq, FieldInfo typeF, FieldInfo curF, FieldInfo capF, float lvlMult)
        {
            if (eq == null || typeF == null || curF == null || capF == null) return;
            if (lvlMult <= 0f) return;

            object st = typeF.GetValue(eq);
            if (IsSpecNone(st)) return;

            object capObj = capF.GetValue(eq);
            float cap = 0f;
            if (capObj is float ff) cap = ff;
            else if (capObj is int ii) cap = ii;

            if (cap <= 0f) return;

            float baseVal = cap / lvlMult;           // <-- key change (was cap * lvlMult)
            if (baseVal < 0f) baseVal = 0f;
            if (baseVal > cap) baseVal = cap;

            if (curF.FieldType == typeof(int))
                curF.SetValue(eq, Mathf.FloorToInt(baseVal));
            else
                curF.SetValue(eq, baseVal);
        }


        private static bool IsSpecNone(object specValue)
        {
            if (specValue == null) return true;

            if (specValue.GetType().IsEnum)
                return Convert.ToInt32(specValue) == 0;

            if (specValue is int ii) return ii == 0;
            return false;
        }
    }
}
