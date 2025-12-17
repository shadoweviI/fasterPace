using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.TextCore;

namespace fasterPace
{
    [HarmonyPatch]
    internal class NoNGUSoftcaps
    {
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "hardCapNormalLevel")]
        private static bool hardCapNormalLevel(ref long __result)
        {
            __result = 100000000L;
            return false;
        }
        //
        //Energy NGUs
        //
        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "augmentBonusNormal")]
        private static bool augmentBonusNormal(AllNGUController __instance, ref double __result)
        {
            float level = character.NGU.skills[0].level;
            float scaled = level * 10f; 

            __result = 1.0 + (double)(scaled * __instance.normalEnergyBoostFactor[0]);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "augmentBonusEvil")]
        private static bool augmentBonusEvil(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.skills[0].evilLevel;
            float scaled = level * 10f;

            __result = 1.0 + (double)(scaled * __instance.evilEnergyBoostFactor[0]);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "augmentBonusSadistic")]
        private static bool augmentBonusSadistic(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.skills[0].sadisticLevel;
            float scaled = level * 10f;

            __result = 1.0 + (double)(scaled * __instance.sadisticEnergyBoostFactor[0]);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "wandoosBonusNormal")]
        private static bool wandoosBonusNormal(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.skills[1].level;
            float scaled = level * 10f;

            __result = 1f + scaled * __instance.normalEnergyBoostFactor[1];
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "wandoosBonusEvil")]
        private static bool wandoosBonusEvil(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.skills[1].evilLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.evilEnergyBoostFactor[1];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.25f) * 177.9f * __instance.evilEnergyBoostFactor[1];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "wandoosBonusSadistic")]
        private static bool wandoosBonusSadistic(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.skills[1].sadisticLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.sadisticEnergyBoostFactor[1];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.15f) * 354.81f * __instance.sadisticEnergyBoostFactor[1];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "respawnBonusNormal")]
        private static bool respawnBonusNormal(AllNGUController __instance, ref float __result)
        {
            const float SOFTCAP = 400f;

            const float MIN_NORMAL = 0.30f;
            const float OUT0_NORMAL = 0.80f;   
            const float POWER_NORMAL = 0.51f;     

            float level = character.NGU.skills[2].level;

            if (level <= SOFTCAP)
            {
                __result = 1f - level * __instance.normalEnergyBoostFactor[2];
                if (__result <= OUT0_NORMAL) __result = OUT0_NORMAL;
                return false;
            }

            float scaled = level * 10f;

            float frac = scaled / (scaled * 5f + 2000000f);
            float baseCurve = 1f - (frac + 0.2f);
            const float baseInf = 0.6f;

            float scaled0 = SOFTCAP * 10f;
            float frac0 = scaled0 / (scaled0 * 5f + 2000000f);
            float base0 = 1f - (frac0 + 0.2f);

            float t = (baseCurve - baseInf) / (base0 - baseInf);
            t = Mathf.Clamp01(t);
            t = Mathf.Pow(t, POWER_NORMAL);

            __result = MIN_NORMAL + t * (OUT0_NORMAL - MIN_NORMAL);

            if (__result < MIN_NORMAL) __result = MIN_NORMAL;
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "respawnBonusEvil")]
        private static bool respawnBonusEvil(AllNGUController __instance, ref float __result)
        {

            const float SOFTCAP = 10000f;

            const float MIN_EVSAD = 0.8164966f;
            const float OUT0_EVSAD = 0.925f;    
            const float POWER_EVSAD = 0.45f;      

            float level = character.NGU.skills[2].evilLevel;

            if (level <= SOFTCAP)
            {
                __result = 1f - level * __instance.evilEnergyBoostFactor[2];
                if (__result <= OUT0_EVSAD) __result = OUT0_EVSAD;
                return false;
            }

            float scaled = level * 10f;

            float frac = scaled / (scaled * 20f + 2000000f);
            float baseCurve = 1f - (frac + 0.05f);
            const float baseInf = 0.9f;

            float scaled0 = SOFTCAP * 10f;
            float frac0 = scaled0 / (scaled0 * 20f + 2000000f);
            float base0 = 1f - (frac0 + 0.05f);

            float t = (baseCurve - baseInf) / (base0 - baseInf);
            t = Mathf.Clamp01(t);
            t = Mathf.Pow(t, POWER_EVSAD);

            __result = MIN_EVSAD + t * (OUT0_EVSAD - MIN_EVSAD);

            if (__result < MIN_EVSAD) __result = MIN_EVSAD;
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "respawnBonusSadistic")]
        private static bool respawnBonusSadistic(AllNGUController __instance, ref float __result)
        {

            const float SOFTCAP = 10000f;

            const float MIN_EVSAD = 0.8164966f;
            const float OUT0_EVSAD = 0.925f;
            const float POWER_EVSAD = 0.45f;

            float level = character.NGU.skills[2].sadisticLevel;
            if (level <= SOFTCAP)
            {
                __result = 1f - level * __instance.sadisticEnergyBoostFactor[2];
                if (__result <= OUT0_EVSAD) __result = OUT0_EVSAD;
                return false;
            }

            float scaled = level * 10f;
            float frac = scaled / (scaled * 20f + 2000000f);
            float baseCurve = 1f - (frac + 0.05f);
            const float baseInf = 0.9f;
            float scaled0 = SOFTCAP * 10f;
            float frac0 = scaled0 / (scaled0 * 20f + 2000000f);
            float base0 = 1f - (frac0 + 0.05f);

            float t = (baseCurve - baseInf) / (base0 - baseInf);
            t = Mathf.Clamp01(t);
            t = Mathf.Pow(t, POWER_EVSAD);

            __result = MIN_EVSAD + t * (OUT0_EVSAD - MIN_EVSAD);

            if (__result < MIN_EVSAD) __result = MIN_EVSAD;
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "goldBonusNormal")]
        private static bool goldBonusNormal(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.skills[3].level;
            float scaled = level * 10f; 

            __result = 1f + scaled * __instance.normalEnergyBoostFactor[3];

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "goldBonusEvil")]
        private static bool goldBonusEvil(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.skills[3].evilLevel;
            float scaled = level * 10f;

            __result = 1f + scaled * __instance.evilEnergyBoostFactor[3];

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "goldBonusSadistic")]
        private static bool goldBonusSadistic(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.skills[3].sadisticLevel;
            float scaled = level * 10f;

            __result = 1f + scaled * __instance.sadisticEnergyBoostFactor[3];

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonusNormal")]
        private static bool adventureBonusNormal(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.skills[4].level;
            float scaledLevel = level * 10f; 

            if (level <= 1000)
            {
                __result = 1f + level * __instance.normalEnergyBoostFactor[4];
            }
            else
            {
                __result = 1f + Mathf.Sqrt(scaledLevel) * 31.7f * __instance.normalEnergyBoostFactor[4];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonusEvil")]
        private static bool adventureBonusEvil(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.skills[4].evilLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.evilEnergyBoostFactor[4];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.25f) * 177.9f * __instance.evilEnergyBoostFactor[4];
            }

            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonusSadistic")]
        private static bool adventureBonusSadistic(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.skills[4].sadisticLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.sadisticEnergyBoostFactor[4];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.2f) * 251.19f * __instance.sadisticEnergyBoostFactor[4];
            }

            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "alphaStatBonusNormal")]
        private static bool alphaStatBonusNormal(AllNGUController __instance, ref double __result)
        {
            float level = character.NGU.skills[5].level;
            float scaled = level * 10f; 

            __result = 1.0 + (double)(scaled * __instance.normalEnergyBoostFactor[5]);
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "alphaStatBonusEvil")]
        private static bool alphaStatBonusEvil(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.skills[5].evilLevel;
            float scaled = level * 10f;

            __result = 1.0 + (double)(scaled * __instance.evilEnergyBoostFactor[5]);
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "alphaStatBonusSadistic")]
        private static bool alphaStatBonusSadistic(AllNGUController __instance, ref double __result)
        {
            if (character.settings.rebirthDifficulty < difficulty.sadistic)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.skills[5].sadisticLevel;
            float scaled = level * 10f;

            __result = 1.0 + (double)(scaled * __instance.sadisticEnergyBoostFactor[5]);
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "lootBonusNormal")]
        private static bool lootBonusNormal(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.skills[6].level;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.normalEnergyBoostFactor[6];
            }
            else
            {
                __result = 1f + Mathf.Sqrt(scaled) * 31.7f * __instance.normalEnergyBoostFactor[6];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "lootBonusEvil")]
        private static bool lootBonusEvil(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.skills[6].evilLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.evilEnergyBoostFactor[6];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.3f) * 125.9f * __instance.evilEnergyBoostFactor[6];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "lootBonusSadistic")]
        private static bool lootBonusSadistic(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.skills[6].sadisticLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.sadisticEnergyBoostFactor[6];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.2f) * 251.2f * __instance.sadisticEnergyBoostFactor[6];
            }

            return false;
        }



        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "magicNGUBonusNormal")]
        private static bool magicNGUBonusNormal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.skills[7].level;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.normalEnergyBoostFactor[7];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.3f) * 125.9f * __instance.normalEnergyBoostFactor[7];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "magicNGUBonusEvil")]
        private static bool magicNGUBonusEvil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.skills[7].evilLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.evilEnergyBoostFactor[7];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.3f) * 125.9f * __instance.evilEnergyBoostFactor[7];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "magicNGUBonusSadistic")]
        private static bool magicNGUBonusSadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.skills[7].sadisticLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.sadisticEnergyBoostFactor[7];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.1f) * 501.19f * __instance.sadisticEnergyBoostFactor[7];
            }

            return false;
        }



        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "PPBonusNormal")]
        private static bool PPBonusNormal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.skills[8].level;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.normalEnergyBoostFactor[8];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.3f) * 125.9f * __instance.normalEnergyBoostFactor[8];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "PPBonusEvil")]
        private static bool PPBonusEvil(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.skills[8].evilLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.evilEnergyBoostFactor[8];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.2f) * 251.2f * __instance.evilEnergyBoostFactor[8];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "PPBonusSadistic")]
        private static bool PPBonusSadistic(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.skills[8].sadisticLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.sadisticEnergyBoostFactor[8];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.1f) * 501.21f * __instance.sadisticEnergyBoostFactor[8];
            }

            return false;
        }

        //
        //Magic NGUs
        //

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "yggdrasilBonusNormal")]
        private static bool yggdrasilBonusNormal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.magicSkills[0].level;
            float scaled = level * 10f;

            if (level <= 400)
            {
                __result = 1f + level * __instance.normalMagicBoostFactor[0];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.33f) * 55.4f * __instance.normalMagicBoostFactor[0];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "yggdrasilBonusEvil")]
        private static bool yggdrasilBonusEvil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.magicSkills[0].evilLevel;
            float scaled = level * 10f;

            if (level <= 400)
            {
                __result = 1f + level * __instance.evilMagicBoostFactor[0];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.1f) * 219.72f * __instance.evilMagicBoostFactor[0];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "yggdrasilBonusSadistic")]
        private static bool yggdrasilBonusSadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.magicSkills[0].sadisticLevel;
            float scaled = level * 10f;

            if (level <= 400)
            {
                __result = 1f + level * __instance.sadisticMagicBoostFactor[0];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.08f) * 247.69f * __instance.sadisticMagicBoostFactor[0];
            }

            return false;
        }



        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "expBonusNormal")]
        private static bool expBonusNormal(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.magicSkills[1].level;
            float scaled = level * 10f;

            if (level <= 2000)
            {
                __result = 1f + level * character.NGUController.normalMagicBoostFactor[1];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.4f) * 95.66f * character.NGUController.normalMagicBoostFactor[1];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "expBonusEvil")]
        private static bool expBonusEvil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.magicSkills[1].evilLevel;
            float scaled = level * 10f;

            if (level <= 2000)
            {
                __result = 1f + level * character.NGUController.evilMagicBoostFactor[1];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.2f) * 437.35f * character.NGUController.evilMagicBoostFactor[1];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "expBonusSadistic")]
        private static bool expBonusSadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.magicSkills[1].sadisticLevel;
            float scaled = level * 10f;

            if (level <= 2000)
            {
                __result = 1f + level * character.NGUController.sadisticMagicBoostFactor[1];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.15f) * 639.56f * character.NGUController.sadisticMagicBoostFactor[1];
            }

            return false;
        }



        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "betaStatBonusNormal")]
        private static bool betaStatBonusNormal(AllNGUController __instance, ref double __result)
        {
            float level = character.NGU.magicSkills[2].level;
            float scaled = level * 10f; // 100M behaves like 1B

            __result = 1.0 + (double)(scaled * __instance.normalMagicBoostFactor[2]);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "betaStatBonusEvil")]
        private static bool betaStatBonusEvil(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.magicSkills[2].evilLevel;
            float scaled = level * 10f;

            __result = 1.0 + (double)(scaled * __instance.evilMagicBoostFactor[2]);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "betaStatBonusSadistic")]
        private static bool betaStatBonusSadistic(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.magicSkills[2].sadisticLevel;
            float scaled = level * 10f;

            __result = 1.0 + (double)(scaled * __instance.sadisticMagicBoostFactor[2]);
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "numberBonusNormal", [])]
        private static bool numberBonusNormal(AllNGUController __instance, ref double __result)
        {
            float level = character.NGU.magicSkills[3].level;
            float scaled = level * 10f; 

            if (level <= 1000)
            {
                __result = 1.0 + (double)(scaled * __instance.normalMagicBoostFactor[3]) * character.timeMulti;
            }
            else
            {
                __result = 1.0 + (double)(Mathf.Pow(scaled, 0.5f) * 31.7f * __instance.normalMagicBoostFactor[3]) * character.timeMulti;
            }

            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "numberBonusEvil", [])]
        private static bool numberBonusEvil(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.magicSkills[3].evilLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1.0 + (double)(scaled * __instance.evilMagicBoostFactor[3]) * character.timeMulti;
            }
            else
            {
                __result = 1.0 + (double)(Mathf.Pow(scaled, 0.3f) * 125.9f * __instance.evilMagicBoostFactor[3]) * character.timeMulti;
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "numberBonusSadistic", [])]
        private static bool numberBonusSadistic(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.magicSkills[3].sadisticLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1.0 + (double)(scaled * __instance.sadisticMagicBoostFactor[3]) * character.timeMulti;
            }
            else
            {
                __result = 1.0 + (double)(Mathf.Pow(scaled, 0.2f) * 251.2f * __instance.sadisticMagicBoostFactor[3]) * character.timeMulti;
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "numberBonusNormal", [typeof(bool)])]
        private static bool numberBonusNormal(AllNGUController __instance, ref double __result, bool noTimeMulti)
        {
            float level = character.NGU.magicSkills[3].level;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1.0 + (double)(scaled * __instance.normalMagicBoostFactor[3]);
            }
            else
            {
                __result = 1.0 + (double)(Mathf.Pow(scaled, 0.5f) * 31.7f * __instance.normalMagicBoostFactor[3]);
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "numberBonusEvil", [typeof(bool)])]
        private static bool numberBonusEvil(AllNGUController __instance, ref double __result, bool noTimeMulti)
        {

            float level = character.NGU.magicSkills[3].evilLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1.0 + (double)(scaled * __instance.evilMagicBoostFactor[3]);
            }
            else
            {
                __result = 1.0 + (double)(Mathf.Pow(scaled, 0.3f) * 125.9f * __instance.evilMagicBoostFactor[3]);
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "numberBonusSadistic", [typeof(bool)])]
        private static bool numberBonusSadistic(AllNGUController __instance, ref double __result, bool noTimeMulti)
        {

            float level = character.NGU.magicSkills[3].sadisticLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1.0 + (double)(scaled * __instance.sadisticMagicBoostFactor[3]);
            }
            else
            {
                __result = 1.0 + (double)(Mathf.Pow(scaled, 0.2f) * 251.2f * __instance.sadisticMagicBoostFactor[3]);
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "timeMachineBonusNormal")]
        private static bool timeMachineBonusNormal(AllNGUController __instance, ref double __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.magicSkills[4].level;
            float scaled = level * 10f; 

            if (character.NGU.magicSkills[1].level <= 2000)
            {
                __result = 1.0 + (double)(scaled * __instance.normalMagicBoostFactor[4]);
            }
            else
            {
                __result = 1.0 + Math.Pow(scaled, 0.8) * 3.981 * (double)__instance.normalMagicBoostFactor[4];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "timeMachineBonusEvil")]
        private static bool timeMachineBonusEvil(AllNGUController __instance, ref double __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.magicSkills[4].evilLevel;
            float scaled = level * 10f;

            if (character.NGU.magicSkills[1].level <= 2000)
            {
                __result = 1.0 + (double)(scaled * __instance.evilMagicBoostFactor[4]);
            }
            else
            {
                __result = 1.0 + Math.Pow(scaled, 0.8) * 3.981 * (double)__instance.evilMagicBoostFactor[4];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "timeMachineBonusSadistic")]
        private static bool timeMachineBonusSadistic(AllNGUController __instance, ref double __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.magicSkills[4].sadisticLevel;
            float scaled = level * 10f;

            if (character.NGU.magicSkills[1].level <= 2000)
            {
                __result = 1.0 + (double)(scaled * __instance.sadisticMagicBoostFactor[4]);
            }
            else
            {
                __result = 1.0 + Math.Pow(scaled, 0.8) * 3.981 * (double)__instance.sadisticMagicBoostFactor[4];
            }

            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "energyNGUBonusNormal")]
        private static bool energyNGUBonusNormal(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.magicSkills[5].level;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.normalMagicBoostFactor[5];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.3f) * 125.9f * __instance.normalMagicBoostFactor[5];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "energyNGUBonusEvil")]
        private static bool energyNGUBonusEvil(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.magicSkills[5].evilLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.evilMagicBoostFactor[5];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.2f) * 251.2f * __instance.evilMagicBoostFactor[5];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "energyNGUBonusSadistic")]
        private static bool energyNGUBonusSadistic(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.magicSkills[5].sadisticLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.sadisticMagicBoostFactor[5];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.15f) * 354.82f * __instance.sadisticMagicBoostFactor[5];
            }

            return false;
        }



        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonus2Normal")]
        private static bool adventureBonus2Normal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.magicSkills[6].level;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.normalMagicBoostFactor[6];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.4f) * 63.13f * __instance.normalMagicBoostFactor[6];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonus2Evil")]
        private static bool adventureBonus2Evil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.magicSkills[6].evilLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.evilMagicBoostFactor[6];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.25f) * 177.83f * __instance.evilMagicBoostFactor[6];
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonus2Sadistic")]
        private static bool adventureBonus2Sadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled)
            {
                __result = 1;
                return false;
            }

            float level = character.NGU.magicSkills[6].sadisticLevel;
            float scaled = level * 10f;

            if (level <= 1000)
            {
                __result = 1f + level * __instance.sadisticMagicBoostFactor[6];
            }
            else
            {
                __result = 1f + Mathf.Pow(scaled, 0.12f) * 436.53f * __instance.sadisticMagicBoostFactor[6];
            }

            return false;
        }
    }
}
