using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;

    internal static class ItemBuffs
    {
        // Shared helper: preserve absolute boost delta (cur - cap), not ratio.
        private static void ApplyCapsPreserveBoostDelta(
            Equipment e,
            float newCapAtk, float newCapDef,
            float newCapS1, float newCapS2, float newCapS3)
        {
            if (e == null) return;

            float lvlMult = 1f + (e.level / 100f);

            // Attack
            if (e.capAttack > 0f)
            {
                float oldCap = e.capAttack;
                float oldCur = e.curAttack;
                float oldBoost = oldCur - oldCap;

                e.capAttack = newCapAtk;
                float newMax = newCapAtk * lvlMult;
                float newCur = newCapAtk + oldBoost;

                if (newCur < 0f) newCur = 0f;
                if (newCur > newMax) newCur = newMax;
                e.curAttack = newCur;
            }

            // Defense
            if (e.capDefense > 0f)
            {
                float oldCap = e.capDefense;
                float oldCur = e.curDefense;
                float oldBoost = oldCur - oldCap;

                e.capDefense = newCapDef;
                float newMax = newCapDef * lvlMult;
                float newCur = newCapDef + oldBoost;
            
                if (newCur < 0f) newCur = 0f;
                if (newCur > newMax) newCur = newMax;
                e.curDefense = newCur;
            }

            // Spec 1
            if (e.spec1Cap > 0f)
            {
                float oldCap = e.spec1Cap;
                float oldCur = e.spec1Cur;
                float oldBoost = oldCur - oldCap;

                e.spec1Cap = newCapS1;
                float newMax = newCapS1 * lvlMult;
                float newCur = newCapS1 + oldBoost;

                if (newCur < 0f) newCur = 0f;
                if (newCur > newMax) newCur = newMax;
                e.spec1Cur = newCur;
            }

            // Spec 2
            if (e.spec2Cap > 0f)
            {
                float oldCap = e.spec2Cap;
                float oldCur = e.spec2Cur;
                float oldBoost = oldCur - oldCap;

                e.spec2Cap = newCapS2;
                float newMax = newCapS2 * lvlMult;
                float newCur = newCapS2 + oldBoost;

                if (newCur < 0f) newCur = 0f;
                if (newCur > newMax) newCur = newMax;
                e.spec2Cur = newCur;
            }

            // Spec 3
            if (e.spec3Cap > 0f)
            {
                float oldCap = e.spec3Cap;
                float oldCur = e.spec3Cur;
                float oldBoost = oldCur - oldCap;

                e.spec3Cap = newCapS3;
                float newMax = newCapS3 * lvlMult;
                float newCur = newCapS3 + oldBoost;

                if (newCur < 0f) newCur = 0f;
                if (newCur > newMax) newCur = newMax;
                e.spec3Cur = newCur;
            }
        }

        private static void ApplyToList(List<Equipment> list, int id,
            float capAtk, float capDef, float capS1, float capS2, float capS3)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e == null || e.id != id) continue;
                ApplyCapsPreserveBoostDelta(e, capAtk, capDef, capS1, capS2, capS3);
            }
        }

        private static void ApplyToSlots(Character c, int id,
            float capAtk, float capDef, float capS1, float capS2, float capS3)
        {
            if (c?.inventory == null) return;

            void ApplyOne(Equipment e)
            {
                if (e == null || e.id != id) return;
                ApplyCapsPreserveBoostDelta(e, capAtk, capDef, capS1, capS2, capS3);
            }

            ApplyOne(c.inventory.head);
            ApplyOne(c.inventory.chest);
            ApplyOne(c.inventory.legs);
            ApplyOne(c.inventory.boots);
            ApplyOne(c.inventory.weapon);
            ApplyOne(c.inventory.trash);
        }
    private static void ApplySpecTypes(Equipment e, specType t1, specType t2, specType t3)
    {
        if (e == null) return;
        e.spec1Type = t1;
        e.spec2Type = t2;
        e.spec3Type = t3;
    }

    private static void ApplyTypesToList(List<Equipment> list, int id, specType t1, specType t2, specType t3)
    {
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];
            if (e == null || e.id != id) continue;
            ApplySpecTypes(e, t1, t2, t3);
        }
    }

    private static void ApplyTypesToSlots(Character c, int id, specType t1, specType t2, specType t3)
    {
        if (c?.inventory == null) return;

        void ApplyOne(Equipment e)
        {
            if (e == null || e.id != id) return;
            ApplySpecTypes(e, t1, t2, t3);
        }

        ApplyOne(c.inventory.head);
        ApplyOne(c.inventory.chest);
        ApplyOne(c.inventory.legs);
        ApplyOne(c.inventory.boots);
        ApplyOne(c.inventory.weapon);
        ApplyOne(c.inventory.trash);
    }
    // ─────────────────────────────────────────────
    // Item 226 (Energy Bar Bar) - all specs EnergyPerBar3
    // ─────────────────────────────────────────────
    [HarmonyPatch]
    internal static class Item226Buff
    {
        private const int ID = 226;

        // "change their stats a bit" -> tweak this whenever you want
        private const float MULT = 1.00f;

        // Vanilla from ItemNameDesc:
        // ATK/DEF = 50,000
        // Spec1 = 3,000,000  (was EnergyPower3)
        // Spec2 = 2,500,000  (was EnergyCap3)
        // Spec3 = 2,200,000  (was EnergyPerBar3)
        private const float BASE_ATK = 300000f;
        private const float BASE_DEF = 300000f;
        private const float BASE_SPEC1 = 4400000f;
        private const float BASE_SPEC2 = 4400000f;
        private const float BASE_SPEC3 = 4400000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.EnergyPerBar3;
        private static readonly specType T2 = specType.EnergyPerBar3;
        private static readonly specType T3 = specType.EnergyPerBar3;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;
            var ii = __instance.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
    internal static class Item2310Buff
    {
        private const int ID = 296;

        // "change their stats a bit" -> tweak this whenever you want
        private const float MULT = 1.00f;
        private const float BASE_ATK = 50000000f;
        private const float BASE_DEF = 50000000f;
        private const float BASE_SPEC1 = 50000000f;
        private const float BASE_SPEC2 = 12000000f;
        private const float BASE_SPEC3 = 100000000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.GoldDrop2;
        private static readonly specType T2 = specType.Looting2;
        private static readonly specType T3 = specType.AllPower;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;
            var ii = __instance.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
    internal static class Item230Buff
    {
        private const int ID = 230;

        // "change their stats a bit" -> tweak this whenever you want
        private const float MULT = 1.00f;
        private const float BASE_ATK = 4000000f;
        private const float BASE_DEF = 4000000f;
        private const float BASE_SPEC1 = 10000000f;
        private const float BASE_SPEC2 = 4000000f;
        private const float BASE_SPEC3 = 15000000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.GoldDrop2;
        private static readonly specType T2 = specType.Looting2;
        private static readonly specType T3 = specType.AllPower;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;
            var ii = __instance.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }
    // ─────────────────────────────────────────────
    // Item 272 Violin
    // ─────────────────────────────────────────────
    [HarmonyPatch]
    internal static class Item272Buff
    {
        private const int ID = 272;
        private const float MULT = 1.00f;


        private const float BASE_ATK = 10500000f;
        private const float BASE_DEF = 10500000f;
        private const float BASE_SPEC1 = 26000000f;
        private const float BASE_SPEC2 = 26000000f;
        private const float BASE_SPEC3 = 125000000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.AllPerBar;
        private static readonly specType T2 = specType.Res3Power;
        private static readonly specType T3 = specType.WishSpeed;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;
            var ii = __instance.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }


    // ─────────────────────────────────────────────
    // Item 227 (Magic Bar Bar) - all specs MagicPerBar3
    // ─────────────────────────────────────────────
    [HarmonyPatch]
    internal static class Item227Buff
    {
        private const int ID = 227;

        // "change their stats a bit" -> tweak this whenever you want
        private const float MULT = 1.00f;

        // Vanilla from ItemNameDesc:
        // ATK/DEF = 50,000
        // Spec1 = 3,000,000  (was MagicPower3)
        // Spec2 = 2,500,000  (was MagicCap3)
        // Spec3 = 2,200,000  (was MagicPerBar3)
        private const float BASE_ATK = 300000f;
        private const float BASE_DEF = 300000f;
        private const float BASE_SPEC1 = 4400000f;
        private const float BASE_SPEC2 = 4400000f;
        private const float BASE_SPEC3 = 4400000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.MagicPerBar3;
        private static readonly specType T2 = specType.MagicPerBar3;
        private static readonly specType T3 = specType.MagicPerBar3;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;
            var ii = __instance.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }


    // ─────────────────────────────────────────────
    // Item 220
    // ─────────────────────────────────────────────
    // ─────────────────────────────────────────────
    // Item 162 (Brown Heart) - add Yggdrasil Yield on Spec 3
    // SpecType.Yggdrasil is displayed as "Yggdrasil Yield" and uses amount/100000.
    // 25% => 0.25 => raw amount 25000.
    // Starts at 0 (cur = 0).
    // ─────────────────────────────────────────────
    [HarmonyPatch]
    internal static class Item162BrownHeart_YggYieldSpec3
    {
        private const int ID = 162;

        // 25% yield = 0.25f. InventoryController converts: bonus = amount / 100000f
        private const float RAW_CAP = 1250000f;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Change ONLY spec3 to be Yggdrasil Yield, and start it at 0.
            ii.specType3[ID] = specType.Yggdrasil;
            ii.capSpec3[ID] = RAW_CAP;
            ii.curSpec3[ID] = 0f;
        }

        private static void ApplyToOne(Equipment e)
        {
            if (e == null || e.id != ID) return;

            // Force spec3 to the new type/cap.
            e.spec3Type = specType.Yggdrasil;
            e.spec3Cap = RAW_CAP;

            // Respect item level max scaling (same style as your cap helpers)
            float lvlMult = 1f + (e.level / 100f);
            float max = RAW_CAP * lvlMult;

            // Starts at 0 (and stays clamped if something weird happens)
            if (e.spec3Cur < 0f) e.spec3Cur = 0f;
            if (e.spec3Cur > max) e.spec3Cur = max;
        }

        private static void ApplyToList(List<Equipment> list)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e == null || e.id != ID) continue;
                ApplyToOne(e);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character?.inventory == null) return;

            // Brown Heart is an accessory, but it can exist in multiple lists.
            ApplyToList(character.inventory.inventory);
            ApplyToList(character.inventory.accs);

            // If your build ever stores equipped accessories elsewhere, this is harmless.
            ApplyToOne(character.inventory.head);
            ApplyToOne(character.inventory.chest);
            ApplyToOne(character.inventory.legs);
            ApplyToOne(character.inventory.boots);
            ApplyToOne(character.inventory.weapon);
            ApplyToOne(character.inventory.trash);
        }
    }

    // ─────────────────────────────────────────────
    // Item 160 (Fanny Pack) - x2 stats + Spec3 becomes Gold Drops
    // Vanilla:
    //   atk/def: 15000
    //   spec1 (AllPower): 500000
    //   spec2 (AllCap):   300000
    //   spec3 (Looting):  300
    // Change:
    //   x2 caps for atk/def/spec1/spec2
    //   spec3 -> GoldDrop2 (Gold Drops), cap = 600 (also x2 from 300)
    // ─────────────────────────────────────────────
    [HarmonyPatch]
    internal static class Item160FannyPack_BuffAndGoldDropSpec3
    {
        private const int ID = 160;
        private const float MULT = 2f;

        private const float BASE_ATK = 15000f;
        private const float BASE_DEF = 15000f;
        private const float BASE_SPEC1 = 500000f;
        private const float BASE_SPEC2 = 300000f;

        // We’re replacing Looting(300) with GoldDrop2.
        // Keeping “x2 from 300” => 600. (GoldDrop2 uses amount/1000 in vanilla)
        private const float BASE_SPEC3_OLD = 300000f;
        private const float NEW_SPEC3_CAP = BASE_SPEC3_OLD * MULT;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => NEW_SPEC3_CAP;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;
            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // 1) Template edits (future drops)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;

            // Replace Spec3 type + cap
            ii.specType3[ID] = specType.GoldDrop2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }

        private static void ApplyGoldSpec3ToOne(Equipment e)
        {
            if (e == null || e.id != ID) return;

            // Force spec3 to the new type
            e.spec3Type = specType.GoldDrop2;

            // Clamp spec3Cur to new max (respect item level scaling)
            float lvlMult = 1f + (e.level / 100f);
            float max = T_S3 * lvlMult;
            if (e.spec3Cur < 0f) e.spec3Cur = 0f;
            if (e.spec3Cur > max) e.spec3Cur = max;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character?.inventory == null) return;

            // 2) Update existing items (bag + equipped) while preserving boost deltas
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec3 type is correct everywhere
            // (ApplyToList/Slots only adjusts caps; it doesn’t change spec types)
            ApplyGoldSpec3ToOne(character.inventory.head);
            ApplyGoldSpec3ToOne(character.inventory.chest);
            ApplyGoldSpec3ToOne(character.inventory.legs);
            ApplyGoldSpec3ToOne(character.inventory.boots);
            ApplyGoldSpec3ToOne(character.inventory.weapon);
            ApplyGoldSpec3ToOne(character.inventory.trash);

            // bag lists
            var bag = character.inventory.inventory;
            if (bag != null)
                for (int i = 0; i < bag.Count; i++) ApplyGoldSpec3ToOne(bag[i]);

            var accs = character.inventory.accs;
            if (accs != null)
                for (int i = 0; i < accs.Count; i++) ApplyGoldSpec3ToOne(accs[i]);
        }
    }

    // ─────────────────────────────────────────────
    // Item 244 - Spec types: AllPerBar / AllCap / AllPower
    // ─────────────────────────────────────────────
    [HarmonyPatch]
    internal static class Item244Buff
    {
        private const int ID = 244;

        // You will tweak these
        private const float MULT = 1.00f;

        private const float BASE_ATK = 1000000f;
        private const float BASE_DEF = 1000000f;
        private const float BASE_SPEC1 = 10000000f;
        private const float BASE_SPEC2 = 10000000f;
        private const float BASE_SPEC3 = 10000000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.AllPower;
        private static readonly specType T2 = specType.AllPerBar;
        private static readonly specType T3 = specType.AllCap;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }


    // ─────────────────────────────────────────────
    // Item 245 - Spec types: Res3Power / Res3Cap / HackSpeed
    // ─────────────────────────────────────────────
    [HarmonyPatch]
    internal static class Item245Buff
    {
        private const int ID = 245;

        // You will tweak these
        private const float MULT = 1.00f;

        private const float BASE_ATK = 400000f;
        private const float BASE_DEF = 400000f;
        private const float BASE_SPEC1 = 40000000f;
        private const float BASE_SPEC2 = 40000000f;
        private const float BASE_SPEC3 = 40000000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.Res3Power;
        private static readonly specType T2 = specType.Res3Cap;
        private static readonly specType T3 = specType.Res3Bar;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    // ─────────────────────────────────────────────
    // Item 245 - Spec types: Res3Power / Res3Cap / HackSpeed
    // ─────────────────────────────────────────────
    [HarmonyPatch]
    internal static class Item295Buff
    {
        private const int ID = 295;

        // You will tweak these
        private const float MULT = 1.00f;

        private const float BASE_ATK = 30000000f;
        private const float BASE_DEF = 30000000f;
        private const float BASE_SPEC1 = 500000000f;
        private const float BASE_SPEC2 = 100000000f;
        private const float BASE_SPEC3 = 100000000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.Res3Cap;
        private static readonly specType T2 = specType.Res3Power;
        private static readonly specType T3 = specType.Res3Bar;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    // ─────────────────────────────────────────────
    // Item 245 - Spec types: Res3Power / Res3Cap / HackSpeed
    // ─────────────────────────────────────────────
    [HarmonyPatch]
    internal static class Item276Buff
    {
        private const int ID = 276;

        // You will tweak these
        private const float MULT = 1.00f;

        private const float BASE_ATK = 100000000f;
        private const float BASE_DEF = 100000000f;
        private const float BASE_SPEC1 = 100000000f;
        private const float BASE_SPEC2 = 100000000f;
        private const float BASE_SPEC3 = 100000000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.Res3Cap;
        private static readonly specType T2 = specType.Res3Power;
        private static readonly specType T3 = specType.Res3Bar;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
    internal static class Item307Buff
    {
        private const int ID = 307;

        // You will tweak these
        private const float MULT = 1.00f;

            private const float BASE_ATK = 6666666f;
            private const float BASE_DEF = 6666666f;
            private const float BASE_SPEC1 = 50000000f;
            private const float BASE_SPEC2 = 50000000f;
            private const float BASE_SPEC3 = 50000000f;

            private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

            private static readonly specType T1 = specType.Res3Cap;
            private static readonly specType T2 = specType.Res3Power;
            private static readonly specType T3 = specType.HackSpeed;

            private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    // Pirate set buff

    [HarmonyPatch]
    internal static class Item507Buff
    {
        private const int ID = 507;

        // You will tweak these
        private const float MULT = 2.00f;

        private const float BASE_ATK = 2.4E+08f;
        private const float BASE_DEF = 5.6E+09f;
        private const float BASE_SPEC1 = 2.8E+09f;
        private const float BASE_SPEC2 = 1.79E+09f;
        private const float BASE_SPEC3 = 2000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.AllCap;
        private static readonly specType T2 = specType.Res3Cap;
        private static readonly specType T3 = specType.Respawn;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
    internal static class Item508Buff
    {
        private const int ID = 508;

        // You will tweak these
        private const float MULT = 2.00f;

        private const float BASE_ATK = 2.5E+08f;
        private const float BASE_DEF = 5.7E+09f;
        private const float BASE_SPEC1 = 1.76E+09f;
        private const float BASE_SPEC2 = 1.79E+09f;
        private const float BASE_SPEC3 = 1000000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.Res3Power;
        private static readonly specType T2 = specType.Res3Bar;
        private static readonly specType T3 = specType.Yggdrasil;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
    internal static class Item509Buff
    {
        private const int ID = 509;

        // You will tweak these
        private const float MULT = 2.00f;

        private const float BASE_ATK = 2.4E+08f;
        private const float BASE_DEF = 5.5E+09f;
        private const float BASE_SPEC1 = 1.76E+09f;
        private const float BASE_SPEC2 = 1.82E+09f;
        private const float BASE_SPEC3 = 1.8E+09f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.AllPower;
        private static readonly specType T2 = specType.Res3Cap;
        private static readonly specType T3 = specType.Res3Power;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
    internal static class Item510Buff
    {
        private const int ID = 510;

        // You will tweak these
        private const float MULT = 2.00f;

        private const float BASE_ATK = 2.4E+08f;
        private const float BASE_DEF = 5.5E+09f;
        private const float BASE_SPEC1 = 1.82E+09f;
        private const float BASE_SPEC2 = 1.77E+09f;
        private const float BASE_SPEC3 = 1.81E+09f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.AllPerBar;
        private static readonly specType T2 = specType.Res3Cap;
        private static readonly specType T3 = specType.Res3Power;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
    internal static class Item511Buff
    {
        private const int ID = 511;

        // You will tweak these
        private const float MULT = 2.00f;

        private const float BASE_ATK = 6.3E+10f;
        private const float BASE_DEF = 1E+09f;
        private const float BASE_SPEC1 = 1.79E+09f;
        private const float BASE_SPEC2 = 1.82E+09f;
        private const float BASE_SPEC3 = 1.83E+09f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.AllCap;
        private static readonly specType T2 = specType.AllPower;
        private static readonly specType T3 = specType.WishSpeed;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
    internal static class Item512Buff
    {
        private const int ID = 512;

        // You will tweak these
        private const float MULT = 2.00f;

        private const float BASE_ATK = 5.9E+09f;
        private const float BASE_DEF = 5.9E+09f;
        private const float BASE_SPEC1 = 1.82E+09f;
        private const float BASE_SPEC2 = 1.3E+08f;
        private const float BASE_SPEC3 = 1.82E+09f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.Res3Bar;
        private static readonly specType T2 = specType.NGU2;
        private static readonly specType T3 = specType.Res3Power;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
    internal static class Item513Buff
    {
        private const int ID = 513;

        // You will tweak these
        private const float MULT = 2.00f;

        private const float BASE_ATK = 5.9E+09f;
        private const float BASE_DEF = 5.9E+09f;
        private const float BASE_SPEC1 = 1000f;
        private const float BASE_SPEC2 = 1.82E+09f;
        private const float BASE_SPEC3 = 1.82E+09f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.Respawn;
        private static readonly specType T2 = specType.Res3Power;
        private static readonly specType T3 = specType.WishSpeed;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
    internal static class Item514Buff
    {
        private const int ID = 514;

        // You will tweak these
        private const float MULT = 2.00f;

        private const float BASE_ATK = 6.9E+10f;
        private const float BASE_DEF = 1E+09f;
        private const float BASE_SPEC1 = 1.79E+09f;
        private const float BASE_SPEC2 = 1.79E+09f;
        private const float BASE_SPEC3 = 1.79E+09f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.AllPerBar;
        private static readonly specType T2 = specType.AllPower;
        private static readonly specType T3 = specType.AllCap;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
    internal static class Item277Buff
    {
        private const int ID = 277;

        // You will tweak these
        private const float MULT = 1.00f;

        private const float BASE_ATK = 0f;
        private const float BASE_DEF = 0f;
        private const float BASE_SPEC1 = 100000000f;
        private const float BASE_SPEC2 = 100000000f;
        private const float BASE_SPEC3 = 100000000f;

        private static float T_ATK => BASE_ATK * MULT;
        private static float T_DEF => BASE_DEF * MULT;
        private static float T_S1 => BASE_SPEC1 * MULT;
        private static float T_S2 => BASE_SPEC2 * MULT;
        private static float T_S3 => BASE_SPEC3 * MULT;

        private static readonly specType T1 = specType.AllPerBar;
        private static readonly specType T2 = specType.AllPower;
        private static readonly specType T3 = specType.AllCap;

        private static Character character;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_Postfix(Character __instance)
        {
            character = __instance;

            var ii = __instance?.itemInfo;
            if (ii == null) return;

            // Force spec types in the item table (affects newly created loot, dummies, etc.)
            ii.specType1[ID] = T1;
            ii.specType2[ID] = T2;
            ii.specType3[ID] = T3;

            // Caps stored WITHOUT level multiplier (game applies level multiplier later)
            ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
            ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

            ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
            ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
            ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
        }

        

            [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
        private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
        {
            if (character == null) return;

            // Update existing item instances (already owned/equipped)
            ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);

            // Ensure spec types are correct everywhere (bags + equipped)
            ApplyTypesToList(character.inventory.inventory, ID, T1, T2, T3);
            ApplyTypesToList(character.inventory.accs, ID, T1, T2, T3);
            ApplyTypesToSlots(character, ID, T1, T2, T3);
        }
    }

    [HarmonyPatch]
        internal static class Item220Buff
        {
            private const int ID = 220;

            private const float MULT = 2.00f;

            private const float BASE_ATK = 60000f;
            private const float BASE_DEF = 540000f;
            private const float BASE_SPEC1 = 4200000f;
            private const float BASE_SPEC2 = 4200000f;
            private const float BASE_SPEC3 = 4200000f;

            private static float T_ATK => BASE_ATK * MULT;
            private static float T_DEF => BASE_DEF * MULT;
            private static float T_S1 => BASE_SPEC1 * MULT;
            private static float T_S2 => BASE_SPEC2 * MULT;
            private static float T_S3 => BASE_SPEC3 * MULT;

            private static Character character;

            [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
            private static void Character_Start_Postfix(Character __instance)
            {
                character = __instance;

                var ii = __instance.itemInfo;
                if (ii == null) return;

                // IMPORTANT: caps stored WITHOUT level multiplier (game applies level multiplier later)
                ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
                ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

                ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
                ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
                ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
            private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
            {
                if (character == null) return;

                ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
                ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
                ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            }
        }

        // ─────────────────────────────────────────────
        // Item 229
        // ─────────────────────────────────────────────
        [HarmonyPatch]
        internal static class Item229Buff
        {
            private const int ID = 229;
            private const float MULT = 1.0f;

            private const float BASE_ATK = 3000000f;
            private const float BASE_DEF = 3000000f;
            private const float BASE_SPEC1 = 4000000f;
            private const float BASE_SPEC2 = 10000000f;
            private const float BASE_SPEC3 = 10000000f;

            private static float T_ATK => BASE_ATK * MULT;
            private static float T_DEF => BASE_DEF * MULT;
            private static float T_S1 => BASE_SPEC1 * MULT;
            private static float T_S2 => BASE_SPEC2 * MULT;
            private static float T_S3 => BASE_SPEC3 * MULT;

            private static Character character;

            [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
            private static void Character_Start_Postfix(Character __instance)
            {
                character = __instance;

                var ii = __instance.itemInfo;
                if (ii == null) return;

                ii.curAttack[ID] = ii.capAttack[ID] = T_ATK;
                ii.curDefense[ID] = ii.capDefense[ID] = T_DEF;

                ii.curSpec1[ID] = ii.capSpec1[ID] = T_S1;
                ii.curSpec2[ID] = ii.capSpec2[ID] = T_S2;
                ii.curSpec3[ID] = ii.capSpec3[ID] = T_S3;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateItemStats")]
            private static void InventoryController_updateItemStats_Postfix(InventoryController __instance)
            {
                if (character == null) return;

                ApplyToList(character.inventory.inventory, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
                ApplyToList(character.inventory.accs, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
                ApplyToSlots(character, ID, T_ATK, T_DEF, T_S1, T_S2, T_S3);
            }
        }
    }

