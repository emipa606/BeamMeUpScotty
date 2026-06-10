using Verse;

namespace LTF_Teleport;

public static class MyDefs
{
    public static readonly ThingDef tpBenchDef = ThingDef.Named("LTF_TpBench");

    public static readonly ThingDef tpSpotDef = ThingDef.Named("LTF_TpSpot");

    public static readonly ThingDef tpCatcherDef = ThingDef.Named("LTF_TpCatcher");

    private static readonly ThingDef miniStationDef = ThingDef.Named("LTF_MiniStation");

    private static readonly ThingDef tpBedDef = ThingDef.Named("LTF_TpBed");

    private static readonly ThingDef tpBoxDef = ThingDef.Named("LTF_TpBox");

    extension(Thing tpThing)
    {
        public bool UnpoweredTp()
        {
            return tpThing.def == tpBedDef || tpThing.def == tpBoxDef || tpThing.def == tpCatcherDef;
        }

        public bool IsTpSpot()
        {
            return tpThing.def == tpSpotDef;
        }

        public bool IsMiniStation()
        {
            return tpThing.def == miniStationDef;
        }
    }
}