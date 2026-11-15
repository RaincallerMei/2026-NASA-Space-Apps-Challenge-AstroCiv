using Mapmakers.Data;
using Mapmakers.Sim;

namespace Mapmakers.Tests
{
    static class SimTestKit
    {
        // Plain power and life tests: no events, no layout links, no deadline.
        public static Simulator Sim(double sun = 1.0, int width = 20, int height = 15)
        {
            return new Simulator(BalanceLoader.Load(), new TerrainGrid(width, height, sun)) { missionEnabled = false, layoutRules = false };
        }

        // A timed mission without layout rules, for end-condition tests.
        public static Simulator Timed(double hours, params ScheduledEvent[] events)
        {
            var scenario = new Scenario { durationHours = hours, shelterUnlocked = true, maxBatteries = 2 };
            scenario.events.AddRange(events);
            return new Simulator(BalanceLoader.Load(), new TerrainGrid(20, 15, 1), scenario) { layoutRules = false };
        }

        // Layout rules on, events off.
        public static Simulator Layout(TerrainGrid terrain = null)
        {
            return new Simulator(BalanceLoader.Load(), terrain ?? new TerrainGrid(20, 15, 1)) { missionEnabled = false };
        }

        public static PlacedBuilding Put(Simulator sim, SimState s, BuildingType type, int x, int y)
        {
            s.supplies += sim.balance.Def(type).cost;
            s.soil += sim.balance.Def(type).soilCost;
            var result = sim.Place(s, type, x, y);
            NUnit.Framework.Assert.IsTrue(result.Ok, type + " at " + x + "," + y + ": " + result.message);
            return result.building;
        }

        // Habitat 1 kW + Life Support 4 kW + Comms 1 kW + Greenhouse 4 kW = 10 kW, plus `panels` solar arrays and one battery.
        public static SimState FullBase(Simulator sim, int panels)
        {
            var s = sim.NewGame();
            Put(sim, s, BuildingType.Habitat, 0, 0);
            Put(sim, s, BuildingType.LifeSupport, 4, 0);
            Put(sim, s, BuildingType.Communications, 7, 0);
            Put(sim, s, BuildingType.Greenhouse, 9, 0);
            Put(sim, s, BuildingType.Battery, 13, 0);
            for (int i = 0; i < panels; i++) Put(sim, s, BuildingType.SolarArray, i * 3, 5);
            return s;
        }

        public static PlacedBuilding First(SimState s, BuildingType type) => s.First(type);
    }
}
