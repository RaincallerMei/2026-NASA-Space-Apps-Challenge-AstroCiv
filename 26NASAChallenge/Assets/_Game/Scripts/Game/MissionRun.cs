using System;
using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Sim;

namespace Mapmakers.Game
{
    public enum AlertLevel { Info, Warning, Danger }

    public sealed class Alert
    {
        public AlertLevel level;
        public string text;
        public string actionLabel;
        public Action action;
        public bool actionEnabled = true;
    }

    // One playable level or Free Build run. No UI: views read it and call its commands.
    public sealed class MissionRun
    {
        public static readonly BuildingType[] AllTypes =
        {
            BuildingType.Habitat, BuildingType.SolarArray, BuildingType.Battery, BuildingType.LifeSupport,
            BuildingType.Communications, BuildingType.Greenhouse, BuildingType.Shelter
        };

        public readonly GameMode mode;
        public readonly LevelDef level;
        public readonly FreeSettings free;
        public readonly Simulator sim;
        public readonly StoryData story;
        public readonly string[] goals;
        public readonly List<BuildingType> buildable = new List<BuildingType>();

        public SimState State { get; private set; }
        public BuildingType? BuildChoice { get; private set; }
        public int SelectedId { get; private set; }
        public int Revision { get; private set; }
        public string Hint { get; private set; } = "";
        public bool HintsOn = Progress.HintsOn;
        public bool Ended { get; private set; }
        readonly HashSet<string> seenHints = new HashSet<string>();
        SimState forecast;
        int forecastRevision = -1;

        public bool IsDrawing => level != null && level.IsDrawing;
        public PlanetProfile Planet => sim.planet;
        public string Title => mode == GameMode.Free ? "Free Build · " + Planet.name : level.title;
        public string Subtitle => mode == GameMode.Free ? "Map #" + free.seed + " · " + free.difficulty : level.subtitle;
        public int Days => sim.scenario.days;

        MissionRun(GameMode mode, LevelDef level, FreeSettings free, Simulator sim, string[] goals)
        {
            this.mode = mode;
            this.level = level;
            this.free = free;
            this.sim = sim;
            this.goals = goals;
            story = StoryData.Load();
            State = sim.NewGame();
        }

        public static MissionRun ForLevel(LevelDef level)
        {
            var run = new MissionRun(GameMode.Story, level, null, LevelSetup.CreateSimulator(level), level.goals);
            if (level.IsDrawing)
                foreach (var name in level.build) run.buildable.Add((BuildingType)Enum.Parse(typeof(BuildingType), name));
            else run.buildable.AddRange(AllTypes);
            if (!string.IsNullOrEmpty(level.startBase)) LevelSetup.LoadBase(run.sim, run.State, Progress.LoadBase(level.startBase));
            run.ShowHint("h_build");
            return run;
        }

        public static MissionRun ForFree(FreeSettings settings)
        {
            var sim = LevelSetup.FreeBuild(settings.planet, settings.seed, settings.difficulty, settings.days);
            var run = new MissionRun(GameMode.Free, null, settings, sim, new[] { "core_base", "harvest", "recall_early", "shelter_used", "all_days" });
            run.buildable.AddRange(AllTypes);
            run.ShowHint("h_build");
            return run;
        }

        void Changed() => Revision++;

        // ---------- queries ----------

        public SimState Forecast
        {
            get
            {
                if (IsDrawing || State.Finished) return null;
                if (forecastRevision != Revision) { forecast = sim.Forecast(State); forecastRevision = Revision; }
                return forecast;
            }
        }

        public StepReport Power => sim.Preview(State);

        public bool GoalMet(string id) => Goals.IsMet(id, sim, State);
        public int GoalsMet => Goals.CountMet(goals, sim, State);

        public string CurrentGoal
        {
            get
            {
                foreach (var id in goals)
                    if (id != "all_days" && !GoalMet(id)) return LevelCatalog.Load().GoalLabel(id);
                return IsDrawing ? "Your drawing is ready!" : LevelCatalog.Load().GoalLabel("all_days");
            }
        }

        public bool CanBuild(BuildingType type, out string why)
        {
            why = "";
            var def = sim.balance.Def(type);
            if (!buildable.Contains(type)) { why = "Not in this level."; return false; }
            if (!Placement.IsUnlocked(State, type)) { why = "Unlocks after the first harvest."; return false; }
            int max = Placement.MaxCount(sim.balance, State, type);
            if (State.Count(type) >= max) { why = type == BuildingType.Battery && max == 1 ? "A second one unlocks later." : "All built."; return false; }
            if (State.supplies < def.cost) { why = "Need " + def.cost + " supplies."; return false; }
            return true;
        }

        public string CountText(BuildingType type)
        {
            int max = Placement.MaxCount(sim.balance, State, type);
            return max > 1 ? State.Count(type) + "/" + max : "";
        }

        public PlacedBuilding Selected => State.Find(SelectedId);

        public bool DrawingReady => IsDrawing && GoalsMet == goals.Length;

        public List<Alert> Alerts()
        {
            var list = new List<Alert>();
            if (IsDrawing || State.Finished) return list;
            var s = State;
            var f = Forecast;

            if (s.geologist == CrewLocation.Outside && !s.recallIssued)
            {
                bool can = sim.CanRecall(s, out string why);
                list.Add(new Alert
                {
                    level = AlertLevel.Danger, text = can ? "Yuki is outside. Call her in!" : "Yuki is outside. " + why,
                    actionLabel = "Call Yuki", action = () => Recall(), actionEnabled = can
                });
            }
            if (s.StormActive && !ShelterOn())
                list.Add(new Alert { level = AlertLevel.Danger, text = s.Count(BuildingType.Shelter) == 0 ? "Build a Shelter for the storm." : "Turn on the Shelter.",
                    actionLabel = s.Count(BuildingType.Shelter) == 0 ? "Build" : "Turn on", action = ShelterAction });
            if (s.Count(BuildingType.Habitat) > 0 && s.Count(BuildingType.LifeSupport) == 0)
                list.Add(new Alert { level = AlertLevel.Danger, text = "No Life Support. Air is dropping." });
            if (f != null && f.failed)
                list.Add(new Alert { level = AlertLevel.Danger, text = "Tomorrow the crew is in danger!" });
            else if (f != null && f.lastDay != null && f.lastDay.airRanOut)
                list.Add(new Alert { level = AlertLevel.Danger, text = "Air runs out tomorrow." });
            if (f != null && f.lastDay != null && f.lastDay.brownouts > 0)
            {
                var greenhouse = s.buildings.Find(b => b.type == BuildingType.Greenhouse && b.enabled);
                list.Add(new Alert
                {
                    level = AlertLevel.Warning, text = "Power runs out tomorrow. Turn something off.",
                    actionLabel = greenhouse != null ? "Rest Greenhouse" : null,
                    action = greenhouse != null ? () => Toggle(greenhouse.id) : (Action)null
                });
            }
            foreach (var b in s.buildings)
            {
                if (!b.broken) continue;
                int id = b.id;
                list.Add(new Alert { level = AlertLevel.Warning, text = "A Solar Array is broken.", actionLabel = "Fix (" + sim.balance.repairCost + ")",
                    action = () => Repair(id), actionEnabled = s.supplies >= sim.balance.repairCost });
                break;
            }
            if (sim.scenario.On(s.day + 1, EventKind.Shadow).Count > 0)
                list.Add(new Alert { level = AlertLevel.Info, text = "Tomorrow: " + Planet.dimmingName.ToLowerInvariant() + ". Less sun." });
            if (f != null && f.meals <= 0 && s.Count(BuildingType.Habitat) > 0)
                list.Add(new Alert { level = AlertLevel.Warning, text = "Food runs out tomorrow." });
            if (list.Count > 3) list.RemoveRange(3, list.Count - 3);
            return list;
        }

        bool ShelterOn()
        {
            foreach (var b in State.buildings)
                if (b.type == BuildingType.Shelter && b.enabled) return true;
            return false;
        }

        void ShelterAction()
        {
            foreach (var b in State.buildings)
            {
                if (b.type != BuildingType.Shelter) continue;
                Toggle(b.id);
                return;
            }
            Choose(BuildingType.Shelter);
        }

        // ---------- commands ----------

        public void Choose(BuildingType? type)
        {
            if (Ended) return;
            if (type.HasValue && !CanBuild(type.Value, out _)) return;
            BuildChoice = type;
            if (type.HasValue) SelectedId = 0;
            if (type == BuildingType.SolarArray) ShowHint("h_sun");
            Changed();
        }

        public PlacementResult Place(int x, int y)
        {
            if (!BuildChoice.HasValue || Ended) return PlacementResult.Fail(PlacementReason.Locked, "Pick a building first.");
            var type = BuildChoice.Value;
            var result = sim.Place(State, type, x, y);
            if (!result.Ok) return result;
            if (!CanBuild(type, out _)) BuildChoice = null;
            if (IsDrawing && State.Count(BuildingType.Greenhouse) > 0 && !Goals.PowerPositive(sim, State)) ShowHint("h_power", true);
            else if (!IsDrawing) ShowHint(State.buildings.Count >= 2 ? "h_next" : "h_info");
            Changed();
            return result;
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

        public DayReport NextDay()
        {
            if (IsDrawing || Ended || State.Finished) return null;
            State = sim.AdvanceDay(State);
            BuildChoice = null;
            if (Hint == story.Hint("h_next")) Hint = "";
            if (State.lastDay.brownouts > 0) ShowHint("h_toggle");
            Changed();
            return State.lastDay;
        }

        public void DismissHint()
        {
            Hint = "";
            Changed();
        }

        void ShowHint(string id, bool evenIfSeen = false)
        {
            if (!seenHints.Add(id) && !evenIfSeen) return;
            Hint = story.Hint(id);
        }

        // ---------- ending ----------

        public MissionResult Finish()
        {
            Ended = true;
            var result = new MissionResult
            {
                mode = mode, levelId = level != null ? level.id : "free", title = Title, failed = State.failed,
                score = IsDrawing ? DrawingScore() : Grader.Mission(sim.balance, State, GoalsMet, goals.Length)
            };
            AddNotes(result);
            if (level != null && level.saveBase && result.score.passed) Progress.SaveBase(level.id, LevelSetup.Save(State));
            Changed();
            return result;
        }

        LevelScore DrawingScore()
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
            return Grader.Drawing(power.generationKW, power.demandKW, panels == 0 ? 0 : sun / panels, GoalsMet, goals.Length);
        }

        void AddNotes(MissionResult r)
        {
            var s = State;
            if (IsDrawing)
            {
                r.notes.Add(story.Story("p_locked"));
                r.science = story.Hint("h_power");
                return;
            }
            if (s.failed) r.notes.Add(story.Debrief("fail_" + s.failReason));
            if (s.stormsStarted > 0)
            {
                r.notes.Add(story.Debrief(s.earlyRecalls >= s.stormsStarted ? "did_recall_yes" : "did_recall_late"));
                r.notes.Add(story.Debrief(s.shelterHours > 0 ? "did_shelter_yes" : "did_shelter_no"));
                if (s.greenhouseOffInStorm) r.notes.Add(story.Debrief("did_shed_yes"));
                else if (s.batteriesAtOnset >= 2) r.notes.Add(story.Debrief("did_stored_yes"));
            }
            if (s.brownoutDays > 0) r.notes.Add(story.Debrief("did_brownout"));
            if (r.notes.Count == 0) r.notes.Add(story.Debrief(s.completed ? "body_win" : "body_fail"));
            if (r.notes.Count > 3) r.notes.RemoveRange(3, r.notes.Count - 3);
            r.science = story.Debrief(s.stormsStarted > 0 ? "sci_warning" : s.brownoutDays > 0 ? "sci_priority" : "sci_battery");
        }
    }
}
