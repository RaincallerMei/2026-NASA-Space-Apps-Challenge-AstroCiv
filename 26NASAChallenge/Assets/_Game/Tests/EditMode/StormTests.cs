using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    public class StormTests
    {
        static SimState BaseInStorm(Simulator sim, int panels = 4)
        {
            var s = SimTestKit.FullBase(sim, panels);
            s.batteryKWh = 4;
            s = sim.Advance(s, 1);
            sim.StartStorm(s);
            return s;
        }

        [Test]
        public void StormAlert_DoesNotReduceSolarGeneration()
        {
            var sim = SimTestKit.Sim(0.9);
            var s = SimTestKit.FullBase(sim, 4);
            s = sim.Step(s);
            double before = s.report.generationKW;
            Assert.Greater(before, 0.0);

            sim.StartStorm(s);
            sim.RecallGeologist(s);
            s.shelterUnlocked = true;
            sim.SetEnabled(s, SimTestKit.Put(sim, s, BuildingType.Shelter, 15, 0).id, true);
            var t = sim.balance.storm;
            for (double h = 0; h < t.endHour + 1; h += sim.balance.dtHours)
            {
                s = sim.Step(s);
                Assert.AreEqual(before, s.report.generationKW, 1e-9, "generation changed in storm phase " + s.stormPhase);
            }
            Assert.AreEqual(StormPhase.Over, s.stormPhase);
        }

        [Test]
        public void Shadow_ReducesSolar_AndIsSeparateFromTheStorm()
        {
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 4);
            s = sim.Step(s);
            Assert.AreEqual(12.0, s.report.generationKW, 1e-9);

            sim.StartShadow(s);
            s = sim.Step(s);
            Assert.AreEqual(12.0 * sim.balance.shadow.factor, s.report.generationKW, 1e-9);
            Assert.AreEqual(StormPhase.None, s.stormPhase);

            s = sim.Advance(s, sim.balance.shadow.durationHours + 0.5);
            Assert.AreEqual(12.0, s.report.generationKW, 1e-9);
        }

        [Test]
        public void Sunrise_RaisesSolarOutput()
        {
            var sim = SimTestKit.Sim(0.8);
            var s = SimTestKit.FullBase(sim, 4);
            s = sim.Step(s);
            Assert.AreEqual(9.6, s.report.generationKW, 1e-9);
            s.sunrise = 1;
            s = sim.Step(s);
            Assert.AreEqual(12.0, s.report.generationKW, 1e-9);
        }

        [Test]
        public void CrewOutsideDuringParticles_LosesSafety_RecallBeforeOnsetPreventsIt()
        {
            var sim = SimTestKit.Sim();
            var t = sim.balance.storm;

            var ignored = BaseInStorm(sim);
            ignored = sim.Advance(ignored, t.onsetHour + 1);
            Assert.Less(ignored.safety, 100.0);
            Assert.Greater(ignored.damageOutside, 0.0);

            var recalled = BaseInStorm(sim);
            Assert.IsTrue(sim.RecallGeologist(recalled));
            recalled = sim.Advance(recalled, t.onsetHour + 1);
            Assert.AreEqual(CrewLocation.Inside, recalled.geologist);
            Assert.AreEqual(0.0, recalled.damageOutside, 1e-9);
        }

        [Test]
        public void Recall_NeedsAPoweredRadio()
        {
            var sim = SimTestKit.Sim();
            var s = BaseInStorm(sim);
            sim.SetEnabled(s, SimTestKit.First(s, BuildingType.Communications).id, false);
            Assert.IsFalse(sim.CanRecall(s, out string why));
            StringAssert.Contains("radio", why.ToLowerInvariant());
            Assert.IsFalse(sim.RecallGeologist(s));

            sim.SetEnabled(s, SimTestKit.First(s, BuildingType.Communications).id, true);
            s = sim.Step(s);
            Assert.IsTrue(sim.RecallGeologist(s));
        }

        [Test]
        public void PoweredShelter_ProtectsTheCrew()
        {
            var sim = SimTestKit.Sim();
            var s = BaseInStorm(sim);
            s.shelterUnlocked = true;
            sim.RecallGeologist(s);
            var shelter = SimTestKit.Put(sim, s, BuildingType.Shelter, 15, 0);
            sim.SetEnabled(s, shelter.id, true);
            sim.SetEnabled(s, SimTestKit.First(s, BuildingType.Greenhouse).id, false);
            s = sim.Advance(s, sim.balance.storm.endHour + 0.5);

            Assert.AreEqual(100.0, s.minSafetyStorm, 1e-6);
            Assert.IsFalse(s.failed);
            Assert.AreEqual(OutcomeKind.Win, Outcome.Evaluate(sim.balance, s).kind);
        }

        [Test]
        public void IgnoringTheStorm_FailsGently_WithAReason()
        {
            var sim = SimTestKit.Sim();
            var s = BaseInStorm(sim);
            double safetyAtOnset = -1;
            for (double h = 0; h < sim.balance.storm.endHour + 1 && !s.failed; h += sim.balance.dtHours)
            {
                s = sim.Step(s);
                if (s.stormPhase == StormPhase.Particles && safetyAtOnset < 0) safetyAtOnset = s.safety;
            }

            Assert.IsTrue(s.failed);
            Assert.AreNotEqual(FailReason.None, s.failReason);
            Assert.Greater(safetyAtOnset, 95.0, "no instant game over at onset");
            Assert.Greater(s.StormRelHour, sim.balance.storm.onsetHour + 2, "failure takes hours, not a moment");
            Assert.AreEqual(OutcomeKind.Fail, Outcome.Evaluate(sim.balance, s).kind);
        }

        [Test]
        public void FailedState_StaysStable()
        {
            var sim = SimTestKit.Sim();
            var s = BaseInStorm(sim);
            s = sim.Advance(s, 20);
            Assert.IsTrue(s.failed);
            double t = s.timeHours;
            s = sim.Advance(s, 5);
            Assert.AreEqual(t, s.timeHours);
        }
    }
}
