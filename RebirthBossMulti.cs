using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.TextCore;

namespace fasterPace


{
    [HarmonyPatch]
    internal class RebirthBossMulti
    {
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }
        private static void RefreshRebirthUI()
        {
            if (character == null) return;

            // Try common member names across versions
            object rc =
                GetMember(character, "rebirthController") ??
                GetMember(character, "rebirthMenu") ??
                GetMember(character, "rebirth") ??
                GetMember(character, "rebirthUI") ??
                GetMember(character, "rebirthCtrl");

            if (rc == null) return;

            var t = rc.GetType();
            (t.GetMethod("updateMenu", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             ?? t.GetMethod("updateRebirthMenu", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             ?? t.GetMethod("updateText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            ?.Invoke(rc, null);
        }

        private static object GetMember(object obj, string name)
        {
            var type = obj.GetType();
            return (object)type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj, null)
                ?? type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
        }
        [HarmonyPrefix, HarmonyPatch(typeof(BossController), "advanceBoss")]
        private static bool advanceBoss(BossController __instance)
        {
            character.buttons.updateButtons();
            __instance.rewardExp();
            character.bossID++;
            __instance.bossTextScrollbar.value = 1f;
            if (character.bossID == 30)
            {
                character.timeMachineController.setBankedLevels();
            }

            __instance.inventoryController.updateBonuses();
            character.augmentsController.updateMenu();
            character.stats.bossesDefeated++;
            character.adventureController.constructDropdown();
            if (character.bossID > character.highestBoss && character.settings.rebirthDifficulty == difficulty.normal)
            {
                character.highestBoss = character.bossID;
                character.stats.highestBoss = character.bossID;
            }

            if (character.bossID > character.highestHardBoss && character.settings.rebirthDifficulty == difficulty.evil)
            {
                character.highestHardBoss = character.bossID;
            }

            if (character.bossID > character.highestSadisticBoss && character.settings.rebirthDifficulty == difficulty.sadistic)
            {
                character.highestSadisticBoss = character.bossID;
            }

            if (character.bossID > character.currentHighestBoss)
            {
                character.currentHighestBoss = character.bossID;
            }

            if (character.settings.rebirthDifficulty == difficulty.normal)
            {
                character.bossMulti *= 2.1;
                RefreshRebirthUI();
            }

            else if (character.settings.rebirthDifficulty == difficulty.evil)
            {
                character.bossMulti *= 1.75;
                RefreshRebirthUI();
            }
            else if (character.settings.rebirthDifficulty == difficulty.sadistic)
            {
                character.bossMulti *= __instance.sadisticBossMultiplier();
                RefreshRebirthUI();
            }

            if (character.bossID >= 4)
            {
                character.settings.inventoryOn = true;
            }

            if (character.bossID == 37)
            {
                character.magic.unlockMagic();
            }

            if (character.bossID == 58)
            {
                character.challenges.unlocked = true;
            }

            if (character.bossID > 300)
            {
                __instance.bossName.text = "NONE (YOU KILLED THEM ALL)";
                __instance.bossDesc.text = "You've defeated every single boss! Rebirth to bring them back, or try a higher difficulty rebirth!";
                character.bossAttack = 69.0;
                character.bossDefense = 69.0;
                character.bossRegen = 420.0;
                character.bossCurHP = 420.0;
                character.bossMaxHP = 420.0;
                character.buttons.updateButtons();
                __instance.updateMenu();
                if (character.settings.rebirthDifficulty >= difficulty.sadistic)
                {
                    if (character.itemInfo.findIndexWithID(487) == -1)
                    {
                        character.itemInfo.makeLevelledLoot(487, 100);
                    }

                    character.tooltip.showTooltip("THE END NEARS.");
                }
            }
            else
            {
                character.bossAttack = __instance.boss.bossAttack[character.bossID];
                character.bossDefense = __instance.boss.bossDefense[character.bossID];
                character.bossRegen = __instance.boss.bossRegen[character.bossID];
                character.bossCurHP = __instance.boss.bossCurHP[character.bossID];
                character.bossMaxHP = __instance.boss.bossMaxHP[character.bossID];
                __instance.updateMenu();
                __instance.bossTextScrollbar.value = 1f;
                character.buttons.updateButtons();
            }
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BossController), "sadisticBossMultiplier")]
        private static bool sadisticBossMultiplier(BossController __instance, ref float __result)
        {
            __result = 1.4f + character.adventureController.itopod.sadisticBossMultiplierBonus() + character.beastQuestPerkController.sadisticBossMultiplierBonus() + character.wishesController.sadisticBossMultiplierBonus();
            return false;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(RebirthPowerDisplay), "difficultyFactor")]
        private static bool RebirthPowerDisplay_difficultyFactor(RebirthPowerDisplay __instance, ref float __result)
        {
            switch (__instance.character.settings.rebirthDifficulty)
            {
                case difficulty.normal:
                    __result = 2.1f;
                    return false;

                case difficulty.evil:
                    __result = 1.75f;
                    return false;

                case difficulty.sadistic:
                    __result = __instance.character.bossController.sadisticBossMultiplier();
                    return false;

                default:
                    __result = 1.2f;
                    return false;
            }
        }

    }
}
