using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace fasterPace
{
    internal static class UIScaler
    {
        private static Matrix4x4? _originalMatrix;
        private static Dictionary<GUIStyle, int> _originalFontSizes;

        private static float _cachedScale = -1f;
        private static int _lastScreenWidth;
        private static int _lastScreenHeight;
        private static float _lastDpi;

        internal static float CurrentScale()
        {
            float dpi = Screen.dpi > 0 ? Screen.dpi : 96f;

            if (_cachedScale < 0f
                || Screen.width != _lastScreenWidth
                || Screen.height != _lastScreenHeight
                || Mathf.Abs(dpi - _lastDpi) > 0.1f)
            {
                _cachedScale = DetectUIScale();
                _lastScreenWidth = Screen.width;
                _lastScreenHeight = Screen.height;
                _lastDpi = dpi;
            }

            return _cachedScale;
        }

        internal static void Begin()
        {
            if (_originalMatrix == null)
                _originalMatrix = GUI.matrix;

            float scale = CurrentScale();
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

            if (_originalFontSizes == null)
            {
                _originalFontSizes = new Dictionary<GUIStyle, int>();
                foreach (GUIStyle style in GUI.skin)
                    _originalFontSizes[style] = style.fontSize;
            }

            foreach (GUIStyle style in GUI.skin)
            {
                if (style.fontSize > 0)
                    style.fontSize = Mathf.RoundToInt(_originalFontSizes[style] * scale);
            }
        }

        internal static void End()
        {
            if (_originalMatrix.HasValue)
                GUI.matrix = _originalMatrix.Value;

            if (_originalFontSizes != null)
            {
                foreach (var kvp in _originalFontSizes)
                    kvp.Key.fontSize = kvp.Value;
            }
        }

        private static float DetectUIScale()
        {
            CanvasScaler[] scalers = Object.FindObjectsOfType<CanvasScaler>();
            CanvasScaler scaler = scalers != null && scalers.Length > 0
                ? scalers.OrderByDescending(s => s.GetComponent<Canvas>().scaleFactor).FirstOrDefault()
                : null;

            float dpi = Screen.dpi > 0 ? Screen.dpi : 96f;

            if (scaler != null)
            {
                float scale = scaler.GetComponent<Canvas>().scaleFactor;

                if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize)
                    scale *= dpi / 96f;

                return scale;
            }

            return dpi / 96f;
        }
    }
}