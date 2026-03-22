using HarmonyLib;
using UnityEngine;

namespace fasterPace
{
    /// <summary>
    /// Vanilla reconstruction of Money Pit per-toss tier rewards (Tier 1-10).
    /// Tier 11-16 are wish-gated in vanilla; intentionally not patched here.
    /// Edit the switch bodies to change rewards.
    /// </summary>
    [HarmonyPatch(typeof(PitController))]
    internal static class MoneyPit_TierRewards_VanillaReconstruct
    {
        // PitController.message is private in vanilla.
        private static readonly AccessTools.FieldRef<PitController, string> _message =
            AccessTools.FieldRefAccess<PitController, string>("message");

        private static ref string Message(PitController pit) => ref _message(pit);

        [HarmonyPrefix]
        [HarmonyPatch("tier1Reward")]
        private static bool Tier1Reward_Prefix(PitController __instance)
        {
            if (__instance?.character == null || __instance.character.pit == null) return true;

                        UnityEngine.Random.state = __instance.character.pit.pitState;
            		int num = UnityEngine.Random.Range(1, 9);
            		__instance.character.pit.pitState = UnityEngine.Random.state;
            		switch (num)
            		{
            		case 1:
            			__instance.itemInfo.makeLoot(1);
            			Message(__instance) = "The Pit Belches and spits out an " + __instance.itemInfo.itemName[1] + "!\n\n";
            			return false;
            case 2:
            			__instance.itemInfo.makeLoot(14);
            			Message(__instance) = "The Pit Belches and spits out a " + __instance.itemInfo.itemName[14] + "!\n\n";
            			return false;
            case 3:
            			__instance.itemInfo.makeLoot(27);
            			Message(__instance) = "The Pit Belches and spits out a " + __instance.itemInfo.itemName[27] + "!\n\n";
            			return false;
            case 4:
            			__instance.character.adventure.attack += 2f;
            			Message(__instance) = "You feel slightly more powerful. +2 Power to be exact!";
            			return false;
            case 5:
            			__instance.character.adventure.defense += 2f;
            			Message(__instance) = "You gain +2 Toughness units! Or just Toughness, whatever you prefer.";
            			return false;
            case 6:
            			__instance.character.adventure.maxHP += 20f;
            			Message(__instance) = "You have gained +20 Max Health. You can have the crap kicked out of you just a little more, rejoice!";
            			return false;
            case 7:
            			__instance.character.adventure.regen += 0.2f;
            			Message(__instance) = "You gain +0.2 health regen. Everyone is happy except for the adventure mode monsters.";
            			return false;
            default:
            			Message(__instance) = "The Pit Belches and it smells awful. Unlucky!\n\n";
            			return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("tier2Reward")]
        private static bool Tier2Reward_Prefix(PitController __instance)
        {
            if (__instance?.character == null || __instance.character.pit == null) return true;

                        UnityEngine.Random.state = __instance.character.pit.pitState;
            		int num = UnityEngine.Random.Range(1, 10);
            		__instance.character.pit.pitState = UnityEngine.Random.state;
            		switch (num)
            		{
            		case 1:
            			__instance.itemInfo.makeLoot(2);
            			Message(__instance) = "The Pit Belches and spits out an " + __instance.itemInfo.itemName[2] + "!\n\n";
            			return false;
            case 2:
            			__instance.itemInfo.makeLoot(15);
            			Message(__instance) = "The Pit Belches and spits out a " + __instance.itemInfo.itemName[15] + "!\n\n";
            			return false;
            case 3:
            			__instance.itemInfo.makeLoot(28);
            			Message(__instance) = "The Pit Belches and spits out a " + __instance.itemInfo.itemName[28] + "!\n\n";
            			return false;
            case 4:
            			__instance.inventoryController.randomLevelUp();
	            		Message(__instance) = "The pit sends out a shockwave of energy... you feel like one of your items has grown in power!";
            			return false;
            case 5:
            			__instance.character.addExp(2L);
            			Message(__instance) = "You see a yellow '+2 EXP' float up out of the pit and land on your head before disappearing.";
            			return false;
            case 6:
            			__instance.character.adventure.attack += 4f;
            			Message(__instance) = "You feel slightly more powerful. +4 Power to be exact!";
            			return false;
            case 7:
            			__instance.character.adventure.defense += 4f;
            			Message(__instance) = "You gain +4 Toughness units! Or just Toughness, whatever you prefer.";
            			return false;
            case 8:
            			__instance.character.adventure.maxHP += 40f;
            			Message(__instance) = "You have gained +40 Max Health! You can have the crap kicked out of you just a little more, rejoice!";
            			return false;
            case 9:
            			__instance.character.adventure.regen += 0.4f;
            			Message(__instance) = "You gain +0.4 health regen. Everyone is happy except for the adventure mode monsters.";
            			return false;
            default:
            			Message(__instance) = "The Pit Belches and it smells awful.\n\n";
            			return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("tier3Reward")]
        private static bool Tier3Reward_Prefix(PitController __instance)
        {
            if (__instance?.character == null || __instance.character.pit == null) return true;

                        UnityEngine.Random.state = __instance.character.pit.pitState;
            		int num = UnityEngine.Random.Range(1, 10);
            		__instance.character.pit.pitState = UnityEngine.Random.state;
            		switch (num)
            		{
            		case 1:
            			__instance.itemInfo.makeLoot(3);
            			Message(__instance) = "The Pit Belches and spits out an " + __instance.itemInfo.itemName[3] + "!\n\n";
            			return false;
            case 2:
            			__instance.itemInfo.makeLoot(16);
            			Message(__instance) = "The Pit Belches and spits out a " + __instance.itemInfo.itemName[16] + "!\n\n";
            			return false;
            case 3:
            			__instance.itemInfo.makeLoot(29);
            			Message(__instance) = "The Pit Belches and spits out a " + __instance.itemInfo.itemName[29] + "!\n\n";
            			return false;
            case 4:
            			__instance.inventoryController.randomLevelUp();
	            		Message(__instance) = "The pit sends out a shockwave of energy... you feel like one of your items has grown in power!";
            			return false;
            case 5:
            			__instance.character.addExp(4L);
            			Message(__instance) = "You see a yellow '4' float up out of the pit and land on your head before disappearing. Weird. Wonder what that could mean?";
            			return false;
            case 6:
            			__instance.character.adventure.attack += 10f;
            			Message(__instance) = "You feel slightly more powerful. +10 Power to be exact!";
            			return false;
            case 7:
            			__instance.character.adventure.defense += 10f;
            			Message(__instance) = "You gain +10 Toughness units! Or just Toughness, whatever you prefer.";
            			return false;
            case 8:
            			__instance.character.adventure.maxHP += 100f;
            			Message(__instance) = "You have gained +100 Max Health! You can have the crap kicked out of you just a little more, rejoice!";
            			return false;
            case 9:
            			__instance.character.adventure.regen += 1f;
            			Message(__instance) = "You gain +1 health regen. Everyone is happy except for the adventure mode monsters.";
            			return false;
            default:
            			Message(__instance) = "The Pit Belches and it smells awful.\n\n";
            			return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("tier4Reward")]
        private static bool Tier4Reward_Prefix(PitController __instance)
        {
            if (__instance?.character == null || __instance.character.pit == null) return true;

                        UnityEngine.Random.state = __instance.character.pit.pitState;
            		int num = UnityEngine.Random.Range(1, 10);
            		__instance.character.pit.pitState = UnityEngine.Random.state;
            		switch (num)
            		{
            		case 1:
            			__instance.itemInfo.makeLoot(4);
            			Message(__instance) = "The Pit Belches and spits out an " + __instance.itemInfo.itemName[4] + "!\n\n";
            			return false;
            case 2:
            			__instance.itemInfo.makeLoot(17);
            			Message(__instance) = "The Pit Belches and spits out a " + __instance.itemInfo.itemName[17] + "!\n\n";
            			return false;
            case 3:
            			__instance.itemInfo.makeLoot(30);
            			Message(__instance) = "The Pit Belches and spits out a " + __instance.itemInfo.itemName[30] + "!\n\n";
            			return false;
            case 4:
            			__instance.inventoryController.randomLevelUp();
	            		__instance.inventoryController.randomLevelUp();
	            		Message(__instance) = "The pit sends out a shockwave of energy... you feel like two of your items have grown in power!";
            			return false;
            case 5:
            			__instance.character.addExp(6L);
            			Message(__instance) = "You see a yellow '+6 EXP' float up out of the pit and land on your head before disappearing.";
            			return false;
            case 6:
            			__instance.character.adventure.attack += 20f;
            			Message(__instance) = "You feel slightly more powerful. +20 Power to be exact!";
            			return false;
            case 7:
            			__instance.character.adventure.defense += 20f;
            			Message(__instance) = "You gain +20 Toughness units! Or just Toughness, whatever you prefer.";
            			return false;
            case 8:
            			__instance.character.adventure.maxHP += 150f;
            			Message(__instance) = "You have gained +150 Max Health! You can have the crap kicked out of you just a little more, rejoice!";
            			return false;
            case 9:
            			__instance.character.adventure.regen += 2f;
            			Message(__instance) = "You gain +2 health regen! Everyone is happy except for the adventure mode monsters.";
            			return false;
            default:
            			Message(__instance) = "The Pit Belches and it smells awful.\n\n";
            			return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("tier5Reward")]
        private static bool Tier5Reward_Prefix(PitController __instance)
        {
            if (__instance?.character == null || __instance.character.pit == null) return true;

                        UnityEngine.Random.state = __instance.character.pit.pitState;
            		int num = UnityEngine.Random.Range(1, 11);
            		__instance.character.pit.pitState = UnityEngine.Random.state;
            		switch (num)
            		{
            		case 1:
            			__instance.character.inventory.cubePower += 10f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 5 Power!";
            			return false;
            case 2:
            			__instance.character.inventory.cubeToughness += 10f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 10 Toughness!";
            			return false;
            case 3:
            			__instance.character.inventory.cubePower += 6f;
            			__instance.character.inventory.cubeToughness += 6f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 6 Power and 6 Toughness!";
            			return false;
            case 4:
            			__instance.inventoryController.allLevelUp();
            			Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your worn Equipment has grown in power!";
            			return false;
            case 5:
            			__instance.character.addExp(20L);
            			Message(__instance) = "You see a yellow '+20 EXP' float up out of the pit and land on your head before disappearing.";
            			return false;
            case 6:
            			if (__instance.character.settings.yggdrasilOn)
            			{
            				__instance.character.yggdrasil.seeds += 20L;
            				Message(__instance) = "An explosion of giant green seeds shoot out of the pit. They smack you over the head as they fall back to the ground... ow ow ow!! You manage to grab a hold of twenty of them!";
            				return false;
            }
            			Message(__instance) = "A giant green seed shoots out of the pit and lands by your feet! Before you can grab it, it hops back into the pit! WTF was that??";
            			return false;
            case 7:
            			__instance.character.adventure.attack += 40f;
            			Message(__instance) = "You feel slightly more powerful. +40 Power to be exact!";
            			return false;
            case 8:
            			__instance.character.adventure.defense += 40f;
            			Message(__instance) = "You gain +40 Toughness units! Or just Toughness, whatever you prefer.";
            			return false;
            case 9:
            			__instance.character.adventure.maxHP += 300f;
            			Message(__instance) = "You have gained +300 Max Health! You can have the crap kicked out of you just a little more, rejoice!";
            			return false;
            case 10:
            			__instance.character.adventure.regen += 3f;
            			Message(__instance) = "You gain +3 health regen! Everyone is happy except for the adventure mode monsters.";
            			return false;
            default:
            			Message(__instance) = "The Pit Belches and it smells awful.\n\n";
            			return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("tier6Reward")]
        private static bool Tier6Reward_Prefix(PitController __instance)
        {
            if (__instance?.character == null || __instance.character.pit == null) return true;

                        UnityEngine.Random.state = __instance.character.pit.pitState;
            		int num = UnityEngine.Random.Range(1, 12);
            		__instance.character.pit.pitState = UnityEngine.Random.state;
            		switch (num)
            		{
            		case 1:
            			__instance.character.inventory.cubePower += 20f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 20 Power!";
            			return false;
            case 2:
            			__instance.character.inventory.cubeToughness += 20f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 20 Toughness!";
            			return false;
            case 3:
            			__instance.character.inventory.cubePower += 10f;
            			__instance.character.inventory.cubeToughness += 10f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 10 Power and 10 Toughness!";
            			return false;
            case 4:
            			__instance.inventoryController.allLevelUp();
            			Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your worn Equipment has grown in power!";
            			return false;
            case 5:
            			__instance.character.addExp(50L);
            			Message(__instance) = "You gained 50 EXP! Don't get addicted to this stuff now!";
            			return false;
            case 6:
            			if (__instance.character.settings.yggdrasilOn)
            			{
            				__instance.character.yggdrasil.seeds += 50L;
            				Message(__instance) = "An explosion of giant green seeds shoot out of the pit. They smack you over the head as they fall back to the ground... ow ow ow!! You manage to grab a hold of 25 seeds at least!";
            				return false;
            }
            			Message(__instance) = "A giant green seed shoots out of the pit and lands by your feet! Before you can grab it, it hops back into the pit! WTF was that??";
            			return false;
            case 7:
            			__instance.character.adventure.attack += 100f;
            			Message(__instance) = "You feel slightly more powerful. +100 Power to be exact!";
            			return false;
            case 8:
            			__instance.character.adventure.defense += 100f;
            			Message(__instance) = "You gain +50 Toughness units! Or just Toughness, whatever you prefer.";
            			return false;
            case 9:
            			__instance.character.adventure.maxHP += 400f;
            			Message(__instance) = "You have gained +400 Max Health! You can have the crap kicked out of you just a little more, rejoice!";
            			return false;
            case 10:
            			__instance.character.adventure.regen += 4f;
            			Message(__instance) = "You gain +4.0 health regen! Everyone is happy except for the adventure mode monsters.";
            			return false;
            case 11:
            			if (__instance.character.wandoos98.pitOSLevels < 99L)
            			{
            				__instance.character.wandoos98.pitOSLevels += 2L;
            				Message(__instance) = "The spirit of Wandoos XP floats out of the pit and blesses your crappy OS. +2 Wandoos level!";
            				return false;
            }
            			Message(__instance) = "The Pit belches and it smells awful.\n\n";
            			return false;
            default:
            			Message(__instance) = "The Pit belches and it smells awful.\n\n";
            			return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("tier7Reward")]
        private static bool Tier7Reward_Prefix(PitController __instance)
        {
            if (__instance?.character == null || __instance.character.pit == null) return true;

                        UnityEngine.Random.state = __instance.character.pit.pitState;
            		int num = UnityEngine.Random.Range(1, 13);
            		__instance.character.pit.pitState = UnityEngine.Random.state;
            		switch (num)
            		{
            		case 1:
            			__instance.character.inventory.cubePower += 40f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 40 Power!";
            			return false;
            case 2:
            			__instance.character.inventory.cubeToughness += 40f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 40 Toughness!";
            			return false;
            case 3:
            			__instance.character.inventory.cubePower += 20f;
            			__instance.character.inventory.cubeToughness += 20f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 20 Power and 20 Toughness!";
            			return false;
            case 4:
            			__instance.inventoryController.allLevelUp();
            			Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your worn Equipment has grown in power!";
            			return false;
            case 5:
            			__instance.character.addExp(50L);
            			Message(__instance) = "You gained 50 EXP! Don't get addicted to this stuff now!";
            			return false;
            case 6:
            			if (__instance.character.settings.yggdrasilOn)
            			{
            				__instance.character.yggdrasil.seeds += 200L;
            				Message(__instance) = "An explosion of giant green seeds shoot out of the pit. They smack you over the head as they fall back to the ground... ow ow ow!! You manage to grab a hold of 200 seeds at least!";
            				return false;
            }
            			Message(__instance) = "A giant green seed shoots out of the pit and lands by your feet! Before you can grab it, it hops back into the pit! WTF was that??";
            			return false;
            case 7:
            			__instance.character.adventure.attack += 200f;
            			Message(__instance) = "You feel slightly more powerful. +200 Power to be exact!";
            			return false;
            case 8:
            			__instance.character.adventure.defense += 200f;
            			Message(__instance) = "You gain +200 Toughness units! Or just Toughness, whatever you prefer.";
            			return false;
            case 9:
            			__instance.character.adventure.maxHP += 600f;
            			Message(__instance) = "You have gained +600 Max Health! You can have the crap kicked out of you just a little more, rejoice!";
            			return false;
            case 10:
            			__instance.character.adventure.regen += 6f;
            			Message(__instance) = "You gain +6 health regen! Everyone is happy except for the adventure mode monsters.";
            			return false;
            case 11:
            			if (__instance.character.wandoos98.pitOSLevels < 99L)
            			{
            				__instance.character.wandoos98.pitOSLevels += 2L;
            				Message(__instance) = "The spirit of Wandoos XP floats out of the pit and blesses your crappy OS. +1 Wandoos level!";
            				return false;
            }
            			Message(__instance) = "The Pit belches and it smells awful.\n\n";
            			return false;
            case 12:
            			__instance.inventoryController.daycareLevelUp();
            			Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your daycare items have grown in power!";
            			return false;
            default:
            			Message(__instance) = "The Pit belches and it smells awful.\n\n";
            			return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("tier8Reward")]
        private static bool Tier8Reward_Prefix(PitController __instance)
        {
            if (__instance?.character == null || __instance.character.pit == null) return true;

                        UnityEngine.Random.state = __instance.character.pit.pitState;
            		int num = UnityEngine.Random.Range(1, 13);
            		__instance.character.pit.pitState = UnityEngine.Random.state;
            		switch (num)
            		{
            		case 1:
            			__instance.character.inventory.cubePower += 100f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 50 Power!";
            			return false;
            case 2:
            			__instance.character.inventory.cubeToughness += 100f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 50 Toughness!";
            			return false;
            case 3:
            			__instance.character.inventory.cubePower += 50f;
            			__instance.character.inventory.cubeToughness += 50f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 50 Power and 50 Toughness!";
            			return false;
            case 4:
            			__instance.inventoryController.allLevelUp();
            			Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your worn Equipment has grown in power!";
            			return false;
            case 5:
            			__instance.character.addExp(400L);
            			Message(__instance) = "You gained 400 EXP! Don't get addicted to this stuff now!";
            			return false;
            case 6:
            			if (__instance.character.settings.yggdrasilOn)
            			{
            				__instance.character.yggdrasil.seeds += 400L;
            				Message(__instance) = "An explosion of giant green seeds shoot out of the pit. They smack you over the head as they fall back to the ground... ow ow ow!! You manage to grab a hold of 200 seeds! How the heck can you carry so many in your arms?";
            				return false;
            }
            			Message(__instance) = "A giant green seed shoots out of the pit and lands by your feet! Before you can grab it, it hops back into the pit! WTF was that??";
            			return false;
            case 7:
            			__instance.character.adventure.attack += 300f;
            			Message(__instance) = "You feel slightly more powerful. +300 Power to be exact!";
            			return false;
            case 8:
            			__instance.character.adventure.defense += 300f;
            			Message(__instance) = "You gain +150 Toughness units! Or just Toughness, whatever you prefer.";
            			return false;
            case 9:
            			__instance.character.adventure.maxHP += 900f;
            			Message(__instance) = "You have gained +900 Max Health! You can have the crap kicked out of you just a little more, rejoice!";
            			return false;
            case 10:
            			__instance.character.adventure.regen += 10f;
            			Message(__instance) = "You gain +5 health regen! Everyone is happy except for the adventure mode monsters.";
            			return false;
            case 11:
            			if (__instance.character.wandoos98.pitOSLevels < 100L)
            			{
            				__instance.character.wandoos98.pitOSLevels += 4L;
            				if (__instance.character.wandoos98.pitOSLevels > 100L)
            				{
            					__instance.character.wandoos98.pitOSLevels = 100L;
            				}
            				Message(__instance) = "The spirit of Wandoos XP floats out of the pit and blesses your crappy OS. +4 Wandoos level!";
            				return false;
            }
            			Message(__instance) = "The Pit belches and it smells awful.\n\n";
            			return false;
            case 12:
            			__instance.inventoryController.daycareLevelUp();
            			Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your daycare items have grown in power!";
            			return false;
            default:
            			Message(__instance) = "The Pit belches and it smells awful.\n\n";
            			return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("tier9Reward")]
        private static bool Tier9Reward_Prefix(PitController __instance)
        {
            if (__instance?.character == null || __instance.character.pit == null) return true;

                        UnityEngine.Random.state = __instance.character.pit.pitState;
            		int num = UnityEngine.Random.Range(1, 13);
            		__instance.character.pit.pitState = UnityEngine.Random.state;
            		switch (num)
            		{
            		case 1:
            			__instance.character.inventory.cubePower += 200f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 200 Power!";
            			return false;
            case 2:
            			__instance.character.inventory.cubeToughness += 200f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 200 Toughness!";
            			return false;
            case 3:
            			__instance.character.inventory.cubePower += 100f;
            			__instance.character.inventory.cubeToughness += 100f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 100 Power and 100 Toughness!";
            			return false;
            case 4:
            			__instance.inventoryController.allLevelUp();
            			Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your worn Equipment has grown in power!";
            			return false;
            case 5:
            			__instance.character.addExp(600L);
            			Message(__instance) = "You gained 600 EXP! Don't get addicted to this stuff now!";
            			return false;
            case 6:
            			if (__instance.character.settings.yggdrasilOn)
            			{
            				__instance.character.yggdrasil.seeds += 600L;
            				Message(__instance) = "An explosion of giant green seeds shoot out of the pit. They smack you over the head as they fall back to the ground... ow ow ow!! You manage to grab a hold of 300 seeds! How the heck can you carry so many in your arms?";
            				return false;
            }
            			Message(__instance) = "A giant green seed shoots out of the pit and lands by your feet! Before you can grab it, it hops back into the pit! WTF was that??";
            			return false;
            case 7:
            			__instance.character.adventure.attack += 400f;
            			Message(__instance) = "You feel slightly more powerful. +400 Power to be exact!";
            			return false;
            case 8:
            			__instance.character.adventure.defense += 400f;
            			Message(__instance) = "You gain +400 Toughness units! Or just Toughness, whatever you prefer.";
            			return false;
            case 9:
            			__instance.character.adventure.maxHP += 1400f;
            			Message(__instance) = "You have gained +1400 Max Health! You can have the crap kicked out of you just a little more, rejoice!";
            			return false;
            case 10:
            			__instance.character.adventure.regen += 12f;
            			Message(__instance) = "You gain +12 health regen! Everyone is happy except for the adventure mode monsters.";
            			return false;
            case 11:
            			if (__instance.character.wandoos98.pitOSLevels < 100L)
            			{
            				__instance.character.wandoos98.pitOSLevels += 4L;
            				if (__instance.character.wandoos98.pitOSLevels > 100L)
            				{
            					__instance.character.wandoos98.pitOSLevels = 100L;
            				}
            				Message(__instance) = "The spirit of Wandoos XP floats out of the pit and blesses your crappy OS. +4 Wandoos level!";
            				return false;
            }
            			Message(__instance) = "The Pit belches and it smells awful.\n\n";
            			return false;
            case 12:
            			__instance.inventoryController.daycareLevelUp();
            			Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your daycare items have grown in power!";
            			return false;
            default:
            			Message(__instance) = "The Pit belches and it smells awful.\n\n";
            			return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("tier10Reward")]
        private static bool Tier10Reward_Prefix(PitController __instance)
        {
            if (__instance?.character == null || __instance.character.pit == null) return true;

                        UnityEngine.Random.state = __instance.character.pit.pitState;
            		int num = UnityEngine.Random.Range(1, 13);
            		__instance.character.pit.pitState = UnityEngine.Random.state;
            		switch (num)
            		{
            		case 1:
            			__instance.character.inventory.cubePower += 300f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 300 Power!";
            			return false;
            case 2:
            			__instance.character.inventory.cubeToughness += 300f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 300 Toughness!";
            			return false;
            case 3:
            			__instance.character.inventory.cubePower += 150f;
            			__instance.character.inventory.cubeToughness += 150f;
            			Message(__instance) = "The Pit Blesses your Infinity Cube with 150 Power and 150 Toughness!";
            			return false;
            case 4:
            			__instance.inventoryController.allLevelUp();
            			Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your worn Equipment has grown in power!";
            			return false;
            case 5:
            			__instance.character.addExp(800L);
            			Message(__instance) = "You gained 800 EXP! Don't get addicted to this stuff now!";
            			return false;
            case 6:
            			if (__instance.character.settings.yggdrasilOn)
            			{
            				__instance.character.yggdrasil.seeds += 1000L;
            				Message(__instance) = "An explosion of giant green seeds shoot out of the pit. They smack you over the head as they fall back to the ground... ow ow ow!! You've rented a moving truck in advance and stuffed 1000 seeds into the back!";
            				return false;
            }
            			Message(__instance) = "A giant green seed shoots out of the pit and lands by your feet! Before you can grab it, it hops back into the pit! WTF was that??";
            			return false;
            case 7:
            			__instance.character.adventure.attack += 500f;
            			Message(__instance) = "You feel slightly more powerful. +500 Power to be exact!";
            			return false;
            case 8:
            			__instance.character.adventure.defense += 500f;
            			Message(__instance) = "You gain +500 Toughness units! Or just Toughness, whatever you prefer.";
            			return false;
            case 9:
            			__instance.character.adventure.maxHP += 1400f;
            			Message(__instance) = "You have gained +1400 Max Health! You can have the crap kicked out of you just a little more, rejoice!";
            			return false;
            case 10:
            			__instance.character.adventure.regen += 15f;
            			Message(__instance) = "You gain +15 health regen! Everyone is happy except for the adventure mode monsters.";
            			return false;
            case 11:
            			if (__instance.character.wandoos98.pitOSLevels < 100L)
            			{
            				__instance.character.wandoos98.pitOSLevels += 4L;
            				if (__instance.character.wandoos98.pitOSLevels > 100L)
            				{
            					__instance.character.wandoos98.pitOSLevels = 100L;
            				}
            				Message(__instance) = "The spirit of Wandoos XP floats out of the pit and blesses your crappy OS. +4 Wandoos level!";
            				return false;
            }
            			Message(__instance) = "The Pit belches and it smells awful.\n\n";
            			return false;
            case 12:
            			__instance.inventoryController.daycareLevelUp();
            			Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your daycare items have grown in power!";
            			return false;
            default:
            			Message(__instance) = "The Pit belches and it smells awful.\n\n";
            			return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("tier11Reward")]
        private static bool Tier11Reward_Prefix(PitController __instance)
        {
            if (__instance?.character == null || __instance.character.pit == null) return true;

            UnityEngine.Random.state = __instance.character.pit.pitState;
            int num = UnityEngine.Random.Range(1, 13);
            __instance.character.pit.pitState = UnityEngine.Random.state;
            switch (num)
            {
                case 1:
                    __instance.character.inventory.cubePower += 400f;
                    Message(__instance) = "The Pit Blesses your Infinity Cube with 400 Power!";
                    return false;
                case 2:
                    __instance.character.inventory.cubeToughness += 400f;
                    Message(__instance) = "The Pit Blesses your Infinity Cube with 400 Toughness!";
                    return false;
                case 3:
                    __instance.character.inventory.cubePower += 200f;
                    __instance.character.inventory.cubeToughness += 200f;
                    Message(__instance) = "The Pit Blesses your Infinity Cube with 200 Power and 200 Toughness!";
                    return false;
                case 4:
                    __instance.inventoryController.allLevelUp();
                    Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your worn Equipment has grown in power!";
                    return false;
                case 5:
                    __instance.character.addExp(1000L);
                    Message(__instance) = "You gained 1000 EXP! Don't get addicted to this stuff now!";
                    return false;
                case 6:
                    if (__instance.character.settings.yggdrasilOn)
                    {
                        __instance.character.yggdrasil.seeds += 1400L;
                        Message(__instance) = "An explosion of giant green seeds shoot out of the pit. They smack you over the head as they fall back to the ground... ow ow ow!! You've rented a moving truck in advance and stuffed 2000 seeds into the back!";
                        return false;
                    }
                    Message(__instance) = "A giant green seed shoots out of the pit and lands by your feet! Before you can grab it, it hops back into the pit! WTF was that??";
                    return false;
                case 7:
                    __instance.character.adventure.attack += 600f;
                    Message(__instance) = "You feel slightly more powerful. +600 Power to be exact!";
                    return false;
                case 8:
                    __instance.character.adventure.defense += 600f;
                    Message(__instance) = "You gain +600 Toughness units! Or just Toughness, whatever you prefer.";
                    return false;
                case 9:
                    __instance.character.adventure.maxHP += 1800f;
                    Message(__instance) = "You have gained +1800 Max Health! You can have the crap kicked out of you just a little more, rejoice!";
                    return false;
                case 10:
                    __instance.character.adventure.regen += 18f;
                    Message(__instance) = "You gain +18 health regen! Everyone is happy except for the adventure mode monsters.";
                    return false;
                case 11:
                    if (__instance.character.wandoos98.pitOSLevels < 100L)
                    {
                        __instance.character.wandoos98.pitOSLevels += 6L;
                        if (__instance.character.wandoos98.pitOSLevels > 100L)
                        {
                            __instance.character.wandoos98.pitOSLevels = 100L;
                        }
                        Message(__instance) = "The spirit of Wandoos XP floats out of the pit and blesses your crappy OS. +6 Wandoos level!";
                        return false;
                    }
                    Message(__instance) = "The Pit belches and it smells awful.\n\n";
                    return false;
                case 12:
                    __instance.inventoryController.daycareLevelUp();
                    Message(__instance) = "The pit sends out a shockwave of energy... you feel like all of your daycare items have grown in power!";
                    return false;
                default:
                    Message(__instance) = "The Pit belches and it smells awful.\n\n";
                    return false;
            }
        }


    }
}
