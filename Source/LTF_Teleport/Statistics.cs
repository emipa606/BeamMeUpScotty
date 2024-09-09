using RimWorld;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

public class Statistics
{
    public float benchSynergy = 1f;

    public float cooldownBase;
    public float range;

    public int warmUpBase;

    public float weightCapacity;

    public void SetWeightedProperties(Comp_LTF_TpSpot compSpot, CompQuality compQ, bool debug = false)
    {
        if (compQ == null || compSpot == null)
        {
            if (debug)
            {
                Log.Warning("no quality or tpSpot Comp provided");
            }

            return;
        }

        SetCooldownBase(compSpot, compQ);
        SetWarmUpBase(compSpot, compQ);
        SetWeight(compSpot, debug);
        SetRange(compSpot, compQ);
        if (debug)
        {
            Log.Warning($"{compSpot.tpSpot.DumpProps(compSpot.building)} / {compSpot.tpSpot.DumpSettings}");
        }
    }

    private float StuffMultiplier(Thing thing)
    {
        if (thing == null)
        {
            return 1f;
        }

        var stuff = thing.Stuff;
        var stuffPower_Armor_Sharp = StatDefOf.StuffPower_Armor_Sharp;
        var stuffPower_Armor_Blunt = StatDefOf.StuffPower_Armor_Blunt;
        var bluntDamageMultiplier = StatDefOf.BluntDamageMultiplier;
        var sharpDamageMultiplier = StatDefOf.SharpDamageMultiplier;
        var mass = StatDefOf.Mass;
        var statValueAbstract = stuff.GetStatValueAbstract(stuffPower_Armor_Sharp);
        var statValueAbstract2 = stuff.GetStatValueAbstract(stuffPower_Armor_Blunt);
        var statValueAbstract3 = stuff.GetStatValueAbstract(sharpDamageMultiplier);
        var statValueAbstract4 = stuff.GetStatValueAbstract(bluntDamageMultiplier);
        var statValueAbstract5 = stuff.GetStatValueAbstract(mass);
        return (0.5f + statValueAbstract5) *
               (statValueAbstract + statValueAbstract2 + statValueAbstract3 + statValueAbstract4);
    }

    public void SetRange(Comp_LTF_TpSpot compSpot, CompQuality compQ)
    {
        range = ToolsQuality.FactorCapacity(compSpot.Props.rangeBase, compSpot.Props.rangeQualityFactor, compQ, false,
            true, false, compSpot.prcDebug);
        range += 3f * compSpot.TwinWorstFacilityRange();
        range *= benchSynergy;
        var num = StuffMultiplier(compSpot.parent);
        range *= num;
    }

    public void SetWarmUpBase(Comp_LTF_TpSpot compSpot, CompQuality compQ)
    {
        warmUpBase = (int)ToolsQuality.FactorCapacity(compSpot.Props.warmUpBase, compSpot.Props.warmUpQualityFactor,
            compQ, false, false, false, compSpot.prcDebug);
    }

    public void SetCooldownBase(Comp_LTF_TpSpot compSpot, CompQuality compQ)
    {
        cooldownBase = ToolsQuality.FactorCapacity(compSpot.Props.cooldownBase, compSpot.Props.cooldownQualityFactor,
            compQ, false, false, false, compSpot.prcDebug);
        if (compSpot.Props.requiresFacility && benchSynergy != 0f)
        {
            cooldownBase /= benchSynergy;
        }

        compSpot.Values.currentCooldown = Mathf.Min(cooldownBase, compSpot.CurrentCooldown);
        if (compSpot.Values.currentCooldown < 1f)
        {
            compSpot.Values.currentCooldown = 1f;
        }
    }

    public void SetBenchSynergy(Comp_LTF_TpSpot compSpot, bool debug = false)
    {
        if (compSpot is { HasQuality: true })
        {
            benchSynergy = ToolsQuality.FactorCapacity(compSpot.Props.benchSynergyBase,
                compSpot.Props.benchSynergyQualityFactor, compSpot.QualityComp, false, false, false, debug);
        }
    }

    public void SetWeight(Comp_LTF_TpSpot compSpot, bool debug = false)
    {
        if (compSpot is { HasQuality: true })
        {
            weightCapacity = ToolsQuality.FactorCapacity(compSpot.Props.weightBase, compSpot.Props.weightQualityFactor,
                compSpot.QualityComp, false, false, false, debug);
        }
    }
}