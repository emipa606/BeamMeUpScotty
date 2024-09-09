using RimWorld;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

[StaticConstructorOnStartup]
public static class ToolsGizmo
{
    public static Texture2D IssueGizmoing(this Comp_LTF_TpSpot comp, out string description, bool twin = false)
    {
        var text = twin ? "BmuS.LinkedSpot".Translate().RawText : "";
        if (comp.Props.requiresPower && !comp.HasPower())
        {
            description = "BmuS.PowerRequired".Translate(text);
            return twin ? MyGizmo.IssuePowerTwinGz : MyGizmo.IssuePowerGz;
        }

        if (comp.Props.requiresFacility)
        {
            if (comp.HasNoFacility())
            {
                description = "BmuS.FacilityRequired".Translate(text);
                return twin ? MyGizmo.IssueNoFacilityTwinGz : MyGizmo.IssueNoFacilityGz;
            }

            if (!comp.HasPoweredFacility())
            {
                description = "BmuS.FacilityPowerRequired".Translate(text);
                return twin ? MyGizmo.IssueNoPoweredFacilityTwinGz : MyGizmo.IssueNoPoweredFacilityGz;
            }
        }

        if (comp.HasOverweight())
        {
            description = "BmuS.ExcessWeight".Translate(text);
            return twin ? MyGizmo.IssueOverweightTwinGz : MyGizmo.IssueOverweightGz;
        }

        if (comp.StatusWaitingForCooldown())
        {
            description = "BmuS.Cooldown".Translate(text);
            return twin ? MyGizmo.IssueCooldownTwinGz : MyGizmo.IssueCooldownGz;
        }

        if (comp.IsOrphan())
        {
            description = "BmuS.OrphanSpotIs".Translate(text);
            return twin ? MyGizmo.IssueOrphanTwinGz : MyGizmo.IssueOrphanGz;
        }

        description = "BmuS.ShouldNotHappen".Translate(text);
        return null;
    }

    public static float Quality2Size(CompQuality qualityComp)
    {
        var result = 0f;
        switch ((int)qualityComp.Quality)
        {
            case 0:
            case 6:
                result = 1f;
                break;
            case 3:
            case 5:
                result = 0.9f;
                break;
            case 1:
            case 2:
            case 4:
                result = 0.75f;
                break;
        }

        return result;
    }

    public static Texture2D Quality2Mat(CompQuality qualityComp)
    {
        Texture2D result = null;
        switch ((int)qualityComp.Quality)
        {
            case 0:
            case 1:
                result = MyGizmo.QualityBadGz;
                break;
            case 2:
            case 3:
                result = MyGizmo.QualityNormalGz;
                break;
            case 4:
            case 5:
            case 6:
                result = MyGizmo.QualityGoodGz;
                break;
        }

        return result;
    }
}