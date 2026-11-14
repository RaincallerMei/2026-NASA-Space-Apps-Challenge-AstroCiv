using Mapmakers.Data;
using Mapmakers.Sim;

namespace Mapmakers.Tests
{
    static class SimTestKit
    {
        public static Simulator Sim(double sun = 1.0, int width = 20, int height = 15)
        {
            return new Simulator(BalanceLoader.Load(), new TerrainGrid(width, height, sun)) { missionEnabled = false };
        }

        public static PlacedBuilding Put(Simulator sim, SimState s, BuildingType type, int x, int y)
        {
            s.supplies += sim.balance.Def(type).cost;
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

        public static PlacedBuilding First(SimState s, BuildingType type)
        {
            foreach (var b in s.buildings)
                if (b.type == type) return b;
            return null;
        }
    }
}
