using System;
using System.Collections.Generic;

namespace Mapmakers.Sim
{
    // Scripted players used to test and tune the balance. Pure C#, no UI. They act every few game hours.
    public abstract class Bot
    {
        public abstract string Name { get; }
        public abstract void Act(Simulator sim, SimState s);
        public virtual bool Careful => true;

        protected static readonly BuildingType[] BuildOrder =
        {
            BuildingType.Habitat, BuildingType.SolarArray, BuildingType.Battery, BuildingType.LifeSupport, BuildingType.SolarArray,
            BuildingType.RoverGarage, BuildingType.Communications, BuildingType.SolarArray, BuildingType.Greenhouse, BuildingType.Shelter,
            BuildingType.SolarArray, BuildingType.SolarArray, BuildingType.SolarArray, BuildingType.SolarArray, BuildingType.SolarArray
        };

        protected static void Switch(Simulator sim, SimState s, BuildingType type, bool on)
        {
            var b = s.First(type);
            if (b != null && b.enabled != on) sim.SetEnabled(s, b.id, on);
        }

        // Works through the build order as supplies and soil allow, then repairs.
        protected void BuildBase(Simulator sim, SimState s)
        {
            var seen = new Dictionary<BuildingType, int>();
            foreach (var type in BuildOrder)
            {
                seen.TryGetValue(type, out int n);
                seen[type] = n + 1;
                if (s.Count(type) >= seen[type]) continue;
                if (s.Count(type) >= Placement.MaxCount(sim, s, type) || !Placement.IsUnlocked(s, type)) continue;
                if (Planner.Place(sim, s, type, Careful)) continue;
                if (sim.balance.Def(type).cost > s.supplies) return;
            }
            foreach (var b in s.buildings.ToArray())
                if (b.broken) sim.Repair(s, b.id);
        }

        protected void ProtectCrew(Simulator sim, SimState s)
        {
            if (s.stormPhase == StormPhase.None) return;
            if (s.stormPhase == StormPhase.Over)
            {
                Switch(sim, s, BuildingType.Shelter, false);
                return;
            }
            if (s.geologist == CrewLocation.Outside) sim.RecallGeologist(s);
            if (s.Count(BuildingType.Shelter) == 0) Planner.Place(sim, s, BuildingType.Shelter, Careful);
            Switch(sim, s, BuildingType.Shelter, true);
        }

        protected void SecondBattery(Simulator sim, SimState s)
        {
            if (s.Count(BuildingType.Battery) < s.maxBatteries) Planner.Place(sim, s, BuildingType.Battery, Careful);
        }

        protected static bool Particles(SimState s) => s.stormPhase == StormPhase.Particles || s.stormPhase == StormPhase.Decay;
    }

    // Picks building spots. Careful = sunny ground, short cables and pipes, rover close to soil and water.
    // Careless = the free spot closest to the Habitat, whatever it is.
    public static class Planner
    {
        public static bool Place(Simulator sim, SimState s, BuildingType type, bool careful)
        {
            int bestX = -1, bestY = -1;
            double best = double.NegativeInfinity;
            for (int y = 0; y < sim.terrain.height; y++)
            {
                for (int x = 0; x < sim.terrain.width; x++)
                {
                    if (!sim.CanPlace(s, type, x, y).Ok) continue;
                    double value = careful ? Value(sim, s, type, x, y) : -Near(sim, s, type, x, y);
                    if (value <= best) continue;
                    best = value;
                    bestX = x;
                    bestY = y;
                }
            }
            return bestX >= 0 && sim.Place(s, type, bestX, bestY).Ok;
        }

        static double Near(Simulator sim, SimState s, BuildingType type, int x, int y)
        {
            if (type == BuildingType.Habitat) return Math.Abs(x - sim.terrain.width / 2 + 1) + Math.Abs(y - sim.terrain.height / 2 + 1);
            var def = sim.balance.Def(type);
            return Layout.GapTo(sim.balance, s, BuildingType.Habitat, x, y, def.width, def.height) * 100 + x + y * 0.1;
        }

        static double Value(Simulator sim, SimState s, BuildingType type, int x, int y)
        {
            var b = sim.balance;
            var def = b.Def(type);
            int w = def.width, h = def.height;
            double value = 0;
            foreach (var link in Layout.Checks(b, sim.terrain, s, type, x, y))
                if (link.met) value += 5;
            bool hugsHabitat = Layout.GapTo(b, s, BuildingType.Habitat, x, y, w, h) <= 1;
            if (hugsHabitat && (type == BuildingType.SolarArray || type == BuildingType.Battery || type == BuildingType.RoverGarage)) value -= 6;
            switch (type)
            {
                case BuildingType.Habitat: return HabitatValue(sim, x, y);
                case BuildingType.SolarArray: return value + 20 * sim.terrain.AverageSun(x, y, w, h);
                case BuildingType.Battery:
                    int panels = 0;
                    foreach (var p in s.buildings)
                        if (p.type == BuildingType.SolarArray && Layout.Gap(x, y, w, h, p.x, p.y, 2, 2) <= b.layout.cableGap) panels++;
                    return value + 3 * panels - 2 * sim.terrain.AverageSun(x, y, w, h);
                case BuildingType.RoverGarage:
                    double trips = Layout.SoilGap(sim.terrain, x, y, w, h) + Layout.WaterGap(sim.terrain, x, y, w, h);
                    return value - trips;
                default:
                    return value - 3 * sim.terrain.AverageSun(x, y, w, h) - 0.1 * Layout.GapTo(b, s, BuildingType.Habitat, x, y, w, h);
            }
        }

        // A good Habitat spot keeps sunny ground, soil and water inside the base area.
        static double HabitatValue(Simulator sim, int x, int y)
        {
            var t = sim.terrain;
            int radius = sim.balance.layout.baseRadius;
            double bright = 0, soil = 0, top = 0;
            for (int ty = 0; ty < t.height; ty++)
                for (int tx = 0; tx < t.width; tx++) top = Math.Max(top, t.Sun(tx, ty));
            for (int ty = 0; ty < t.height; ty++)
                for (int tx = 0; tx < t.width; tx++)
                {
                    if (t.IsBlocked(tx, ty) || Layout.Gap(x, y, 3, 3, tx, ty, 1, 1) > radius - 1) continue;
                    if (t.Sun(tx, ty) >= top - 0.08) bright++;
                    if (t.IsSoil(tx, ty)) soil++;
                }
            int reach = radius + sim.balance.layout.roverRange - 2;
            int water = Layout.WaterGap(t, x, y, 3, 3);
            int soilGap = Layout.SoilGap(t, x, y, 3, 3);
            return Math.Min(bright, 16) + Math.Min(soil, 4) * 2 - Math.Max(0, water - radius) * 3 - (soilGap > reach ? 40 : 0);
        }
    }

    public sealed class IdleBot : Bot
    {
        public override string Name => "Idle";
        public override void Act(Simulator sim, SimState s) { }
    }

    // Builds the base, then ignores every event.
    public sealed class DoNothingBot : Bot
    {
        public override string Name => "DoNothing";
        public override void Act(Simulator sim, SimState s) => BuildBase(sim, s);
    }

    // Protects the crew and rests the Greenhouse in the particle storm (unless food is low).
    public sealed class ShedLoadsBot : Bot
    {
        public override string Name => "ShedLoads";
        public override void Act(Simulator sim, SimState s)
        {
            BuildBase(sim, s);
            ProtectCrew(sim, s);
            Switch(sim, s, BuildingType.Greenhouse, !(Particles(s) || s.shadowFactor < 1) || s.meals < 8);
        }
    }

    // Protects the crew and keeps everything on, using a second Battery as soon as it is allowed.
    public sealed class StoredEnergyBot : Bot
    {
        public override string Name => "StoredEnergy";
        public override void Act(Simulator sim, SimState s)
        {
            BuildBase(sim, s);
            SecondBattery(sim, s);
            ProtectCrew(sim, s);
        }
    }

    // A careful child: second Battery, protects the crew, rests the Greenhouse only while the battery is low and draining.
    public sealed class SmartBot : Bot
    {
        public override string Name => "Smart";
        public override void Act(Simulator sim, SimState s)
        {
            BuildBase(sim, s);
            SecondBattery(sim, s);
            ProtectCrew(sim, s);
            double cap = sim.BatteryCapacity(s);
            bool low = cap > 0 && s.batteryKWh < 0.4 * cap && s.report.batteryFlowKW < 0;
            bool full = cap > 0 && s.batteryKWh > 0.8 * cap;
            bool hungry = s.meals < 8;
            if ((low || s.report.Has("loads_off")) && !hungry) Switch(sim, s, BuildingType.Greenhouse, false);
            else if (full || cap == 0 || hungry) Switch(sim, s, BuildingType.Greenhouse, true);
        }
    }

    // Protects the crew but keeps every load on with a single Battery (ignores power warnings).
    public sealed class GreedyBot : Bot
    {
        public override string Name => "Greedy";
        public override void Act(Simulator sim, SimState s)
        {
            BuildBase(sim, s);
            ProtectCrew(sim, s);
        }
    }

    // Plays the events well but drops buildings wherever there is room near the Habitat.
    public sealed class CarelessBot : Bot
    {
        public override string Name => "Careless";
        public override bool Careful => false;
        public override void Act(Simulator sim, SimState s)
        {
            BuildBase(sim, s);
            SecondBattery(sim, s);
            ProtectCrew(sim, s);
            Switch(sim, s, BuildingType.Greenhouse, !(Particles(s) || s.shadowFactor < 1) || s.meals < 8);
        }
    }

    // A ready-made base, placed for free. Used when a level starts from a base the player has not saved.
    public static class StarterBase
    {
        public static void Build(Simulator sim, SimState s)
        {
            int supplies = s.supplies;
            double soil = s.soil;
            s.supplies = 1000;
            s.soil = 100;
            var bot = new DoNothingBot();
            for (int i = 0; i < 3; i++) bot.Act(sim, s);
            s.supplies = supplies;
            s.soil = soil;
        }
    }

    public sealed class RunResult
    {
        public string bot;
        public SimState state;
        public LevelScore score;
        public List<string> log = new List<string>();
    }

    public static class ScenarioRunner
    {
        public const double ActEveryHours = 2;

        public static RunResult Run(Simulator sim, Bot bot, IList<string> goals = null, SimState start = null, double maxHours = 400)
        {
            var s = start ?? sim.NewGame();
            double nextAct = 0;
            while (!s.Finished && s.timeHours < maxHours)
            {
                if (s.timeHours >= nextAct - 1e-6)
                {
                    bot.Act(sim, s);
                    nextAct += ActEveryHours;
                }
                s = sim.Step(s);
            }
            goals = goals ?? new string[0];
            var result = new RunResult { bot = bot.Name, state = s, score = Grader.Mission(sim, s, goals) };
            foreach (var e in s.events)
                result.log.Add(SimText.Num(e.timeHours) + "h " + e.code + ": " + e.text);
            return result;
        }
    }
}
