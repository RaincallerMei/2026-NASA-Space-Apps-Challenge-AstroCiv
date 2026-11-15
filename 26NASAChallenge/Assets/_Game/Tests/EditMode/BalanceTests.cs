using System.Text;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    // Bots play the real Story Mode levels (Levels.json + Balance.json) in real time, acting every 2 game hours.
    public class BalanceTests
    {
        static RunResult Play(string levelId, Bot bot)
        {
            var level = LevelCatalog.Load().Level(levelId);
            var sim = LevelSetup.CreateSimulator(level);
            var s = sim.NewGame();
            if (!string.IsNullOrEmpty(level.startBase)) LevelSetup.LoadBase(sim, s, null);
            var result = ScenarioRunner.Run(sim, bot, level.goals, s);
            UnityEngine.Debug.Log(Summary(levelId, result));
            return result;
        }

        static string Summary(string level, RunResult r)
        {
            var s = r.state;
            var sb = new StringBuilder();
            sb.Append("BOT ").Append(level).Append(' ').Append(r.bot).Append(": score ").Append(r.score.total)
              .Append(' ').Append(r.score.letter).Append(", end ").Append(s.end).Append(' ').Append(s.failReason)
              .Append(" at ").Append(SimText.Num(s.timeHours)).Append(" h, min safety ").Append(SimText.Num(s.minSafety))
              .Append(", brownouts ").Append(s.brownouts).Append(", storm brownouts ").Append(s.stormBrownouts)
              .Append(", meals ").Append(SimText.Num(s.meals)).Append(", water ").Append(SimText.Num(s.waterL))
              .Append(", supplies ").Append(s.supplies);
            foreach (var row in r.score.rows) sb.Append("\n  ").Append(row.label).Append(' ').Append(row.points).Append('/').Append(row.max);
            if (r.score.capNote.Length > 0) sb.Append("\n  ").Append(r.score.capNote);
            return sb.ToString();
        }

        static void Evaluated(RunResult r) => Assert.AreEqual(MissionEnd.Deadline, r.state.end, r.bot + " ended with " + r.state.failReason);

        [Test]
        public void L2_StrategicLayoutEarnsS_CarelessLayoutEarnsA_IdleRunsOutOfAir()
        {
            var smart = Play("L2", new SmartBot());
            Evaluated(smart);
            Assert.AreEqual("S", smart.score.letter);

            var careless = Play("L2", new CarelessBot());
            Evaluated(careless);
            Assert.That(careless.score.total, Is.InRange(61, 80), "placing buildings anywhere survives but earns A at best");
            Assert.Greater(smart.score.total - careless.score.total, 10);

            var idle = Play("L2", new IdleBot());
            Assert.AreEqual(FailReason.NoAir, idle.state.failReason);
            Assert.Greater(idle.state.timeHours, 30, "an idle player has over a minute at 1x before the air runs out");
        }

        [Test]
        public void L2_AnIncompleteBase_IsEvaluated_NotFailed_AndCannotContinue()
        {
            var r = Play("L2", new NoRadioBot());
            Evaluated(r);
            Assert.LessOrEqual(r.score.total, BalanceLoader.Load().grade.missingCoreCap);
            Assert.IsFalse(Simulator.HasCoreBase(r.state));
        }

        [Test]
        public void L3_ShedLoadsAndStoredEnergyBothWin_IgnoringPowerWarningsIsNoS_IgnoringTheStormFails()
        {
            var shed = Play("L3", new ShedLoadsBot());
            var stored = Play("L3", new StoredEnergyBot());
            var greedy = Play("L3", new GreedyBot());
            var none = Play("L3", new DoNothingBot());

            Evaluated(shed);
            Evaluated(stored);
            Assert.AreEqual("S", shed.score.letter);
            Assert.AreEqual("S", stored.score.letter);

            Evaluated(greedy);
            Assert.Greater(greedy.state.stormBrownouts, 0, "one battery and every load on runs short in the storm");
            Assert.LessOrEqual(greedy.score.total, 80, "ignoring the power warnings cannot earn S");

            Assert.IsTrue(none.state.Failed);
            Assert.AreEqual(FailReason.RadiationNoShelter, none.state.failReason);
            Assert.AreEqual("C", none.score.letter);
        }

        [Test]
        public void L4_Mars_CarefulPlayWins_GreedyIsNoS_IdleFails()
        {
            var smart = Play("L4", new SmartBot());
            var shed = Play("L4", new ShedLoadsBot());
            var greedy = Play("L4", new GreedyBot());
            Evaluated(smart);
            Evaluated(shed);
            Assert.AreEqual("S", smart.score.letter);
            Assert.AreEqual("S", shed.score.letter);
            Evaluated(greedy);
            Assert.LessOrEqual(greedy.score.total, 80);
            Assert.IsTrue(Play("L4", new IdleBot()).state.Failed);
        }

        [Test]
        public void FreeBuild_EasyAndNormalAlwaysWinnable_HardMostly()
        {
            int hardWins = 0;
            foreach (var difficulty in new[] { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard })
            {
                for (int seed = 1; seed <= 12; seed++)
                {
                    var sim = LevelSetup.FreeBuild(seed % 2 == 0 ? "Mars" : "Moon", seed, difficulty, new FreeSettings().hours);
                    var r = ScenarioRunner.Run(sim, new SmartBot(), FreeSettings.Goals);
                    UnityEngine.Debug.Log("FREE " + difficulty + " seed " + seed + ": " + r.score.total + " " + r.score.letter
                        + ", end " + r.state.end + " " + r.state.failReason);
                    if (difficulty == Difficulty.Hard) { if (!r.state.Failed) hardWins++; continue; }
                    Assert.IsFalse(r.state.Failed, difficulty + " seed " + seed + " failed: " + r.state.failReason);
                }
            }
            Assert.GreaterOrEqual(hardWins, 8, "Hard is hard, but a careful player usually survives");
        }

        // Builds everything except the radio: survives, but the base is incomplete.
        sealed class NoRadioBot : Bot
        {
            public override string Name => "NoRadio";
            public override void Act(Simulator sim, SimState s)
            {
                foreach (var type in new[] { BuildingType.Habitat, BuildingType.SolarArray, BuildingType.Battery, BuildingType.LifeSupport,
                             BuildingType.SolarArray, BuildingType.RoverGarage, BuildingType.SolarArray, BuildingType.Greenhouse, BuildingType.SolarArray })
                    if (s.Count(type) < Placement.MaxCount(sim, s, type) && (type != BuildingType.SolarArray || s.Count(type) < 4))
                        Planner.Place(sim, s, type, true);
            }
        }
    }
}
