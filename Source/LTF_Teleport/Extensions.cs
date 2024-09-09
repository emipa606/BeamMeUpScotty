using System;

namespace LTF_Teleport;

public static class Extensions
{
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");
        }

        var array = (T[])Enum.GetValues(src.GetType());
        var num = Array.IndexOf(array, src) + 1;
        return array.Length == num ? array[0] : array[num];
    }
}