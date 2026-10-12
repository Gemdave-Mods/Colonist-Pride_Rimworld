using RimWorld;
using Verse;

namespace ColonistPride
{
    public class CompProperties_ColonistPrideTag : CompProperties
    {
        public CompProperties_ColonistPrideTag()
        {
            compClass = typeof(CompColonistPrideTag);
        }
    }

    public class CompColonistPrideTag : ThingComp
    {
        public bool labelled;
        public Pawn maker;
        public QualityCategory quality = QualityCategory.Normal;
        public bool isArt;

        /*---------- Stamps the label. No-op if the item was already labelled (permanent after first fire). ----------*/
        public bool TryStamp(Pawn maker, QualityCategory quality, bool isArt)
        {
            if (labelled) return false;
            labelled = true;
            this.maker = maker;
            this.quality = quality;
            this.isArt = isArt;
            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref labelled, "labelled", false);
            Scribe_Values.Look(ref quality, "quality", QualityCategory.Normal);
            Scribe_Values.Look(ref isArt, "isArt", false);

            /*----- A discarded pawn can't be resolved on load, so don't save a dangling reference -----*/
            if (Scribe.mode == LoadSaveMode.Saving && maker != null && maker.Discarded)
            {
                maker = null;
            }
            Scribe_References.Look(ref maker, "maker");
        }
    }
}