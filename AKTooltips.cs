using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using UnityEngine;

namespace fasterPace
{
    [HarmonyPatch]
    internal static class AdvancedTrainingTitanAK_FasterPace
    {
        private static Character character;
        private static AllAdvancedTraining _advancedTraining;

        // totals with the AT multiplier removed (so we can solve for the needed AT levels)
        private static float totalPowerWithoutAdvPower;
        private static float totalDefWithoutAdvDef;
        private static float totalRegenWithoutAdvDef;

        // Mirror of jshelper's PTR struct (Power/Toughness/Regen thresholds)
        private sealed class PTR
        {
            public float Power;
            public float Toughness;
            public float Regen;

            public bool IsNothing => Power == 0f && Toughness == 0f && Regen == 0f;

            public PTR(float power, float toughness, float regen = 0f)
            {
                Power = power;
                Toughness = toughness;
                Regen = regen;
            }

            public static bool operator <=(PTR a, PTR b)
            {
                float aDef = Mathf.Max(a.Toughness, a.Regen);
                float bDef = Mathf.Max(b.Toughness, b.Regen);
                return a.Power <= b.Power && aDef <= bDef;
            }

            public static bool operator >=(PTR a, PTR b) => b <= a;
        }

        private sealed class AKReq
        {
            public int effectiveBossId;
            public int enemyId;
            public string name;
            public PTR ptr;
            public int optionalKills; // if you want "perma AK kills", set >0; otherwise keep 0

            public AKReq(int eff, int enemy, string name, PTR ptr, int optionalKills = 0)
            {
                effectiveBossId = eff;
                enemyId = enemy;
                this.name = name;
                this.ptr = ptr;
                this.optionalKills = optionalKills;
            }
        }

        // If you want this to match YOUR mod's “5 kills makes it autokill”, set OptionalKills=5 for all.
        // If you want vanilla-ish “perma” kills (like jshelper), keep these values.
        private static readonly List<AKReq> Requirements = new()
        {
            new AKReq(58,  302, "GRB",      new PTR(3000f,     2500f), 5),
            new AKReq(66,  303, "GCT",      new PTR(9000f,     7000f), 5),
            new AKReq(82,  304, "JAKE",     new PTR(25000f,    15000f), 5),
            new AKReq(100, 305, "UUG",      new PTR(800000f,   400000f,   14000f), 5),
            new AKReq(116, 310, "WALDERP",  new PTR(1.3e7f,    7.0e6f,     150000f), 5),

            new AKReq(132, 312, "BEAST v1", new PTR(2.5e9f,    1.6e9f,     2.5e7f), 5),
            new AKReq(132, 313, "BEAST v2", new PTR(2.5e10f,   1.6e10f,    2.5e8f), 5),
            new AKReq(132, 314, "BEAST v3", new PTR(2.5e11f,   1.6e11f,    2.5e9f), 5),
            new AKReq(132, 315, "BEAST v4", new PTR(2.5e12f,   1.6e12f,    2.5e10f), 5),

            new AKReq(426, 334, "NERD v1",  new PTR(5e14f,     2.5e14f,    5e12f), 5),
            new AKReq(426, 335, "NERD v2",  new PTR(1e16f,     5e15f,      1e14f), 5),
            new AKReq(426, 336, "NERD v3",  new PTR(2e17f,     1e17f,      2e15f), 5),
            new AKReq(426, 337, "NERD v4",  new PTR(5e18f,     2.5e18f,    5e16f), 5),

            new AKReq(467, 339, "GM v1",    new PTR(5e18f,     2.5e18f,    5e16f), 5),
            new AKReq(467, 340, "GM v2",    new PTR(1e20f,     5e19f,      1e18f), 5),
            new AKReq(467, 341, "GM v3",    new PTR(2e21f,     1e21f,      2e19f), 5),
            new AKReq(467, 342, "GM v4",    new PTR(5e22f,     2.5e22f,    5e20f), 5),

            new AKReq(491, 344, "EXILE v1", new PTR(1E+23f, 5E+22f, 1E+21f), 5),
            new AKReq(491, 345, "EXILE v2", new PTR(2E+24f, 1E+24f, 2E+22f), 5),
            new AKReq(491, 346, "EXILE v3", new PTR(4E+25f, 2E+25f, 4E+23f), 5),
            new AKReq(491, 347, "EXILE v4", new PTR(7.5E+26f, 3.7E+26f, 7.5E+24f), 5),

            new AKReq(777, 365, "IH v1",   new PTR(4E+28f,   2E+28f,   4E+26f), 5),
            new AKReq(777, 366, "IH v2",   new PTR(3.2E+29f, 1.6E+29f, 1.6E+27f), 5),
            new AKReq(777, 367, "IH v3",   new PTR(2E+30f,   1E+30f,   1E+28f), 5),
            new AKReq(777, 368, "IH v4",   new PTR(1E+31f,   5E+30f,   5E+28f), 5),


            new AKReq(826, 369, "RL v1",   new PTR(1.8E+31f, 6E+30f,   1.2E+29f), 5),
            new AKReq(826, 370, "RL v2",   new PTR(9E+31f,   3E+31f,   6E+29f), 5),
            new AKReq(826, 371, "RL v3",   new PTR(3.6E+32f, 1.2E+32f, 2.5E+30f), 5),
            new AKReq(826, 372, "RL v4",   new PTR(1.1E+33f, 3.6E+32f, 7.5E+30f), 5),


            new AKReq(850, 373, "AMAL v1", new PTR(3E+33f,   1E+33f,   2E+31f), 5),
            new AKReq(850, 374, "AMAL v2", new PTR(1.2E+34f, 4E+33f,   8E+31f), 5),
            new AKReq(850, 375, "AMAL v3", new PTR(3.6E+34f, 1.2E+34f, 2.4E+32f), 5),
            new AKReq(850, 376, "AMAL v4", new PTR(7.2E+34f, 2.4E+34f, 4.8E+32f), 5),

        };

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_Postfix(ButtonShower __instance)
        {
            character = __instance?.character;
            _advancedTraining = character?.advancedTrainingController;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "showTitanTimer")]
        [HarmonyAfter(new[] { "jshelper.ngu.mods" })]
        [HarmonyPriority(Priority.Last)]
        private static void ButtonShower_showTitanTimer_Postfix(ButtonShower __instance, ref string ___message)
        {
            if (__instance == null || character == null || _advancedTraining == null) return;
            if (string.IsNullOrWhiteSpace(___message)) return;
            if (__instance.adventure != null && __instance.adventure.interactable == false) return;

            // 1) Remove any existing AK block (jshelper or older versions)
            const string HeaderTagged = "<b>Adv. Training needed to autokill Titans:</b>";
            const string HeaderPlain = "Adv. Training needed to autokill Titans:";

            int idx = ___message.IndexOf(HeaderTagged, StringComparison.Ordinal);
            if (idx < 0) idx = ___message.IndexOf(HeaderPlain, StringComparison.Ordinal);

            if (idx >= 0)
                ___message = ___message.Substring(0, idx).TrimEnd();

            while (___message.EndsWith("<b>", StringComparison.Ordinal))
                ___message = ___message.Substring(0, ___message.Length - 3).TrimEnd();

            var extra = BuildTooltipText();
            if (string.IsNullOrEmpty(extra)) return;

            ___message += $"\n\n{HeaderTagged}{extra}";
            __instance.tooltip.showTooltip(___message);
        }



        private static string BuildTooltipText()
        {
            totalPowerWithoutAdvPower = character.totalAdvAttack() / (_advancedTraining.adventurePowerBonus(0) + 1f);
            totalDefWithoutAdvDef = character.totalAdvDefense() / (_advancedTraining.adventureToughnessBonus(0) + 1f);
            totalRegenWithoutAdvDef = character.totalAdvHPRegen() / (_advancedTraining.adventureToughnessBonus(0) + 1f);

            int effectiveBossId = character.effectiveBossID();
            var enemies = character.bestiary?.enemies;
            if (enemies == null) return null;

            // current AT levels: [1]=toughness, [0]=power
            var currentAT = new PTR(character.advancedTraining.level[1], character.advancedTraining.level[0]);
            var sb = new StringBuilder();

            foreach (var req in Requirements)
            {
                long enemyKills = enemies[req.enemyId].kills;

                // hide stuff far ahead unless you’ve at least reached it / killed it
                if (effectiveBossId < req.effectiveBossId && enemyKills == 0)
                    continue;

                // optional “perma AK” skip
                if (req.optionalKills > 0 && enemyKills >= req.optionalKills)
                    continue;

                var neededAT = GetNeededAT(req.ptr);
                if (neededAT.IsNothing)
                    continue;

                bool haveIt = neededAT <= currentAT;
                string color = haveIt ? "green" : "red";

                string p = float.IsInfinity(neededAT.Power) ? " UNREACHABLE" : character.display(neededAT.Power);
                string t = float.IsInfinity(neededAT.Toughness) ? " UNREACHABLE" : character.display(neededAT.Toughness);

                sb.Append($"\n<color={color}>{req.name}:  T={t}, P={p}</color>");

                // Walderp special “kills left for AK” line (matches what you showed)
                if (req.effectiveBossId == 116 && character.adventure.boss5Kills < 3)
                {
                    int killsLeft = 7 - (character.adventure.waldoDefeats + character.adventure.boss5Kills);
                    sb.Append($"\n   ({killsLeft} kill{(killsLeft == 1 ? "" : "s")} left for Walderp Requirements)");
                }

                // Generic "kills left for AK" counter (use bestiary kills)
                if (req.optionalKills > 0 && enemyKills < req.optionalKills)
                {
                    long left = req.optionalKills - enemyKills;
                    sb.Append($"\n   ({left} kill{(left == 1 ? "" : "s")} left for AK)");
                }

                // Like jshelper: only show the next “red” goal unless holding Alt
                if (!haveIt && !Input.GetKey(KeyCode.LeftAlt))
                    break;
            }

            return sb.Length == 0 ? null : sb.ToString();
        }

        // Convert required stat multipliers into needed AT levels (same formula jshelper uses)
        private static PTR GetNeededAT(PTR ak)
        {
            var needed = new PTR(0, 0);

            if (totalPowerWithoutAdvPower < ak.Power)
            {
                float pct = ((ak.Power / totalPowerWithoutAdvPower) - 1f) * 100f;
                needed.Power = (float)Math.Ceiling(Math.Pow(pct / 10f, 2.5));
                if (needed.Power < 0) needed.Power = 0;
            }

            if (totalDefWithoutAdvDef < ak.Toughness)
            {
                float pct = ((ak.Toughness / totalDefWithoutAdvDef) - 1f) * 100f;
                needed.Toughness = (float)Math.Ceiling(Math.Pow(pct / 10f, 2.5));
                if (needed.Toughness < 0) needed.Toughness = 0;
            }

            // regen also benefits from toughness multiplier
            if (ak.Regen > 0f && totalRegenWithoutAdvDef < ak.Regen)
            {
                float pct = ((ak.Regen / totalRegenWithoutAdvDef) - 1f) * 100f;
                float regenNeeded = (float)Math.Ceiling(Math.Pow(pct / 10f, 2.5));
                if (regenNeeded > needed.Toughness) needed.Toughness = regenNeeded;
            }

            if (needed.Power > long.MaxValue) needed.Power = float.PositiveInfinity;
            if (needed.Toughness > long.MaxValue) needed.Toughness = float.PositiveInfinity;

            return needed;
        }
    }
}
