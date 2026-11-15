using System;
using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    // Tutorial 2 must teach every action a real mission asks for, and a player who follows it can always finish.
    public class TrainingTests
    {
        static MissionRun NewRun() => MissionRun.ForLevel(LevelCatalog.Load().Level("T2"));

        [Test]
        public void Steps_CoverEveryMissionAction_WithValidSpeakersAndTargets()
        {
            var steps = HelpData.Load().training;
            Assert.Greater(steps.Length, 20);
            var built = new HashSet<string>();
            var kinds = new HashSet<string>();
            var speakers = new HashSet<string>();
            var ids = new HashSet<string>();
            foreach (var step in steps)
            {
                Assert.IsTrue(ids.Add(step.id), "unique id " + step.id);
                Assert.IsNotNull(StoryData.Load().Crew(step.speaker), step.id + " speaker");
                speakers.Add(step.speaker);
                Assert.IsFalse(string.IsNullOrEmpty(step.goal), step.id);
                Assert.LessOrEqual(StoryData.WordCount(step.text), 20, step.id + ": " + step.text);
                kinds.Add(step.done.Split(':')[0]);
                if (step.target.StartsWith("card:"))
                {
                    string type = step.target.Substring(5);
                    Assert.DoesNotThrow(() => Enum.Parse(typeof(BuildingType), type), step.id);
                    built.Add(type);
                }
            }
            foreach (var type in MissionRun.AllTypes) Assert.IsTrue(built.Contains(type.ToString()), "teaches " + type);
            foreach (var kind in new[] { "chat", "power", "speed", "halt", "harvest", "select", "off", "on", "shadow_over", "paused", "recall", "playing", "storm_over", "wiki", "next" })
                Assert.IsTrue(kinds.Contains(kind), "teaches " + kind);
            Assert.AreEqual(3, speakers.Count, "all three crew members guide");
        }

        // Play, Pause and speed come before the first building; the rules that end a mission come before Yuki.
        [Test]
        public void TimeControlsAndEndingRules_AreTaughtEarly()
        {
            var steps = new List<string>();
            foreach (var step in HelpData.Load().training) steps.Add(step.id);
            Assert.Less(steps.IndexOf("play"), steps.IndexOf("habitat"));
            Assert.Less(steps.IndexOf("pause"), steps.IndexOf("habitat"));
            Assert.Less(steps.IndexOf("fast"), steps.IndexOf("habitat"));
            Assert.Less(steps.IndexOf("lifeline"), steps.IndexOf("y_hello"));
            Assert.Less(steps.IndexOf("outage"), steps.IndexOf("y_hello"));
            var run = NewRun();
            string outage = Training.Fill(HelpData.Load().training[steps.IndexOf("outage")].text, run);
            StringAssert.Contains(SimText.Duration(run.sim.balance.powerLossToleranceHours), outage);
            StringAssert.DoesNotContain("{", outage);
        }

        [Test]
        public void Training_StartsWithTimeStopped_AndSaysSoWhileWaiting()
        {
            var run = NewRun();
            run.StartMission();
            Assert.IsTrue(run.clock.UserPaused, "time waits for the player");
            run.TrainingAction("chat:engineer");
            Assert.AreEqual("play", run.Training.Current.id);
            run.clock.SetPaused(false);
            run.UpdateTraining();
            Assert.AreEqual("pause", run.Training.Current.id);
            run.clock.SetPaused(true);
            run.UpdateTraining();
            Assert.AreEqual("fast", run.Training.Current.id);
        }

        [Test]
        public void Buildings_UnlockOnlyWhenTheCrewIntroducesThem()
        {
            var run = NewRun();
            run.StartMission();
            Assert.IsFalse(run.CanBuild(BuildingType.Greenhouse, out string why));
            Assert.AreEqual("Your crew will show you this soon.", why);
            Assert.IsFalse(run.CanBuild(BuildingType.Habitat, out _), "first say hi to Priya");
            run.TrainingAction("chat:engineer");
            Assert.IsFalse(run.CanBuild(BuildingType.Habitat, out _), "first Play, Pause and speed");
            run.clock.SetPaused(false);
            run.UpdateTraining();
            run.clock.SetPaused(true);
            run.clock.SetSpeed(2);
            run.UpdateTraining();
            Assert.AreEqual("habitat", run.Training.Current.id);
            Assert.IsTrue(run.CanBuild(BuildingType.Habitat, out _));
        }

        [Test]
        public void Untimed_AndCannotFail()
        {
            var run = NewRun();
            Assert.IsFalse(run.sim.scenario.Timed);
            Assert.IsFalse(run.sim.canFail);
            run.StartMission();
            run.clock.SetPaused(false);
            run.clock.SetSpeed(4);
            for (int i = 0; i < 120; i++) run.Tick(0.25);
            Assert.AreEqual(0, run.State.airHours, 1e-6, "no Air Support: the air runs out");
            Assert.IsFalse(run.State.Finished, "but the practice run never stops");
        }

        // A player who does exactly what each step says reaches the end, with every event seen once.
        [Test]
        public void FollowingTheCrew_FinishesTheTraining()
        {
            var run = NewRun();
            run.StartMission();
            run.clock.SetSpeed(4);
            for (int i = 0; i < 20000 && !run.Training.Done; i++)
            {
                Act(run, run.Training.Current);
                run.UpdateTraining();
                run.Tick(0.25);
                run.UpdateTraining();
            }
            Assert.IsTrue(run.Training.Done, "stuck at " + run.Training.Current?.id);
            var s = run.State;
            Assert.IsFalse(run.Ended);
            Assert.IsTrue(s.harvestDone);
            Assert.IsTrue(s.shadowPassed);
            Assert.AreEqual(1, s.stormsStarted);
            Assert.AreEqual(1, s.earlyRecalls, "Yuki was called in before the particles");
            Assert.Greater(s.shelterHours, 0);
            Assert.AreEqual(2, s.Count(BuildingType.Battery));
            Assert.IsTrue(Simulator.HasCoreBase(s));
            Assert.Less(s.timeHours, 120, "about " + s.timeHours + " game hours");
        }

        static void Act(MissionRun run, TrainingStep step)
        {
            var parts = step.done.Split(':');
            var s = run.State;
            var greenhouse = s.First(BuildingType.Greenhouse);
            switch (parts[0])
            {
                case "next": run.TrainingAction("next:" + step.id); return;
                case "chat":
                case "wiki": run.TrainingAction(step.done); return;
                case "paused":
                case "halt": run.clock.SetPaused(true); return;
                case "playing": run.clock.SetPaused(false); return;
                case "recall": run.Recall(); return;
                case "select": run.ClickTile(greenhouse.x, greenhouse.y); return;
                case "off":
                case "on":
                    var b = s.First((BuildingType)Enum.Parse(typeof(BuildingType), parts[1]));
                    if (b != null && b.enabled == (parts[0] == "off")) run.Toggle(b.id);
                    return;
                case "built": Build(run, (BuildingType)Enum.Parse(typeof(BuildingType), parts[1])); return;
                case "power": Build(run, BuildingType.SolarArray); return;
            }
        }

        static void Build(MissionRun run, BuildingType type)
        {
            if (!run.CanBuild(type, out _)) return;
            var spots = Layout.Suggest(run.sim, run.State, type);
            run.Choose(type);
            foreach (var (x, y) in spots)
                if (run.Place(x, y).Ok) return;
            var t = run.sim.terrain;
            for (int y = 0; y < t.height; y++)
                for (int x = 0; x < t.width; x++)
                    if (run.Place(x, y).Ok) return;
            Assert.Fail("no spot for " + type);
        }
    }
}
