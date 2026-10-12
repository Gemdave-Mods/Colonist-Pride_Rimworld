using RimWorld;
using Verse;

namespace ColonistPride
{
    /*---------- True only while Frame.CompleteConstruction is running, so we can tell building completions from crafting. ----------*/
    public static class ConstructionContext
    {
        public static bool Active;
    }

    public static class ColonistPrideTracker
    {
        /*---------- Called from the SendCraftNotification postfix (crafting and construction both land here).----------*/
        public static void OnCreated(Thing thing, Pawn worker)
        {
            if (thing == null || worker == null) return;

            /*----- Constructed items are gated by a setting; if off, don't label them at all -----*/
            if (ConstructionContext.Active && !ColonistPrideMod.Settings.trackConstructedItems) return;

            var tag = thing.TryGetComp<CompColonistPrideTag>();
            if (tag == null || tag.labelled) return;

            var qualityComp = thing.TryGetComp<CompQuality>();
            if (qualityComp == null) return;

            QualityCategory quality = qualityComp.Quality;
            bool isArt = ColonistPrideUtility.IsArtItem(thing);

            /*----- Label every qualifying-source item permanently, whether or not Pride fires -----*/
            if (!tag.TryStamp(worker, quality, isArt)) return;

            if (ColonistPrideUtility.IsEligibleMaker(worker)
                && ColonistPrideUtility.Qualifies(ReactionType.Pride, quality, isArt))
            {
                ColonistPrideUtility.TryGiveMemory(worker, ReactionType.Pride, quality);
            }
        }

        /*---------- Called from the Thing.Destroy prefix, while the item is still spawned/held. ----------*/
        public static void OnDestroying(Thing thing, DestroyMode mode)
        {
            var withComps = thing as ThingWithComps;
            if (withComps == null || thing.Destroyed)
                return;

            var tag = withComps.GetComp<CompColonistPrideTag>();
            if (tag == null || !tag.labelled)
                return;

            Pawn maker = tag.maker;
            if (!ColonistPrideUtility.IsEligibleMaker(maker))
                return;

            if (!ColonistPrideUtility.TryGetDestroyReaction(mode, tag.quality, tag.isArt, out ReactionType type))
                return;

            /*----- Checked last because it's the heaviest test -----*/
            if (!ColonistPrideUtility.IsInColonyPossession(thing))
                return;

            ColonistPrideUtility.TryGiveMemory(maker, type, tag.quality);
        }
    }
}