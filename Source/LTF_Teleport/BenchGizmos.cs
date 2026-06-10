using System;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

public static class BenchGizmos
{
    extension(Comp_LTF_TpBench bComp)
    {
        public Gizmo ShowQuality()
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

        public Gizmo ShowRegistryReport()
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

        public Gizmo ManualCast()
        {
            var compLtfTpSpot = bComp.CurrentSpot?.TryGetComp<Comp_LTF_TpSpot>();
            if (compLtfTpSpot == null)
            {
                Log.Warning("Comp_LTF_TpSpot null in ChangeWay - should not be this way");
                return null;
            }

            if (compLtfTpSpot.WParams.myWay.InvalidWay())
            {
                Log.Warning("invalid way in ChangeWay - should not be this way");
                return null;
            }

            var text = compLtfTpSpot.WParams.myWay.WayNaming();
            Texture2D icon;
            var hasNoIssue = compLtfTpSpot.HasNoIssue();
            var isOrphan = compLtfTpSpot.IsOrphan() || compLtfTpSpot.TwinComp.HasNoIssue();
            string defaultDesc;
            string description;
            Action @object = delegate { Tools.Warn("rip action on no way", Prefs.DevMode); };
            switch (hasNoIssue)
            {
                case true when isOrphan:
                {
                    icon = compLtfTpSpot.WParams.myWay.WayGizmoing();
                    description = $"Cast {text}";
                    defaultDesc = compLtfTpSpot.WParams.myWay.WayDescription(compLtfTpSpot.tpSpot.AutoLabeling(),
                        bComp.buildingName, compLtfTpSpot.IsLinked());
                    var noItems = false;
                    if (compLtfTpSpot.WParams.myWay.IsOut())
                    {
                        if (!compLtfTpSpot.HasItems())
                        {
                            noItems = true;
                            defaultDesc = "BmuS.CloseSpot".Translate();
                            icon = MyGizmo.IssueEmptyGz;
                        }
                        else
                        {
                            @object = compLtfTpSpot.OrderOut;
                        }
                    }
                    else if (compLtfTpSpot.WParams.myWay.IsIn())
                    {
                        if (!compLtfTpSpot.TwinComp.HasItems())
                        {
                            noItems = true;
                            defaultDesc = "BmuS.AwaySpot".Translate();
                            icon = MyGizmo.IssueEmptyTwinGz;
                        }
                        else
                        {
                            @object = compLtfTpSpot.OrderIn;
                        }
                    }
                    else if (compLtfTpSpot.WParams.myWay.IsSwap())
                    {
                        if (!compLtfTpSpot.HasItems() || !compLtfTpSpot.TwinComp.HasItems())
                        {
                            noItems = true;
                            defaultDesc = "BmuS.OneOfTwo".Translate();
                            icon = MyGizmo.IssueEmptyGz;
                        }
                        else
                        {
                            @object = compLtfTpSpot.OrderSwap;
                        }
                    }

                    if (noItems)
                    {
                        description = "BmuS.NothingToTeleport".Translate();
                    }

                    break;
                }
                case false:
                    icon = compLtfTpSpot.IssueGizmoing(out description);
                    defaultDesc = compLtfTpSpot.StatusLog;
                    break;
                default:
                    icon = compLtfTpSpot.TwinComp.IssueGizmoing(out description, true);
                    defaultDesc = compLtfTpSpot.TwinComp.StatusLog;
                    break;
            }

            return new Command_Action
            {
                icon = icon,
                defaultLabel = description,
                defaultDesc = defaultDesc,
                action = @object.Invoke
            };
        }

        public Gizmo BrowseTpSpots()
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
}