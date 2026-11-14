using System.Text;
using Mapmakers.Data;
using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    // Bots play the real Story Mode levels (Levels.json + Balance.json) one day at a time.
    public class BalanceTests
    {
        static RunResult Play(string levelId, Bot bot)
        {
            var level = LevelCatalog.Load().Level(levelId);
            var sim = LevelSetup.CreateSimulator(level);
            var s = sim.NewGame();
            if (!string.IsNullOrEmpty(level.startBase)) LevelSetup.LoadBase(sim, s, null);
            var result = ScenarioRunner.Run(sim, bot, level.goals, 40, s);
            UnityEngine.Debug.Log(Summary(levelId, result));
            return result;
        }

        static string Summary(string level, RunResult r)
        {
            var s = r.state;
            var sb = new StringBuilder();
            sb.Append("BOT ").Append(level).Append(' ').Append(r.bot).Append(": score ").Append(r.score.total)
              .Append(' ').Append(r.score.letter).Append(", passed ").Append(r.score.passed)
              .Append(", failed ").Append(s.failed).Append(' ').Append(s.failReason)
              .Append(", day ").Append(s.day).Append(", min safety ").Append(SimText.Num(s.minSafety))
              .Append(", brownout days ").Append(s.brownoutDays).Append(", storm brownouts ").Append(s.stormBrownouts)
              .Append(", meals ").Append(SimText.Num(s.meals)).Append(", water ").Append(SimText.Num(s.waterL))
              .Append(", supplies ").Append(s.supplies);
            foreach (var row in r.score.rows) sb.Append("\n  ").Append(row.label).Append(' ').Append(row.points).Append('/').Append(row.max);
            foreach (var line in r.log) sb.Append("\n  ").Append(line);
            return sb.ToString();
        }

        [Test]
        public void L2_ShedLoads_Passes_AndIdle_FailsGently()
        {
            var shed = Play("L2", new ShedLoadsBot());
            Assert.IsTrue(shed.score.passed);
            Assert.GreaterOrEqual(shed.score.total, 61, "a careful player earns A or S");

            var idle = Play("L2", new IdleBot());
            Assert.IsTrue(idle.state.failed);
            Assert.Greater(idle.state.timeHours, 20, "an idle player has a long time before anything fails");
        }

        [Test]
        public void L2_ShadowUnlocksASecondBattery()
        {
            var r = Play("L2", new ShedLoadsBot());
            Assert.IsTrue(r.state.shadowPassed);
            Assert.AreEqual(2, r.state.maxBatteries);
        }

        [Test]
        public void L3_TwoStrategiesWin_GreedyScoresLower_IgnoringTheStormFails()
        {
            var shed = Play("L3", new ShedLoadsBot());
            var stored = Play("L3", new StoredEnergyBot());
            var greedy = Play("L3", new GreedyBot());
            var none = Play("L3", new DoNothingBot());

            Assert.IsTrue(shed.score.passed);
            Assert.IsTrue(stored.score.passed);
            Assert.AreEqual("S", shed.score.letter);
            Assert.AreEqual("S", stored.score.letter);
            Assert.Greater(greedy.state.stormBrownouts, 0, "one battery and every load on runs out in the storm");
            Assert.Less(greedy.score.total, shed.score.total);
            Assert.Less(greedy.score.total, stored.score.total);

            Assert.IsTrue(none.state.failed);
            Assert.AreNotEqual(FailReason.None, none.state.failReason);
            Assert.AreEqual("C", none.score.letter);
        }

        [Test]
        public void L4_Mars_CanBeWon_TwoWays()
        {
            Assert.IsTrue(Play("L4", new ShedLoadsBot()).score.passed);
            Assert.IsTrue(Play("L4", new StoredEnergyBot()).score.passed);
        }

        [Test]
        public void FreeBuild_IsWinnable_OnManySeeds()
        {
            foreach (var difficulty in new[] { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard })
            {
                for (int seed = 1; seed <= 12; seed++)
                {
                    var sim = LevelSetup.FreeBuild(seed % 2 == 0 ? "Mars" : "Moon", seed, difficulty, 10);
                    var r = ScenarioRunner.Run(sim, new SmartBot(), new[] { "core_base", "all_days" });
                    UnityEngine.Debug.Log("FREE " + difficulty + " seed " + seed + ": " + r.score.total + " " + r.score.letter
                        + ", meals " + SimText.Num(r.state.meals) + ", brownout days " + r.state.brownoutDays + ", fail " + r.state.failReason);
                    Assert.IsTrue(r.score.passed, difficulty + " seed " + seed + " failed: " + r.state.failReason);
                }
            }
        }
    }
}
