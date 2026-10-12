using System.Linq;
using RimWorld;
using Verse;

namespace ColonistPride
{
    [StaticConstructorOnStartup]
    public static class CompInjector
    {
        static CompInjector()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.comps == null) continue;

                bool hasQuality = def.comps.Any(c => c.compClass != null
                                                     && typeof(CompQuality).IsAssignableFrom(c.compClass));
                if (!hasQuality) continue;

                if (def.comps.Any(c => c is CompProperties_ColonistPrideTag)) continue;

                def.comps.Add(new CompProperties_ColonistPrideTag());
            }
        }
    }
}