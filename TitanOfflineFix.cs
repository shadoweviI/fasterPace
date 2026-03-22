using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace fasterPace
{
    // Fix offline autokills for Titan 6-12 to respect version unlocks AND credit the correct version.
    // Rule: pick the highest version <= selected version that is unlocked (achieved). If none unlocked => no offline kills.
    // Also: temporarily force the titanXVersion field to that chosen version while offline progress runs (then restore).
    [HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
    internal static class Patch_Character_AdventureOfflineProgress_Titans_VersionFallback
    {
        // Set by our achieved helpers right before the offline block executes.
        // 0..3 represent V1..V4.
        [ThreadStatic] private static int _chosenVersion;

        // Original selected versions we temporarily override during offline progress.
        // int.MinValue means "not captured / not overridden".
        [ThreadStatic] private static int _orig6;
        [ThreadStatic] private static int _orig7;
        [ThreadStatic] private static int _orig8;
        [ThreadStatic] private static int _orig9;
        [ThreadStatic] private static int _orig10;
        [ThreadStatic] private static int _orig11;
        [ThreadStatic] private static int _orig12;

        // -----------------------------
        // Prefix/Postfix to reset + restore
        // -----------------------------
        [HarmonyPrefix]
        private static void Prefix(Character __instance)
        {
            _chosenVersion = -1;

            _orig6 = _orig7 = _orig8 = _orig9 = _orig10 = _orig11 = _orig12 = int.MinValue;
        }

        [HarmonyPostfix]
        private static void Postfix(Character __instance)
        {
            var adv = __instance?.adventure;
            if (adv == null) return;

            if (_orig6 != int.MinValue) adv.titan6Version = _orig6;
            if (_orig7 != int.MinValue) adv.titan7Version = _orig7;
            if (_orig8 != int.MinValue) adv.titan8Version = _orig8;
            if (_orig9 != int.MinValue) adv.titan9Version = _orig9;
            if (_orig10 != int.MinValue) adv.titan10Version = _orig10;
            if (_orig11 != int.MinValue) adv.titan11Version = _orig11;
            if (_orig12 != int.MinValue) adv.titan12Version = _orig12;

            _chosenVersion = -1;
        }

        // -----------------------------
        // Transpiler:
        // 1) Replace V1-only achieved checks with "best unlocked <= selected" checks.
        // 2) Replace bestiary addKills IDs (V1 constants) with "base + chosenVersion".
        // -----------------------------
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Replace V1-only checks with our helpers
            ReplaceAchievedCall(codes, "autokillTitan6V1Achieved", nameof(AutokillTitan6_BestUnlockedLeSelected));
            ReplaceAchievedCall(codes, "autokillTitan7V1Achieved", nameof(AutokillTitan7_BestUnlockedLeSelected));
            ReplaceAchievedCall(codes, "autokillTitan8V1Achieved", nameof(AutokillTitan8_BestUnlockedLeSelected));
            ReplaceAchievedCall(codes, "autokillTitan9V1Achieved", nameof(AutokillTitan9_BestUnlockedLeSelected));
            ReplaceAchievedCall(codes, "autokillTitan10V1Achieved", nameof(AutokillTitan10_BestUnlockedLeSelected));
            ReplaceAchievedCall(codes, "autokillTitan11V1Achieved", nameof(AutokillTitan11_BestUnlockedLeSelected));
            ReplaceAchievedCall(codes, "autokillTitan12V1Achieved", nameof(AutokillTitan12_BestUnlockedLeSelected));

            // Fix bestiary counters: replace the V1 constants used in offline addKills(...) with getters.
            //
            // Titan 6 V1 bestiary id = 312 (you confirmed 312..315)
            // Titan 7..12 V1 ids are the vanilla ones:
            //  334, 339, 344, 365, 369, 373
            ReplaceBestiaryIdConstant(codes, 312, AccessTools.Method(typeof(Patch_Character_AdventureOfflineProgress_Titans_VersionFallback), nameof(Titan6BestiaryId)));
            ReplaceBestiaryIdConstant(codes, 334, AccessTools.Method(typeof(Patch_Character_AdventureOfflineProgress_Titans_VersionFallback), nameof(Titan7BestiaryId)));
            ReplaceBestiaryIdConstant(codes, 339, AccessTools.Method(typeof(Patch_Character_AdventureOfflineProgress_Titans_VersionFallback), nameof(Titan8BestiaryId)));
            ReplaceBestiaryIdConstant(codes, 344, AccessTools.Method(typeof(Patch_Character_AdventureOfflineProgress_Titans_VersionFallback), nameof(Titan9BestiaryId)));
            ReplaceBestiaryIdConstant(codes, 365, AccessTools.Method(typeof(Patch_Character_AdventureOfflineProgress_Titans_VersionFallback), nameof(Titan10BestiaryId)));
            ReplaceBestiaryIdConstant(codes, 369, AccessTools.Method(typeof(Patch_Character_AdventureOfflineProgress_Titans_VersionFallback), nameof(Titan11BestiaryId)));
            ReplaceBestiaryIdConstant(codes, 373, AccessTools.Method(typeof(Patch_Character_AdventureOfflineProgress_Titans_VersionFallback), nameof(Titan12BestiaryId)));

            return codes;
        }

        private static void ReplaceAchievedCall(List<CodeInstruction> codes, string oldMethodName, string newHelperName)
        {
            var oldMi = AccessTools.Method(typeof(AdventureController), oldMethodName);
            var newMi = AccessTools.Method(typeof(Patch_Character_AdventureOfflineProgress_Titans_VersionFallback), newHelperName);
            if (oldMi == null || newMi == null) return;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(oldMi))
                {
                    // callvirt AdventureController.autokillTitanXV1Achieved()
                    // => call bool Helper(AdventureController)
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = newMi;
                }
            }
        }

        private static void ReplaceBestiaryIdConstant(List<CodeInstruction> codes, int oldId, MethodInfo replacementGetter)
        {
            if (replacementGetter == null) return;

            for (int i = 0; i < codes.Count; i++)
            {
                // Find "ldc.i4 <oldId>" and replace with:
                //   ldarg.0
                //   call int Getter(Character)
                if (codes[i].opcode == OpCodes.Ldc_I4 && codes[i].operand is int v && v == oldId)
                {
                    codes[i] = new CodeInstruction(OpCodes.Ldarg_0);
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, replacementGetter));
                    i++;
                }
            }
        }

        // -----------------------------
        // Core selection rule + applying it
        // -----------------------------
        private static bool ChooseBestLeSelected(int selected, Func<int, bool> achievedByVersion0to3)
        {
            _chosenVersion = -1;

            int v = Clamp(selected, 0, 3);
            for (int cur = v; cur >= 0; cur--)
            {
                if (achievedByVersion0to3(cur))
                {
                    _chosenVersion = cur;
                    return true;
                }
            }
            return false;
        }

        private static void ApplyChosenVersion(Character c, int titanIndex, int chosen)
        {
            var adv = c?.adventure;
            if (adv == null) return;

            int chosenClamped = Clamp(chosen, 0, 3);

            switch (titanIndex)
            {
                case 6:
                    if (_orig6 == int.MinValue) _orig6 = adv.titan6Version;
                    adv.titan6Version = chosen;
                    break;
                case 7:
                    if (_orig7 == int.MinValue) _orig7 = adv.titan7Version;
                    adv.titan7Version = chosen;
                    break;
                case 8:
                    if (_orig8 == int.MinValue) _orig8 = adv.titan8Version;
                    adv.titan8Version = chosen;
                    break;
                case 9:
                    if (_orig9 == int.MinValue) _orig9 = adv.titan9Version;
                    adv.titan9Version = chosen;
                    break;
                case 10:
                    if (_orig10 == int.MinValue) _orig10 = adv.titan10Version;
                    adv.titan10Version = chosen;
                    break;
                case 11:
                    if (_orig11 == int.MinValue) _orig11 = adv.titan11Version;
                    adv.titan11Version = chosen;
                    break;
                case 12:
                    if (_orig12 == int.MinValue) _orig12 = adv.titan12Version;
                    adv.titan12Version = chosen;
                    break;
            }
        }

        private static int Clamp(int x, int min, int max)
        {
            if (x < min) return min;
            if (x > max) return max;
            return x;
        }

        // -----------------------------
        // Achieved helpers (Titan 6..12):
        // If ok => ApplyChosenVersion so the offline block uses the chosen version for EXP/PP/loot/switches.
        // -----------------------------
        private static bool AutokillTitan6_BestUnlockedLeSelected(AdventureController ac)
        {
            var c = ac?.character;
            if (c?.adventure == null) return false;

            bool ok = ChooseBestLeSelected(c.adventure.titan6Version, ver =>
            {
                switch (ver)
                {
                    case 0: return ac.autokillTitan6V1Achieved();
                    case 1: return ac.autokillTitan6V2Achieved();
                    case 2: return ac.autokillTitan6V3Achieved();
                    case 3: return ac.autokillTitan6V4Achieved();
                    default: return false;
                }
            });

            if (ok) ApplyChosenVersion(c, 6, _chosenVersion);
            return ok;
        }

        private static bool AutokillTitan7_BestUnlockedLeSelected(AdventureController ac)
        {
            var c = ac?.character;
            if (c?.adventure == null) return false;

            bool ok = ChooseBestLeSelected(c.adventure.titan7Version, ver =>
            {
                switch (ver)
                {
                    case 0: return ac.autokillTitan7V1Achieved();
                    case 1: return ac.autokillTitan7V2Achieved();
                    case 2: return ac.autokillTitan7V3Achieved();
                    case 3: return ac.autokillTitan7V4Achieved();
                    default: return false;
                }
            });

            if (ok) ApplyChosenVersion(c, 7, _chosenVersion);
            return ok;
        }

        private static bool AutokillTitan8_BestUnlockedLeSelected(AdventureController ac)
        {
            var c = ac?.character;
            if (c?.adventure == null) return false;

            bool ok = ChooseBestLeSelected(c.adventure.titan8Version, ver =>
            {
                switch (ver)
                {
                    case 0: return ac.autokillTitan8V1Achieved();
                    case 1: return ac.autokillTitan8V2Achieved();
                    case 2: return ac.autokillTitan8V3Achieved();
                    case 3: return ac.autokillTitan8V4Achieved();
                    default: return false;
                }
            });

            if (ok) ApplyChosenVersion(c, 8, _chosenVersion);
            return ok;
        }

        private static bool AutokillTitan9_BestUnlockedLeSelected(AdventureController ac)
        {
            var c = ac?.character;
            if (c?.adventure == null) return false;

            bool ok = ChooseBestLeSelected(c.adventure.titan9Version, ver =>
            {
                switch (ver)
                {
                    case 0: return ac.autokillTitan9V1Achieved();
                    case 1: return ac.autokillTitan9V2Achieved();
                    case 2: return ac.autokillTitan9V3Achieved();
                    case 3: return ac.autokillTitan9V4Achieved();
                    default: return false;
                }
            });

            if (ok) ApplyChosenVersion(c, 9, _chosenVersion);
            return ok;
        }

        private static bool AutokillTitan10_BestUnlockedLeSelected(AdventureController ac)
        {
            var c = ac?.character;
            if (c?.adventure == null) return false;

            bool ok = ChooseBestLeSelected(c.adventure.titan10Version, ver =>
            {
                switch (ver)
                {
                    case 0: return ac.autokillTitan10V1Achieved();
                    case 1: return ac.autokillTitan10V2Achieved();
                    case 2: return ac.autokillTitan10V3Achieved();
                    case 3: return ac.autokillTitan10V4Achieved();
                    default: return false;
                }
            });

            if (ok) ApplyChosenVersion(c, 10, _chosenVersion);
            return ok;
        }

        private static bool AutokillTitan11_BestUnlockedLeSelected(AdventureController ac)
        {
            var c = ac?.character;
            if (c?.adventure == null) return false;

            bool ok = ChooseBestLeSelected(c.adventure.titan11Version, ver =>
            {
                switch (ver)
                {
                    case 0: return ac.autokillTitan11V1Achieved();
                    case 1: return ac.autokillTitan11V2Achieved();
                    case 2: return ac.autokillTitan11V3Achieved();
                    case 3: return ac.autokillTitan11V4Achieved();
                    default: return false;
                }
            });

            if (ok) ApplyChosenVersion(c, 11, _chosenVersion);
            return ok;
        }

        private static bool AutokillTitan12_BestUnlockedLeSelected(AdventureController ac)
        {
            var c = ac?.character;
            if (c?.adventure == null) return false;

            bool ok = ChooseBestLeSelected(c.adventure.titan12Version, ver =>
            {
                switch (ver)
                {
                    case 0: return ac.autokillTitan12V1Achieved();
                    case 1: return ac.autokillTitan12V2Achieved();
                    case 2: return ac.autokillTitan12V3Achieved();
                    case 3: return ac.autokillTitan12V4Achieved();
                    default: return false;
                }
            });

            if (ok) ApplyChosenVersion(c, 12, _chosenVersion);
            return ok;
        }

        // -----------------------------
        // Bestiary ID getters:
        // Use _chosenVersion (set by achieved helper) if available, else fall back to selected.
        // Base IDs are V1 IDs.
        // -----------------------------
        private static int GetChosenOrSelected(int? selected)
        {
            if (_chosenVersion >= 0 && _chosenVersion <= 3) return _chosenVersion;
            int s = selected ?? 0;
            return Clamp(s, 0, 3);
        }

        private static int Titan6BestiaryId(Character c) => 312 + GetChosenOrSelected(c?.adventure?.titan6Version);
        private static int Titan7BestiaryId(Character c) => 334 + GetChosenOrSelected(c?.adventure?.titan7Version);
        private static int Titan8BestiaryId(Character c) => 339 + GetChosenOrSelected(c?.adventure?.titan8Version);
        private static int Titan9BestiaryId(Character c) => 344 + GetChosenOrSelected(c?.adventure?.titan9Version);
        private static int Titan10BestiaryId(Character c) => 365 + GetChosenOrSelected(c?.adventure?.titan10Version);
        private static int Titan11BestiaryId(Character c) => 369 + GetChosenOrSelected(c?.adventure?.titan11Version);
        private static int Titan12BestiaryId(Character c) => 373 + GetChosenOrSelected(c?.adventure?.titan12Version);
    }

    [HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
    internal static class Patch_Offline_T5Loot
    {
        private const int MAX_ROLLS = 200;

        // IMPORTANT: class (reference type) to avoid Harmony003 "non-ref parameter modified" warnings
        private sealed class State
        {
            public long best310;
            public bool ok;
        }

        [HarmonyPrefix]
        private static void Prefix(Character __instance, out State __state)
        {
            __state = null;

            var b = __instance?.bestiary?.enemies;
            if (b == null) return;

            __state = new State
            {
                best310 = b[310].kills,
                ok = true
            };
        }

        [HarmonyPostfix]
        private static void Postfix(Character __instance, State __state)
        {
            if (__state == null || !__state.ok) return;

            var c = __instance;
            var b = c?.bestiary?.enemies;
            var advc = c?.adventureController;
            var ld = advc?.lootDrop;
            var enemyList = advc?.enemyList;

            if (b == null || ld == null || enemyList == null) return;

            int d310 = (int)Math.Max(0, b[310].kills - __state.best310);
            if (d310 <= 0) return;

            int rolls = Math.Min(d310, MAX_ROLLS);

            for (int i = 0; i < rolls; i++)
                ld.zone16Drop(enemyList[16][4]);

            if (d310 > MAX_ROLLS)
                Plugin.LogInfo($"[OfflineT5Loot] Capped T5 loot rolls: wanted {d310}, rolled {MAX_ROLLS}.");
        }
    }
}
