using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore;
using static fasterPace.DailyRewardsRework;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace fasterPace


{
    [HarmonyPatch]
    internal class DailyRewardsRework
    {
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }
        internal class DailyRewardEntry
        {
            public int Rarity;                
            public float Weight;               
            public Func<Character, string> Name;
            public Action<Character> Apply;
        }

        internal static class DailyRewardsTable
        {
            // IMPORTANT: Each tier must have at least 1 entry.
            // Weight is relative (2 means twice as likely as 1).
            public static readonly List<DailyRewardEntry>[] Tiers = new List<DailyRewardEntry>[8]
            {
            // Tier 0
            new()
            {
                new DailyRewardEntry {
                    Rarity = 0, Weight = 60f,
                    Name = c => $"+{c.checkAPAdded(50L)} Arbitrary Points!",
                    Apply = c => c.addAP(50)
                },
                new DailyRewardEntry {
                    Rarity = 0, Weight = 30f,
                    Name = c => $"+{c.checkAPAdded(100L)} Arbitrary Points!",
                    Apply = c => c.addAP(100)
                },
                new DailyRewardEntry {
                    Rarity = 1, Weight = 10f,
                    Name = c => $"+{c.checkAPAdded(500L)} Arbitrary Points!",
                    Apply = c => c.addAP(500)
                },
            },

            new()
            {
                new DailyRewardEntry { Rarity=0, Weight=50f, Name=c=>$"+{c.checkAPAdded(50L)} Arbitrary Points!", Apply=c=>c.addAP(50)},
                new DailyRewardEntry { Rarity=0, Weight=35f, Name=c=>$"+{c.checkAPAdded(100L)} Arbitrary Points!", Apply=c=>c.addAP(100)},
                new DailyRewardEntry { Rarity=1, Weight=10f, Name=c=>$"+{c.checkAPAdded(500L)} Arbitrary Points!", Apply=c=>c.addAP(500)},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Energy Potion α!", Apply=c=>c.arbitrary.energyPotion1Count += 1},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Magic Potion α!",  Apply=c=>c.arbitrary.magicPotion1Count  += 1},
            },

            new()
            {
                new DailyRewardEntry { Rarity=0, Weight=50f, Name=c=>$"+{c.checkAPAdded(100L)} Arbitrary Points!", Apply=c=>c.addAP(100)},
                new DailyRewardEntry { Rarity=0, Weight=35f, Name=c=>$"+{c.checkAPAdded(200L)} Arbitrary Points!", Apply=c=>c.addAP(200)},
                new DailyRewardEntry { Rarity=1, Weight=10f, Name=c=>$"+{c.checkAPAdded(1000L)} Arbitrary Points!", Apply=c=>c.addAP(1000)},
                new DailyRewardEntry { Rarity=2, Weight=5f, Name=c=>"+1 Energy Potion α!", Apply=c=>c.arbitrary.energyPotion1Count += 1},
                new DailyRewardEntry { Rarity=2, Weight=5f, Name=c=>"+1 Magic Potion α!",  Apply=c=>c.arbitrary.magicPotion1Count  += 1},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Energy Potion β!", Apply=c=>c.arbitrary.energyPotion2Count += 1},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Magic Potion β!",  Apply=c=>c.arbitrary.magicPotion2Count  += 1},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Energy Bar Bar!", Apply=c=>c.arbitrary.energyBarBar1Count += 1},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Magic Bar Bar!",  Apply=c=>c.arbitrary.magicBarBar1Count  += 1},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Lucky Charm!",  Apply=c=>c.arbitrary.lootCharm1Count  += 1},
                new DailyRewardEntry { Rarity=3, Weight=2f, Name=c=>$"+{c.checkAPAdded(10000L)} Arbitrary Points!", Apply=c=>c.addAP(10000)},
                new DailyRewardEntry { Rarity=4, Weight=1f, Name=c=>$"+{c.checkAPAdded(50000L)} Arbitrary Points!", Apply=c=>c.addAP(50000)}
            }, 
            new()
            {
                new DailyRewardEntry { Rarity=0, Weight=50f, Name=c=>$"+{c.checkAPAdded(300L)} Arbitrary Points!", Apply=c=>c.addAP(300)},
                new DailyRewardEntry { Rarity=0, Weight=35f, Name=c=>$"+{c.checkAPAdded(600L)} Arbitrary Points!", Apply=c=>c.addAP(600)},
                new DailyRewardEntry { Rarity=1, Weight=10f, Name=c=>$"+{c.checkAPAdded(3000L)} Arbitrary Points!", Apply=c=>c.addAP(3000)},
                new DailyRewardEntry { Rarity=2, Weight=5f, Name=c=>"+1 Energy Potion α!", Apply=c=>c.arbitrary.energyPotion1Count += 1},
                new DailyRewardEntry { Rarity=2, Weight=5f, Name=c=>"+1 Magic Potion α!",  Apply=c=>c.arbitrary.magicPotion1Count  += 1},
                new DailyRewardEntry { Rarity=2, Weight=4f, Name=c=>"+1000 Blue Pills!",  Apply=c=>c.adventure.itopod.buffedKills += 1000L},
                new DailyRewardEntry { Rarity=2, Weight=4f, Name=c=>"+5 Poop!",  Apply=c=>c.arbitrary.poop1Count += 5},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Energy Bar Bar!", Apply=c=>c.arbitrary.energyBarBar1Count += 1},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Magic Bar Bar!",  Apply=c=>c.arbitrary.magicBarBar1Count  += 1},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Lucky Charm!",  Apply=c=>c.arbitrary.lootCharm1Count  += 1},
                new DailyRewardEntry { Rarity=3, Weight=2f, Name=c=>$"+{c.checkAPAdded(10000L)} Arbitrary Points!", Apply=c=>c.addAP(10000)},
                new DailyRewardEntry { Rarity=4, Weight=1.5f, Name=c=>"+1 Energy Potion β!", Apply=c=>c.arbitrary.energyPotion2Count += 1},
                new DailyRewardEntry { Rarity=4, Weight=1.5f, Name=c=>"+1 Magic Potion β!",  Apply=c=>c.arbitrary.magicPotion2Count  += 1},
                new DailyRewardEntry { Rarity=4, Weight=1f, Name=c=>$"+{c.checkAPAdded(50000L)} Arbitrary Points! :o", Apply=c=>c.addAP(50000)},
                new DailyRewardEntry {Rarity = 4, Weight = 1f,Name = c => "CONSUMABLE JACKPOT!",
                Apply = c =>
                {
                c.arbitrary.energyPotion1Count += 2;
                c.arbitrary.energyPotion2Count += 2;
                c.arbitrary.energyPotion3Count += 1;
                c.arbitrary.magicPotion1Count += 2;
                c.arbitrary.magicPotion2Count += 2;
                c.arbitrary.magicPotion3Count += 1;
                c.arbitrary.lootCharm1Count += 2;
                c.arbitrary.lootCharm2Count += 1;
                c.arbitrary.energyBarBar1Count += 2;
                c.arbitrary.magicBarBar1Count += 2;
                c.arbitrary.poop1Count += 10;
                c.adventure.itopod.buffedKills += 2000L;
                c.arbitrary.beastButterCount += 1;
                c.arbitrary.macGuffinBooster1Count += 1;
                c.refreshMenus();
                }
                }

            },
            new()
            {
                new DailyRewardEntry { Rarity=0, Weight=50f, Name=c=>$"+{c.checkAPAdded(500L)} Arbitrary Points!", Apply=c=>c.addAP(500L)},
                new DailyRewardEntry { Rarity=0, Weight=35f, Name=c=>$"+{c.checkAPAdded(1000L)} Arbitrary Points!", Apply=c=>c.addAP(1000L)},
                new DailyRewardEntry { Rarity=1, Weight=10f, Name=c=>$"+{c.checkAPAdded(5000L)} Arbitrary Points!", Apply=c=>c.addAP(5000L)},
                new DailyRewardEntry { Rarity=2, Weight=5f, Name=c=>"+1 Energy Potion α!", Apply=c=>c.arbitrary.energyPotion1Count += 1},
                new DailyRewardEntry { Rarity=2, Weight=5f, Name=c=>"+1 Magic Potion α!",  Apply=c=>c.arbitrary.magicPotion1Count  += 1},
                new DailyRewardEntry { Rarity=2, Weight=4f, Name=c=>"+2000 Blue Pills!",  Apply=c=>c.adventure.itopod.buffedKills += 2000L},
                new DailyRewardEntry { Rarity=2, Weight=4f, Name=c=>"+20 Poop!",  Apply=c=>c.arbitrary.poop1Count += 20},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Energy Bar Bar!", Apply=c=>c.arbitrary.energyBarBar1Count += 1},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+1 Magic Bar Bar!",  Apply=c=>c.arbitrary.magicBarBar1Count  += 1},
                new DailyRewardEntry { Rarity=3, Weight=2.5f, Name=c=>"+2 Lucky Charm!",  Apply=c=>c.arbitrary.lootCharm1Count  += 2},
                new DailyRewardEntry { Rarity=4, Weight=1.5f, Name=c=>"+1 Energy Potion β!", Apply=c=>c.arbitrary.energyPotion2Count += 1},
                new DailyRewardEntry { Rarity=4, Weight=1.5f, Name=c=>"+1 Magic Potion β!",  Apply=c=>c.arbitrary.magicPotion2Count  += 1},
                new DailyRewardEntry { Rarity=4, Weight=1.5f, Name=c=>"+1 MacGuffin Muffin!",  Apply=c=>c.arbitrary.macGuffinBooster1Count++},
                new DailyRewardEntry { Rarity=4, Weight=1f, Name=c=>$"+{c.checkAPAdded(75000L)} Arbitrary Points! :o", Apply=c=>c.addAP(75000L)},
                new DailyRewardEntry {Rarity = 4, Weight = 1f,Name = c => "CONSUMABLE JACKPOT!",
                Apply = c =>
                {
                c.arbitrary.energyPotion1Count += 2;
                c.arbitrary.energyPotion2Count += 2;
                c.arbitrary.energyPotion3Count += 1;
                c.arbitrary.magicPotion1Count += 2;
                c.arbitrary.magicPotion2Count += 2;
                c.arbitrary.magicPotion3Count += 1;
                c.arbitrary.lootCharm1Count += 2;
                c.arbitrary.lootCharm2Count += 1;
                c.arbitrary.energyBarBar1Count += 2;
                c.arbitrary.magicBarBar1Count += 2;
                c.arbitrary.poop1Count += 10;
                c.adventure.itopod.buffedKills += 2000L;
                c.arbitrary.beastButterCount += 1;
                c.arbitrary.macGuffinBooster1Count += 1;
                c.refreshMenus();
                }
                }
            }, new()
            {
                new DailyRewardEntry { Rarity=0, Weight=50f, Name=c=>$"+{c.checkAPAdded(800L)} Arbitrary Points!", Apply=c=>c.addAP(800L)},
                new DailyRewardEntry { Rarity=0, Weight=40f, Name=c=>$"+{c.checkAPAdded(1600L)} Arbitrary Points!", Apply=c=>c.addAP(1600L)},
                new DailyRewardEntry { Rarity=1, Weight=20f, Name=c=>$"+{c.checkAPAdded(8000L)} Arbitrary Points!", Apply=c=>c.addAP(8000L)},
                new DailyRewardEntry { Rarity=2, Weight=15f, Name=c=>"+2 Energy Potion α!", Apply=c=>c.arbitrary.energyPotion1Count += 2},
                new DailyRewardEntry { Rarity=2, Weight=15f, Name=c=>"+2 Magic Potion α!",  Apply=c=>c.arbitrary.magicPotion1Count  += 2},
                new DailyRewardEntry { Rarity=2, Weight=10f, Name=c=>"+4000 Blue Pills!",  Apply=c=>c.adventure.itopod.buffedKills += 4000L},
                new DailyRewardEntry { Rarity=2, Weight=10f, Name=c=>"+3 Beast Butter!",  Apply=c=>c.arbitrary.beastButterCount += 3},
                new DailyRewardEntry { Rarity=2, Weight=10f, Name=c=>"+30 Poop!",  Apply=c=>c.arbitrary.poop1Count += 30},
                new DailyRewardEntry { Rarity=2, Weight=10f, Name=c=>"+2 Energy Bar Bar!", Apply=c=>c.arbitrary.energyBarBar1Count += 2},
                new DailyRewardEntry { Rarity=2, Weight=10f, Name=c=>"+2 Magic Bar Bar!",  Apply=c=>c.arbitrary.magicBarBar1Count  += 2},
                new DailyRewardEntry { Rarity=3, Weight=8f, Name=c=>"+3 Lucky Charm!",  Apply=c=>c.arbitrary.lootCharm1Count  += 3},
                new DailyRewardEntry { Rarity=3, Weight=8f, Name=c=>$"Mayo Pack!", Apply = c => c.cardsController.awardSomeMayoIGuess(10,10,10,10,10,10)},
                new DailyRewardEntry { Rarity=3, Weight=8f, Name=c=>"+1 Energy Potion β!", Apply=c=>c.arbitrary.energyPotion2Count += 1},
                new DailyRewardEntry { Rarity=3, Weight=8f, Name=c=>"+1 Magic Potion β!",  Apply=c=>c.arbitrary.magicPotion2Count  += 1},
                new DailyRewardEntry { Rarity=3, Weight=8f, Name=c=>"+1 MacGuffin Muffin!",  Apply=c=>c.arbitrary.macGuffinBooster1Count++},
                new DailyRewardEntry { Rarity=3, Weight=5f,Name = c => $"Free Hack Day! :o",
                Apply = c =>
                {
                c.arbitrary.res3Potion1Count++;
                c.arbitrary.res3Potion2Count++;
                c.arbitrary.res3Potion3Count++;
                c.refreshMenus();
                }
                },
                new DailyRewardEntry {Rarity = 4, Weight = 3f,Name = c => "CONSUMABLE JACKPOT!",
                Apply = c =>
                {
                c.arbitrary.energyPotion1Count += 2;
                c.arbitrary.energyPotion2Count += 2;
                c.arbitrary.energyPotion3Count += 1;
                c.arbitrary.magicPotion1Count += 2;
                c.arbitrary.magicPotion2Count += 2;
                c.arbitrary.magicPotion3Count += 1;
                c.arbitrary.lootCharm1Count += 2;
                c.arbitrary.lootCharm2Count += 1;
                c.arbitrary.energyBarBar1Count += 2;
                c.arbitrary.magicBarBar1Count += 2;
                c.arbitrary.poop1Count += 10;
                c.adventure.itopod.buffedKills += 2000L;
                c.arbitrary.beastButterCount += 1;
                c.arbitrary.macGuffinBooster1Count += 1;
                c.refreshMenus();
                }
                },
               new DailyRewardEntry { Rarity=4, Weight=3f, Name=c=>$"+{c.checkAPAdded(100000L)} Arbitrary Points! :o", Apply=c=>c.addAP(100000L)}
            }, 
              new()
              {
                new DailyRewardEntry { Rarity=0, Weight=50f, Name=c=>$"+{c.checkAPAdded(1200L)} Arbitrary Points!", Apply=c=>c.addAP(1200L)},
                new DailyRewardEntry { Rarity=0, Weight=40f, Name=c=>$"+{c.checkAPAdded(2400L)} Arbitrary Points!", Apply=c=>c.addAP(2400L)},
                new DailyRewardEntry { Rarity=1, Weight=20f, Name=c=>$"+{c.checkAPAdded(12000L)} Arbitrary Points!", Apply=c=>c.addAP(12000L)},
                new DailyRewardEntry { Rarity=2, Weight=15f, Name=c=>"+2 Energy Potion β!", Apply=c=>c.arbitrary.energyPotion2Count += 2},
                new DailyRewardEntry { Rarity=2, Weight=15f, Name=c=>"+2 Magic Potion β!",  Apply=c=>c.arbitrary.magicPotion2Count  += 2},
                new DailyRewardEntry { Rarity=2, Weight=15f, Name=c=>"+16000 Blue Pills!",  Apply=c=>c.adventure.itopod.buffedKills += 16000L},
                new DailyRewardEntry { Rarity=2, Weight=15f, Name=c=>"+5 Beast Butter!",  Apply=c=>c.arbitrary.beastButterCount += 5},
                new DailyRewardEntry { Rarity=2, Weight=15f, Name=c=>"+50 Poop!",  Apply=c=>c.arbitrary.poop1Count += 50},
                new DailyRewardEntry { Rarity=2, Weight=10f, Name=c=>"+4 Energy Bar Bar!", Apply=c=>c.arbitrary.energyBarBar1Count += 4},
                new DailyRewardEntry { Rarity=2, Weight=10f, Name=c=>"+4 Magic Bar Bar!",  Apply=c=>c.arbitrary.magicBarBar1Count  += 4},
                new DailyRewardEntry { Rarity=3, Weight=8f, Name=c=>"+2 Super Lucky Charm!",  Apply=c=>c.arbitrary.lootCharm2Count  += 2},
                new DailyRewardEntry { Rarity=3, Weight=8f, Name=c=>$"Mayo Pack!", Apply = c => c.cardsController.awardSomeMayoIGuess(10,10,10,10,10,10)},
                new DailyRewardEntry { Rarity=3, Weight=8f, Name=c=>"+1 Energy Potion δ!", Apply=c=>c.arbitrary.energyPotion3Count += 1},
                new DailyRewardEntry { Rarity=3, Weight=8f, Name=c=>"+1 Magic Potion δ!",  Apply=c=>c.arbitrary.magicPotion3Count  += 1},
                new DailyRewardEntry { Rarity=3, Weight=8f, Name=c=>"+2 MacGuffin Muffin!",  Apply=c=>c.arbitrary.macGuffinBooster1Count+= 2},
                new DailyRewardEntry { Rarity=3, Weight=5f,Name = c => $"Free Hack Day! :o",
                Apply = c =>
                {
                c.arbitrary.res3Potion1Count++;
                c.arbitrary.res3Potion2Count++;
                c.arbitrary.res3Potion3Count++;
                c.refreshMenus();
                }
                },
                new DailyRewardEntry {Rarity = 4, Weight = 3f,Name = c => "CONSUMABLE JACKPOT!",
                Apply = c =>
                {
                c.arbitrary.energyPotion1Count += 2;
                c.arbitrary.energyPotion2Count += 2;
                c.arbitrary.energyPotion3Count += 1;
                c.arbitrary.magicPotion1Count += 2;
                c.arbitrary.magicPotion2Count += 2;
                c.arbitrary.magicPotion3Count += 1;
                c.arbitrary.lootCharm1Count += 2;
                c.arbitrary.lootCharm2Count += 1;
                c.arbitrary.energyBarBar1Count += 2;
                c.arbitrary.magicBarBar1Count += 2;
                c.arbitrary.poop1Count += 10;
                c.adventure.itopod.buffedKills += 2000L;
                c.arbitrary.beastButterCount += 1;
                c.arbitrary.macGuffinBooster1Count += 1;
                c.refreshMenus();
                }
                },
               new DailyRewardEntry { Rarity=4, Weight=3f, Name=c=>$"+{c.checkAPAdded(150000L)} Arbitrary Points! :o", Apply=c=>c.addAP(150000L)}
              }, 
                new()
                {
                new DailyRewardEntry { Rarity=0, Weight=50f, Name=c=>$"+{c.checkAPAdded(1500L)} Arbitrary Points!", Apply=c=>c.addAP(1500L)},
                new DailyRewardEntry { Rarity=0, Weight=45f, Name=c=>$"+{c.checkAPAdded(3000L)} Arbitrary Points!", Apply=c=>c.addAP(3000L)},
                new DailyRewardEntry { Rarity=1, Weight=20f, Name=c=>$"+{c.checkAPAdded(15000L)} Arbitrary Points!", Apply=c=>c.addAP(15000L)},
                new DailyRewardEntry { Rarity=2, Weight=10f, Name=c=>"+4 Energy Potion β!", Apply=c=>c.arbitrary.energyPotion2Count += 4},
                new DailyRewardEntry { Rarity=2, Weight=10f, Name=c=>"+4 Magic Potion β!",  Apply=c=>c.arbitrary.magicPotion2Count  += 4},
                new DailyRewardEntry { Rarity=2, Weight=10f, Name=c=>"+20000 Blue Pills!",  Apply=c=>c.adventure.itopod.buffedKills += 20000L},
                new DailyRewardEntry { Rarity=2, Weight=10f, Name=c=>"+10 Beast Butter!",  Apply=c=>c.arbitrary.beastButterCount += 10},
                new DailyRewardEntry { Rarity=2, Weight=8f, Name=c=>"+80 Poop!",  Apply=c=>c.arbitrary.poop1Count += 80},
                new DailyRewardEntry { Rarity=2, Weight=8f, Name=c=>"+5 Energy Bar Bar!", Apply=c=>c.arbitrary.energyBarBar1Count += 5},
                new DailyRewardEntry { Rarity=2, Weight=8f, Name=c=>"+5 Magic Bar Bar!",  Apply=c=>c.arbitrary.magicBarBar1Count  += 5},
                new DailyRewardEntry { Rarity=2, Weight=8f, Name=c=>"+3 Super Lucky Charm!",  Apply=c=>c.arbitrary.lootCharm2Count  += 3},
                new DailyRewardEntry { Rarity=2, Weight=8f, Name=c=>$"Mayo Pack!", Apply = c => c.cardsController.awardSomeMayoIGuess(15,15,15,15,15,15)},
                new DailyRewardEntry { Rarity=2, Weight=8f, Name=c=>"+2 Energy Potion δ!", Apply=c=>c.arbitrary.energyPotion3Count += 2},
                new DailyRewardEntry { Rarity=2, Weight=8f, Name=c=>"+2 Magic Potion δ!",  Apply=c=>c.arbitrary.magicPotion3Count  += 2},
                new DailyRewardEntry { Rarity=2, Weight=8f, Name=c=>"+4 MacGuffin Muffin!",  Apply=c=>c.arbitrary.macGuffinBooster1Count+= 4},
                new DailyRewardEntry { Rarity=3, Weight=5f,Name = c => $"Free Hack Day! :o",
                Apply = c =>
                {
                c.arbitrary.res3Potion1Count++;
                c.arbitrary.res3Potion2Count++;
                c.arbitrary.res3Potion3Count++;
                c.refreshMenus();
                }
                },
                new DailyRewardEntry {Rarity = 4, Weight = 3f,Name = c => "CONSUMABLE JACKPOT!",
                Apply = c =>
                {
                c.arbitrary.energyPotion1Count += 2;
                c.arbitrary.energyPotion2Count += 2;
                c.arbitrary.energyPotion3Count += 1;
                c.arbitrary.magicPotion1Count += 2;
                c.arbitrary.magicPotion2Count += 2;
                c.arbitrary.magicPotion3Count += 1;
                c.arbitrary.lootCharm1Count += 2;
                c.arbitrary.lootCharm2Count += 1;
                c.arbitrary.energyBarBar1Count += 2;
                c.arbitrary.magicBarBar1Count += 2;
                c.arbitrary.poop1Count += 10;
                c.adventure.itopod.buffedKills += 2000L;
                c.arbitrary.beastButterCount += 1;
                c.arbitrary.macGuffinBooster1Count += 1;
                c.refreshMenus();
                }
                },
               new DailyRewardEntry { Rarity=4, Weight=3f, Name=c=>$"+{c.checkAPAdded(175000L)} Arbitrary Points! :o", Apply=c=>c.addAP(175000L)}
                },
            };

            public static int PickIndex(List<DailyRewardEntry> list, float rng01)
            {
                float total = 0f;
                for (int i = 0; i < list.Count; i++) total += Mathf.Max(0f, list[i].Weight);
                if (total <= 0f) return 0;

                float roll = rng01 * total;
                float acc = 0f;
                for (int i = 0; i < list.Count; i++)
                {
                    acc += Mathf.Max(0f, list[i].Weight);
                    if (roll <= acc) return i;
                }
                return list.Count - 1;
            }

        }
    }

    // This makes the ACTUAL rewards match the custom table entries.
    [HarmonyPatch(typeof(DailyRewardController))]
    internal static class Patch_DailyRewardController_UseCustomTierRewards
    {
        private static bool ApplyTierReward(DailyRewardController d, int tier, int rewardID)
        {
            if (d == null || d.character == null)
                return true; // fallback to vanilla if something is wrong

            var tiers = DailyRewardsRework.DailyRewardsTable.Tiers;
            if (tiers == null || tier < 0 || tier >= tiers.Length)
                return true;

            var list = tiers[tier];
            if (list == null || list.Count == 0)
                return true;

            int idx = rewardID;
            if (idx < 0) idx = 0;
            if (idx >= list.Count) idx = list.Count - 1;

            try
            {
                var entry = list[idx];
                entry?.Apply?.Invoke(d.character);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[fasterPace] Daily reward apply failed (tier={tier}, id={rewardID} -> idx={idx}): {e}");
                return true; 
            }

            try { d.character.refreshMenus(); } catch { }

            return false; // skip vanilla reward logic
        }

        [HarmonyPrefix, HarmonyPatch("tier0Reward")] private static bool Tier0(DailyRewardController __instance, int rewardID) => ApplyTierReward(__instance, 0, rewardID);
        [HarmonyPrefix, HarmonyPatch("tier1Reward")] private static bool Tier1(DailyRewardController __instance, int rewardID) => ApplyTierReward(__instance, 1, rewardID);
        [HarmonyPrefix, HarmonyPatch("tier2Reward")] private static bool Tier2(DailyRewardController __instance, int rewardID) => ApplyTierReward(__instance, 2, rewardID);
        [HarmonyPrefix, HarmonyPatch("tier3Reward")] private static bool Tier3(DailyRewardController __instance, int rewardID) => ApplyTierReward(__instance, 3, rewardID);
        [HarmonyPrefix, HarmonyPatch("tier4Reward")] private static bool Tier4(DailyRewardController __instance, int rewardID) => ApplyTierReward(__instance, 4, rewardID);
        [HarmonyPrefix, HarmonyPatch("tier5Reward")] private static bool Tier5(DailyRewardController __instance, int rewardID) => ApplyTierReward(__instance, 5, rewardID);
        [HarmonyPrefix, HarmonyPatch("tier6Reward")] private static bool Tier6(DailyRewardController __instance, int rewardID) => ApplyTierReward(__instance, 6, rewardID);
        [HarmonyPrefix, HarmonyPatch("tier7Reward")] private static bool Tier7(DailyRewardController __instance, int rewardID) => ApplyTierReward(__instance, 7, rewardID);
    }



    [HarmonyPatch(typeof(DailyRewardController), "spinsToNextTier")]
    internal static class Patch_DailyReward_SpinsToNextTier_x6Faster
    {
        [HarmonyPrefix]
        private static bool Prefix(DailyRewardController __instance, ref long __result)
        {
            long total = __instance.character.daily.totalSpins;

            if (total < 3L) { __result = 3L - total; return false; }
            if (total < 5L) { __result = 5L - total; return false; }
            if (total < 10L) { __result = 10L - total; return false; }
            if (total < 20L) { __result = 20L - total; return false; }
            if (total < 40L) { __result = 40L - total; return false; }
            if (total < 60L) { __result = 60L - total; return false; }
            if (total < 122L) { __result = 122L - total; return false; }

            __result = 0L;
            return false; // skip original method
        }
    }

    [HarmonyPatch(typeof(DailyRewardController), "currentTier")]
    internal static class Patch_DailyReward_CurrentTier_x6Faster
    {
        [HarmonyPrefix]
        private static bool Prefix(DailyRewardController __instance, ref int __result)
        {
            long total = __instance.character.daily.totalSpins;

            if (total < 3L) { __result = 0; return false; }
            if (total < 5L) { __result = 1; return false; }
            if (total < 10L) { __result = 2; return false; }
            if (total < 20L) { __result = 3; return false; }
            if (total < 40L) { __result = 4; return false; }
            if (total < 60L) { __result = 5; return false; }
            if (total < 122L) { __result = 6; return false; }

            __result = 7;
            return false;
        }
    }



    [HarmonyPatch(typeof(DailyRewardController))]
    internal static class Patch_DailyRewardController_Tables
    {
        [HarmonyPrefix]
        [HarmonyPatch("constructRewardNames")]
        private static bool constructRewardNames(DailyRewardController __instance)
        {
            BuildTables(__instance);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(DailyRewardController), "targetSpinTime")]
        private static bool targetSpinTime(ref float __result)
        {
            __result = 86400f / (GeneralBuffs.GenSpeed * 2);
            return false;
        }

        internal static class DailySpinBankFix
        {
            // 6x more spins stored than vanilla.
            public const float BankMultiplier = GeneralBuffs.GenSpeed * 2; 

            [HarmonyPrefix, HarmonyPatch(typeof(DailyRewardController), "maxSpinTime")]
            private static bool Patch_MaxSpinTime(DailyRewardController __instance, ref float __result)
            {
                if (__instance?.character == null) return true;

                float spins = __instance.character.arbitrary.hasExtendedSpinBank ? 7f : 1.5f;
                __result = __instance.targetSpinTime() * (spins * BankMultiplier);
                return false;
            }
        



        // ------------------------------------------------------------
        // 2) Critical fix: Spending a spin must subtract targetSpinTime(), not 86400.
        // Otherwise banking breaks as soon as you change the timer.
        // ------------------------------------------------------------
        [HarmonyPrefix, HarmonyPatch(typeof(DailyRewardController), "startSpin")]
            private static bool Patch_StartSpin(DailyRewardController __instance)
            {
                if (__instance == null || __instance.character == null)
                    return true;

                // Keep vanilla gating
                if (!__instance.canSpin() || __instance.inSpinLoop)
                    return true;

                if (__instance.character.daily.freeSpins > 0L)
                {
                    __instance.character.daily.freeSpins -= 1L;
                }
                else
                {
                    double t = (double)__instance.targetSpinTime();
                    __instance.character.daily.spinTime.setTime(__instance.character.daily.spinTime.totalseconds - t);
                }

                // Mirror vanilla side effects so the UI/loop behaves
                __instance.inSpinLoop = true;
                __instance.outcomeBorder.gameObject.SetActive(false);
                __instance.oldOffset = __instance.getRewardID(UnityEngine.Random.value);

                return false; // skip vanilla (so it doesn't subtract 86400)
            }

            [HarmonyPrefix, HarmonyPatch(typeof(DailyRewardController), "startNoBullshitSpin")]
            private static bool Patch_StartNoBullshitSpin(DailyRewardController __instance)
            {
                if (__instance == null || __instance.character == null)
                    return true;

                if (!__instance.canSpin() || __instance.inSpinLoop)
                    return true; // keep vanilla tooltip behavior

                // Spend the spin (FIXED: do it once, and skip vanilla)
                if (__instance.character.daily.freeSpins > 0L)
                {
                    __instance.character.daily.freeSpins -= 1L;
                }
                else
                {
                    double t = (double)__instance.targetSpinTime();
                    __instance.character.daily.spinTime.setTime(__instance.character.daily.spinTime.totalseconds - t);
                }

                // Now do the rest of vanilla startNoBullshitSpin
                __instance.outcomeBorder.gameObject.SetActive(false);

                UnityEngine.Random.state = __instance.character.daily.dailyRewardState;
                float value = UnityEngine.Random.value;
                __instance.character.daily.dailyRewardState = UnityEngine.Random.state;

                int rewardID = __instance.getRewardID(value);

                switch (__instance.currentTier())
                {
                    case 0: __instance.tier0Reward(rewardID); break;
                    case 1: __instance.tier1Reward(rewardID); break;
                    case 2: __instance.tier2Reward(rewardID); break;
                    case 3: __instance.tier3Reward(rewardID); break;
                    case 4: __instance.tier4Reward(rewardID); break;
                    case 5: __instance.tier5Reward(rewardID); break;
                    case 6: __instance.tier6Reward(rewardID); break;
                    case 7: __instance.tier7Reward(rewardID); break;
                }

                __instance.outcomeText.text = __instance.rewardNames[__instance.currentTier()][rewardID];
                __instance.rarityBackground.color = __instance.rarityColor(__instance.rewardRarity[__instance.currentTier()][rewardID]);
                __instance.outcomeBorder.gameObject.SetActive(true);

                __instance.character.daily.totalSpins += 1L;
                __instance.Invoke("updateList", 5f);

                return false; // IMPORTANT: skip vanilla so it doesn't subtract 86400 too
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch("reConstructRewardNames")]
        private static bool reConstructRewardNames(DailyRewardController __instance)
        {
            BuildTables(__instance);
            return false;
        }

        private static void BuildTables(DailyRewardController d)
        {
            // Keep vanilla rarity colors (or change them here)
            d.rarity0.color = Color.white;
            d.rarity1.color = new Color(0.6f, 0.851f, 0.917f);
            d.rarity2.color = new Color(1f, 0.682f, 0.788f);
            d.rarity3.color = new Color(0.784f, 0.749f, 0.906f);
            d.rarity4.color = new Color(1f, 0.827f, 0.235f);

            // Ensure lists exist + cleared
            if (d.rewardNames == null) d.rewardNames = new List<List<string>>();
            if (d.rewardRarity == null) d.rewardRarity = new List<List<int>>();

            d.rewardNames.Clear();
            d.rewardRarity.Clear();

            while (d.rewardNames.Count < 8) d.rewardNames.Add(new List<string>());
            while (d.rewardRarity.Count < 8) d.rewardRarity.Add(new List<int>());

            for (int tier = 0; tier < 8; tier++)
            {
                var list = DailyRewardsTable.Tiers[tier];
                if (list == null || list.Count == 0) continue;

                for (int i = 0; i < list.Count; i++)
                {
                    d.rewardRarity[tier].Add(list[i].Rarity);
                    d.rewardNames[tier].Add(list[i].Name(d.character));
                }
            }
        }
    }


}


