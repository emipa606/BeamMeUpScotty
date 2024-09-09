using UnityEngine;
using Verse;

namespace LTF_Teleport;

[StaticConstructorOnStartup]
public static class MyGfx
{
    public static readonly string tpSpotPath = "Things/Building/TpSpot/";

    public static readonly string underlayPath = $"{tpSpotPath}Underlay/";

    public static readonly string overlayPath = $"{tpSpotPath}Overlay/";

    public static readonly string warningPath = $"{overlayPath}Warning/";

    public static readonly Material WayOutM = MaterialPool.MatFrom($"{underlayPath}WayOut", ShaderDatabase.MoteGlow);

    public static readonly Material WayInM = MaterialPool.MatFrom($"{underlayPath}WayIn", ShaderDatabase.MoteGlow);

    public static readonly Material WaySwapM = MaterialPool.MatFrom($"{underlayPath}WaySwap", ShaderDatabase.MoteGlow);

    public static readonly Material InitOrderM =
        MaterialPool.MatFrom($"{underlayPath}InitOrder", ShaderDatabase.MoteGlow);

    public static readonly Material
        UnderlayM = MaterialPool.MatFrom($"{underlayPath}Underlay", ShaderDatabase.MoteGlow);

    public static readonly Material OverweightM =
        MaterialPool.MatFrom($"{warningPath}Overweight", ShaderDatabase.MetaOverlay);

    public static readonly Material CooldownM =
        MaterialPool.MatFrom($"{warningPath}Cooldown", ShaderDatabase.MetaOverlay);

    public static readonly Material FacilityM =
        MaterialPool.MatFrom($"{warningPath}Facility", ShaderDatabase.MetaOverlay);

    public static readonly Material FacilityPowerM =
        MaterialPool.MatFrom($"{warningPath}FacilityPower", ShaderDatabase.MetaOverlay);

    public static readonly ThingDef[] EllipseMoteList =
    [
        null,
        ThingDef.Named("LTF_Teleport_Mote_EllipseOut"),
        ThingDef.Named("LTF_Teleport_Mote_EllipseIn"),
        ThingDef.Named("LTF_Teleport_Mote_EllipseSwap")
    ];

    public static readonly ThingDef[] SpiralMoteList =
    [
        null,
        ThingDef.Named("LTF_Teleport_Mote_SpiralOut"),
        ThingDef.Named("LTF_Teleport_Mote_SpiralIn"),
        ThingDef.Named("LTF_Teleport_Mote_SpiralSwap")
    ];

    public static readonly ThingDef FlashAppearMote = ThingDef.Named("LTF_Teleport_Mote_FlashAppear");

    public static readonly ThingDef FlashDisappearMote = ThingDef.Named("LTF_Teleport_Mote_FlashDisappear");

    public static ThingDef EllipseMote(this Comp_LTF_TpSpot comp)
    {
        if (comp.WParams == null || !comp.HasSetWay)
        {
            return null;
        }

        return EllipseMoteList[(int)comp.WParams.myWay];
    }

    public static ThingDef SpiralMote(this Comp_LTF_TpSpot comp)
    {
        if (comp.WParams == null || !comp.HasSetWay)
        {
            return null;
        }

        return SpiralMoteList[(int)comp.WParams.myWay];
    }

    public static Material Status2WarningMaterial(Comp_LTF_TpSpot comp, bool debug = false)
    {
        var empty = string.Empty;
        if (comp.HasNoFacility())
        {
            empty = "no facility gfx";
            if (debug)
            {
                Log.Warning(empty);
            }

            return FacilityM;
        }

        if (!comp.HasPoweredFacility())
        {
            empty = "no facility power gfx";
            if (debug)
            {
                Log.Warning(empty);
            }

            return FacilityPowerM;
        }

        if (comp.StatusWaitingForCooldown())
        {
            empty = "cooldown gfx";
            if (debug)
            {
                Log.Warning(empty);
            }

            return CooldownM;
        }

        if (comp.HasOverweight())
        {
            empty = "overweight gfx";
            if (debug)
            {
                Log.Warning(empty);
            }

            return OverweightM;
        }

        empty += "everything fine;no warning";
        if (debug)
        {
            Log.Warning(empty);
        }

        return null;
    }

    public static Material Status2UnderlayMaterial(Comp_LTF_TpSpot comp, bool debug = false)
    {
        Material material;
        var text = "Gfx - Under: ";
        switch (comp.WParams.myWay)
        {
            case MyWay.Way.Out:
                material = WayOutM;
                break;
            case MyWay.Way.In:
                material = WayInM;
                break;
            case MyWay.Way.Swap:
                material = WaySwapM;
                break;
            default:
                return null;
        }

        if (comp.TpSequenceBegin)
        {
            material = InitOrderM;
        }

        if (debug)
        {
            Log.Warning($"{text}null");
        }

        return material;
    }
}