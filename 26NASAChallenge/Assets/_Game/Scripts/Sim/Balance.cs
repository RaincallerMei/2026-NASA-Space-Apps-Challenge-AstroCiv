using System;
using System.Collections.Generic;

namespace Mapmakers.Sim
{
    public enum BuildingType { Habitat, SolarArray, Battery, LifeSupport, Communications, Greenhouse, Shelter, RoverGarage }

    [Serializable]
    public sealed class BuildingDef
    {
        public string type;
        public string displayName;
        public string blurb;
        public int width = 1;
        public int height = 1;
        public int cost;
        public int soilCost;
        public int maxCount = 1;
        public double generationKW;
        public double demandKW;
        public int priority;
        public bool optional;
        public double batteryKWh;
        public double batteryRateKW;
        public double waterInLph;
        public double waterReturnLph;
        public double foodPerHour;
        public double airBonusPerHour;
    }

    // Storm times are game hours after the warning.
    [Serializable]
    public sealed class StormTimeline
    {
        public double onsetHour = 12;
        public double decayHour = 24;
        public double endHour = 30;
        public double recallWalkHours = 1.5;
        public double selfReturnDelayHours = 1;
    }

    [Serializable]
    public sealed class ShadowEventDef
    {
        public double warningHours = 8;
        public double durationHours = 12;
        public double factor = 0.7;
    }

    [Serializable]
    public sealed class TerrainRules
    {
        public double sunFloor = 0.5;
        public double sunFullAtPercent = 65;
        public double blockedBelowPercent = 10;
        public int[] rockTiles = new int[0];
        public int[] soilTiles = new int[0];
    }

    // Jezero crater (Mars) from MOLA heights. All thresholds are game choices.
    [Serializable]
    public sealed class MarsTerrainRules
    {
        public double slopeBlockedM = 450;
        public double sunLow = 0.7;
        public double fullSunAboveFloorM = 800;
        public double soilBelowM = -2550;
        public int[] waterTiles = new int[0];
    }

    // How building positions help or hurt each other. Gap = empty tiles between two footprints (touching = 0).
    [Serializable]
    public sealed class LayoutRules
    {
        public int baseRadius = 6;
        public int cableGap = 2;
        public double longCableLoss = 0.3;
        public int ductGap = 1;
        public double longDuctAirFactor = 0.5;
        public int plantLoopGap = 1;
        public double plantLoopSavesKW = 1;
        public int shelterGap = 2;
        public double farShelterExposure = 0.5;
        public int roverRange = 8;
        public double roverBaseTripHours = 1;
        public double roverHoursPerTile = 0.5;
        public double soilPerTrip = 2;
        public double waterPerTrip = 4;
        public double soilStockCap = 12;
    }

    // All values are FICTIONAL BALANCE numbers for play, not equipment specifications.
    [Serializable]
    public sealed class Balance
    {
        public string classification = "fictional balance";
        public double dtHours = 0.05;
        public double realSecondsPerGameHour = 2;
        public double maxRealSecondsPerFrame = 0.25;
        public int startClockHour = 8;

        public int startSupplies = 100;
        public double startWaterL = 100;
        public double startMeals = 20;
        public double startAirHours = 36;
        public double airCapHours = 36;
        public double startSoil = 0;

        public int crewCount = 3;
        public double mealsPerCrewPerDay = 3;
        public double waterPerCrewPerHour = 0.13;
        public double airRefillPerHour = 2;
        public double airDrainPerHour = 1;
        public double restoreBatteryFraction = 0.5;

        public double warnHours = 12;
        public double criticalHours = 4;
        public double powerLossToleranceHours = 10;

        public double safetyRecoverPerHour = 1;
        public double radiationOutsidePerHour = 10;
        public double radiationUnshelteredPerHour = 6;
        public double safetyFailThreshold = 50;
        public double safetyFailHours = 2;
        public double perfectSafety = 95;

        public double harvestMeals = 3;
        public int harvestSupplyReward = 30;
        public int startMaxBatteries = 1;
        public int unlockedMaxBatteries = 2;
        public double sunriseHours = 6;
        public int repairCost = 5;

        public StormTimeline storm = new StormTimeline();
        public ShadowEventDef shadow = new ShadowEventDef();
        public TerrainRules terrain = new TerrainRules();
        public MarsTerrainRules mars = new MarsTerrainRules();
        public LayoutRules layout = new LayoutRules();
        public GradeRules grade = new GradeRules();
        public BuildingDef[] buildings = new BuildingDef[0];

        [NonSerialized] Dictionary<BuildingType, BuildingDef> byType;

        public BuildingDef Def(BuildingType type)
        {
            if (byType == null)
            {
                byType = new Dictionary<BuildingType, BuildingDef>();
                foreach (var d in buildings)
                    byType[(BuildingType)Enum.Parse(typeof(BuildingType), d.type)] = d;
            }
            return byType[type];
        }

        public double CrewMealsPerHour => crewCount * mealsPerCrewPerDay / 24.0;
        public double CrewWaterPerHour => crewCount * waterPerCrewPerHour;
    }

    // Grade weights and caps. Rows add up to 100.
    [Serializable]
    public sealed class GradeRules
    {
        public int baseAndGoals = 30;
        public int layout = 20;
        public int crewSafety = 20;
        public int reserves = 15;
        public int steadyPower = 15;
        public int missingCoreCap = 60;
        public int missingGoalCap = 80;
        public int failCap = 30;
        public double shedPenalty = 3;
        public double criticalPenalty = 8;
    }
}
