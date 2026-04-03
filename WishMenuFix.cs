using HarmonyLib;

namespace fasterPace
{
    /// <summary>
    /// Fixes the Wishes menu bars/text not visually updating unless you click a wish.
    ///
    /// Vanilla bug:
    /// updateAllBars() loops visible pods, but uses the loop index as a wish ID
    /// instead of using pod.id. So many visible bars never refresh correctly.
    ///
    /// This patch:
    /// 1) Fully replaces updateAllBars() so visible pods refresh using the correct wish ID.
    /// 2) Refreshes the top selected-wish text after updateAllWishes() ticks progress.
    /// </summary>
    internal static class WishMenuRefreshFix
    {
        [HarmonyPatch(typeof(WishesController), nameof(WishesController.updateAllBars))]
        internal static class Patch_WishesController_UpdateAllBars_Fix
        {
            [HarmonyPrefix]
            private static bool Prefix(WishesController __instance)
            {
                if (__instance == null || __instance.character == null)
                    return false;

                if (__instance.character.menuID != 53)
                    return false;

                if (__instance.pods == null)
                    return false;

                for (int i = 0; i < __instance.pods.Count; i++)
                {
                    var pod = __instance.pods[i];
                    if (pod == null || pod.invalidID())
                        continue;

                    int id = pod.id;

                    if (id < 0 || id >= __instance.character.wishes.wishes.Count)
                        continue;

                    // Refresh bars for visible wishes that are progressing,
                    // and also for maxed wishes so the bar display stays correct.
                    if ((long)__instance.character.wishes.wishes[id].level >= __instance.maxWishLevel(id))
                    {
                        pod.updateBar();
                        continue;
                    }

                    if (__instance.progressPerTick(id) > 0f)
                        pod.updateBar();
                }

                // Skip vanilla method entirely.
                return false;
            }
        }

        [HarmonyPatch(typeof(WishesController), "Start")]
        internal static class Patch_WishesController_Start_FasterBars
        {
            [HarmonyPostfix]
            private static void Postfix(WishesController __instance)
            {
                if (__instance == null)
                    return;

                // Cancel vanilla slow updates
                __instance.CancelInvoke("updateAllBars");

                // Run much more frequently (match wish ticking feel)
                __instance.InvokeRepeating("updateAllBars", 0f, 0.02f); // 20 FPS UI
            }
        }

        [HarmonyPatch(typeof(WishesController), nameof(WishesController.updateAllWishes))]
        internal static class Patch_WishesController_UpdateAllWishes_RefreshSelectedText
        {
            [HarmonyPostfix]
            private static void Postfix(WishesController __instance)
            {
                if (__instance == null || __instance.character == null)
                    return;

                if (__instance.character.menuID != 53)
                    return;

                // Keep the top panel live while wishes are ticking.
                __instance.updateWishPodText();
                __instance.updateEnergyPodText();
                __instance.updateMagicPodText();
                __instance.updateRes3PodText();
                __instance.updateWishSlotText();
            }
        }
    }
}