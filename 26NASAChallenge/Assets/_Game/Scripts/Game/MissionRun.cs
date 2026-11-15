using System;
using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Sim;

namespace Mapmakers.Game
{
    public enum AlertLevel { Info, Warning, Danger }
    public enum RunPhase { Briefing, Running, Ended }

    public sealed class Alert
    {
        public AlertLevel level;
        public string icon = "alert";
        public string text;
        public string actionLabel;
        public Action action;
        public bool actionEnabled = true;
    }

    // One playable level or Free Build run. No UI: views read it and call its commands.
    // Real time: a MissionClock turns real seconds into fixed simulation steps; Tick is the only thing that advances time.
    public sealed class MissionRun
    {
        public static readonly BuildingType[] AllTypes =
        {
            BuildingType.Habitat, BuildingType.SolarArray, BuildingType.Battery, BuildingType.LifeSupport,
            BuildingType.Communications, BuildingType.Greenhouse, BuildingType.RoverGarage, BuildingType.Shelter
        };

        public readonly GameMode mode;
        public readonly LevelDef level;
        public readonly FreeSettings free;
        public readonly Simulator sim;
        public readonly StoryData story;
        public readonly string[] goals;
        public readonly List<BuildingType> buildable = new List<BuildingType>();
        public readonly MissionClock clock;

        public SimState State { get; private set; }
        public RunPhase Phase { get; private set; }
        public BuildingType? BuildChoice { get; private set; }
        public int SelectedId { get; private set; }
        public int Revision { get; private set; }
        public string Hint { get; private set; } = "";
        public string HintId { get; private set; } = "";
        public bool HintsOn = Progress.HintsOn;
        readonly HashSet<string> seenHints = new HashSet<string>();
        readonly List<SimEvent> newEvents = new List<SimEvent>();
        MissionResult result;

        public Training Training { get; private set; }
        public bool IsDrawing => level != null && level.IsDrawing;
        public bool IsTraining => Training != null;
        public bool Ended => Phase == RunPhase.Ended;
        public bool Running => Phase == RunPhase.Running && !IsDrawing;
        public PlanetProfile Planet => sim.planet;
        public string Title => mode == GameMode.Free ? "Free Build · " + Planet.name : level.title;
        public string Subtitle => mode == GameMode.Free ? "Map #" + free.seed + " · " + free.difficulty : level.subtitle;
        public double Duration => sim.DurationHours;
        public double TimeLeft => Math.Max(0, Duration - State.timeHours);

        MissionRun(GameMode mode, LevelDef level, FreeSettings free, Simulator sim, string[] goals)
        {
            this.mode = mode;
            this.level = level;
            this.free = free;
            this.sim = sim;
            this.goals = goals;
            story = StoryData.Load();
            clock = new MissionClock(sim.balance.dtHours, sim.balance.realSecondsPerGameHour, sim.balance.maxRealSecondsPerFrame);
            State = sim.NewGame();
            Phase = IsDrawing ? RunPhase.Running : RunPhase.Briefing;
        }

        public static MissionRun ForLevel(LevelDef level)
        {
            var sim = LevelSetup.CreateSimulator(level);
            sim.geologistName = StoryData.Load().Crew("geologist").name;
            var run = new MissionRun(GameMode.Story, level, null, sim, level.goals);
            if (level.IsDrawing)
                foreach (var name in level.build) run.buildable.Add((BuildingType)Enum.Parse(typeof(BuildingType), name));
            else run.buildable.AddRange(AllTypes);
            if (!string.IsNullOrEmpty(level.startBase))
            {
                run.UsedStarterBase = !LevelSetup.LoadBase(run.sim, run.State, Progress.LoadBase(level.startBase));
            }
            if (level.IsTraining) run.Training = new Training(HelpData.Load().training);
            run.ShowHint(level.IsDrawing ? "h_build" : "h_habitat");
            return run;
        }

        public static MissionRun ForFree(FreeSettings settings)
        {
            var sim = LevelSetup.FreeBuild(settings.planet, settings.seed, settings.difficulty, settings.hours);
            var run = new MissionRun(GameMode.Free, null, settings, sim, FreeSettings.Goals);
            run.buildable.AddRange(AllTypes);
            run.ShowHint("h_habitat");
            return run;
        }

        public bool UsedStarterBase { get; private set; }

        void Changed() => Revision++;

        // ---------- time ----------

        // Ends the briefing. Time stays stopped until the player presses Play.
        public void StartMission()
        {
            if (Phase != RunPhase.Briefing) return;
            Phase = RunPhase.Running;
            clock.Start();
            clock.SetPaused(true);
            ShowHint("h_clock");
            Changed();
        }

        // Called every frame with real (unscaled) seconds. Returns true when anything visible may have changed.
        public bool Tick(double realSeconds)
        {
            if (!Running) return false;
            int steps = clock.Advance(realSeconds);
            if (steps == 0) return false;
            int before = State.events.Count;
            for (int i = 0; i < steps && !State.Finished; i++) State = sim.Step(State);
            for (int i = before; i < State.events.Count; i++) newEvents.Add(State.events[i]);
            if (State.events.Count != before) Changed();
            if (State.Finished) End();
            return true;
        }

        // Events since the last call (story banners, popups, sounds).
        public List<SimEvent> TakeEvents()
        {
            var list = new List<SimEvent>(newEvents);
            newEvents.Clear();
            return list;
        }

        // ---------- queries ----------

        // The base as laid out right now. While paused it still follows every building the player adds, removes or switches.
        public SimState Preview => sim.PreviewState(State);
        public StepReport Power => Preview.report;

        public bool IsPowered(PlacedBuilding b, SimState preview = null)
        {
            var p = (preview ?? Preview).Find(b.id);
            return p != null && p.powered;
        }

        public bool GoalMet(string id) => Goals.IsMet(id, sim, State);
        public int GoalsMet => Goals.CountMet(goals, sim, State);

        public string CurrentGoal
        {
            get
            {
                foreach (var id in goals)
                    if (id != Goals.SafeToEnd && !GoalMet(id)) return LevelCatalog.Load().GoalLabel(id);
                return IsDrawing ? "Your drawing is ready!" : LevelCatalog.Load().GoalLabel(Goals.SafeToEnd);
            }
        }

        public bool CanBuild(BuildingType type, out string why)
        {
            why = "";
            var def = sim.balance.Def(type);
            if (Ended) { why = "The mission has ended."; return false; }
            if (!buildable.Contains(type)) { why = "Not in this level."; return false; }
            if (IsTraining && !Training.Allows(type)) { why = "Your crew will show you this soon."; return false; }
            if (!Placement.IsUnlocked(State, type)) { why = "Unlocks after the first harvest."; return false; }
            int max = Placement.MaxCount(sim, State, type);
            if (State.Count(type) >= max) { why = type == BuildingType.Battery && max == 1 ? "A second one unlocks later." : "All built."; return false; }
            if (State.supplies < def.cost) { why = "Need " + def.cost + " supplies."; return false; }
            if (sim.layoutRules && def.soilCost > State.soil + 1e-6) { why = "Needs " + def.soilCost + " soil. Build a Rover Garage."; return false; }
            if (sim.layoutRules && type != BuildingType.Habitat && State.Count(BuildingType.Habitat) == 0) { why = "Place the Habitat first."; return false; }
            return true;
        }

        public string CountText(BuildingType type)
        {
            int max = Placement.MaxCount(sim, State, type);
            return max > 1 ? State.Count(type) + "/" + max : "";
        }

        public PlacedBuilding Selected => State.Find(SelectedId);

        public bool DrawingReady => IsDrawing && GoalsMet == goals.Length;

        // Hours until a reserve is empty at the current rate (infinite when it is not falling).
        public static double HoursLeft(double amount, double perHour) => perHour < -1e-6 ? amount / -perHour : double.PositiveInfinity;

        public List<Alert> Alerts()
        {
            var list = new List<Alert>();
            if (IsDrawing || Ended) return list;
            var s = State;
            var r = s.step > 0 ? Power : s.report;
            var b = sim.balance;
            string geo = sim.geologistName;

            if (s.geologist == CrewLocation.Outside && !s.recallIssued)
            {
                bool can = sim.CanRecall(s, out string why);
                list.Add(new Alert
                {
                    level = AlertLevel.Danger, icon = "crew", text = can ? geo + " is outside. Call her in!" : geo + " is outside. " + why,
                    actionLabel = "Call " + geo, action = () => Recall(), actionEnabled = can
                });
            }
            if (s.StormActive && !ShelterOn())
                list.Add(new Alert { level = AlertLevel.Danger, icon = "Shelter", text = s.Count(BuildingType.Shelter) == 0 ? "Build a Shelter for the storm." : "Turn on the Shelter.",
                    actionLabel = s.Count(BuildingType.Shelter) == 0 ? "Build" : "Turn on", action = ShelterAction });

            AddReserve(list, "air", "Air", s.airHours, r.airPerHour, s.Count(BuildingType.LifeSupport) == 0 ? "Build " + sim.balance.Def(BuildingType.LifeSupport).displayName + "." : sim.LifeSupportProblem(s), null, null);
            AddReserve(list, "water", "Water", s.waterL, r.waterPerHour, WaterTip(), GreenhouseOn() ? "Rest Greenhouse" : null, GreenhouseOn() ? (Action)RestGreenhouse : null);
            AddReserve(list, "food", "Food", s.meals, r.foodPerHour, FoodTip(), null, null);

            if (r.unmetCriticalKW > 0)
            {
                double left = b.powerLossToleranceHours - s.criticalOutHours;
                list.Add(new Alert { level = AlertLevel.Danger, icon = "power",
                    text = NoPowerText() + (sim.canFail ? " Mission stops in " + SimText.Duration(left) + "." : ""),
                    actionLabel = GreenhouseOn() ? "Rest Greenhouse" : null, action = GreenhouseOn() ? (Action)RestGreenhouse : null });
            }
            else if (r.Has("loads_off"))
                list.Add(new Alert { level = AlertLevel.Warning, icon = "power", text = "Not enough power. Some systems switched off.",
                    actionLabel = GreenhouseOn() ? "Rest Greenhouse" : null, action = GreenhouseOn() ? (Action)RestGreenhouse : null });
            else if (r.batteryFlowKW < -1e-6)
            {
                double hours = s.batteryKWh / -r.batteryFlowKW;
                if (hours < b.warnHours)
                    list.Add(new Alert { level = AlertLevel.Warning, icon = "battery", text = "Battery empty in " + SimText.Duration(hours) + ". Use less power.",
                        actionLabel = GreenhouseOn() ? "Rest Greenhouse" : null, action = GreenhouseOn() ? (Action)RestGreenhouse : null });
            }

            foreach (var x in s.buildings)
            {
                if (!x.broken) continue;
                int id = x.id;
                list.Add(new Alert { level = AlertLevel.Warning, icon = "SolarArray", text = "A Solar Array is broken.", actionLabel = "Fix (" + b.repairCost + ")",
                    action = () => Repair(id), actionEnabled = s.supplies >= b.repairCost });
                break;
            }
            if (s.shadowFactor < 1)
                list.Add(new Alert { level = AlertLevel.Info, icon = "power", text = Planet.dimmingName + ": less sun for " + SimText.Duration(s.shadowEndsAt - s.timeHours) + "." });
            if (list.Count > 3) list.RemoveRange(3, list.Count - 3);
            return list;
        }

        // Names of critical buildings (Habitat, Air Support) that are built, switched on and have no power right now.
        public List<string> UnpoweredCritical()
        {
            var names = new List<string>();
            var preview = Preview;
            foreach (var b in State.buildings)
            {
                var def = sim.balance.Def(b.type);
                if (def.priority <= 1 && def.demandKW > 0 && b.enabled && !IsPowered(b, preview) && !names.Contains(def.displayName)) names.Add(def.displayName);
            }
            return names;
        }

        public string NoPowerText()
        {
            var names = UnpoweredCritical();
            if (names.Count == 0) return "Critical systems have no power!";
            return string.Join(" and ", names) + (names.Count > 1 ? " have" : " has") + " no power!";
        }

        // The order a new commander usually builds a base in. Used for "Next:" tips.
        static readonly BuildingType[] BuildOrder =
        {
            BuildingType.Habitat, BuildingType.LifeSupport, BuildingType.SolarArray, BuildingType.Battery,
            BuildingType.Communications, BuildingType.RoverGarage, BuildingType.Greenhouse
        };

        public BuildingType? NextToBuild()
        {
            foreach (var type in BuildOrder)
                if (buildable.Contains(type) && State.Count(type) == 0) return type;
            return null;
        }

        void AddReserve(List<Alert> list, string icon, string name, double amount, double rate, string tip, string label, Action action)
        {
            double hours = HoursLeft(amount, rate);
            if (hours > sim.balance.warnHours) return;
            bool critical = hours <= sim.balance.criticalHours;
            list.Add(new Alert
            {
                level = critical ? AlertLevel.Danger : AlertLevel.Warning, icon = icon,
                text = name + " runs out in " + SimText.Duration(hours) + ". " + tip,
                actionLabel = label, action = action
            });
        }

        string WaterTip()
        {
            if (State.Count(BuildingType.RoverGarage) == 0) return "A rover can fetch " + Planet.waterSource + ".";
            return GreenhouseOn() ? "The Greenhouse drinks the most." : "Keep the rover working.";
        }

        public string FoodTip()
        {
            var g = State.First(BuildingType.Greenhouse);
            if (g == null) return State.soil < sim.balance.Def(BuildingType.Greenhouse).soilCost ? "Get soil, then build a Greenhouse." : "Build a Greenhouse.";
            if (!g.enabled) return "Turn the Greenhouse back on.";
            if (!IsPowered(g)) return "The Greenhouse needs power.";
            if (State.waterL <= 0) return "The Greenhouse needs water.";
            return "The crew eats more than it grows.";
        }

        bool GreenhouseOn()
        {
            var g = State.First(BuildingType.Greenhouse);
            return g != null && g.enabled;
        }

        void RestGreenhouse()
        {
            var g = State.First(BuildingType.Greenhouse);
            if (g != null && g.enabled) Toggle(g.id);
        }

        bool ShelterOn()
        {
            var shelter = State.First(BuildingType.Shelter);
            return shelter != null && shelter.enabled;
        }

        void ShelterAction()
        {
            var shelter = State.First(BuildingType.Shelter);
            if (shelter != null) { Toggle(shelter.id); return; }
            Choose(BuildingType.Shelter);
        }

        // ---------- commands (refused after the end) ----------

        public void Choose(BuildingType? type)
        {
            if (Ended) return;
            if (type.HasValue && !CanBuild(type.Value, out _)) return;
            BuildChoice = type;
            if (type.HasValue) SelectedId = 0;
            if (type == BuildingType.SolarArray) ShowHint("h_sun");
            else if (type == BuildingType.Battery) ShowHint("h_cable");
            else if (type == BuildingType.RoverGarage) ShowHint("h_rover");
            Changed();
        }

        public PlacementResult Place(int x, int y)
        {
            if (!BuildChoice.HasValue || Ended) return PlacementResult.Fail(PlacementReason.Locked, "Pick a building first.");
            var type = BuildChoice.Value;
            var placed = sim.Place(State, type, x, y);
            if (!placed.Ok) return placed;
            if (!CanBuild(type, out _)) BuildChoice = null;
            if (IsDrawing && State.Count(BuildingType.Greenhouse) > 0 && !Goals.PowerPositive(sim, State)) ShowHint("h_power", true);
            else if (!IsDrawing) ShowNextHint(type);
            Changed();
            return placed;
        }

        public void ClickTile(int x, int y)
        {
            if (BuildChoice.HasValue)
            {
                Place(x, y);
                return;
            }
            var b = Placement.BuildingAt(sim.balance, State, x, y);
            SelectedId = b == null ? 0 : b.id;
            Changed();
        }

        public void Deselect()
        {
            SelectedId = 0;
            BuildChoice = null;
            Changed();
        }

        public bool Toggle(int id)
        {
            if (IsDrawing || Ended) return false;
            var b = State.Find(id);
            if (b == null) return false;
            bool ok = sim.SetEnabled(State, id, !b.enabled);
            if (ok) ShowHint("h_toggle");
            Changed();
            return ok;
        }

        public bool Repair(int id)
        {
            bool ok = !Ended && sim.Repair(State, id);
            Changed();
            return ok;
        }

        public bool Remove(int id)
        {
            bool ok = !Ended && sim.Remove(State, id);
            if (ok && SelectedId == id) SelectedId = 0;
            Changed();
            return ok;
        }

        public bool Recall()
        {
            bool ok = !Ended && sim.RecallGeologist(State);
            Changed();
            return ok;
        }

        // After a placement: warn first if power out beats power in, otherwise point at the next useful building.
        void ShowNextHint(BuildingType placed)
        {
            var power = Power;
            if (power.demandKW > power.generationKW + 1e-6 && State.Count(BuildingType.SolarArray) > 0)
            {
                ShowHint("n_power");
                return;
            }
            var next = NextToBuild();
            if (next.HasValue) ShowHint("n_" + next.Value, placed == BuildingType.Habitat);
            else ShowHint("h_info");
        }

        public void DismissHint()
        {
            Hint = "";
            HintId = "";
            Changed();
        }

        void ShowHint(string id, bool evenIfSeen = false)
        {
            if (IsTraining) return;
            if (!seenHints.Add(id) && !evenIfSeen) return;
            Hint = story.Hint(id);
            HintId = id;
        }

        // ---------- Tutorial 2 ----------

        // Checked every frame, also while paused, because many steps are orders given with time stopped.
        public void UpdateTraining()
        {
            if (!IsTraining || Ended || Phase == RunPhase.Briefing) return;
            int before = State.events.Count;
            if (!Training.Advance(this)) return;
            if (BuildChoice.HasValue && Training.Current?.target != "card:" + BuildChoice.Value) BuildChoice = null;
            for (int i = before; i < State.events.Count; i++) newEvents.Add(State.events[i]);
            Changed();
        }

        public void TrainingAction(string action)
        {
            if (!IsTraining) return;
            Training.Mark(action);
            UpdateTraining();
        }

        // ---------- ending ----------

        // The drawing ends when the player presses Finish.
        public MissionResult FinishDrawing()
        {
            if (!IsDrawing || !DrawingReady) return null;
            return End();
        }

        // Freezes the run and captures the result exactly once. Later calls return the same result.
        MissionResult End()
        {
            if (result != null) return result;
            Phase = RunPhase.Ended;
            BuildChoice = null;
            clock.SetPaused(true);
            result = IsDrawing ? DrawingResult() : MissionResult.FromState(this);
            if (level != null && level.saveBase && result.canContinue) Progress.SaveBase(level.id, LevelSetup.Save(State), result.score.total);
            Changed();
            return result;
        }

        public MissionResult Result => result;

        MissionResult DrawingResult()
        {
            var power = Power;
            double sun = 0;
            int panels = 0;
            foreach (var b in State.buildings)
            {
                if (b.type != BuildingType.SolarArray) continue;
                sun += sim.SunFor(State, b);
                panels++;
            }
            var score = Grader.Drawing(power.generationKW, power.demandKW, panels == 0 ? 0 : sun / panels, GoalsMet, goals.Length);
            var r = new MissionResult
            {
                mode = mode, levelId = level.id, title = Title, end = MissionEnd.Deadline, score = score,
                canContinue = score.goalsComplete, evaluation = true
            };
            r.notes.Add(story.Story("p_locked"));
            r.science = story.Hint("h_power");
            return r;
        }
    }
}
