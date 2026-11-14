using System.Collections.Generic;

namespace Mapmakers.Sim
{
    public enum StormPhase { None, Warning, Particles, Decay, Over }
    public enum CrewLocation { Inside, Outside, Returning }
    public enum FailReason { None, NoAir, NoFood, RadiationOutside, RadiationNoShelter }

    public sealed class PlacedBuilding
    {
        public int id;
        public BuildingType type;
        public int x;
        public int y;
        public bool enabled = true;
        public bool powered;
        public bool tripped;
        public bool broken;

        public PlacedBuilding Clone() => (PlacedBuilding)MemberwiseClone();
    }

    public sealed class SimEvent
    {
        public double timeHours;
        public int day;
        public string code;
        public string text;
    }

    public sealed class SimWarning
    {
        public string code;
        public string text;
    }

    public sealed class StepReport
    {
        public double generationKW;
        public double demandKW;
        public double servedKW;
        public double batteryFlowKW;
        public double batteryCapacityKWh;
        public double unmetCriticalKW;
        public double airPerHour;
        public double waterPerHour;
        public double foodPerHour;
        public double safetyPerHour;
        public double radiation;
        public bool sheltered;
        public List<SimWarning> warnings = new List<SimWarning>();

        public double HoursToEmpty(double batteryKWh) => batteryFlowKW < 0 ? batteryKWh / -batteryFlowKW : double.PositiveInfinity;
        public bool Has(string code) => warnings.Exists(w => w.code == code);
    }

    // What one Next Day press did. Values are start-of-day and end-of-day snapshots.
    public sealed class DayReport
    {
        public int day;
        public double batteryStart, batteryEnd;
        public double airStart, airEnd;
        public double waterStart, waterEnd;
        public double mealsStart, mealsEnd;
        public double safetyStart, safetyEnd, safetyLowest;
        public int brownouts;
        public bool airRanOut;
        public bool failed;
        public List<SimEvent> events = new List<SimEvent>();
    }

    public sealed class SimState
    {
        public long step;
        public double timeHours;
        public int day = 1;
        public int supplies;
        public double batteryKWh;
        public double airHours;
        public double waterL;
        public double meals;
        public double safety = 100;
        public List<PlacedBuilding> buildings = new List<PlacedBuilding>();
        public int nextId = 1;
        public int maxBatteries = 1;
        public bool shelterUnlocked;

        // Lighting. Independent from the storm: an alert never changes these.
        public double shadowFactor = 1;
        public double shadowEndsAt = -1;
        public double sunrise;
        public double sunriseStartedAt = -1;

        public bool shadowPassed;
        public bool harvestDone;
        public bool completed;

        public StormPhase stormPhase = StormPhase.None;
        public double stormStartHour = -1;
        public double stormStrength = 1;
        public int stormsStarted;
        public int earlyRecalls;
        public CrewLocation geologist = CrewLocation.Inside;
        public double geologistArrivesAt = -1;
        public bool recallIssued;
        public double recallRelHour = -1;

        public double mealsHarvested;
        public double noFoodHours;
        public double lowSafetyHours;
        public double minSafety = 100;
        public double minSafetyStorm = 100;
        public int brownouts;
        public int brownoutDays;
        public int lastBrownoutDay;
        public int stormBrownouts;
        public bool criticalOut;
        public double unmetCriticalKWh;
        public double shelterHours;
        public double batteryAtOnsetKWh = -1;
        public int batteriesAtOnset;
        public bool greenhouseOffInStorm;
        public double damageNoAir;
        public double damageNoFood;
        public double damageOutside;
        public double damageNoShelter;
        public bool failed;
        public FailReason failReason;

        public List<SimEvent> events = new List<SimEvent>();
        public StepReport report = new StepReport();
        public DayReport lastDay;

        public SimState Clone()
        {
            var c = (SimState)MemberwiseClone();
            c.buildings = new List<PlacedBuilding>(buildings.Count);
            foreach (var b in buildings) c.buildings.Add(b.Clone());
            c.events = new List<SimEvent>(events);
            return c;
        }

        public PlacedBuilding Find(int id)
        {
            foreach (var b in buildings)
                if (b.id == id) return b;
            return null;
        }

        public int Count(BuildingType type)
        {
            int n = 0;
            foreach (var b in buildings)
                if (b.type == type) n++;
            return n;
        }

        public bool AnyPowered(BuildingType type)
        {
            foreach (var b in buildings)
                if (b.type == type && b.enabled && b.powered) return true;
            return false;
        }

        public bool StormActive => stormPhase != StormPhase.None && stormPhase != StormPhase.Over;
        public bool Finished => failed || completed;
        public double StormRelHour => stormStartHour < 0 ? -1 : timeHours - stormStartHour;
    }
}
