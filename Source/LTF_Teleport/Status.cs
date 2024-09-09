using System;
using Verse;

namespace LTF_Teleport;

public static class Status
{
    [Flags]
    public enum BuildingStatus
    {
        na = 0,
        noPower = 1,
        noFacility = 2,
        noItem = 4,
        overweight = 8,
        cooldown = 0x10,
        noPowerNoFacility = 3,
        noPowerNoItem = 5,
        noPowerOverweight = 9,
        noPowerCooldown = 0x11,
        noFacilityNoitem = 6,
        noFacilityoverweight = 0xA,
        noFacilityCooldown = 0x12,
        noItemOverweight = 0xC,
        noItemCooldown = 0x14,
        Overweight = 0x18,
        noPowernoFacilityNoItem = 7,
        noPowerNoFacilityOverweight = 0xB,
        noPowernoFacilityCooldown = 0x13,
        noFacilityNoitemOverweight = 0xE,
        noFacilityNoitemCooldown = 0x16,
        noItemOverweightCooldown = 0x1C,
        powerOk = 0x1E,
        facilityOk = 0x1D,
        itemOk = 0x1B,
        weightOk = 0x17,
        cooldownOk = 0xF,
        allWrong = 0x1F,
        capable = 0x40
    }

    public static BuildingStatus TeleportCapable(this Comp_LTF_TpSpot compSpot)
    {
        var buildingStatus = BuildingStatus.na;
        if (compSpot.tpSpot == null)
        {
            return buildingStatus;
        }

        if (compSpot.Props.requiresPower && !compSpot.tpSpot.HasPower)
        {
            buildingStatus ^= BuildingStatus.noPower;
        }

        if (compSpot.Props.requiresFacility && !compSpot.tpSpot.HasRegisteredFacility)
        {
            buildingStatus ^= BuildingStatus.noFacility;
        }

        if (compSpot.CurrentWeight > compSpot.WeightCapacity)
        {
            buildingStatus ^= BuildingStatus.overweight;
        }

        if (compSpot.Values.WaitingForCooldown)
        {
            buildingStatus ^= BuildingStatus.cooldown;
        }

        switch (compSpot.WParams.myWay)
        {
            case MyWay.Way.Out:
                if (compSpot.HasNothing())
                {
                    buildingStatus ^= BuildingStatus.noItem;
                }

                break;
            case MyWay.Way.Swap:
                if (compSpot.HasNothing())
                {
                    buildingStatus ^= BuildingStatus.noItem;
                }

                break;
            default:
                if (compSpot.HasNothing())
                {
                    buildingStatus ^= BuildingStatus.noItem;
                }

                break;
            case MyWay.Way.In:
                break;
        }

        if (buildingStatus == BuildingStatus.na)
        {
            buildingStatus = BuildingStatus.capable;
        }

        return buildingStatus;
    }

    private static bool HasStatus(this Comp_LTF_TpSpot compSpot, BuildingStatus buildingStatus)
    {
        return (compSpot.TeleportCapable() & buildingStatus) != 0;
    }

    public static bool HasPowerTrader(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.tpSpot.HasPowerTrader;
    }

    public static bool HasNoPower(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.HasStatus(BuildingStatus.noPower);
    }

    public static bool HasPower(this Comp_LTF_TpSpot compSpot)
    {
        return !compSpot.HasNoPower();
    }

    public static bool StatusNoFacility(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.HasStatus(BuildingStatus.noFacility);
    }

    private static bool StatusNoItem(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.HasStatus(BuildingStatus.noItem);
    }

    private static bool StatusHasItem(this Comp_LTF_TpSpot compSpot)
    {
        return !compSpot.StatusNoItem();
    }

    public static bool HasOverweight(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.HasStatus(BuildingStatus.overweight);
    }

    public static bool SupportsWeight(this Comp_LTF_TpSpot compSpot)
    {
        return !compSpot.HasOverweight();
    }

    public static bool StatusWaitingForCooldown(this Comp_LTF_TpSpot compSpot)
    {
        if (compSpot.tpSpot == null || compSpot.TwinComp == null)
        {
            return false;
        }

        if (compSpot.HasStatus(BuildingStatus.cooldown))
        {
            return true;
        }

        return compSpot.tpSpot.IsLinked && compSpot.TwinComp.HasStatus(BuildingStatus.cooldown);
    }

    public static bool HasNoCooldown(this Comp_LTF_TpSpot compSpot)
    {
        return !compSpot.StatusWaitingForCooldown();
    }

    public static bool StatusReady(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.HasStatus(BuildingStatus.capable);
    }

    public static bool StatusNoIssue(this Comp_LTF_TpSpot compSpot)
    {
        var noSpot = true;
        if (compSpot.tpSpot == null)
        {
            return false;
        }

        if (compSpot.Props.requiresPower)
        {
            noSpot &= compSpot.HasPower();
        }

        if (compSpot.Props.requiresFacility)
        {
            noSpot &= compSpot.HasPoweredFacility();
        }

        noSpot &= compSpot.IsLinked();
        noSpot &= compSpot.HasNoCooldown();
        return noSpot & compSpot.SupportsWeight();
    }

    public static void StatusNoIssueDebug(this Comp_LTF_TpSpot compSpot)
    {
        Log.Warning(
            $"requiresPower:{compSpot.Props.requiresPower}; hasPower:{compSpot.HasPower()}; requiresFacility{compSpot.Props.requiresFacility}; hasFacility:{compSpot.HasPoweredFacility()}; IsLinked:{compSpot.tpSpot.IsLinked}; HasNoCooldown:{compSpot.HasNoCooldown()}; SupportsWeight:{compSpot.SupportsWeight()}");
    }

    public static bool IsLinked(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.tpSpot is { IsLinked: true };
    }

    public static bool IsOrphan(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.tpSpot is { IsOrphan: true };
    }

    public static bool HasNoFacility(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.StatusNoFacility();
    }

    public static bool HasPoweredFacility(this Comp_LTF_TpSpot compSpot)
    {
        if (compSpot.tpSpot == null || compSpot.tpSpot.facility == null)
        {
            return false;
        }

        return Facility.HasPoweredFacility(compSpot.tpSpot.facility, compSpot.tpSpot.compPowerFacility);
    }

    public static bool HasNothing(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.tpSpot.values.HasNothing;
    }

    public static bool HasItems(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.tpSpot.values.HasItems;
    }

    public static bool HasHumanoid(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.tpSpot.values.HasHumanoid;
    }

    public static bool HasAnimal(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.tpSpot.values.HasAnimal;
    }

    public static bool HasNoIssue(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.StatusNoIssue();
    }

    public static bool IsReady(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.StatusReady();
    }

    public static bool TeleportOrder(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.tpSpot.teleportOrder;
    }

    public static bool HasWarmUp(this Comp_LTF_TpSpot compSpot)
    {
        return compSpot.tpSpot.values.WaitingForWarmUp;
    }
}