using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    // How a timed run ends: resource failures, power and radiation safety stops, and the deadline evaluation.
    public class EndConditionTests
    {
        static SimState RunToEnd(Simulator sim, SimState s, double maxHours = 400)
        {
            while (!s.Finished && s.timeHours < maxHours) s = sim.Step(s);
            return s;
        }

        [Test]
        public void NoAir_EndsWhenTheReserveIsEmpty_EvenWithoutAHabitat()
        {
            var sim = SimTestKit.Timed(200);
            var s = RunToEnd(sim, sim.NewGame());
            Assert.AreEqual(MissionEnd.Failed, s.end);
            Assert.AreEqual(FailReason.NoAir, s.failReason, "the crew uses air even before a Habitat is placed");
            Assert.AreEqual(0.0, s.airHours, 0.0, "clamped at zero");
            Assert.AreEqual(sim.balance.startAirHours, s.endHour, sim.balance.dtHours + 1e-9, "ends on the step the air runs out");
        }

        [Test]
        public void NoFood_EndsWhenMealsRunOut()
        {
            var sim = SimTestKit.Timed(200);
            var s = SimTestKit.FullBase(sim, 4);
            sim.SetEnabled(s, s.First(BuildingType.Greenhouse).id, false);
            s = RunToEnd(sim, s);
            Assert.AreEqual(FailReason.NoFood, s.failReason);
            Assert.AreEqual(0.0, s.meals, 0.0);
            Assert.AreEqual(sim.balance.startMeals / sim.balance.CrewMealsPerHour, s.endHour, 0.1);
        }

        [Test]
        public void NoWater_EndsWhenUsableWaterRunsOut()
        {
            var sim = SimTestKit.Timed(200);
            var s = SimTestKit.FullBase(sim, 4);
            s.waterL = 4;
            s = RunToEnd(sim, s);
            Assert.AreEqual(FailReason.NoWater, s.failReason);
            Assert.AreEqual(0.0, s.waterL, 0.0);
        }

        [Test]
        public void CriticalPowerLoss_EndsAfterTheTolerance_NotBefore()
        {
            var sim = SimTestKit.Timed(200);
            var s = sim.NewGame();
            SimTestKit.Put(sim, s, BuildingType.Habitat, 0, 0);
            SimTestKit.Put(sim, s, BuildingType.LifeSupport, 4, 0);
            s = sim.Advance(s, sim.balance.powerLossToleranceHours - 1);
            Assert.IsFalse(s.Finished, "a short outage is recoverable");
            s = RunToEnd(sim, s);
            Assert.AreEqual(FailReason.PowerLoss, s.failReason);
            Assert.AreEqual(sim.balance.powerLossToleranceHours, s.endHour, sim.balance.dtHours * 2);
            Assert.Greater(s.airHours, 0.0, "power failure is its own reason, not air");
        }

        [Test]
        public void BriefOutage_Recovers_WhenPowerComesBack()
        {
            var sim = SimTestKit.Timed(200);
            var s = sim.NewGame();
            SimTestKit.Put(sim, s, BuildingType.Habitat, 0, 0);
            SimTestKit.Put(sim, s, BuildingType.LifeSupport, 4, 0);
            s = sim.Advance(s, 5);
            Assert.Greater(s.criticalOutHours, 4.0);
            SimTestKit.Put(sim, s, BuildingType.SolarArray, 0, 5);
            SimTestKit.Put(sim, s, BuildingType.SolarArray, 3, 5);
            s = sim.Advance(s, 20);
            Assert.IsFalse(s.Finished);
            Assert.AreEqual(0.0, s.criticalOutHours, 0.0);
            Assert.Greater(s.airHours, 30.0, "air refills once Life Support has power");
        }

        [Test]
        public void EmptyBattery_WithEnoughSunlight_DoesNotFail()
        {
            var sim = SimTestKit.Timed(200);
            var s = SimTestKit.FullBase(sim, 4);
            s.batteryKWh = 0;
            s = sim.Advance(s, 30);
            Assert.IsFalse(s.Finished);
            Assert.AreEqual(0.0, s.criticalLostHours, 0.0);
        }

        [Test]
        public void Deadline_StopsExactlyOnTime_AndSkipsLaterEvents()
        {
            var sim = SimTestKit.Timed(10,
                ScheduledEvent.Make(5, EventKind.SupplyDrop, 1, 10),
                ScheduledEvent.Make(10, EventKind.SupplyDrop, 1, 99),
                ScheduledEvent.Make(12, EventKind.SupplyDrop, 1, 99));
            var s = SimTestKit.FullBase(sim, 4);
            int supplies = s.supplies;
            s = RunToEnd(sim, s);
            Assert.AreEqual(MissionEnd.Deadline, s.end);
            Assert.AreEqual(FailReason.None, s.failReason);
            Assert.AreEqual(10.0, s.timeHours, 1e-9);
            int harvest = s.harvestDone ? sim.balance.harvestSupplyReward : 0;
            Assert.AreEqual(supplies + 10 + harvest, s.supplies, "only the event before the deadline happens");
            var after = sim.Step(s);
            Assert.AreEqual(s.timeHours, after.timeHours, "no step past the deadline");
        }

        [Test]
        public void FailureOnTheLastStep_BeatsTheDeadline()
        {
            var sim = SimTestKit.Timed(2);
            var s = sim.NewGame();
            s.airHours = 2;
            s = RunToEnd(sim, s);
            Assert.AreEqual(2.0, s.timeHours, 1e-9);
            Assert.AreEqual(MissionEnd.Failed, s.end);
            Assert.AreEqual(FailReason.NoAir, s.failReason);
        }

        [Test]
        public void SimultaneousCauses_AreResolvedInFixedOrder_AndTheOthersRecorded()
        {
            var sim = SimTestKit.Timed(50);
            var s = sim.NewGame();
            s.airHours = 0.01;
            s.waterL = 0.001;
            s = sim.Step(s);
            Assert.AreEqual(FailReason.NoAir, s.failReason);
            Assert.Contains(FailReason.NoWater, s.otherHazards);
        }

        [Test]
        public void AfterTheEnd_CommandsAreRefused()
        {
            var sim = SimTestKit.Timed(200);
            var s = SimTestKit.FullBase(sim, 0);
            s.airHours = 0.01;
            s = sim.Step(s);
            Assert.IsTrue(s.Failed);
            int count = s.buildings.Count, supplies = s.supplies;
            s.supplies += 100;
            Assert.IsFalse(sim.Place(s, BuildingType.SolarArray, 15, 10).Ok);
            Assert.IsFalse(sim.Remove(s, s.First(BuildingType.Greenhouse).id));
            Assert.IsFalse(sim.SetEnabled(s, s.First(BuildingType.Greenhouse).id, false));
            Assert.AreEqual(count, s.buildings.Count);
            Assert.IsTrue(s.First(BuildingType.Greenhouse).enabled);
        }

        [Test]
        public void IncompleteBase_IsEvaluated_AndCappedAtB()
        {
            var sim = SimTestKit.Timed(10);
            var s = sim.NewGame();
            SimTestKit.Put(sim, s, BuildingType.Habitat, 0, 0);
            SimTestKit.Put(sim, s, BuildingType.LifeSupport, 4, 0);
            SimTestKit.Put(sim, s, BuildingType.SolarArray, 0, 5);
            SimTestKit.Put(sim, s, BuildingType.SolarArray, 3, 5);
            s = RunToEnd(sim, s);
            Assert.AreEqual(MissionEnd.Deadline, s.end, "running out of time is an evaluation, not a failure");
            var score = Grader.Mission(sim, s, new[] { "core_base", Goals.SafeToEnd });
            Assert.LessOrEqual(score.total, sim.balance.grade.missingCoreCap);
            StringAssert.Contains("capped", score.capNote);
            Assert.Greater(score.total, sim.balance.grade.failCap, "a safe base gets more than a stopped mission");
        }

        [Test]
        public void MissingGoal_PreventsS_FullBaseCanReachIt()
        {
            var sim = SimTestKit.Timed(10);
            var s = RunToEnd(sim, SimTestKit.FullBase(sim, 4));
            var withGoal = Grader.Mission(sim, s, new[] { "core_base", "harvest_never", Goals.SafeToEnd });
            Assert.LessOrEqual(withGoal.total, sim.balance.grade.missingGoalCap);
            var full = Grader.Mission(sim, s, new[] { "core_base", Goals.SafeToEnd });
            Assert.AreEqual("", full.capNote);
            Assert.AreEqual(full.rawTotal, full.total);
        }

        [Test]
        public void NoSteadyPowerPoints_WithoutCriticalSystems()
        {
            var sim = SimTestKit.Timed(10);
            var s = sim.NewGame();
            SimTestKit.Put(sim, s, BuildingType.SolarArray, 0, 5);
            s = sim.Advance(s, 5);
            var score = Grader.Mission(sim, s, new string[0]);
            Assert.AreEqual(0, score.rows.Find(r => r.label == "Steady power").points);
        }

        [Test]
        public void Grade_RowsAddTo100()
        {
            var sim = SimTestKit.Timed(10);
            var score = Grader.Mission(sim, SimTestKit.FullBase(sim, 4), new string[0]);
            int max = 0;
            foreach (var row in score.rows) max += row.max;
            Assert.AreEqual(100, max);
        }
    }
}
