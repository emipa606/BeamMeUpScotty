using RimWorld;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

public static class ToolsBuilding
{
    public static bool Negligeable(this Building b)
    {
        int result;
        if (b is { Spawned: true, Map: not null })
        {
            _ = b.Position;
            result = 0;
        }
        else
        {
            result = 1;
        }

        return (byte)result != 0;
    }

    public static bool CheckPower(Building building)
    {
        var compPowerTrader = building?.TryGetComp<CompPowerTrader>();
        return compPowerTrader is { PowerOn: true };
    }

    public static bool CheckPower(CompPowerTrader comp)
    {
        return comp is { PowerOn: true };
    }

    public static bool CheckBuilding(Building building)
    {
        if (building is not { Map: not null })
        {
            return false;
        }

        _ = building.Position;
        return true;
    }

    public static CompAffectedByFacilities GetAffectedComp(Building building, bool debug = false)
    {
        if (!CheckBuilding(building))
        {
            Tools.Warn("//bad building, wont check facility", debug);
            return null;
        }

        var compAffectedByFacilities = building.TryGetComp<CompAffectedByFacilities>();
        if (compAffectedByFacilities != null)
        {
            return compAffectedByFacilities;
        }

        if (debug)
        {
            Log.Warning("//no affected by facility comp found");
        }

        return null;
    }

    public static Building GetFacility(CompAffectedByFacilities buildingFacilityComp, bool debug = false)
    {
        if (buildingFacilityComp == null)
        {
            if (debug)
            {
                Log.Warning("//no comp");
            }

            return null;
        }

        if (buildingFacilityComp.LinkedFacilitiesListForReading.NullOrEmpty())
        {
            if (debug)
            {
                Log.Warning("//no linked facility found");
            }

            return null;
        }

        if (debug)
        {
            Log.Warning($"Found: {buildingFacilityComp.LinkedFacilitiesListForReading.Count} facilities");
        }

        var thing = buildingFacilityComp.LinkedFacilitiesListForReading.RandomElement();
        if (thing != null)
        {
            return thing as Building;
        }

        if (debug)
        {
            Log.Warning("no facility found; ok on load");
        }

        return null;
    }

    public static float TheoricBestRange(Comp_LTF_TpSpot comp1, Comp_LTF_TpSpot comp2)
    {
        return Mathf.Max(comp1?.Range ?? 0f, comp2?.Range ?? 0f);
    }
}