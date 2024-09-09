using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LTF_Teleport;

public class WorkValues : IExposable
{
    public float currentCooldown;

    public float currentWeight;

    public bool factionMajority;
    public float orderRange;

    public Pawn standingUser;

    public List<Thing> thingList = [];

    public int warmUpCalculated;

    public int warmUpLeft;

    public int warmUpDone => warmUpCalculated - warmUpLeft;

    public bool WaitingForWarmUp => warmUpLeft > 0;

    public float WarmUpProgress
    {
        get
        {
            if (warmUpCalculated == 0)
            {
                return 0f;
            }

            if (warmUpLeft == 0)
            {
                return 1f;
            }

            return warmUpLeft / (float)warmUpCalculated;
        }
    }

    public bool WaitingForCooldown => currentCooldown > 0f;

    public bool HasRegisteredPawn => standingUser != null;

    public bool HasAnimal =>
        HasRegisteredPawn && !standingUser.RaceProps.Humanlike && !standingUser.RaceProps.IsMechanoid;

    public bool HasMechanoid => HasRegisteredPawn && standingUser.RaceProps.IsMechanoid;

    public bool HasHumanoid => HasRegisteredPawn && !HasAnimal;

    public int RegisteredCount => thingList.Count;

    public bool HasItems => !thingList.NullOrEmpty();

    public bool HasNothing => !HasItems;

    public void ExposeData()
    {
        Scribe_Values.Look(ref orderRange, "Values_orderRange");
        Scribe_Values.Look(ref warmUpCalculated, "Values_warmUpCalculated");
        Scribe_Values.Look(ref warmUpLeft, "Values_warmUpLeft");
        Scribe_Values.Look(ref currentWeight, "Values_currentWeight");
        Scribe_Values.Look(ref currentCooldown, "Values_currentCooldown");
        Scribe_References.Look(ref standingUser, "Values_standingUser");
        Scribe_Values.Look(ref factionMajority, "Values_factionMajority");
        Scribe_Collections.Look(ref thingList, "things", LookMode.Reference);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && thingList == null)
        {
            thingList = [];
        }
    }

    public string WarmUpString()
    {
        return Tools.TimeLeftString(warmUpDone, warmUpCalculated);
    }

    public void SetWarmUpLeft()
    {
        warmUpLeft = warmUpCalculated;
    }

    public string DumpList()
    {
        var text = string.Empty;
        var num = 0;
        foreach (var thing in thingList)
        {
            text = $"{text}{num:D1}:{thing.Label}; ";
            num++;
        }

        return text;
    }

    private void SetPawn(Pawn pawn = null)
    {
        standingUser = pawn;
    }

    public void ResetPawn()
    {
        SetPawn();
    }

    public void ResetWeight()
    {
        currentWeight = 0f;
    }

    private void AddWeight(Thing thing)
    {
        ChangeWeight(thing);
    }

    private void RemoveWeight(Thing thing)
    {
        ChangeWeight(thing, false);
    }

    private void ChangeWeight(Thing thing, bool addWeight = true, bool debug = false)
    {
        var statValue = thing.GetStatValue(StatDefOf.Mass);
        float num = addWeight ? 1 : -1;
        currentWeight += num * statValue;
        currentWeight = Tools.LimitToRange(currentWeight, 0f, 3000f);
        currentWeight = (float)Math.Round((decimal)currentWeight, 2, MidpointRounding.AwayFromZero);
        if (debug)
        {
            Log.Warning($"{thing.LabelShort} adds({num}){statValue} -> {currentWeight}");
        }
    }

    public void ResetItems()
    {
        thingList.Clear();
        ResetWeight();
    }

    private bool RemoveItemsIfAbsent(Building building)
    {
        if (HasNothing)
        {
            return false;
        }

        var num = thingList.Count / 2;
        for (var num2 = thingList.Count - 1; num2 >= 0; num2--)
        {
            var thing = thingList[num2];
            if (thing == null)
            {
                continue;
            }

            if (thing is Pawn pawn && standingUser != null &&
                (pawn != standingUser || pawn.Position != building.Position))
            {
                ResetPawn();
            }

            if (thing.Position != building.Position || thing == building)
            {
                RemoveItem(thing);
            }
        }

        factionMajority = num <= 0;
        return HasItems;
    }

    private void AddItem(Thing thing, bool debug = false)
    {
        if (debug)
        {
            Log.Warning($"Adding {thing.Label}");
        }

        thingList.Add(thing);
        AddWeight(thing);
    }

    private void RemoveItem(Thing thing, bool debug = false)
    {
        if (debug)
        {
            Log.Warning($"Removing {thing.Label}");
        }

        thingList.Remove(thing);
        RemoveWeight(thing);
    }

    public bool CheckItems(Building building)
    {
        var removeItemsIfAbsent = false;
        removeItemsIfAbsent |= RemoveItemsIfAbsent(building);
        return removeItemsIfAbsent | AddSpotItems(building);
    }

    private bool AddSpotItems(Building building, bool debug = false)
    {
        var list = building.Position.GetThingList(building.Map);
        if (debug)
        {
            Log.Warning($"{building.Label} checking items");
        }

        var addSpotItems = false;
        var num = 0;
        Pawn pawn = null;
        if (debug)
        {
            Log.Warning($"{building.Label}:{list.Count}");
        }

        var text = "Scanning: ";
        foreach (var thing in list)
        {
            if (thing == null)
            {
                break;
            }

            if (thing.def.mote != null || thing.def.plant != null || thing.def.IsFilth ||
                thing.def.IsBuildingArtificial)
            {
                text += "mote / filth / building skip";
                if (debug)
                {
                    Log.Warning(text);
                }

                continue;
            }

            if (thing is Building)
            {
                text += "Wont self register as an item";
                if (debug)
                {
                    Log.Warning(text);
                }

                continue;
            }

            if (thing is Pawn pawn2)
            {
                pawn = pawn2;
                num++;
            }

            if (!thingList.Contains(thing))
            {
                AddItem(thing);
                text = $"{text}{thing.Label} added;";
            }

            addSpotItems = true;
        }

        if (debug)
        {
            Log.Warning(text);
        }

        if (num == 0)
        {
            ResetPawn();
        }
        else if (num > 1)
        {
            ResetPawn();
            ResetItems();
        }
        else
        {
            SetPawn(pawn);
        }

        if (!addSpotItems)
        {
            ResetItems();
        }

        return addSpotItems;
    }
}