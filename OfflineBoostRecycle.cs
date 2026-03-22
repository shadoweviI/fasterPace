using HarmonyLib;
using UnityEngine;

namespace fasterPace
{
    [HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
    internal static class Patch_OfflineCubeBoost_IncludeBoostRecycling
    {
        // Postfix so we *add* the missing value after vanilla runs.
        [HarmonyPostfix]
        private static void Postfix(Character __instance, int seconds)
        {
            var c = __instance;
            if (c == null) return;

            // Must match vanilla gate
            if (!c.settings.itopodOn) return;
            if (c.totalAdvAttack() < 650f) return;
            if (!c.arbitrary.hasCubeFilter) return;

            // Same inputs vanilla uses
            int floor = c.calculateBestItopodLevel();
            int tierRaw = c.adventureController.lootDrop.itopodTier(floor);
            if (floor <= 0) return;

            // Vanilla tier mapping -> num27 (1..13)
            int tier = 1;
            if (tierRaw > 0) tier = Mathf.Min(tierRaw, 24);

            if (tier < 1) tier = 1;

            if (tier >= 24) tier = 13;
            else if (tier >= 18) tier = 12;
            else if (tier >= 15) tier = 11;
            else if (tier > 10) tier = 10;

            if (tier > 13) tier = 13;
            if (tier <= 1) return; // no lower tier to recycle into

            // Kills offline (vanilla)
            float respawn = 1f + c.adventureController.respawnTime();
            if (c.inventory.itemList.redLiquidComplete)
                respawn = 0.8f + c.adventureController.respawnTime();

            int kills = Mathf.FloorToInt(seconds / respawn);
            if (kills <= 0) return;

            // Same divisor logic vanilla uses
            float ratio = 100f;
            if (c.adventure.itopod.perkLevel[26] >= 1L) ratio = 50f;
            ratio /= c.wishesController.totalBoostRatioDivider();

            float boostBonus = c.allItemList.boostBonus();

            // Boost Recycling chance (vanilla source)
            float r = c.totalRecycleBonus();
            if (r <= 0f) return;

            // Keep sane if something modded it beyond 1
            if (r > 1f) r = 1f;

            // Vanilla base uses capAttack[tier]
            // Recycling means: expected extra = capAttack[tier-1] * r + capAttack[tier-2] * r^2 + ... + capAttack[1] * r^(tier-1)
            float pow = r;
            float extraCapAttack = 0f;

            for (int t = tier - 1; t >= 1; t--)
            {
                extraCapAttack += c.itemInfo.capAttack[t] * pow;
                pow *= r;

                // tiny guard for extreme cases; finite anyway, but prevents float weirdness
                if (pow <= 0f) break;
            }

            if (extraCapAttack <= 0f) return;

            // Convert capAttack contribution into cube boost exactly like vanilla does:
            // num36 = capAttack[tier] * (kills/8)
            // num38 = num36 * boostBonus / (ratio*2)
            float extraNum36 = extraCapAttack * (kills / 8f);
            if (extraNum36 < 0f) extraNum36 = 0f;

            float extraCube = extraNum36 * boostBonus / (ratio * 2f);
            if (extraCube <= 0f) return;

            c.inventory.cubePower += extraCube;
            c.inventory.cubeToughness += extraCube;

            // Optional: message line so testers can see it’s working
            c.message += "\n(Boost Recycling) +" + c.display((double)extraCube) + " extra Cube Boost (offline expected)";
        }
    }
}
