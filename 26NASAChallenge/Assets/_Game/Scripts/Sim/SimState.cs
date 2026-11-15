using System.Collections.Generic;

namespace Mapmakers.Sim
{
    public enum StormPhase { None, Warning, Particles, Decay, Over }
    public enum CrewLocation { Inside, Outside, Returning }

    // Why a run ended. Running out of mission time is an evaluation, never a failure.
    public enum MissionEnd { None, Deadline, Failed }
    public enum FailReason { None, NoAir, NoFood, NoWater, PowerLoss, RadiationOutside, RadiationNoShelter }

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
        public double soilPerHour;
        public double safetyPerHour;
        public double radiation;
        public bool sheltered;
        public List<SimWarning> warnings = new List<SimWarning>();

        public bool Has(string code) => warnings.Exists(w => w.code == code);
    }

    public sealed class SimState
    {
        public long step;
        public double timeHours;
        public int supplies;
        public double batteryKWh;
        public double airHours;
        public double waterL;
        public double meals;
        public double soil;
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
        public int nextEvent;

        public StormPhase stormPhase = StormPhase.None;
        public double stormStartHour = -1;
        public double stormStrength = 1;
        public int stormsStarted;
        public int stormsSeenParticles;
        public int earlyRecalls;
        public CrewLocation geologist = CrewLocation.Inside;
        public double geologistArrivesAt = -1;
        public bool recallIssued;
        public double recallRelHour = -1;

        // Rover: 0..1 along the current trip, carrying soil or water.
        public double roverTrip;
        public string roverCargo = "";

        public double mealsHarvested;
        public double lowSafetyHours;
        public double minSafety = 100;
        public double minAirHours = double.MaxValue;
        public double minSafetyStorm = 100;
        public int brownouts;
        public int stormBrownouts;
        public int dimBrownouts;
        public bool criticalOut;
        public double criticalOutHours;
        public double unmetCriticalKWh;
        public double criticalServedHours;
        public double shedHours;
        public double criticalLostHours;
        public double shelterHours;
        public double batteryAtOnsetKWh = -1;
        public int batteriesAtOnset;
        public bool greenhouseOffInStorm;
        public double damageOutside;
        public double damageNoShelter;

        public MissionEnd end;
        public FailReason failReason;
        public double endHour = -1;
        public List<FailReason> otherHazards = new List<FailReason>();

        public List<SimEvent> events = new List<SimEvent>();
        public StepReport report = new StepReport();

        public SimState Clone()
        {
            var c = (SimState)MemberwiseClone();
            c.buildings = new List<PlacedBuilding>(buildings.Count);
            foreach (var b in buildings) c.buildings.Add(b.Clone());
            c.events = new List<SimEvent>(events);
            c.otherHazards = new List<FailReason>(otherHazards);
            return c;
        }

        public PlacedBuilding Find(int id)
        {
            foreach (var b in buildings)
                if (b.id == id) return b;
            return null;
        }

        public PlacedBuilding First(BuildingType type)
        {
            foreach (var b in buildings)
                if (b.type == type) return b;
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
        public bool Finished => end != MissionEnd.None;
        public bool Failed => end == MissionEnd.Failed;
        public bool Evaluated => end == MissionEnd.Deadline;
        public double StormRelHour => stormStartHour < 0 ? -1 : timeHours - stormStartHour;
    }
}
