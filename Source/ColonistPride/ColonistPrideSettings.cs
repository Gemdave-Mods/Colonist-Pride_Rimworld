using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ColonistPride
{
    public enum ReactionType { Pride, Heartbreak, Relief }

    public class TierSetting : IExposable
    {
        public float magnitude;
        public float durationDays;

        public TierSetting() { }

        public TierSetting(float magnitude, float durationDays)
        {
            this.magnitude = magnitude;
            this.durationDays = durationDays;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref magnitude, "magnitude");
            Scribe_Values.Look(ref durationDays, "durationDays");
        }
    }

    public class ReactionSettings : IExposable
    {
        /*----- Index matches the ThoughtDef stage index (see ColonistPrideUtility.TiersFor) -----*/
        public List<TierSetting> tiers = new List<TierSetting>();
        public int maxStack = 1;

        public static ReactionSettings CreateDefault(ReactionType type)
        {
            var rs = new ReactionSettings();
            switch (type)
            {
                case ReactionType.Pride:
                    rs.tiers.Add(new TierSetting(2f, 2f));   // Excellent
                    rs.tiers.Add(new TierSetting(4f, 3f));   // Masterwork
                    rs.tiers.Add(new TierSetting(6f, 5f));   // Legendary
                    rs.maxStack = 3;
                    break;
                case ReactionType.Heartbreak:
                    rs.tiers.Add(new TierSetting(-2f, 2f));  // Excellent
                    rs.tiers.Add(new TierSetting(-4f, 3f));  // Masterwork
                    rs.tiers.Add(new TierSetting(-6f, 5f));  // Legendary
                    rs.maxStack = 3;
                    break;
                case ReactionType.Relief:
                    rs.tiers.Add(new TierSetting(1f, 1f));   // Poor
                    rs.tiers.Add(new TierSetting(2f, 2f));   // Awful
                    rs.maxStack = 2;
                    break;
            }
            return rs;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref maxStack, "maxStack", 1);
            Scribe_Collections.Look(ref tiers, "tiers", LookMode.Deep);
        }
    }

    public class ColonistPrideSettings : ModSettings
    {
        public ReactionSettings pride = ReactionSettings.CreateDefault(ReactionType.Pride);
        public ReactionSettings heartbreak = ReactionSettings.CreateDefault(ReactionType.Heartbreak);
        public ReactionSettings relief = ReactionSettings.CreateDefault(ReactionType.Relief);

        public bool trackConstructedItems = true;

        /*----- Reserved / not shown in UI yet -----*/
        public bool prideIncludesArt = false;
        public bool heartbreakIncludesArt = false;
        public bool reliefIncludesArt = true;
        public bool reactToNormalAndGood = false;
        public bool dynamicIntensity = false;
        /*--------------------------------------------------*/

        public ReactionSettings Get(ReactionType type)
        {
            switch (type)
            {
                case ReactionType.Pride: return pride;
                case ReactionType.Heartbreak: return heartbreak;
                default: return relief;
            }
        }

        public void ResetToDefaults()
        {
            pride = ReactionSettings.CreateDefault(ReactionType.Pride);
            heartbreak = ReactionSettings.CreateDefault(ReactionType.Heartbreak);
            relief = ReactionSettings.CreateDefault(ReactionType.Relief);
            trackConstructedItems = true;
        }

        /*----- Repairs missing/malformed data (e.g. first run, or settings from an older version). -----*/
        public void Validate()
        {
            EnsureValid(ref pride, ReactionType.Pride);
            EnsureValid(ref heartbreak, ReactionType.Heartbreak);
            EnsureValid(ref relief, ReactionType.Relief);
        }

        private static void EnsureValid(ref ReactionSettings rs, ReactionType type)
        {
            int expected = ColonistPrideUtility.TiersFor(type).Length;
            if (rs == null || rs.tiers == null || rs.tiers.Count != expected || rs.tiers.Contains(null))
            {
                rs = ReactionSettings.CreateDefault(type);
            }
            rs.maxStack = Mathf.Clamp(rs.maxStack, 1, 20);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref pride, "pride");
            Scribe_Deep.Look(ref heartbreak, "heartbreak");
            Scribe_Deep.Look(ref relief, "relief");
            Scribe_Values.Look(ref trackConstructedItems, "trackConstructedItems", true);

            Scribe_Values.Look(ref prideIncludesArt, "prideIncludesArt", false);
            Scribe_Values.Look(ref heartbreakIncludesArt, "heartbreakIncludesArt", false);
            Scribe_Values.Look(ref reliefIncludesArt, "reliefIncludesArt", true);
            Scribe_Values.Look(ref reactToNormalAndGood, "reactToNormalAndGood", false);
            Scribe_Values.Look(ref dynamicIntensity, "dynamicIntensity", false);
        }
    }
}