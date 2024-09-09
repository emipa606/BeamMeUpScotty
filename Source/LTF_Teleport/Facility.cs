using RimWorld;
using Verse;

namespace LTF_Teleport;

public static class Facility
{
    public static bool HasPoweredFacility(Building facility, CompPowerTrader compPowerFacility)
    {
        return TickCheckFacilityPower(facility, compPowerFacility);
    }

    public static bool TickCheckFacilityPower(Building facility, CompPowerTrader powerComp = null, bool debug = false)
    {
        var facilityPower = true;
        if (debug)
        {
            Log.Warning("tick check facility");
        }

        facilityPower &= ToolsBuilding.CheckBuilding(facility);
        facilityPower &= powerComp == null ? ToolsBuilding.CheckPower(facility) : ToolsBuilding.CheckPower(powerComp);
        if (debug)
        {
            Log.Warning("tick check facility");
        }

        Tools.WarnRare("no powered facility", 300, debug);
        return facilityPower;
    }

    public static bool CheckBuildingBelongsFacility(CompAffectedByFacilities buildingFacilityComp, Building facility,
        bool debug = false)
    {
        if (!ToolsBuilding.CheckBuilding(facility))
        {
            if (debug)
            {
                Log.Warning("null facility");
            }

            return false;
        }

        if (buildingFacilityComp == null)
        {
            if (debug)
            {
                Log.Warning("null facility comp");
            }

            return false;
        }

        if (buildingFacilityComp.LinkedFacilitiesListForReading.NullOrEmpty())
        {
            if (debug)
            {
                Log.Warning("no linked facility found");
            }

            return false;
        }

        if (debug)
        {
            Log.Warning($"Found: {buildingFacilityComp.LinkedFacilitiesListForReading.Count} facilities");
        }

        var thing = buildingFacilityComp.LinkedFacilitiesListForReading.RandomElement();
        if (thing == null)
        {
            if (debug)
            {
                Log.Warning("no facility found; ok on load");
            }

            return false;
        }

        var building = thing as Building;
        if (building == facility)
        {
            return true;
        }

        if (debug)
        {
            Log.Warning($"Found {building?.ThingID}, but it's not {facility.ThingID}");
        }

        return false;
    }
}