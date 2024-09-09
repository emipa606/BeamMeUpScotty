using Verse;

namespace LTF_Teleport;

public static class MyDefs
{
    public static readonly ThingDef tpBenchDef = ThingDef.Named("LTF_TpBench");

    public static readonly ThingDef tpSpotDef = ThingDef.Named("LTF_TpSpot");

    public static readonly ThingDef tpCatcherDef = ThingDef.Named("LTF_TpCatcher");

    public static readonly ThingDef miniStationDef = ThingDef.Named("LTF_MiniStation");

    public static readonly ThingDef tpBedDef = ThingDef.Named("LTF_TpBed");

    public static readonly ThingDef tpBoxDef = ThingDef.Named("LTF_TpBox");

    public static bool UnpoweredTp(this Thing tpThing)
    {
        return tpThing.def == tpBedDef || tpThing.def == tpBoxDef || tpThing.def == tpCatcherDef;
    }

    public static bool IsTpSpot(this Thing thing)
    {
        return thing.def == tpSpotDef;
    }

    public static bool IsMiniStation(this Thing thing)
    {
        return thing.def == miniStationDef;
    }
}