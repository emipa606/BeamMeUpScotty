using RimWorld;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

public class ToolsQuality
{
    public static string VirtualQuality(CompQuality comp, int relativeChange = 0)
    {
        var result = "no quality comp";
        if (comp == null)
        {
            return result;
        }

        var qualityCategory = QualityCategory.Normal;
        if (relativeChange > 0)
        {
            if (comp.Quality != QualityCategory.Legendary)
            {
                qualityCategory = comp.Quality + 1;
            }
        }
        else if (comp.Quality != 0)
        {
            qualityCategory = comp.Quality - 1;
        }

        return qualityCategory.ToString();
    }

    public static bool ChangeQuality(Building building, CompQuality comp, bool better = true)
    {
        comp ??= building?.TryGetComp<CompQuality>();

        if (comp == null)
        {
            return false;
        }

        var qualityCategory = comp.Quality;
        var qualityCategory2 = qualityCategory;
        if (better)
        {
            if (qualityCategory != QualityCategory.Legendary)
            {
                qualityCategory++;
            }
        }
        else if (qualityCategory != 0)
        {
            qualityCategory--;
        }

        return qualityCategory2 != qualityCategory;
    }

    public static float WeightedCapacity(float capacityBase, float capacitySpectrum, CompQuality comp = null,
        bool debug = false)
    {
        if (comp != null)
        {
            return capacityBase + ((int)comp.Quality * (capacitySpectrum / 8f));
        }

        if (debug)
        {
            Log.Warning("no qualit comp found");
        }

        return capacityBase;
    }

    public static float FactorCapacity(float capacityBase, float factor, CompQuality comp = null, bool pow2 = false,
        bool round = false, bool opposite = false, bool debug = false)
    {
        if (comp == null)
        {
            Tools.Warn($"no quality comp found, ret={capacityBase}", debug);
            return capacityBase;
        }

        if (opposite && round)
        {
            if (debug)
            {
                Log.Warning("chance with roundup");
            }

            return capacityBase;
        }

        float num2 = (int)comp.Quality;
        var num = !pow2 ? capacityBase + (factor * num2) : Mathf.Pow(2f, (num2 + capacityBase) * factor);
        if (opposite)
        {
            if (num == 0f)
            {
                if (debug)
                {
                    Log.Warning("0 div");
                }

                return capacityBase;
            }

            num = 1f / num;
        }

        if (round)
        {
            num = Mathf.RoundToInt(num);
        }

        Tools.Warn($"factor capacity, ret={capacityBase}", debug);
        return num;
    }
}