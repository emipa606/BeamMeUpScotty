using Verse;

namespace LTF_Teleport;

public class CompProperties_LTF_TpSpot : CompProperties
{
    public readonly float benchSynergyBase = 0.85f;

    public readonly float benchSynergyQualityFactor = 0.08f;

    public readonly int cooldownBase = 900;

    public readonly int cooldownQualityFactor = -120;

    public readonly bool debug = false;

    public readonly float rangeBase = 10f;

    public readonly float rangeQualityFactor = 1.2f;

    public readonly bool requiresFacility = true;

    public readonly bool requiresPower = true;

    public readonly int warmUpBase = 300;

    public readonly int warmUpQualityFactor = -33;
    public readonly float weightBase = 150f;

    public readonly float weightQualityFactor = 50f;

    public CompProperties_LTF_TpSpot()
    {
        compClass = typeof(Comp_LTF_TpSpot);
    }
}