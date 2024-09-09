using RimWorld;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

[StaticConstructorOnStartup]
public class CompMatrixOverlay : ThingComp
{
    public static readonly Graphic AnimationGraphic =
        GraphicDatabase.Get<Graphic_MatrixOverlay>("AnimationOverlay/matrix", ShaderDatabase.TransparentPostLight,
            Vector2.one, Color.white);

    protected CompPowerTrader powerComp;

    public CompProperties_MatrixOverlay Props => (CompProperties_MatrixOverlay)props;

    public override void PostDraw()
    {
        base.PostDraw();
        if (parent == null || !powerComp.PowerOn)
        {
            return;
        }

        var drawPos = parent.DrawPos;
        AnimationGraphic.Draw(drawPos, Rot4.North, parent);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        powerComp = parent.GetComp<CompPowerTrader>();
    }
}