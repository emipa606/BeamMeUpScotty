using System;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

public class Graphic_MatrixOverlay : Graphic_Collection
{
    private const int BaseTicksPerFrameChange = 7;

    private const int ExtraTicksPerFrameChange = 10;

    private const float MaxOffset = 0.05f;

    public override Material MatSingle
    {
        get
        {
            var num = (int)Math.Floor((float)Find.TickManager.TicksGame / BaseTicksPerFrameChange);
            var num2 = num % subGraphics.Length;
            return subGraphics[num2].MatSingle;
        }
    }

    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        if (thingDef == null)
        {
            Log.ErrorOnce($"Graphic_Animation DrawWorker with null thingDef: {loc}", 3427324);
            return;
        }

        if (subGraphics == null)
        {
            Log.ErrorOnce($"Graphic_Animation has no subgraphics {thingDef}", 358773632);
            return;
        }

        var vector = new Vector2(1f, 1f);
        var num = 4f;
        var drawPos = thing.DrawPos;
        var compProperties_MatrixOverlay = thingDef.GetCompProperties<CompProperties_MatrixOverlay>();
        if (compProperties_MatrixOverlay != null)
        {
            vector = compProperties_MatrixOverlay.animationSize;
            num = (float)compProperties_MatrixOverlay.layer;
            drawPos += compProperties_MatrixOverlay.offset;
        }

        var s = new Vector3(vector.x, 1f, vector.y);
        var matrix = default(Matrix4x4);
        drawPos.y += num;
        matrix.SetTRS(drawPos, Quaternion.AngleAxis(0f, Vector3.up), s);
        Graphics.DrawMesh(MeshPool.plane14, matrix, MatSingle, (int)num);
    }
}