using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;

namespace LTF_Teleport;

public class CompTargetable_Building : CompTargetable
{
    protected override bool PlayerChoosesTarget => true;

    protected override TargetingParameters GetTargetingParameters()
    {
        return new TargetingParameters
        {
            canTargetPawns = false,
            canTargetBuildings = true,
            validator = x => ValidateTarget(x.Thing)
        };
    }

    [DebuggerHidden]
    public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
    {
        yield return targetChosenByPlayer;
    }
}