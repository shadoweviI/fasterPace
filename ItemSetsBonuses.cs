using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace fasterPace


{

    [HarmonyPatch]
    internal class ItemSetsBonuses
    {
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(ItemListController), "setBonusText")]
        private static bool setBonusText(ItemListController __instance, ref string __result)
        {
        string EXP(long baseExp) => character != null ? (character.display(character.checkExpAdded(baseExp)) + " EXP") : (baseExp + " EXP");
        string AP(long baseAp) => character != null ? (character.display(character.checkAPAdded(baseAp)) + " AP") : (baseAp + " AP");

        string text = "";
        text = __instance.setID switch
        {
            -1 => "",
            0 => "\n\n<b>Training Set:</b>\nItems 62, 63, 64, 65, and 75.\n\n<b>Completion Bonus (All items level 100):</b>\n\n2 Energy Speed\n" + EXP(10L) + "." + "\n+12 Inventory Space.",
            1 => "\n\n<b>Sewers Set</b>\nItems 40-46.\n\n<b>Completion Bonus (All items level 100):</b>\n+5 Power and Toughness\n+15 max Health\n+0.2 regen\n" + EXP(20L) + "." + "\n+12 Inventory Space.",
            2 => "\n\n<b>Forest Set:</b>\nItems 47-53.\n\n<b>Completion Bonus (All items level 100):</b>\n2 Energy Potion α\n2 Energy Potion β\n2 Energy Bar Bar\n5 Energy Power\n" + EXP(200L) + "." + "\n+12 Inventory Space.",
            3 => "\n\n<b>Cave Set:</b>\nItems 54-61.\n\n<b>Completion Bonus (All items level 100):</b>\n2 Magic Power\n40000 Magic Cap\n2 Magic Per Bar\n" + EXP(300L) + "." + "\n+12 Inventory Space.",
            4 => "\n\n<b>HSB Set:</b>\nItems 68-74.\n\n<b>Completion Bonus (All items level 100):</b>\n3 Magic Power\n30000 Magic Cap\n3 Magic Bars\n1 Magic Potion α\n1 Magic Potion β\n1 Magic Bar Bar\n" + EXP(500L) + "\n+12 Inventory Space.",
            5 => "\n\n<b>GRB Set:</b>\nItems 78-84.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(2000L) + "\nA small perk: The Safe Zone will now regenerate health 10x faster, instead of 5x!",
            6 => "\n\n<b>Clock Set:</b>\nItems 85-91.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(1000L) + "\nA small perk: Enemies in Adventure will now spawn 5% Faster!",
            7 => "\n\n<b>2D Set:</b>\nItems 95-101.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(2000L) + "\nGain a permanent 7.43% bonus drop chance for loot!",
            8 => "\n\n<b>Spoopy Set:</b>\nItems 103-109.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(3000L) + "\nIdle attack will gain the same damage multiplier as Regular Attack!",
            9 => "\n\n<b>Jake Set:</b>\nItems 111-117.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(7000L) + "\nYou'll also unlock a new Wandoos OS: Wandoos MEH! This is a much stronger OS, provided you have the energy and magic to spare!",
            10 => "\n\n<b>Gaudy Set:</b>\nItems 122-126.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(5000L) + "\n2 Lucky Charms! Items drop one level higher now!",
            11 => "\n\n<b>Mega Set:</b>\nItems 130-134.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(6000L) + "\nCharge will now give a 2.2x boost to the next skill used!",
            12 => "\n\n<b>Beardverse Set:</b>\nItems 143-147.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(8000L) + "\n10% reduced penalty to levelling speed, when equipping multiple beards that use Energy or Magic at the same time!",
            13 => "\n\n<b>Wanderer's Set:</b>\nItems 150-153.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(50000L) + "!" + "\n" + AP(10000L) + "!" + "\nA new, ultra-rare accessory is now dropped by WALDERP!",
            14 => "\n\n<b>s'rerednaW Set:</b>\nItems 155-158.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(50000L) + "!" + "\n" + AP(10000L) + "!" + "\nA new, ultra-rare accessory is now dropped by WALDERP!",
            15 => "\n\n<b>Badly Drawn Set:</b>\nItems 164-168.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(30000L) + "!" + "\n" + AP(5000L) + "!" + "\nBoosts are now 20% more effective!",
            16 => "\n\n<b>Stealth Set:</b>\nItems 173-177.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(50000L) + "!" + "\n" + AP(10000L) + "!" + "\nUnlock an ultra-rare chest drop in Boring-Ass Earth!",
            17 => "\n\n<b>Slimy Set:</b>\nItems 184-188.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(100000L) + "!" + "\n" + AP(10000L) + "!" + "\nParry's reflected attack is now 3x stronger!",
            18 => "\n\n<b>Edgy Set:</b>\nItems 213-215, 217, and 218.\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(250000L) + "!\nGain a MacGuffin slot!",
            19 => "\n\n<b>Edgy Boots Set:</b>\nItems 216 and 219.\n\n<b>Completion Bonus (All items level 100):</b>\nUnlock a special drop in The Evilverse!",
            20 => "\n\n<b>Choco Set:</b>\nItems 221-225.\n\n<b>Completion Bonus (All items level 100):</b>\nUnlock 2 special drops in Chocolate World, plus a new MacGuffin! Also: Reduce the number of kills needed per MacGuffin drop outside of the ITOPOD by 10%! Chocolate is some powerful stuff.",
            21 => "\n\n<b>Pretty Pink Princess Set:</b>\nItems 231-236.\n\n<b>Completion Bonus (All items level 100):</b>\nEarn 10% more PP!",
            22 => "\n\n<b>Greasy Nerd Set:</b>\nItems 237-241.\n\n<b>Completion Bonus (All items level 100):</b>\nAll MacGuffins drop 1 level higher!",
            23 => "\n\n<b>Meta Set:</b>\nItems 251-257.\n\n<b>Completion Bonus (All items level 100):</b> +20% NGU Speed! Gotta get those Numbers Going Up!",
            24 => "\n\n<b>Party Set:</b>\nItems 258-264.\n\n<b>Completion Bonus (All items level 100):</b>+5% Global Digger Bonus!",
            25 => "\n\n<b>Mobster Set:</b>\nItems 265-271.\n\n<b>Completion Bonus (All items level 100):</b>+15% QP earned while Questing!",
            26 => "\n\n<b>Typo Set:</b>\nItems 301-307.\n\n<b>Completion Bonus (All items level 100):</b>+20% Wish Speed!",
            27 => "\n\n<b>Fad Set:</b>\nItems 308-314.\n\n<b>Completion Bonus (All items level 100):</b>10% Faster Major Quests!",
            28 => "\n\n<b>JRPG Set:</b>\nItems 315-321.\n\n<b>Completion Bonus (All items level 100):</b>A better Ultimate Attack!",
            29 => "\n\n<b>Exile Set:</b>\nItems 322-326.\n\n<b>Completion Bonus (All items level 100):</b>Unlocks something secret!",
            30 => "\n\n<b>Rad Set:</b>\nItems 345-351.\n\n<b>Completion Bonus (All items level 100):</b>+5 Max Deck Size! AND +1 to card tiers!",
            31 => "\n\n<b>Back To School Set:</b>\nItems 352-358.\n\n<b>Completion Bonus (All items level 100):</b>+15% NGU Speed!\n",
            32 => "\n\n<b>Western Set:</b>\nItems 359-365.\n\n<b>Completion Bonus (All items level 100):</b>An Extra Drop in this zone! As a plus, cube divider is set to be 4. This is normally 50 (25 with the wish)\n",
            33 => "\n\n<b>Space Set:</b>\nItems 373-379.\n\n<b>Completion Bonus (All items level 100):</b>+10% Cooking EXP Bonus!",
            34 => "\n\n<b>Bread Set:</b>\nItems 392-399.\n\n<b>Completion Bonus (All items level 100):</b>Faster Cooks!!",
            35 => "\n\n<b>Disco Set:</b>\nItems 400-407.\n\n<b>Completion Bonus (All items level 100):</b>Less crappy cards!",
            36 => "\n\n<b>Halloweenie Set:</b>\nItems 408-415.\n\n<b>Completion Bonus (All items level 100):</b>+45% PP gain!",
            37 => "\n\n<b>Rock Set:</b>\nItems 416-423.\n\n<b>Completion Bonus (All items level 100):</b>+2 tier to ALL CARDS!",
            38 => "\n\n<b>Construction Set:</b>\nItems 453-460.\n\n<b>Completion Bonus (All items level 100):</b>20% Boostier Boosts!",
            39 => "\n\n<b>Duck Set:</b>\nItems 496-503.\n\n<b>Completion Bonus (All items level 100):</b>+6% Mayo and Card Speed!",
            40 => "\n\n<b>Dutch Set:</b>\nItems 461-468.\n\n<b>Completion Bonus (All items level 100): Faster Ritual Speed? And double guff time factor.</b>",
            41 => "\n\n<b>Amalgamate Set:</b>\nItems 469-476.\n\n<b>Completion Bonus (All items level 100):</b> +10 Max Deck size\n\n+10% NGU Level Effectiveness\n\n+10% Yggdrasil Yield\n\n+10% Global Digger bonus\n\n10% Permanent Beard Level Effectiveness.",
            42 => "\n\n<b>Pirate Set:</b>\nItems 507-514.\n\n<b>Completion Bonus (All items level 100):</b> Pride and Accomplishment. Oh, and 25% NGU effectiveness.",
            1000 => "\n\n<b>Wandoos Set</b>\nJust this Item. Like, only this. Just level this up to 100, that's it. It's that simple.\n\n<b>Completion Bonus (All items level 100):</b>\n\nWhen Wandoos completes the booting process, gain an additional 10% speed bonus. Also, " + EXP(300L) + "!",
            1001 => "\n\n<b>Tutorial Cube Set:</b>\nJust this cube, easy peasy.\n\n<b>Completion Bonus (All items level 100):</b>\nSomething special!",
            1002 => "\n\n<b>Number Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+10% NGU Speed!",
            1003 => "\n\n<b>Flubber Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n" + AP(30000L) + "!",
            1004 => "\n\n<b>Seed Set ;):</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n10 Premium samples of Icarus Proudbottom's Homemade Boom Boom Fertilizers! Check out the Sellout shop for more info on what these poops do.",
            1005 => "\n\n<b>Armpit Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+10% Beard Speed!",
            1006 => "\n\n<b>Red Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nYou will receive the max heart EXP bonus (10%) even when the heart is not equipped!",
            1007 => "\n\n<b>Yellow Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nYou will receive the max heart AP bonus (20%) even when the heart is not equipped!",
            1008 => "\n\n<b>UUG's Rings Set:</b>\nItems 136-140\n\n<b>Completion Bonus (All items level 100):</b>\n" + EXP(20000L) + "\n" + AP(20000L) + "!" + "\nUnlock a new, super-ultra rare drop from UUG! ",
            1009 => "\n\n<b>Boosts Set:</b>\nItems 1-39\n\n<b>Completion Bonus (For each item maxxed):\n+2% permanent boosting power to ALL boosts!</b>",
            1010 => "\n\n<b>Red Liquid Set:</b>\nJust this thing.\n\n<b>Completion Bonus(All items level 100):</b>\n-10% on the global cooldown timer (0.7 attack speed), AND for idle attack speed! The global cooldown timer is the cooldown between using different moves, if you didn't know!",
            1011 => "\n\n<b>Brown Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nEvery 10th poop you use on a fruit will not be consumed!",
            1012 => "\n\n<b>Wandoos XL Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nWandoos now boots up 10% faster!",
            1013 => "\n\n<b>Green Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nGain 20% faster progress towards Perk Points (PP) in the I.T.O.P.O.D!",
            1014 => "\n\n<b>Pissed Off Key Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nGain 10% faster progress towards Perk Points (PP) in the I.T.O.P.O.D!",
            1015 => "\n\n<b>Purple Liquid Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nBEAST MODE now grants +50% to your Power, instead of +40%!",
            1016 => "\n\n<b>Blue Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nAll consumables give 10% better effects!",
            1017 => "\n\n<b>Scrap of Paper Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nGain a Digger Slot!",
            1018 => "\n\n<b>Purple Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nMacGuffins drop 20% more often, and 20% MacGuffin time factor!",
            1019 => "\n\n<b>Quest Items Set</b>\nItems 278-287\n\n<b>Completion Bonus (For each item maxxed):</b>\n+2% QP rewards in Questing!",
            1020 => "\n\n<b>Orange Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nQuests give 20% more QP!",
            1021 => "\n\n<b>Heroic Sigil Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nQuests Items drop 10% more often!",
            1022 => "\n\n<b>Grey Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n25% Faster Hacks!",
            1023 => "\n\n<b>Incriminating Evidence Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+2 base " + character.res3.res3Name + " Power\n+80K base " + character.res3.res3Name + " Cap\n+2 base " + character.res3.res3Name + " Bars\n+1 of every Resource 3 Potion!",
            1024 => "\n\n<b>Pink Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nGain an additional Wish slot!",
            1025 => "\n\n<b>Severed Head Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+13.37% Wish Speed!",
            1026 => "\n\n<b>Rainbow Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+10% Mayo and Card Generation Speed!",
            1027 => "\n\n<b>Still-Beating Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+1% Tag Effect!",
            1028 => "\n\n<b>Normal Bonus Accs Set:</b>\nItems 432-444.\n\n<b>Completion Bonus (All items level 100):</b>\n+25% Drop Chance",
            1029 => "\n\n<b>Evil Bonus Accs Set:</b>\nItems 445-452.\n\n<b>Completion Bonus (All items level 100):</b>\n+20% Adventure Stats!",
            _ => "\n\nCongratulations! If you see this, 4G messed something else up in the game too!"
        };


        if ((__instance.setID == 0 && character.inventory.itemList.trainingComplete) ||
            (__instance.setID == 1 && character.inventory.itemList.sewersComplete) ||
            (__instance.setID == 2 && character.inventory.itemList.forestComplete) ||
            (__instance.setID == 3 && character.inventory.itemList.caveComplete) ||
            (__instance.setID == 4 && character.inventory.itemList.HSBComplete) ||
            (__instance.setID == 5 && character.inventory.itemList.GRBComplete) ||
            (__instance.setID == 6 && character.inventory.itemList.clockComplete) ||
            (__instance.setID == 7 && character.inventory.itemList.twoDComplete) ||
            (__instance.setID == 8 && character.inventory.itemList.ghostComplete) ||
            (__instance.setID == 9 && character.inventory.itemList.jakeComplete) ||
            (__instance.setID == 10 && character.inventory.itemList.gaudyComplete) ||
            (__instance.setID == 11 && character.inventory.itemList.megaComplete) ||
            (__instance.setID == 12 && character.inventory.itemList.beardverseComplete) ||
            (__instance.setID == 13 && character.inventory.itemList.waldoComplete) ||
            (__instance.setID == 14 && character.inventory.itemList.antiWaldoComplete) ||
            (__instance.setID == 15 && character.inventory.itemList.badlyDrawnComplete) ||
            (__instance.setID == 16 && character.inventory.itemList.stealthComplete) ||
            (__instance.setID == 17 && character.inventory.itemList.beast1complete) ||
            (__instance.setID == 18 && character.inventory.itemList.edgyComplete) ||
            (__instance.setID == 19 && character.inventory.itemList.edgyBootsComplete) ||
            (__instance.setID == 20 && character.inventory.itemList.chocoComplete) ||
            (__instance.setID == 21 && character.inventory.itemList.prettyComplete) ||
            (__instance.setID == 22 && character.inventory.itemList.nerdComplete) ||
            (__instance.setID == 23 && character.inventory.itemList.metaComplete) ||
            (__instance.setID == 24 && character.inventory.itemList.partyComplete) ||
            (__instance.setID == 25 && character.inventory.itemList.godmotherComplete) ||
            (__instance.setID == 26 && character.inventory.itemList.typoComplete) ||
            (__instance.setID == 27 && character.inventory.itemList.fadComplete) ||
            (__instance.setID == 28 && character.inventory.itemList.jrpgComplete) ||
            (__instance.setID == 29 && character.inventory.itemList.exileComplete) ||
            (__instance.setID == 30 && character.inventory.itemList.radComplete) ||
            (__instance.setID == 31 && character.inventory.itemList.schoolComplete) ||
            (__instance.setID == 32 && character.inventory.itemList.westernComplete) ||
            (__instance.setID == 33 && character.inventory.itemList.spaceComplete) ||
            (__instance.setID == 34 && character.inventory.itemList.breadverseComplete) ||
            (__instance.setID == 35 && character.inventory.itemList.that70sComplete) ||
            (__instance.setID == 36 && character.inventory.itemList.halloweeniesComplete) ||
            (__instance.setID == 37 && character.inventory.itemList.rockLobsterComplete) ||
            (__instance.setID == 38 && character.inventory.itemList.constructionComplete) ||
            (__instance.setID == 39 && character.inventory.itemList.duckComplete) ||
            (__instance.setID == 40 && character.inventory.itemList.netherComplete) ||
            (__instance.setID == 41 && character.inventory.itemList.amalgamateComplete) ||
            (__instance.setID == 42 && character.inventory.itemList.pirateComplete) ||
            (__instance.setID == 1000 && character.inventory.itemList.wandoosComplete) ||
            (__instance.setID == 1001 && character.inventory.itemList.tutorialCubeComplete) ||
            (__instance.setID == 1002 && character.inventory.itemList.numberComplete) ||
            (__instance.setID == 1003 && character.inventory.itemList.flubberComplete) ||
            (__instance.setID == 1004 && character.inventory.itemList.seedComplete) ||
            (__instance.setID == 1005 && character.inventory.itemList.uugComplete) ||
            (__instance.setID == 1006 && character.inventory.itemList.itemMaxxed[119]) ||
            (__instance.setID == 1007 && character.inventory.itemList.itemMaxxed[129]) ||
            (__instance.setID == 1008 && character.inventory.itemList.uugRingComplete) ||
            (__instance.setID == 1009 && character.inventory.itemList.itemMaxxed[__instance.id]) ||
            (__instance.setID == 1010 && character.inventory.itemList.itemMaxxed[93]) ||
            (__instance.setID == 1011 && character.inventory.itemList.itemMaxxed[162]) ||
            (__instance.setID == 1012 && character.inventory.itemList.xlComplete) ||
            (__instance.setID == 1013 && character.inventory.itemList.greenHeartComplete) ||
            (__instance.setID == 1014 && character.inventory.itemList.itopodKeyComplete) ||
            (__instance.setID == 1015 && character.inventory.itemList.purpleLiquidComplete) ||
            (__instance.setID == 1016 && character.inventory.itemList.blueHeartComplete) ||
            (__instance.setID == 1017 && character.inventory.itemList.jakeNoteComplete) ||
            (__instance.setID == 1018 && character.inventory.itemList.purpleHeartComplete) ||
            (__instance.setID == 1019 && character.inventory.itemList.itemMaxxed[__instance.id]) ||
            (__instance.setID == 1020 && character.inventory.itemList.orangeHeartComplete) ||
            (__instance.setID == 1021 && character.inventory.itemList.sigilComplete) ||
            (__instance.setID == 1022 && character.inventory.itemList.greyHeartComplete) ||
            (__instance.setID == 1023 && character.inventory.itemList.evidenceComplete) ||
            (__instance.setID == 1024 && character.inventory.itemList.pinkHeartComplete) ||
            (__instance.setID == 1025 && character.inventory.itemList.severedHeadComplete) ||
            (__instance.setID == 1026 && character.inventory.itemList.rainbowHeartComplete) ||
            (__instance.setID == 1027 && character.inventory.itemList.beatingHeartComplete) ||
            (__instance.setID == 1028 && character.inventory.itemList.normalBonusAccComplete) ||
            (__instance.setID == 1029 && character.inventory.itemList.evilBonusAccComplete))
        {
            text += "<color=green><b> COMPLETE</b></color>";
        }

        __result = text;
        return false;
    }
        private static string SaveScopedKey(Character c, string key)
        {
            string name = "unknown";
            string plat = "unknownPlat";

            try
            {
                if (c != null && !string.IsNullOrEmpty(c.playerName))
                    name = c.playerName;
            }
            catch { }

            try
            {
                if (c != null)
                    plat = c.platform.ToString();
            }
            catch { }

            return $"{key}.{plat}.{name}";
        }

        private static bool GetFlag(Character c, string key)
            => PlayerPrefs.GetInt(SaveScopedKey(c, key), 0) != 0;

        private static void SetFlag(Character c, string key)
        {
            PlayerPrefs.SetInt(SaveScopedKey(c, key), 1);
            PlayerPrefs.Save();
        }

        [HarmonyPatch]
        internal static class CardTierSetExtras
        {
            private static Character _character;

            // ---- Item IDs ----
            private const int T11V2_A = 424;
            private const int T11V2_B = 425;
            private const int T11V3_A = 426;
            private const int T11V3_B = 427;
            private const int T11V4_A = 428;
            private const int T11V4_B = 429;

            // ---- Combined one-time tooltip flag ----
            private const string PP_T11_COMBINED = "fasterPace.extraComplete.t11_cards_combined";

            [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
            private static void CacheCharacter(AdventureController __instance)
            {
                _character = __instance?.character;
            }

            private static Character C => _character;

            private static bool IsMaxxed(Character c, int itemId)
            {
                if (c?.inventory?.itemList == null) return false;

                var maxxed = c.inventory.itemList.itemMaxxed as IList<bool>;
                if (maxxed == null) return false;
                if (itemId < 0 || itemId >= maxxed.Count) return false;

                return maxxed[itemId];
            }

            private static bool IsCombinedSetComplete(Character c)
            {
                return IsMaxxed(c, T11V2_A)
                    && IsMaxxed(c, T11V2_B)
                    && IsMaxxed(c, T11V3_A)
                    && IsMaxxed(c, T11V3_B)
                    && IsMaxxed(c, T11V4_A)
                    && IsMaxxed(c, T11V4_B);
            }

            private static bool IsTrackedItem(int id)
            {
                return id == T11V2_A || id == T11V2_B
                    || id == T11V3_A || id == T11V3_B
                    || id == T11V4_A || id == T11V4_B;
            }

            private static string BuildCombinedSetBlock(bool complete)
            {
                string status = complete
                    ? "\n<color=green><b>COMPLETE</b></color>"
                    : "\n<color=red><b>NOT COMPLETE</b></color>";

                return
                    "\n\n<b>T11 Harder Version Set:</b>\n" +
                    "Items 424, 425, 426, 427, 428 & 429.\n\n" +
                    "<b>Completion Bonus (All items level 100):</b>\n" +
                    "+5 Global Card Tier." +
                    status;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "getGlobalTierBonus")]
            private static void CardsController_getGlobalTierBonus_Postfix(ref int __result)
            {
                var c = C;
                if (c?.inventory?.itemList == null) return;

                if (IsCombinedSetComplete(c))
                    __result += 5;
                if (c.inventory.itemList.radComplete)
                    __result += 1;
                if (c.inventory.itemList.rockLobsterComplete)
                    __result += 1;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(ItemListController), "setBonusText")]
            private static void ItemListController_setBonusText_Postfix(ItemListController __instance, ref string __result)
            {
                if (__instance == null) return;

                var c = C;
                if (c == null) return;

                if (!IsTrackedItem(__instance.id))
                    return;

                // avoid duplicate insertion
                if (!string.IsNullOrEmpty(__result) && __result.Contains("T11 Combined Card Tier Set"))
                    return;

                bool complete = IsCombinedSetComplete(c);
                __result = (__result ?? "") + BuildCombinedSetBlock(complete);
            }

            [HarmonyPostfix, HarmonyPatch(typeof(AllItemListController), "checkforBonuses")]
            private static void AllItemListController_checkforBonuses_Postfix(AllItemListController __instance)
            {
                var c = __instance?.character;
                if (c?.inventory?.itemList == null) return;

                if (!GetFlag(c, PP_T11_COMBINED) && IsCombinedSetComplete(c))
                {
                    SetFlag(c, PP_T11_COMBINED);
                    __instance.tooltip.showOverrideTooltip(
                        "You've maxxed out the combined T11 Harder Version Set, congrats!\n\n" +
                        "You gained +5 Global Card Tier.",
                        5f
                    );

                    c.refreshMenus();
                }
            }
        }

        [HarmonyPatch]
        internal static class CookingT10ItemBonuses
        {
            private static readonly int[] Items = { 381, 382, 383, 384, 385, 386 };

            private const string CombinedKey = "fasterPace.cook10.combined";

            private static string SaveScopedKey(Character c, string key)
            {
                string name = "unknown";
                string plat = "unknownPlat";

                try
                {
                    if (c != null && !string.IsNullOrEmpty(c.playerName))
                        name = c.playerName;
                }
                catch { }

                try
                {
                    if (c != null)
                        plat = c.platform.ToString();
                }
                catch { }

                return $"{key}.{plat}.{name}";
            }

            private static bool GetFlag(Character c, string key)
                => PlayerPrefs.GetInt(SaveScopedKey(c, key), 0) != 0;

            private static void SetFlag(Character c, string key)
            {
                PlayerPrefs.SetInt(SaveScopedKey(c, key), 1);
                PlayerPrefs.Save();
            }

            private static bool IsMaxxed(Character c, int itemId)
            {
                if (c?.inventory?.itemList == null) return false;

                var maxxed = c.inventory.itemList.itemMaxxed as IList<bool>;
                if (maxxed == null) return false;
                if (itemId < 0 || itemId >= maxxed.Count) return false;

                return maxxed[itemId];
            }

            private static bool IsTrackedItem(int id)
            {
                for (int i = 0; i < Items.Length; i++)
                    if (Items[i] == id)
                        return true;

                return false;
            }

            private static bool IsCombinedSetComplete(Character c)
            {
                if (c?.inventory?.itemList == null) return false;

                for (int i = 0; i < Items.Length; i++)
                {
                    if (!IsMaxxed(c, Items[i]))
                        return false;
                }

                return true;
            }

            private static string BuildCombinedSetBlock(bool complete)
            {
                string status = complete
                    ? "\n<color=green><b>COMPLETE</b></color>"
                    : "\n<color=red><b>NOT COMPLETE</b></color>";

                return
                    "\n\n<b>T10 Harder Version Set:</b>\n" +
                    "Items 381, 382, 383, 384, 385 & 386.\n\n" +
                    "<b>Completion Bonus (All items level 100):</b>\n" +
                    "+50% Cooking Bonus." +
                    status;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CookingController), "totalCookingBonuses")]
            private static void CookingController_totalCookingBonuses_Postfix(CookingController __instance, ref float __result)
            {
                var c = __instance?.character;
                if (c == null) return;

                if (IsCombinedSetComplete(c))
                    __result *= 1.5f;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ItemListController), "setBonusText")]
            private static void ItemListController_setBonusText_Postfix(ItemListController __instance, ref string __result)
            {
                if (__instance == null) return;

                int id = __instance.id;
                if (!IsTrackedItem(id))
                    return;

                if (!string.IsNullOrEmpty(__result) && __result.Contains("T10 Combined Cooking Set"))
                    return;

                var c = __instance.character;
                bool complete = IsCombinedSetComplete(c);

                __result = (__result ?? "") + BuildCombinedSetBlock(complete);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(AllItemListController), "checkforBonuses")]
            private static void AllItemListController_checkforBonuses_Postfix(AllItemListController __instance)
            {
                var c = __instance?.character;
                if (c?.inventory?.itemList == null) return;

                if (!GetFlag(c, CombinedKey) && IsCombinedSetComplete(c))
                {
                    SetFlag(c, CombinedKey);
                    __instance.tooltip.showOverrideTooltip(
                        "You've maxxed out the combined T10 Harder Version Set, congrats!\n\n" +
                        "You gained +50% Cooking Bonus.",
                        5f
                    );

                    c.refreshMenus();
                }
            }
        }

        [HarmonyPatch]
        internal static class T12HarderVersionFillsPerBarSet
        {
            private static readonly int[] Items = { 477, 478, 479 };

            private const string CombinedKey = "fasterPace.t12harder.fillsperbar.combined";

            private static string SaveScopedKey(Character c, string key)
            {
                string name = "unknown";
                string plat = "unknownPlat";

                try
                {
                    if (c != null && !string.IsNullOrEmpty(c.playerName))
                        name = c.playerName;
                }
                catch { }

                try
                {
                    if (c != null)
                        plat = c.platform.ToString();
                }
                catch { }

                return $"{key}.{plat}.{name}";
            }

            private static bool GetFlag(Character c, string key)
                => PlayerPrefs.GetInt(SaveScopedKey(c, key), 0) != 0;

            private static void SetFlag(Character c, string key)
            {
                PlayerPrefs.SetInt(SaveScopedKey(c, key), 1);
                PlayerPrefs.Save();
            }

            private static bool IsMaxxed(Character c, int itemId)
            {
                if (c?.inventory?.itemList == null) return false;

                var maxxed = c.inventory.itemList.itemMaxxed as IList<bool>;
                if (maxxed == null) return false;
                if (itemId < 0 || itemId >= maxxed.Count) return false;

                return maxxed[itemId];
            }

            private static bool IsTrackedItem(int id)
            {
                for (int i = 0; i < Items.Length; i++)
                    if (Items[i] == id)
                        return true;

                return false;
            }

            private static bool IsCombinedSetComplete(Character c)
            {
                if (c?.inventory?.itemList == null) return false;

                for (int i = 0; i < Items.Length; i++)
                {
                    if (!IsMaxxed(c, Items[i]))
                        return false;
                }

                return true;
            }

            private static string BuildCombinedSetBlock(bool complete)
            {
                string status = complete
                    ? "\n<color=green><b>COMPLETE</b></color>"
                    : "\n<color=red><b>NOT COMPLETE</b></color>";

                return
                    "\n\n<b>T12 Harder Version Set:</b>\n" +
                    "Items 477, 478 & 479.\n\n" +
                    "<b>Completion Bonus (All items level 100):</b>\n" +
                    "Fills Per Bar is increased from 6 to 12." +
                    status;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ItemListController), "setBonusText")]
            private static void ItemListController_setBonusText_Postfix(ItemListController __instance, ref string __result)
            {
                if (__instance == null) return;

                int id = __instance.id;
                if (!IsTrackedItem(id))
                    return;

                if (!string.IsNullOrEmpty(__result) && __result.Contains("T12 Harder Version Set"))
                    return;

                var c = __instance.character;
                bool complete = IsCombinedSetComplete(c);

                __result = (__result ?? "") + BuildCombinedSetBlock(complete);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(AllItemListController), "checkforBonuses")]
            private static void AllItemListController_checkforBonuses_Postfix(AllItemListController __instance)
            {
                var c = __instance?.character;
                if (c?.inventory?.itemList == null) return;

                if (!GetFlag(c, CombinedKey) && IsCombinedSetComplete(c))
                {
                    SetFlag(c, CombinedKey);
                    __instance.tooltip.showOverrideTooltip(
                        "You've maxxed out the combined T12 Harder Version Set, congrats!\n\n" +
                        "Fills Per Bar is now 12 instead of 6.",
                        5f
                    );

                    c.refreshMenus();
                }
            }
        }

        internal static class FPSetBonuses
        {
            internal static bool T12HarderComplete(Character ch)
            {
                var maxxed = ch?.inventory?.itemList?.itemMaxxed as IList<bool>;
                return maxxed != null
                    && maxxed.Count > 479
                    && maxxed[477]
                    && maxxed[478]
                    && maxxed[479];
            }
        }
        private static string ExpText(long gained) => character.display(gained) + " EXP";
        private static string ApText(long gained) => character.display(gained) + " AP";


        [HarmonyPrefix, HarmonyPatch(typeof(AllItemListController), "checkforBonuses")]
        private static bool checkforBonuses(AllItemListController __instance)
        {
            if (!character.inventory.itemList.trainingComplete && character.inventory.itemList.maxxedTraining())
            {
                character.inventory.itemList.trainingComplete = true;
                character.inventoryController.updateInvCount();
                character.energySpeed += 2f;

                long exp = character.addExp(10L);

                __instance.tooltip.showTooltip(
                    "You've maxxed out every item in the training set, congrats! You've been awarded 12 Inventory Space, 2 Energy Speed and "
                    + ExpText(exp)
                    + "! You also unlocked a new player portrait in the Fight Boss Menu!",
                    5f);

                character.portraits.portraitUnlocked[11] = true;
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.sewersComplete && character.inventory.itemList.maxxedSewers())
            {
                character.inventory.itemList.sewersComplete = true;
                character.inventoryController.updateInvCount();
                character.adventure.attack += 5f;
                character.adventure.defense += 5f;
                character.adventure.maxHP += 15f;
                character.adventure.regen += 0.2f;

                long exp = character.addExp(20L);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the sewers set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also been awarded:\n+5 to Power and Toughness\n15 max Health\n0.2 regen\n12 Inventory Space\nAnd  "
                    + ExpText(exp)
                    + "!",
                    5f);

                character.portraits.portraitUnlocked[12] = true;
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.forestComplete && character.inventory.itemList.maxxedForest())
            {
                character.inventory.itemList.forestComplete = true;
                character.inventoryController.updateInvCount();
                character.arbitrary.energyPotion1Count += 2;
                character.arbitrary.energyPotion2Count += 2;
                character.arbitrary.energyBarBar1Count += 2;
                character.energyPower += 5f;

                long exp = character.addExp(200L);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the forest set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n\n2 Energy Potion α\n2 Energy Potion β\n2 Energy Bar Bar\n5 Energy Power\n12 Inventory Space\nAnd "
                    + ExpText(exp)
                    + "!",
                    5f);

                character.portraits.portraitUnlocked[13] = true;
                character.portraits.portraitUnlocked[14] = true;
                character.portraits.portraitUnlocked[15] = true;
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.caveComplete && character.inventory.itemList.maxxedCave())
            {
                character.inventory.itemList.caveComplete = true;
                character.inventoryController.updateInvCount();

                long exp = character.addExp(300L);

                character.magic.magicPower += 2f;
                character.magic.capMagic += 40000L;
                character.magic.magicPerBar += 2L;
                character.portraits.portraitUnlocked[16] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the cave set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n2 Magic Power\n40,000 Magic Cap\n2 Magic Per Bar\n12 Inventory Space\nAnd "
                    + ExpText(exp)
                    + "!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.HSBComplete && character.inventory.itemList.maxxedHSB())
            {
                character.inventory.itemList.HSBComplete = true;

                long exp = character.addExp(500L);

                character.magic.magicPerBar += 3L;
                character.magic.magicPower += 3f;
                character.magic.capMagic += 30000L;
                character.arbitrary.magicBarBar1Count++;
                character.arbitrary.magicPotion1Count++;
                character.arbitrary.magicPotion2Count++;
                character.portraits.portraitUnlocked[17] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the HSB set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n3 Magic Power\n30000 magic Cap\n3 Magic Bars\n1 Magic Bar Bar\n1 Magic Potion α\n1 Magic Potion β\n12 Inventory Space\nAnd "
                    + ExpText(exp)
                    + "!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.GRBComplete && character.inventory.itemList.maxxedGRB())
            {
                character.inventory.itemList.GRBComplete = true;

                long exp = character.addExp(2000L);

                character.portraits.portraitUnlocked[18] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the GRB set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp)
                    + "!\n Also, the Safe Zone will now provide a 10x HP Regen boost instead of 5x!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.clockComplete && character.inventory.itemList.maxxedClock())
            {
                character.inventory.itemList.clockComplete = true;

                long exp = character.addExp(1000L);

                character.portraits.portraitUnlocked[19] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Clockwork set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp)
                    + "!\n Also, enemies will now spawn 5% Faster!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.twoDComplete && character.inventory.itemList.maxxed2D())
            {
                character.inventory.itemList.twoDComplete = true;

                long exp = character.addExp(2000L);

                character.portraits.portraitUnlocked[20] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the 2D set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp)
                    + "!\nYour drop chance in Adventure has permanently increased by 7.43%! Why that weird number? Ask room 1 of the NGU Idle chat!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.ghostComplete && character.inventory.itemList.maxxedGhost())
            {
                character.inventory.itemList.ghostComplete = true;

                long exp = character.addExp(3000L);

                character.portraits.portraitUnlocked[21] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Ghost set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp)
                    + "!\nAlso, Idle attack now has the damage multiplier of regular Attack!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.jakeComplete && character.inventory.itemList.maxxedJake())
            {
                character.inventory.itemList.jakeComplete = true;

                long exp = character.addExp(7000L);

                character.portraits.portraitUnlocked[22] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Jake set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp)
                    + "!\nAlso, you've unlocked wandoos MEH!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.gaudyComplete && character.inventory.itemList.maxxedGaudy())
            {
                character.inventory.itemList.gaudyComplete = true;

                long exp = character.addExp(5000L);

                character.arbitrary.lootCharm1Count += 2;
                character.portraits.portraitUnlocked[23] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Gaudy set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp)
                    + "!\n2 Lucky charms!\nAlso, item drop one level higher now!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.megaComplete && character.inventory.itemList.maxxedMega())
            {
                character.inventory.itemList.megaComplete = true;

                long exp = character.addExp(6000L);

                character.portraits.portraitUnlocked[24] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Mega set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp)
                    + "!\nAlso, Charge attack now gives a 2.2x bonus to your next move, instead of 2.0!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.beardverseComplete && character.inventory.itemList.maxxedBeardverse())
            {
                character.inventory.itemList.beardverseComplete = true;

                long exp = character.addExp(8000L);

                character.portraits.portraitUnlocked[25] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You completed the Beardverse Set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp)
                    + "!\nAlso, Equipping multiple beards that use Energy or Magic at the same time have a 10% reduced penalty to levelling speed!",
                    4f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.uugRingComplete && character.inventory.itemList.maxxedRingUUG())
            {
                character.inventory.itemList.uugRingComplete = true;

                long exp = character.addExp(20000L);
                long ap = character.addAP(20000);

                __instance.tooltip.showOverrideTooltip(
                    "You completed the UUG's Rings Set, congrats! You've been awarded:\n"
                    + ExpText(exp) + "!\n"
                    + ApText(ap) + "!\n"
                    + "Also, you've unlocked a sixth and ULTRA rare ring drop from UUG! Happy grinding! :D",
                    4f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.wandoosComplete && character.inventory.itemList.maxxedWandoos())
            {
                character.inventory.itemList.wandoosComplete = true;

                long exp = character.addExp(300L);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out your Wandoos item, congrats! You've been awarded "
                    + ExpText(exp)
                    + " and a special little perk: When wandoos finishes booting up, you will receive a 10% bonus to its leveling speed! And sure, let's add 300 EXP to the pot.",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.tutorialCubeComplete && character.inventory.itemList.maxxedTutorialCube())
            {
                character.inventory.itemList.tutorialCubeComplete = true;

                long ap = character.addAP(10000);

                __instance.tooltip.showOverrideTooltip(
                    "You've merged the Tutorial Cube to level 100, and it cracks open! You look inside and see... :o! "
                    + ApText(ap)
                    + "! These meaningless points can buy cool items in the 4G Sellout shop! You'll earn AP for a lot of things as you continue to play.",
                    8f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.numberComplete && character.inventory.itemList.maxxedNumber())
            {
                character.inventory.itemList.numberComplete = true;
                __instance.tooltip.showOverrideTooltip("For creating a level 100 Number, you've been awarded a 10% speed boost to the NGU feature!", 4f);
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.flubberComplete && character.inventory.itemList.maxxedFlubber())
            {
                character.inventory.itemList.flubberComplete = true;

                long ap = character.addAP(30000);

                __instance.tooltip.showOverrideTooltip(
                    "For creating a level 100 Triple Flubber (:o), you've been awarded "
                    + ApText(ap)
                    + "!",
                    4f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.seedComplete && character.inventory.itemList.maxxedSeed())
            {
                character.inventory.itemList.seedComplete = true;
                __instance.tooltip.showOverrideTooltip("For creating a level 100 Seed (:o), you've been awarded 10 Premium samples of Icarus Proudbottom's Homeamde Boom Boom Fertilizers! Check out the Sellout shop for more info on what these poops do.", 4f);
                character.arbitrary.poop1Count += 10;
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.uugComplete && character.inventory.itemList.maxxedUUG())
            {
                character.inventory.itemList.uugComplete = true;
                __instance.tooltip.showOverrideTooltip("For creating a level 100 piece of Armpit Hair (gross), you've been awarded a 10% boost to your Beard Speed!", 4f);
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.redLiquidComplete && character.inventory.itemList.maxxedRedLiquid())
            {
                character.inventory.itemList.redLiquidComplete = true;
                character.adventure.setFasterIdleAttack();
                __instance.tooltip.showOverrideTooltip("For creating a level 100 Red Liquid, the global cooldown timer is reduced by 10%! Yes, this also means Idle Attack (0.7)! Yaaaaay!", 4f);
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.waldoComplete && character.inventory.itemList.maxxedWaldo())
            {
                character.inventory.itemList.waldoComplete = true;

                long exp = character.addExp(50000L);
                long ap = character.addAP(10000);

                character.portraits.portraitUnlocked[26] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Wanderer's set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp) + "!\n"
                    + ApText(ap) + "!\n"
                    + "A new, ultra-rare accessory can now drop from WALDERP!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.antiWaldoComplete && character.inventory.itemList.maxxedAntiWaldo())
            {
                character.inventory.itemList.antiWaldoComplete = true;

                long exp = character.addExp(50000L);
                long ap = character.addAP(10000);

                character.portraits.portraitUnlocked[27] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the s'rerednaW set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp) + "!\n"
                    + ApText(ap) + "!\n"
                    + "A new, ultra-rare accessory can now drop from WALDERP!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.badlyDrawnComplete && character.inventory.itemList.maxxedBadlyDrawn())
            {
                character.inventory.itemList.badlyDrawnComplete = true;
                character.portraits.portraitUnlocked[28] = true;

                long exp = character.addExp(30000L);
                long ap = character.addAP(5000);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Badly Drawn set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp) + "!\n"
                    + ApText(ap) + "!\n"
                    + "Boosts now provide 20% more boostification! (Hey, that's not even a word!)",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.stealthComplete && character.inventory.itemList.maxxedStealth())
            {
                character.inventory.itemList.stealthComplete = true;
                character.portraits.portraitUnlocked[29] = true;

                long exp = character.addExp(50000L);
                long ap = character.addAP(10000);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Stealth Set set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp) + "!\n"
                    + ApText(ap) + "!\n"
                    + "You can now also find a SUPER rare chest drop in Boring-Ass Earth!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.beast1complete && character.inventory.itemList.maxxedBeast1())
            {
                character.inventory.itemList.beast1complete = true;
                character.portraits.portraitUnlocked[30] = true;

                long exp = character.addExp(100000L);
                long ap = character.addAP(10000);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Beast Set set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + ExpText(exp) + "!\n"
                    + ApText(ap) + "!\n"
                    + "Parry now performs an attack that does 3x the damage!",
                    5f);

                character.refreshMenus();
            }
            else if (!character.inventory.itemList.brownHeartComplete && character.inventory.itemList.maxxedBrownHeart())
            {
                character.inventory.itemList.brownHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Brown Heart Set, Congrats. Now, once every 10 poops you use on a fruit will not be consumed!", 5f);
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.xlComplete && character.inventory.itemList.maxxedXL())
            {
                character.inventory.itemList.xlComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Wandoos XL Set, Congrats! Wandoos bootup time has now been reduced by 10%!", 5f);
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.brownHeartComplete && character.inventory.itemList.maxxedBrownHeart())
            {
                character.inventory.itemList.brownHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Brown Heart Set, Congrats. Now, once every 10 poops you use on a fruit will not be consumed!", 5f);
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.itopodKeyComplete && character.inventory.itemList.maxxedItopodKey())
            {
                character.inventory.itemList.itopodKeyComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Pissed Off Key Set, Congrats! Progress towards your next Perk Point (PP) in the I.T.O.P.O.D is now 10% faster!", 5f);
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.purpleLiquidComplete && character.inventory.itemList.maxxedPurpleLiquid())
            {
                character.inventory.itemList.purpleLiquidComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Purple Liquid Set, Congrats! Beast Mode will now increase your power by 50% instead of 40%!", 5f);
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.blueHeartComplete && character.inventory.itemList.maxxedBlueHeart())
            {
                character.inventory.itemList.blueHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Blue Heart Set, Congrats! All consumables now grant 10% better effects!", 5f);
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.jakeNoteComplete && character.inventory.itemList.maxxedJakeNote())
            {
                character.inventory.itemList.jakeNoteComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Scrap of Paper Set, Congrats! You earned a new digger slot!", 5f);
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.purpleHeartComplete && character.inventory.itemList.maxxedPurpleHeart())
            {
                character.inventory.itemList.purpleHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Purple Heart Set, Congrats! All MacGuffins will now drop 20% more often, and MacGuffin time factor is increased by 20%", 5f);
                character.refreshMenus();
            }
            else if (!character.inventory.itemList.greenHeartComplete && character.inventory.itemList.maxxedGreenHeart())
            {
                character.inventory.itemList.greenHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Green Heart Set, Congrats! Progress towards your next Perk Point (PP) in the I.T.O.P.O.D is now 20% faster!", 5f);
                character.refreshMenus();
            }
            if (!character.inventory.itemList.edgyComplete && character.inventory.itemList.maxxedEdgy())
            {
                character.inventory.itemList.edgyComplete = true;
                character.addExp(250000L);
                character.portraits.portraitUnlocked[32] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Edgy Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded " + character.display((double)character.checkExpAdded(250000L)) + " EXP! You also gained a free MacGuffin slot!", 5f);
                character.inventoryController.updateMacguffinCount();
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.edgyBootsComplete && character.inventory.itemList.maxxedEdgyBoots())
            {
                character.inventory.itemList.edgyBootsComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Boots Set, Congrats! You've unlocked a special drop in The Evilverse!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.chocoComplete && character.inventory.itemList.maxxedChoco())
            {
                character.inventory.itemList.chocoComplete = true;
                character.portraits.portraitUnlocked[31] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Choco Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've unlocked:\n\n2 rare accessory drops in Chocolate World!\nA new MacGuffin drops in Chocolate World!\nMacGuffins require 10% fewer kills per drop outside of the ITOPOD!\n\nChocolate is awesome!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.prettyComplete && character.inventory.itemList.maxxedPretty())
            {
                character.inventory.itemList.prettyComplete = true;
                character.portraits.portraitUnlocked[33] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Pretty Pink Princess Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You'll now earn PP 10% Faster", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.nerdComplete && character.inventory.itemList.maxxedNerd())
            {
                character.inventory.itemList.nerdComplete = true;
                character.portraits.portraitUnlocked[34] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Greasy Nerd Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! Now every MacGuffin will drop 1 level higher!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.metaComplete && character.inventory.itemList.maxxedMeta())
            {
                character.inventory.itemList.metaComplete = true;
                character.portraits.portraitUnlocked[35] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Meta Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've gained +20% NGU Speed!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.partyComplete && character.inventory.itemList.maxxedParty())
            {
                character.inventory.itemList.partyComplete = true;
                character.portraits.portraitUnlocked[36] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Party Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've gained +5% to your Global Digger Bonus!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.godmotherComplete && character.inventory.itemList.maxxedGodmother())
            {
                character.inventory.itemList.godmotherComplete = true;
                character.portraits.portraitUnlocked[37] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Mobster Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! Quests will now reward 15% more QP!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.orangeHeartComplete && character.inventory.itemList.maxxedOrangeHeart())
            {
                character.inventory.itemList.orangeHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Orange Heart Set, Congrats! Quests will now reward 20% more QP!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.sigilComplete && character.inventory.itemList.maxxedHeroicSigil())
            {
                character.inventory.itemList.sigilComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Heroic Sigil Set, congrats! Quest Items will now drop 10% more often.", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.greyHeartComplete && character.inventory.itemList.maxxedGreyHeart())
            {
                character.inventory.itemList.greyHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Grey Heart Set, Congrats! Now your Hacks will be 25% faster", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.evidenceComplete && character.inventory.itemList.maxxedEvidence() && character.res3.res3On)
            {
                character.inventory.itemList.evidenceComplete = true;
                character.res3.res3Power += 2f;
                character.res3.capRes3 += 80000L;
                character.res3.res3PerBar += 2L;
                character.arbitrary.res3Potion1Count++;
                character.arbitrary.res3Potion2Count++;
                character.arbitrary.res3Potion3Count++;
                __instance.tooltip.showOverrideTooltip(string.Concat(new string[]
                {
                        "You've maxxed out every item in the Incriminating Evidence Set, Congrats! You've gained +2 base ",
                        character.res3.res3Name,
                        " Power, 80K base",
                        character.res3.res3Name,
                        " Cap and +2 base ",
                        character.res3.res3Name,
                        " Bars! you also gained +1 to each Resource 3 Potion"
                }), 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.typoComplete && character.inventory.itemList.maxxedTypo())
            {
                character.inventory.itemList.typoComplete = true;
                character.portraits.portraitUnlocked[38] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Typo set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also gained +20% Wish Speed!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.fadComplete && character.inventory.itemList.maxxedFad())
            {
                character.inventory.itemList.fadComplete = true;
                character.arbitrary.beastButterCount += 3;
                character.portraits.portraitUnlocked[39] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Fad set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You'll now gain Major Quests 10% faster! You also gained 3 Beast Butter!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.jrpgComplete && character.inventory.itemList.maxxedJRPG())
            {
                character.inventory.itemList.jrpgComplete = true;
                character.portraits.portraitUnlocked[40] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the JRPG set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! Your ultimate attack is even more ultimate-r now!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.exileComplete && character.inventory.itemList.maxxedExile())
            {
                character.inventory.itemList.exileComplete = true;
                character.portraits.portraitUnlocked[41] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Exile set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You gained nothing else though...or have you? Up to you to figure out this mystery!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.pinkHeartComplete && character.inventory.itemList.maxxedPinkHeart())
            {
                character.inventory.itemList.pinkHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Pink Heart set, Congrats! You unlocked an additional Wish Slot!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.severedHeadComplete && character.inventory.itemList.maxxedSeveredHead())
            {
                character.inventory.itemList.severedHeadComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Severed Head set, Congrats! You've gained +13.37% Wish Speed! Wouldn't this joke be better used for Hacks though?", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.radComplete && character.inventory.itemList.maxxedRad())
            {
                character.inventory.itemList.radComplete = true;
                character.portraits.portraitUnlocked[47] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Rad set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! you've also gained +5 Max Deck Size", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.schoolComplete && character.inventory.itemList.maxxedSchool())
            {
                character.inventory.itemList.schoolComplete = true;
                character.portraits.portraitUnlocked[48] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Back To School set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! you also gained +15% NGU Speed", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.westernComplete && character.inventory.itemList.maxxedWestern())
            {
                character.inventory.itemList.westernComplete = true;
                character.portraits.portraitUnlocked[49] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Western set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also unlocked a new drop in The West World AND Cube Divider is 4 instead of 50!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.spaceComplete && character.inventory.itemList.maxxedSpace())
            {
                character.inventory.itemList.spaceComplete = true;
                character.portraits.portraitUnlocked[50] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Space set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also gained 10% improved cook results!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.rainbowHeartComplete && character.inventory.itemList.maxxedRainbowHeart())
            {
                character.inventory.itemList.rainbowHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Rainbow Heart set, Congrats! You've gained +10% Mayo and Card Generation Speed!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.beatingHeartComplete && character.inventory.itemList.maxxedBeatingHeart())
            {
                character.inventory.itemList.beatingHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Still-Beating Heart set, Congrats! You've gained +1% Tag Effect!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.breadverseComplete && character.inventory.itemList.maxxedBread())
            {
                character.inventory.itemList.breadverseComplete = true;
                character.portraits.portraitUnlocked[52] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Bread set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You can now eat 30 minutes faster!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.that70sComplete && character.inventory.itemList.maxxed70sZone())
            {
                character.inventory.itemList.that70sComplete = true;
                character.portraits.portraitUnlocked[53] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Disco set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You also generate slightly less crappier cards!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.halloweeniesComplete && character.inventory.itemList.maxxedHalloweenies())
            {
                character.inventory.itemList.halloweeniesComplete = true;
                character.portraits.portraitUnlocked[54] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Halloweenies set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also earned +45% PP gain!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.rockLobsterComplete && character.inventory.itemList.maxxedRockLobster())
            {
                character.inventory.itemList.rockLobsterComplete = true;
                character.portraits.portraitUnlocked[55] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Rock Lobster set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You also gained +2 tier to ALL CARDS. Hoo yeah!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.constructionComplete && character.inventory.itemList.maxxedConstruction())
            {
                character.inventory.itemList.constructionComplete = true;
                character.portraits.portraitUnlocked[59] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Construction set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You also gained 20% Boostier Boosts!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.duckComplete && character.inventory.itemList.maxxedDuck())
            {
                character.inventory.itemList.duckComplete = true;
                character.portraits.portraitUnlocked[60] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Duck set, Conquacks! You unclucked a new player portrait in the Fight Goss Menu! You also generate 6% Faster Mayo and Cards! QUACK.", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.netherComplete && character.inventory.itemList.maxxedNether())
            {
                character.inventory.itemList.netherComplete = true;
                character.portraits.portraitUnlocked[61] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Nether set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also earned +25% Faster Blood Magic Rituals! And also, Guff time factor is doubled!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.amalgamateComplete && character.inventory.itemList.maxxedAmalgamate())
            {
                character.inventory.itemList.amalgamateComplete = true;
                character.portraits.portraitUnlocked[62] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Amalgamate set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You also gain +10 max deck size, 10% NGU Effectiveness, Perm Beard Effectiveness, Yggdrasil Yield, AND Global digger bonus! ", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.pirateComplete && character.inventory.itemList.maxxedPirate())
            {
                character.inventory.itemList.pirateComplete = true;
                character.portraits.portraitUnlocked[66] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Pirate set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! NGUs are also 25% more effective!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.normalBonusAccComplete && character.inventory.itemList.maxxedNormalBonusAcc())
            {
                character.inventory.itemList.normalBonusAccComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Bonus Shinies Set (Normal), Congrats! You gained +25% Drop Chance!", 5f);
                character.refreshMenus();
                return false;
            }
            if (!character.inventory.itemList.evilBonusAccComplete && character.inventory.itemList.maxxedEvilBonusAcc())
            {
                character.inventory.itemList.evilBonusAccComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Bonus Shinies Set (Evil), Congrats! You gained 20% to adventure Stats", 5f);
                character.refreshMenus();
                return false;
            }

            return false;
        }
    }

    }
