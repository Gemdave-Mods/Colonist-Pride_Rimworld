using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ColonistPride
{
    public static class ColonistPrideUtility
    {
        /*----- Stage index in each ThoughtDef == index into these arrays -----*/
        private static readonly QualityCategory[] HighTiers =
            { QualityCategory.Excellent, QualityCategory.Masterwork, QualityCategory.Legendary };

        private static readonly QualityCategory[] LowTiers =
            { QualityCategory.Poor, QualityCategory.Awful };

        public static QualityCategory[] TiersFor(ReactionType type)
        {
            return type == ReactionType.Relief ? LowTiers : HighTiers;
        }

        /*---------- Returns the ThoughtDef stage index for this quality, or -1 if the reaction doesn't cover it. ----------*/
        public static int StageIndexFor(ReactionType type, QualityCategory quality)
        {
            return Array.IndexOf(TiersFor(type), quality);
        }

        public static ThoughtDef DefFor(ReactionType type)
        {
            switch (type)
            {
                case ReactionType.Pride: return ColonistPrideDefOf.ColonistPride_Pride;
                case ReactionType.Heartbreak: return ColonistPrideDefOf.ColonistPride_Heartbreak;
                default: return ColonistPrideDefOf.ColonistPride_Relief;
            }
        }

        public static bool TryGetReaction(ThoughtDef def, out ReactionType type)
        {
            if (def == ColonistPrideDefOf.ColonistPride_Pride) { type = ReactionType.Pride; return true; }
            if (def == ColonistPrideDefOf.ColonistPride_Heartbreak) { type = ReactionType.Heartbreak; return true; }
            if (def == ColonistPrideDefOf.ColonistPride_Relief) { type = ReactionType.Relief; return true; }
            type = ReactionType.Pride;
            return false;
        }

        /*---------- Art check based on ThingDef.IsArt (category-based). 
        GetInnerIfMinified is needed because a MinifiedThing's own def isn't the art def. ----------*/
        public static bool IsArtItem(Thing thing)
        {
            if (thing == null)
                return false;
            Thing inner = thing.GetInnerIfMinified();
            return inner != null && inner.def.IsArt;
        }

        /*---------- Does this reaction apply to an item of this quality / art status? (Tier + art rules only.) ----------*/
        public static bool Qualifies(ReactionType type, QualityCategory quality, bool isArt)
        {
            if (StageIndexFor(type, quality) < 0) return false;
            if (!isArt) return true;

            var s = ColonistPrideMod.Settings;
            if (type == ReactionType.Pride) return s.prideIncludesArt;
            if (type == ReactionType.Heartbreak) return s.heartbreakIncludesArt;
            return s.reliefIncludesArt;
        }

        /*---------- Maker eligibility: alive, and either spawned on a loaded map or in a player-controlled caravan. ----------*/
        public static bool IsEligibleMaker(Pawn pawn)
        {
            return pawn != null && !pawn.Dead && (pawn.Spawned || pawn.IsPlayerControlledCaravanMember());
        }

        /*---------- Gives the pawn one independent memory instance at the stage matching the quality. ----------*/
        public static bool TryGiveMemory(Pawn pawn, ReactionType type, QualityCategory quality)
        {
            int stage = StageIndexFor(type, quality);
            if (stage < 0) return false;

            var memories = pawn?.needs?.mood?.thoughts?.memories;
            if (memories == null) return false;

            ThoughtDef def = DefFor(type);
            if (def == null) return false;

            var memory = (Thought_Memory)ThoughtMaker.MakeThought(def, stage);
            memories.TryGainMemory(memory);
            return true;
        }

        /*---------- Maps a destroy mode + the item's label to a reaction, per the locked DestroyMode rules:
        Vanish / KillFinalize / KillFinalizeLeavingsOnly count normally;
        Deconstruct / Refund give Heartbreak only for Legendary (Relief still applies to Poor/Awful);
        everything else (QuestLogic, WillReplace, FailConstruction, Cancel) is excluded. ----------*/
        public static bool TryGetDestroyReaction(DestroyMode mode, QualityCategory quality, bool isArt, out ReactionType type)
        {
            type = ReactionType.Heartbreak;

            bool normal = mode == DestroyMode.Vanish
                          || mode == DestroyMode.KillFinalize
                          || mode == DestroyMode.KillFinalizeLeavingsOnly;
            bool gated = mode == DestroyMode.Deconstruct || mode == DestroyMode.Refund;
            if (!normal && !gated)
                return false;

            if (quality == QualityCategory.Poor || quality == QualityCategory.Awful)
            {
                type = ReactionType.Relief;
                return Qualifies(ReactionType.Relief, quality, isArt);
            }

            if (gated && quality != QualityCategory.Legendary)
                return false;

            type = ReactionType.Heartbreak;
            return Qualifies(ReactionType.Heartbreak, quality, isArt);
        }

        /*---------- Is this item still in the colony's possession? Spawned items count on player-home maps; 
        held items count when the holder chain leads to a colony-owned pawn (incl. prisoners/slaves), a player caravan, 
        or a spawned holder on a player-home map. Items in transit (no holder, not spawned), e.g. sold/gifted stock, do not count. ----------*/
        public static bool IsInColonyPossession(Thing thing)
        {
            if (thing == null)
                return false;
            if (thing.Spawned)
                return thing.Map != null && thing.Map.IsPlayerHome;

            IThingHolder holder = thing.ParentHolder;
            for (int depth = 0; holder != null && depth < 24; depth++)
            {
                if (holder is Pawn pawn)
                    return pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer;
                if (holder is Caravan caravan)
                    return caravan.IsPlayerControlled;
                if (holder is Map map)
                    return map.IsPlayerHome;
                if (holder is Thing holderThing && holderThing.Spawned)
                    return holderThing.Map != null && holderThing.Map.IsPlayerHome;

                holder = holder.ParentHolder;
            }
            return false;
        }

        /*---------- Pushes the max-stack settings onto the live ThoughtDefs. ----------*/
        public static void ApplyStackLimits()
        {
            foreach (ReactionType type in Enum.GetValues(typeof(ReactionType)))
            {
                ThoughtDef def = DefFor(type);
                if (def != null)
                {
                    def.stackLimit = ColonistPrideMod.Settings.Get(type).maxStack;
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class ColonistPrideStartup
    {
        static ColonistPrideStartup()
        {
            ColonistPrideUtility.ApplyStackLimits();
        }
    }
}