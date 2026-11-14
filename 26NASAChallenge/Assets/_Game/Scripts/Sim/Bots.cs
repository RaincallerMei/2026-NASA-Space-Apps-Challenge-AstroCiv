using System.Collections.Generic;

namespace Mapmakers.Sim
{
    // Scripted players used to test and tune the balance. Pure C#, no UI.
    public abstract class Bot
    {
        public abstract string Name { get; }
        public abstract void Act(Simulator sim, SimState s);

        protected static PlacedBuilding First(SimState s, BuildingType type)
        {
            foreach (var b in s.buildings)
                if (b.type == type) return b;
            return null;
        }

        protected static void Switch(Simulator sim, SimState s, BuildingType type, bool on)
        {
            var b = First(s, type);
            if (b != null && b.enabled != on) sim.SetEnabled(s, b.id, on);
        }

        // Places one building on the sunniest free site (solar) or the first free site (others).
        protected static bool PlaceBest(Simulator sim, SimState s, BuildingType type)
        {
            var def = sim.balance.Def(type);
            int bestX = -1, bestY = -1;
            double bestSun = -1;
            for (int y = 0; y < sim.terrain.height; y++)
            {
                for (int x = 0; x < sim.terrain.width; x++)
                {
                    if (!sim.CanPlace(s, type, x, y).Ok) continue;
                    double sun = def.generationKW > 0 ? sim.terrain.AverageSun(x, y, def.width, def.height) : 0;
                    if (sun <= bestSun) continue;
                    bestSun = sun;
                    bestX = x;
                    bestY = y;
                }
            }
            return bestX >= 0 && sim.Place(s, type, bestX, bestY).Ok;
        }

        protected static void BuildCoreBase(Simulator sim, SimState s)
        {
            if (s.Count(BuildingType.Habitat) == 0) PlaceBest(sim, s, BuildingType.Habitat);
            while (s.Count(BuildingType.SolarArray) < sim.balance.Def(BuildingType.SolarArray).maxCount)
                if (!PlaceBest(sim, s, BuildingType.SolarArray)) break;
            foreach (var type in new[] { BuildingType.LifeSupport, BuildingType.Battery, BuildingType.Communications, BuildingType.Greenhouse })
                if (s.Count(type) == 0) PlaceBest(sim, s, type);
        }

        protected static void ProtectCrew(Simulator sim, SimState s)
        {
            if (s.stormPhase == StormPhase.None) return;
            if (s.stormPhase == StormPhase.Over)
            {
                Switch(sim, s, BuildingType.Shelter, false);
                return;
            }
            if (s.geologist == CrewLocation.Outside) sim.RecallGeologist(s);
            if (s.Count(BuildingType.Shelter) == 0) PlaceBest(sim, s, BuildingType.Shelter);
            if (s.StormRelHour >= sim.balance.storm.onsetHour - 0.5) Switch(sim, s, BuildingType.Shelter, true);
        }
    }

    // Builds nothing at all.
    public sealed class IdleBot : Bot
    {
        public override string Name => "Idle";
        public override void Act(Simulator sim, SimState s) { }
    }

    // Builds the base, then ignores the storm.
    public sealed class DoNothingBot : Bot
    {
        public override string Name => "DoNothing";
        public override void Act(Simulator sim, SimState s) => BuildCoreBase(sim, s);
    }

    // Protects the crew and switches the Greenhouse off while particles are arriving.
    public sealed class ShedLoadsBot : Bot
    {
        public override string Name => "ShedLoads";
        public override void Act(Simulator sim, SimState s)
        {
            BuildCoreBase(sim, s);
            ProtectCrew(sim, s);
            bool particles = s.stormPhase == StormPhase.Particles || s.stormPhase == StormPhase.Decay;
            if (s.stormPhase != StormPhase.None) Switch(sim, s, BuildingType.Greenhouse, !particles);
        }
    }

    // Protects the crew and keeps everything on, using a second, pre-charged Battery.
    public sealed class StoredEnergyBot : Bot
    {
        public override string Name => "StoredEnergy";
        public override void Act(Simulator sim, SimState s)
        {
            BuildCoreBase(sim, s);
            if (s.Count(BuildingType.Battery) < s.maxBatteries) PlaceBest(sim, s, BuildingType.Battery);
            ProtectCrew(sim, s);
        }
    }

    // Protects the crew but keeps every load on with a single Battery.
    public sealed class GreedyBot : Bot
    {
        public override string Name => "Greedy";
        public override void Act(Simulator sim, SimState s)
        {
            BuildCoreBase(sim, s);
            ProtectCrew(sim, s);
        }
    }

    public sealed class RunResult
    {
        public string bot;
        public SimState state;
        public Outcome outcome;
        public double hours;
        public List<string> log = new List<string>();
    }

    public static class ScenarioRunner
    {
        public static RunResult Run(Simulator sim, Bot bot, double maxHours = 200)
        {
            var s = sim.NewGame();
            long maxSteps = (long)(maxHours / sim.balance.dtHours);
            for (long i = 0; i < maxSteps; i++)
            {
                bot.Act(sim, s);
                s = sim.Step(s);
                if (s.failed || s.stage == MissionStage.Done) break;
            }
            var result = new RunResult { bot = bot.Name, state = s, hours = s.timeHours, outcome = Outcome.Evaluate(sim.balance, s) };
            foreach (var e in s.events)
                result.log.Add(SimText.Num(e.timeHours) + "h " + e.code + ": " + e.text);
            return result;
        }
    }
}
