using System;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

public static class Tools
{
    public static bool TwoTicksOneTrue(int period = 60, bool debug = false)
    {
        if (debug)
        {
            Log.Warning(
                $"\u00b0\u00b0\u00b0\u00b0\u00b0\u00b02t1true{(int)(Time.realtimeSinceStartup * 100f % period)}");
        }

        return (int)(Time.realtimeSinceStartup * 100f % period) == 1;
    }

    public static string OkStr(bool boolean = false)
    {
        return $"[{(boolean ? "OK" : "KO")}]";
    }

    public static string IfStr(bool boolean, string myString, bool AddCr = true)
    {
        var text = string.Empty;
        if (AddCr)
        {
            text = "\n";
        }

        if (boolean)
        {
            text += myString;
        }

        return text;
    }

    public static string PosStr(IntVec3 position)
    {
        return $" [{position.x};{position.z}];";
    }

    public static string LabelByDefName(string DefName, bool debug = false)
    {
        var empty = ThingDef.Named(DefName)?.label;
        if (debug)
        {
            Log.Warning($"Answer: {empty}");
        }

        return empty;
    }

    public static string Ticks2Str(float ticks)
    {
        return $"{Math.Round(ticks / 60f, 1)}s";
    }

    public static void Warn(string warning, bool debug = false)
    {
        if (debug)
        {
            Log.Warning(warning);
        }
    }

    public static void WarnRare(string warning, int period = 300, bool debug = false)
    {
        if (debug && Find.TickManager.TicksGame % period == 0)
        {
            Log.Warning(warning);
        }
    }

    public static float LimitToRange(float val, float min, float max)
    {
        if (val < min)
        {
            return min;
        }

        return val > max ? max : val;
    }

    public static int LimitToRange(int val, int min, int max)
    {
        if (val < min)
        {
            return min;
        }

        return val > max ? max : val;
    }

    public static float LimitRadius(float value)
    {
        return LimitToRange(value, 0f, 55f);
    }

    public static int NextIndexRoundBrowser(int index, int count)
    {
        if (count == 1)
        {
            return 0;
        }

        if (index + 1 >= count)
        {
            return 0;
        }

        return index + 1;
    }

    public static string TimeLeftString(int ticksDone, int ticksMax)
    {
        return $"{ticksDone / 60f:F1}/{ticksMax / 60f:F1}s";
    }

    public static string CapacityString(float capacity, float capacityMax)
    {
        return $"{(int)capacity}/{(int)capacityMax}";
    }

    public static string PawnResumeString(Pawn pawn)
    {
        return
            $"{pawn?.LabelShort.CapitalizeFirst()}, {pawn?.ageTracker?.AgeBiologicalYears ?? 0} y/o {pawn?.gender.GetLabel()}, {pawn?.def?.label}({pawn?.kindDef})";
    }

    public static string DebugStatus(bool debug)
    {
        return $"{debug}->{!debug}";
    }

    public static bool WarnBoolToggle(bool boolean, string PropertyName = "Debug")
    {
        Log.Warning($"{(boolean ? "<<<" : ">>>")} {PropertyName} {DebugStatus(boolean)}");
        boolean = !boolean;
        return boolean;
    }

    public static string DescriptionAttr<T>(this T source)
    {
        var field = source.GetType().GetField(source.ToString());
        var array = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return array.Length != 0 ? array[0].description : source.ToString();
    }
}