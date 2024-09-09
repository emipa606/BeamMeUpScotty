using System;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

public static class BenchGizmos
{
    public static Gizmo ShowQuality(this Comp_LTF_TpBench bComp)
    {
        var icon = ToolsGizmo.Quality2Mat(bComp.compQuality);
        var iconDrawScale = ToolsGizmo.Quality2Size(bComp.compQuality);
        return new Command_Action
        {
            icon = icon,
            iconDrawScale = iconDrawScale,
            defaultLabel = "BmuS.QualityMatters".Translate(),
            defaultDesc = bComp.QualityLog(),
            action = delegate { Tools.Warn("rip quality button", bComp.prcDebug); }
        };
    }

    public static Gizmo ShowRegistryReport(this Comp_LTF_TpBench bComp)
    {
        var isEmpty = bComp.IsEmpty;
        var isFull = bComp.IsFull;
        var tpSpotName = bComp.TpSpotName;
        var num = !bComp.Registry.NullOrEmpty() ? bComp.Registry.Count : 0;
        var icon = MyGizmo.EmptyStatus2Gizmo(bComp.IsEmpty, bComp.IsFull);
        var defaultLabel = "BmuS.Registry".Translate();
        var text = $"{Tools.CapacityString(num, bComp.FacilityCapacity)} {bComp.TpSpotName}.";
        if (isEmpty || isFull)
        {
            text = $"{text}." + (isEmpty ? "BmuS.RegistryEmpty".Translate() : "BmuS.RegistryFull".Translate());
            if (isEmpty)
            {
                text = $"{text}\n{"BmuS.BuildInRange".Translate(tpSpotName)}";
            }

            if (isFull)
            {
                text =
                    $"{text}\n{"BmuS.NoAdditional".Translate(tpSpotName, bComp.buildingName)}";
            }
        }

        text = num > 1
            ? $"{text}\n{"BmuS.ListTeleporters".Translate(num)}"
            : $"{text}\n{"BmuS.ListTeleporter".Translate(num)}";

        return new Command_Action
        {
            icon = icon,
            defaultLabel = defaultLabel,
            defaultDesc = text,
            action = bComp.ShowReport
        };
    }

    public static Gizmo ManualCast(this Comp_LTF_TpBench bComp)
    {
        var comp_LTF_TpSpot = bComp.CurrentSpot?.TryGetComp<Comp_LTF_TpSpot>();
        if (comp_LTF_TpSpot == null)
        {
            Log.Warning("Comp_LTF_TpSpot null in ChangeWay - should not be this way");
            return null;
        }

        if (comp_LTF_TpSpot.WParams.myWay.InvalidWay())
        {
            Log.Warning("invalid way in ChangeWay - should not be this way");
            return null;
        }

        var text = comp_LTF_TpSpot.WParams.myWay.WayNaming();
        Texture2D icon;
        var hasNoIssue = comp_LTF_TpSpot.HasNoIssue();
        var isOrphan = comp_LTF_TpSpot.IsOrphan() || comp_LTF_TpSpot.TwinComp.HasNoIssue();
        string defaultDesc;
        string description;
        Action @object = delegate { Tools.Warn("rip action on no way", Prefs.DevMode); };
        if (hasNoIssue && isOrphan)
        {
            icon = comp_LTF_TpSpot.WParams.myWay.WayGizmoing();
            description = $"Cast {text}";
            defaultDesc = comp_LTF_TpSpot.WParams.myWay.WayDescription(comp_LTF_TpSpot.tpSpot.AutoLabeling(),
                bComp.buildingName, comp_LTF_TpSpot.IsLinked());
            var noItems = false;
            if (comp_LTF_TpSpot.WParams.myWay.IsOut())
            {
                if (!comp_LTF_TpSpot.HasItems())
                {
                    noItems = true;
                    defaultDesc = "BmuS.CloseSpot".Translate();
                    icon = MyGizmo.IssueEmptyGz;
                }
                else
                {
                    @object = comp_LTF_TpSpot.OrderOut;
                }
            }
            else if (comp_LTF_TpSpot.WParams.myWay.IsIn())
            {
                if (!comp_LTF_TpSpot.TwinComp.HasItems())
                {
                    noItems = true;
                    defaultDesc = "BmuS.AwaySpot".Translate();
                    icon = MyGizmo.IssueEmptyTwinGz;
                }
                else
                {
                    @object = comp_LTF_TpSpot.OrderIn;
                }
            }
            else if (comp_LTF_TpSpot.WParams.myWay.IsSwap())
            {
                if (!comp_LTF_TpSpot.HasItems() || !comp_LTF_TpSpot.TwinComp.HasItems())
                {
                    noItems = true;
                    defaultDesc = "BmuS.OneOfTwo".Translate();
                    icon = MyGizmo.IssueEmptyGz;
                }
                else
                {
                    @object = comp_LTF_TpSpot.OrderSwap;
                }
            }

            if (noItems)
            {
                description = "BmuS.NothingToTeleport".Translate();
            }
        }
        else if (!hasNoIssue)
        {
            icon = comp_LTF_TpSpot.IssueGizmoing(out description);
            defaultDesc = comp_LTF_TpSpot.StatusLog;
        }
        else
        {
            icon = comp_LTF_TpSpot.TwinComp.IssueGizmoing(out description, true);
            defaultDesc = comp_LTF_TpSpot.TwinComp.StatusLog;
        }

        return new Command_Action
        {
            icon = icon,
            defaultLabel = description,
            defaultDesc = defaultDesc,
            action = @object.Invoke
        };
    }

    public static Gizmo BrowseTpSpots(this Comp_LTF_TpBench bComp)
    {
        var nextTpGz = MyGizmo.NextTpGz;
        var defaultLabel = Tools.CapacityString(bComp.GizmoIndex + 1, bComp.Registry.Count) +
                           Tools.PosStr(bComp.CurrentSpot.Position);
        var defaultDesc = "BmuS.BrowseRecords".Translate(bComp.Registry.Count);
        return new Command_Action
        {
            icon = nextTpGz,
            defaultLabel = defaultLabel,
            defaultDesc = defaultDesc,
            action = bComp.NextIndex
        };
    }
}