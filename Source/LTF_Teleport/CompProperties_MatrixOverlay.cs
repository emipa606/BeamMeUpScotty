using UnityEngine;
using Verse;

namespace LTF_Teleport;

public class CompProperties_MatrixOverlay : CompProperties
{
    public readonly Gfx.Layer layer = Gfx.Layer.over;
    public Vector2 animationSize = new(1f, 1f);

    public Vector3 offset = new(0f, 0f, 0f);

    public CompProperties_MatrixOverlay()
    {
        compClass = typeof(CompMatrixOverlay);
    }
}