using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace fasterPace
{
    [HarmonyPatch]
    internal static class FasterPaceMenuLabel
    {
        private const string ObjName = "fasterpace_version_label";
        private static string LabelText => $"(fasterpace {PluginInfo.PLUGIN_VERSION})";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuController), "Start")]
        private static void MainMenuController_Start_Postfix(MainMenuController __instance)
            => TryCreateOrUpdate(__instance);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuController), nameof(MainMenuController.updateMiscText))]
        private static void MainMenuController_updateMiscText_Postfix(MainMenuController __instance)
            => TryCreateOrUpdate(__instance);

        private static void TryCreateOrUpdate(MainMenuController mm)
        {
            if (mm == null) return;

            var buildTxt = mm.buildText;
            if (buildTxt == null) return;

            var buildRT = buildTxt.GetComponent<RectTransform>();
            if (buildRT == null) return;

            // Search within the same UI root as the build text (reliable; avoids random global matches)
            var uiRoot = buildTxt.transform.root;
            if (uiRoot == null) return;

            var texts = uiRoot.GetComponentsInChildren<Text>(true);

            Text infoLabel = null;
            Text saveLabel = null;

            // Prefer ACTIVE matches (so we don't grab a hidden/load-panel copy)
            for (int i = 0; i < texts.Length; i++)
            {
                var t = texts[i];
                if (t == null || string.IsNullOrEmpty(t.text)) continue;
                if (!t.gameObject.activeInHierarchy) continue;

                if (infoLabel == null && t.text == "Info 'n Stuff") infoLabel = t;
                else if (saveLabel == null && t.text == "Save Game") saveLabel = t;

                if (infoLabel != null && saveLabel != null) break;
            }

            // Fallback if exact text differs
            if (infoLabel == null || saveLabel == null)
            {
                for (int i = 0; i < texts.Length; i++)
                {
                    var t = texts[i];
                    if (t == null || string.IsNullOrEmpty(t.text)) continue;
                    if (!t.gameObject.activeInHierarchy) continue;

                    if (infoLabel == null && t.text.Contains("Info")) infoLabel = t;
                    if (saveLabel == null && t.text.Contains("Save")) saveLabel = t;

                    if (infoLabel != null && saveLabel != null) break;
                }
            }

            if (infoLabel == null || saveLabel == null) return;

            var infoRT = infoLabel.GetComponentInParent<RectTransform>();
            var saveRT = saveLabel.GetComponentInParent<RectTransform>();
            if (infoRT == null || saveRT == null) return;

            // Parent under the same parent as the Info button (so it lives in that menu area)
            // Parent under the same parent as the Info button (so it lives in that menu area)
            Transform parent = infoRT.parent;
            if (parent == null) return;

            Text label;
            var existing = parent.Find(ObjName);
            if (existing != null)
            {
                label = existing.GetComponent<Text>();
                if (label == null) return;
            }
            else
            {
                var go = Object.Instantiate(buildTxt.gameObject, parent);
                go.name = ObjName;

                label = go.GetComponent<Text>();
                if (label == null) return;
            }

            // Ensure it's visible
            label.gameObject.SetActive(true);
            label.enabled = true;

            // ---- Styling (font 12, black, centered) ----
            label.text = LabelText;
            label.supportRichText = false;
            label.alignment = TextAnchor.MiddleCenter;

            label.resizeTextForBestFit = false;
            label.fontSize = 12;
            label.fontStyle = FontStyle.Normal;

            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Truncate;

            label.color = Color.black;
            label.raycastTarget = false;

            var rt = label.GetComponent<RectTransform>();
            if (rt == null) return;

            // Width: match Build text width (keeps it aligned with Build + jshepler line)
            rt.sizeDelta = new Vector2(buildRT.sizeDelta.x, 22f);

            // ---- Position: EXACT gap midpoint (red box) ----
            float saveBottomWorld = GetWorldBottom(saveRT);
            float infoTopWorld = GetWorldTop(infoRT);
            float midWorldY = (saveBottomWorld + infoTopWorld) * 0.5f;

            // X alignment from Build text (matches Build + jshepler)
            Vector3 worldPos = buildRT.position;
            worldPos.y = midWorldY;

            // move left (tweak this value)
            worldPos.x -= 38f;

            rt.position = worldPos;
            rt.SetAsLastSibling();

        }

        private static float GetWorldTop(RectTransform r)
        {
            var corners = new Vector3[4];
            r.GetWorldCorners(corners);
            return corners[1].y; // TL
        }

        private static float GetWorldBottom(RectTransform r)
        {
            var corners = new Vector3[4];
            r.GetWorldCorners(corners);
            return corners[0].y; // BL
        }
    }
}
