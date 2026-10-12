using HarmonyLib;
using RimWorld;
using Verse;

namespace ColonistPride
{
    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.SendCraftNotification),
        new[] { typeof(Thing), typeof(Pawn) })]
    public static class Patch_SendCraftNotification
    {
        public static void Postfix(Thing __0, Pawn __1)
        {
            ColonistPrideTracker.OnCreated(__0, __1);
        }
    }

    /*----------- Marks the window during which a SendCraftNotification call is coming from building construction. ----------*/
    [HarmonyPatch(typeof(Frame), "CompleteConstruction")]
    public static class Patch_Frame_CompleteConstruction
    {
        public static void Prefix()
        {
            ConstructionContext.Active = true;
        }

        /*----- Finalizer runs even if CompleteConstruction throws, so the flag can't get stuck on -----*/
        public static void Finalizer()
        {
            ConstructionContext.Active = false;
        }
    }
}