using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;

namespace fasterPace
{
    [HarmonyPatch]
    internal class NoNGUSoftcaps
    {
        internal const float EffNGULevel = (GeneralBuffs.GenSpeed * 2);
        public static float EffectiveMultForUI() => EffNGULevel * SadNoNGU_ScaledMult();
        private static Character character;
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void start(AdventureController __instance)
        {
            character = __instance.character;
        }

        // ============================================================
        // NGU TOOLTIP HELPERS (DISPLAY FIX)
        // ============================================================
        internal static class NGUTooltipHelpers
        {
            // Use the real cached character you already set in AdventureController.Start
            private static Character C => NoNGUSoftcaps.character;

            public static float SadNoNGU_ScaledMult()
            {
                var ch = C;
                if (ch?.settings == null || ch?.allChallenges == null)
                    return 1f;

                float mult = 1f;

                // Sadistic NGU challenge scaling
                if (ch.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    int c = ch.allChallenges.NGUChallenge.sadisticCompletions();
                    c = Mathf.Clamp(c, 0, 10);
                    mult += 0.05f * c;
                }

                // T12 Amalgamate set bonus
                if (ch.inventory?.itemList?.amalgamateComplete == true)
                {
                    mult *= 1.10f; // +10% NGU effectiveness
                }

                if (ch.inventory?.itemList?.pirateComplete == true)
                {
                    mult *= 1.25f; 
                }

                return mult;
            }

            // This is your "effective level" multiplier used by formulas.
            // If you ever change the 10f to something else, tooltips follow automatically.
            internal static float EffMult() => EffNGULevel * SadNoNGU_ScaledMult();

            private static bool LooksLikeNGUTooltip(string msg)
            {
                if (string.IsNullOrEmpty(msg)) return false;
                if (!msg.Contains("NGU")) return false;
                if (string.IsNullOrEmpty(msg)) return false;

                // must be NGU-related at all
                if (msg.IndexOf("NGU", StringComparison.OrdinalIgnoreCase) < 0) return false;

                // ---- EXCLUDE ITOPOD PERK TOOLTIPS ----
                // They include "COST:" and "Perk Points" (like your screenshot)
                if (msg.IndexOf("Perk Points", StringComparison.OrdinalIgnoreCase) >= 0) return false;
                if (msg.IndexOf("COST:", StringComparison.OrdinalIgnoreCase) >= 0) return false;

                // (optional) other common non-NGU panels you might not want scaled:
                // if (msg.IndexOf("Wish", StringComparison.OrdinalIgnoreCase) >= 0) return false;

                return true; // keep broad; we gate by regex matches anyway
            }

            private static string FormatScaledNumber(float value)
                => value.ToString("0.######", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');

            private static string ScaleIntString(string rawDigitsWithCommas, float mult)
            {
                if (string.IsNullOrEmpty(rawDigitsWithCommas)) return rawDigitsWithCommas;

                string raw = rawDigitsWithCommas.Replace(",", "");
                if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                    return rawDigitsWithCommas;

                double scaled = v * mult;
                long asInt = (long)Math.Ceiling(scaled);
                return asInt.ToString(CultureInfo.InvariantCulture);
            }

            private static readonly Regex PerLevelAnyRegex =
                new Regex(
                    @"(?ix)
(
    \b(?:increases?|increased)\s+by\s+ |
    \bmultipl(?:y|ies)\b.*?\bby\s+ |
    \bby\s+
)
\s*
([0-9]+(?:[.,][0-9]+)?(?:[eE][+\-]?[0-9]+)?)   # number (dot OR comma decimal)
\s*
(%?)                                            # optional %
\s*
(?:per|each)\s*(?:\r?\n\s*)?(?:level|lvl)\b
",
                    RegexOptions.Compiled | RegexOptions.CultureInvariant
                );




            // Matches: "Softcap starts at 1,000", "diminishing returns after level 500", etc.
            private static readonly Regex SoftcapRegex =
                new Regex(
                    @"(?ix)
            (
                softly?\s*diminishing\s*returns?\s+(?:start\s+)?(?:after|past|above)\s+ |
                sharply?\s*diminishing\s*returns?\s+(?:start\s+)?(?:after|past|above)\s+ |
                diminishing\s*returns?\s+(?:start\s+)?(?:after|past|above)\s+ |
                softcap(?:s)?(?:\s+starts)?\s+(?:at|after)\s+ |
                after\s+ |
                past\s+ |
                above\s+
            )
            (?:level(?:s)?\s+)?
            (\d{1,3}(?:,\d{3})*|\d+)",
                    RegexOptions.Compiled | RegexOptions.CultureInvariant
                );

            // NEW: explicitly scale "Effective Level: N" and "Effective Softcap: N" if present
            private static readonly Regex EffectiveLevelRegex =
                new Regex(@"(?ix)(\beffective\s+level\s*:\s*)(\d{1,3}(?:,\d{3})*|\d+)",
                    RegexOptions.Compiled | RegexOptions.CultureInvariant);

            private static readonly Regex EffectiveSoftcapRegex =
                new Regex(@"(?ix)(\beffective\s+softcap\s*:\s*)(\d{1,3}(?:,\d{3})*|\d+)",
                    RegexOptions.Compiled | RegexOptions.CultureInvariant);

            public static string ApplyAll(string msg)
            {
                if (!LooksLikeNGUTooltip(msg)) return msg;

                float mult = EffMult();
                if (mult <= 0f || Math.Abs(mult - 1f) < 0.0001f)
                    return msg;

                // (A) Scale explicit "Effective Level" / "Effective Softcap" lines (if your tooltips include them)
                msg = EffectiveLevelRegex.Replace(msg, m =>
                    m.Groups[1].Value + ScaleIntString(m.Groups[2].Value, mult), 1);

                msg = EffectiveSoftcapRegex.Replace(msg, m =>
                    m.Groups[1].Value + ScaleIntString(m.Groups[2].Value, mult), 1);

                // (B) Scale any "X per level" numbers (percent OR flat).
                // Use Replace WITHOUT the "count=1" so multiple NGU lines get fixed.
                msg = PerLevelAnyRegex.Replace(msg, m =>
                {
                    string prefix = m.Groups[1].Value;
                    string num = m.Groups[2].Value;
                    string pct = m.Groups[3].Value;

                    // Handle locales where ToString() uses comma decimals (e.g., "0,02")
                    string numNorm = num.Replace(',', '.');

                    if (!float.TryParse(numNorm, NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                        return m.Value;


                    float y = x * EffMult(); // per-level scales UP

                    return (pct == "%")
                        ? $"{prefix}{FormatScaledNumber(y)}% per level"
                        : $"{prefix}{FormatScaledNumber(y)} per level";
                });



                // (C) Scale softcap display to EFFECTIVE (multiply).
                // This replaces your previous "effective -> real" divide conversion.
                // 2) Replace softcap number if present (EFFECTIVE -> REAL, so DIVIDE)
                msg = SoftcapRegex.Replace(msg, m =>
                {
                    string prefix = m.Groups[1].Value;
                    string raw = m.Groups[2].Value.Replace(",", "");

                    if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var effCap))
                        return m.Value;

                    float mult = EffMult();
                    if (mult <= 0f) return m.Value;

                    int realCap = Mathf.CeilToInt(effCap / mult); // <-- DIVIDE
                    if (realCap < 1) realCap = 1;

                    return prefix + "level " + realCap.ToString(CultureInfo.InvariantCulture);
                }, 1);


                return msg;
            }
        }

        // ============================================================
        // HOVER TOOLTIP PATCHES (PATCH ALL STRING OVERLOADS, RUN FIRST)
        // ============================================================

        [HarmonyPatch(typeof(HoverTooltip))]
        internal static class Patch_HoverTooltip_AllStringOverloads
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                var t = typeof(HoverTooltip);
                var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var m in methods)
                {
                    if (m == null) continue;
                    if (m.IsAbstract) continue;

                    // We only care about showTooltip / showOverrideTooltip
                    if (!string.Equals(m.Name, "showTooltip", StringComparison.Ordinal) &&
                        !string.Equals(m.Name, "showOverrideTooltip", StringComparison.Ordinal))
                        continue;

                    var p = m.GetParameters();
                    if (p == null || p.Length < 1) continue;

                    // Only patch overloads where the *first* parameter is the tooltip string
                    if (p[0].ParameterType != typeof(string))
                        continue;

                    yield return m;
                }
            }

            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)]
            private static void Prefix(ref string __0)
            {
                __0 = NGUTooltipHelpers.ApplyAll(__0);
            }
        }


        [HarmonyPatch(typeof(HoverTooltip), "showTooltip", new[] { typeof(string), typeof(float) })]
        internal static class Patch_HoverTooltip_ShowTooltip_String_Float
        {
            [HarmonyPrefix]
            private static void Prefix(ref string __0)
            {
                __0 = NGUTooltipHelpers.ApplyAll(__0);
            }
        }

        [HarmonyPatch(typeof(HoverTooltip), "showTooltip", new[] { typeof(string), typeof(float), typeof(float) })]
        internal static class Patch_HoverTooltip_ShowTooltip_String_Float_Float
        {
            [HarmonyPrefix]
            private static void Prefix(ref string __0)
            {
                __0 = NGUTooltipHelpers.ApplyAll(__0);
            }
        }

        [HarmonyPatch(typeof(HoverTooltip), "showOverrideTooltip", new[] { typeof(string) })]
        internal static class Patch_HoverTooltip_ShowOverrideTooltip_String
        {
            [HarmonyPrefix]
            private static void Prefix(ref string __0)
            {
                __0 = NGUTooltipHelpers.ApplyAll(__0);
            }
        }

        [HarmonyPatch(typeof(HoverTooltip), "showOverrideTooltip", new[] { typeof(string), typeof(float) })]
        internal static class Patch_HoverTooltip_ShowOverrideTooltip_String_Float
        {
            [HarmonyPrefix]
            private static void Prefix(ref string __0)
            {
                __0 = NGUTooltipHelpers.ApplyAll(__0);
            }
        }

        [HarmonyPatch(typeof(AllNGUController), "Start")]
        internal static class Patch_AllNGUController_Start_AddEffMultText
        {
            private const string GO_NAME = "FP_EffMultText";

            [HarmonyPostfix]
            private static void Postfix(AllNGUController __instance)
            {
                try
                {
                    if (__instance == null) return;

                    // We only want to attach to the actual NGU panels.
                    // We locate them by the unique toggle button labels.
                    var root = __instance.transform;
                    if (root == null) return;

                    // Try find the toggle label within THIS controller hierarchy.
                    // If neither exists, we are not in an NGU panel (prevents showing in other menus).
                    var toggleText =
                        FindTextContains(root, "TO NGU MAGIC") ??
                        FindTextContains(root, "TO NGU ENERGY");

                    if (toggleText == null)
                        return;

                    // Find a decent "panel root" to parent under:
                    // climb a bit to a reasonably-sized RectTransform container.
                    Transform panelRoot = FindReasonablePanelRoot(toggleText.transform) ?? root;

                    // Find the NGU list panel (so it lands in your red-box area)
                    Transform listPanel = FindListPanel(panelRoot) ?? panelRoot;

                    // Reuse if already exists under this panel
                    var existing = listPanel.Find(GO_NAME);
                    Text txt;
                    if (existing != null)
                    {
                        txt = existing.GetComponent<Text>();
                        if (txt == null) return;

                        EnsureStyle(txt, GetTemplateText(panelRoot));
                        EnsurePosition(existing.GetComponent<RectTransform>());
                        EnsureUpdater(existing.gameObject, __instance, txt);
                        return;
                    }

                    // Template text from THIS NGU panel (keeps font/style correct)
                    var template = GetTemplateText(panelRoot);
                    if (template == null) return;

                    // Clone under list panel (so it hides/shows with NGU menu)
                    var go = UnityEngine.Object.Instantiate(template.gameObject, listPanel, false);
                    go.name = GO_NAME;

                    txt = go.GetComponent<Text>();
                    if (txt == null) return;

                    // Apply style + position + updater
                    EnsureStyle(txt, template);
                    EnsurePosition(go.GetComponent<RectTransform>());
                    EnsureUpdater(go, __instance, txt);
                }
                catch
                {
                    // never break UI
                }
            }

            // -------------------------
            // Updater component (refreshes text automatically)
            // -------------------------
            private sealed class EffMultTextUpdater : MonoBehaviour
            {
                public AllNGUController ngu;
                public Text txt;

                private float _next;

                private void Update()
                {
                    if (ngu == null || txt == null) return;

                    // update ~4x/sec (cheap)
                    if (Time.unscaledTime < _next) return;
                    _next = Time.unscaledTime + 0.25f;

                    try
                    {
                        var c = GetCharacter(ngu);
                        if (c == null || c.NGUController == null)
                        {
                            txt.text = "";
                            return;
                        }

                        float effMult = GetEffectiveMultForUI(c);
                        long cap = 0;
                        try { cap = c.NGUController.hardCapNormalLevel(); } catch { cap = 0; }

                        txt.text =
                            $"<b>EFFECTIVE NGU MULT:</b> x{effMult:0.###}    <b>MAX CAP:</b> {(cap > 0 ? cap.ToString("N0") : "Unknown")}";
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            private static void EnsureUpdater(GameObject go, AllNGUController ngu, Text txt)
            {
                var u = go.GetComponent<EffMultTextUpdater>();
                if (u == null) u = go.AddComponent<EffMultTextUpdater>();
                u.ngu = ngu;
                u.txt = txt;
            }

            // -------------------------
            // Style + position
            // -------------------------
            private static void EnsureStyle(Text txt, Text template)
            {
                if (txt == null) return;

                if (template != null)
                {
                    txt.font = template.font;
                    txt.fontStyle = template.fontStyle;
                    txt.fontSize = template.fontSize;
                }

                txt.supportRichText = true;
                txt.raycastTarget = false;

                // Match your screenshot vibe
                txt.alignment = TextAnchor.MiddleLeft;
                txt.horizontalOverflow = HorizontalWrapMode.Overflow;
                txt.verticalOverflow = VerticalWrapMode.Overflow;
                txt.color = new Color(0f, 0f, 0f, 0.85f);
            }

            private static void EnsurePosition(RectTransform rt)
            {
                if (rt == null) return;

                // bottom-left inside list panel (your red box)
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 0f);
                rt.pivot = new Vector2(0f, 0f);

                rt.anchoredPosition = new Vector2(12f, 8f);
                rt.sizeDelta = new Vector2(760f, 26f);
                rt.localScale = Vector3.one;
            }

            // -------------------------
            // Correct effective multiplier (includes Sadistic completion scaling)
            // -------------------------
            private static float GetEffectiveMultForUI(Character c)
            {
                // Your base eff level multiplier:
                float baseMult = EffNGULevel; // from your file

                // Sadistic scaling
                float sad = 1f;
                try
                {
                    if (c?.settings != null && c.allChallenges != null)
                    {
                        int comps = c.allChallenges.NGUChallenge.sadisticCompletions();
                        comps = Mathf.Clamp(comps, 0, 10);
                        sad = 1f + 0.05f * comps;
                    }
                }
                catch { sad = 1f; }

                // T12 Amalgamate set bonus
                float setMult = 1f;
                try
                {
                    if (c?.inventory?.itemList?.amalgamateComplete == true)
                        setMult = 1.10f;
                }
                catch { setMult = 1f; }

                float setMult1 = 1f;
                try
                {
                    if (c?.inventory?.itemList?.pirateComplete == true)
                        setMult = 1.25f;
                }
                catch { setMult = 1f; }

                return baseMult * sad * setMult * setMult1;
            }

            private static Character GetCharacter(AllNGUController nguUI)
            {
                try
                {
                    var f = AccessTools.Field(nguUI.GetType(), "character");
                    return f?.GetValue(nguUI) as Character;
                }
                catch { return null; }
            }

            // -------------------------
            // Helpers: find correct panel + template
            // -------------------------
            private static Text GetTemplateText(Transform panelRoot)
            {
                // Prefer a header label from inside NGU panel so font matches
                return FindTextExact(panelRoot, "NGU Name")
                       ?? FindTextExact(panelRoot, "Magic Allocated")
                       ?? FindTextExact(panelRoot, "Energy Allocated")
                       ?? panelRoot.GetComponentInChildren<Text>(true);
            }

            private static Transform FindReasonablePanelRoot(Transform from)
            {
                // Walk up a few parents looking for a big-ish rect (the NGU screen container).
                Transform t = from;
                for (int i = 0; i < 10 && t != null; i++)
                {
                    var rt = t as RectTransform;
                    if (rt != null)
                    {
                        float w = rt.rect.width;
                        float h = rt.rect.height;
                        if (w >= 700f && h >= 350f)
                            return t;
                    }
                    t = t.parent;
                }
                return null;
            }

            private static Transform FindListPanel(Transform panelRoot)
            {
                // Find the "NGU AUGMENTS" row and walk up to a large container
                var aug = FindTextContains(panelRoot, "NGU AUGMENTS");
                if (aug == null) return null;

                Transform t = aug.transform;
                for (int i = 0; i < 8 && t != null; i++)
                {
                    var rt = t as RectTransform;
                    if (rt != null && rt.rect.width > 600f && rt.rect.height > 250f)
                        return t;

                    t = t.parent;
                }

                return aug.transform.parent;
            }

            private static Text FindTextExact(Transform root, string exact)
            {
                var texts = root.GetComponentsInChildren<Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    var t = texts[i];
                    if (t != null && string.Equals(t.text?.Trim(), exact, StringComparison.Ordinal))
                        return t;
                }
                return null;
            }

            private static Text FindTextContains(Transform root, string contains)
            {
                var texts = root.GetComponentsInChildren<Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    var t = texts[i];
                    var s = t?.text;
                    if (string.IsNullOrEmpty(s)) continue;

                    // IMPORTANT: no Contains(StringComparison) in your target framework
                    if (s.IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0)
                        return t;
                }
                return null;
            }
        }



        private static float SadNoNGU_ScaledMult()
        {
            if (character == null || character.settings == null || character.allChallenges == null)
                return 1f;

            int c = character.allChallenges.NGUChallenge.sadisticCompletions();
            c = Mathf.Clamp(c, 0, 10);

            float mult = 1f + (0.05f * c);

            // T12 Amalgamate set bonus
            if (character.inventory?.itemList?.amalgamateComplete == true)
                mult *= 1.10f;
            if (character.inventory?.itemList?.pirateComplete == true)
                mult *= 1.25f;

            return mult;
        }

        internal static long GetRealNormalCap()
        {
            const double BASE_CAP = 1_000_000_000d;
            return (long)Math.Ceiling(BASE_CAP / EffNGULevel);
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), nameof(AllNGUController.hardCapNormalLevel))]
        private static bool hardCapNormalLevel(ref long __result)
        {
            __result = GetRealNormalCap();
            if (__result < 1L) __result = 1L;
            return false;
        }



        //
        //Energy NGUs
        //
        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "augmentBonusNormal")]
        private static bool augmentBonusNormal(AllNGUController __instance, ref double __result)
        {
            float level = character.NGU.skills[0].level;
            float scaled = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1.0 + (double)(scaled * __instance.normalEnergyBoostFactor[0]);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "augmentBonusEvil")]
        private static bool augmentBonusEvil(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.skills[0].evilLevel;
            float scaled = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1.0 + (double)(scaled * __instance.evilEnergyBoostFactor[0]);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "augmentBonusSadistic")]
        private static bool augmentBonusSadistic(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.skills[0].sadisticLevel;
            float scaled = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1.0 + (double)(scaled * __instance.sadisticEnergyBoostFactor[0]);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "wandoosBonusNormal")]
        private static bool wandoosBonusNormal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[1].level;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1f + eff * __instance.normalEnergyBoostFactor[1];
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "wandoosBonusEvil")]
        private static bool wandoosBonusEvil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[1].evilLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.evilEnergyBoostFactor[1];

            if (eff <= 1000f)
                __result = 1f + eff * factor;   // use eff here too (prevents jump)
            else
                __result = 1f + Mathf.Pow(eff, 0.25f) * 177.9f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "wandoosBonusSadistic")]
        private static bool wandoosBonusSadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[1].sadisticLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.sadisticEnergyBoostFactor[1];

            if (eff <= 1000f)
                __result = 1f + eff * factor;   // use eff here too (prevents jump)
            else
                __result = 1f + Mathf.Pow(eff, 0.15f) * 354.81f * factor;

            return false;
        }


        // ============================================================
        // RESPAWN NGU CURVE (Effective-cap anchored)
        // - Works in effective-level space: eff = level * EffNGULevel * SadNoNGU_ScaledMult()
        // - Uses EFFECTIVE_CAP (1e9) as the cap anchor (NOT hardCapNormalLevel())
        // - Linear until OUT0 is reached, then curves from OUT0 -> (MIN+TailDelta at 10% cap) -> MIN at cap
        // ============================================================

        private const float RESPAWN_EFFECTIVE_CAP = 1_000_000_000f; // eff-space cap anchor

        private static float Pow01(float x, float pow)
        {
            float y = Mathf.Clamp01(x);
            return (pow == 1f) ? y : Mathf.Pow(y, pow);
        }

        /// <summary>
        /// After linear reaches OUT0 at effFloor, transition:
        /// pFloor..pMid (10% cap): OUT0 -> mid (mid = min + tailDelta, clamped not above OUT0)
        /// pMid..1.0: mid -> MIN
        /// </summary>
        private static float CapAnchoredBonus(
            float eff,
            float effFloor,
            float out0,
            float min,
            float tailDelta,
            float headPow,
            float tailPow
        )
        {
            float cap = RESPAWN_EFFECTIVE_CAP;
            if (cap <= 0f) return out0;

            float pFloor = Mathf.Clamp01(effFloor / cap);
            float pMid = 0.10f;

            float mid = Mathf.Min(out0, min + tailDelta);

            float p = Mathf.Clamp01(eff / cap);

            // OUT0 -> mid from pFloor to 10% cap
            if (p <= pMid)
            {
                if (pFloor >= pMid) return mid; // safety
                float u = Mathf.InverseLerp(pFloor, pMid, p);
                u = Pow01(u, headPow);
                return Mathf.Lerp(out0, mid, u);
            }

            // mid -> MIN from 10% cap to cap
            float v = Mathf.InverseLerp(pMid, 1f, p);
            v = Pow01(v, tailPow);
            return Mathf.Lerp(mid, min, v);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "respawnBonusNormal")]
        private static bool respawnBonusNormal(AllNGUController __instance, ref float __result)
        {
            const float MIN = 0.50f;
            const float OUT0 = 0.80f;

            // Target behavior:
            // - At 10% effective cap, be around MIN + 0.05 => 0.35
            // - Then slowly approach MIN near cap.
            const float TailDelta = 0.05f;
            const float HeadPow = 0.55f; // OUT0 -> mid shaping (smaller = faster early drop)
            const float TailPow = 2.00f; // mid -> MIN shaping (bigger = slower approach to MIN)

            float level = character.NGU.skills[2].level;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();

            float k = __instance.normalEnergyBoostFactor[2];
            if (k <= 0f) { __result = OUT0; return false; }

            // Where linear would reach OUT0 (this is the "134" point in vanilla-ish normal)
            float effFloor = (1f - OUT0) / k;

            // Linear part (only while it's still above OUT0)
            if (eff <= effFloor)
            {
                float lin = 1f - eff * k;
                __result = (lin <= OUT0) ? OUT0 : lin;
                return false;
            }

            // Curve part (no plateau)
            __result = CapAnchoredBonus(eff, effFloor, OUT0, MIN, TailDelta, HeadPow, TailPow);
            if (__result < MIN) __result = MIN;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "respawnBonusEvil")]
        private static bool respawnBonusEvil(AllNGUController __instance, ref float __result)
        {
            const float MIN = 0.6f;
            const float OUT0 = 0.925f;

            // Same idea: at 10% eff cap, MIN + 0.05 => 0.8664966
            const float TailDelta = 0.05f;
            const float HeadPow = 0.55f;
            const float TailPow = 2.00f;

            float level = character.NGU.skills[2].evilLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();

            float k = __instance.evilEnergyBoostFactor[2];
            if (k <= 0f) { __result = OUT0; return false; }

            float effFloor = (1f - OUT0) / k;

            if (eff <= effFloor)
            {
                float lin = 1f - eff * k;
                __result = (lin <= OUT0) ? OUT0 : lin;
                return false;
            }

            __result = CapAnchoredBonus(eff, effFloor, OUT0, MIN, TailDelta, HeadPow, TailPow);
            if (__result < MIN) __result = MIN;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "respawnBonusSadistic")]
        private static bool respawnBonusSadistic(AllNGUController __instance, ref float __result)
        {
            const float MIN = 0.7f;
            const float OUT0 = 0.925f;

            const float TailDelta = 0.05f;
            const float HeadPow = 0.55f;
            const float TailPow = 2.00f;

            float level = character.NGU.skills[2].sadisticLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();

            float k = __instance.sadisticEnergyBoostFactor[2];
            if (k <= 0f) { __result = OUT0; return false; }

            float effFloor = (1f - OUT0) / k;

            if (eff <= effFloor)
            {
                float lin = 1f - eff * k;
                __result = (lin <= OUT0) ? OUT0 : lin;
                return false;
            }

            __result = CapAnchoredBonus(eff, effFloor, OUT0, MIN, TailDelta, HeadPow, TailPow);
            if (__result < MIN) __result = MIN;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "goldBonusNormal")]
        private static bool goldBonusNormal(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.skills[3].level;
            float scaled = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1f + scaled * __instance.normalEnergyBoostFactor[3];

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "goldBonusEvil")]
        private static bool goldBonusEvil(AllNGUController __instance, ref float __result)
        {

            float level = character.NGU.skills[3].evilLevel;
            float scaled = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1f + scaled * __instance.evilEnergyBoostFactor[3];

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "goldBonusSadistic")]
        private static bool goldBonusSadistic(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.skills[3].sadisticLevel;
            float scaled = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1f + scaled * __instance.sadisticEnergyBoostFactor[3];

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonusNormal")]
        private static bool adventureBonusNormal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[4].level;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.normalEnergyBoostFactor[4];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Sqrt(eff) * 31.7f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonusEvil")]
        private static bool adventureBonusEvil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[4].evilLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.evilEnergyBoostFactor[4];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.25f) * 177.9f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonusSadistic")]
        private static bool adventureBonusSadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[4].sadisticLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.sadisticEnergyBoostFactor[4];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.2f) * 251.19f * factor;

            return false;
        }



        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "alphaStatBonusNormal")]
        private static bool alphaStatBonusNormal(AllNGUController __instance, ref double __result)
        {
            float level = character.NGU.skills[5].level;
            float scaled = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1.0 + (double)(scaled * __instance.normalEnergyBoostFactor[5]);
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "alphaStatBonusEvil")]
        private static bool alphaStatBonusEvil(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.skills[5].evilLevel;
            float scaled = level * EffNGULevel;

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
            float scaled = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1.0 + (double)(scaled * __instance.sadisticEnergyBoostFactor[5]);
            return false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "lootBonusNormal")]
        private static bool lootBonusNormal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[6].level;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.normalEnergyBoostFactor[6];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Sqrt(eff) * 31.7f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "lootBonusEvil")]
        private static bool lootBonusEvil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[6].evilLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.evilEnergyBoostFactor[6];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.3f) * 125.9f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "lootBonusSadistic")]
        private static bool lootBonusSadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[6].sadisticLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.sadisticEnergyBoostFactor[6];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.2f) * 251.2f * factor;

            return false;
        }




        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "magicNGUBonusNormal")]
        private static bool magicNGUBonusNormal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[7].level;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.normalEnergyBoostFactor[7];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.3f) * 125.9f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "magicNGUBonusEvil")]
        private static bool magicNGUBonusEvil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[7].evilLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.evilEnergyBoostFactor[7];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.3f) * 125.9f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "magicNGUBonusSadistic")]
        private static bool magicNGUBonusSadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[7].sadisticLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.sadisticEnergyBoostFactor[7];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.1f) * 501.19f * factor;

            return false;
        }




        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "PPBonusNormal")]
        private static bool PPBonusNormal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[8].level;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.normalEnergyBoostFactor[8];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.3f) * 125.9f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "PPBonusEvil")]
        private static bool PPBonusEvil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[8].evilLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.evilEnergyBoostFactor[8];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.2f) * 251.2f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "PPBonusSadistic")]
        private static bool PPBonusSadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.skills[8].sadisticLevel;
            float eff = level * EffNGULevel* SadNoNGU_ScaledMult();
            float factor = __instance.sadisticEnergyBoostFactor[8];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.1f) * 501.21f * factor;

            return false;
        }


        //
        //Magic NGUs
        //

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "yggdrasilBonusNormal")]
        private static bool yggdrasilBonusNormal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.magicSkills[0].level;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.normalMagicBoostFactor[0];

            if (eff <= 400f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.33f) * 55.4f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "yggdrasilBonusEvil")]
        private static bool yggdrasilBonusEvil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.magicSkills[0].evilLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.evilMagicBoostFactor[0];

            if (eff <= 400f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.1f) * 219.72f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "yggdrasilBonusSadistic")]
        private static bool yggdrasilBonusSadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.magicSkills[0].sadisticLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.sadisticMagicBoostFactor[0];

            if (eff <= 400f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.08f) * 247.69f * factor;

            return false;
        }




        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "expBonusNormal")]
        private static bool expBonusNormal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.magicSkills[1].level;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = character.NGUController.normalMagicBoostFactor[1];

            if (eff <= 2000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.4f) * 95.66f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "expBonusEvil")]
        private static bool expBonusEvil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.magicSkills[1].evilLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = character.NGUController.evilMagicBoostFactor[1];

            if (eff <= 2000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.2f) * 437.35f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "expBonusSadistic")]
        private static bool expBonusSadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float level = character.NGU.magicSkills[1].sadisticLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = character.NGUController.sadisticMagicBoostFactor[1];

            if (eff <= 2000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.15f) * 639.56f * factor;

            return false;
        }




        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "betaStatBonusNormal")]
        private static bool betaStatBonusNormal(AllNGUController __instance, ref double __result)
        {
            float level = character.NGU.magicSkills[2].level;
            float scaled = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1.0 + (double)(scaled * __instance.normalMagicBoostFactor[2]);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "betaStatBonusEvil")]
        private static bool betaStatBonusEvil(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.magicSkills[2].evilLevel;
            float scaled = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1.0 + (double)(scaled * __instance.evilMagicBoostFactor[2]);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "betaStatBonusSadistic")]
        private static bool betaStatBonusSadistic(AllNGUController __instance, ref double __result)
        {

            float level = character.NGU.magicSkills[2].sadisticLevel;
            float scaled = level * EffNGULevel * SadNoNGU_ScaledMult();

            __result = 1.0 + (double)(scaled * __instance.sadisticMagicBoostFactor[2]);
            return false;
        }

        [HarmonyPatch]
        internal static class NGU_NumberBonus_VanillaStyle
        {
            // -------------------------
            // Normal: no-arg
            // -------------------------
            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllNGUController), "numberBonusNormal", new Type[] { })]
            private static bool numberBonusNormal(AllNGUController __instance, ref double __result)
            {
                if (character.NGU.disabled) { __result = 1.0; return false; }

                float level = character.NGU.magicSkills[3].level;
                float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
                float factor = __instance.normalMagicBoostFactor[3];

                if (eff <= 1000f)
                    __result = 1.0 + (double)(eff * factor) * character.timeMulti;
                else
                    __result = 1.0 + (double)(UnityEngine.Mathf.Pow(eff, 0.5f) * 31.7f * factor) * character.timeMulti;

                return false;
            }

            // Normal: bool overload (vanilla: NO timeMulti here)
            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllNGUController), "numberBonusNormal", new[] { typeof(bool) })]
            private static bool numberBonusNormal_bool(AllNGUController __instance, ref double __result, bool noTimeMulti)
            {
                if (character.NGU.disabled) { __result = 1.0; return false; }

                float level = character.NGU.magicSkills[3].level;
                float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
                float factor = __instance.normalMagicBoostFactor[3];

                if (eff <= 1000f)
                    __result = 1.0 + (double)(eff * factor);
                else
                    __result = 1.0 + (double)(UnityEngine.Mathf.Pow(eff, 0.5f) * 31.7f * factor);

                return false;
            }

            // -------------------------
            // Evil: no-arg
            // -------------------------
            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllNGUController), "numberBonusEvil", new Type[] { })]
            private static bool numberBonusEvil(AllNGUController __instance, ref double __result)
            {
                if (character.settings.rebirthDifficulty < difficulty.evil) { __result = 1.0; return false; }
                if (character.NGU.disabled) { __result = 1.0; return false; }

                float level = character.NGU.magicSkills[3].evilLevel;
                float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
                float factor = __instance.evilMagicBoostFactor[3];

                if (eff <= 1000f)
                    __result = 1.0 + (double)(eff * factor) * character.timeMulti;
                else
                    __result = 1.0 + (double)(UnityEngine.Mathf.Pow(eff, 0.3f) * 125.9f * factor) * character.timeMulti;

                return false;
            }

            // Evil: bool overload (vanilla: NO timeMulti here)
            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllNGUController), "numberBonusEvil", new[] { typeof(bool) })]
            private static bool numberBonusEvil_bool(AllNGUController __instance, ref double __result, bool noTimeMulti)
            {
                if (character.settings.rebirthDifficulty < difficulty.evil) { __result = 1.0; return false; }
                if (character.NGU.disabled) { __result = 1.0; return false; }

                float level = character.NGU.magicSkills[3].evilLevel;
                float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
                float factor = __instance.evilMagicBoostFactor[3];

                if (eff <= 1000f)
                    __result = 1.0 + (double)(eff * factor);
                else
                    __result = 1.0 + (double)(UnityEngine.Mathf.Pow(eff, 0.3f) * 125.9f * factor);

                return false;
            }

            // -------------------------
            // Sadistic: no-arg
            // -------------------------
            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllNGUController), "numberBonusSadistic", new Type[] { })]
            private static bool numberBonusSadistic(AllNGUController __instance, ref double __result)
            {
                if (character.settings.rebirthDifficulty < difficulty.sadistic) { __result = 1.0; return false; }
                if (character.NGU.disabled) { __result = 1.0; return false; }

                float level = character.NGU.magicSkills[3].sadisticLevel;
                float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
                float factor = __instance.sadisticMagicBoostFactor[3];

                if (eff <= 1000f)
                    __result = 1.0 + (double)(eff * factor) * character.timeMulti;
                else
                    __result = 1.0 + (double)(UnityEngine.Mathf.Pow(eff, 0.2f) * 251.2f * factor) * character.timeMulti;

                return false;
            }

            // Sadistic: bool overload (vanilla: NO timeMulti here)
            [HarmonyPrefix]
            [HarmonyPatch(typeof(AllNGUController), "numberBonusSadistic", new[] { typeof(bool) })]
            private static bool numberBonusSadistic_bool(AllNGUController __instance, ref double __result, bool noTimeMulti)
            {
                if (character.settings.rebirthDifficulty < difficulty.sadistic) { __result = 1.0; return false; }
                if (character.NGU.disabled) { __result = 1.0; return false; }

                float level = character.NGU.magicSkills[3].sadisticLevel;
                float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
                float factor = __instance.sadisticMagicBoostFactor[3];

                if (eff <= 1000f)
                    __result = 1.0 + (double)(eff * factor);
                else
                    __result = 1.0 + (double)(UnityEngine.Mathf.Pow(eff, 0.2f) * 251.2f * factor);

                return false;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "timeMachineBonusNormal")]
        private static bool timeMachineBonusNormal(AllNGUController __instance, ref double __result)
        {
            if (character.NGU.disabled) { __result = 1.0; return false; }

            float level = character.NGU.magicSkills[4].level;
            double eff = level * EffNGULevel * (double)SadNoNGU_ScaledMult();
            double factor = (double)__instance.normalMagicBoostFactor[4];

            if (eff <= 2000f)
                __result = 1.0 + eff * factor;
            else
                __result = 1.0 + Math.Pow(eff, 0.8) * 3.981 * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "timeMachineBonusEvil")]
        private static bool timeMachineBonusEvil(AllNGUController __instance, ref double __result)
        {
            if (character.NGU.disabled) { __result = 1.0; return false; }

            float level = character.NGU.magicSkills[4].evilLevel;
            double eff = level * EffNGULevel * (double)SadNoNGU_ScaledMult();
            double factor = (double)__instance.evilMagicBoostFactor[4];

            if (eff <= 2000f)
                __result = 1.0 + eff * factor;
            else
                __result = 1.0 + Math.Pow(eff, 0.8) * 3.981 * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "timeMachineBonusSadistic")]
        private static bool timeMachineBonusSadistic(AllNGUController __instance, ref double __result)
        {
            if (character.NGU.disabled) { __result = 1.0; return false; }

            float level = character.NGU.magicSkills[4].sadisticLevel;
            double eff = level * EffNGULevel * (double)SadNoNGU_ScaledMult();
            double factor = (double)__instance.sadisticMagicBoostFactor[4];

            if (eff <= 2000f)
                __result = 1.0 + eff * factor;
            else
                __result = 1.0 + Math.Pow(eff, 0.8) * 3.981 * factor;

            return false;
        }



        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "energyNGUBonusNormal")]
        private static bool energyNGUBonusNormal(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.magicSkills[5].level;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.normalMagicBoostFactor[5];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.3f) * 125.9f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "energyNGUBonusEvil")]
        private static bool energyNGUBonusEvil(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.magicSkills[5].evilLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.evilMagicBoostFactor[5];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.2f) * 251.2f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "energyNGUBonusSadistic")]
        private static bool energyNGUBonusSadistic(AllNGUController __instance, ref float __result)
        {
            float level = character.NGU.magicSkills[5].sadisticLevel;
            float eff = level * EffNGULevel * SadNoNGU_ScaledMult();
            float factor = __instance.sadisticMagicBoostFactor[5];

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.15f) * 354.82f * factor;

            return false;
        }




        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonus2Normal")]
        private static bool adventureBonus2Normal(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float lvl = character.NGU.magicSkills[6].level;
            float factor = __instance.normalMagicBoostFactor[6];

            // Apply your scaling consistently
            float eff = lvl * EffNGULevel * SadNoNGU_ScaledMult();

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.4f) * 63.13f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonus2Evil")]
        private static bool adventureBonus2Evil(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float lvl = character.NGU.magicSkills[6].evilLevel;
            float factor = __instance.evilMagicBoostFactor[6];

            float eff = lvl * EffNGULevel * SadNoNGU_ScaledMult();

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.25f) * 177.83f * factor;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "adventureBonus2Sadistic")]
        private static bool adventureBonus2Sadistic(AllNGUController __instance, ref float __result)
        {
            if (character.NGU.disabled) { __result = 1f; return false; }

            float lvl = character.NGU.magicSkills[6].sadisticLevel;
            float factor = __instance.sadisticMagicBoostFactor[6];

            float eff = lvl * EffNGULevel * SadNoNGU_ScaledMult();

            if (eff <= 1000f)
                __result = 1f + eff * factor;
            else
                __result = 1f + Mathf.Pow(eff, 0.12f) * 436.53f * factor;

            return false;
        }

    }
}
