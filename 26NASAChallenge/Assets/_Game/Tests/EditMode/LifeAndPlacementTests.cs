using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    public class LifeAndPlacementTests
    {
        [Test]
        public void Greenhouse_PoweredAndWatered_MakesFood_AndRecyclesMostWater()
        {
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 4);
            double meals = s.meals, water = s.waterL;
            s = sim.Advance(s, 10);

            double eaten = sim.balance.CrewMealsPerHour * 10;
            Assert.AreEqual(meals + 0.5 * 10 - eaten, s.meals, 1e-6);
            Assert.AreEqual(5.0, s.mealsHarvested, 1e-6);

            var def = sim.balance.Def(BuildingType.Greenhouse);
            Assert.GreaterOrEqual(def.waterReturnLph / def.waterInLph, 0.8, "most of the water is recycled");
            double plants = (def.waterInLph - def.waterReturnLph) * 10, crew = sim.balance.CrewWaterPerHour * 10;
            Assert.AreEqual(water - plants - crew, s.waterL, 1e-6, "plants and crew are counted once each");
        }

        [Test]
        public void Greenhouse_Unpowered_MakesNoFood_AndOnlyTheCrewDrinks()
        {
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 4);
            sim.SetEnabled(s, SimTestKit.First(s, BuildingType.Greenhouse).id, false);
            double water = s.waterL;
            s = sim.Advance(s, 10);

            Assert.AreEqual(0.0, s.mealsHarvested, 1e-9);
            Assert.AreEqual(water - sim.balance.CrewWaterPerHour * 10, s.waterL, 1e-6);
            Assert.Less(s.report.foodPerHour, 0.0);
        }

        [Test]
        public void Greenhouse_WithoutWater_MakesNoFood()
        {
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 4);
            s.waterL = 0;
            s = sim.Step(s);
            Assert.AreEqual(0.0, s.mealsHarvested, 1e-9);
            Assert.IsTrue(s.report.warnings.Exists(w => w.code == "water_empty"));
        }

        [Test]
        public void Air_RefillsWithLifeSupport_AndDrainsOnePerHourWithout()
        {
            var sim = SimTestKit.Sim();
            var none = sim.NewGame();
            SimTestKit.Put(sim, none, BuildingType.Habitat, 0, 0);
            none = sim.Advance(none, 5);
            Assert.AreEqual(sim.balance.startAirHours - 5, none.airHours, 1e-6);

            var s = SimTestKit.FullBase(sim, 4);
            s.airHours = 10;
            s = sim.Advance(s, 2);
            Assert.Greater(s.airHours, 10.0);
            s = sim.Advance(s, 20);
            Assert.AreEqual(sim.balance.airCapHours, s.airHours, 1e-6);
        }

        [Test]
        public void Placement_ChecksFootprintOverlapCostAndTerrain()
        {
            var terrain = new TerrainGrid(20, 15);
            terrain.SetBlocked(10, 10, true);
            var sim = new Simulator(Data.BalanceLoader.Load(), terrain);
            var s = sim.NewGame();

            Assert.AreEqual(PlacementReason.OutOfBounds, sim.CanPlace(s, BuildingType.Habitat, 18, 0).reason, "3x3 footprint leaves the map");
            Assert.AreEqual(PlacementReason.OutOfBounds, sim.CanPlace(s, BuildingType.Habitat, -1, 0).reason);
            Assert.AreEqual(PlacementReason.BlockedTerrain, sim.CanPlace(s, BuildingType.Habitat, 8, 8).reason, "footprint touches a blocked tile");

            Assert.IsTrue(sim.Place(s, BuildingType.Habitat, 0, 0).Ok);
            Assert.AreEqual(sim.balance.startSupplies - 30, s.supplies);
            Assert.AreEqual(PlacementReason.Overlap, sim.CanPlace(s, BuildingType.SolarArray, 2, 2).reason);
            Assert.IsTrue(sim.CanPlace(s, BuildingType.SolarArray, 3, 0).Ok);

            Assert.AreEqual(PlacementReason.MaxCount, sim.CanPlace(s, BuildingType.Habitat, 5, 5).reason);
            Assert.AreEqual(PlacementReason.Locked, sim.CanPlace(s, BuildingType.Shelter, 5, 5).reason);

            s.supplies = 5;
            Assert.AreEqual(PlacementReason.TooExpensive, sim.CanPlace(s, BuildingType.SolarArray, 3, 0).reason);
            Assert.IsFalse(sim.Place(s, BuildingType.SolarArray, 3, 0).Ok);
            Assert.AreEqual(1, s.buildings.Count);
            Assert.AreEqual(5, s.supplies);
        }

        [Test]
        public void Placement_LimitsSolarToFive_AndBatteriesUntilUnlocked()
        {
            var sim = SimTestKit.Sim();
            var s = sim.NewGame();
            s.supplies = 500;
            for (int i = 0; i < 5; i++) Assert.IsTrue(sim.Place(s, BuildingType.SolarArray, i * 2, 0).Ok);
            Assert.AreEqual(PlacementReason.MaxCount, sim.CanPlace(s, BuildingType.SolarArray, 12, 0).reason);

            Assert.IsTrue(sim.Place(s, BuildingType.Battery, 0, 4).Ok);
            Assert.AreEqual(PlacementReason.MaxCount, sim.CanPlace(s, BuildingType.Battery, 2, 4).reason);
            s.maxBatteries = 2;
            Assert.IsTrue(sim.CanPlace(s, BuildingType.Battery, 2, 4).Ok);
        }

        [Test]
        public void Remove_ReturnsTheFullCost()
        {
            var sim = SimTestKit.Sim();
            var s = sim.NewGame();
            var b = sim.Place(s, BuildingType.Greenhouse, 0, 0).building;
            Assert.IsTrue(sim.Remove(s, b.id));
            Assert.AreEqual(sim.balance.startSupplies, s.supplies);
            Assert.IsTrue(sim.CanPlace(s, BuildingType.Greenhouse, 0, 0).Ok);
        }

        [Test]
        public void StartingSupplies_CannotBuyTheWholeBase_LandersArriveLater()
        {
            var b = Data.BalanceLoader.Load();
            int full = b.Def(BuildingType.Habitat).cost + b.Def(BuildingType.SolarArray).cost * b.Def(BuildingType.SolarArray).maxCount
                + b.Def(BuildingType.Battery).cost + b.Def(BuildingType.LifeSupport).cost + b.Def(BuildingType.Communications).cost
                + b.Def(BuildingType.Greenhouse).cost + b.Def(BuildingType.RoverGarage).cost;
            Assert.Greater(full, b.startSupplies, "the base cannot all be built in the first minute");
            int first = b.Def(BuildingType.Habitat).cost + b.Def(BuildingType.LifeSupport).cost + b.Def(BuildingType.SolarArray).cost * 2
                + b.Def(BuildingType.Battery).cost;
            Assert.LessOrEqual(first, b.startSupplies, "air and power can always be started right away");

            var level = Data.LevelCatalog.Load().Level("L2");
            int landers = 0;
            foreach (var e in level.events) if (e.Kind == EventKind.SupplyDrop) landers += e.amount;
            Assert.GreaterOrEqual(b.startSupplies + landers, full, "landers bring enough for the full base");
        }

        [Test]
        public void SolarOutput_FollowsTileSunFraction()
        {
            var terrain = new TerrainGrid(20, 15, 1.0);
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++) terrain.SetSun(x, y, 0.5);
            var sim = new Simulator(Data.BalanceLoader.Load(), terrain) { layoutRules = false };
            var s = sim.NewGame();
            sim.Place(s, BuildingType.SolarArray, 0, 0);
            s = sim.Step(s);
            Assert.AreEqual(1.5, s.report.generationKW, 1e-9);

            sim.Place(s, BuildingType.SolarArray, 4, 4);
            s = sim.Step(s);
            Assert.AreEqual(4.5, s.report.generationKW, 1e-9);
        }
    }
}
