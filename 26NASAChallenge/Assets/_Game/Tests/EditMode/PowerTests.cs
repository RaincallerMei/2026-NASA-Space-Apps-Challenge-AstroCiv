using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    public class PowerTests
    {
        [Test]
        public void Deficit_DrainsBatteryToExactlyZero_AtTheRightHour()
        {
            // 3 panels = 9 kW, demand 10 kW, so the 4 kWh battery loses 1 kW and is empty after exactly 4 hours.
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 3);
            s.batteryKWh = 4;

            s = sim.Advance(s, 2);
            Assert.AreEqual(2.0, s.batteryKWh, 1e-6);
            Assert.AreEqual(-1.0, s.report.batteryFlowKW, 1e-6);

            s = sim.Advance(s, 2);
            Assert.AreEqual(0.0, s.batteryKWh, 0.0);
            Assert.AreEqual(0, s.brownouts, "all loads stay on until the battery is empty");

            s = sim.Step(s);
            Assert.AreEqual(1, s.brownouts, "the first step after empty sheds load");
            Assert.AreEqual(4.0, s.timeHours - sim.balance.dtHours, 1e-6);
        }

        [Test]
        public void Energy_NeverGoesNegative_OrAboveCapacity()
        {
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 1);
            s.batteryKWh = 4;
            for (int i = 0; i < 2000; i++)
            {
                s = sim.Step(s);
                Assert.GreaterOrEqual(s.batteryKWh, 0.0);
                Assert.LessOrEqual(s.batteryKWh, s.report.batteryCapacityKWh + 1e-9);
            }

            double cap = sim.balance.Def(BuildingType.Battery).batteryKWh;
            var charged = SimTestKit.FullBase(sim, 4);
            for (int i = 0; i < 2000; i++)
            {
                charged = sim.Step(charged);
                Assert.LessOrEqual(charged.batteryKWh, cap + 1e-9);
            }
            Assert.AreEqual(cap, charged.batteryKWh, 1e-6);
        }

        [Test]
        public void Battery_RespectsChargeAndDischargePowerLimit()
        {
            var sim = SimTestKit.Sim();
            var s = sim.NewGame();
            SimTestKit.Put(sim, s, BuildingType.Battery, 0, 0);
            for (int i = 0; i < 4; i++) SimTestKit.Put(sim, s, BuildingType.SolarArray, 2 + i * 3, 0);

            s = sim.Step(s);
            Assert.AreEqual(12.0, s.report.generationKW, 1e-6);
            double rate = sim.balance.Def(BuildingType.Battery).batteryRateKW;
            Assert.Less(rate, 12.0);
            Assert.AreEqual(rate, s.report.batteryFlowKW, 1e-6, "charge is capped at the battery's power limit");
        }

        [Test]
        public void Shedding_KeepsLifeSupportAndHabitat_ShedsCommunicationsThenGreenhouse()
        {
            // 2 panels = 6 kW. Critical loads need 5 kW. The battery is empty.
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 2);
            s = sim.Step(s);

            Assert.IsTrue(SimTestKit.First(s, BuildingType.LifeSupport).powered);
            Assert.IsTrue(SimTestKit.First(s, BuildingType.Habitat).powered);
            Assert.IsFalse(SimTestKit.First(s, BuildingType.Greenhouse).powered);
            Assert.IsFalse(SimTestKit.First(s, BuildingType.Communications).powered);
            Assert.AreEqual(0.0, s.report.unmetCriticalKW, 1e-9);

            // 3 panels = 9 kW: only the lowest priority load (Communications, 1 kW) has to go.
            var s3 = SimTestKit.FullBase(sim, 3);
            s3 = sim.Step(s3);
            Assert.IsFalse(SimTestKit.First(s3, BuildingType.Communications).powered);
            Assert.IsTrue(SimTestKit.First(s3, BuildingType.Greenhouse).powered);
        }

        [Test]
        public void Shedding_ProtectsShelterBeforeGreenhouse()
        {
            // 8 kW supply: Habitat 1 + Life Support 4 + Shelter 2 = 7 fit; Greenhouse and Communications do not.
            var sim = SimTestKit.Sim(sun: 8.0 / 9.0);
            var s = SimTestKit.FullBase(sim, 3);
            s.shelterUnlocked = true;
            var shelter = SimTestKit.Put(sim, s, BuildingType.Shelter, 15, 0);
            sim.SetEnabled(s, shelter.id, true);
            s = sim.Step(s);

            Assert.IsTrue(SimTestKit.First(s, BuildingType.Shelter).powered);
            Assert.IsFalse(SimTestKit.First(s, BuildingType.Greenhouse).powered);
        }

        [Test]
        public void UnmetCriticalDemand_IsTrackedSeparately()
        {
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 1);
            s = sim.Advance(s, 0.5);

            Assert.IsFalse(SimTestKit.First(s, BuildingType.LifeSupport).powered);
            Assert.AreEqual(4.0, s.report.unmetCriticalKW, 1e-9);
            Assert.Greater(s.unmetCriticalKWh, 0.0);
            Assert.IsTrue(s.report.warnings.Exists(w => w.code == "critical_unpowered"));
        }

        [Test]
        public void Warning_SaysExactlyWhy()
        {
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 3);
            s.batteryKWh = 4;
            s = sim.Step(s);
            var warning = s.report.warnings.Find(w => w.code == "battery_draining");
            Assert.IsNotNull(warning);
            StringAssert.Contains("demand 10 kW > supply 9 kW", warning.text);

            s = sim.Advance(s, 4.5);
            var brownout = s.events.Find(e => e.code == "brownout");
            Assert.IsNotNull(brownout);
            StringAssert.Contains("Not enough power. Switched off: Radio Dish", brownout.text);
            var off = s.report.warnings.Find(w => w.code == "loads_off");
            Assert.IsNotNull(off);
            StringAssert.Contains("demand 10 kW > supply 9 kW", off.text);
        }

        [Test]
        public void TrippedLoad_ComesBackWhenSupplyCoversIt()
        {
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 3);
            s = sim.Step(s);
            Assert.IsFalse(SimTestKit.First(s, BuildingType.Communications).powered);

            SimTestKit.Put(sim, s, BuildingType.SolarArray, 12, 5);
            s = sim.Step(s);
            Assert.IsTrue(SimTestKit.First(s, BuildingType.Communications).powered);
        }

        [Test]
        public void SameInputs_GiveSameState()
        {
            var a = Run();
            var b = Run();
            Assert.AreEqual(a.timeHours, b.timeHours);
            Assert.AreEqual(a.batteryKWh, b.batteryKWh);
            Assert.AreEqual(a.airHours, b.airHours);
            Assert.AreEqual(a.waterL, b.waterL);
            Assert.AreEqual(a.meals, b.meals);
            Assert.AreEqual(a.safety, b.safety);
            Assert.AreEqual(a.events.Count, b.events.Count);
            for (int i = 0; i < a.events.Count; i++) Assert.AreEqual(a.events[i].text, b.events[i].text);
        }

        static SimState Run()
        {
            var sim = SimTestKit.Sim(0.9);
            var s = SimTestKit.FullBase(sim, 4);
            s = sim.Advance(s, 10);
            sim.SetEnabled(s, SimTestKit.First(s, BuildingType.Greenhouse).id, false);
            s = sim.Advance(s, 30);
            return s;
        }

        [Test]
        public void Step_DoesNotChangeTheInputState()
        {
            var sim = SimTestKit.Sim();
            var s = SimTestKit.FullBase(sim, 3);
            s.batteryKWh = 4;
            var next = sim.Step(s);
            Assert.AreEqual(4.0, s.batteryKWh);
            Assert.AreEqual(0.0, s.timeHours);
            Assert.AreNotSame(s, next);
            Assert.Less(next.batteryKWh, 4.0);
        }
    }
}
