using System;
using System.Collections.Generic;

namespace Mapmakers.Sim
{
    public enum BuildingType { Habitat, SolarArray, Battery, LifeSupport, Communications, Greenhouse, Shelter }

    [Serializable]
    public sealed class BuildingDef
    {
        public string type;
        public string displayName;
        public string blurb;
        public int width = 1;
        public int height = 1;
        public int cost;
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

    [Serializable]
    public sealed class StormTimeline
    {
        public double leadHours = 3;
        public double evaPromptHour = 2;
        public double onsetHour = 4;
        public double decayHour = 10;
        public double endHour = 12;
        public double recallWalkHours = 1.5;
        public double selfReturnDelayHours = 1;
    }

    [Serializable]
    public sealed class ShadowEventDef
    {
        public double warningHours = 2;
        public double durationHours = 3;
        public double factor = 0.7;
    }

    [Serializable]
    public sealed class TerrainRules
    {
        public double sunFloor = 0.5;
        public double sunFullAtPercent = 65;
        public double blockedBelowPercent = 10;
        public int[] rockTiles = new int[0];
    }

    // All values are FICTIONAL BALANCE numbers for play, not equipment specifications.
    [Serializable]
    public sealed class Balance
    {
        public string classification = "fictional balance";
        public double dtHours = 0.05;
        public double realSecondsPerGameHour = 5;
        public int startClockHour = 8;

        public int startSupplies = 150;
        public double startWaterL = 100;
        public double startMeals = 20;
        public double startAirHours = 24;
        public double airCapHours = 24;

        public int crewCount = 3;
        public double mealsPerCrewPerDay = 3;
        public double airRefillPerHour = 2;
        public double airDrainPerHour = 1;
        public double restoreBatteryFraction = 0.5;

        public double safetyRecoverPerHour = 1;
        public double safetyNoAirPerHour = 6;
        public double safetyNoFoodPerHour = 2;
        public double noFoodGraceHours = 6;
        public double radiationOutsidePerHour = 10;
        public double radiationUnshelteredPerHour = 6;
        public double safetyFailThreshold = 50;
        public double safetyFailHours = 2;
        public double closeCallSafety = 70;
        public double perfectSafety = 95;

        public double harvestMeals = 3;
        public int harvestSupplyReward = 30;
        public int startMaxBatteries = 1;
        public int unlockedMaxBatteries = 2;
        public double sunriseHours = 3;
        public int repairCost = 5;

        public StormTimeline storm = new StormTimeline();
        public ShadowEventDef shadow = new ShadowEventDef();
        public TerrainRules terrain = new TerrainRules();
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
    }
}
