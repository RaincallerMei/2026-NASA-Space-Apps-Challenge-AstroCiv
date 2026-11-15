using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using NUnit.Framework;
using UnityEngine;

namespace Mapmakers.Tests
{
    // Scheduled events in game hours, grades, random worlds, level data, Mars and crew chat.
    public class WorldTests
    {
        [Test]
        public void Grade_LettersAndStars_FollowTheAgreedBands()
        {
            var cases = new Dictionary<int, string> { { 100, "S" }, { 81, "S" }, { 80, "A" }, { 61, "A" }, { 60, "B" }, { 31, "B" }, { 30, "C" }, { 0, "C" } };
            foreach (var c in cases) Assert.AreEqual(c.Value, Grader.Letter(c.Key), "score " + c.Key);
            Assert.AreEqual(3, Grader.Stars("S"));
            Assert.AreEqual(2, Grader.Stars("A"));
            Assert.AreEqual(1, Grader.Stars("B"));
            Assert.AreEqual(0, Grader.Stars("C"));
        }

        [Test]
        public void StormEvent_WarnsAtItsHour_ParticlesFollowLater()
        {
            var sim = SimTestKit.Timed(80, ScheduledEvent.Make(10, EventKind.SolarStorm));
            var s = SimTestKit.FullBase(sim, 4);
            s = sim.Advance(s, 10.05);
            Assert.AreEqual(StormPhase.Warning, s.stormPhase);
            Assert.AreEqual(CrewLocation.Outside, s.geologist);
            s = sim.Advance(s, sim.balance.storm.onsetHour);
            Assert.AreEqual(StormPhase.Particles, s.stormPhase);
        }

        [Test]
        public void ShadowEvent_IsWarnedFirst_DimsForItsDuration_ThenUnlocksASecondBattery()
        {
            var scenario = new Scenario { durationHours = 80, maxBatteries = 1 };
            scenario.events.Add(ScheduledEvent.Make(20, EventKind.Shadow));
            var sim = new Simulator(BalanceLoader.Load(), new TerrainGrid(20, 15, 1), scenario) { layoutRules = false };
            var s = SimTestKit.FullBase(sim, 4);
            s = sim.Advance(s, 20 - sim.balance.shadow.warningHours + 0.1);
            Assert.IsTrue(s.events.Exists(e => e.code == "shadow_warning"));
            Assert.AreEqual(1.0, s.shadowFactor);
            s = sim.Advance(s, sim.balance.shadow.warningHours);
            Assert.AreEqual(sim.balance.shadow.factor, s.shadowFactor, 1e-9);
            Assert.AreEqual(1, s.maxBatteries);
            s = sim.Advance(s, sim.balance.shadow.durationHours);
            Assert.AreEqual(1.0, s.shadowFactor, 1e-9);
            Assert.AreEqual(sim.balance.unlockedMaxBatteries, s.maxBatteries);
        }

        [Test]
        public void DimEvent_CanSetItsOwnStrengthAndLength()
        {
            var e = ScheduledEvent.Make(1, EventKind.Shadow, 0.6);
            e.durationHours = 30;
            var sim = SimTestKit.Timed(80, e);
            var s = sim.Advance(SimTestKit.FullBase(sim, 4), 2);
            Assert.AreEqual(0.6, s.shadowFactor, 1e-9);
            s = sim.Advance(s, 28);
            Assert.AreEqual(0.6, s.shadowFactor, 1e-9);
            s = sim.Advance(s, 2);
            Assert.AreEqual(1.0, s.shadowFactor, 1e-9);
        }

        [Test]
        public void Fault_BreaksAPanel_RepairCostsSupplies()
        {
            var sim = SimTestKit.Timed(80, ScheduledEvent.Make(3, EventKind.Fault, 1, 0));
            var s = SimTestKit.FullBase(sim, 4);
            double before = sim.Preview(s).generationKW;
            s = sim.Advance(s, 4);
            var broken = s.buildings.Find(b => b.broken);
            Assert.IsNotNull(broken);
            Assert.Less(sim.Preview(s).generationKW, before);

            int supplies = s.supplies;
            Assert.IsTrue(sim.Repair(s, broken.id));
            Assert.AreEqual(supplies - sim.balance.repairCost, s.supplies);
            Assert.AreEqual(before, sim.Preview(s).generationKW, 1e-6);
        }

        [Test]
        public void Preview_NeverChangesStateOrFiresEvents()
        {
            var sim = SimTestKit.Timed(10, ScheduledEvent.Make(0, EventKind.SupplyDrop, 1, 50));
            var s = sim.NewGame();
            s.airHours = 0.01;
            var report = sim.Preview(s);
            Assert.IsNotNull(report);
            Assert.AreEqual(0.0, s.timeHours);
            Assert.AreEqual(sim.balance.startSupplies, s.supplies);
            Assert.IsFalse(s.Finished);
            Assert.AreEqual(0, s.events.Count);
        }

        [Test]
        public void RandomWorlds_AreRepeatableBySeed_AndHaveSoilAndWater()
        {
            var a = TerrainGenerator.Generate(42);
            var b = TerrainGenerator.Generate(42);
            var c = TerrainGenerator.Generate(43);
            bool differ = false;
            for (int y = 0; y < a.height; y++)
                for (int x = 0; x < a.width; x++)
                {
                    Assert.AreEqual(a.Sun(x, y), b.Sun(x, y));
                    Assert.AreEqual(a.IsBlocked(x, y), b.IsBlocked(x, y));
                    Assert.AreEqual(a.IsSoil(x, y), b.IsSoil(x, y));
                    differ |= a.Sun(x, y) != c.Sun(x, y) || a.IsBlocked(x, y) != c.IsBlocked(x, y);
                }
            Assert.IsTrue(differ);

            for (int seed = 1; seed <= 50; seed++)
            {
                var t = TerrainGenerator.Generate(seed);
                Assert.GreaterOrEqual(TerrainGenerator.FreeTiles(t), TerrainGenerator.MinFreeTiles, "seed " + seed);
                Assert.IsTrue(t.AnySoil() && t.AnyWater(), "seed " + seed);
            }

            var balance = BalanceLoader.Load();
            Assert.AreEqual(Describe(ScenarioFactory.Random(7, 144, Difficulty.Hard, balance)), Describe(ScenarioFactory.Random(7, 144, Difficulty.Hard, balance)));
            Assert.AreNotEqual(Describe(ScenarioFactory.Random(7, 144, Difficulty.Hard, balance)), Describe(ScenarioFactory.Random(8, 144, Difficulty.Hard, balance)));
        }

        [Test]
        public void RandomEvents_NeverOverlap_AndStormsEndBeforeTheEvaluation()
        {
            var balance = BalanceLoader.Load();
            for (int seed = 1; seed <= 200; seed++)
            {
                var scenario = ScenarioFactory.Random(seed, 144, Difficulty.Hard, balance);
                var busy = new List<(double from, double to)>();
                foreach (var e in scenario.events)
                {
                    double length = e.Kind == EventKind.SolarStorm ? balance.storm.endHour : e.Kind == EventKind.Shadow ? balance.shadow.durationHours : 0;
                    if (length <= 0) continue;
                    foreach (var (from, to) in busy) Assert.IsFalse(e.hour < to && e.hour + length > from, "seed " + seed + " overlap at " + e.hour);
                    busy.Add((e.hour, e.hour + length));
                    if (e.Kind == EventKind.SolarStorm) Assert.Less(e.hour + length, 144 - ScenarioFactory.RecoveryHours + 1e-9, "seed " + seed);
                }
                Assert.GreaterOrEqual(scenario.Count(EventKind.SolarStorm), 1, "seed " + seed);
            }
        }

        static string Describe(Scenario s)
        {
            var parts = new List<string>();
            foreach (var e in s.events) parts.Add(e.kind + "@" + e.hour + ":" + e.strength + ":" + e.amount);
            return string.Join(",", parts);
        }

        [Test]
        public void Levels_HaveFourStages_TimedMissions_ValidCutscenes_AndGoalLabels()
        {
            var catalog = LevelCatalog.Load();
            var story = StoryData.Load();
            Assert.AreEqual(4, catalog.levels.Length);
            Assert.IsTrue(catalog.levels[0].IsDrawing, "level 1 is the untimed drawing");
            Assert.AreEqual(0, catalog.levels[0].durationHours);
            Assert.AreEqual("Mars", catalog.levels[3].planet);

            double minutes = 0;
            foreach (var level in catalog.levels)
            {
                foreach (var id in new[] { level.before, level.after })
                    if (!string.IsNullOrEmpty(id)) Assert.IsNotNull(catalog.Cutscene(id), level.id + " cutscene " + id);
                foreach (var goal in level.goals) Assert.AreNotEqual(goal, catalog.GoalLabel(goal), level.id + " goal " + goal);
                foreach (var e in level.events)
                {
                    Assert.DoesNotThrow(() => { var k = e.Kind; }, level.id + " event " + e.kind);
                    Assert.Less(e.hour, level.durationHours, level.id + " event after the evaluation");
                }
                if (!level.IsDrawing) Assert.Greater(level.durationHours, 0, level.id);
                minutes += level.durationHours * BalanceLoader.Load().realSecondsPerGameHour / 60;
            }
            Assert.That(minutes, Is.InRange(8, 13), "timed play at 1x leaves room for the tutorial and story in 12-16 minutes");
            foreach (var cutscene in catalog.cutscenes)
                foreach (var slide in cutscene.slides)
                {
                    Assert.IsNotNull(Resources.Load<Texture2D>("Cutscenes/" + slide.image), cutscene.id + " image " + slide.image);
                    foreach (var line in slide.lines)
                        Assert.IsFalse(story.Story(line).StartsWith("["), cutscene.id + " line " + line);
                }
        }

        [Test]
        public void StormLevel_StormAndRecoveryHappenBeforeTheEvaluation()
        {
            var level = LevelCatalog.Load().Level("L3");
            var balance = BalanceLoader.Load();
            foreach (var e in level.events)
                if (e.Kind == EventKind.SolarStorm) Assert.Less(e.hour + balance.storm.endHour, level.durationHours - 8);
        }

        [Test]
        public void MarsLevel_UsesJezeroHeights_Sols_AndLessSunlight()
        {
            var level = LevelCatalog.Load().Level("L4");
            var sim = LevelSetup.CreateSimulator(level);
            Assert.AreEqual("Sol", sim.planet.dayWord);
            Assert.AreEqual(24.6597, sim.planet.dayHours, 1e-4, "NASA Mars Fact Sheet sol length");
            Assert.Less(sim.planet.solarScale, 1.0, "Mars panels make less power");

            var jezero = TerrainFactory.LoadMars(sim.balance);
            int blocked = 0, soil = 0, water = 0;
            for (int y = 0; y < jezero.height; y++)
                for (int x = 0; x < jezero.width; x++)
                {
                    Assert.AreEqual(jezero.Sun(x, y), sim.terrain.Sun(x, y));
                    if (jezero.IsBlocked(x, y)) blocked++;
                    if (jezero.IsSoil(x, y)) soil++;
                    if (jezero.IsWater(x, y)) water++;
                }
            Assert.Greater(blocked, 10, "the crater rim is steep");
            Assert.Greater(soil, 10, "the old lake floor has mud");
            Assert.Greater(water, 3, "the delta has wet clay");
        }

        [Test]
        public void MoonLevel_IceSitsInTheDarkCraters()
        {
            var terrain = TerrainFactory.Load(BalanceLoader.Load());
            var grid = FixtureLoader.Illumination();
            for (int row = 0; row < grid.height; row++)
                for (int x = 0; x < grid.width; x++)
                    Assert.AreEqual(grid.percent[row, x] < BalanceLoader.Load().terrain.blockedBelowPercent, terrain.IsWater(x, grid.height - 1 - row));
            Assert.IsTrue(terrain.AnySoil());
        }

        [Test]
        public void CrewChat_RemembersTheStormChoice()
        {
            var run = MissionRun.ForLevel(LevelCatalog.Load().Level("L3"));
            run.StartMission();
            run.clock.SetSpeed(4);
            for (int i = 0; i < 200 && run.State.stormsStarted == 0; i++) run.Tick(0.25);
            Assert.AreEqual("storm_outside", CrewChat.When("geologist", run));
            Assert.IsTrue(run.Recall());
            Assert.AreEqual("storm_inside", CrewChat.When("geologist", run));
            PlaceAndPowerShelter(run);
            for (int i = 0; i < 400 && run.State.StormActive; i++) run.Tick(0.25);
            Assert.AreEqual("after_called_first", CrewChat.When("geologist", run));
            Assert.IsNotNull(CrewChat.Entry(run.story, "geologist", run));
        }

        public static void PlaceAndPowerShelter(MissionRun run)
        {
            var shelter = run.State.First(BuildingType.Shelter);
            if (shelter == null)
            {
                run.Choose(BuildingType.Shelter);
                for (int y = 0; y < run.sim.terrain.height && run.State.Count(BuildingType.Shelter) == 0; y++)
                    for (int x = 0; x < run.sim.terrain.width && run.State.Count(BuildingType.Shelter) == 0; x++)
                        if (run.sim.CanPlace(run.State, BuildingType.Shelter, x, y).Ok) run.Place(x, y);
                shelter = run.State.First(BuildingType.Shelter);
            }
            Assert.IsNotNull(shelter, "no room for a Shelter");
            if (!shelter.enabled) run.Toggle(shelter.id);
        }
    }
}
