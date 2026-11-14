using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using NUnit.Framework;
using UnityEngine;

namespace Mapmakers.Tests
{
    // Day turns, scheduled events, grades, random worlds and level data.
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
        public void Grade_MissionRowsAddTo100_AndAFailIsCappedAtC()
        {
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 4);
            s.completed = true;
            s.day = 6;
            var perfect = Grader.Mission(sim.balance, s, 3, 3);
            int max = 0;
            foreach (var row in perfect.rows) max += row.max;
            Assert.AreEqual(100, max);
            Assert.IsTrue(perfect.passed);

            s.failed = true;
            var failed = Grader.Mission(sim.balance, s, 3, 3);
            Assert.LessOrEqual(failed.total, Grader.FailCap);
            Assert.AreEqual("C", failed.letter);
            Assert.IsFalse(failed.passed);
        }

        [Test]
        public void NextDay_RunsExactly24Hours_AndReportsTheDay()
        {
            var sim = new Simulator(BalanceLoader.Load(), new TerrainGrid(20, 15, 1), new Scenario { days = 5 });
            var s = SimTestKit.FullBase(sim, 4);
            s = sim.AdvanceDay(s);
            Assert.AreEqual(2, s.day);
            Assert.AreEqual(24.0, s.timeHours, 1e-6);
            Assert.AreEqual(1, s.lastDay.day);
            Assert.Greater(s.lastDay.batteryEnd, s.lastDay.batteryStart, "spare sunlight charges the battery");
        }

        [Test]
        public void ScenarioEnds_AfterTheLastDay()
        {
            var sim = new Simulator(BalanceLoader.Load(), new TerrainGrid(20, 15, 1), new Scenario { days = 2 });
            var s = SimTestKit.FullBase(sim, 4);
            s = sim.AdvanceDay(s);
            Assert.IsFalse(s.completed);
            s = sim.AdvanceDay(s);
            Assert.IsTrue(s.completed);
            double t = s.timeHours;
            s = sim.AdvanceDay(s);
            Assert.AreEqual(t, s.timeHours, "a finished run does not advance");
        }

        [Test]
        public void StormEvent_WarnsTheDayBefore_ParticlesStartAtDawn()
        {
            var scenario = new Scenario { days = 5, shelterUnlocked = true };
            scenario.events.Add(ScheduledEvent.Make(3, EventKind.SolarStorm));
            var sim = new Simulator(BalanceLoader.Load(), new TerrainGrid(20, 15, 1), scenario);
            var s = SimTestKit.FullBase(sim, 4);
            s = sim.AdvanceDay(s);
            Assert.AreEqual(StormPhase.Warning, s.stormPhase, "day 2 starts with the warning");
            Assert.AreEqual(CrewLocation.Outside, s.geologist);
            s = sim.AdvanceDay(s);
            Assert.AreEqual(StormPhase.Particles, s.stormPhase, "day 3 starts with particles");
        }

        [Test]
        public void ShadowEvent_DimsOneDay_ThenUnlocksASecondBattery()
        {
            var scenario = new Scenario { days = 5 };
            scenario.events.Add(ScheduledEvent.Make(2, EventKind.Shadow));
            var sim = new Simulator(BalanceLoader.Load(), new TerrainGrid(20, 15, 1), scenario);
            var s = SimTestKit.FullBase(sim, 4);
            s = sim.AdvanceDay(s);
            Assert.AreEqual(sim.balance.shadow.factor, s.shadowFactor, 1e-9);
            Assert.AreEqual(1, s.maxBatteries);
            s = sim.AdvanceDay(s);
            Assert.AreEqual(1.0, s.shadowFactor, 1e-9);
            Assert.AreEqual(sim.balance.unlockedMaxBatteries, s.maxBatteries);
        }

        [Test]
        public void Fault_BreaksAPanel_RepairCostsSupplies()
        {
            var scenario = new Scenario { days = 5 };
            scenario.events.Add(ScheduledEvent.Make(2, EventKind.Fault, 1, 0));
            var sim = new Simulator(BalanceLoader.Load(), new TerrainGrid(20, 15, 1), scenario);
            var s = SimTestKit.FullBase(sim, 4);
            double before = sim.Preview(s).generationKW;
            s = sim.AdvanceDay(s);
            var broken = s.buildings.Find(b => b.broken);
            Assert.IsNotNull(broken);
            Assert.Less(sim.Preview(s).generationKW, before);

            int supplies = s.supplies;
            Assert.IsTrue(sim.Repair(s, broken.id));
            Assert.AreEqual(supplies - sim.balance.repairCost, s.supplies);
            Assert.AreEqual(before, sim.Preview(s).generationKW, 1e-6);
        }

        [Test]
        public void RandomWorlds_AreRepeatableBySeed_AndDifferAcrossSeeds()
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
                    differ |= a.Sun(x, y) != c.Sun(x, y) || a.IsBlocked(x, y) != c.IsBlocked(x, y);
                }
            Assert.IsTrue(differ);

            for (int seed = 1; seed <= 50; seed++)
                Assert.GreaterOrEqual(TerrainGenerator.FreeTiles(TerrainGenerator.Generate(seed)), TerrainGenerator.MinFreeTiles, "seed " + seed);

            Assert.AreEqual(Describe(ScenarioFactory.Random(7, 10, Difficulty.Hard)), Describe(ScenarioFactory.Random(7, 10, Difficulty.Hard)));
            Assert.AreNotEqual(Describe(ScenarioFactory.Random(7, 10, Difficulty.Hard)), Describe(ScenarioFactory.Random(8, 10, Difficulty.Hard)));
        }

        [Test]
        public void RandomEvents_StormsAndDimDays_NeverOverlap()
        {
            for (int seed = 1; seed <= 200; seed++)
            {
                var scenario = ScenarioFactory.Random(seed, 10, Difficulty.Hard);
                var busy = new HashSet<int>();
                foreach (var e in scenario.events)
                {
                    if (e.Kind == EventKind.SolarStorm)
                        for (int d = e.day - 1; d <= e.day + 1; d++) Assert.IsTrue(busy.Add(d), "seed " + seed + " day " + d);
                }
                foreach (var e in scenario.events)
                    if (e.Kind == EventKind.Shadow) Assert.IsTrue(busy.Add(e.day), "seed " + seed + " dim day " + e.day);
                Assert.GreaterOrEqual(scenario.Count(EventKind.SolarStorm), 1, "seed " + seed);
            }
        }

        static string Describe(Scenario s)
        {
            var parts = new List<string>();
            foreach (var e in s.events) parts.Add(e.kind + "@" + e.day + ":" + e.strength + ":" + e.amount);
            return string.Join(",", parts);
        }

        [Test]
        public void Levels_HaveFourStages_ValidCutscenes_AndGoalLabels()
        {
            var catalog = LevelCatalog.Load();
            var story = StoryData.Load();
            Assert.AreEqual(4, catalog.levels.Length);
            Assert.IsTrue(catalog.levels[0].IsDrawing, "level 1 is the tutorial");
            Assert.AreEqual("Mars", catalog.levels[3].planet);

            foreach (var level in catalog.levels)
            {
                foreach (var id in new[] { level.before, level.after })
                    if (!string.IsNullOrEmpty(id)) Assert.IsNotNull(catalog.Cutscene(id), level.id + " cutscene " + id);
                foreach (var goal in level.goals) Assert.AreNotEqual(goal, catalog.GoalLabel(goal), level.id + " goal " + goal);
                foreach (var e in level.events) Assert.DoesNotThrow(() => { var k = e.Kind; }, level.id + " event " + e.kind);
            }
            foreach (var cutscene in catalog.cutscenes)
                foreach (var slide in cutscene.slides)
                {
                    Assert.IsNotNull(Resources.Load<Texture2D>("Cutscenes/" + slide.image), cutscene.id + " image " + slide.image);
                    foreach (var line in slide.lines)
                        Assert.IsFalse(story.Story(line).StartsWith("["), cutscene.id + " line " + line);
                }
        }

        [Test]
        public void MarsLevel_UsesSols_AndAMadeUpMap()
        {
            var level = LevelCatalog.Load().Level("L4");
            var sim = LevelSetup.CreateSimulator(level);
            Assert.AreEqual("Sol", sim.planet.dayWord);
            var lola = TerrainFactory.Load(sim.balance);
            bool same = true;
            for (int y = 0; y < lola.height && same; y++)
                for (int x = 0; x < lola.width && same; x++)
                    same = lola.Sun(x, y) == sim.terrain.Sun(x, y);
            Assert.IsFalse(same, "Mars must not reuse the real Moon map");
        }

        [Test]
        public void CrewChat_RemembersTheStormChoice()
        {
            var run = MissionRun.ForLevel(LevelCatalog.Load().Level("L3"));
            run.NextDay();
            Assert.AreEqual("storm_outside", CrewChat.When("geologist", run));
            Assert.IsTrue(run.Recall());
            Assert.AreEqual("storm_inside", CrewChat.When("geologist", run));
            PlaceAndPowerShelter(run);
            for (int i = 0; i < 3; i++) run.NextDay();
            Assert.AreEqual("after_called_first", CrewChat.When("geologist", run));
            Assert.IsNotNull(CrewChat.Entry(run.story, "geologist", run));
        }

        public static void PlaceAndPowerShelter(MissionRun run)
        {
            run.Choose(BuildingType.Shelter);
            for (int y = 0; y < run.sim.terrain.height && run.State.Count(BuildingType.Shelter) == 0; y++)
                for (int x = 0; x < run.sim.terrain.width && run.State.Count(BuildingType.Shelter) == 0; x++)
                    if (run.sim.CanPlace(run.State, BuildingType.Shelter, x, y).Ok) run.Place(x, y);
            var shelter = run.State.buildings.Find(b => b.type == BuildingType.Shelter);
            Assert.IsNotNull(shelter, "no room for a Shelter");
            if (!shelter.enabled) run.Toggle(shelter.id);
        }
    }
}
