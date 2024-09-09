using Verse;

namespace LTF_Teleport;

public class CompProperties_LTF_TpBench : CompProperties
{
    public readonly float FacilityCapacityBase = 1f;
    public readonly float FacilityCapacitySpectrum = 16f;

    public readonly float moreRange = 0.6f;

    public readonly float moreRangeBase = 1f;

    public CompProperties_LTF_TpBench()
    {
        compClass = typeof(Comp_LTF_TpBench);
    }
}