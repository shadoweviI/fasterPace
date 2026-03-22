using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace fasterPace
{
    [HarmonyPatch]
    internal static class Patch_Jshep_NextYggRewards_FoR
    {
        private static readonly float Mult = GeneralBuffs.GenSpeed * 2f;

        private static readonly Type NextYggType =
            AccessTools.TypeByName("jshepler.ngu.mods.NextYggRewards");

        private static readonly MethodInfo PpDiggerActiveGetter =
            NextYggType == null ? null : AccessTools.PropertyGetter(NextYggType, "_ppDiggerActive");

        private static readonly Type PluginType =
            AccessTools.TypeByName("jshepler.ngu.mods.Plugin");

        private static readonly MethodInfo AltIsDownGetter =
            PluginType == null ? null : AccessTools.PropertyGetter(PluginType, "AltIsDown");

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(NextYggType, "FoR");
        }

        [HarmonyPrefix]
        private static bool Prefix(FruitController fc, ref string __result)
        {
            if (fc?.character == null)
            {
                __result = string.Empty;
                return false;
            }

            var character = fc.character;

            int tierInt = fc.tierFactor(fc.harvestTier(9));
            float tierFactor = tierInt;
            float poopMulti = fc.usePoop(9);
            long seeds = fc.seedReward(9, tierInt, poopMulti);

            float ygg = character.yggdrasilYieldBonus();
            float fh = character.adventureController.itopod.totalHarvestBonus(9);
            float globalPPPMulti = character.adventureController.itopod.totalPPBonus(usePills: false);

            long ppp = (long)Mathf.Ceil(60000f * tierFactor * poopMulti * ygg * fh * globalPPPMulti * Mult);
            long pp = character.adventureController.itopod.progressToPP(ppp);
            long remainder = character.adventureController.itopod.progressToRemainder(ppp);

            string text =
                $"+{character.display(pp)} PP" +
                $"\n+{character.display(remainder)} progress to next PP" +
                $"\n+{character.display(seeds)} seeds";

            bool ppDiggerActive = true;
            try
            {
                if (PpDiggerActiveGetter != null)
                    ppDiggerActive = (bool)PpDiggerActiveGetter.Invoke(null, null);
            }
            catch { }

            if (!ppDiggerActive)
                text += "\n\n<b><color=red>PP DIGGER IS NOT ACTIVE!</color></b>";

            bool altDown = false;
            try
            {
                if (AltIsDownGetter != null)
                    altDown = (bool)AltIsDownGetter.Invoke(null, null);
            }
            catch { }

            if (altDown)
            {
                text += "\n\n<b>Base PPP:</b> 60,000" +
                        $"\n<b>Tier Factor:</b> <size=10>x</size>{tierFactor:#,##0.####}";

                if (ygg > 1f)
                    text += $"\n<b>YGG Yield:</b> <size=10>x</size>{ygg:#,##0.####}";

                if (poopMulti > 1f)
                    text += $"\n<b>Poop:</b> <size=10>x</size>{poopMulti:#,##0.####}";

                if (fh > 1f)
                    text += $"\n<b>First Harvest:</b> <size=10>x</size>{fh:#,##0.####}";

                text += $"\n<b>Global PPP Multi (w/o pills):</b> <size=10>x</size>{globalPPPMulti:#,##0.####}";
                text += $"\n<b>FasterPace PP Fruit:</b> <size=10>x</size>{Mult:#,##0.####}";
            }

            __result = text;
            return false;
        }
    }
}