using UnityEngine;
using Verse;

namespace LTF_Teleport;

public static class MyWay
{
    public enum Way
    {
        [Description("No way")] No,
        [Description("Out")] Out,
        [Description("In")] In,
        [Description("Swap")] Swap
    }

    private static readonly string[] WayLabel = ["No way", "Tp out", "Tp in", "Swap"];

    private static readonly string[] WayArrow = ["(x)", " =>", "<= ", "<=>"];

    private static readonly string[] WayActionLabel = ["do nothing", "send away", "bring back", "exchange"];

    public static readonly Way[] ReflexiveWay =
    [
        Way.No,
        Way.In,
        Way.Out,
        Way.Swap
    ];

    public static readonly Way[] NextWay =
    [
        Way.Out,
        Way.In,
        Way.Swap,
        Way.No
    ];

    public static readonly SimpleColor[] WayColor =
    [
        SimpleColor.White,
        SimpleColor.Red,
        SimpleColor.Blue,
        SimpleColor.Magenta
    ];

    public static Way GetNextWay(this Way MyWay, bool IsOrphan)
    {
        var result = Way.No;
        return IsOrphan ? result : NextWay[(int)MyWay];
    }

    public static Way BrowseWay(this Way MyWay, Comp_LTF_TpSpot compTwin)
    {
        return MyWay.GetNextWay(compTwin.IsOrphan());
    }

    public static bool InvalidWay(this Way MyWay)
    {
        return !MyWay.ValidWay();
    }

    public static bool ValidWay(this Way MyWay)
    {
        var num = 0;
        var num2 = 3;
        return (int)MyWay >= num && (int)MyWay <= num2;
    }

    public static string WayNaming(this Way MyWay)
    {
        return (int)MyWay > WayLabel.Length - 1 ? "way outbound" : WayLabel[(int)MyWay];
    }

    public static string NextWayNaming(this Way MyWay, bool IsOrphan)
    {
        return (int)MyWay.GetNextWay(IsOrphan) > WayLabel.Length - 1
            ? "next way outbound"
            : WayLabel[(int)MyWay.GetNextWay(IsOrphan)];
    }

    public static string WayActionLabeling(this Way MyWay)
    {
        return (int)MyWay > WayActionLabel.Length - 1 ? "way action outbound" : WayActionLabel[(int)MyWay];
    }

    public static string WayArrowLabeling(this Way MyWay)
    {
        return (int)MyWay > WayArrow.Length - 1 ? "Arrow outbound" : WayArrow[(int)MyWay];
    }

    public static SimpleColor WayColoring(this Way MyWay)
    {
        return (int)MyWay > WayColor.Length - 1 ? SimpleColor.White : WayColor[(int)MyWay];
    }

    public static void ResetWay(this Way MyWay)
    {
        MyWay = Way.No;
    }

    public static void SetOut(this Way MyWay)
    {
        MyWay = Way.Out;
    }

    public static void SetIn(this Way MyWay)
    {
        MyWay = Way.In;
    }

    public static void SetSwap(this Way MyWay)
    {
        MyWay = Way.Swap;
    }

    public static bool IsIn(this Way MyWay)
    {
        return MyWay == Way.In;
    }

    public static bool IsOut(this Way MyWay)
    {
        return MyWay == Way.Out;
    }

    public static bool IsSwap(this Way MyWay)
    {
        return MyWay == Way.Swap;
    }

    public static string WayDescription(this Way MyWay, string AutoLabeling, string myDefName, bool IsLinked)
    {
        var empty = string.Empty;
        empty = $"{empty}{AutoLabeling}will {MyWay.WayActionLabeling()} what stands on this {myDefName}";
        if (IsLinked)
        {
            empty += " and its twin.";
        }

        return empty;
    }

    public static Texture2D WayGizmoing(this Way MyWay)
    {
        return (int)MyWay > MyGizmo.WayGizmo.Length - 1 ? null : MyGizmo.WayGizmo[(int)MyWay];
    }
}