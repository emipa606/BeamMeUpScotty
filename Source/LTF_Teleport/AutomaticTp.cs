using Verse;

namespace LTF_Teleport;

public static class AutomaticTp
{
    private static readonly string[] AutoLabel = ["BmuS.ManuallyInfo".Translate(), "BmuS.AutomaticInfo".Translate()];

    public static string AutoName(bool auto)
    {
        return auto ? "BmuS.Automatic".Translate() : "BmuS.Manual".Translate();
    }

    public static string ComplementaryAutoName(bool auto)
    {
        return AutoName(!auto);
    }

    public static string AutoLabeling(this TpSpot tpSpot)
    {
        var num = tpSpot.wParams.automaticTeleportation ? 1 : 0;
        return num > AutoLabel.Length - 1 ? "BmuS.AutoOutbound".Translate() : AutoLabel[num];
    }
}