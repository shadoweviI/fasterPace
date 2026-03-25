using System;
using System.Collections;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;

namespace fasterPace
{
    internal static class NewVersionMonitor
    {
        private const string LatestApiUrl = "https://api.github.com/repos/shadoweviI/fasterPace/releases/latest";
        private const string ReleasesPageUrl = "https://github.com/shadoweviI/fasterPace/releases/latest";

        private static BaseUnityPlugin _plugin;
        private static Coroutine _routine;

        private static string _latestVersion;
        private static string _latestUrl = ReleasesPageUrl;

        private static Rect _area = new Rect(20f, 5f, 360f, 30f);

        private static ConfigEntry<bool> _enabled;
        private static ConfigEntry<string> _skippedVersion;

        internal static void Init(BaseUnityPlugin plugin, ConfigFile config)
        {
            _plugin = plugin;

            _enabled = config.Bind(
                "Updates",
                "CheckForUpdates",
                true,
                "Check GitHub releases for newer versions of fasterPace."
            );

            _skippedVersion = config.Bind(
                "Updates",
                "SkippedVersion",
                "",
                "Specific version to ignore in the update popup."
            );

            if (_enabled.Value && _routine == null)
                _routine = _plugin.StartCoroutine(CheckLoop());
        }

        private static IEnumerator CheckLoop()
        {
            yield return new WaitForSeconds(8f);

            while (true)
            {
                yield return CheckOnce();
                yield return new WaitForSeconds(21600f); // 6 hours
            }
        }

        private static IEnumerator CheckOnce()
        {
            using (var req = UnityWebRequest.Get(LatestApiUrl))
            {
                req.timeout = 15;
                req.SetRequestHeader("User-Agent", "fasterPace");

                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool failed = req.result != UnityWebRequest.Result.Success;
#else
                bool failed = req.isNetworkError || req.isHttpError;
#endif

                if (failed)
                {
                    Plugin.LogInfo("[UpdateCheck] Failed: " + req.error);
                    yield break;
                }

                string body = req.downloadHandler.text;
                if (string.IsNullOrEmpty(body))
                    yield break;

                string tag = ExtractJsonString(body, "tag_name");
                string htmlUrl = ExtractJsonString(body, "html_url");

                if (string.IsNullOrWhiteSpace(tag))
                    yield break;

                if (string.IsNullOrWhiteSpace(htmlUrl))
                    htmlUrl = ReleasesPageUrl;

                string current = NormalizeVersion(PluginInfo.PLUGIN_VERSION);
                string latest = NormalizeVersion(tag);
                string skipped = NormalizeVersion(_skippedVersion.Value);

                if (!string.IsNullOrEmpty(skipped) &&
                    string.Equals(latest, skipped, StringComparison.OrdinalIgnoreCase))
                {
                    yield break;
                }

                if (IsNewer(latest, current))
                {
                    _latestVersion = tag;
                    _latestUrl = htmlUrl;
                    Plugin.LogInfo("[UpdateCheck] New version found: " + tag + " (current: " + PluginInfo.PLUGIN_VERSION + ")");
                }
                else
                {
                    _latestVersion = null;
                    _latestUrl = ReleasesPageUrl;
                }
            }
        }

        private static string ExtractJsonString(string json, string key)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
                return null;

            try
            {
                string pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"";
                var match = Regex.Match(json, pattern, RegexOptions.CultureInvariant);

                if (!match.Success || match.Groups.Count < 2)
                    return null;

                return JsonUnescape(match.Groups[1].Value);
            }
            catch (Exception ex)
            {
                Plugin.LogWarning("[UpdateCheck] Failed to extract key '" + key + "': " + ex);
                return null;
            }
        }

        private static string JsonUnescape(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s
                .Replace("\\/", "/")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\r", "\r")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t");
        }

        private static string NormalizeVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return "0.0.0";

            version = version.Trim();
            if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                version = version.Substring(1);

            return version;
        }

        private static bool IsNewer(string latest, string current)
        {
            Version latestV;
            Version currentV;

            if (Version.TryParse(latest, out latestV) && Version.TryParse(current, out currentV))
                return latestV > currentV;

            return !string.Equals(latest, current, StringComparison.OrdinalIgnoreCase);
        }

        internal static void DrawGUI()
        {
            if (_enabled == null || !_enabled.Value)
                return;

            if (string.IsNullOrEmpty(_latestVersion))
                return;

            UIScaler.Begin();
            try
            {
                GUILayout.BeginArea(_area, GUI.skin.box);
                GUILayout.BeginHorizontal();

                GUILayout.Label("fasterPace update available: " + _latestVersion);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Open", GUILayout.Width(70f), GUILayout.Height(22f)))
                {
                    Application.OpenURL(_latestUrl);
                }

                if (GUILayout.Button("Skip", GUILayout.Width(70f), GUILayout.Height(22f)))
                {
                    _skippedVersion.Value = NormalizeVersion(_latestVersion);
                    _latestVersion = null;
                }

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
            finally
            {
                UIScaler.End();
            }
        }
    }
}