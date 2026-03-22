using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore;


namespace fasterPace
{
    [HarmonyPatch]
    internal class GeneralBuffs
    {
        internal const float GenSpeed = 3f;
        internal const float EXP_AP = 2f;
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "questRewardFactor")]
        private static void questRewardFactor(ref float __result)
        {
            __result *= GenSpeed * 2;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(BeastQuestController), "questDropChance")]
        private static bool questDropChance(BeastQuestController __instance, ref float __result)
        {
            __result = 0.05f * __instance.questDropBonuses() * EXP_AP;
            return false;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "totalMayoSpeed")]
        private static void totalMayoSpeed(ref float __result)
        {
            __result *= GenSpeed * 2;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(Character), "lootFactor")]
        private static void lootFactor(ref float __result)
        {
            __result *= GenSpeed * 2;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "totalCardSpeed")]
        private static void totalCardSpeed(ref float __result)
        {
            __result *=  GenSpeed * 2;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "totalWishSpeedBonuses")]
        private static void totalWishSpeedBonuses(ref float __result)
        {
            __result *= GenSpeed * 2;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(HacksController), "totalHackSpeedBonus")]
        private static void totalHackSpeedBonus(ref float __result)
        {
            __result *= GenSpeed * 2;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(AllBeardsController), "energyBeardSpeedFactor")]
        private static void energyBeardSpeedFactor(ref float __result)
        {
            __result *= GenSpeed;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(AllBeardsController), "magicBeardSpeedFactor")]
        private static void magicBeardSpeedFactor(ref float __result)
        {
            __result *= GenSpeed;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addExp", [typeof(float)])]
        private static void addExp(ref float exp)
        {
            exp *= GenSpeed * 2;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addExp", [typeof(long)])]
        private static void addExp(ref long rexp)
        {
            rexp *= (long)GenSpeed * 2;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "checkExpAdded", [typeof(long)])]
        private static void checkExpAdded(ref long rexp)
        {
            rexp *= (long)GenSpeed * 2;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addAP", [typeof(int)])]
        private static void addAP(ref int amount)
        {
            amount *= (int)EXP_AP * 2;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addAP", [typeof(long)])]
        private static void addAP(ref long amount)
        {
            amount *= (long)EXP_AP * 2;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "checkAPAdded", [typeof(long)])]
        private static void checkAPAdded(ref long amount)
        {
            amount *= (long)EXP_AP * 2;
        }
        internal static class GainScaleUtil
        {
            public static long ScaleLongGain(long baseGain, double mult, long hardCapPowBar)
            {
                if (mult <= 1.0) return baseGain;
                if (baseGain <= 0) return baseGain;

                // scale with double, then clamp
                double scaled = Math.Floor(baseGain * mult);

                if (scaled > hardCapPowBar) return hardCapPowBar;
                if (scaled > long.MaxValue) return long.MaxValue; // extra safety
                if (scaled < 1.0) return 1L;

                return (long)scaled;
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.totalEnergyBar))]
        internal static class Patch_TotalEnergyBar_GenSpeed
        {
            [HarmonyPostfix]
            private static void Postfix(Character __instance, ref long __result)
            {
                if (__instance == null) return;
                long cap = __instance.hardCapPowBar();
                __result = GainScaleUtil.ScaleLongGain(__result, GenSpeed, cap);
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.totalMagicBar))]
        internal static class Patch_TotalMagicBar_GenSpeed
        {
            [HarmonyPostfix]
            private static void Postfix(Character __instance, ref long __result)
            {
                if (__instance == null) return;
                long cap = __instance.hardCapPowBar();
                __result = GainScaleUtil.ScaleLongGain(__result, GenSpeed, cap);
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.totalRes3Bar))]
        internal static class Patch_TotalRes3Bar_GenSpeed
        {
            [HarmonyPostfix]
            private static void Postfix(Character __instance, ref long __result)
            {
                if (__instance == null) return;
                long cap = __instance.hardCapPowBar();
                __result = GainScaleUtil.ScaleLongGain(__result, GenSpeed, cap);
            }
        }
        static float GetBaseRespawn(AdventureController a)
        {
            var t = a.GetType();

            var f = AccessTools.Field(t, "baseRespawn")
                 ?? AccessTools.Field(t, "baseRespawnTime")
                 ?? AccessTools.Field(t, "_baseRespawn");
            if (f != null) return (float)f.GetValue(a);

            var p = AccessTools.Property(t, "baseRespawn")
                 ?? AccessTools.Property(t, "baseRespawnTime");
            if (p != null) return (float)p.GetValue(a, null);

            if (a.character?.adventure != null)
            {
                var at = a.character.adventure.GetType();
                var f2 = AccessTools.Field(at, "baseRespawn") ?? AccessTools.Field(at, "baseRespawnTime");
                if (f2 != null) return (float)f2.GetValue(a.character.adventure);
            }

            return 1f;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "respawnTime")]
        private static bool respawnTime(AdventureController __instance, ref float __result)
        {
            float num = GetBaseRespawn(__instance) * character.NGUController.respawnBonus();
            float num2 = 1f - character.inventoryController.bonuses[specType.Respawn];
            float num3 = 2f;
            if ((double)num2 < 0.2)
            {
                num2 = 0.2f;
            }

            num *= num2;
            if (character.inventory.itemList.clockComplete)
            {
                num *= 0.95f;
            }

            if (character.adventure.itopod.perkLevel[93] >= 1)
            {
                num *= character.adventureController.itopod.totalRespawnBonus();
            }

            __result = num * character.wishesController.totalRespawnBonus() / num3;
            return false;
        }

        [HarmonyPatch(typeof(AdventureController), "respawnBonus")]
        internal static class Patch_AdventureController_RespawnBonus_HalveForUI
        {
            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(ref float __result)
            {
                __result /= 2f;
            }
        }

       
        [HarmonyPrefix, HarmonyPatch(typeof(FruitController), "seedReward")]
        private static bool seedReward(FruitController __instance, ref long __result, int fruitID, int tier, float poopBonus)
        {
            float reward = (float)(character.yggdrasilController.baseSeedReward[fruitID] * tier)
                           * poopBonus
                           * character.adventureController.itopod.totalSeedBonus()
                           * character.beastQuestPerkController.totalSeedBonus()
                           * (1f + character.inventoryController.bonuses[specType.Seeds])
                           * character.NGUController.yggdrasilBonus()
                           * character.adventureController.itopod.totalHarvestBonus(fruitID);
            reward *= (GenSpeed * 2);
            __result = (long)Mathf.Ceil(reward);
            return false; 
        }


        [HarmonyPrefix, HarmonyPatch(typeof(FruitController), "harvestSeedReward")]
        private static bool harvestSeedReward(FruitController __instance, ref long __result, int fruitID, int tier, float poopBonus)
        {
            float reward = (float)(character.yggdrasilController.baseSeedReward[fruitID] * tier * EXP_AP)
                           * poopBonus
                           * character.adventureController.itopod.totalSeedBonus()
                           * character.beastQuestPerkController.totalSeedBonus()
                           * (1f + character.inventoryController.bonuses[specType.Seeds])
                           * character.NGUController.yggdrasilBonus()
                           * character.adventureController.itopod.totalHarvestBonus(fruitID);
            reward *= (GenSpeed * 2);
            __result = (long)Mathf.Ceil(reward);
            return false;
        }

        [HarmonyPatch(typeof(ArbitraryController))]
        internal static class Patch_LootFilter_Purchase
        {
            private const long COST = 50000L;

            [HarmonyPrefix, HarmonyPatch("lootFilterCost")]
            private static bool lootFilterCost(ref long __result)
            {
                __result = COST;
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("startLootFilterAP")]
            private static bool startLootFilterAP(ArbitraryController __instance)
            {
                var c = __instance?.character;
                if (c?.arbitrary == null) return true;

                if (c.arbitrary.lootFilter)
                {
                    __instance.tooltip.showTooltip(
                        "Are you so eager to throw away all your AP? You already bought the improved Loot Filter!", 2f);
                    return false;
                }

                if (c.arbitrary.curArbitraryPoints < COST)
                {
                    __instance.tooltip.showTooltip(
                        "You don't have enough AP to buy the improved Loot Filter! Though to be fair, it costs a fair bit.", 3f);
                    return false;
                }

                // Buy immediately (no confirmation UI)
                __instance.buyLootFilterAP();
                return false;
            }
        }

        [HarmonyPatch(typeof(CardsController), nameof(CardsController.maxDeckSize))]
        internal static class Patch_CardsController_MaxDeckSize_Base25
        {
            [HarmonyPrefix]
            private static bool Prefix(CardsController __instance, ref int __result)
            {
                var character = __instance?.character;
                if (character == null)
                {
                    __result = 25;
                    return false;
                }

                int num = 25; // <-- changed from 10 to 25

                num += character.adventureController.itopod.totalDeckSizeBonus();
                num += character.beastQuestPerkController.totalDeckSizeBonus();
                num += character.wishesController.totalDeckSizeBonus();

                if (character.inventory.itemList.radComplete)
                    num += 5;

                if (character.inventory.itemList.amalgamateComplete)
                    num += 10;

                __result = num + character.arbitrary.deckSpaceBought;
                return false; // skip vanilla
            }
        }



        [HarmonyPrefix, HarmonyPatch(typeof(AllItemListController), "boostBonus")]
        private static bool boostBonus(ref float __result)
        {
            int num = 0;
            float num2 = 1f;
            for (int i = 0; i <= 39; i++)
            {
                if (character.inventory.itemList.itemMaxxed[i])
                {
                    num++;
                }
            }

            num2 += 0.02f * (float)num;
            if (character.inventory.itemList.badlyDrawnComplete)
            {
                num2 *= 1.2f;
            }

            if (character.inventory.itemList.constructionComplete)
            {
                num2 *= 1.2f;
            }

            num2 *= character.adventureController.itopod.totalBoostBonus();
            __result = num2 * character.beastQuestPerkController.totalBoostBonus() * (GenSpeed * 2);
            return false;
        }
    }

    [HarmonyPatch(typeof(FruitController), nameof(FruitController.consumePPFruit))]
    internal static class Patch_ConsumePPFruit_BuffPPGain
    {
        [HarmonyPrefix]
        private static bool Prefix(FruitController __instance)
        {
            var character = __instance.character;
            if (character == null)
                return false;

            if (9 < Math.Min(character.yggdrasil.fruits.Count, __instance.character.yggdrasilController.activationCost.Count))
            {
                int num = __instance.tierFactor(__instance.harvestTier(9));
                float num2 = __instance.usePoop(9);
                long num3 = __instance.seedReward(9, num, num2);

                long num4 = (long)Mathf.Ceil(
                    (float)(60000 * num) *
                    num2 *
                    character.yggdrasilYieldBonus() *
                    character.adventureController.itopod.totalPPBonus(usePills: false) *
                    character.adventureController.itopod.totalHarvestBonus(9));

                // Your buff: multiply PP progress by (GenSpeed * 2)
                num4 = (long)Mathf.Ceil(num4 * (GeneralBuffs.GenSpeed * 2f));

                character.yggdrasil.seeds += num3;

                string text =
                    "You eat the fruit and gain " +
                    character.display(character.adventureController.itopod.progressToPP(num4)) +
                    " Perk Points and " +
                    character.display(character.adventureController.itopod.progressToRemainder(num4)) +
                    " Progress to your next PP! You also gained " +
                    character.display(num3) +
                    " Seeds!";

                __instance.tooltip.showOverrideTooltip(text, 2f);
                character.adventureController.itopod.addProgress(num4);
                character.yggdrasil.fruits[9].deactivate();
                character.yggdrasil.fruits[9].harvests++;
                __instance.updateFruitDisplay();
                __instance.updateFruitSlider();
            }

            return false; // skip original
        }
    }
    [HarmonyPatch(typeof(AdventureController))]
    internal static class Patch_BossPP_Buff
    {
        private static readonly float MULT = GeneralBuffs.GenSpeed * 2f;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(AdventureController.boss6PP))]
        [HarmonyPatch(nameof(AdventureController.boss7PP))]
        [HarmonyPatch(nameof(AdventureController.boss8PP))]
        [HarmonyPatch(nameof(AdventureController.boss9PP))]
        [HarmonyPatch(nameof(AdventureController.boss10PP))]
        [HarmonyPatch(nameof(AdventureController.boss11PP))]
        [HarmonyPatch(nameof(AdventureController.boss12PP))]
        private static void Postfix(ref long __result)
        {
            __result = (long)(__result * MULT);
        }
    }
}
