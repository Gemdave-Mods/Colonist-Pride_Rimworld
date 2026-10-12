using RimWorld;

namespace ColonistPride
{
    [DefOf]
    public static class ColonistPrideDefOf
    {
        public static ThoughtDef ColonistPride_Pride;
        public static ThoughtDef ColonistPride_Heartbreak;
        public static ThoughtDef ColonistPride_Relief;

        static ColonistPrideDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ColonistPrideDefOf));
        }
    }
}