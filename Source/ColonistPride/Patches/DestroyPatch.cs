using System;
using HarmonyLib;
using Verse;

namespace ColonistPride
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.Destroy), new[] { typeof(DestroyMode) })]
    public static class Patch_Thing_Destroy
    {
        public static void Prefix(Thing __instance, DestroyMode __0)
        {
            try
            {
                ColonistPrideTracker.OnDestroying(__instance, __0);
            }
            catch (Exception ex)
            {
                Log.ErrorOnce("[Colonist Pride] Error in destroy hook: " + ex, 0x436F6C50);
            }
        }
    }
}