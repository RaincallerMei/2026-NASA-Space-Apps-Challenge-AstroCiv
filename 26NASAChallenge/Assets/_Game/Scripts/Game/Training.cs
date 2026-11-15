using System;
using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Sim;

namespace Mapmakers.Game
{
    // Tutorial 2: the crew walks the player through every action of a real mission, one step at a time.
    // Steps come from Help.json. A step is done when its condition holds; every condition also accepts the other
    // ways the same state can be reached, so the player can never get stuck. Buildings unlock when a step names them.
    public sealed class Training
    {
        readonly TrainingStep[] steps;
        readonly HashSet<string> actions = new HashSet<string>();

        public Training(TrainingStep[] steps) => this.steps = steps;

        public TrainingStep[] Steps => steps;
        public int Index { get; private set; }
        public bool Done => Index >= steps.Length;
        public TrainingStep Current => Done ? null : steps[Index];

        // Things only the screen sees: "chat:<crew id>", "wiki", "next:<step id>".
        public void Mark(string action) => actions.Add(action);

        public bool Allows(BuildingType type)
        {
            for (int i = 0; i <= Index && i < steps.Length; i++)
                if (steps[i].target == "card:" + type) return true;
            return false;
        }

        // Moves past every finished step and starts the next one. Returns true when it moved.
        public bool Advance(MissionRun run)
        {
            bool moved = false;
            while (!Done && IsDone(steps[Index], run))
            {
                Index++;
                moved = true;
                if (!Done) Begin(steps[Index], run);
            }
            return moved;
        }

        bool IsDone(TrainingStep step, MissionRun run)
        {
            var s = run.State;
            var parts = step.done.Split(':');
            string arg = parts.Length > 1 ? parts[1] : "";
            switch (parts[0])
            {
                case "next": return actions.Contains("next:" + step.id);
                case "chat":
                case "wiki": return actions.Contains(step.done);
                case "built": return s.Count(Type(arg)) >= (parts.Length > 2 ? int.Parse(parts[2]) : 1);
                case "power": return Goals.PowerPositive(run.sim, s) || !run.CanBuild(BuildingType.SolarArray, out _);
                case "speed": return run.clock.Speed > 1;
                case "halt": return run.clock.UserPaused;
                case "soil": return s.soil + 1e-6 >= run.sim.balance.Def(BuildingType.Greenhouse).soilCost || s.Count(BuildingType.Greenhouse) > 0;
                case "harvest": return s.harvestDone;
                case "select": return Off(s, arg) || s.First(Type(arg)) == null || run.SelectedId == s.First(Type(arg)).id;
                case "off": return Off(s, arg) || s.First(Type(arg)) == null || s.shadowPassed;
                case "on": return !Off(s, arg);
                case "shadow_over": return s.shadowPassed;
                case "paused": return run.clock.UserPaused || s.recallIssued || s.geologist != CrewLocation.Outside;
                case "recall": return s.recallIssued || s.geologist != CrewLocation.Outside;
                case "playing": return !run.clock.UserPaused;
                case "storm_over": return s.stormsStarted > 0 && s.stormPhase == StormPhase.Over;
                default: return true;
            }
        }

        static bool Off(SimState s, string type)
        {
            var b = s.First(Type(type));
            return b != null && !b.enabled;
        }

        static BuildingType Type(string name) => (BuildingType)Enum.Parse(typeof(BuildingType), name);

        static void Begin(TrainingStep step, MissionRun run)
        {
            if (step.start == "shadow") run.sim.StartShadow(run.State, 0.6, 10);
            else if (step.start == "storm") run.sim.StartStorm(run.State);
        }

        static readonly string[] Waits = { "soil", "harvest", "shadow_over", "storm_over" };

        // One extra sentence when the player is stuck waiting for something that cannot happen yet.
        public string Note(MissionRun run)
        {
            var step = Current;
            if (step == null) return "";
            if (run.clock.UserPaused && Array.IndexOf(Waits, step.done) >= 0) return "Time is paused. Press Play.";
            if (run.State.step == 0) return "";
            var s = run.State;
            if (step.done == "soil")
            {
                var garage = s.First(BuildingType.RoverGarage);
                if (garage == null) return "";
                if (double.IsInfinity(run.sim.TripHours(garage, false))) return "The garage is too far from soil. Remove it and build it nearer brown dots.";
                if (!garage.enabled || !garage.powered) return "The rover has no power. Add a Solar Array.";
            }
            if (step.done == "harvest")
            {
                var g = s.First(BuildingType.Greenhouse);
                if (g == null) return "";
                if (!g.enabled) return "Turn the Greenhouse back on.";
                if (!g.powered) return "The Greenhouse has no power. Add a Solar Array.";
                if (s.waterL <= 0) return "The Greenhouse needs water.";
            }
            return "";
        }

        // {outage}: how long the Habitat may go without power. {mission} and {minutes}: how long the next level lasts,
        // in game time and in real minutes at 1x.
        public static string Fill(string text, MissionRun run)
        {
            text = text.Replace("{outage}", SimText.Duration(run.sim.balance.powerLossToleranceHours));
            var next = run.level == null ? null : LevelCatalog.Load().Next(run.level.id);
            if (next == null || next.durationHours <= 0) return text;
            double minutes = Math.Max(1, Math.Round(next.durationHours * run.sim.balance.realSecondsPerGameHour / 60));
            return text.Replace("{level}", next.Label).Replace("{mission}", SimText.Duration(next.durationHours, run.Planet.dayWord, run.Planet.dayHours))
                .Replace("{minutes}", minutes.ToString());
        }
    }
}
