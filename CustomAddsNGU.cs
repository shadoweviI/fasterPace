// ============================================================
// CustomAddsNGU.cs
// NGU COOKING (Sadistic / Magic) - Custom independent NGU row
//
// IMPORTANT (your Assembly):
// - There is NO NGUMagicController.refreshMenu() in your build.
// - The correct menu rebuild hook is: AllNGUController.refreshMenu()
//
// What this file does:
// - Adds a new Magic NGU row called "NGU COOKING" (does NOT replace vanilla NGUs)
// - Positions it as its OWN row BELOW the last vanilla NGU row (no overlap)
// - Fully interactable (+ / - / Cap / request input) because we reuse the row UI
// - Tooltip works
// - Unlock: Itopod perk 34 (Sadistic only)
// - Cooking bonus: +0.001% per level (0.00001f per level)
// - Persistent via PlayerPrefs
// ============================================================

using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace fasterPace
{
    // ============================================================
    // Persistent State
    // ============================================================
    internal static class NGUCookingState
    {
        private const string KeyLevel = "fasterpace.ngu.cooking.level";
        private const string KeyProg = "fasterpace.ngu.cooking.progress";
        private const string KeyAlloc = "fasterpace.ngu.cooking.alloc";

        public static long Level;
        public static float Progress;      // 0..1
        public static long AllocatedMagic; // taken from character.magic.idleMagic

        private static bool _loaded;
        private static float _lastSave;

        public static void LoadOnce()
        {
            if (_loaded) return;
            _loaded = true;

            Level = PlayerPrefs.GetInt(KeyLevel, 0);
            Progress = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyProg, 0f));

            var s = PlayerPrefs.GetString(KeyAlloc, "0");
            long.TryParse(s, out AllocatedMagic);
            if (AllocatedMagic < 0) AllocatedMagic = 0;
        }

        public static void SaveSometimes()
        {
            if (Time.unscaledTime - _lastSave < 5f) return;
            _lastSave = Time.unscaledTime;

            PlayerPrefs.SetInt(KeyLevel, (int)Mathf.Clamp(Level, 0, int.MaxValue));
            PlayerPrefs.SetFloat(KeyProg, Mathf.Clamp01(Progress));
            PlayerPrefs.SetString(KeyAlloc, AllocatedMagic.ToString());
            PlayerPrefs.Save();
        }
    }

    // ============================================================
    // Helpers
    // ============================================================
    internal static class NGUCookingUtil
    {
        public static bool Unlocked(Character c)
        {
            var perk = c?.adventure?.itopod?.perkLevel;
            if (perk == null) return false;
            if (perk.Count <= 34) return false;
            return perk[34] > 0;
        }

        public static Transform FindMagicNGURowsParent()
        {
            // In your screenshots, rows live directly under this:
            // Canvas/NGU Magic Canvas/NGU Menu
            var menuGO = GameObject.Find("Canvas/NGU Magic Canvas/NGU Menu");
            if (menuGO == null) return null;

            var menu = menuGO.transform;

            // Some versions nest rows under a content object.
            // If we find a child with lots of NGU-looking rows, use it.
            Transform best = null;
            int bestCount = 0;

            // Check menu itself and its direct children
            CheckCandidate(menu, ref best, ref bestCount);

            for (int i = 0; i < menu.childCount; i++)
            {
                var ch = menu.GetChild(i);
                if (ch == null) continue;
                CheckCandidate(ch, ref best, ref bestCount);
            }

            return best ?? menu;

            static void CheckCandidate(Transform t, ref Transform best, ref int bestCount)
            {
                if (t == null) return;

                int count = 0;
                for (int i = 0; i < t.childCount; i++)
                {
                    var row = t.GetChild(i);
                    if (IsNGURow(row)) count++;
                }

                if (count > bestCount)
                {
                    bestCount = count;
                    best = t;
                }
            }
        }

        public static bool IsNGURow(Transform t)
        {
            if (t == null) return false;

            // A row typically has NGUMagicController + Slider + a few Buttons.
            return t.GetComponentInChildren<NGUMagicController>(true) != null
                && t.GetComponentInChildren<Slider>(true) != null
                && t.GetComponentsInChildren<Button>(true).Length >= 2;
        }

        public static Transform FindTemplateRow(Transform rowsParent)
        {
            if (rowsParent == null) return null;

            // Prefer a stable row by text (Power β usually exists and has all controls)
            for (int i = 0; i < rowsParent.childCount; i++)
            {
                var ch = rowsParent.GetChild(i);
                if (!IsNGURow(ch)) continue;

                var mg = ch.GetComponentInChildren<NGUMagicController>(true);
                var txt = mg != null ? mg.nguName : null;
                if (txt != null && !string.IsNullOrEmpty(txt.text) &&
                    txt.text.ToUpperInvariant().Contains("NGU POWER"))
                {
                    return ch;
                }
            }

            // Fallback: first NGU-looking row
            for (int i = 0; i < rowsParent.childCount; i++)
            {
                var ch = rowsParent.GetChild(i);
                if (IsNGURow(ch)) return ch;
            }

            return null;
        }

        public static void ForceRebuild(Transform t)
        {
            if (t is RectTransform rt)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        // --- Manual placement (the key fix for your overlap) ---

        public static RectTransform GetBottomMostRowRT(Transform parent)
        {
            RectTransform best = null;
            float bestY = float.PositiveInfinity; // most-down is usually most negative y

            if (parent == null) return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                var ch = parent.GetChild(i);
                if (!IsNGURow(ch)) continue;

                var rt = ch as RectTransform;
                if (rt == null) continue;

                float y = rt.anchoredPosition.y;
                if (y < bestY)
                {
                    bestY = y;
                    best = rt;
                }
            }

            return best;
        }

        public static float GetRowHeight(RectTransform rt)
        {
            if (rt == null) return 64f;

            float h = rt.rect.height;
            if (h > 1f) return h;

            // fallback if rect isn't valid yet
            return Mathf.Max(64f, rt.sizeDelta.y);
        }

        public static void PlaceRowBelowLast(Transform parent, RectTransform newRowRT)
        {
            if (parent == null || newRowRT == null) return;

            var bottom = GetBottomMostRowRT(parent);
            if (bottom == null) return;

            float rowH = GetRowHeight(bottom);

            // Put our row directly below the bottom-most vanilla row
            var bottomPos = bottom.anchoredPosition;
            newRowRT.anchoredPosition = new Vector2(newRowRT.anchoredPosition.x, bottomPos.y - rowH);

            // Expand parent height so it doesn't clip (for scroll/content containers)
            var parentRT = parent as RectTransform;
            if (parentRT != null)
            {
                float neededExtra = rowH + 8f;
                parentRT.sizeDelta = new Vector2(parentRT.sizeDelta.x, parentRT.sizeDelta.y + neededExtra);
            }
        }
    }

    // ============================================================
    // The Custom Row Controller
    // (drives UI + leveling; does NOT touch vanilla NGU arrays)
    // ============================================================
    internal sealed class NGUCookingBar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // Conservative progression (feel free to tune later)
        private const float TickSeconds = 0.02f;
        private const double Divider = 2e11;

        // 0.001% per level = 0.00001 multiplier per level
        public const float BonusPerLevel = 0.00001f;

        private Character _c;

        private NGUMagicController _ui;
        private Text _name;
        private Text _level;
        private Text _allocated;
        private InputField _requested;
        private Slider _slider;
        private HoverTooltip _tooltip;

        private Button _btnAdd;
        private Button _btnRemove;
        private Button _btnCap;

        private float _accum;
        private string _tip;

        public void Init(Character c, NGUMagicController ui)
        {
            _c = c;
            _ui = ui;

            NGUCookingState.LoadOnce();

            _name = _ui != null ? _ui.nguName : null;
            _level = _ui != null ? _ui.levelText : null;
            _allocated = _ui != null ? _ui.energyMagicText : null;
            _requested = _ui != null ? _ui.magicRequested : null;
            _slider = _ui != null ? _ui.slider : null;
            _tooltip = _ui != null ? _ui.tooltip : null;

            if (_name != null)
            {
                _name.text = "NGU COOKING";
                _name.color = new Color(0.55f, 0.25f, 0.7f);
            }

            _tip =
                "<b>NGU COOKING</b>\n\n" +
                "Boosts Cooking bonuses.\n\n" +
                "Effect: +0.001% per level\n" +
                "Unlock: Itopod Perk 34 (Sadistic).";

            WireButtons();
            RefreshUI();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltip != null) _tooltip.showTooltip(_tip);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_tooltip != null) _tooltip.hideTooltip();
        }

        private void Update()
        {
            if (_c == null) return;

            _accum += Time.unscaledDeltaTime;
            if (_accum < TickSeconds) return;
            _accum = 0f;

            // Gate
            if (_c.settings.rebirthDifficulty != difficulty.sadistic) { RefreshUI(); return; }
            if (!NGUCookingUtil.Unlocked(_c)) { RefreshUI(); return; }

            long alloc = NGUCookingState.AllocatedMagic;
            if (alloc <= 0) { RefreshUI(); return; }

            // Simple NGU-like progression
            double mp = _c.totalMagicPower();
            double lvl = Math.Max(1.0, (double)(NGUCookingState.Level + 1));

            double gain = (mp * (double)alloc) / lvl;
            gain /= Divider;

            // Respect NGU speed bonus (if you have it)
            gain *= _c.totalNGUSpeedBonus();
            gain *= TickSeconds;

            if (gain < 0) gain = 0;
            if (gain > 1000) gain = 1000;

            NGUCookingState.Progress += (float)gain;

            while (NGUCookingState.Progress >= 1f)
            {
                NGUCookingState.Progress -= 1f;
                NGUCookingState.Level++;
            }

            NGUCookingState.SaveSometimes();
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (_name != null) _name.text = "NGU COOKING";

            if (_c != null)
            {
                if (_allocated != null) _allocated.text = _c.display((double)NGUCookingState.AllocatedMagic);
                if (_level != null) _level.text = _c.display((double)NGUCookingState.Level);
            }

            if (_slider != null) _slider.value = Mathf.Clamp01(NGUCookingState.Progress);
        }

        private void WireButtons()
        {
            _btnAdd = null;
            _btnRemove = null;
            _btnCap = null;

            // Identify by their label text
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b == null) continue;
                var t = b.GetComponentInChildren<Text>(true);
                var s = (t != null ? (t.text ?? "") : "").Trim();

                if (_btnCap == null && s.Equals("Cap", StringComparison.OrdinalIgnoreCase)) _btnCap = b;
                else if (_btnRemove == null && s == "-") _btnRemove = b;
                else if (_btnAdd == null && (s == "+" || s == "++")) _btnAdd = b;
            }

            if (_btnAdd != null)
            {
                _btnAdd.onClick.RemoveAllListeners();
                _btnAdd.onClick.AddListener(OnAdd);
            }

            if (_btnRemove != null)
            {
                _btnRemove.onClick.RemoveAllListeners();
                _btnRemove.onClick.AddListener(OnRemove);
            }

            if (_btnCap != null)
            {
                _btnCap.onClick.RemoveAllListeners();
                _btnCap.onClick.AddListener(OnCap);
            }
        }

        private long ReadAmount()
        {
            if (_requested == null) return 0;

            string s = (_requested.text ?? "").Replace(",", "").Trim();
            if (!long.TryParse(s, out var v)) return 0;
            if (v < 0) v = 0;
            return v;
        }

        private void OnAdd()
        {
            if (_c == null) return;
            if (_c.settings.rebirthDifficulty != difficulty.sadistic) return;
            if (!NGUCookingUtil.Unlocked(_c)) return;

            long amt = ReadAmount();
            if (amt <= 0) amt = 1;

            long idle = _c.magic.idleMagic;
            if (idle <= 0) return;
            if (amt > idle) amt = idle;

            _c.magic.idleMagic -= amt;
            NGUCookingState.AllocatedMagic += amt;

            NGUCookingState.SaveSometimes();
            RefreshUI();
        }

        private void OnRemove()
        {
            if (_c == null) return;

            long amt = ReadAmount();
            if (amt <= 0) amt = 1;

            if (amt > NGUCookingState.AllocatedMagic)
                amt = NGUCookingState.AllocatedMagic;

            if (amt <= 0) return;

            NGUCookingState.AllocatedMagic -= amt;
            _c.magic.idleMagic += amt;

            NGUCookingState.SaveSometimes();
            RefreshUI();
        }

        private void OnCap()
        {
            if (_c == null) return;
            if (_c.settings.rebirthDifficulty != difficulty.sadistic) return;
            if (!NGUCookingUtil.Unlocked(_c)) return;

            // Put back current alloc then allocate 50% idle as a simple "cap"
            _c.magic.idleMagic += NGUCookingState.AllocatedMagic;
            NGUCookingState.AllocatedMagic = 0;

            long idle = _c.magic.idleMagic;
            if (idle <= 0) { RefreshUI(); return; }

            long amt = (long)(idle * 0.5f);
            if (amt < 0) amt = 0;
            if (amt > idle) amt = idle;

            _c.magic.idleMagic -= amt;
            NGUCookingState.AllocatedMagic = amt;

            NGUCookingState.SaveSometimes();
            RefreshUI();
        }
    }

    // ============================================================
    // UI Injection Patch (Correct target)
    // - Vanilla rebuilds/repositions rows for known NGUs only.
    // - We inject AFTER rebuild, THEN manually place below bottom row to avoid overlap.
    // ============================================================
    [HarmonyPatch(typeof(AllNGUController), "refreshMenu")]
    internal static class Patch_AllNGUController_RefreshMenu_AddNGUCooking
    {
        private static bool _running;

        [HarmonyPostfix]
        private static void Postfix(AllNGUController __instance)
        {
            if (__instance == null || __instance.character == null) return;
            if (_running) return;

            _running = true;
            __instance.StartCoroutine(InjectLater(__instance));
        }

        private static IEnumerator InjectLater(AllNGUController ctrl)
        {
            // Wait until vanilla finishes laying out & positioning this refresh.
            yield return null;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            _running = false;

            var c = ctrl?.character;
            if (c == null) yield break;

            if (c.settings.rebirthDifficulty != difficulty.sadistic) yield break;
            if (!NGUCookingUtil.Unlocked(c)) yield break;

            var rowsParent = NGUCookingUtil.FindMagicNGURowsParent();
            if (rowsParent == null) yield break;

            // If already exists, just re-place it below bottom (no overlap)
            var existing = rowsParent.Find("NGU_Cooking");
            if (existing != null)
            {
                var exRT = existing as RectTransform;
                if (exRT != null)
                    NGUCookingUtil.PlaceRowBelowLast(rowsParent, exRT);

                NGUCookingUtil.ForceRebuild(rowsParent);
                yield break;
            }

            // Choose a template row that has the right components/buttons/input
            var template = NGUCookingUtil.FindTemplateRow(rowsParent);
            if (template == null) yield break;

            // Clone
            var row = UnityEngine.Object.Instantiate(template.gameObject, rowsParent);
            row.name = "NGU_Cooking";
            row.SetActive(true);

            // Disable vanilla scripts on CLONE ONLY
            var mg = row.GetComponentInChildren<NGUMagicController>(true);
            if (mg != null) mg.enabled = false;

            var ngu = row.GetComponentInChildren<NGUController>(true);
            if (ngu != null) ngu.enabled = false;

            // Rename and color
            if (mg != null && mg.nguName != null)
            {
                mg.nguName.text = "NGU COOKING";
                mg.nguName.color = new Color(0.55f, 0.25f, 0.7f);
            }

            // Attach our controller
            var bar = row.GetComponent<NGUCookingBar>();
            if (bar == null) bar = row.AddComponent<NGUCookingBar>();
            bar.Init(c, mg);

            // CRITICAL: manually place below bottom-most row to prevent overlap
            var rowRT = row.transform as RectTransform;
            if (rowRT != null)
                NGUCookingUtil.PlaceRowBelowLast(rowsParent, rowRT);

            // Rebuild (safe)
            NGUCookingUtil.ForceRebuild(rowsParent);
        }
    }

    // ============================================================
    // Apply Cooking bonus
    // Bonus: 0.001% per level => multiplier = 1 + level * 0.00001
    // ============================================================
    [HarmonyPatch(typeof(CookingController), "totalCookingBonuses")]
    internal static class Patch_CookingController_totalCookingBonuses_NGUCooking
    {
        [HarmonyPostfix]
        private static void Postfix(CookingController __instance, ref float __result)
        {
            var c = __instance?.character;
            if (c == null) return;

            if (c.settings.rebirthDifficulty != difficulty.sadistic) return;
            if (!NGUCookingUtil.Unlocked(c)) return;

            NGUCookingState.LoadOnce();

            __result *= 1f + (NGUCookingState.Level * NGUCookingBar.BonusPerLevel);
        }
    }
}
