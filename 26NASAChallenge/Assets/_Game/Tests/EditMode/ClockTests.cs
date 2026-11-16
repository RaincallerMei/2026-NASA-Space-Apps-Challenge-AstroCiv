using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    // The runtime clock: fixed steps from real time, pause sources, speeds, and no catch-up bursts.
    public class ClockTests
    {
        static MissionClock Clock()
        {
            var c = new MissionClock(0.05, 2, 0.25);
            c.Start();
            return c;
        }

        static int Run(MissionClock c, double frameSeconds, int frames)
        {
            int steps = 0;
            for (int i = 0; i < frames; i++) steps += c.Advance(frameSeconds);
            return steps;
        }

        [Test]
        public void SameRealTime_GivesSameSteps_AtAnyFrameRate()
        {
            Assert.AreEqual(10, Run(Clock(), 1.0 / 60, 60), "1 s at 1x = 0.5 game hours = 10 steps");
            Assert.AreEqual(10, Run(Clock(), 1.0 / 30, 30));
            Assert.AreEqual(10, Run(Clock(), 0.25, 4));
        }

        [Test]
        public void Speeds_ScaleSteps_AndKeepTheCollectedFraction()
        {
            var c = Clock();
            c.SetSpeed(4);
            Assert.AreEqual(40, Run(c, 1.0 / 50, 50));

            c = Clock();
            Assert.AreEqual(0, c.Advance(0.06), "0.03 h is less than one step");
            c.SetSpeed(2);
            Assert.AreEqual(1, c.Advance(0.02), "the 0.03 h already collected is kept: 0.03 + 0.02 = 0.05 h");
            c.SetSpeed(3);
            Assert.AreEqual(2, c.Speed, "only 1x, 2x and 4x exist");
        }

        [Test]
        public void Pause_StopsTime_AndResumeContinues()
        {
            var c = Clock();
            c.Advance(0.06);
            c.SetPaused(true);
            Assert.AreEqual(0, Run(c, 0.1, 50));
            c.SetPaused(false);
            Assert.AreEqual(1, c.Advance(0.04), "the fraction from before the pause is still there");
        }

        [Test]
        public void ALongFrame_IsClamped_NoCatchUpBurst()
        {
            var c = Clock();
            c.SetSpeed(4);
            int steps = c.Advance(30);
            Assert.LessOrEqual(steps, 10, "a 30 s browser-tab pause runs at most 0.25 s of play");
        }

        [Test]
        public void Windows_HoldTheClock_NestedHoldsStayPaused()
        {
            var c = Clock();
            c.Hold();
            c.Hold();
            c.Release();
            Assert.IsTrue(c.Paused, "one window still open");
            c.Release();
            Assert.IsFalse(c.Paused, "closing the last window restores play");
            c.SetPaused(true);
            c.Hold();
            c.Release();
            Assert.IsTrue(c.Paused, "closing a window keeps the player's own pause");
        }

        [Test]
        public void LosingFocus_NeedsAnExplicitResume()
        {
            var c = Clock();
            c.FocusLost();
            Assert.IsTrue(c.Paused);
            c.Hold();
            c.Release();
            Assert.IsTrue(c.Paused);
            c.SetPaused(false);
            Assert.IsFalse(c.Paused);
        }

        [Test]
        public void NotStarted_MeansPaused()
        {
            var c = new MissionClock(0.05, 2, 0.25);
            Assert.AreEqual(0, c.Advance(1));
        }

        // ---------- the run on top of the clock ----------

        [Test]
        public void Run_WaitsForStart_ThenFollowsRealTime()
        {
            var run = MissionRun.ForLevel(LevelCatalog.Load().Level("L2"));
            Assert.AreEqual(RunPhase.Briefing, run.Phase);
            run.Tick(1);
            Assert.AreEqual(0.0, run.State.timeHours, "the briefing is paused");
            run.StartMission();
            run.Tick(1);
            Assert.AreEqual(0.0, run.State.timeHours, "time waits for the player's first Play");
            Assert.IsTrue(run.clock.UserPaused);
            run.clock.SetPaused(false);
            for (int i = 0; i < 4; i++) run.Tick(0.25);
            Assert.AreEqual(0.5, run.State.timeHours, 1e-9);
            run.clock.Hold();
            run.Tick(1);
            Assert.AreEqual(0.5, run.State.timeHours, 1e-9, "an open window pauses the mission");
        }

        [Test]
        public void PausedBuilding_IsAllowed_AndDoesNotMoveTime()
        {
            var run = MissionRun.ForLevel(LevelCatalog.Load().Level("L2"));
            run.StartMission();
            run.Choose(BuildingType.Habitat);
            Assert.IsTrue(run.Place(8, 5).Ok);
            run.Tick(1);
            Assert.AreEqual(0.0, run.State.timeHours);
        }

        // The screen hears about a new building at once, even with time stopped, so its science fact can pop up.
        [Test]
        public void Placing_SendsItsEventToTheScreen_EvenWhilePaused()
        {
            var run = MissionRun.ForLevel(LevelCatalog.Load().Level("L2"));
            run.StartMission();
            run.TakeEvents();
            run.Choose(BuildingType.Habitat);
            Assert.IsTrue(run.Place(8, 5).Ok);
            var events = run.TakeEvents();
            Assert.IsTrue(events.Exists(e => e.code == "placed_Habitat"), "placed_Habitat reaches the screen");
            run.clock.SetPaused(false);
            run.Tick(1);
            Assert.IsFalse(run.TakeEvents().Exists(e => e.code == "placed_Habitat"), "and only once");
        }

        // Pause and plan: after time has run, new buildings still change the rates, power flags and alerts at once.
        [Test]
        public void PausedAfterRunning_RatesFollowNewBuildings()
        {
            var run = MissionRun.ForLevel(LevelCatalog.Load().Level("L2"));
            run.StartMission();
            run.Choose(BuildingType.Habitat);
            Assert.IsTrue(run.Place(8, 5).Ok);
            run.clock.SetPaused(false);
            for (int i = 0; i < 4; i++) run.Tick(0.25);
            run.clock.SetPaused(true);
            var habitat = run.State.First(BuildingType.Habitat);
            double time = run.State.timeHours;
            Assert.Greater(time, 0.0);
            Assert.IsFalse(run.IsPowered(habitat));
            Assert.IsTrue(run.Alerts().Exists(a => a.icon == "power"), "no power before any Solar Array");

            for (int n = 0; n < 6 && !Goals.PowerPositive(run.sim, run.State); n++) PlaceSunniest(run, BuildingType.SolarArray);

            Assert.IsTrue(Goals.PowerPositive(run.sim, run.State));
            Assert.AreEqual(time, run.State.timeHours, "placing while paused never moves time");
            Assert.Greater(run.Power.generationKW, 0.0);
            Assert.AreEqual(0.0, run.State.report.generationKW, "the last real step had no power");
            Assert.GreaterOrEqual(run.Power.generationKW - run.Power.demandKW, -1e-6, "the top bar's net power follows the plan");
            Assert.IsTrue(run.IsPowered(habitat));
            Assert.IsFalse(run.Alerts().Exists(a => a.icon == "power"), "the no-power alert clears while paused");
        }

        static void PlaceSunniest(MissionRun run, BuildingType type)
        {
            int bestX = -1, bestY = -1;
            double best = -1;
            var def = run.sim.balance.Def(type);
            for (int x = 0; x < run.sim.terrain.width; x++)
                for (int y = 0; y < run.sim.terrain.height; y++)
                {
                    if (!run.sim.CanPlace(run.State, type, x, y).Ok) continue;
                    double sun = run.sim.terrain.AverageSun(x, y, def.width, def.height);
                    if (sun > best) { best = sun; bestX = x; bestY = y; }
                }
            Assert.GreaterOrEqual(bestX, 0, "a free spot for " + type);
            run.Choose(type);
            Assert.IsTrue(run.Place(bestX, bestY).Ok);
        }

        [Test]
        public void Run_EndsOnce_AndRefusesChangesAfterwards()
        {
            var run = MissionRun.ForLevel(LevelCatalog.Load().Level("L2"));
            run.StartMission();
            run.clock.SetPaused(false);
            run.clock.SetSpeed(4);
            for (int i = 0; i < 400 && !run.Ended; i++) run.Tick(0.25);
            Assert.IsTrue(run.Ended);
            Assert.AreEqual(FailReason.NoAir, run.Result.failReason, "an idle player runs out of air");
            Assert.IsTrue(run.Result.Failed);
            Assert.IsFalse(run.Result.canContinue);
            var first = run.Result;
            double t = run.State.timeHours;
            run.Tick(1);
            Assert.AreSame(first, run.Result, "one result only");
            Assert.AreEqual(t, run.State.timeHours);
            run.Choose(BuildingType.Habitat);
            Assert.IsFalse(run.Place(8, 5).Ok);
            StringAssert.Contains("air", run.Result.headline.ToLowerInvariant());
            Assert.IsFalse(string.IsNullOrEmpty(run.Result.fix));
        }

        [Test]
        public void DepletionAlert_ShowsHoursLeft_WithAnAction()
        {
            var run = MissionRun.ForLevel(LevelCatalog.Load().Level("L2"));
            run.StartMission();
            run.clock.SetPaused(false);
            run.clock.SetSpeed(4);
            for (int i = 0; i < 400 && run.State.airHours > 10; i++) run.Tick(0.25);
            var alert = run.Alerts().Find(a => a.icon == "air");
            Assert.IsNotNull(alert, "air below the warning level gets an alert");
            StringAssert.Contains("runs out in", alert.text);
            StringAssert.Contains("Air Support", alert.text);
        }
    }
}
