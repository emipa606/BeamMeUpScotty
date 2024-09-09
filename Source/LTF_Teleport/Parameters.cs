using Verse;

namespace LTF_Teleport;

public class Parameters : IExposable
{
    public bool automaticTeleportation;

    public MyWay.Way myWay = MyWay.Way.No;

    public void ExposeData()
    {
        Scribe_Values.Look(ref automaticTeleportation, "Parameters_automaticTeleportation");
        Scribe_Values.Look(ref myWay, "Parameters_myWay");
    }

    public void AutoToggle()
    {
        automaticTeleportation = !automaticTeleportation;
    }
}