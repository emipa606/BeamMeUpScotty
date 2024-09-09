using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace LTF_Teleport;

[StaticConstructorOnStartup]
public static class Gfx
{
    public enum AnimStep
    {
        na,
        begin,
        active,
        end
    }

    public enum Layer
    {
        over = 4,
        under = -1
    }

    public enum OpacityWay
    {
        no,
        forced,
        pulse,
        loop
    }

    public static bool ImpossibleMote(Map map, IntVec3 cell)
    {
        if (map == null || !cell.IsValid)
        {
            return true;
        }

        return !cell.InBounds(map) || !cell.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority;
    }

    public static Thing ThrowTpSpotEllipseMote(this Comp_LTF_TpSpot comp)
    {
        var building = comp.building;
        if (building.Negligeable())
        {
            return null;
        }

        var map = building.Map;
        var position = building.Position;
        if (ImpossibleMote(map, position))
        {
            return null;
        }

        var thingDef = comp.EllipseMote();
        if (thingDef == null)
        {
            return null;
        }

        var moteThrown = (MoteThrown)ThingMaker.MakeThing(thingDef);
        moteThrown.exactPosition = building.DrawPos;
        if (comp.IsIn)
        {
            moteThrown.exactPosition.z += 0.75f;
            moteThrown.SetVelocity(Vector3.down.AngleFlat(), -0.6f);
            moteThrown.Scale = 1.5f;
        }
        else if (comp.IsOut)
        {
            moteThrown.SetVelocity(Vector3.up.AngleFlat(), 0.6f);
            moteThrown.Scale = 0.65f;
        }
        else if (comp.IsSwap)
        {
            if (Rand.Chance(0.5f))
            {
                moteThrown.exactPosition.z += 0.75f;
                moteThrown.SetVelocity(Vector3.down.AngleFlat(), -0.6f);
            }
            else
            {
                moteThrown.SetVelocity(Vector3.up.AngleFlat(), 0.6f);
            }
        }

        return GenSpawn.Spawn(moteThrown, position, map);
    }

    public static void ThrowTpSpotFlashMote(this Comp_LTF_TpSpot comp, bool appear = true)
    {
        var building = comp.building;
        if (building.Negligeable())
        {
            return;
        }

        var map = building.Map;
        var position = building.Position;
        if (ImpossibleMote(map, position))
        {
            return;
        }

        var thingDef = appear ? MyGfx.FlashAppearMote : MyGfx.FlashDisappearMote;
        if (thingDef == null)
        {
            return;
        }

        var moteThrown = (MoteThrown)ThingMaker.MakeThing(thingDef);
        moteThrown.exactPosition = building.DrawPos;
        moteThrown.exactRotation = Rand.Range(0, 359);
        moteThrown.rotationRate = 15f;
        GenSpawn.Spawn(moteThrown, position, map);
    }

    public static void ThrowTpSpotSpiralMote(this Comp_LTF_TpSpot comp)
    {
        var building = comp.building;
        if (building.Negligeable())
        {
            return;
        }

        var map = building.Map;
        var position = building.Position;
        if (ImpossibleMote(map, position))
        {
            return;
        }

        var thingDef = comp.SpiralMote();
        if (thingDef == null)
        {
            return;
        }

        var moteThrown = (MoteThrown)ThingMaker.MakeThing(thingDef);
        moteThrown.exactPosition = building.DrawPos;
        moteThrown.exactRotation = Rand.Range(0, 359);
        moteThrown.rotationRate = 60f;
        GenSpawn.Spawn(moteThrown, position, map);
    }

    private static float UpdateOpacity(Thing thing, OpacityWay opacityWay = OpacityWay.no, float opacity = 1f,
        bool debug = false)
    {
        var num = -1f;
        switch (opacityWay)
        {
            case OpacityWay.no:
                num = 1f;
                break;
            case OpacityWay.forced:
                num = opacity;
                break;
            case OpacityWay.pulse:
                num = PulseOpacity(thing);
                break;
            case OpacityWay.loop:
                num = LoopFactorOne(thing);
                break;
        }

        var num2 = num;
        if (num2 != (num = Tools.LimitToRange(num, 0f, 1f)) && debug)
        {
            Log.Warning($"dumb opacity({opacityWay}):{num}(def={opacity})");
        }

        return num;
    }

    public static void DrawTickRotating(Thing thing, Material mat, float x, float z, float size = 1f, float angle = 0f,
        float opacity = 1f, Layer myLayer = Layer.over, bool debug = false)
    {
        var s = new Vector3(size, 1f, size);
        var matrix = default(Matrix4x4);
        var pos = thing.TrueCenter();
        pos.x += x;
        pos.z += z;
        pos.y += (float)myLayer;
        var material = mat;
        if (opacity != 1f)
        {
            material = FadedMaterialPool.FadedVersionOf(mat, opacity);
        }

        if (debug)
        {
            Log.Warning($"Drawing - ang: {angle}; opa:{opacity}");
        }

        matrix.SetTRS(pos, Quaternion.AngleAxis(angle, Vector3.up), s);
        Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
    }

    public static void DrawRandRotating(Thing thing, Material dotM, float x, float z, Layer myLayer = Layer.over,
        bool debug = false)
    {
        var drawPos = thing.DrawPos;
        drawPos.x += x;
        drawPos.z += z;
        drawPos.y += (float)myLayer;
        float num = Rand.Range(0, 360);
        if (debug)
        {
            Log.Warning($"randRot angle: {num}");
        }

        var s = new Vector3(1f, 1f, 1f);
        var matrix = default(Matrix4x4);
        matrix.SetTRS(drawPos, Quaternion.AngleAxis(num, Vector3.up), s);
        Graphics.DrawMesh(MeshPool.plane10, matrix, dotM, 0);
    }

    public static void DrawPulse(Thing thing, Material mat, Mesh mesh, Layer myLayer = Layer.over,
        OpacityWay opacityWay = OpacityWay.no, bool debug = false)
    {
        var num = UpdateOpacity(thing, opacityWay, 1f, debug);
        var material = FadedMaterialPool.FadedVersionOf(mat, num);
        var drawPos = thing.DrawPos;
        drawPos.y += (float)myLayer;
        var s = new Vector3(1f, 1f, 1f);
        var matrix = default(Matrix4x4);
        matrix.SetTRS(drawPos, Quaternion.AngleAxis(0f, Vector3.up), s);
        Graphics.DrawMesh(mesh, matrix, material, 0);
        if (debug)
        {
            Log.Warning($"{thing.LabelShort}; opa: {num}; pos: {drawPos}; col: {mat.color}");
        }
    }

    public static void DrawColorPulse(Thing thing, Material mat, Vector3 drawPos, Mesh mesh, Color color)
    {
        var num = PulseOpacity(thing);
        var material = FadedMaterialPool.FadedVersionOf(mat, num);
        ChangeColor(mat, color, num);
        var s = new Vector3(1f, 1f, 1f);
        var matrix = default(Matrix4x4);
        matrix.SetTRS(drawPos, Quaternion.AngleAxis(0f, Vector3.up), s);
        Graphics.DrawMesh(mesh, matrix, material, 0);
    }

    public static void Draw1x1Overlay(Vector3 buildingPos, Material gfx, Mesh mesh, float drawSize, bool debug)
    {
        Graphics.DrawMesh(mesh, default, gfx, 0);
        if (debug)
        {
            Log.Warning($"Drew:{gfx.color}");
        }
    }

    public static void Draw1x1OverlayBS(Vector3 buildingPos, Material gfx, Mesh mesh, float x, float z,
        Color overlayColor, bool randRotation = false, bool randFlicker = false, bool oscillatingOpacity = false,
        float currentOpacity = 0.5f, float noFlickChance = 0.985f, float minOpacity = 0.65f, float maxOpacity = 1f,
        bool debug = false)
    {
        var pos = buildingPos;
        pos.x += x;
        pos.z += z;
        var s = new Vector3(1f, 1f, 1f);
        var matrix = default(Matrix4x4);
        var chance = 1f - noFlickChance;
        var angle = 0f;
        if (randRotation)
        {
            angle = Rand.Range(0, 360);
        }

        var num = maxOpacity;
        if (randFlicker && Rand.Chance(chance))
        {
            num -= (currentOpacity - minOpacity) / 4f;
        }

        if (num < minOpacity)
        {
            num = minOpacity;
        }
        else if (num > maxOpacity)
        {
            num = maxOpacity;
        }

        var material = FadedMaterialPool.FadedVersionOf(gfx, num);
        matrix.SetTRS(pos, Quaternion.AngleAxis(angle, Vector3.up), s);
        if (mesh == null)
        {
            Log.Warning("mesh null");
            return;
        }

        var color = overlayColor;
        color.a = num;
        material.color = color;
        Graphics.DrawMesh(mesh, matrix, material, 0);
        if (debug)
        {
            Log.Warning($"Drew:{color}");
        }
    }

    public static void ChangeColor(Material mat, Color color, float opacity = -1f, bool debug = false)
    {
        var color2 = color;
        if (debug)
        {
            Log.Warning($"In Color: {mat.color}");
        }

        if (opacity == -1f)
        {
            color2.a = Tools.LimitToRange(mat.color.a, 0f, 1f);
        }

        color2.a = opacity;
        mat.color = color2;
        if (debug)
        {
            Log.Warning($"Out Color: {mat.color}");
        }
    }

    public static void PulseWarning(Thing thing, Material mat)
    {
        var material = FadedMaterialPool.FadedVersionOf(mat, VanillaPulse(thing));
        var s = new Vector3(0.6f, 1f, 0.6f);
        var matrix = default(Matrix4x4);
        var plane = MeshPool.plane14;
        matrix.SetTRS(thing.DrawPos, Quaternion.AngleAxis(0f, Vector3.up), s);
        Graphics.DrawMesh(plane, matrix, material, 0);
    }

    public static float PulseOpacity(Thing thing, float mask = 1f, bool debug = false)
    {
        var num = (Time.realtimeSinceStartup + (397f * (thing.thingIDNumber % 571))) * 4f;
        var num2 = ((float)Math.Sin(num) + 1f) * 0.5f;
        if (debug)
        {
            Log.Warning($"pulse opacity: !{num2}; mask: {mask}; masked: {num % mask}");
        }

        return num2 % mask;
    }

    public static float LoopFactorOne(Thing thing, float mask = 1f, bool debug = false)
    {
        var num = (Time.realtimeSinceStartup + (397f * (thing.thingIDNumber % 571))) * 4f;
        var num2 = ((float)Math.Tan(num) + 1f) * 0.5f;
        Tools.Warn($"loop factor one{num2}; mask: {mask}; masked: {num % mask}", debug);
        return num2 % mask;
    }

    public static float VanillaPulse(Thing thing)
    {
        var num = (Time.realtimeSinceStartup + (397f * (thing.thingIDNumber % 571))) * 4f;
        var num2 = ((float)Math.Sin(num) + 1f) * 0.5f;
        return 0.3f + (num2 * 0.7f);
    }

    public static float PulseFactorOne(Thing thing, float mask = 1f, bool debug = false)
    {
        var num = 397f;
        var num2 = 571f;
        var num3 = 2f;
        var num4 = (Time.realtimeSinceStartup + (num * (thing.thingIDNumber % num2))) * num3;
        var num5 = ((float)Math.Sin(num4) + 1f) * 0.5f;
        Tools.Warn($"pulse factor one: {num5}; mask: {mask}; masked: {num4 % mask}", debug);
        return num5 % mask;
    }

    public static float RealLinear(Thing thing, float speedUp, bool debug = false)
    {
        var num = 397f;
        var num2 = 571f;
        var num3 = (Time.realtimeSinceStartup + (num * (thing.thingIDNumber % num2))) * speedUp * 20f;
        var num4 = num3 % 1000f / 1000f;
        if (debug)
        {
            Log.Warning($"RealLinear: {num3}->{num4}");
        }

        return num4;
    }

    public static float RealtimeFactor(Thing thing, float mask = 1f, bool debug = false)
    {
        var num = 397f;
        var num2 = 571f;
        var num3 = 2f;
        var num4 = (Time.realtimeSinceStartup + (num * (thing.thingIDNumber % num2))) * num3;
        var num5 = ((float)Math.Cos(0f - num4) + 1f) * 0.5f;
        Tools.Warn($"RealtimeFactor:{num5}->{num5 % mask}", debug);
        return num5 % mask;
    }
}