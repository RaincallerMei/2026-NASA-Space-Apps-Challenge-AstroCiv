using System;
using System.Collections.Generic;

namespace Mapmakers.Sim
{
    // Deterministic fixed-step simulation. Commands mutate the state they are given; Step returns a new state
    // and never reads frame time. A runtime clock outside the simulation decides how many steps to run.
    public sealed class Simulator
    {
        const double Eps = 1e-6;
        public readonly Balance balance;
        public readonly TerrainGrid terrain;
        public readonly Scenario scenario;
        public PlanetProfile planet = PlanetProfile.Moon;
        public string geologistName = "Yuki";
        // Scheduled events and the first-harvest reward run only in missions (off in the drawing and in unit tests).
        public bool missionEnabled = true;
        // Off in Tutorial 2: reserves can run low, but the run never stops.
        public bool canFail = true;
        // Placement links, the base area, soil and the rover (off in the drawing and in plain power tests).
        public bool layoutRules = true;
        bool previewing;

        sealed class Trigger { public double time; public ScheduledEvent e; public bool warning; }
        readonly List<Trigger> triggers = new List<Trigger>();

        public Simulator(Balance balance, TerrainGrid terrain, Scenario scenario = null)
        {
            this.balance = balance;
            this.terrain = terrain;
            this.scenario = scenario ?? new Scenario();
            BuildTriggers();
        }

        void BuildTriggers()
        {
            foreach (var e in scenario.events)
            {
                if (e.Kind == EventKind.Shadow && e.hour - balance.shadow.warningHours >= 0)
                    triggers.Add(new Trigger { time = e.hour - balance.shadow.warningHours, e = e, warning = true });
                triggers.Add(new Trigger { time = e.hour, e = e });
            }
            if (scenario.Timed) triggers.RemoveAll(t => t.time >= scenario.durationHours - Eps);
            triggers.Sort((a, b) => a.time.CompareTo(b.time));
        }

        public double DurationHours => scenario.durationHours;

        public SimState NewGame()
        {
            return new SimState
            {
                supplies = balance.startSupplies + planet.extraSupplies,
                waterL = balance.startWaterL,
                meals = balance.startMeals,
                airHours = balance.startAirHours,
                minAirHours = balance.startAirHours,
                soil = balance.startSoil,
                maxBatteries = Math.Max(balance.startMaxBatteries, scenario.maxBatteries),
                shelterUnlocked = scenario.shelterUnlocked
            };
        }

        // ---------- commands (all refused once the run has ended) ----------

        public PlacementResult CanPlace(SimState s, BuildingType type, int x, int y)
        {
            if (s.Finished) return PlacementResult.Fail(PlacementReason.Locked, "The mission has ended.");
            return Placement.Check(this, s, type, x, y);
        }

        public PlacementResult Place(SimState s, BuildingType type, int x, int y)
        {
            var result = CanPlace(s, type, x, y);
            if (!result.Ok) return result;
            var def = balance.Def(type);
            var b = new PlacedBuilding { id = s.nextId++, type = type, x = x, y = y, enabled = type != BuildingType.Shelter };
            s.buildings.Add(b);
            s.supplies -= def.cost;
            if (layoutRules) s.soil -= def.soilCost;
            Log(s, "placed_" + type, def.displayName + " placed.");
            result.building = b;
            return result;
        }

        // Rebuilds a saved base at no cost (a level that continues the previous level's base).
        public PlacementResult PlaceFree(SimState s, BuildingType type, int x, int y)
        {
            var def = balance.Def(type);
            s.supplies += def.cost;
            s.soil += def.soilCost;
            var result = Place(s, type, x, y);
            if (!result.Ok)
            {
                s.supplies -= def.cost;
                s.soil -= def.soilCost;
            }
            else if (!layoutRules) s.soil -= def.soilCost;
            return result;
        }

        public bool Remove(SimState s, int id)
        {
            var b = s.Find(id);
            if (b == null || s.Finished) return false;
            if (b.type == BuildingType.Habitat && s.buildings.Count > 1) return false;
            var def = balance.Def(b.type);
            s.buildings.Remove(b);
            s.supplies += def.cost;
            if (layoutRules) s.soil += def.soilCost;
            s.batteryKWh = Math.Min(s.batteryKWh, BatteryCapacity(s));
            Log(s, "removed_" + b.type, def.displayName + " removed. Supplies returned.");
            return true;
        }

        public bool SetEnabled(SimState s, int id, bool on)
        {
            var b = s.Find(id);
            if (b == null || s.Finished) return false;
            var def = balance.Def(b.type);
            if (!def.optional) return false;
            if (b.enabled == on) return true;
            b.enabled = on;
            b.tripped = false;
            if (!on) b.powered = false;
            Log(s, (on ? "on_" : "off_") + b.type, def.displayName + (on ? " switched on." : " switched off."));
            return true;
        }

        public bool CanRepair(SimState s, int id, out string why)
        {
            why = "";
            var b = s.Find(id);
            if (s.Finished) { why = "The mission has ended."; return false; }
            if (b == null || !b.broken) { why = "Nothing to fix."; return false; }
            if (s.supplies < balance.repairCost) { why = "Need " + balance.repairCost + " supplies."; return false; }
            return true;
        }

        public bool Repair(SimState s, int id)
        {
            if (!CanRepair(s, id, out _)) return false;
            var b = s.Find(id);
            b.broken = false;
            s.supplies -= balance.repairCost;
            Log(s, "repaired_" + b.type, balance.Def(b.type).displayName + " fixed.");
            return true;
        }

        // strength below 1 sets how much sun gets through (a thick dust storm); otherwise the balance default.
        public void StartShadow(SimState s, double strength = 1, double durationHours = 0)
        {
            s.shadowFactor = strength < 1 ? strength : balance.shadow.factor;
            s.shadowEndsAt = s.timeHours + (durationHours > 0 ? durationHours : balance.shadow.durationHours);
            Log(s, "shadow_start", planet.dimmingStart);
        }

        public void StartStorm(SimState s, double strength = 1)
        {
            s.stormStartHour = s.timeHours;
            s.stormStrength = strength;
            s.stormPhase = StormPhase.Warning;
            s.stormsStarted++;
            s.recallIssued = false;
            s.recallRelHour = -1;
            s.geologist = CrewLocation.Outside;
            s.minSafetyStorm = Math.Min(s.minSafetyStorm, s.safety);
            Log(s, "storm_warning", "Solar warning! Particles arrive in " + SimText.Num(balance.storm.onsetHour) + " hours. "
                + geologistName + " is outside.");
        }

        public bool CanRecall(SimState s, out string why)
        {
            why = "";
            if (s.Finished) { why = "The mission has ended."; return false; }
            if (s.geologist != CrewLocation.Outside || s.recallIssued) { why = geologistName + " is not outside."; return false; }
            if (!s.AnyPowered(BuildingType.Communications)) { why = "The radio needs power."; return false; }
            return true;
        }

        public bool RecallGeologist(SimState s)
        {
            if (!CanRecall(s, out _)) return false;
            s.recallIssued = true;
            s.recallRelHour = s.StormRelHour;
            if (s.recallRelHour < balance.storm.onsetHour) s.earlyRecalls++;
            s.geologist = CrewLocation.Returning;
            s.geologistArrivesAt = s.timeHours + balance.storm.recallWalkHours;
            Log(s, "recall", geologistName + " is coming back to base.");
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

        public bool HasCable(SimState s, PlacedBuilding solar)
        {
            return !layoutRules || Layout.GapTo(balance, s, solar, BuildingType.Battery) <= balance.layout.cableGap;
        }

        // kW one Solar Array makes right now: tile sun x planet sunlight x cable.
        public double SolarOutput(SimState s, PlacedBuilding b)
        {
            double kw = balance.Def(b.type).generationKW * SunFor(s, b) * planet.solarScale;
            return HasCable(s, b) ? kw : kw * (1 - balance.layout.longCableLoss);
        }

        public bool PlantLoop(SimState s)
        {
            if (!layoutRules) return false;
            var ls = s.First(BuildingType.LifeSupport);
            if (ls == null) return false;
            foreach (var g in s.buildings)
                if (g.type == BuildingType.Greenhouse && g.enabled && !g.broken && Layout.Gap(balance, ls, g) <= balance.layout.plantLoopGap) return true;
            return false;
        }

        public bool ShortDuct(SimState s)
        {
            var ls = s.First(BuildingType.LifeSupport);
            return !layoutRules || ls == null || Layout.GapTo(balance, s, ls, BuildingType.Habitat) <= balance.layout.ductGap;
        }

        public bool ShelterClose(SimState s)
        {
            var shelter = s.First(BuildingType.Shelter);
            return !layoutRules || shelter == null || Layout.GapTo(balance, s, shelter, BuildingType.Habitat) <= balance.layout.shelterGap;
        }

        public double DemandOf(SimState s, PlacedBuilding b)
        {
            double demand = balance.Def(b.type).demandKW;
            if (b.type == BuildingType.LifeSupport && PlantLoop(s)) demand = Math.Max(0, demand - balance.layout.plantLoopSavesKW);
            return demand;
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
            if (rel < t.decayHour) return s.stormStrength;
            return s.stormStrength * (1 - (rel - t.decayHour) / (t.endHour - t.decayHour));
        }

        // Soil still needed for buildings that are not built yet.
        public double SoilNeeded(SimState s)
        {
            double need = 0;
            foreach (var def in balance.buildings)
            {
                var type = (BuildingType)Enum.Parse(typeof(BuildingType), def.type);
                if (def.soilCost > 0 && s.Count(type) < def.maxCount) need += def.soilCost;
            }
            return need;
        }

        // Rover trip time to soil or water from the garage (infinite if out of reach).
        public double TripHours(PlacedBuilding garage, bool water)
        {
            var def = balance.Def(garage.type);
            int gap = water ? Layout.WaterGap(terrain, garage.x, garage.y, def.width, def.height)
                            : Layout.SoilGap(terrain, garage.x, garage.y, def.width, def.height);
            return Layout.TripHours(balance.layout, gap);
        }

        // One step ahead on a copy: rates and powered flags for the current layout, also while time is stopped.
        // Never changes the state, never fires events or ends the run.
        public SimState PreviewState(SimState s)
        {
            previewing = true;
            try { return Step(s); }
            finally { previewing = false; }
        }

        public StepReport Preview(SimState s) => PreviewState(s).report;

        // ---------- stepping ----------

        public SimState Advance(SimState s, double hours)
        {
            long steps = (long)Math.Round(hours / balance.dtHours);
            for (long i = 0; i < steps && !s.Finished; i++) s = Step(s);
            return s;
        }

        public SimState Step(SimState previous)
        {
            var s = previous.Clone();
            if (s.Finished) return s;
            double dt = balance.dtHours;
            var r = new StepReport();
            s.report = r;

            if (missionEnabled && !previewing) FireTriggers(s);
            UpdateLighting(s);
            UpdateStorm(s);
            StepPower(s, r, dt);
            StepRover(s, r, dt);
            StepLife(s, r, dt);
            StepSafety(s, r, dt);

            s.step++;
            s.timeHours = s.step * dt;
            if (previewing) return s;
            if (missionEnabled) UpdateHarvest(s);
            if (canFail) CheckFailure(s);
            if (!s.Finished && scenario.Timed && s.timeHours >= scenario.durationHours - Eps) EndRun(s, MissionEnd.Deadline, FailReason.None);
            return s;
        }

        void FireTriggers(SimState s)
        {
            while (s.nextEvent < triggers.Count && triggers[s.nextEvent].time <= s.timeHours + Eps)
            {
                var t = triggers[s.nextEvent++];
                var e = t.e;
                switch (e.Kind)
                {
                    case EventKind.Shadow:
                        if (t.warning) Log(s, "shadow_warning", planet.dimmingWarning);
                        else StartShadow(s, e.strength, e.durationHours);
                        break;
                    case EventKind.SolarStorm:
                        StartStorm(s, e.strength);
                        break;
                    case EventKind.Fault:
                        BreakPanel(s, e.amount);
                        break;
                    case EventKind.SupplyDrop:
                        s.supplies += e.amount;
                        Log(s, "supply_drop", "A supply lander brings " + e.amount + " supplies.");
                        break;
                    case EventKind.Sunrise:
                        s.sunriseStartedAt = s.timeHours;
                        Log(s, "sunrise_start", "Sunlight creeps over the ridge. Solar power climbs.");
                        break;
                }
            }
        }

        void BreakPanel(SimState s, int pick)
        {
            var panels = s.buildings.FindAll(b => b.type == BuildingType.SolarArray && !b.broken);
            if (panels.Count == 0) return;
            panels[Math.Abs(pick) % panels.Count].broken = true;
            Log(s, "fault", "A tiny space rock hit a Solar Array. Fix it for " + balance.repairCost + " supplies.");
        }

        void UpdateLighting(SimState s)
        {
            if (s.shadowEndsAt >= 0 && s.timeHours >= s.shadowEndsAt - Eps)
            {
                s.shadowFactor = 1;
                s.shadowEndsAt = -1;
                Log(s, "shadow_end", planet.dimmingEnd);
                if (missionEnabled && !s.shadowPassed)
                {
                    s.shadowPassed = true;
                    if (s.maxBatteries < balance.unlockedMaxBatteries)
                    {
                        s.maxBatteries = balance.unlockedMaxBatteries;
                        Log(s, "battery_unlock", "You got through the dark. A second Battery is unlocked.");
                    }
                }
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
                    s.stormsSeenParticles++;
                    s.batteryAtOnsetKWh = s.batteryKWh;
                    s.batteriesAtOnset = s.Count(BuildingType.Battery);
                    Log(s, "storm_onset", "Solar particles have arrived. Stay in the Shelter.");
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
                if (def.generationKW > 0)
                {
                    b.powered = b.enabled && !b.broken;
                    if (b.powered) gen += SolarOutput(s, b);
                }
                if (def.demandKW > 0)
                {
                    if (b.enabled && !b.broken) loads.Add(b);
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
                double demand = DemandOf(s, b);
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
                total -= DemandOf(s, last);
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
            bool anyCritical = false;
            var lostNames = new List<string>();
            foreach (var b in shedNow)
            {
                b.tripped = true;
                if (wasPowered.Contains(b.id)) lostNames.Add(balance.Def(b.type).displayName);
            }
            foreach (var b in loads)
            {
                if (balance.Def(b.type).priority > 1) continue;
                anyCritical = true;
                if (!b.powered) unmetCritical += DemandOf(s, b);
            }

            if (lostNames.Count > 0 && !previewing)
            {
                s.brownouts++;
                if (s.StormActive) s.stormBrownouts++;
                if (s.shadowFactor < 1) s.dimBrownouts++;
                Log(s, "brownout", "Not enough power. Switched off: " + string.Join(", ", lostNames) + ".");
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

            bool shedding = false;
            foreach (var b in loads) shedding |= !b.powered;
            s.unmetCriticalKWh += unmetCritical * dt;
            if (unmetCritical > 0 && !s.criticalOut && !previewing) Log(s, "critical_out", "Critical systems lost power.");
            s.criticalOut = unmetCritical > 0;
            s.criticalOutHours = unmetCritical > 0 ? s.criticalOutHours + dt : 0;
            if (anyCritical) s.criticalServedHours += dt;
            if (anyCritical && unmetCritical > 0) s.criticalLostHours += dt;
            if (shedding) s.shedHours += dt;

            r.generationKW = gen;
            r.demandKW = wanted;
            r.servedKW = total;
            r.batteryFlowKW = flow;
            r.batteryCapacityKWh = cap;
            r.unmetCriticalKW = unmetCritical;

            if (unmetCritical > 0)
                Warn(r, "critical_unpowered", "Critical systems have no power: they need " + SimText.Num(CriticalDemand(s, loads))
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

        double CriticalDemand(SimState s, List<PlacedBuilding> loads)
        {
            double total = 0;
            foreach (var b in loads)
                if (balance.Def(b.type).priority <= 1) total += DemandOf(s, b);
            return total;
        }

        // The rover fetches soil until the Greenhouse can be built, then water. Shorter trips bring more per hour.
        void StepRover(SimState s, StepReport r, double dt)
        {
            var garage = s.First(BuildingType.RoverGarage);
            if (!layoutRules || garage == null || !garage.enabled || !garage.powered)
            {
                if (garage == null) { s.roverTrip = 0; s.roverCargo = ""; }
                return;
            }
            if (s.roverCargo == "") s.roverCargo = PickCargo(s, garage);
            if (s.roverCargo == "") return;
            bool water = s.roverCargo == "water";
            double trip = TripHours(garage, water);
            var rules = balance.layout;
            double perHour = (water ? rules.waterPerTrip : rules.soilPerTrip) / trip;
            if (water) r.waterPerHour += perHour;
            else r.soilPerHour = perHour;

            s.roverTrip += dt / trip;
            if (s.roverTrip < 1 - Eps) return;
            s.roverTrip = 0;
            if (water) s.waterL += rules.waterPerTrip;
            else s.soil = Math.Min(rules.soilStockCap, s.soil + rules.soilPerTrip);
            s.roverCargo = PickCargo(s, garage);
        }

        string PickCargo(SimState s, PlacedBuilding garage)
        {
            bool soilReach = !double.IsInfinity(TripHours(garage, false));
            bool waterReach = !double.IsInfinity(TripHours(garage, true));
            if (soilReach && s.soil + 1e-6 < Math.Min(balance.layout.soilStockCap, SoilNeeded(s))) return "soil";
            if (waterReach) return "water";
            return soilReach && s.soil + 1e-6 < balance.layout.soilStockCap ? "soil" : "";
        }

        void StepLife(SimState s, StepReport r, double dt)
        {
            bool lifeSupport = s.AnyPowered(BuildingType.LifeSupport);
            double airBonus = 0, plantWater = 0, food = 0;
            foreach (var b in s.buildings)
            {
                var def = balance.Def(b.type);
                if (def.foodPerHour <= 0 || !b.enabled || !b.powered) continue;
                if (s.waterL <= 0) continue;
                airBonus += def.airBonusPerHour;
                plantWater += def.waterInLph - def.waterReturnLph;
                food += def.foodPerHour;
            }

            double refill = balance.airRefillPerHour * (ShortDuct(s) ? 1 : balance.layout.longDuctAirFactor);
            double airRate = (lifeSupport ? refill : -balance.airDrainPerHour) + airBonus;
            double air = Empty(Math.Min(balance.airCapHours, s.airHours + airRate * dt));
            r.airPerHour = (air - s.airHours) / dt;
            s.airHours = air;
            s.minAirHours = Math.Min(s.minAirHours, air);

            // Rover water arrives in whole trips (StepRover); the report shows its average rate on top.
            double roverAverage = r.waterPerHour;
            double water = Empty(s.waterL - (plantWater + balance.CrewWaterPerHour) * dt);
            r.waterPerHour = (water - s.waterL) / dt + roverAverage;
            s.waterL = water;

            double meals = Empty(s.meals + (food - balance.CrewMealsPerHour) * dt);
            r.foodPerHour = (meals - s.meals) / dt;
            s.meals = meals;
            s.mealsHarvested += food * dt;

            if (!lifeSupport) Warn(r, "air_dropping", "Air is dropping: " + LifeSupportProblem(s));
            if (s.Count(BuildingType.Greenhouse) > 0 && s.waterL <= 0) Warn(r, "water_empty", "Water is gone: the Greenhouse cannot grow food.");
        }

        // Reserves never go negative, and a rounding crumb counts as empty.
        static double Empty(double value) => value < Eps ? 0 : value;

        public string LifeSupportProblem(SimState s)
        {
            string name = balance.Def(BuildingType.LifeSupport).displayName;
            if (s.Count(BuildingType.LifeSupport) == 0) return "there is no " + name + " yet.";
            var ls = s.First(BuildingType.LifeSupport);
            if (ls.broken) return name + " is broken.";
            return name + " has no power.";
        }

        void StepSafety(SimState s, StepReport r, double dt)
        {
            double radiation = RadiationIntensity(s);
            bool sheltered = s.AnyPowered(BuildingType.Shelter);
            bool outside = s.geologist != CrewLocation.Inside;
            r.radiation = radiation;
            r.sheltered = sheltered;

            double radOutside = outside ? balance.radiationOutsidePerHour * radiation : 0;
            double exposure = !sheltered ? 1 : ShelterClose(s) ? 0 : balance.layout.farShelterExposure;
            double radInside = balance.radiationUnshelteredPerHour * radiation * exposure;
            double harm = radOutside + radInside;

            double rate = harm > 0 ? -harm : balance.safetyRecoverPerHour;
            double safety = Math.Max(0, Math.Min(100, s.safety + rate * dt));
            r.safetyPerHour = (safety - s.safety) / dt;
            s.safety = safety;
            s.minSafety = Math.Min(s.minSafety, s.safety);
            s.damageOutside += radOutside * dt;
            s.damageNoShelter += radInside * dt;

            if (s.StormActive)
            {
                s.minSafetyStorm = Math.Min(s.minSafetyStorm, s.safety);
                if (sheltered) s.shelterHours += dt;
                var greenhouse = s.First(BuildingType.Greenhouse);
                if (radiation > 0 && greenhouse != null && !greenhouse.enabled) s.greenhouseOffInStorm = true;
            }

            if (radOutside > 0) Warn(r, "radiation_outside", geologistName + " is outside in the particle storm.");
            if (radInside > 0) Warn(r, "radiation_noshelter", s.Count(BuildingType.Shelter) == 0
                ? "Particle storm: the crew has no Shelter."
                : sheltered ? "Particle storm: the Shelter is far from the Habitat."
                : "Particle storm: the Shelter is not switched on or has no power.");

            s.lowSafetyHours = s.safety < balance.safetyFailThreshold ? s.lowSafetyHours + dt : 0;
            if (s.safety < balance.safetyFailThreshold) Warn(r, "safety_low", "Crew safety is low. Get everyone inside a powered Shelter.");
        }

        // Fixed order decides which cause is named when several happen on the same step. The others are recorded.
        void CheckFailure(SimState s)
        {
            var causes = new List<FailReason>();
            if (s.airHours <= 0) causes.Add(FailReason.NoAir);
            if (s.waterL <= 0) causes.Add(FailReason.NoWater);
            if (s.meals <= 0) causes.Add(FailReason.NoFood);
            if (s.criticalOutHours >= balance.powerLossToleranceHours - Eps) causes.Add(FailReason.PowerLoss);
            if (s.lowSafetyHours >= balance.safetyFailHours - Eps) causes.Add(RadiationCause(s));
            if (causes.Count == 0) return;
            EndRun(s, MissionEnd.Failed, causes[0]);
            for (int i = 1; i < causes.Count; i++) s.otherHazards.Add(causes[i]);
            if (s.report.radiation > 0 && !causes.Contains(RadiationCause(s)) && s.report.safetyPerHour < 0) s.otherHazards.Add(RadiationCause(s));
        }

        FailReason RadiationCause(SimState s)
        {
            bool outsideNow = s.geologist != CrewLocation.Inside && s.report.radiation > 0;
            if (outsideNow) return FailReason.RadiationOutside;
            if (s.report.radiation > 0) return FailReason.RadiationNoShelter;
            return s.damageOutside > s.damageNoShelter ? FailReason.RadiationOutside : FailReason.RadiationNoShelter;
        }

        void EndRun(SimState s, MissionEnd end, FailReason reason)
        {
            s.end = end;
            s.failReason = reason;
            s.endHour = s.timeHours;
            if (end == MissionEnd.Deadline) Log(s, "evaluation", "Time is up. Mission control inspects the base.");
            else Log(s, "failed", "Mission stopped: " + reason + ".");
        }

        void UpdateHarvest(SimState s)
        {
            if (s.harvestDone || s.mealsHarvested < balance.harvestMeals - Eps) return;
            s.harvestDone = true;
            s.supplies += balance.harvestSupplyReward;
            if (s.shelterUnlocked)
            {
                Log(s, "first_harvest", "First harvest! A lander brings " + balance.harvestSupplyReward + " supplies.");
                return;
            }
            s.shelterUnlocked = true;
            Log(s, "first_harvest", "First harvest! +" + balance.harvestSupplyReward + " supplies. Shelter unlocked.");
        }

        public static readonly BuildingType[] CoreTypes =
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

        public static List<BuildingType> MissingCore(SimState s)
        {
            var missing = new List<BuildingType>();
            foreach (var type in CoreTypes)
                if (s.Count(type) == 0) missing.Add(type);
            return missing;
        }

        void Log(SimState s, string code, string text)
        {
            if (previewing) return;
            s.events.Add(new SimEvent { timeHours = s.timeHours, code = code, text = text });
        }

        static void Warn(StepReport r, string code, string text)
        {
            r.warnings.Add(new SimWarning { code = code, text = text });
        }
    }
}
