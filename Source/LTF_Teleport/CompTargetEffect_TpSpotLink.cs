using RimWorld;
using Verse;

namespace LTF_Teleport;

public class CompTargetEffect_TpSpotLink : CompTargetEffect
{
    public override void DoEffectOn(Pawn user, Thing target)
    {
        var building = user.CurJob.targetA.Thing as Building;
        var building2 = (Building)target;
        if (!ToolsBuilding.CheckBuilding(building) || !ToolsBuilding.CheckBuilding(building2))
        {
            return;
        }

        var text = Comp_LTF_TpSpot.ValidTpSpot(building);
        if (!text.NullOrEmpty())
        {
            Messages.Message(text, parent, MessageTypeDefOf.TaskCompletion);
            return;
        }

        var text2 = Comp_LTF_TpSpot.ValidTpSpot(building2);
        if (!text2.NullOrEmpty())
        {
            Messages.Message(text2, parent, MessageTypeDefOf.TaskCompletion);
            return;
        }

        if (!Comp_LTF_TpSpot.AtLeastOneTpSpot(building, building2))
        {
            Messages.Message("BmuS.OnePowered".Translate(), parent, MessageTypeDefOf.TaskCompletion);
            return;
        }

        if (building == building2)
        {
            Messages.Message("BmuS.CannotTargetItself".Translate(building?.Label), parent,
                MessageTypeDefOf.TaskCompletion);
            return;
        }

        var comp_LTF_TpSpot = building.TryGetComp<Comp_LTF_TpSpot>();
        var comp_LTF_TpSpot2 = building2.TryGetComp<Comp_LTF_TpSpot>();
        if (comp_LTF_TpSpot == null || comp_LTF_TpSpot2 == null)
        {
            return;
        }

        if (comp_LTF_TpSpot.prcDebug)
        {
            Log.Warning($"Trying to register: {building2.Label} in {building?.Label}");
        }

        if (building != null)
        {
            var num = building.Position.DistanceTo(building2.Position);
            var num2 = ToolsBuilding.TheoricBestRange(comp_LTF_TpSpot, comp_LTF_TpSpot2);
            if (comp_LTF_TpSpot.prcDebug)
            {
                Log.Warning($"dist:{num} ; range:{num2}");
            }

            if (num > num2)
            {
                Messages.Message("BmuS.OutOfRange".Translate(building2.Label, num, num2),
                    MessageTypeDefOf.TaskCompletion);
                return;
            }
        }

        var link = comp_LTF_TpSpot.tpSpot.CreateLink(building, comp_LTF_TpSpot, building2, comp_LTF_TpSpot2,
            comp_LTF_TpSpot.prcDebug);
        Messages.Message(
            "BmuS.WasWasntLinked".Translate(Tools.OkStr(link), building?.Label, comp_LTF_TpSpot.MyCoordinates,
                link ? "" : "BmuS.Not".Translate(), building2.Label, comp_LTF_TpSpot2.MyCoordinates),
            parent, MessageTypeDefOf.TaskCompletion);
        if (comp_LTF_TpSpot.prcDebug)
        {
            Log.Warning($"{Tools.OkStr(link)}registered: {building2.Label} in {building?.Label}");
        }
    }
}