using System;
using System.Collections.Generic;

namespace Mapmakers.Sim
{
    // Deterministic fixed-step simulation. Commands mutate the state they are given;
    // Step returns a new state and never reads frame time.
    public sealed class Simulator
    {
        const double Eps = 1e-6;
        public readonly Balance balance;
        public readonly TerrainGrid terrain;
        public string geologistName = "Yuki";
        public bool missionEnabled = true;

        public Simulator(Balance balance, TerrainGrid terrain)
        {
            this.balance = balance;
            this.terrain = terrain;
        }

        public SimState NewGame()
        {
            return new SimState
            {
                supplies = balance.startSupplies,
                waterL = balance.startWaterL,
                meals = balance.startMeals,
                airHours = balance.startAirHours,
                maxBatteries = balance.startMaxBatteries
            };
        }

        // ---------- commands ----------

        public PlacementResult CanPlace(SimState s, BuildingType type, int x, int y)
        {
            return Placement.Check(balance, terrain, s, type, x, y);
        }

        public PlacementResult Place(SimState s, BuildingType type, int x, int y)
        {
            var result = CanPlace(s, type, x, y);
            if (!result.Ok) return result;
            var def = balance.Def(type);
            var b = new PlacedBuilding { id = s.nextId++, type = type, x = x, y = y, enabled = type != BuildingType.Shelter };
            s.buildings.Add(b);
            s.supplies -= def.cost;
            Log(s, "placed_" + type, def.displayName + " placed.");
            result.building = b;
            return result;
        }

        public bool Remove(SimState s, int id)
        {
            var b = s.Find(id);
            if (b == null) return false;
            s.buildings.Remove(b);
            s.supplies += balance.Def(b.type).cost;
            s.batteryKWh = Math.Min(s.batteryKWh, BatteryCapacity(s));
            Log(s, "removed_" + b.type, balance.Def(b.type).displayName + " removed. Supplies returned.");
            return true;
        }

        public bool SetEnabled(SimState s, int id, bool on)
        {
            var b = s.Find(id);
            if (b == null) return false;
            var def = balance.Def(b.type);
            if (!def.optional) return false;
            if (b.enabled == on) return true;
            b.enabled = on;
            b.tripped = false;
            if (!on) b.powered = false;
            Log(s, (on ? "on_" : "off_") + b.type, def.displayName + (on ? " switched on." : " switched off."));
            return true;
        }

        public void StartShadow(SimState s)
        {
            s.shadowFactor = balance.shadow.factor;
            s.shadowEndsAt = s.timeHours + balance.shadow.durationHours;
            Log(s, "shadow_start", "A long shadow covers the base. Less sunlight reaches the panels.");
        }

        public void StartStorm(SimState s)
        {
            s.stormStartHour = s.timeHours;
            s.stormPhase = StormPhase.Warning;
            s.geologist = CrewLocation.Outside;
            s.minSafetyStorm = s.safety;
            s.stage = MissionStage.Storm;
            Log(s, "storm_warning", "Solar particle warning from mission control.");
        }

        public bool CanRecall(SimState s, out string why)
        {
            why = "";
            if (s.geologist != CrewLocation.Outside || s.recallIssued) { why = geologistName + " is not outside."; return false; }
            if (!s.AnyPowered(BuildingType.Communications)) { why = "The radio is off. Communications needs power."; return false; }
            return true;
        }

        public bool RecallGeologist(SimState s)
        {
            if (!CanRecall(s, out _)) return false;
            s.recallIssued = true;
            s.recallRelHour = s.StormRelHour;
            s.geologist = CrewLocation.Returning;
            s.geologistArrivesAt = s.timeHours + balance.storm.recallWalkHours;
            Log(s, "recall", geologistName + " is called back to base.");
            return true;
        }

        // ---------- queries ----------

        public double EffectiveSun(SimState s, double tileSun)
        {
            double lit = tileSun + (1 - tileSun) * s.sunrise;
            return Math.Max(0, Math.Min(1, lit * s.shadowFactor));
        }

        public double SunFor(SimState s, PlacedBuilding b)
        {
            var def = balance.Def(b.type);
            return EffectiveSun(s, terrain.AverageSun(b.x, b.y, def.width, def.height));
        }

        public double BatteryCapacity(SimState s)
        {
            double cap = 0;
            foreach (var b in s.buildings) cap += balance.Def(b.type).batteryKWh;
            return cap;
        }

        public double RadiationIntensity(SimState s)
        {
            if (s.stormStartHour < 0) return 0;
            var t = balance.storm;
            double rel = s.StormRelHour;
            if (rel < t.onsetHour || rel >= t.endHour) return 0;
            if (rel < t.decayHour) return 1;
            return 1 - (rel - t.decayHour) / (t.endHour - t.decayHour);
        }

        // ---------- stepping ----------

        public SimState Advance(SimState s, double hours)
        {
            long steps = (long)Math.Round(hours / balance.dtHours);
            for (long i = 0; i < steps; i++) s = Step(s);
            return s;
        }

        public SimState Step(SimState previous)
        {
            var s = previous.Clone();
            if (s.failed) return s;
            double dt = balance.dtHours;
            var r = new StepReport();
            s.report = r;

            UpdateLighting(s);
            UpdateStorm(s);
            StepPower(s, r, dt);
            StepLife(s, r, dt);
            StepSafety(s, r, dt);

            s.step++;
            s.timeHours = s.step * dt;
            if (missionEnabled) UpdateMission(s);
            return s;
        }

        void UpdateLighting(SimState s)
        {
            if (s.shadowEndsAt >= 0 && s.timeHours >= s.shadowEndsAt - Eps)
            {
                s.shadowFactor = 1;
                s.shadowEndsAt = -1;
                Log(s, "shadow_end", "The long shadow has passed. Full sunlight is back.");
            }
            if (s.sunriseStartedAt >= 0)
                s.sunrise = Math.Min(1, (s.timeHours - s.sunriseStartedAt) / balance.sunriseHours);
        }

        void UpdateStorm(SimState s)
        {
            if (s.stormStartHour < 0 || s.stormPhase == StormPhase.Over) return;
            var t = balance.storm;
            double rel = s.StormRelHour;
            var phase = rel < t.onsetHour - Eps ? StormPhase.Warning
                : rel < t.decayHour - Eps ? StormPhase.Particles
                : rel < t.endHour - Eps ? StormPhase.Decay
                : StormPhase.Over;

            if (phase != s.stormPhase)
            {
                s.stormPhase = phase;
                if (phase == StormPhase.Particles)
                {
                    s.batteryAtOnsetKWh = s.batteryKWh;
                    s.batteriesAtOnset = s.Count(BuildingType.Battery);
                    Log(s, "storm_onset", "Solar particles have reached the Moon.");
                }
                else if (phase == StormPhase.Decay) Log(s, "storm_decay", "The particle storm is fading.");
                else if (phase == StormPhase.Over) Log(s, "storm_over", "All clear. The storm is over.");
            }

            if (s.geologist == CrewLocation.Outside && !s.recallIssued && rel >= t.onsetHour + t.selfReturnDelayHours - Eps)
            {
                s.geologist = CrewLocation.Returning;
                s.geologistArrivesAt = s.timeHours + t.recallWalkHours;
                Log(s, "self_return", geologistName + " hears her suit alarm and heads back.");
            }
            if (s.geologist == CrewLocation.Returning && s.timeHours >= s.geologistArrivesAt - Eps)
            {
                s.geologist = CrewLocation.Inside;
                Log(s, "geologist_inside", geologistName + " is back inside.");
            }
        }

        void StepPower(SimState s, StepReport r, double dt)
        {
            double gen = 0, cap = 0, rate = 0;
            var loads = new List<PlacedBuilding>();
            foreach (var b in s.buildings)
            {
                var def = balance.Def(b.type);
                cap += def.batteryKWh;
                rate += def.batteryRateKW;
                if (def.generationKW > 0 && b.enabled)
                {
                    gen += def.generationKW * SunFor(s, b);
                    b.powered = true;
                }
                if (def.demandKW > 0)
                {
                    if (b.enabled) loads.Add(b);
                    else b.powered = false;
                }
            }
            loads.Sort((a, b) =>
            {
                int byPriority = balance.Def(a.type).priority.CompareTo(balance.Def(b.type).priority);
                return byPriority != 0 ? byPriority : a.id.CompareTo(b.id);
            });

            if (s.batteryKWh > cap) s.batteryKWh = cap;
            double discharge = Math.Min(rate, s.batteryKWh / dt);
            double available = gen + discharge;
            bool recovered = cap > 0 && s.batteryKWh >= balance.restoreBatteryFraction * cap - Eps;

            // Loads that tripped earlier stay off until the battery recovers or generation alone covers them.
            var candidates = new List<PlacedBuilding>();
            double wanted = 0, total = 0;
            foreach (var b in loads)
            {
                double demand = balance.Def(b.type).demandKW;
                wanted += demand;
                if (b.tripped && !recovered && gen + Eps < total + demand) continue;
                candidates.Add(b);
                total += demand;
            }

            // Shed strictly from the lowest priority until demand fits the supply.
            var shedNow = new List<PlacedBuilding>();
            while (candidates.Count > 0 && total > available + Eps)
            {
                var last = candidates[candidates.Count - 1];
                candidates.RemoveAt(candidates.Count - 1);
                total -= balance.Def(last.type).demandKW;
                shedNow.Add(last);
            }

            var wasPowered = new HashSet<int>();
            foreach (var b in loads)
                if (b.powered) wasPowered.Add(b.id);
            foreach (var b in loads) b.powered = false;
            foreach (var b in candidates)
            {
                b.powered = true;
                b.tripped = false;
            }

            double unmetCritical = 0;
            var lostNames = new List<string>();
            foreach (var b in shedNow)
            {
                var def = balance.Def(b.type);
                if (def.priority <= 1) unmetCritical += def.demandKW;
                b.tripped = true;
                if (wasPowered.Contains(b.id)) lostNames.Add(def.displayName);
            }
            foreach (var b in loads)
                if (!b.powered && balance.Def(b.type).priority <= 1 && !shedNow.Contains(b))
                    unmetCritical += balance.Def(b.type).demandKW;

            if (lostNames.Count > 0)
            {
                s.brownouts++;
                if (s.stormPhase != StormPhase.None && s.stormPhase != StormPhase.Over) s.stormBrownouts++;
                Log(s, "brownout", "Battery empty at " + SimText.Clock(s.timeHours, balance.startClockHour)
                    + ": demand " + SimText.Num(wanted) + " kW > supply " + SimText.Num(gen)
                    + " kW. Switched off: " + string.Join(", ", lostNames) + ".");
            }

            double net = gen - total;
            double flow;
            if (net >= 0)
            {
                flow = cap > 0 ? Math.Min(net, Math.Min(rate, (cap - s.batteryKWh) / dt)) : 0;
                s.batteryKWh += flow * dt;
            }
            else
            {
                double draw = Math.Min(-net, discharge);
                s.batteryKWh -= draw * dt;
                flow = -draw;
            }
            if (s.batteryKWh < 1e-9) s.batteryKWh = 0;
            if (s.batteryKWh > cap) s.batteryKWh = cap;

            s.unmetCriticalKWh += unmetCritical * dt;
            if (unmetCritical > 0 && !s.criticalOut) Log(s, "critical_out", "Critical systems lost power.");
            s.criticalOut = unmetCritical > 0;

            r.generationKW = gen;
            r.demandKW = wanted;
            r.servedKW = total;
            r.batteryFlowKW = flow;
            r.batteryCapacityKWh = cap;
            r.unmetCriticalKW = unmetCritical;

            if (unmetCritical > 0)
                Warn(r, "critical_unpowered", "Critical systems have no power: they need " + SimText.Num(CriticalDemand(loads))
                    + " kW but supply is " + SimText.Num(available) + " kW.");
            var off = new List<string>();
            foreach (var b in loads)
                if (b.tripped) off.Add(balance.Def(b.type).displayName);
            if (off.Count > 0)
                Warn(r, "loads_off", "Not enough power: demand " + SimText.Num(wanted) + " kW > supply " + SimText.Num(gen)
                    + " kW. Switched off: " + string.Join(", ", off) + ".");
            else if (flow < -Eps)
                Warn(r, "battery_draining", "Battery draining: demand " + SimText.Num(total) + " kW > supply " + SimText.Num(gen)
                    + " kW. Empty in " + SimText.Num(s.batteryKWh / -flow) + " h.");
        }

        double CriticalDemand(List<PlacedBuilding> loads)
        {
            double total = 0;
            foreach (var b in loads)
                if (balance.Def(b.type).priority <= 1) total += balance.Def(b.type).demandKW;
            return total;
        }

        void StepLife(SimState s, StepReport r, double dt)
        {
            bool lifeSupport = s.AnyPowered(BuildingType.LifeSupport);
            double airBonus = 0, waterNet = 0, food = 0;
            foreach (var b in s.buildings)
            {
                var def = balance.Def(b.type);
                if (def.foodPerHour <= 0 || !b.enabled || !b.powered) continue;
                if (s.waterL <= 0) continue;
                airBonus += def.airBonusPerHour;
                waterNet -= def.waterInLph - def.waterReturnLph;
                food += def.foodPerHour;
            }

            double airRate = (lifeSupport ? balance.airRefillPerHour : -balance.airDrainPerHour) + airBonus;
            double air = Math.Max(0, Math.Min(balance.airCapHours, s.airHours + airRate * dt));
            r.airPerHour = (air - s.airHours) / dt;
            s.airHours = air;

            double water = Math.Max(0, s.waterL + waterNet * dt);
            r.waterPerHour = (water - s.waterL) / dt;
            s.waterL = water;

            double meals = Math.Max(0, s.meals + (food - balance.CrewMealsPerHour) * dt);
            r.foodPerHour = (meals - s.meals) / dt;
            s.meals = meals;
            s.mealsHarvested += food * dt;
            s.noFoodHours = s.meals <= 0 ? s.noFoodHours + dt : 0;

            if (s.Count(BuildingType.Habitat) == 0) return;
            if (s.airHours <= 0)
                Warn(r, "air_empty", "No air left: " + LifeSupportProblem(s));
            else if (!lifeSupport)
                Warn(r, "air_dropping", "Air is dropping: " + LifeSupportProblem(s) + " Air left: " + SimText.Num(s.airHours) + " h.");
            if (s.meals <= 0) Warn(r, "food_empty", "No meals left: the crew is hungry. A powered Greenhouse grows food.");
            if (s.waterL <= 0 && s.Count(BuildingType.Greenhouse) > 0) Warn(r, "water_empty", "Water is gone: the Greenhouse cannot grow food.");
        }

        string LifeSupportProblem(SimState s)
        {
            if (s.Count(BuildingType.LifeSupport) == 0) return "there is no Life Support yet.";
            return "Life Support has no power.";
        }

        void StepSafety(SimState s, StepReport r, double dt)
        {
            double radiation = RadiationIntensity(s);
            bool sheltered = s.AnyPowered(BuildingType.Shelter);
            bool outside = s.geologist != CrewLocation.Inside;
            r.radiation = radiation;
            r.sheltered = sheltered;

            double noAir = s.airHours <= 0 ? balance.safetyNoAirPerHour : 0;
            double noFood = s.noFoodHours > balance.noFoodGraceHours ? balance.safetyNoFoodPerHour : 0;
            double radOutside = outside ? balance.radiationOutsidePerHour * radiation : 0;
            double radInside = sheltered ? 0 : balance.radiationUnshelteredPerHour * radiation;
            double harm = noAir + noFood + radOutside + radInside;

            double rate = harm > 0 ? -harm : balance.safetyRecoverPerHour;
            double safety = Math.Max(0, Math.Min(100, s.safety + rate * dt));
            r.safetyPerHour = (safety - s.safety) / dt;
            s.safety = safety;
            s.damageNoAir += noAir * dt;
            s.damageNoFood += noFood * dt;
            s.damageOutside += radOutside * dt;
            s.damageNoShelter += radInside * dt;

            if (s.stormPhase != StormPhase.None && s.stormPhase != StormPhase.Over)
            {
                s.minSafetyStorm = Math.Min(s.minSafetyStorm, s.safety);
                if (sheltered) s.shelterHours += dt;
                var greenhouse = FirstOf(s, BuildingType.Greenhouse);
                if (radiation > 0 && greenhouse != null && !greenhouse.enabled) s.greenhouseOffInStorm = true;
            }

            if (radOutside > 0) Warn(r, "radiation_outside", geologistName + " is outside in the particle storm.");
            if (radInside > 0) Warn(r, "radiation_noshelter", s.Count(BuildingType.Shelter) == 0
                ? "Particle storm: the crew has no Shelter."
                : "Particle storm: the Shelter is not switched on or has no power.");

            s.lowSafetyHours = s.safety < balance.safetyFailThreshold ? s.lowSafetyHours + dt : 0;
            if (s.safety < balance.safetyFailThreshold)
                Warn(r, "safety_low", "Crew safety is low. Fix the warnings above.");
            if (s.lowSafetyHours >= balance.safetyFailHours - Eps)
            {
                s.failed = true;
                s.failReason = WorstCause(s);
                Log(s, "failed", "Mission paused: crew safety stayed too low.");
            }
        }

        static FailReason WorstCause(SimState s)
        {
            var reason = FailReason.NoAir;
            double worst = s.damageNoAir;
            if (s.damageNoFood > worst) { worst = s.damageNoFood; reason = FailReason.NoFood; }
            if (s.damageOutside > worst) { worst = s.damageOutside; reason = FailReason.RadiationOutside; }
            if (s.damageNoShelter > worst) reason = FailReason.RadiationNoShelter;
            return reason;
        }

        void UpdateMission(SimState s)
        {
            if (!s.harvestDone && s.mealsHarvested >= balance.harvestMeals - Eps)
            {
                s.harvestDone = true;
                s.shelterUnlocked = true;
                s.supplies += balance.harvestSupplyReward;
                Log(s, "first_harvest", "First harvest! A supply lander brings " + balance.harvestSupplyReward + " supplies. Shelter unlocked.");
            }

            switch (s.stage)
            {
                case MissionStage.Build:
                    if (!HasCoreBase(s)) return;
                    s.stage = MissionStage.ShadowWarning;
                    s.shadowStartsAt = s.timeHours + balance.shadow.warningHours;
                    Log(s, "shadow_warning", "A long shadow will cross the base in " + SimText.Num(balance.shadow.warningHours) + " hours.");
                    return;
                case MissionStage.ShadowWarning:
                    if (s.timeHours < s.shadowStartsAt - Eps) return;
                    s.stage = MissionStage.Shadow;
                    StartShadow(s);
                    return;
                case MissionStage.Shadow:
                    if (s.shadowEndsAt >= 0) return;
                    s.stage = MissionStage.Recover;
                    s.shadowPassed = true;
                    s.maxBatteries = balance.unlockedMaxBatteries;
                    Log(s, "shadow_passed", "Power shortage passed. A second Battery is unlocked.");
                    return;
                case MissionStage.Recover:
                    if (!s.harvestDone) return;
                    if (s.stormStartsAt < 0) s.stormStartsAt = s.timeHours + balance.storm.leadHours;
                    if (s.timeHours >= s.stormStartsAt - Eps) StartStorm(s);
                    return;
                case MissionStage.Storm:
                    if (s.stormPhase != StormPhase.Over) return;
                    s.stage = MissionStage.Sunrise;
                    s.sunriseStartedAt = s.timeHours;
                    Log(s, "sunrise_start", "Sunlight creeps over the ridge.");
                    return;
                case MissionStage.Sunrise:
                    if (s.timeHours < s.sunriseStartedAt + balance.sunriseHours - Eps) return;
                    s.sunrise = 1;
                    s.stage = MissionStage.Done;
                    Log(s, "sunrise_done", "The first lunar sunrise lights the whole base.");
                    return;
            }
        }

        static readonly BuildingType[] CoreTypes =
        {
            BuildingType.Habitat, BuildingType.SolarArray, BuildingType.Battery,
            BuildingType.LifeSupport, BuildingType.Communications, BuildingType.Greenhouse
        };

        public static bool HasCoreBase(SimState s)
        {
            foreach (var type in CoreTypes)
                if (s.Count(type) == 0) return false;
            return true;
        }

        static PlacedBuilding FirstOf(SimState s, BuildingType type)
        {
            foreach (var b in s.buildings)
                if (b.type == type) return b;
            return null;
        }

        void Log(SimState s, string code, string text)
        {
            s.events.Add(new SimEvent { timeHours = s.timeHours, code = code, text = text });
        }

        static void Warn(StepReport r, string code, string text)
        {
            r.warnings.Add(new SimWarning { code = code, text = text });
        }
    }
}
