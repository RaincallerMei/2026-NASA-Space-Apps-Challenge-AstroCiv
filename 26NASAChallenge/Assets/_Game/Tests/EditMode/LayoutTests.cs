using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    // Placement strategy: base area, links between buildings, soil and the rover.
    public class LayoutTests
    {
        [Test]
        public void Gap_CountsEmptyTilesBetweenFootprints()
        {
            Assert.AreEqual(0, Layout.Gap(0, 0, 3, 3, 3, 0, 2, 2), "touching");
            Assert.AreEqual(2, Layout.Gap(0, 0, 3, 3, 5, 0, 2, 2));
            Assert.AreEqual(4, Layout.Gap(0, 0, 1, 1, 2, 5, 1, 1), "the larger axis counts");
        }

        [Test]
        public void HabitatFirst_ThenOnlyInsideTheBaseArea()
        {
            var sim = SimTestKit.Layout();
            var s = sim.NewGame();
            s.supplies = 500;
            Assert.AreEqual(PlacementReason.HabitatFirst, sim.CanPlace(s, BuildingType.SolarArray, 0, 0).reason);
            Assert.IsTrue(sim.Place(s, BuildingType.Habitat, 0, 0).Ok);
            Assert.IsTrue(sim.CanPlace(s, BuildingType.SolarArray, 3 + sim.balance.layout.baseRadius, 0).Ok);
            Assert.AreEqual(PlacementReason.OutsideBase, sim.CanPlace(s, BuildingType.SolarArray, 4 + sim.balance.layout.baseRadius, 0).reason);
        }

        [Test]
        public void SolarWithoutANearbyBattery_LosesPower()
        {
            var sim = SimTestKit.Layout();
            var s = sim.NewGame();
            SimTestKit.Put(sim, s, BuildingType.Habitat, 0, 0);
            var panel = SimTestKit.Put(sim, s, BuildingType.SolarArray, 6, 0);
            double far = sim.SolarOutput(s, panel);
            Assert.AreEqual(3 * (1 - sim.balance.layout.longCableLoss), far, 1e-9);
            SimTestKit.Put(sim, s, BuildingType.Battery, 8, 0);
            Assert.AreEqual(3.0, sim.SolarOutput(s, panel), 1e-9, "a Battery next to it fixes the cable");
        }

        [Test]
        public void LifeSupport_FarFromTheHabitat_MakesLessAir()
        {
            var sim = SimTestKit.Layout();
            var near = sim.NewGame();
            SimTestKit.Put(sim, near, BuildingType.Habitat, 0, 0);
            SimTestKit.Put(sim, near, BuildingType.LifeSupport, 3, 0);
            for (int i = 0; i < 3; i++) SimTestKit.Put(sim, near, BuildingType.SolarArray, i * 2, 4);
            SimTestKit.Put(sim, near, BuildingType.Battery, 6, 4);
            near.airHours = 10;

            var far = sim.NewGame();
            SimTestKit.Put(sim, far, BuildingType.Habitat, 0, 0);
            SimTestKit.Put(sim, far, BuildingType.LifeSupport, 6, 0);
            for (int i = 0; i < 3; i++) SimTestKit.Put(sim, far, BuildingType.SolarArray, i * 2, 4);
            SimTestKit.Put(sim, far, BuildingType.Battery, 6, 4);
            far.airHours = 10;

            near = sim.Advance(near, 2);
            far = sim.Advance(far, 2);
            Assert.AreEqual(10 + 2 * sim.balance.airRefillPerHour, near.airHours, 1e-6);
            Assert.AreEqual(10 + 2 * sim.balance.airRefillPerHour * sim.balance.layout.longDuctAirFactor, far.airHours, 1e-6);
        }

        [Test]
        public void GreenhouseNextToLifeSupport_SavesPower()
        {
            var sim = SimTestKit.Layout();
            var s = sim.NewGame();
            SimTestKit.Put(sim, s, BuildingType.Habitat, 0, 0);
            var ls = SimTestKit.Put(sim, s, BuildingType.LifeSupport, 3, 0);
            Assert.AreEqual(4.0, sim.DemandOf(s, ls), 1e-9);
            SimTestKit.Put(sim, s, BuildingType.Greenhouse, 5, 0);
            Assert.AreEqual(4.0 - sim.balance.layout.plantLoopSavesKW, sim.DemandOf(s, ls), 1e-9);
            sim.SetEnabled(s, s.First(BuildingType.Greenhouse).id, false);
            Assert.AreEqual(4.0, sim.DemandOf(s, ls), 1e-9, "only a working Greenhouse helps");
        }

        static TerrainGrid SoilAndIce()
        {
            var t = new TerrainGrid(20, 15, 1);
            for (int y = 0; y < 3; y++) t.SetSoil(10, y, true);
            for (int y = 0; y < 3; y++) { t.SetWater(19, y, true); t.SetBlocked(19, y, true); }
            return t;
        }

        [Test]
        public void Greenhouse_NeedsSoil_ThatTheRoverBrings()
        {
            var sim = SimTestKit.Layout(SoilAndIce());
            var s = sim.NewGame();
            s.supplies = 500;
            sim.Place(s, BuildingType.Habitat, 4, 0);
            Assert.AreEqual(PlacementReason.NeedSoil, sim.CanPlace(s, BuildingType.Greenhouse, 4, 4).reason);
            for (int i = 0; i < 3; i++) sim.Place(s, BuildingType.SolarArray, 0, i * 2 + 4);
            sim.Place(s, BuildingType.Battery, 2, 4);
            Assert.IsTrue(sim.Place(s, BuildingType.RoverGarage, 8, 0).Ok);
            s = sim.Advance(s, 12);
            Assert.GreaterOrEqual(s.soil, sim.balance.Def(BuildingType.Greenhouse).soilCost);
            double soil = s.soil;
            Assert.IsTrue(sim.Place(s, BuildingType.Greenhouse, 4, 4).Ok);
            Assert.AreEqual(soil - sim.balance.Def(BuildingType.Greenhouse).soilCost, s.soil, 1e-9, "building it uses the soil");
        }

        [Test]
        public void Rover_FetchesWaterOnceSoilIsEnough_AndShortTripsAreFaster()
        {
            var sim = SimTestKit.Layout(SoilAndIce());
            var near = sim.NewGame();
            near.supplies = 500;
            sim.Place(near, BuildingType.Habitat, 13, 0);
            for (int i = 0; i < 2; i++) sim.Place(near, BuildingType.SolarArray, 13, 3 + i * 2);
            sim.Place(near, BuildingType.Battery, 15, 3);
            var garage = sim.Place(near, BuildingType.RoverGarage, 17, 0).building;
            Assert.AreEqual(sim.balance.layout.roverBaseTripHours, sim.TripHours(garage, true), 1e-9, "touching the ice");
            Assert.Greater(sim.TripHours(garage, false), sim.TripHours(garage, true));

            near.soil = 20;
            double water = near.waterL;
            near = sim.Advance(near, 10);
            Assert.AreEqual("water", near.roverCargo);
            Assert.Greater(near.waterL + 10 * sim.balance.CrewWaterPerHour, water + 10, "the rover brings water");
        }

        [Test]
        public void LayoutScore_RewardsLinks()
        {
            var sim = SimTestKit.Layout();
            var good = sim.NewGame();
            SimTestKit.Put(sim, good, BuildingType.Habitat, 0, 0);
            SimTestKit.Put(sim, good, BuildingType.LifeSupport, 3, 0);
            SimTestKit.Put(sim, good, BuildingType.Greenhouse, 5, 0);
            SimTestKit.Put(sim, good, BuildingType.SolarArray, 0, 3);
            SimTestKit.Put(sim, good, BuildingType.Battery, 2, 3);

            var poor = sim.NewGame();
            SimTestKit.Put(sim, poor, BuildingType.Habitat, 0, 0);
            SimTestKit.Put(sim, poor, BuildingType.LifeSupport, 6, 6);
            SimTestKit.Put(sim, poor, BuildingType.Greenhouse, 3, 0);
            SimTestKit.Put(sim, poor, BuildingType.SolarArray, 0, 6);
            SimTestKit.Put(sim, poor, BuildingType.Battery, 8, 0);

            Assert.AreEqual(1.0, Layout.Score(sim.balance, sim.terrain, good, out _, out _), 1e-9);
            Assert.Less(Layout.Score(sim.balance, sim.terrain, poor, out _, out _), 0.5);
            Assert.Greater(Grader.LayoutQuality(sim, good), Grader.LayoutQuality(sim, poor));
        }
    }
}
