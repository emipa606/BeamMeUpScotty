using System;

namespace LTF_Teleport;

public static class Status
{
    [Flags]
    public enum BuildingStatus
    {
        na = 0,
        noPower = 1,
        noFacility = 2,
        noItem = 4,
        overweight = 8,
        cooldown = 0x10,
        noPowerNoFacility = 3,
        noPowerNoItem = 5,
        noPowerOverweight = 9,
        noPowerCooldown = 0x11,
        noFacilityNoitem = 6,
        noFacilityoverweight = 0xA,
        noFacilityCooldown = 0x12,
        noItemOverweight = 0xC,
        noItemCooldown = 0x14,
        Overweight = 0x18,
        noPowernoFacilityNoItem = 7,
        noPowerNoFacilityOverweight = 0xB,
        noPowernoFacilityCooldown = 0x13,
        noFacilityNoitemOverweight = 0xE,
        noFacilityNoitemCooldown = 0x16,
        noItemOverweightCooldown = 0x1C,
        powerOk = 0x1E,
        facilityOk = 0x1D,
        itemOk = 0x1B,
        weightOk = 0x17,
        cooldownOk = 0xF,
        allWrong = 0x1F,
        capable = 0x40
    }

    extension(Comp_LTF_TpSpot compSpot)
    {
        public BuildingStatus TeleportCapable()
        {
            var buildingStatus = BuildingStatus.na;
            if (compSpot.tpSpot == null)
            {
                return buildingStatus;
            }

            if (compSpot.Props.requiresPower && !compSpot.tpSpot.HasPower)
            {
                buildingStatus ^= BuildingStatus.noPower;
            }

            if (compSpot.Props.requiresFacility && !compSpot.tpSpot.HasRegisteredFacility)
            {
                buildingStatus ^= BuildingStatus.noFacility;
            }

            if (compSpot.CurrentWeight > compSpot.WeightCapacity)
            {
                buildingStatus ^= BuildingStatus.overweight;
            }

            if (compSpot.Values.WaitingForCooldown)
            {
                buildingStatus ^= BuildingStatus.cooldown;
            }

            switch (compSpot.WParams.myWay)
            {
                case MyWay.Way.Out:
                case MyWay.Way.Swap:
                default:
                    if (compSpot.HasNothing())
                    {
                        buildingStatus ^= BuildingStatus.noItem;
                    }

                    break;
                case MyWay.Way.In:
                    break;
            }

            if (buildingStatus == BuildingStatus.na)
            {
                buildingStatus = BuildingStatus.capable;
            }

            return buildingStatus;
        }

        private bool hasStatus(BuildingStatus buildingStatus)
        {
            return (compSpot.TeleportCapable() & buildingStatus) != 0;
        }

        public bool HasNoPower()
        {
            return compSpot.hasStatus(BuildingStatus.noPower);
        }

        public bool HasPower()
        {
            return !compSpot.HasNoPower();
        }

        private bool StatusNoFacility()
        {
            return compSpot.hasStatus(BuildingStatus.noFacility);
        }

        private bool StatusNoItem()
        {
            return compSpot.hasStatus(BuildingStatus.noItem);
        }

        public bool HasOverweight()
        {
            return compSpot.hasStatus(BuildingStatus.overweight);
        }

        private bool SupportsWeight()
        {
            return !compSpot.HasOverweight();
        }

        public bool StatusWaitingForCooldown()
        {
            if (compSpot.tpSpot == null || compSpot.TwinComp == null)
            {
                return false;
            }

            if (compSpot.hasStatus(BuildingStatus.cooldown))
            {
                return true;
            }

            return compSpot.tpSpot.IsLinked && compSpot.TwinComp.hasStatus(BuildingStatus.cooldown);
        }

        private bool HasNoCooldown()
        {
            return !compSpot.StatusWaitingForCooldown();
        }

        private bool StatusReady()
        {
            return compSpot.hasStatus(BuildingStatus.capable);
        }

        private bool StatusNoIssue()
        {
            var noSpot = true;
            if (compSpot.tpSpot == null)
            {
                return false;
            }

            if (compSpot.Props.requiresPower)
            {
                noSpot &= compSpot.HasPower();
            }

            if (compSpot.Props.requiresFacility)
            {
                noSpot &= compSpot.HasPoweredFacility();
            }

            noSpot &= compSpot.IsLinked();
            noSpot &= compSpot.HasNoCooldown();
            return noSpot & compSpot.SupportsWeight();
        }

        public bool IsLinked()
        {
            return compSpot.tpSpot is { IsLinked: true };
        }

        public bool IsOrphan()
        {
            return compSpot.tpSpot is { IsOrphan: true };
        }

        public bool HasNoFacility()
        {
            return compSpot.StatusNoFacility();
        }

        public bool HasPoweredFacility()
        {
            return compSpot.tpSpot?.facility != null &&
                   Facility.HasPoweredFacility(compSpot.tpSpot.facility, compSpot.tpSpot.compPowerFacility);
        }

        public bool HasNothing()
        {
            return compSpot.tpSpot.values.HasNothing;
        }

        public bool HasItems()
        {
            return compSpot.tpSpot.values.HasItems;
        }

        public bool HasNoIssue()
        {
            return compSpot.StatusNoIssue();
        }

        public bool IsReady()
        {
            return compSpot.StatusReady();
        }

        public bool TeleportOrder()
        {
            return compSpot.tpSpot.teleportOrder;
        }

        public bool HasWarmUp()
        {
            return compSpot.tpSpot.values.WaitingForWarmUp;
        }
    }
}