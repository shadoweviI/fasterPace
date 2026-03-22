using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace fasterPace
{



    // Integrate into jshepler Drop Table Tooltip by patching BuildDropTable() output
    [HarmonyPatch]
    internal static class Patch_JsheplerDropTableTooltip_AddEarlyZoneSetDrops
    {
        internal const float STEALTHIEST_ARMOUR_NEW_CAP = 0.03f; 
        internal static bool _patchedJsheplerStealthCap;
        [HarmonyPrepare]
        private static bool Prepare()
        {
            // If jshepler isn't installed, don't let Harmony apply this patch at all.
            return AccessTools.TypeByName("jshepler.ngu.mods.ZoneDropsTooltip") != null;
        }


        private static MethodBase TargetMethod()
        {
            // jshepler.ngu.mods.ZoneDropsTooltip.BuildDropTable(int zoneId)
            var t = AccessTools.TypeByName("jshepler.ngu.mods.ZoneDropsTooltip");
            return t == null ? null : AccessTools.Method(t, "BuildDropTable", new[] { typeof(int) });
        }


        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(int zoneId, ref string __result)
        {
            if (string.IsNullOrEmpty(__result))
                return;

            var zdt = AccessTools.TypeByName("jshepler.ngu.mods.ZoneDropsTooltip");
            if (zdt == null) return;

            int offset = 0;
            try { offset = (int)AccessTools.Field(zdt, "_offset").GetValue(null); } catch { }

            float dcMulti = 0f;
            try { dcMulti = (float)AccessTools.Field(zdt, "_dcMulti").GetValue(null); } catch { }

            Func<int, string> nameFn = null;
            try { nameFn = (Func<int, string>)AccessTools.Field(zdt, "_name").GetValue(null); } catch { }
            if (nameFn == null) nameFn = id => $"Item {id}";
            TryPatchJsheplerStealthiestArmourCap();

            int actualZone = zoneId + offset;

            // ---- Integrate LootDropOverrides (PendantLootyDrops) tables into tooltip ----
            try
            {
                // Prevent duplicate injection if BuildDropTable() is called again while tooltip is open
                bool alreadyInjectedOverrides =
                    __result.Contains("\n\n<b>Replacements:</b>") ||
                    __result.Contains("\n\n<b>Added Drops:</b>");

                if (!alreadyInjectedOverrides)
                {
                    // 1) SWAPS (replacements)
                    var swapsField = AccessTools.Field(typeof(LootDropOverrides), "Swaps");
                    var swapsObj = swapsField?.GetValue(null) as System.Collections.IEnumerable;

                    string swapsText = "";
                    if (swapsObj != null)
                    {
                        foreach (var s in swapsObj)
                        {
                            if (s == null) continue;

                            int z = (int)AccessTools.Field(s.GetType(), "Zone").GetValue(s);
                            if (z != actualZone) continue;

                            bool titanOnly = (bool)AccessTools.Field(s.GetType(), "TitanOnly").GetValue(s);
                            bool bossOnly = (bool)AccessTools.Field(s.GetType(), "BossOnly").GetValue(s);

                            int origId = (int)AccessTools.Field(s.GetType(), "OriginalId").GetValue(s);
                            int newId = (int)AccessTools.Field(s.GetType(), "NewId").GetValue(s);
                            int newLevel = (int)AccessTools.Field(s.GetType(), "NewLevel").GetValue(s);

                            string scope =
                                titanOnly ? "Titan" :
                                bossOnly ? "Boss" : "Boss/Titan";

                            swapsText += $"\n  {scope}: {nameFn(origId)} -> {nameFn(newId)} (lvl {newLevel})";
                        }
                    }

                    if (!string.IsNullOrEmpty(swapsText))
                        __result += "\n\n<b>Replacements:</b>" + swapsText;

                    // 2) EXTRAS (added drops) - boss/titan only
                    var extrasField = AccessTools.Field(typeof(LootDropOverrides), "Extras");
                    var extrasObj = extrasField?.GetValue(null) as System.Collections.IEnumerable;

                    string extrasText = "";
                    if (extrasObj != null)
                    {
                        foreach (var r in extrasObj)
                        {
                            if (r == null) continue;

                            int z = (int)AccessTools.Field(r.GetType(), "Zone").GetValue(r);
                            if (z != actualZone) continue;

                            bool titanOnly = (bool)AccessTools.Field(r.GetType(), "TitanOnly").GetValue(r);
                            bool bossOnly = (bool)AccessTools.Field(r.GetType(), "BossOnly").GetValue(r);

                            int itemId = (int)AccessTools.Field(r.GetType(), "ItemId").GetValue(r);
                            int level = (int)AccessTools.Field(r.GetType(), "Level").GetValue(r);

                            float baseChance = (float)AccessTools.Field(r.GetType(), "Chance").GetValue(r);
                            bool useLootFactor = (bool)AccessTools.Field(r.GetType(), "UseLootFactor").GetValue(r);
                            float maxChance = (float)AccessTools.Field(r.GetType(), "MaxChance").GetValue(r);

                            // Match tooltip math: apply dcMulti only if rule uses loot factor
                            float raw2 = baseChance * (useLootFactor ? dcMulti : 1f);
                            float capped2 = (maxChance > 0f) ? Mathf.Min(maxChance, raw2) : raw2;
                            capped2 = Mathf.Clamp01(capped2);

                            // Green when capped (or if no cap, green at 100%)
                            bool atCap2 = (maxChance > 0f)
                                ? (raw2 >= maxChance - 1e-6f)
                                : (capped2 >= 1f - 1e-6f);

                            string color2 = atCap2 ? "green" : "red";
                            string pct2 = (capped2 * 100f).ToString("0.##") + "%";

                            string scope =
                                titanOnly ? "Titan" :
                                bossOnly ? "Boss" : "Boss/Titan";

                            extrasText += $"\n  {scope}: <color={color2}>{pct2}</color> {nameFn(itemId)} (lvl {level})";
                        }
                    }

                    if (!string.IsNullOrEmpty(extrasText))
                        __result += "\n\n<b>Added Drops:</b>" + extrasText;
                }
            }
            catch { /* never break the tooltip */ }

            // ---- Your existing Extra Normal Set Drops injection ----
            if (!TryGetEarlyZoneData(actualZone, out float coeff, out float cap, out int[] itemIds))
                return;

            float rawChance = (coeff * dcMulti) * GetNormalEnemyScale();
            float chance = Mathf.Min(cap, rawChance);
            if (chance <= 0f) return;

            if (__result.Contains("<b>Extra Normal Set Drops:</b>"))
                return;

            string color = (rawChance >= cap - 1e-6f) ? "green" : "red";
            string pct = (chance * 100f).ToString("0.##") + "%";

            string itemsBlock = string.Join("", itemIds.Select(id => "\n    " + nameFn(id)));

            int normalIdx = __result.IndexOf("<b>Normal Drops:</b>", StringComparison.Ordinal);
            if (normalIdx < 0)
            {
                __result += $"\n\n<b>Extra Normal Set Drops:</b>\n<b><color={color}>{pct}</color></b> for 1 of the following:{itemsBlock}";
                return;
            }

            int insertAt = FindNextSectionStart(__result, normalIdx);
            string injected =
                $"\n<b>Extra Normal Set Drops:</b>"
              + $"\n<b><color={color}>{pct}</color></b> for 1 of the following:{itemsBlock}";

            __result = __result.Insert(insertAt, injected);
        }



        private static void TryPatchJsheplerStealthiestArmourCap()
        {
            if (_patchedJsheplerStealthCap) return;

            try
            {
                // jshepler.ngu.mods.GameData.DropTable
                var dropTableType = AccessTools.TypeByName("jshepler.ngu.mods.GameData.DropTable");
                if (dropTableType == null) return;

                var zonesField = AccessTools.Field(dropTableType, "Zones");
                var zonesObj = zonesField?.GetValue(null) as System.Collections.IList;
                if (zonesObj == null || zonesObj.Count <= 18) return;

                // ZoneDrops at index 18
                var zoneDrops = zonesObj[18];
                if (zoneDrops == null) return;

                var bossDropsField = AccessTools.Field(zoneDrops.GetType(), "BossDrops");
                var bossDrops = bossDropsField?.GetValue(zoneDrops);
                if (bossDrops == null) return;

                var itemsField = AccessTools.Field(bossDrops.GetType(), "Items");
                var items = itemsField?.GetValue(bossDrops) as System.Array; // DropItems[]
                if (items == null) return;

                foreach (var di in items)
                {
                    if (di == null) continue;

                    var itemIdsField = AccessTools.Field(di.GetType(), "ItemIds");
                    var itemIds = itemIdsField?.GetValue(di) as int[];
                    if (itemIds == null) continue;

                    // Stealthiest Armour is item 178 (BAE_TheStealhiestArmor in jshepler enum)
                    if (Array.IndexOf(itemIds, 178) < 0) continue;

                    var maxDcField = AccessTools.Field(di.GetType(), "MaxDC");
                    if (maxDcField == null) return;

                    maxDcField.SetValue(di, STEALTHIEST_ARMOUR_NEW_CAP);

                    _patchedJsheplerStealthCap = true;
                    return;
                }
            }
            catch
            {
                // never break tooltip
            }
        }



        private static int FindNextSectionStart(string text, int startIdx)
        {
            // next major headers in jshepler tooltip
            int boss = text.IndexOf("\n\n<b>Boss Drops:</b>", startIdx, StringComparison.Ordinal);
            int titan = text.IndexOf("\n\n<b>Titan", startIdx, StringComparison.Ordinal);
            int extra = text.IndexOf("\n\n<b>Extra drops for", startIdx, StringComparison.Ordinal);
            int mac = text.IndexOf("\n\n<b>MacGuffin:", startIdx, StringComparison.Ordinal);
            int quest = text.IndexOf("\n\n<b>Quest", startIdx, StringComparison.Ordinal);

            int[] candidates = { boss, titan, extra, mac, quest };
            int next = candidates.Where(i => i >= 0).DefaultIfEmpty(text.Length).Min();
            return next;
        }

        // ---- Pull your data from FixEarlyZones.cs (no behavior changes; just reuse the same numbers) ----

        private static float GetNormalEnemyScale()
        {
            // Patch_ZoneItemsFromNormals has const NormalEnemyScale=0.33f, but const isn't reflectable.
            // Hardcode to match your file, or change it here if you change it there.
            return 0.33f;
        }

        private static bool TryGetEarlyZoneData(int zone, out float coeff, out float cap, out int[] itemIds)
        {
            coeff = 0f;
            cap = 0f;
            itemIds = null;

            // These are private static readonly dictionaries in Patch_ZoneItemsFromNormals
            var t = typeof(Patch_ZoneItemsFromNormals);

            var fCoeff = AccessTools.Field(t, "ZoneBossCoeff");
            var fCap = AccessTools.Field(t, "ZoneCapChance");
            var fItems = AccessTools.Field(t, "ZoneToBossSetItems");

            if (fCoeff == null || fCap == null || fItems == null)
                return false;

            var coeffDict = fCoeff.GetValue(null) as System.Collections.IDictionary;
            var capDict = fCap.GetValue(null) as System.Collections.IDictionary;
            var itemsDict = fItems.GetValue(null) as System.Collections.IDictionary;

            if (itemsDict == null || !itemsDict.Contains(zone))
                return false;

            itemIds = itemsDict[zone] as int[];
            if (itemIds == null || itemIds.Length == 0)
                return false;

            if (coeffDict != null && coeffDict.Contains(zone))
                coeff = (float)coeffDict[zone];
            if (capDict != null && capDict.Contains(zone))
                cap = (float)capDict[zone];

            if (cap <= 0f) cap = 0.10f;
            if (coeff <= 0f) return false;

            return true;
        }
    }
}
