using RimWorld;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

public class TpSpot : IExposable
{
    public readonly Statistics baseStats = new Statistics();

    public CompAffectedByFacilities compAffectedByFacilities;

    public Comp_LTF_TpBench compFacility;

    public CompPowerTrader compPowerFacility;

    public CompPowerTrader compPowerTrader;
    public CompQuality compQuality;

    public Comp_LTF_TpSpot compTwin;

    public Building facility;

    public bool facilityManaged;

    public Link.LinkOptions link = Link.LinkOptions.Orphan;

    public bool prcDebug = false;

    public bool teleportOrder = false;

    public Building twin;

    private float twinDistance;

    public WorkValues values = new WorkValues();

    public Parameters wParams = new Parameters();

    private Parameters TwinParams => compTwin?.tpSpot?.wParams;

    private WorkValues TwinValues => compTwin?.tpSpot?.values;

    private Statistics TwinBaseStats => compTwin?.tpSpot?.baseStats;

    private Building TwinFacility => compTwin?.tpSpot?.facility;

    private Comp_LTF_TpBench TwinCompFacility => compTwin?.tpSpot?.compFacility;

    public string DumpSettings =>
        $"; myWay: {wParams.myWay}; IsLinked:{IsLinked}; Auto:{wParams.automaticTeleportation}; HasRegisteredFacility:{HasRegisteredFacility}";

    public bool HasQuality => compQuality != null;

    public bool HasPowerTrader => compPowerTrader != null;

    public bool HasPower => HasPowerTrader && compPowerTrader.PowerOn;

    public bool IsLinked => twin != null && compTwin != null;

    public bool IsOrphan => twin == null || compTwin == null;

    public bool HasFacility => compFacility != null;

    public float CooldownProgress
    {
        get
        {
            if (baseStats.cooldownBase == 0f)
            {
                return 0f;
            }

            return 1f - (values.currentCooldown / baseStats.cooldownBase);
        }
    }

    public string CooldownString => Tools.TimeLeftString((int)values.currentCooldown, (int)baseStats.cooldownBase);

    public float TwinBestRange =>
        IsOrphan ? baseStats.range : Mathf.Max(baseStats.range, compTwin.tpSpot.baseStats.range);

    public bool HasRegisteredFacility => facility != null && compFacility != null;

    public void ExposeData()
    {
        Scribe_References.Look(ref facility, "tpSpot_facility");
        Scribe_Values.Look(ref facilityManaged, "tpSpot_facilityManaged");
        Scribe_References.Look(ref twin, "tpSpot_twin");
        Scribe_Values.Look(ref link, "tpSpot_link");
        Scribe_Values.Look(ref twinDistance, "tpSpot_twinDistance");
        Scribe_Deep.Look(ref wParams, "wParams");
        Scribe_Deep.Look(ref values, "values");
    }

    public void SetQuality(Building building, bool debug = false)
    {
        if (building == null)
        {
            if (debug)
            {
                Log.Warning("TpSpot.SetQuality null building");
            }
        }
        else
        {
            compQuality = building.TryGetComp<CompQuality>();
        }
    }

    public void SetPower(Building building, bool debug = false)
    {
        if (building == null)
        {
            if (debug)
            {
                Log.Warning("TpSpot.SetPower null building");
            }
        }
        else
        {
            compPowerTrader = building.TryGetComp<CompPowerTrader>();
        }
    }

    public void SetAffectedByFacilities(Building building, bool debug = false)
    {
        if (building == null)
        {
            if (debug)
            {
                Log.Warning("TpSpot.SetAffectedByFacilities null building");
            }
        }
        else
        {
            compAffectedByFacilities = building.TryGetComp<CompAffectedByFacilities>();
        }
    }

    public string DumpProps(Building building)
    {
        return building != null
            ? $"{building.Label}{Tools.PosStr(building.Position)}; synergy:{baseStats.benchSynergy}; range:{baseStats.range:F2}; CD:{Tools.CapacityString(values.currentCooldown, baseStats.cooldownBase)}; WuBase:{baseStats.warmUpBase}; WuCal:{values.warmUpCalculated}; WuLeft:{values.warmUpLeft}; WuPrg:{Tools.CapacityString(values.warmUpLeft, values.warmUpCalculated)}; weight:{Tools.CapacityString(values.currentWeight, baseStats.weightCapacity)}"
            : string.Empty;
    }

    public bool IsAffectedByFacilities()
    {
        return compAffectedByFacilities != null;
    }

    public string QualityLog()
    {
        var empty = $"Warmup: {Tools.Ticks2Str(values.warmUpCalculated)}";
        empty = $"{empty} - Cooldown: {Tools.Ticks2Str(baseStats.cooldownBase)}";
        empty = $"{empty}\nRange: {(int)baseStats.range}";
        return $"{empty} - Weight max: {baseStats.weightCapacity}kg";
    }

    public void AutoToggle()
    {
        wParams.automaticTeleportation = !wParams.automaticTeleportation;
        if (IsLinked)
        {
            TwinParams.automaticTeleportation = wParams.automaticTeleportation;
        }
    }

    public void Browse(bool myDebug = false)
    {
        if (myDebug)
        {
            Log.Warning(
                $"oldWay: {wParams.myWay.DescriptionAttr()}TwinOldWay:{compTwin.WParams.myWay.DescriptionAttr()}");
        }

        wParams.myWay = wParams.myWay.BrowseWay(compTwin);
        compTwin.WParams.myWay = MyWay.ReflexiveWay[(int)wParams.myWay];
        if (myDebug)
        {
            Log.Warning(
                $"newWay: {wParams.myWay.DescriptionAttr()}TwinNewWay:{compTwin.WParams.myWay.DescriptionAttr()}");
        }
    }

    public bool CreateLink(Building building, Comp_LTF_TpSpot compSpot, Building newLinked, Comp_LTF_TpSpot newComp,
        bool debug = false)
    {
        if (debug)
        {
            Log.Warning("Entering CreateLink");
        }

        if (newComp == null)
        {
            if (debug)
            {
                Log.Warning("CreateLink null comp");
            }

            return false;
        }

        if (building == newLinked)
        {
            if (debug)
            {
                Log.Warning($"CreateLink - Wont register myself - b1:{building.LabelShort} b2:{newLinked}");
            }

            return false;
        }

        if (debug)
        {
            Log.Warning(
                $"Inc Master - order:{Tools.OkStr(newComp.tpSpot.teleportOrder)}; link:{Tools.OkStr(newComp.tpSpot.IsLinked)}Slave - order:{Tools.OkStr(newComp.tpSpot.teleportOrder)}; link:{Tools.OkStr(newComp.tpSpot.IsLinked)}");
        }

        Unlink();
        twin = newLinked;
        compTwin = newComp;
        compTwin.ResetOrder();
        link = Link.LinkOptions.Linked;
        newComp.tpSpot.twin = building;
        newComp.tpSpot.compTwin = compSpot;
        newComp.tpSpot.link = Link.LinkOptions.Linked;
        newComp.TwinComp.ResetOrder();
        UpdateDistance(building);
        CalculateWarmUp();
        if (debug)
        {
            Log.Warning(
                $"Inc Master - order:{Tools.OkStr(teleportOrder)}; link:{Tools.OkStr(IsLinked)}Slave - order:{Tools.OkStr(newComp.tpSpot.teleportOrder)}; link:{Tools.OkStr(newComp.tpSpot.IsLinked)}");
        }

        return true;
    }

    public void Unlink(bool Once = true)
    {
        if (IsLinked && Once)
        {
            compTwin.tpSpot.Unlink(false);
        }

        compTwin = null;
        twin = null;
        link = Link.LinkOptions.Orphan;
        twinDistance = 0f;
        wParams.myWay.ResetWay();
    }

    private void UpdateDistance(Building building, bool debug = false)
    {
        if (compTwin == null)
        {
            if (debug)
            {
                Log.Warning("cant UpdateDistance bc null compTwin");
            }

            return;
        }

        twinDistance = building.Position.DistanceTo(twin.Position);
        compTwin.tpSpot.twinDistance = twinDistance;
        if (debug)
        {
            Log.Warning($"updated distance:{twinDistance}");
        }
    }

    public void StopCooldown()
    {
        values.currentCooldown = 0f;
    }

    public void StartCooldown()
    {
        values.currentCooldown = baseStats.cooldownBase;
    }

    public void HaxCooldown(float value)
    {
        values.currentCooldown = value;
    }

    private void SetCooldown()
    {
        values.currentCooldown = baseStats.cooldownBase * (0.5f + (0.5f *
                                                                   ((0.3f * values.currentWeight /
                                                                     baseStats.weightCapacity) +
                                                                    (0.7f * values.orderRange / TwinBestRange))));
    }

    private void CalculateWarmUp()
    {
        if (IsLinked)
        {
            values.warmUpCalculated = (int)((baseStats.warmUpBase + (float)TwinBaseStats.warmUpBase) / 2f);
            TwinValues.warmUpCalculated = values.warmUpCalculated;
        }
        else
        {
            values.warmUpCalculated = baseStats.warmUpBase;
        }
    }

    public void ResetFacility(Comp_LTF_TpSpot compSpot)
    {
        facility = null;
        compAffectedByFacilities = null;
        compPowerFacility = null;
        compFacility = null;
        FacilityDependantCapacities(compSpot);
        facilityManaged = false;
    }

    public void SetFacility(Comp_LTF_TpSpot compSpot, bool debug = false)
    {
        if (compSpot.building == null)
        {
            if (debug)
            {
                Log.Warning("no building provided");
            }

            return;
        }

        SetAffectedByFacilities(compSpot.building);
        if (compAffectedByFacilities == null)
        {
            if (debug)
            {
                Log.Warning("no comp found");
            }

            return;
        }

        facility = ToolsBuilding.GetFacility(compAffectedByFacilities, debug);
        if (facility == null)
        {
            if (debug)
            {
                Log.Warning("no facility found");
            }

            return;
        }

        compFacility = facility?.TryGetComp<Comp_LTF_TpBench>();
        if (compFacility == null)
        {
            if (debug)
            {
                Log.Warning("no facility comp found");
            }

            return;
        }

        compPowerFacility = facility?.TryGetComp<CompPowerTrader>();
        if (compPowerFacility == null)
        {
            if (debug)
            {
                Log.Warning("no facility power comp found");
            }
        }
        else
        {
            FacilityDependantCapacities(compSpot);
            facilityManaged = true;
        }
    }

    public void FacilityDependantCapacities(Comp_LTF_TpSpot compSpot, bool debug = false)
    {
        baseStats.SetBenchSynergy(compSpot);
        baseStats.SetRange(compSpot, compQuality);
        baseStats.SetCooldownBase(compSpot, compQuality);
    }

    public bool AreYouMyRegisteredFacility(Building daddy)
    {
        return daddy == facility;
    }

    public void TwinWorstFacilityQuality(bool requiresFacility)
    {
        int num;
        if (!requiresFacility)
        {
            num = (int)(IsLinked && compTwin.tpSpot.HasQuality
                ? TwinCompFacility.compQuality.Quality
                : QualityCategory.Awful);
        }
        else if (IsLinked)
        {
            var a = (int)(HasRegisteredFacility && compFacility.HasQuality
                ? compFacility.compQuality.Quality
                : QualityCategory.Awful);
            var b = (int)(compTwin.tpSpot.HasRegisteredFacility && compFacility.HasQuality
                ? TwinCompFacility.compQuality.Quality
                : QualityCategory.Awful);
            num = Mathf.Min(a, b);
        }
        else
        {
            num = (int)(HasRegisteredFacility && compFacility.HasQuality
                ? compFacility.compQuality.Quality
                : QualityCategory.Awful);
        }

        Tools.LimitToRange(num, 0, 8);
    }

    public float TwinWorstFacilityRange(bool requiresFacility)
    {
        if (!requiresFacility)
        {
            if (IsLinked && compTwin.Props.requiresFacility && compTwin.tpSpot.HasRegisteredFacility)
            {
                return TwinCompFacility.moreRange;
            }

            return 0f;
        }

        if (!IsLinked)
        {
            return HasRegisteredFacility ? compFacility.moreRange : 0f;
        }

        if (!HasRegisteredFacility)
        {
            return 0f;
        }

        var moreRange = compFacility.moreRange;
        if (!compTwin.Props.requiresFacility || !compTwin.tpSpot.HasRegisteredFacility)
        {
            return moreRange;
        }

        var b = compTwin.Props.requiresFacility && compTwin.tpSpot.HasRegisteredFacility
            ? TwinCompFacility.moreRange
            : 0f;
        return Mathf.Max(moreRange, b);
    }
}