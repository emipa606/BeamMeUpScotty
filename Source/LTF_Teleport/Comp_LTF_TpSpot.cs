using System;
using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace LTF_Teleport;

[StaticConstructorOnStartup]
public class Comp_LTF_TpSpot : ThingComp
{
    private readonly int FrameSlowerMax = 3;

    private readonly int TicksNotShowingAlertsBase = 120;

    private readonly int TicksTpSequenceActiveBase = 50;

    private readonly int TicksTpSequenceBeginBase = 20;
    private int BeginSequenceFrameLength = 120;

    private int beginSequenceI;
    public Building building;

    public string buildingName = string.Empty;

    private Vector3 buildingPos;

    public bool drawOverlay = true;

    public bool drawUnderlay = true;

    private Thing ellipseMote;

    private int FrameSlower;

    public bool gfxDebug;

    private bool Hax;

    public string myDefName = string.Empty;

    private Map myMap;

    public float myOpacity = 1f;

    public bool prcDebug;

    public bool slideShowOn;

    public Gfx.AnimStep TeleportItemAnimStatus = Gfx.AnimStep.na;

    private int TicksNotShowingAlerts;

    private int TicksTpSequenceActive;

    private int TicksTpSequenceBegin;

    public TpSpot tpSpot;

    public Parameters WParams => tpSpot?.wParams;

    public WorkValues Values => tpSpot?.values;

    public Statistics BaseStats => tpSpot?.baseStats;

    public Parameters TwinParams => tpSpot?.compTwin?.tpSpot?.wParams;

    public WorkValues TwinValues => tpSpot?.compTwin?.tpSpot?.values;

    public Statistics TwinBaseStats => tpSpot?.compTwin?.tpSpot?.baseStats;

    public Map TwinMap => tpSpot?.twin?.Map;

    public Comp_LTF_TpSpot TwinComp => tpSpot?.compTwin;

    public int RegisteredCount => Values.RegisteredCount;

    public float CurrentWeight => Values.currentWeight;

    public float WeightCapacity => BaseStats.weightCapacity;

    public int WarmUpLeft => Values.warmUpLeft;

    public float CurrentCooldown => Values.currentCooldown;

    public float CooldownBase => BaseStats.cooldownBase;

    public float Range => BaseStats.range;

    public Building TpFacility => tpSpot.facility;

    public bool HasQuality => tpSpot.HasQuality;

    public CompQuality QualityComp => tpSpot.compQuality;

    public Building Twin => tpSpot.twin;

    public CompProperties_LTF_TpSpot Props => (CompProperties_LTF_TpSpot)props;

    private bool SpawnedEllipseMote => ellipseMote is { Spawned: true };

    private IntVec3 TwinPosition
    {
        get
        {
            if (tpSpot == null || Twin == null)
            {
                return IntVec3.Invalid;
            }

            return Twin.Position;
        }
    }

    public string MyCoordinates =>
        !ToolsBuilding.CheckBuilding(building) ? string.Empty : Tools.PosStr(building.Position);

    public bool TpSequenceEnd => TeleportItemAnimStatus == Gfx.AnimStep.end;

    public bool TpSequenceBegin => TeleportItemAnimStatus == Gfx.AnimStep.begin;

    public bool TpSequenceActive => TeleportItemAnimStatus == Gfx.AnimStep.active;

    public bool TeleportItemAnimNa => TeleportItemAnimStatus == Gfx.AnimStep.na;

    public string StatusLog
    {
        get
        {
            var text = string.Empty;
            if (this.HasNoIssue())
            {
                text = text + Tools.OkStr(this.TeleportOrder()) +
                       (this.TeleportOrder() ? "BmuS.Roger".Translate() : "BmuS.SayWhat".Translate());
                text += this.HasNothing()
                    ? "BmuS.Nothing".Translate()
                    : RegisteredCount > 1
                        ? "BmuS.ItemsKg".Translate(RegisteredCount, Tools.CapacityString(CurrentWeight, WeightCapacity))
                        : "BmuS.ItemKg".Translate(RegisteredCount, Tools.CapacityString(CurrentWeight, WeightCapacity));
                if (this.TeleportOrder() && this.HasWarmUp())
                {
                    text +=
                        $" {"BmuS.WarmUp".Translate(tpSpot.values.WarmUpString(), (1f - tpSpot.values.WarmUpProgress).ToStringPercent("F0"))}";
                }

                return text;
            }

            if (Props.requiresPower && this.HasNoPower())
            {
                text += "BmuS.NoPower".Translate();
            }

            if (Props.requiresFacility)
            {
                if (this.HasNoFacility())
                {
                    text += "BmuS.NoFacility".Translate();
                }
                else if (!this.HasPoweredFacility())
                {
                    text += "BmuS.NoPoweredFacility".Translate();
                }
            }

            if (this.HasOverweight())
            {
                text = $"{text}{32f + CurrentWeight}kg > {WeightCapacity}kg;";
            }

            if (this.StatusWaitingForCooldown())
            {
                if (tpSpot.values.WaitingForCooldown)
                {
                    text +=
                        $" {"BmuS.CooldownPercent".Translate(tpSpot.CooldownString, tpSpot.CooldownProgress.ToStringPercent("F0"))}";
                }
                else if (this.IsLinked())
                {
                    text +=
                        $" {"BmuS.LinkedCooldownPercent".Translate(TwinComp.tpSpot.CooldownString, TwinComp.tpSpot.CooldownProgress.ToStringPercent("F0"))}";
                }
            }

            if (this.IsOrphan())
            {
                text += "BmuS.Orphan".Translate();
            }

            return text;
        }
    }

    public string DumpProps => tpSpot.DumpProps(building);

    public string DumpSettings => tpSpot.DumpSettings;

    public bool IsOut => WParams.myWay.IsOut();

    public bool IsIn => WParams.myWay.IsIn();

    public bool IsSwap => WParams.myWay.IsSwap();

    public bool HasSetWay => IsIn || IsOut || IsSwap;

    private void SetBeginSequenceFrameLength()
    {
        var num = (int)(34.5f * FrameSlowerMax);
        BeginSequenceFrameLength = tpSpot.values.warmUpCalculated - num;
        BeginSequenceFrameLength = BeginSequenceFrameLength < num ? num : BeginSequenceFrameLength;
    }

    public void ResetOrder()
    {
        tpSpot.teleportOrder = false;
        TeleportItemAnimStatus = Gfx.AnimStep.na;
        if (!this.IsLinked())
        {
            return;
        }

        TwinComp.TeleportItemAnimStatus = Gfx.AnimStep.na;
        TwinComp.tpSpot.teleportOrder = false;
    }

    private void ResetSettings(bool debug = false)
    {
        if (debug)
        {
            Log.Warning("Entering ResetSettings");
        }

        WParams.myWay.ResetWay();
        WParams.automaticTeleportation = false;
        if (debug)
        {
            Log.Warning("ResetSettings 1");
        }

        if (Props.requiresFacility && tpSpot.HasRegisteredFacility && ToolsBuilding.CheckBuilding(building))
        {
            tpSpot.compFacility.RemoveSpot(building);
        }

        if (debug)
        {
            Log.Warning("ResetSettings 2");
        }

        tpSpot.Unlink();
        tpSpot.StopCooldown();
        Values.ResetWeight();
        if (debug)
        {
            Log.Warning("ResetSettings 2");
        }

        if (Props.requiresFacility)
        {
            tpSpot.ResetFacility(this);
        }

        if (debug)
        {
            Log.Warning("ResetSettings 3");
        }

        Values.ResetItems();
        Values.ResetPawn();
        ResetOrder();
        if (debug)
        {
            Log.Warning("Exiting ResetSettings");
        }
    }

    private void WorkstationOrder(bool debug = false)
    {
        if (!this.HasNoIssue())
        {
            if (debug)
            {
                Log.Warning("cant accept an order: hasIssue");
            }

            return;
        }

        if (TwinPosition == IntVec3.Invalid)
        {
            if (debug)
            {
                Log.Warning(" Twinposition IntVec3 Invalid ");
            }

            return;
        }

        Values.orderRange = building.Position.DistanceTo(TwinPosition);
        tpSpot.TwinWorstFacilityQuality(Props.requiresFacility);
        tpSpot.values.SetWarmUpLeft();
        SetBeginSequenceFrameLength();
        tpSpot.teleportOrder = true;
    }

    public void OrderOut()
    {
        if (Values.HasNothing)
        {
            return;
        }

        WParams.myWay = MyWay.Way.Out;
        TwinParams.myWay = MyWay.Way.In;
        WorkstationOrder(prcDebug);
        BeginTeleportItemAnimSeq();
    }

    public void OrderIn()
    {
        TwinComp.OrderOut();
    }

    public void OrderSwap()
    {
        WParams.myWay.SetSwap();
        TwinComp.tpSpot.wParams.myWay.SetSwap();
        WorkstationOrder(prcDebug);
        BeginTeleportItemAnimSeq();
    }

    private void TeleportItem(Thing thing, IntVec3 destination, Map destinationMap, bool debug = false)
    {
        if (thing == null)
        {
            if (debug)
            {
                Log.Warning("!!! thing == null");
            }

            return;
        }

        if (destinationMap == null)
        {
            if (debug)
            {
                Log.Warning("!!! destinationMap == null");
            }

            return;
        }

        if (thing.Position == destination && debug)
        {
            Log.Warning("!!! Trying to tp something where it already is");
        }

        var firstSelected = Find.Selector.FirstSelectedObject == thing;
        if (thing is Pawn pawn)
        {
            if (debug)
            {
                Log.Warning($"Pawn moving :{pawn.ThingID}");
            }

            if (pawn.RaceProps.Animal)
            {
                pawn.DeSpawn();
                GenSpawn.Spawn(pawn, destination, destinationMap);
                if (debug)
                {
                    Log.Warning($"Pawn moved :{pawn.ThingID}");
                }
            }
            else
            {
                if (pawn.IsColonist)
                {
                    var drafted = pawn.Drafted;
                    pawn.drafter.Drafted = false;
                    pawn.DeSpawn();
                    GenSpawn.Spawn(pawn, destination, destinationMap);
                    pawn.drafter.Drafted = drafted;
                }
                else
                {
                    pawn.DeSpawn();
                    GenSpawn.Spawn(pawn, destination, destinationMap);
                }

                if (debug)
                {
                    Log.Warning($"Pawn moved :{pawn.ThingID} draft:{pawn.Drafted}");
                }
            }
        }
        else
        {
            thing.DeSpawn();
            GenSpawn.Spawn(thing, destination, destinationMap);
            if (debug)
            {
                Log.Warning($"thing moved :{thing.LabelShort}");
            }
        }

        if (firstSelected)
        {
            Find.Selector.Select(thing, false);
        }
    }

    private void TeleportItem(Thing thing, IntVec3 destination, bool debug = false)
    {
        if (thing == null)
        {
            if (debug)
            {
                Log.Warning("!!! thing == null");
            }

            return;
        }

        if (thing.Position == destination && debug)
        {
            Log.Warning("!!! Trying to tp something where it already is");
        }

        if (thing is Pawn pawn)
        {
            if (debug)
            {
                Log.Warning($"Pawn moving :{pawn.ThingID}");
            }

            if (pawn.RaceProps.Animal)
            {
                pawn.DeSpawn();
                GenSpawn.Spawn(pawn, destination, myMap);
                if (debug)
                {
                    Log.Warning($"Pawn moved :{pawn.ThingID}");
                }

                return;
            }

            if (pawn.IsColonist)
            {
                var drafted = pawn.Drafted;
                pawn.drafter.Drafted = false;
                pawn.DeSpawn();
                GenSpawn.Spawn(pawn, destination, myMap);
                pawn.drafter.Drafted = drafted;
            }
            else
            {
                pawn.DeSpawn();
                GenSpawn.Spawn(pawn, destination, myMap);
            }

            if (debug)
            {
                Log.Warning($"Pawn moved :{pawn.ThingID} draft:{pawn.Drafted}");
            }
        }
        else
        {
            thing.Position = destination;
            if (debug)
            {
                Log.Warning($"thing moved :{thing.LabelShort}");
            }
        }
    }

    private bool TryTeleport(bool debug = false)
    {
        if (!this.IsReady())
        {
            if (debug)
            {
                Log.Warning("unready - wont tp");
            }

            return false;
        }

        if (!tpSpot.IsLinked)
        {
            if (debug)
            {
                Log.Warning("orphan - wont tp");
            }

            return false;
        }

        if (!tpSpot.teleportOrder)
        {
            if (debug)
            {
                Log.Warning("no tp order - wont tp");
            }

            return false;
        }

        if (WParams.myWay.IsOut() && !Values.HasItems)
        {
            if (debug)
            {
                Log.Warning("WayOut no item - wont tp");
            }

            return false;
        }

        if (debug)
        {
            Log.Warning($"TP MyWay => {WParams.myWay}");
        }

        var twinPosition = TwinPosition;
        var intVec = buildingPos.ToIntVec3();
        if (debug)
        {
            Log.Warning($"myPos : {intVec} ; twinPos : {twinPosition}");
        }

        var thingList = Values.thingList;
        if (tpSpot.wParams.myWay.IsSwap() && tpSpot.compTwin.tpSpot.wParams.myWay.IsSwap())
        {
            foreach (var thing in Values.thingList)
            {
                if (debug)
                {
                    Log.Warning($"looping {thing.Label}");
                }

                TeleportItem(thing, twinPosition, TwinMap, debug);
            }

            foreach (var thing2 in TwinValues.thingList)
            {
                if (debug)
                {
                    Log.Warning($"looping {thing2.Label}");
                }

                TeleportItem(thing2, intVec, myMap, debug);
            }
        }
        else if (WParams.myWay.IsOut())
        {
            foreach (var item in thingList)
            {
                if (debug)
                {
                    Log.Warning($"looping {item.Label}");
                }

                TeleportItem(item, twinPosition, TwinMap, debug);
            }
        }

        return true;
    }

    public static bool AtLeastOneTpSpot(Thing linkable1, Thing linkable2)
    {
        if (linkable1.IsTpSpot() && linkable2.UnpoweredTp())
        {
            return true;
        }

        if (linkable2.IsTpSpot() && linkable1.UnpoweredTp())
        {
            return true;
        }

        return linkable1.IsTpSpot() && linkable2.IsTpSpot();
    }

    public static string ValidTpSpot(Thing thing)
    {
        var result = string.Empty;
        if (!thing.UnpoweredTp() && !thing.IsTpSpot())
        {
            result = "BmuS.NotAValidSpot".Translate(thing.Label, thing.def.label);
        }

        return result;
    }

    private void SetBeginAnimLength()
    {
        beginSequenceI = BeginSequenceFrameLength;
    }

    public void BeginTeleportItemAnimSeq()
    {
        TeleportItemAnimStatus = Gfx.AnimStep.begin;
        SetBeginAnimLength();
        if (!this.IsLinked() || !WParams.myWay.IsSwap() || !TwinValues.HasItems)
        {
            return;
        }

        TwinComp.TeleportItemAnimStatus = Gfx.AnimStep.begin;
        TwinComp.SetBeginAnimLength();
        SetTicksTpSequenceBegin();
    }

    public void SetTicksNotShowingAlerts()
    {
        TicksNotShowingAlerts = TicksNotShowingAlertsBase;
    }

    public void SetTicksTpSequenceActive()
    {
        TicksTpSequenceActive = TicksTpSequenceActiveBase;
    }

    public void SetTicksTpSequenceBegin()
    {
        TicksTpSequenceBegin = TicksTpSequenceBeginBase;
    }

    public bool IncBeginAnim(bool debug = false)
    {
        beginSequenceI--;
        if (beginSequenceI > 0)
        {
            return false;
        }

        if (debug)
        {
            Log.Warning("%%%%%%%%%%%%%%%Anim End");
        }

        return true;
    }

    public void AnimStatus(bool debug = false)
    {
        if (debug)
        {
            Log.Warning($"AnimStatus - {TeleportItemAnimStatus}: {beginSequenceI}/{BeginSequenceFrameLength}");
        }
    }

    public void SetFrameSlower()
    {
        FrameSlower = FrameSlowerMax;
    }

    public int IncFrameSlower()
    {
        FrameSlower--;
        FrameSlower = Mathf.Max(0, FrameSlower);
        return FrameSlower;
    }

    public void SetSlideShowOn(bool debug = false)
    {
        if (TwinComp == null)
        {
            Tools.Warn("//bad twin comp", true);
            return;
        }

        slideShowOn = true;
    }

    public void NextAnim(bool debug = false)
    {
        if (TwinComp == null)
        {
            Tools.Warn($"{buildingName} //NextAnim bad twin comp", true);
            return;
        }

        switch (TeleportItemAnimStatus)
        {
            case Gfx.AnimStep.begin:
                TeleportItemAnimStatus = Gfx.AnimStep.active;
                break;
            case Gfx.AnimStep.active:
                TeleportItemAnimStatus = Gfx.AnimStep.end;
                break;
            case Gfx.AnimStep.end:
                TeleportItemAnimStatus = Gfx.AnimStep.na;
                break;
        }

        SetFrameSlower();
    }

    public override void PostDraw()
    {
        base.PostDraw();
        _ = buildingPos;

        if (Props.requiresPower && this.HasNoPower() || this.IsOrphan())
        {
            if (gfxDebug)
            {
                Log.Warning($"{buildingName} Nothing to draw: {this.TeleportCapable()}");
            }

            return;
        }

        Material material2 = null;
        Material material3 = null;
        Material material4 = null;
        if (Props.requiresPower && drawUnderlay)
        {
            material2 = MyGfx.Status2UnderlayMaterial(this, gfxDebug);
            material3 = MyGfx.UnderlayM;
            if (gfxDebug)
            {
                Log.Warning($"Underlay calculating - 1: {material2 != null}; 2: {material3 != null}");
            }
        }

        if (drawOverlay)
        {
            if (Props.requiresPower)
            {
                if (TicksNotShowingAlerts > 0)
                {
                    TicksNotShowingAlerts--;
                }
                else if (!this.HasNoIssue())
                {
                    material4 = MyGfx.Status2WarningMaterial(this, gfxDebug);
                }
                else if (!TwinComp.HasNoIssue())
                {
                    material4 = MyGfx.Status2WarningMaterial(TwinComp, gfxDebug);
                }
            }

            if (gfxDebug)
            {
                Log.Warning($"Overlay calculating - warning: {material4 != null}; anim: {false}");
            }
        }

        if (material2 != null)
        {
            var opacity = 1f;
            var num = Gfx.PulseFactorOne(parent);
            if (this.IsReady())
            {
                opacity = 0.8f + (0.2f * Rand.Range(0f, 1f));
            }
            else if (this.HasNothing() && this.HasPoweredFacility())
            {
                opacity = 0.6f + (0.3f * Rand.Range(0f, 1f));
            }

            if (this.HasItems())
            {
                opacity = 0.5f + (0.2f * Rand.Range(0f, 1f));
            }

            Gfx.DrawTickRotating(parent, material2, 0f, 0f, 1f, num * 360f, opacity, Gfx.Layer.under);
        }

        if (material3 != null)
        {
            var opacity2 = Gfx.VanillaPulse(parent);
            var num2 = Gfx.RealLinear(parent, 15f + (CurrentWeight * 5f));
            Gfx.DrawTickRotating(parent, material3, 0f, 0f, 1f, num2 * 360f, opacity2, Gfx.Layer.under);
        }

        if (gfxDebug)
        {
            Log.Warning($"Underlay drew - 1: {material2 != null}; 2: {material3 != null}");
        }

        if (!drawOverlay)
        {
            if (gfxDebug)
            {
                Log.Warning("no overlay asked");
            }

            return;
        }

        if (Props.requiresPower && material4 != null)
        {
            Gfx.PulseWarning(building, material4);
        }

        if (gfxDebug)
        {
            Log.Warning($"Overlay drew - warning: {material4 != null}; anim: {false}");
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        building = (Building)parent;
        myDefName = building?.def?.label;
        if (building != null)
        {
            buildingPos = building.DrawPos;
            buildingName = building?.LabelShort;
            myMap = building?.Map;
            if (Props.debug)
            {
                Log.Warning($"PostSpawnSetup building: {buildingName}");
            }

            if (!respawningAfterLoad && tpSpot == null)
            {
                tpSpot = new TpSpot();
            }

            tpSpot.SetQuality(building, Props.debug);
            if (Props.requiresPower)
            {
                tpSpot.SetPower(building, Props.debug);
            }

            if (Props.requiresFacility)
            {
                tpSpot.SetAffectedByFacilities(building, Props.debug);
                if (respawningAfterLoad && tpSpot.facility != null)
                {
                    tpSpot.SetFacility(this);
                }
            }
        }

        BaseStats.SetWeightedProperties(this, tpSpot.compQuality, Props.debug);
        if (WParams.myWay.InvalidWay())
        {
            Tools.Warn("reset bc bad way", Props.debug);
            ResetSettings(Props.debug);
        }
        else
        {
            Tools.Warn($"valid way: {WParams.myWay}", Props.debug);
        }

        if (!Link.ValidLink(tpSpot.link))
        {
            Tools.Warn("reset bc bad link", Props.debug);
            ResetSettings(Props.debug);
        }
        else
        {
            Tools.Warn($"valid link: {tpSpot.link}", Props.debug);
        }

        if (!this.IsOrphan())
        {
            return;
        }

        Tools.Warn("PostSpawnSetup Is Orphan, trying to retrieve compTwin", Props.debug);
        if (respawningAfterLoad && Twin != null)
        {
            tpSpot.compTwin = Twin.TryGetComp<Comp_LTF_TpSpot>();
        }
    }

    public override void CompTick()
    {
        base.CompTick();
        var firstSelectedObject = Find.Selector.FirstSelectedObject == parent;
        if (prcDebug && firstSelectedObject)
        {
            Log.Warning($">>>TICK begin<<< {building.ThingID}");
        }

        if (prcDebug && this.TeleportOrder())
        {
            Log.Warning(
                $"Validated order: warmUp: {tpSpot.values.WarmUpProgress.ToStringPercent("F0")} begin seq: {Tools.CapacityString(beginSequenceI, BeginSequenceFrameLength)}");
        }

        if (!ToolsBuilding.CheckBuilding(building))
        {
            if (prcDebug)
            {
                Log.Warning("comp building not legit - Exiting");
            }

            return;
        }

        var empty = $"{Tools.OkStr(this.IsReady())}[{this.TeleportCapable()}]{buildingName}: ";
        if (Props.requiresPower)
        {
            empty = $"{empty}{"BmuS.Power".Translate(Tools.OkStr(this.HasPower()))}";
            if (this.HasNoPower())
            {
                if (Props.requiresFacility && this.HasPoweredFacility())
                {
                    ResetSettings();
                }

                if (prcDebug)
                {
                    Log.Warning($"{empty}No power - Exiting");
                }

                return;
            }
        }

        if (Props.requiresFacility)
        {
            if (!tpSpot.facilityManaged)
            {
                tpSpot.SetFacility(this, prcDebug);
                if (tpSpot.facilityManaged)
                {
                    tpSpot.compFacility.AddSpot(building);
                }
            }

            empty = $"{empty}{"BmuS.Facility".Translate(Tools.OkStr(tpSpot.HasRegisteredFacility))}";
            if (this.HasNoFacility())
            {
                empty =
                    $"{empty}{"BmuS.Found".Translate()}{Tools.OkStr(tpSpot.HasRegisteredFacility)}{(tpSpot.HasRegisteredFacility ? tpSpot.facility.LabelShort : "BmuS.Nothing".Translate())}; ";
                if (prcDebug && firstSelectedObject)
                {
                    Log.Warning($"{empty}no facility - Exiting");
                }

                tpSpot.ResetFacility(this);
                return;
            }

            empty = $"{empty}{"BmuS.FacilityPower".Translate(Tools.OkStr(this.HasPoweredFacility()))}";
            if (!this.HasPoweredFacility())
            {
                tpSpot.compPowerFacility = tpSpot.facility?.TryGetComp<CompPowerTrader>();
                if (prcDebug && firstSelectedObject)
                {
                    Log.Warning($"{empty}no powered facility - Exiting");
                }

                tpSpot.ResetFacility(this);
                return;
            }

            empty =
                $"{empty}{"BmuS.BelongsTo".Translate()}{TpFacility.Label}:{TpFacility.ThingID}?{Tools.OkStr(Facility.CheckBuildingBelongsFacility(tpSpot.compAffectedByFacilities, TpFacility,
                    prcDebug && firstSelectedObject))}";
            if (!Facility.CheckBuildingBelongsFacility(tpSpot.compAffectedByFacilities, TpFacility,
                    prcDebug && firstSelectedObject))
            {
                if (prcDebug && firstSelectedObject)
                {
                    Log.Warning($"{empty} - Exiting");
                }

                tpSpot.ResetFacility(this);
                return;
            }
        }

        if (this.IsOrphan())
        {
            if (prcDebug && firstSelectedObject)
            {
                Log.Warning("no need to comptick if not linked");
            }

            return;
        }

        if (!Values.CheckItems(building))
        {
            return;
        }

        if (this.StatusWaitingForCooldown())
        {
            empty += "BmuS.Chillin".Translate();
            Values.currentCooldown -= 1f;
            Values.currentCooldown = Values.currentCooldown < 0f ? 0f : Values.currentCooldown;
        }

        if (this.HasOverweight())
        {
            empty += "BmuS.Overweight".Translate();
        }

        if (this.HasNothing())
        {
            empty += "BmuS.NothingToDo".Translate();
        }

        if (this.StatusWaitingForCooldown() || this.HasOverweight() || TwinComp.StatusWaitingForCooldown() ||
            TwinComp.HasOverweight())
        {
            if (prcDebug)
            {
                Log.Warning($"{empty} - TICK exit bc not ready: ");
            }

            return;
        }

        bool b;
        var b1 = b = false;
        if (TpSequenceActive || TwinComp.TpSequenceActive)
        {
            b1 = b = true;
        }
        else if (TpSequenceBegin || TwinComp.TpSequenceBegin)
        {
            b1 = true;
        }

        if (b && TicksTpSequenceActive-- < 0)
        {
            this.ThrowTpSpotSpiralMote();
            SetTicksTpSequenceActive();
        }

        if (b1)
        {
            if (TicksTpSequenceBegin-- < 0)
            {
                ellipseMote = this.ThrowTpSpotEllipseMote();
                SetTicksTpSequenceBegin();
            }
        }
        else if (HasSetWay && !SpawnedEllipseMote)
        {
            ellipseMote = this.ThrowTpSpotEllipseMote();
        }

        if (this.IsLinked() && this.IsReady() && TwinComp.IsReady())
        {
            empty = $"{empty}{"BmuS.ReadyToTp".Translate(RegisteredCount, Values.DumpList())}";
        }

        if (this.IsLinked() && WParams.automaticTeleportation && !this.TeleportOrder() && !TwinComp.TeleportOrder() &&
            this.IsReady() && TwinComp.IsReady())
        {
            Tools.Warn($"{empty} - Starting automatic order", prcDebug && firstSelectedObject);
            switch (WParams.myWay)
            {
                case MyWay.Way.Out:
                    if (this.HasItems())
                    {
                        OrderOut();
                    }

                    break;
                case MyWay.Way.In:
                    if (TwinComp.HasItems())
                    {
                        OrderIn();
                    }

                    break;
                case MyWay.Way.Swap:
                    if (this.HasItems() || TwinComp.HasItems())
                    {
                        OrderSwap();
                    }

                    break;
            }
        }

        Tools.Warn("Got TP order", this.TeleportOrder() && prcDebug);
        if (this.TeleportOrder())
        {
            if (this.HasWarmUp())
            {
                empty = $"{empty}old warmup:{WarmUpLeft}";
                Values.warmUpLeft--;
                empty = $"{empty}new warmup:{WarmUpLeft}";
                empty =
                    $"{empty}{Tools.CapacityString(WarmUpLeft, Values.warmUpCalculated)}:{tpSpot.values.WarmUpProgress}";
                myOpacity = TpSequenceActive ? 0.6f + (0.4f * Rand.Range(0, 1)) : 1f;
                if (beginSequenceI > 0 && IncBeginAnim(prcDebug))
                {
                    NextAnim();
                    SetSlideShowOn();
                }

                if (WarmUpLeft == 0)
                {
                    empty += "Trying to TP";
                    var tryTeleport = false;
                    if (tryTeleport == TryTeleport(prcDebug))
                    {
                        SoundDef.Named("LTF_TpSpotOut").PlayOneShotOnCamera(parent.Map);
                        SoundDef.Named("LTF_TpSpotOut").PlayOneShotOnCamera(TwinMap);
                        if (IsIn)
                        {
                            this.ThrowTpSpotFlashMote();
                            TwinComp.ThrowTpSpotFlashMote(false);
                        }
                        else if (IsOut)
                        {
                            this.ThrowTpSpotFlashMote(false);
                            TwinComp.ThrowTpSpotFlashMote();
                        }
                        else if (IsSwap)
                        {
                            if (Rand.Chance(0.5f))
                            {
                                this.ThrowTpSpotFlashMote();
                                TwinComp.ThrowTpSpotFlashMote(false);
                            }
                            else
                            {
                                this.ThrowTpSpotFlashMote(false);
                                TwinComp.ThrowTpSpotFlashMote();
                            }
                        }

                        TwinComp.ResetOrder();
                        TwinComp.tpSpot.StartCooldown();
                        TwinComp.Values.orderRange = 0f;
                        ResetOrder();
                        tpSpot.StartCooldown();
                        Values.orderRange = 0f;
                        slideShowOn = false;
                        SetTicksNotShowingAlerts();
                        TwinComp.SetTicksNotShowingAlerts();
                    }

                    empty = $"{empty}\n>>>Did teleport: {Tools.OkStr(tryTeleport)}<<<";
                }
            }
            else
            {
                slideShowOn = false;
            }
        }
        else if (prcDebug)
        {
            Log.Warning("no teleporter order");
        }

        AnimStatus(prcDebug);
        empty = $"{empty}\n{StatusLog}";
        if (prcDebug)
        {
            Log.Warning($"{buildingName}=>{empty}");
        }

        if (prcDebug)
        {
            Log.Warning(">>>TICK End<<< ");
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref tpSpot, "tpSpot");
    }

    public override string CompInspectStringExtra()
    {
        var empty = string.Empty;
        return empty + Tools.OkStr(this.HasNoIssue()) + StatusLog;
    }

    public void UnlinkMe()
    {
        tpSpot.Unlink();
    }

    public void Browse()
    {
        tpSpot.Browse(prcDebug);
    }

    public void StartCooldown()
    {
        tpSpot.StartCooldown();
    }

    public void StopCooldown()
    {
        tpSpot.StopCooldown();
    }

    public float TwinWorstFacilityRange()
    {
        return tpSpot.TwinWorstFacilityRange(Props.requiresFacility);
    }

    [DebuggerHidden]
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (HasQuality)
        {
            var qualityMat = ToolsGizmo.Quality2Mat(QualityComp);
            var qualitySize = ToolsGizmo.Quality2Size(QualityComp);
            yield return new Command_Action
            {
                icon = qualityMat,
                iconDrawScale = qualitySize,
                defaultLabel = "BmuS.QualityMatters".Translate(),
                defaultDesc = tpSpot.QualityLog(),
                action = delegate
                {
                    if (prcDebug)
                    {
                        Log.Warning("rip quality button");
                    }
                }
            };
        }

        if (Link.ValidLink(tpSpot.link))
        {
            tpSpot.link.LinkNaming();
            tpSpot.link.NextLinkNaming();
            var myGizmo = this.IsLinked() ? MyGizmo.LinkedGz : MyGizmo.OrphanGz;
            var myLabel = "BmuS.Unlink".Translate();
            var myDesc = "BmuS.UnlinkTT".Translate();
            Action todo = delegate
            {
                if (prcDebug)
                {
                    Log.Warning("nothing link gizmo");
                }
            };
            if (this.IsLinked())
            {
                myLabel = "BmuS.Unlink".Translate();
                myDesc = "BmuS.UnlinkTTExpanded".Translate(Twin.def.label, Tools.PosStr(building.Position),
                    WParams.myWay.WayArrowLabeling(), Tools.PosStr(Twin.Position));
                todo = UnlinkMe;
            }

            yield return new Command_Action
            {
                icon = myGizmo,
                defaultLabel = myLabel,
                defaultDesc = myDesc,
                action = todo
            };
        }

        if (Props.requiresPower && this.HasPower() && this.IsLinked())
        {
            if (WParams.myWay.ValidWay())
            {
                var WayName = WParams.myWay.WayNaming();
                WParams.myWay.NextWayNaming(this.IsOrphan());
                var myGizmo2 = WParams.myWay.WayGizmoing();
                var myLabel2 = "BmuS.Browse".Translate();
                var myDesc2 = "BmuS.Current".Translate(WayName);
                myDesc2 = $"{myDesc2}\n{"BmuS.ClickToChange".Translate(WParams.myWay.NextWayNaming(this.IsOrphan()))}";
                yield return new Command_Action
                {
                    icon = myGizmo2,
                    defaultLabel = myLabel2,
                    defaultDesc = myDesc2,
                    action = Browse
                };
            }
            else
            {
                Log.Warning($"CompGetGizmosExtra - Invalid way:{(int)WParams.myWay}");
            }

            if (!WParams.automaticTeleportation || WParams.automaticTeleportation)
            {
                var myGizmo3 = WParams.automaticTeleportation ? MyGizmo.AutoOnGz : MyGizmo.AutoOffGz;
                var myLabel3 = "BmuS.SetAutomatic".Translate();
                var myDesc4 = "BmuS.Current".Translate(AutomaticTp.AutoName(WParams.automaticTeleportation));
                myDesc4 =
                    $"{myDesc4}\n{"BmuS.ClickToChange".Translate(AutomaticTp.ComplementaryAutoName(WParams.automaticTeleportation))}";
                yield return new Command_Action
                {
                    icon = myGizmo3,
                    defaultLabel = myLabel3,
                    defaultDesc = myDesc4,
                    action = WParams.AutoToggle
                };
            }
        }

        if (!Prefs.DevMode)
        {
            yield break;
        }

        if (this.HasItems())
        {
            yield return new Command_Action
            {
                defaultLabel = "list items",
                defaultDesc = "items",
                action = delegate { Log.Warning(tpSpot.values.DumpList()); }
            };
        }

        var debugGfxIcon = gfxDebug ? MyGizmo.DebugOnGz : MyGizmo.DebugOffGz;
        var debugPrcIcon = prcDebug ? MyGizmo.DebugOnGz : MyGizmo.DebugOffGz;
        yield return new Command_Action
        {
            icon = debugPrcIcon,
            defaultLabel = $"prc: {Tools.DebugStatus(prcDebug)}",
            defaultDesc = $"process debug:{prcDebug}\n{DumpProps}{DumpSettings}",
            action = delegate { prcDebug = Tools.WarnBoolToggle(prcDebug, $"debug {building.Label}"); }
        };
        yield return new Command_Action
        {
            icon = debugGfxIcon,
            defaultLabel = $"gfx: {Tools.DebugStatus(gfxDebug)}",
            defaultDesc = $"gfx debug:{gfxDebug}",
            action = delegate { gfxDebug = Tools.WarnBoolToggle(gfxDebug, $"debug {building.Label}"); }
        };
        if (gfxDebug)
        {
            yield return new Command_Action
            {
                defaultLabel = $"under {drawUnderlay}->{!drawUnderlay}",
                action = delegate { drawUnderlay = !drawUnderlay; }
            };
            yield return new Command_Action
            {
                defaultLabel = $"over {drawOverlay}->{!drawOverlay}",
                action = delegate { drawOverlay = !drawOverlay; }
            };
        }

        if (prcDebug)
        {
            yield return new Command_Action
            {
                icon = MyGizmo.DebugLogGz,
                defaultLabel = $"hax {Tools.DebugStatus(Hax)}",
                defaultDesc = "$5,000 for you advert here.",
                action = delegate { Hax = Tools.WarnBoolToggle(Hax, $"hax {building.Label}"); }
            };
        }

        if (!prcDebug || !Hax)
        {
            yield break;
        }

        yield return new Command_Action
        {
            icon = null,
            defaultLabel = "raz",
            defaultDesc = "reset();",
            action = delegate { ResetSettings(); }
        };
        yield return new Command_Action
        {
            defaultLabel = $"tpOut {TeleportItemAnimStatus}->{TpSequenceBegin}",
            action = BeginTeleportItemAnimSeq
        };
        if (CurrentCooldown == 0f)
        {
            yield return new Command_Action
            {
                icon = MyGizmo.HaxEmptyGz,
                defaultLabel = $"{CurrentCooldown}->{CooldownBase}",
                defaultDesc = "force cooldown",
                action = StartCooldown
            };
        }
        else
        {
            yield return new Command_Action
            {
                icon = MyGizmo.HaxFullGz,
                defaultLabel = $"{CurrentCooldown}->0",
                defaultDesc = "reset cooldown",
                action = StopCooldown
            };
        }

        var minus10perc = (int)Mathf.Max(0f, CurrentCooldown - (CooldownBase / 10f));
        var plus10perc = (int)Mathf.Min(CooldownBase, CurrentCooldown + (CooldownBase / 10f));
        yield return new Command_Action
        {
            icon = MyGizmo.HaxSubGz,
            defaultLabel = $"{CurrentCooldown}->{plus10perc}",
            defaultDesc = "-10%",
            action = delegate { tpSpot.HaxCooldown(plus10perc); }
        };
        yield return new Command_Action
        {
            icon = MyGizmo.HaxAddGz,
            defaultLabel = $"{CurrentCooldown}->{minus10perc}",
            defaultDesc = "+10%",
            action = delegate { tpSpot.HaxCooldown(minus10perc); }
        };
    }

    public override void PostDrawExtraSelectionOverlays()
    {
        if (this.IsLinked() && myMap == TwinMap)
        {
            GenDraw.DrawLineBetween(parent.TrueCenter(), Twin.TrueCenter(), WParams.myWay.WayColoring());
        }

        if (Range > 0f && Range < GenRadial.MaxRadialPatternRadius)
        {
            GenDraw.DrawRadiusRing(parent.Position, Range);
        }
    }
}