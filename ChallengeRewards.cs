using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;

namespace fasterPace


{
    [HarmonyPatch]
    internal class ChallengeRewards
    {
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NGUController), "sadisticDivider")]
        [HarmonyPatch(typeof(NGUMagicController), "sadisticDivider")]
        private static bool sadisticDivider(ref float __result)
        {
            int completions = character.allChallenges.NGUChallenge.sadisticCompletions();

            const float BASE_DIVIDER = 1e7f;
            const float PER_COMPLETION_MULT = 0.95f; 

            __result = BASE_DIVIDER * Mathf.Pow(PER_COMPLETION_MULT, completions);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AllGoldDiggerController), "totalLevelBonus")]
        private static void AllGoldDigger_TotalLevelBonus_SadNoTimeMachine(ref float __result)
        {
            if (character == null || character.settings == null || character.allChallenges == null)
                return;

            // Sadistic only
            if (character.settings.rebirthDifficulty != difficulty.sadistic)
                return;

            int c = character.allChallenges.timeMachineChallenge.sadisticCompletions();

            if (c > 0)
            {
                __result += 0.03f * c;

                if (c == 10)
                    __result += 0.10f;
            }

            // T12 Amalgamate bonus (+10%)
            if (character.inventory?.itemList?.amalgamateComplete == true)
                __result *= 1.10f;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.takeDamage))]
        private static bool takeDamage(PlayerController __instance, ref float damage, ref float __result)
        {
            if (__instance.isBlocking)
            {
                damage = damage / (1f / __instance.character.advancedTrainingController.block.blockBonus(0)) / __instance.chargeFactor;
                __instance.chargeFactor = 1f;
            }

            if (__instance.isParrying)
            {
                damage /= __instance.character.adventureController.parryMulti * __instance.chargeFactor;
                __instance.isParrying = false;

                if (__instance.character.inventory.itemList.beast1complete)
                    __instance.attack(3f);
                else
                    __instance.attack(1f);

                __instance.chargeFactor = 1f;
            }

            if (__instance.character.adventure.beastModeOn)
            {
                float beastDamageMult = 3f;

                if (__instance.character.allChallenges.trollChallenge.sadisticCompletions() >= 3)
                {
                    beastDamageMult = 2f;
                }

                damage *= beastDamageMult;
            }

            damage = Mathf.Floor(damage / __instance.defenseBuffFactor / __instance.defenseDebuffFactor);
            __instance.character.adventure.curHP -= damage;

            if (damage > __instance.character.stats.highestDamageTaken && __instance.character.adventure.curHP > 0f)
                __instance.character.stats.highestDamageTaken = damage;

            __result = damage;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AllChallengesController), "adventureBonus")]
        private static void AdventureBonus_LaserSword20Evil(ref float __result)
        {
            
            if (character == null || character.allChallenges == null)
                return;

            int evilLaserSwordCompletions =
                character.allChallenges.laserSwordChallenge.evilCompletions();

            
            if (evilLaserSwordCompletions == 20)
            {
                __result *= 1.5f;
            }
        }

        [HarmonyPatch]
        internal static class Sad100LevelsCookingBuffs
        {
            private static int GetSad100LevelsCompletions(Character c)
            {
                if (c == null || c.settings == null || c.challenges == null) return 0;

                if (c.settings.rebirthDifficulty != difficulty.sadistic) return 0;

                int comp = c.challenges.levelChallenge10k.curSadisticCompletions;

                return Mathf.Clamp(comp, 0, 5);
            }

            private static float Mult(int c) => 1f + 0.8f * c; 

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CookingController), "totalCookingBonuses")]
            private static void Cooking_totalCookingBonuses_Postfix(CookingController __instance, ref float __result)
            {
                int c = GetSad100LevelsCompletions(__instance?.character);
                if (c <= 0) return;

                __result *= Mult(c);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CookingController), "eatRate")]
            private static void Cooking_eatRate_Postfix(CookingController __instance, ref float __result)
            {
                int c = GetSad100LevelsCompletions(__instance?.character);
                if (c <= 0) return;

                __result /= Mult(c);

                if (__result < 1f) __result = 1f;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CookingController), "totalExpBonus")]
            private static void Cooking_totalExpBonus_Postfix(CookingController __instance, ref float __result)
            {
                var ch = __instance?.character;
                if (ch == null || ch.settings == null || ch.challenges == null) return;
                if (ch.settings.rebirthDifficulty != difficulty.sadistic) return;

                if (ch.challenges.levelChallenge10k.curSadisticCompletions < 5) return;

                float raw = ch.cooking.expBonus;
                if (raw < 1f) raw = 1f;

                __result = raw;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "yggdrasilYieldBonus")]
        private static bool yggdrasilYieldBonus(Character __instance, ref float __result)
        {
            int laserEvil = __instance.allChallenges.laserSwordChallenge.evilCompletions();
            float completionMult = Mathf.Pow(1.01f, laserEvil);

            float cookingMult = 1f;
            if (__instance.allChallenges.trollChallenge.sadisticCompletions() >= 4)
            {
                float expBonus = 1f;

                if (__instance.cookingController != null)
                    expBonus = __instance.cookingController.totalExpBonus();
                else if (__instance.cooking != null)
                    expBonus = __instance.cooking.expBonus;

                if (expBonus < 1f) expBonus = 1f;

                // convert 50% of extra cooking EXP multiplier into Ygg yield
                cookingMult = 1f + (expBonus - 1f) * 0.5f;
            }

            // T12 Amalgamate set bonus
            float setMult = 1f;
            if (__instance.inventory?.itemList?.amalgamateComplete == true)
                setMult = 1.10f;

            __result =
                (1f + __instance.inventoryController.bonuses[specType.Yggdrasil]) *
                __instance.beastQuestPerkController.totalYggYieldBonus() *
                completionMult *
                cookingMult *
                setMult;

            return false;
        }

        [HarmonyPatch(typeof(CookingController), nameof(CookingController.updateDishUI))]
        internal static class CookingUI_ShowYggFromCooking
        {
            private static Text _extraYggText;

            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(CookingController __instance)
            {
                var ch = __instance?.character;
                if (ch?.allChallenges?.trollChallenge == null) return;
                if (__instance.dishInfoText == null) return;

                if (ch.allChallenges.trollChallenge.sadisticCompletions() < 4)
                {
                    if (_extraYggText != null)
                        _extraYggText.text = "";
                    return;
                }

                float expBonus = __instance.totalExpBonus();
                if (expBonus < 1f) expBonus = 1f;

                float yggCookingMult = 1f + (expBonus - 1f) * 0.5f;
                float percent = (yggCookingMult - 1f) * 100f;

                var refText = __instance.dishInfoText;
                var refRt = refText.rectTransform;

                if (_extraYggText == null)
                {
                    var parent = refText.transform.parent;
                    if (parent == null) return;

                    var go = new GameObject("FP_YggCookingBonusText");
                    go.transform.SetParent(parent, false);

                    _extraYggText = go.AddComponent<Text>();
                    _extraYggText.font = refText.font;
                    _extraYggText.fontStyle = FontStyle.Bold;
                    _extraYggText.fontSize = refText.fontSize - 1;
                    _extraYggText.fontStyle = refText.fontStyle;
                    _extraYggText.alignment = TextAnchor.UpperLeft;
                    _extraYggText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    _extraYggText.verticalOverflow = VerticalWrapMode.Overflow;
                    _extraYggText.supportRichText = true;
                    _extraYggText.color = refText.color; // black like the normal UI text

                    var rt = _extraYggText.rectTransform;
                    rt.anchorMin = refRt.anchorMin;
                    rt.anchorMax = refRt.anchorMax;
                    rt.pivot = refRt.pivot;
                    rt.localScale = Vector3.one;

                    // Start from the position that actually showed
                    rt.anchoredPosition = refRt.anchoredPosition + new Vector2(45f, -42f);
                    rt.sizeDelta = new Vector2(520f, 26f);
                }

                _extraYggText.color = refText.color;
                _extraYggText.text = $"Ygg Bonus From Cooking:     +{percent:0.##}%";
            }
        }

        [HarmonyPatch]
        internal static class SadNoAugs_PermBeards_HalfStrength
        {
            private const float STRENGTH = 0.25f;

            private static int Comps(Character ch)
            {
                if (ch?.settings == null || ch?.challenges == null) return 0;
                if (ch.settings.rebirthDifficulty != difficulty.sadistic) return 0;
                return Mathf.Clamp(ch.challenges.noAugsChallenge.curSadisticCompletions, 0, 5);
            }

            // +1% per completion, but scaled by STRENGTH
            // T12 Amalgamate adds +10% multiplicatively
            private static float LevelMult(Character ch, int c)
            {
                float mult = 1f + (0.01f * c * STRENGTH);

                if (ch?.inventory?.itemList?.amalgamateComplete == true)
                    mult *= 1.20f;

                return mult;
            }

            // At 5/5: soften exponent by moving it toward 1.0, but scaled by STRENGTH
            private static float PowUsed(int c, float pow)
            {
                if (c < 5) return pow;

                float t = 0.40f * STRENGTH;
                return Mathf.Lerp(pow, 1f, t);
            }

            // ─────────────────────────────────────────────
            // 0) STAT BEARD (double, linear only)
            // ─────────────────────────────────────────────
            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllBeardsController), nameof(AllBeardsController.permStatBonus), new Type[] { typeof(long) })]
            private static bool permStatBonus_Long(AllBeardsController __instance, long offset, ref double __result)
            {
                var ch = __instance?.character;
                int c = Comps(ch);
                if (c <= 0) return true; // vanilla

                long baseLevel = ch.beards.beards[0].permLevel + offset;
                if (baseLevel < 0) baseLevel = 0;

                double eff = baseLevel * (double)LevelMult(ch, c);

                __result = 1.0 + eff * 0.01;
                if (__result < 1.0) __result = 1.0;
                return false;
            }

            private static bool HandleFloat(
                AllBeardsController b,
                int beardIndex,
                long offset,
                ref float result,
                float linearMult,
                float pow,
                float powMult)
            {
                var ch = b?.character;
                int c = Comps(ch);
                if (c <= 0) return true; // vanilla

                long baseLevel = ch.beards.beards[beardIndex].permLevel + offset;
                if (baseLevel < 0) baseLevel = 0;

                float eff = baseLevel * LevelMult(ch, c);

                if (eff > 1000f)
                {
                    float p = PowUsed(c, pow);
                    result = 1f + Mathf.Pow(eff, p) * powMult * linearMult;
                }
                else
                {
                    result = 1f + eff * linearMult;
                }

                if (result < 1f) result = 1f;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllBeardsController), nameof(AllBeardsController.permLootBonus), new Type[] { typeof(long) })]
            private static bool permLootBonus_Long(AllBeardsController __instance, long offset, ref float __result)
                => HandleFloat(__instance, 1, offset, ref __result, 0.0005f, 0.33f, 102.4f);

            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllBeardsController), nameof(AllBeardsController.permNumberBonus), new Type[] { typeof(long) })]
            private static bool permNumberBonus_Long(AllBeardsController __instance, long offset, ref float __result)
                => HandleFloat(__instance, 2, offset, ref __result, 0.001f, 0.5f, 31.7f);

            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllBeardsController), nameof(AllBeardsController.permNGUBonus), new Type[] { typeof(long) })]
            private static bool permNGUBonus_Long(AllBeardsController __instance, long offset, ref float __result)
                => HandleFloat(__instance, 3, offset, ref __result, 0.0002f, 0.3f, 125.9f);

            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllBeardsController), nameof(AllBeardsController.permWandoosBonus), new Type[] { typeof(long) })]
            private static bool permWandoosBonus_Long(AllBeardsController __instance, long offset, ref float __result)
                => HandleFloat(__instance, 4, offset, ref __result, 0.002f, 0.5f, 31.7f);

            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllBeardsController), nameof(AllBeardsController.permAdventureBonus), new Type[] { typeof(long) })]
            private static bool permAdventureBonus_Long(AllBeardsController __instance, long offset, ref float __result)
                => HandleFloat(__instance, 5, offset, ref __result, 0.0005f, 0.5f, 31.7f);

            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllBeardsController), nameof(AllBeardsController.permGoldBonus), new Type[] { typeof(long) })]
            private static bool permGoldBonus_Long(AllBeardsController __instance, long offset, ref float __result)
                => HandleFloat(__instance, 6, offset, ref __result, 0.005f, 0.5f, 31.7f);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), nameof(CardsController.tagEffect))]
        private static void Cards_TagEffect_SadLaserSword_Postfix(CardsController __instance, ref float __result)
        {
            var ch = __instance?.character;
            if (ch == null) return;
            if (ch.settings.rebirthDifficulty < difficulty.sadistic) return;

            int c = ch.allChallenges.laserSwordChallenge.sadisticCompletions();
            if (c <= 0) return;

            float add = 0.005f * c;          
            float newCap = 0.25f + add;      

            __result = Mathf.Clamp(__result + add, 0.1f, newCap);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), nameof(CardsController.totalCardSpeed))]
        private static void Cards_TotalCardSpeed_SadLaserSwordFinal_Postfix(CardsController __instance, ref float __result)
        {
            var ch = __instance?.character;
            if (ch == null) return;
            if (ch.settings.rebirthDifficulty < difficulty.sadistic) return;

            var ls = ch.allChallenges.laserSwordChallenge;
            if (ls.sadisticCompletions() >= ls.maxCompletions)
                __result *= 1.25f; 
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), nameof(CardsController.totalMayoSpeed))]
        private static void Cards_TotalMayoSpeed_SadLaserSwordFinal_Postfix(CardsController __instance, ref float __result)
        {
            var ch = __instance?.character;
            if (ch == null) return;
            if (ch.settings.rebirthDifficulty < difficulty.sadistic) return;

            var ls = ch.allChallenges.laserSwordChallenge;
            if (ls.sadisticCompletions() >= ls.maxCompletions)
                __result *= 1.25f; 
        }

        [HarmonyPatch(typeof(TrollChallengeController), "trollFactor")]
        internal static class Patch_TrollFactor_GenSpeed
        {
            [HarmonyPostfix]
            private static void Postfix(ref int __result)
            {
                float mult = (float)GeneralBuffs.GenSpeed;  
                if (mult <= 1f) return;

                int newFactor = Mathf.FloorToInt(__result / mult);
                if (newFactor < 1) newFactor = 1;

                __result = newFactor;
            }
        }
        [HarmonyPatch(typeof(LaserSwordChallengeController), "laserSwordTarget")]
        internal static class Patch_LaserSwordTarget_X6
        {
            private const int MULT = 1;

            [HarmonyPostfix]
            private static void Postfix(ref int __result)
            {
                __result *= MULT;
            }
        }

    }

}    
