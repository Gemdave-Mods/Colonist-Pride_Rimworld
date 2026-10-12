using RimWorld;
using UnityEngine;
using Verse;

namespace ColonistPride
{
    /*---------- Shared memory class for Pride / Heartbreak / Relief. Per-tier duration and magnitude come from settings. ----------*/
    public class Thought_TierMemory : Thought_Memory
    {
        private TierSetting TierSettings()
        {
            if (!ColonistPrideUtility.TryGetReaction(def, out ReactionType type)) return null;

            ReactionSettings rs = ColonistPrideMod.Settings?.Get(type);
            int i = CurStageIndex;
            if (rs == null || i < 0 || i >= rs.tiers.Count) return null;
            return rs.tiers[i];
        }

        public override int DurationTicks
        {
            get
            {
                TierSetting tier = TierSettings();
                if (tier == null) return base.DurationTicks;
                return Mathf.Max(1, Mathf.RoundToInt(tier.durationDays * GenDate.TicksPerDay));
            }
        }

        public override float MoodOffset()
        {
            /*----- Keep all native multipliers, then rescale from the XML default to the configured magnitude -----*/
            float baseOffset = base.MoodOffset();
            TierSetting tier = TierSettings();
            float xmlMagnitude = CurStage != null ? CurStage.baseMoodEffect : 0f;
            if (tier == null || xmlMagnitude == 0f) return baseOffset;
            return baseOffset * (tier.magnitude / xmlMagnitude);
        }

        /*----- Never merge: every trigger is an independent memory. The native stackLimit eviction in MemoryThoughtHandler.
        TryGainMemory (oldest removed) is the only cap. -----*/
        public override bool TryMergeWithExistingMemory(out bool showBubble)
        {
            showBubble = true;
            return false;
        }
    }
}