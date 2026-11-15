using System;
using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapmakers.Game
{
    public enum GameMode { Story, Free }

    public sealed class FreeSettings
    {
        public static readonly string[] Goals = { "core_base", "rover", "harvest", "recall_early", "shelter_used", "safe_to_end" };

        public string planet = "Moon";
        public Difficulty difficulty = Difficulty.Normal;
        public int seed = 1;
        public double hours = 144;

        static readonly System.Random seeds = new System.Random();
        public static int NewSeed() => seeds.Next(1000, 10000);
    }

    public sealed class CheckItem
    {
        public string label;
        public bool met;
    }

    // What happened, captured once when the run ends. The grade and campaign eligibility are separate.
    public sealed class MissionResult
    {
        public GameMode mode;
        public string levelId;
        public string title;
        public MissionEnd end;
        public FailReason failReason;
        public List<FailReason> otherHazards = new List<FailReason>();
        public double endHour;
        public LevelScore score;
        public bool evaluation;
        public bool canContinue;
        public string blockedWhy = "";
        public string cause = "";
        public string headline = "";
        public string fix = "";
        public double air, water, meals, battery, batteryCap, safety, soil;
        public List<CheckItem> checklist = new List<CheckItem>();
        public List<string> notes = new List<string>();
        public List<string> tips = new List<string>();
        public string science;

        public bool Failed => end == MissionEnd.Failed;

        public static MissionResult FromState(MissionRun run)
        {
            var s = run.State;
            var sim = run.sim;
            var story = run.story;
            var r = new MissionResult
            {
                mode = run.mode, levelId = run.level != null ? run.level.id : "free", title = run.Title,
                end = s.end, failReason = s.failReason, otherHazards = new List<FailReason>(s.otherHazards), endHour = s.endHour,
                evaluation = s.end == MissionEnd.Deadline, score = Grader.Mission(sim, s, run.goals),
                air = s.airHours, water = s.waterL, meals = s.meals, battery = s.batteryKWh, batteryCap = sim.BatteryCapacity(s),
                safety = s.safety, soil = s.soil
            };

            var catalog = LevelCatalog.Load();
            foreach (var type in Simulator.CoreTypes)
                r.checklist.Add(new CheckItem { label = sim.balance.Def(type).displayName, met = s.Count(type) > 0 });
            foreach (var goal in run.goals)
                if (goal != "core_base") r.checklist.Add(new CheckItem { label = catalog.GoalLabel(goal), met = run.GoalMet(goal) });

            if (r.Failed) Explain(r, run);
            r.canContinue = r.evaluation && Prerequisite(run, out r.blockedWhy);
            r.tips.AddRange(Tips(r, run));
            AddNotes(r, run);
            return r;
        }

        // A safe evaluation can continue unless the next level needs something this base does not have.
        static bool Prerequisite(MissionRun run, out string why)
        {
            why = "";
            if (run.level == null || !run.level.saveBase || Simulator.HasCoreBase(run.State)) return true;
            var missing = new List<string>();
            foreach (var type in Simulator.MissingCore(run.State)) missing.Add(run.sim.balance.Def(type).displayName);
            why = run.story.Debrief("blocked_core").Replace("{list}", string.Join(", ", missing));
            return false;
        }

        // Cause, explanation from real telemetry, and one practical fix.
        static void Explain(MissionResult r, MissionRun run)
        {
            var s = run.State;
            var story = run.story;
            string id = r.failReason.ToString();
            string why = "", fix = "fix_" + id;
            switch (r.failReason)
            {
                case FailReason.NoAir:
                    why = s.Count(BuildingType.LifeSupport) == 0 ? "missing" : "power";
                    if (why == "missing") fix = "fix_NoAir_missing";
                    break;
                case FailReason.NoFood:
                    why = FoodCause(run);
                    if (why == "missing") fix = "fix_NoFood_missing";
                    break;
                case FailReason.NoWater:
                    var g = s.First(BuildingType.Greenhouse);
                    why = g != null && g.enabled && g.powered ? "plants" : "crew";
                    break;
                case FailReason.PowerLoss:
                    why = "out";
                    break;
                case FailReason.RadiationOutside:
                    why = "out";
                    break;
                case FailReason.RadiationNoShelter:
                    var shelter = s.First(BuildingType.Shelter);
                    why = shelter == null ? "missing" : !shelter.enabled ? "off" : !shelter.powered ? "power" : "far";
                    fix = "fix_" + id + "_" + why;
                    break;
            }
            r.headline = Fill(story.Debrief("head_" + id), run);
            r.cause = Fill(story.Debrief("why_" + id + "_" + why), run);
            r.fix = Fill(story.Debrief(fix), run);
        }

        static string FoodCause(MissionRun run)
        {
            var g = run.State.First(BuildingType.Greenhouse);
            if (g == null) return "missing";
            if (!g.enabled) return "off";
            if (!g.powered) return "power";
            if (run.State.waterL <= 0) return "water";
            return "slow";
        }

        public static string Fill(string text, MissionRun run)
        {
            return text.Replace("{geo}", run.sim.geologistName).Replace("{water}", run.Planet.waterSource)
                .Replace("{hours}", SimText.Num(run.sim.balance.powerLossToleranceHours));
        }

        // Up to two concrete improvements, from the weakest parts of the result.
        static List<string> Tips(MissionResult r, MissionRun run)
        {
            var tips = new List<string>();
            var s = run.State;
            var story = run.story;
            foreach (var type in Simulator.MissingCore(s))
            {
                tips.Add(story.Debrief("tip_build").Replace("{name}", run.sim.balance.Def(type).displayName));
                break;
            }
            foreach (var b in s.buildings)
            {
                foreach (var c in Layout.Checks(run.sim.balance, run.sim.terrain, s, b))
                {
                    if (c.met || tips.Count >= 2) continue;
                    tips.Add(run.sim.balance.Def(b.type).displayName + ": " + c.bad);
                    break;
                }
                if (tips.Count >= 2) break;
            }
            foreach (var row in r.score.rows)
            {
                if (tips.Count >= 3 || row.points * 10 >= row.max * 7) continue;
                string tip = story.Debrief("tip_" + row.label.Split(' ')[0].ToLowerInvariant().TrimEnd(','));
                if (!tip.StartsWith("[")) tips.Add(tip);
            }
            if (tips.Count > 3) tips.RemoveRange(3, tips.Count - 3);
            return tips;
        }

        static void AddNotes(MissionResult r, MissionRun run)
        {
            var s = run.State;
            var story = run.story;
            if (s.stormsStarted > 0)
            {
                r.notes.Add(Fill(story.Debrief(s.earlyRecalls >= s.stormsStarted ? "did_recall_yes" : "did_recall_late"), run));
                r.notes.Add(story.Debrief(s.shelterHours > 0 ? "did_shelter_yes" : "did_shelter_no"));
                if (s.greenhouseOffInStorm) r.notes.Add(story.Debrief("did_shed_yes"));
                else if (s.batteriesAtOnset >= 2) r.notes.Add(story.Debrief("did_stored_yes"));
            }
            if (s.brownouts > 0) r.notes.Add(story.Debrief("did_brownout"));
            if (r.notes.Count == 0) r.notes.Add(story.Debrief(r.evaluation ? "body_win" : "body_fail"));
            if (r.notes.Count > 3) r.notes.RemoveRange(3, r.notes.Count - 3);
            r.science = story.Debrief(s.stormsStarted > 0 ? "sci_warning" : run.Planet.id == "Mars" ? "sci_mars" : s.brownouts > 0 ? "sci_priority" : "sci_layout");
        }
    }

    // Moves the player between scenes: Menu -> Cutscene -> Game -> Results -> Cutscene -> Menu.
    public static class Flow
    {
        public const string MenuScene = "Menu";
        public const string CutsceneScene = "Cutscene";
        public const string GameScene = "Game";
        public const string ResultsScene = "Results";

        public static GameMode Mode { get; private set; } = GameMode.Story;
        public static LevelDef Level { get; private set; }
        public static FreeSettings Free { get; private set; }
        public static string CutsceneId { get; private set; }
        public static string AfterCutscene { get; private set; } = MenuScene;
        public static MissionResult LastResult { get; private set; }
        public static string MenuPanel = "main";

        static readonly Queue<string> pending = new Queue<string>();

        // Required lessons the player has not seen yet come first; then the level's own story.
        // A lesson level (Tutorial 2) is only story and lesson, then back to the level list.
        public static void StartLevel(string levelId)
        {
            Mode = GameMode.Story;
            Level = LevelCatalog.Load().Level(levelId);
            Free = null;
            var sequence = new List<string>();
            foreach (var id in Level.needs)
                if (!Progress.LessonSeen(id)) sequence.Add(id);
            sequence.Add(Level.before);
            if (Level.IsLesson)
            {
                sequence.Add(Level.lesson);
                MenuPanel = "story";
                PlaySequence(sequence, MenuScene);
                return;
            }
            PlaySequence(sequence, GameScene);
        }

        // Replays one lesson from the Wiki, then returns to the menu.
        public static void ReplayLesson(string lessonId)
        {
            MenuPanel = "main";
            PlaySequence(new[] { lessonId }, MenuScene);
        }

        public static void StartFree(FreeSettings settings)
        {
            Mode = GameMode.Free;
            Free = settings;
            Level = null;
            Load(GameScene);
        }

        public static void PlayCutscene(string id, string then) => PlaySequence(new[] { id }, then);

        public static void PlaySequence(IEnumerable<string> ids, string then)
        {
            pending.Clear();
            var catalog = LevelCatalog.Load();
            foreach (var id in ids)
                if (catalog.HasScene(id)) pending.Enqueue(id);
            AfterCutscene = then;
            NextScene();
        }

        // Called when a cutscene or lesson ends (finished or skipped).
        public static void SceneDone(string id)
        {
            if (HelpData.Load().Lesson(id) != null) Progress.MarkLessonSeen(id);
            if (Mode == GameMode.Story && Level != null && Level.IsLesson && Level.lesson == id) Progress.MarkPassed(Level.id);
            NextScene();
        }

        static void NextScene()
        {
            if (pending.Count == 0)
            {
                Load(AfterCutscene);
                return;
            }
            CutsceneId = pending.Dequeue();
            Load(CutsceneScene);
        }

        // Records the result once (a repeated call with the same result does nothing).
        public static void Record(MissionResult result)
        {
            if (result == null || ReferenceEquals(result, LastResult)) return;
            LastResult = result;
            Progress.Record(result);
        }

        public static void ShowReport(MissionResult result)
        {
            Record(result);
            Load(ResultsScene);
        }

        // Retry starts the same level or seed again from its intended start, never from the ended state.
        public static void Retry() => Load(GameScene);

        // After a level that may continue: play its closing cutscene, then return to the level list.
        public static void Continue()
        {
            MenuPanel = Mode == GameMode.Story ? "story" : "free";
            var result = LastResult;
            if (Mode == GameMode.Story && result != null && result.canContinue && LevelCatalog.Load().HasScene(Level.after))
            {
                PlayCutscene(Level.after, MenuScene);
                return;
            }
            Load(MenuScene);
        }

        public static void ToMenu(string panel = "main")
        {
            MenuPanel = panel;
            Load(MenuScene);
        }

        public static void Load(string scene) => SceneManager.LoadScene(scene);
    }

    // Saved between sessions with PlayerPrefs (works in WebGL too). Keys are unchanged from the turn-based version;
    // "passed" now means "may continue". A worse retry or a failure never lowers a best score or replaces a saved base.
    public static class Progress
    {
        const string Prefix = "mapmakers.";

        public static int Best(string levelId) => PlayerPrefs.GetInt(Prefix + "best." + levelId, -1);
        public static bool Passed(string levelId) => PlayerPrefs.GetInt(Prefix + "passed." + levelId, 0) == 1;

        public static bool IsUnlocked(string levelId)
        {
            var catalog = LevelCatalog.Load();
            int i = catalog.IndexOf(levelId);
            return i <= 0 || Passed(catalog.levels[i - 1].id);
        }

        public static bool LessonSeen(string lessonId) => PlayerPrefs.GetInt(Prefix + "lesson." + lessonId, 0) == 1;

        public static void MarkLessonSeen(string lessonId)
        {
            PlayerPrefs.SetInt(Prefix + "lesson." + lessonId, 1);
            PlayerPrefs.Save();
        }

        public static void MarkPassed(string levelId)
        {
            PlayerPrefs.SetInt(Prefix + "passed." + levelId, 1);
            PlayerPrefs.Save();
        }

        public static bool SuggestionsOn
        {
            get => PlayerPrefs.GetInt(Prefix + "suggest", 1) == 1;
            set { PlayerPrefs.SetInt(Prefix + "suggest", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool HintsOn
        {
            get => PlayerPrefs.GetInt(Prefix + "hints", 1) == 1;
            set { PlayerPrefs.SetInt(Prefix + "hints", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool ReducedMotion
        {
            get => PlayerPrefs.GetInt(Prefix + "calm", 0) == 1;
            set { PlayerPrefs.SetInt(Prefix + "calm", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static void Record(MissionResult result)
        {
            string id = result.mode == GameMode.Story ? result.levelId : "free";
            if (result.score.total > Best(id)) PlayerPrefs.SetInt(Prefix + "best." + id, result.score.total);
            if (result.canContinue) PlayerPrefs.SetInt(Prefix + "passed." + id, 1);
            PlayerPrefs.Save();
        }

        // Keeps the better of the saved base and this one.
        public static void SaveBase(string levelId, SavedBase saved, int score)
        {
            string scoreKey = Prefix + "basescore." + levelId;
            if (LoadBase(levelId) != null && PlayerPrefs.GetInt(scoreKey, -1) > score) return;
            PlayerPrefs.SetString(Prefix + "base." + levelId, JsonUtility.ToJson(saved));
            PlayerPrefs.SetInt(scoreKey, score);
            PlayerPrefs.Save();
        }

        public static SavedBase LoadBase(string levelId)
        {
            string json = PlayerPrefs.GetString(Prefix + "base." + levelId, "");
            return string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<SavedBase>(json);
        }

        public static void ResetAll()
        {
            foreach (var level in LevelCatalog.Load().levels)
            {
                PlayerPrefs.DeleteKey(Prefix + "best." + level.id);
                PlayerPrefs.DeleteKey(Prefix + "passed." + level.id);
                PlayerPrefs.DeleteKey(Prefix + "base." + level.id);
                PlayerPrefs.DeleteKey(Prefix + "basescore." + level.id);
            }
            foreach (var lesson in HelpData.Load().lessons) PlayerPrefs.DeleteKey(Prefix + "lesson." + lesson.id);
            PlayerPrefs.DeleteKey(Prefix + "best.free");
            PlayerPrefs.Save();
        }
    }
}
