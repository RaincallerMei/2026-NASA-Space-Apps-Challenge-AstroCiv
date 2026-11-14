using System.Text;
using Mapmakers.Data;
using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    // Bots play the full mission on the real LOLA terrain with the shipped Balance.json.
    public class BalanceTests
    {
        static RunResult Play(Bot bot)
        {
            var balance = BalanceLoader.Load();
            var sim = new Simulator(balance, TerrainFactory.Load(balance));
            var result = ScenarioRunner.Run(sim, bot);
            UnityEngine.Debug.Log(Summary(result));
            return result;
        }

        static string Summary(RunResult r)
        {
            var o = r.outcome;
            var s = r.state;
            var sb = new StringBuilder();
            sb.Append("BOT ").Append(r.bot).Append(": ").Append(o.kind).Append(", stars ").Append(o.stars)
              .Append(", fail ").Append(o.failReason).Append(", hours ").Append(SimText.Num(r.hours))
              .Append(", stage ").Append(s.stage).Append(", min storm safety ").Append(SimText.Num(o.minSafety))
              .Append(", storm brownouts ").Append(o.stormBrownouts).Append(", all brownouts ").Append(s.brownouts)
              .Append(", batteries at onset ").Append(s.batteriesAtOnset)
              .Append(", battery at onset ").Append(SimText.Num(s.batteryAtOnsetKWh)).Append(" kWh")
              .Append(", supplies ").Append(s.supplies).Append(", meals ").Append(SimText.Num(s.meals))
              .Append(", water ").Append(SimText.Num(s.waterL)).Append(" L");
            foreach (var line in r.log) sb.Append("\n  ").Append(line);
            return sb.ToString();
        }

        [Test]
        public void DoNothing_FailsWithAReason()
        {
            var r = Play(new DoNothingBot());
            Assert.AreEqual(OutcomeKind.Fail, r.outcome.kind);
            Assert.AreNotEqual(FailReason.None, r.outcome.failReason);
        }

        [Test]
        public void ShedLoads_Wins()
        {
            var r = Play(new ShedLoadsBot());
            Assert.AreEqual(OutcomeKind.Win, r.outcome.kind);
            Assert.AreEqual(MissionStage.Done, r.state.stage);
        }

        [Test]
        public void StoredEnergy_Wins()
        {
            var r = Play(new StoredEnergyBot());
            Assert.AreEqual(OutcomeKind.Win, r.outcome.kind);
            Assert.AreEqual(MissionStage.Done, r.state.stage);
        }

        [Test]
        public void Greedy_IsAtBestACloseCall()
        {
            var r = Play(new GreedyBot());
            Assert.AreNotEqual(OutcomeKind.Win, r.outcome.kind);
        }

        [Test]
        public void Idle_FailsGently()
        {
            var r = Play(new IdleBot());
            Assert.AreEqual(OutcomeKind.Fail, r.outcome.kind);
            Assert.Greater(r.hours, 20, "an idle player must have a long time before anything fails");
        }

        [Test]
        public void MissionReachesTheStorm_InReasonableGameTime()
        {
            var r = Play(new ShedLoadsBot());
            Assert.Greater(r.state.stormStartHour, 0);
            Assert.Less(r.state.stormStartHour, 40, "storm game hour");
        }
    }
}
