using System;
using Mapmakers.Game;
using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    public class GameSessionTests
    {
        static GameSession NewSession() => new GameSession(SimTestKit.Sim());

        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(4, 4)]
        public void FiveRealSeconds_AdvancesAtSelectedSpeed(int speed, double expectedHours)
        {
            var session = NewSession();
            session.SetSpeed(speed);
            session.Tick(5);
            Assert.AreEqual(expectedHours, session.State.timeHours, 1e-9);
        }

        [Test]
        public void Pause_DoesNotCatchUpTimeSpentPaused()
        {
            var session = NewSession();
            session.Tick(0.125); // Half a fixed step, retained across Pause.
            session.SetSpeed(0);
            session.Tick(120);
            Assert.AreEqual(0, session.State.step);
            session.SetSpeed(1);
            session.Tick(0.125);
            Assert.AreEqual(1, session.State.step);
        }

        [Test]
        public void FrameChunks_ProduceIdenticalResourceState()
        {
            var whole = NewSession();
            var chunks = NewSession();
            whole.Tick(5);
            for (int i = 0; i < 100; i++) chunks.Tick(0.05);
            Assert.AreEqual(whole.State.step, chunks.State.step);
            Assert.AreEqual(whole.State.airHours, chunks.State.airHours);
            Assert.AreEqual(whole.State.meals, chunks.State.meals);
            Assert.AreEqual(whole.State.safety, chunks.State.safety);
        }

        [Test]
        public void Reset_RestoresSuppliesResourcesBuildingsAndClearsPartialTime()
        {
            var session = NewSession();
            Assert.IsTrue(session.Simulator.Place(session.State, BuildingType.Habitat, 0, 0).Ok);
            session.Tick(5.125);
            session.Reset();
            var start = session.Simulator.NewGame();
            Assert.AreEqual(0, session.State.buildings.Count);
            Assert.AreEqual(start.supplies, session.State.supplies);
            Assert.AreEqual(start.airHours, session.State.airHours);
            Assert.AreEqual(start.waterL, session.State.waterL);
            Assert.AreEqual(start.meals, session.State.meals);
            Assert.AreEqual(0, session.State.timeHours);
            Assert.AreEqual(MissionStage.Build, session.State.stage);
            Assert.AreEqual(0, session.State.events.Count);
            session.Tick(0.125);
            Assert.AreEqual(0, session.State.step, "Pre-reset partial time must not carry into Retry.");
        }

        [Test]
        public void FailedMission_StopsAndCanBeRetried()
        {
            var session = NewSession();
            session.Tick(500);
            Assert.IsTrue(session.State.failed);
            double stoppedAt = session.State.timeHours;
            session.Tick(30);
            Assert.AreEqual(stoppedAt, session.State.timeHours);
            session.Reset();
            session.Tick(5);
            Assert.IsFalse(session.State.failed);
            Assert.AreEqual(1, session.State.timeHours, 1e-9);
        }

        [Test]
        public void CompletedMission_StopsUntilReset()
        {
            var session = NewSession();
            session.State.stage = MissionStage.Done;
            session.Tick(30);
            Assert.AreEqual(0, session.State.step);
            session.Reset();
            session.Tick(0.25);
            Assert.AreEqual(1, session.State.step);
        }

        [Test]
        public void UnsupportedSpeedAndInvalidElapsedTime_AreRejected()
        {
            var session = NewSession();
            Assert.Throws<ArgumentOutOfRangeException>(() => session.SetSpeed(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Tick(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Tick(double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Tick(double.PositiveInfinity));
        }
    }
}
