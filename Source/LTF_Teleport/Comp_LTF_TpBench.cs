using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

[StaticConstructorOnStartup]
public class Comp_LTF_TpBench : ThingComp
{
    public readonly bool prcDebug = false;
    public int FacilityCapacity;

    public bool gfxDebug = false;

    public int GizmoIndex;

    public bool Hax = false;

    public float moreRange;

    public float range;

    public List<Building> Registry = [];
    public string TpSpotName = string.Empty;

    public Building building => (Building)parent;

    private Vector3 buildingPos => building.DrawPos;

    public string buildingName => building?.LabelShort;

    private CompPowerTrader compPower => building?.TryGetComp<CompPowerTrader>();

    public CompQuality compQuality => building?.TryGetComp<CompQuality>();

    private CompFacility compFacility => building?.TryGetComp<CompFacility>();

    public CompProperties_LTF_TpBench Props => (CompProperties_LTF_TpBench)props;

    public bool MoreThanOne => HasSpot && Registry.Count > 1;

    public bool IsFull => HasSpot && Registry.Count >= FacilityCapacity;

    public bool IsEmpty => Registry.NullOrEmpty();

    public bool HasSpot => !IsEmpty;

    public Building CurrentSpot => GizmoIndex >= Registry.Count ? null : Registry[GizmoIndex];

    public bool GotThePower => compPower is { PowerOn: true };

    public bool HasQuality => compQuality != null;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        TpSpotName = Tools.LabelByDefName("LTF_TpSpot", prcDebug);
        range = compFacility?.Props.maxDistance ?? 0f;
        SetMoreRange();
        WeightFacilityCapacity(compQuality);
    }

    private void SetMoreRange(CompQuality comp = null)
    {
        if (comp != null || (comp = compQuality) != null)
        {
            moreRange = ToolsQuality.FactorCapacity(Props.moreRangeBase, Props.moreRange, comp, false, false, false,
                prcDebug);
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref Registry, "tpSpots", LookMode.Reference);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && Registry == null)
        {
            Registry = [];
        }

        Scribe_Values.Look(ref GizmoIndex, "index");
    }

    public void RemoveSpot(Building target)
    {
        if ((target == null || target.def.defName != "LTF_TpSpot") && prcDebug)
        {
            Log.Warning("Trying to remove a non tp spot");
        }

        Registry.Remove(target);
        IndexCorrecter();
    }

    public void IndexCorrecter()
    {
        GizmoIndex = Tools.LimitToRange(GizmoIndex, 0, Registry.Count - 1);
    }

    public void AddSpot(Building target)
    {
        if ((target == null || target.def.defName != "LTF_TpSpot") && prcDebug)
        {
            Log.Warning("Trying to register a non tp spot");
        }

        if (!Registry.Contains(target))
        {
            Registry.Add(target);
        }
    }

    private void WeightFacilityCapacity(CompQuality comp, bool debug = false)
    {
        if (debug)
        {
            Log.Warning(
                $">Settin Quality>{Props.FacilityCapacityBase};{Props.FacilityCapacitySpectrum}>FacilityCapacity>{FacilityCapacity}");
        }

        FacilityCapacity =
            (int)ToolsQuality.WeightedCapacity(Props.FacilityCapacityBase, Props.FacilityCapacitySpectrum, comp);
    }

    public void NextIndex()
    {
        GizmoIndex = Tools.NextIndexRoundBrowser(GizmoIndex, Registry.Count);
    }

    public void ShowReport()
    {
        var stringBuilder = new StringBuilder();
        var orphanSpot = false;
        var num = 0;
        stringBuilder.AppendLine("BmuS.RegistryTitle".Translate());
        stringBuilder.AppendLine("+-------------------------+");
        if (!Registry.NullOrEmpty())
        {
            stringBuilder.AppendLine($">>> {"BmuS.Records".Translate(Registry.Count.ToString("D2"))}\n");
        }
        else
        {
            stringBuilder.AppendLine("BmuS.Empty".Translate());
            num++;
        }

        if (!Registry.NullOrEmpty())
        {
            var num2 = 1;
            foreach (var item in Registry)
            {
                var empty2 = $"{num2:D2}. {item.LabelShort}{Tools.PosStr(item.Position)}";
                var comp_LTF_TpSpot = item.TryGetComp<Comp_LTF_TpSpot>();
                if (comp_LTF_TpSpot != null && comp_LTF_TpSpot.IsLinked())
                {
                    empty2 = $"{empty2} {comp_LTF_TpSpot.WParams.myWay.WayArrowLabeling()} ";
                    empty2 = empty2 + comp_LTF_TpSpot.Twin.Label + Tools.PosStr(comp_LTF_TpSpot.Twin.Position);
                }
                else
                {
                    empty2 += " " + "BmuS.OrphanSpot".Translate();
                    orphanSpot = true;
                    num++;
                }

                stringBuilder.AppendLine(empty2);
                num2++;
            }
        }

        if (num > 0)
        {
            stringBuilder.AppendLine(num > 1 ? $"\n{"BmuS.Tips".Translate()}:" : $"\n{"BmuS.Tip".Translate()}:");

            if (IsEmpty)
            {
                stringBuilder.AppendLine("BmuS.BuildaInRange".Translate(MyDefs.tpSpotDef.label, buildingName));
            }

            if (orphanSpot)
            {
                stringBuilder.AppendLine("BmuS.SelectColonist".Translate(MyDefs.tpSpotDef.label,
                    MyDefs.tpCatcherDef.label));
            }
        }

        var window = new Dialog_MessageBox(stringBuilder.ToString());
        Find.WindowStack.Add(window);
    }

    private void ChangeQuality(bool better = true)
    {
        if (!ToolsQuality.ChangeQuality(building, compQuality, better))
        {
            return;
        }

        WeightFacilityCapacity(compQuality);
    }

    private void BetterQuality()
    {
        ChangeQuality();
    }

    private void WorseQuality()
    {
        ChangeQuality(false);
    }

    public string QualityLog()
    {
        return "BmuS.QualityLog".Translate(FacilityCapacity, range, moreRange);
    }

    private float Bench2SpotDistance(Building spot)
    {
        var result = 999f;
        return !ToolsBuilding.CheckBuilding(spot) ? result : building.Position.DistanceTo(spot.Position);
    }

    private bool InRangeSpot(Building spot)
    {
        return Bench2SpotDistance(spot) < range;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!HasSpot)
        {
            return;
        }

        if (Tools.TwoTicksOneTrue())
        {
            for (var num = Registry.Count - 1; num >= 0; num--)
            {
                var target = Registry[num];
                if (ToolsBuilding.CheckBuilding(target) && ToolsBuilding.CheckPower(target) &&
                    InRangeSpot(target))
                {
                    continue;
                }

                var comp_LTF_TpSpot = target?.TryGetComp<Comp_LTF_TpSpot>();
                comp_LTF_TpSpot?.tpSpot.ResetFacility(comp_LTF_TpSpot);
                RemoveSpot(target);
            }
        }

        IndexCorrecter();
    }

    [DebuggerHidden]
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (HasQuality)
        {
            yield return this.ShowQuality();
        }

        if (GotThePower)
        {
            yield return this.ShowRegistryReport();
        }

        if (GotThePower && HasSpot && CurrentSpot != null)
        {
            yield return this.ManualCast();
        }

        if (GotThePower && MoreThanOne)
        {
            yield return this.BrowseTpSpots();
        }
    }

    public override string CompInspectStringExtra()
    {
        if (!GotThePower)
        {
            return null;
        }

        if (Registry.NullOrEmpty())
        {
            return "BmuS.EmptyRegistry".Translate();
        }

        return MoreThanOne
            ? "BmuS.CheckRegistryRecords".Translate(Registry.Count.ToString("D2"))
            : "BmuS.CheckRegistryRecord".Translate(Registry.Count.ToString("D2"));
    }

    public override void PostDrawExtraSelectionOverlays()
    {
        if (prcDebug)
        {
            Log.Warning("Entering PostDrawExtraSelectionOverlays");
        }

        if (!GotThePower)
        {
            return;
        }

        if (range > 0f)
        {
            GenDraw.DrawRadiusRing(radius: !parent.IsMiniStation() ? 5.95f : 3.17f, center: parent.Position);
        }

        if (Registry.NullOrEmpty())
        {
            return;
        }

        var comp_LTF_TpSpot = CurrentSpot?.TryGetComp<Comp_LTF_TpSpot>();
        if (comp_LTF_TpSpot == null)
        {
            return;
        }

        if (prcDebug)
        {
            Log.Warning("PostDrawExtraSelectionOverlays - found comp");
        }

        if (comp_LTF_TpSpot.Props.requiresPower && !comp_LTF_TpSpot.HasPower() ||
            comp_LTF_TpSpot.Props.requiresFacility && !comp_LTF_TpSpot.HasPoweredFacility())
        {
            return;
        }

        if (prcDebug)
        {
            Log.Warning("PostDrawExtraSelectionOverlays - found power and facility");
        }

        GenDraw.DrawLineBetween(parent.TrueCenter(), CurrentSpot.TrueCenter(),
            comp_LTF_TpSpot.WParams.myWay.WayColoring());
        if (prcDebug)
        {
            Log.Warning("PostDrawExtraSelectionOverlays - 1st DrawLineBetween");
        }

        if (!comp_LTF_TpSpot.IsLinked())
        {
            return;
        }

        if (prcDebug)
        {
            Log.Warning("PostDrawExtraSelectionOverlays - 2nd DrawLineBetween");
        }

        GenDraw.DrawLineBetween(CurrentSpot.TrueCenter(), comp_LTF_TpSpot.Twin.TrueCenter(),
            comp_LTF_TpSpot.WParams.myWay.WayColoring());
    }
}