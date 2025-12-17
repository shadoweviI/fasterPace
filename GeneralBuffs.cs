using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.TextCore;

namespace fasterPace
{
    [HarmonyPatch]
    internal class GeneralBuffs
    {
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "awardHighestLevelPP")]
        private static bool awardHighestLevelPP(ItopodPerkController __instance,int level)
        {
            if (level % 10 != 0)
                return false;

            int num = level / 10;

            int perkPoints = Mathf.CeilToInt(num / 10f);
            if (num % 10 == 0)
                perkPoints *= 10;
            if (perkPoints < 10)
                perkPoints = 10;

            const long PROGRESS_PER_PERK = 300000;
            double bonus = __instance.totalPPBonus();
            long add = (long)Math.Ceiling(perkPoints * (double)PROGRESS_PER_PERK * bonus);

            __instance.addProgress(add);

            character.adventureController.log.AddEvent($"You Reached floor {level} of the I.T.O.P.O.D for the first time!");
            character.adventureController.log.AddEvent(
                $"You've been awarded a one-time bonus of {character.display(add)} progress to your next Perk!");

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "poopThreshold")]
        private static bool poopThreshold(ItopodPerkController __instance, ref long __result)
        {
            __result = 1800L;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "progressGained")]
        private static bool progressGained(long itopodLevel, ItopodPerkController __instance, ref long __result)
        {
            long result = 0L;
            if (character.settings.rebirthDifficulty == difficulty.normal)
            {
                result = (long)((10000f + (float)itopodLevel) * (float)__instance.totalPPBonus());
            }
            else if (character.settings.rebirthDifficulty == difficulty.evil)
            {
                result = (long)((20000f + (float)itopodLevel) * (float)__instance.totalPPBonus());
            }
            else if (character.settings.rebirthDifficulty == difficulty.sadistic)
            {
                result = (long)((40000f + (float)itopodLevel + (float)__instance.totalBasePPBonus()) * (float)__instance.totalPPBonus());
            }

            __result = result;
            return false;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "baseProgressGained")]
        private static bool baseProgressGained(long itopodLevel, ItopodPerkController __instance, ref long __result)
        {
            long result = 0L;
            if (__instance.character.settings.rebirthDifficulty == difficulty.normal)
            {
                result = (long)(10000f + (float)itopodLevel);
            }
            else if (__instance.character.settings.rebirthDifficulty == difficulty.evil)
            {
                result = (long)(20000f + (float)itopodLevel);
            }
            else if (__instance.character.settings.rebirthDifficulty == difficulty.sadistic)
            {
                result = (long)(40000f + (float)itopodLevel + (float)__instance.totalBasePPBonus());
            }

            __result = result;
            return false;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "questRewardFactor")]
        private static void questRewardFactor(ref float __result)
        {
            __result *= 10f;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BeastQuestController), "questDropChance")]
        private static bool questDropChance(BeastQuestController __instance, ref float __result)
        {
            __result = 0.05f * __instance.questDropBonuses() * 5;
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllDaycareController), "daycareTime")]
        private static void daycareTime(ref float __result)
        {
            __result *= 0.1f;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "totalMayoSpeed")]
        private static void totalMayoSpeed(ref float __result)
        {
            __result *= 10f;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(Character), "lootFactor")]
        private static void lootFactor(ref float __result)
        {
            __result *= 5f;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "totalCardSpeed")]
        private static void totalCardSpeed(ref float __result)
        {
            __result *= 10f;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "totalWishSpeedBonuses")]
        private static void totalWishSpeedBonuses(ref float __result)
        {
            __result *= 10f;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(HacksController), "totalHackSpeedBonus")]
        private static void totalHackSpeedBonus(ref float __result)
        {
            __result *= 10f;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "minimumWishTime")]
        private static bool minimumWishTime(WishesController __instance,ref float __result)
        {
            float num = 600f;
            num -= character.adventureController.itopod.totalWishMinReduction();
            num -= character.beastQuestPerkController.totalWishMinReduction();
            __result = 1f / (num * 50f);
            return false;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Rebirth), "minRebirthTime")]
        private static bool minRebirthTime(Rebirth __instance, ref int __result)
        {
            int num = 31;
            num -= character.wishes.wishes[20].level * 5;
            if (num < 1)
            {
                num = 1;
            }

            if (num > 31)
            {
                num = 31;
            }
            __result = num;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NGUController), "sadisticDivider"), HarmonyPatch(typeof(NGUMagicController), "sadisticDivider")]
        private static bool sadisticDivider(ref float __result)
        {
            __result = 1E+06f;
            return false;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Wandoos98Controller), "wandoosBootupTime")]
        private static bool wandoosBootupTime(ref float __result)
        {
            float num = 540f;
            if (character.inventory.itemList.xlComplete)
            {
                num = 480f;
            }

            float num2 = 1f - (float)character.allChallenges.level100Challenge.evilCompletions() * 0.1f;
            if (num2 < 0.5f)
            {
                num2 = 0.5f;
            }

            if (num2 > 1f)
            {
                num2 = 1f;
            }

            __result = num * num2;
            return false;
        }
        private static void addEXP(ref long __result)
        {
            __result *= 10;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addExp", [typeof(float)])]
        private static void addExp(ref float exp)
        {
            exp *= 10;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addExp", [typeof(long)])]
        private static void addExp(ref long rexp)
        {
            rexp *= 10;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "checkExpAdded", [typeof(long)])]
        private static void checkExpAdded(ref long rexp)
        {
            rexp *= 10;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addAP", [typeof(int)])]
        private static void addAP(ref int amount)
        {
            amount *= 3;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addAP", [typeof(long)])]
        private static void addAP(ref long amount)
        {
            amount *= 3;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "checkAPAdded", [typeof(long)])]
        private static void checkAPAdded(ref long amount)
        {
            amount *= 3;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start"), HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Character(Character __instance)
        {
            for (int i = 0; i <= 5; i++)
            {
                __instance.training._attackCaps[i] = 1;
                __instance.training._defenseCaps[i] = 1;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(FruitController), "tierThreshold")]
        private static bool tierThreshold(FruitController __instance, ref float __result)
        {
            __result = 720f - Mathf.Min(character.beastQuest.quirkLevel[13] * 60, 180f);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllBeardsController), "timeFactor")]
        private static bool timeFactor(AllBeardsController __instance, ref double __result)
        {
            // Apply global timer acceleration (same idea as macguffins/ygg): treat time as 5x
            double t = character.rebirthTime.totalseconds * 5.0;

            if (t < 3600.0)
            {
                __result = 0.0;
                return false;
            }

            double num = t / 10800.0 * 24.0 / (double)(24 - character.adventure.itopod.perkLevel[21]);

            if (num > 8.0)
                num = 8.0;

            __result = num;
            return false;
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
            reward *= 10f;
            __result = (long)Mathf.Ceil(reward);
            return false; 
        }


        [HarmonyPrefix, HarmonyPatch(typeof(FruitController), "harvestSeedReward")]
        private static bool harvestSeedReward(FruitController __instance, ref long __result, int fruitID, int tier, float poopBonus)
        {
            float reward = (float)(character.yggdrasilController.baseSeedReward[fruitID] * tier * 2)
                           * poopBonus
                           * character.adventureController.itopod.totalSeedBonus()
                           * character.beastQuestPerkController.totalSeedBonus()
                           * (1f + character.inventoryController.bonuses[specType.Seeds])
                           * character.NGUController.yggdrasilBonus()
                           * character.adventureController.itopod.totalHarvestBonus(fruitID);
            reward *= 10f;
            __result = (long)Mathf.Ceil(reward);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(DailyRewardController), "targetSpinTime")]
        private static bool targetSpinTime(ref float __result)
        {
            __result = 8640f;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "totalBankedAdvTraining")]
        private static bool totalBankedAdvTraining(ref float __result)
        {
            float num = 0f;
            num = (float)character.adventure.itopod.perkLevel[36] * character.adventureController.itopod.effectPerLevel[36] + (float)character.adventure.itopod.perkLevel[37] * character.adventureController.itopod.effectPerLevel[37] + (float)character.adventure.itopod.perkLevel[38] * character.adventureController.itopod.effectPerLevel[38] + (float)character.adventure.itopod.perkLevel[39] * character.adventureController.itopod.effectPerLevel[39] + (float)character.adventure.itopod.perkLevel[40] * character.adventureController.itopod.effectPerLevel[40];
            num += (float)character.beastQuest.quirkLevel[20] * character.beastQuestPerkController.effectPerLevel[20] *2;
            num += (float)character.beastQuest.quirkLevel[21] * character.beastQuestPerkController.effectPerLevel[21] *2;
            if (character.settings.rebirthDifficulty >= difficulty.evil)
            {
                num += (float)character.beastQuest.quirkLevel[22] * character.beastQuestPerkController.effectPerLevel[22] *2;
                num += (float)character.beastQuest.quirkLevel[23] * character.beastQuestPerkController.effectPerLevel[23] *2;
                num += (float)character.beastQuest.quirkLevel[24] * character.beastQuestPerkController.effectPerLevel[24] *2;
            }

            if (num < 0f)
            {
                num = 0f;
            }

            if (num > 0.99f)
            {
                num = 0.99f;
            }

            __result = num;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "totalBankedTimeMachine")]
        private static bool totalBankedTimeMachine(ref float __result)
        {
            float num = (float)character.adventure.itopod.perkLevel[41] * character.adventureController.itopod.effectPerLevel[41] + (float)character.adventure.itopod.perkLevel[42] * character.adventureController.itopod.effectPerLevel[42] + (float)character.adventure.itopod.perkLevel[43] * character.adventureController.itopod.effectPerLevel[43] + (float)character.adventure.itopod.perkLevel[44] * character.adventureController.itopod.effectPerLevel[44] + (float)character.adventure.itopod.perkLevel[45] * character.adventureController.itopod.effectPerLevel[45];
            num += (float)character.beastQuest.quirkLevel[25] * character.beastQuestPerkController.effectPerLevel[25] *2;
            num += (float)character.beastQuest.quirkLevel[26] * character.beastQuestPerkController.effectPerLevel[26] *2;
            if (character.settings.rebirthDifficulty >= difficulty.evil)
            {
                num += (float)character.beastQuest.quirkLevel[27] * character.beastQuestPerkController.effectPerLevel[27] *2;
                num += (float)character.beastQuest.quirkLevel[28] * character.beastQuestPerkController.effectPerLevel[28] *2;
                num += (float)character.beastQuest.quirkLevel[29] * character.beastQuestPerkController.effectPerLevel[29] *2;
            }

            if (num < 0f)
            {
                num = 0f;
            }

            if (num > 0.99f)
            {
                num = 0.99f;
            }

            __result = num;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "totalBankedBeardTemp")]
        private static bool totalBankedBeardTemp(ref float __result)
        {
            float num = (float)character.adventure.itopod.perkLevel[46] * character.adventureController.itopod.effectPerLevel[46] + (float)character.adventure.itopod.perkLevel[47] * character.adventureController.itopod.effectPerLevel[47] + (float)character.adventure.itopod.perkLevel[48] * character.adventureController.itopod.effectPerLevel[48] + (float)character.adventure.itopod.perkLevel[49] * character.adventureController.itopod.effectPerLevel[49] + (float)character.adventure.itopod.perkLevel[50] * character.adventureController.itopod.effectPerLevel[50];
            num += (float)character.beastQuest.quirkLevel[30] * character.beastQuestPerkController.effectPerLevel[30] *2;
            num += (float)character.beastQuest.quirkLevel[31] * character.beastQuestPerkController.effectPerLevel[31] *2;
            if (character.settings.rebirthDifficulty >= difficulty.evil)
            {
                num += (float)character.beastQuest.quirkLevel[32] * character.beastQuestPerkController.effectPerLevel[32] *2;
                num += (float)character.beastQuest.quirkLevel[33] * character.beastQuestPerkController.effectPerLevel[33] *2;
                num += (float)character.beastQuest.quirkLevel[34] * character.beastQuestPerkController.effectPerLevel[34] *2;
            }

            if (num < 0f)
            {
                num = 0f;
            }

            if (num > 0.75f)
            {
                num = 0.75f;
            }

            __result = num;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "macGuffinBonusTimeFactor")]
        private static bool macGuffinBonusTimeFactor(ref float __result)
        {
            double t = character.rebirthTime.totalseconds * 5.0;

            float num;

            if (t < 180.0)
                num = 0f;
            else if (t <= 1800.0)
                num = Mathf.Pow((float)(t / 1800.0), 2f);
            else if (t <= 86400.0)
                num = Mathf.Pow((float)(t / 1800.0), 1f);
            else
                num = 48f * Mathf.Pow((float)(t / 86400.0), 0.4f);

            bool sadisticBoost =
                character.settings.rebirthDifficulty >= difficulty.sadistic &&
                character.allChallenges.trollChallenge.sadisticCompletions() >= 2;

            float scale = sadisticBoost ? 1f : 0.67f;
            num *= scale;

            float cap = 104.86f * scale;
            if (num > cap) num = cap;

            if (character.arbitrary.macGuffinBooster1Time.totalseconds > 0.0 || character.arbitrary.macGuffinBooster1InUse)
                num *= character.allArbitrary.potionModifier();

            __result = num;
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(PitController), "currentPitTime")]
        private static bool currentPitTime(ref float __result)
        {
            __result = 600 * (character.pit.tossCount + 1);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "autoMergeTime")]
        private static bool autoMergeTime(ref float __result)
        {
            float num = 600f;
            num *= 1f - (float)character.allChallenges.noEquipmentChallenge.completions() * 0.1f;
            if (character.arbitrary.improvedAutoBoostMerge)
            {
                num *= 0.5f;
            }

            __result = num;
            return false;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "autoBoostTime")]
        private static bool autoBoostTime(ref float __result)
        {
            float num = 600f;
            num *= 1f - (float)character.allChallenges.noEquipmentChallenge.completions() * 0.1f;
            if (character.arbitrary.improvedAutoBoostMerge)
            {
                num *= 0.5f;
            }
            __result = num;
            return false;
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
            __result = num2 * character.beastQuestPerkController.totalBoostBonus() * 10;
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllBeardsController), "energyBeardSpeedFactor")]
        private static void energyBeardSpeedFactor(ref float __result)
        {
            __result *= 10f;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(AllBeardsController), "magicBeardSpeedFactor")]
        private static void magicBeardSpeedFactor(ref float __result)
        {
            __result *= 10f;
        }



        [HarmonyPatch(nameof(ItemNameDesc.makeLoot))]
        private static bool MakeLoot_Prefix(ItemNameDesc __instance, ref string __result, int id)
        {
            if (id == 0)
            {
                __result = "";
                return false;
            }

            __instance.character.allItemList.markItemAsDropped(id);

            Equipment equipment = new Equipment(
                __instance.type[id], __instance.bossRequired[id],
                __instance.curAttack[id], __instance.capAttack[id],
                __instance.curDefense[id], __instance.capDefense[id],
                __instance.specType1[id], __instance.curSpec1[id], __instance.capSpec1[id],
                __instance.specType2[id], __instance.curSpec2[id], __instance.capSpec2[id],
                __instance.specType3[id], __instance.curSpec3[id], __instance.capSpec3[id],
                __instance.path[id], id
            );

            if (equipment != null)
            {
                // --- same early-return filter behavior as vanilla ---
                if (__instance.character.arbitrary.lootFilter &&
                    __instance.character.inventory.itemList.itemFiltered[equipment.id])
                {
                    if (__instance.character.arbitrary.hasCubeFilter && equipment.isBoost())
                        __instance.cubeFilter(equipment);

                    __result = __instance.itemName[id];
                    return false;
                }

                if (__instance.character.purchases.hasFilter && __instance.character.settings.filterOn &&
                    ((equipment.type == part.Accessory && __instance.character.settings.filterAccessory) ||
                     (equipment.type == part.Head && __instance.character.settings.filterHead) ||
                     (equipment.type == part.Chest && __instance.character.settings.filterChest) ||
                     (equipment.type == part.Legs && __instance.character.settings.filterLegs) ||
                     (equipment.type == part.Boots && __instance.character.settings.filterBoots) ||
                     (equipment.type == part.Weapon && __instance.character.settings.filterWeapon) ||
                     (equipment.type == part.atkBoost && __instance.character.settings.filterBoostAtk) ||
                     (equipment.type == part.defBoost && __instance.character.settings.filterBoostDef) ||
                     (equipment.type == part.specBoost && __instance.character.settings.filterBoostSpec) ||
                     (equipment.type == part.Misc && __instance.character.settings.filterMisc)))
                {
                    if (__instance.character.arbitrary.hasCubeFilter && equipment.isBoost())
                        __instance.cubeFilter(equipment);

                    __result = __instance.itemName[id];
                    return false;
                }

                // --- YOUR CHANGE: force “real gear” to be at least level 4 ---
                // makeLoot() normally leaves level at 0; we bump only non-boost, non-macguffin items.
                if (equipment.level < 4 &&
                    equipment.type != part.atkBoost &&
                    equipment.type != part.defBoost &&
                    equipment.type != part.specBoost &&
                    equipment.type != part.MacGuffin)
                {
                    equipment.level = 4;
                }

                // vanilla bonus behavior (only applies if level > 0)
                if (equipment.level > 0 && equipment.level < 100)
                    equipment.level += __instance.bonusLootLevels();

                // vanilla autoTransform behavior
                if (__instance.character.settings.autoTransform >= 1 &&
                    __instance.character.settings.autoTransform <= 3 &&
                    equipment.isBoost())
                {
                    Equipment t = __instance.autoTransform(equipment, __instance.character.settings.autoTransform);
                    if (t.id != 0) equipment = t;
                }

                int slot = __instance.addLoot(equipment);
                __instance.ic.updateItem(slot);
            }

            __result = __instance.itemName[id];
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemNameDesc), nameof(ItemNameDesc.makeLevelledLoot))]
        private static void MakeLevelledLoot_ClampPrefix(ItemNameDesc __instance, int id, ref int lootlevel)
        {
            // Don’t mess with macguffins; the method already treats them specially.
            if (__instance.type[id] == part.MacGuffin)
                return;

            if (lootlevel < 4)
                lootlevel = 4;
        }
    }
}
