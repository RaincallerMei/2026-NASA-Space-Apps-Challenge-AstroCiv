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
        public string planet = "Moon";
        public Difficulty difficulty = Difficulty.Normal;
        public int seed = 1;
        public int days = 10;

        static readonly System.Random seeds = new System.Random();
        public static int NewSeed() => seeds.Next(1000, 10000);
    }

    public sealed class MissionResult
    {
        public GameMode mode;
        public string levelId;
        public string title;
        public LevelScore score;
        public bool failed;
        public List<string> notes = new List<string>();
        public string science;
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

        public static void StartLevel(string levelId)
        {
            Mode = GameMode.Story;
            Level = LevelCatalog.Load().Level(levelId);
            Free = null;
            if (LevelCatalog.Load().Cutscene(Level.before) != null) PlayCutscene(Level.before, GameScene);
            else Load(GameScene);
        }

        public static void StartFree(FreeSettings settings)
        {
            Mode = GameMode.Free;
            Free = settings;
            Level = null;
            Load(GameScene);
        }

        public static void PlayCutscene(string id, string then)
        {
            CutsceneId = id;
            AfterCutscene = then;
            Load(CutsceneScene);
        }

        public static void FinishMission(MissionResult result)
        {
            LastResult = result;
            Progress.Record(result);
            Load(ResultsScene);
        }

        public static void Retry() => Load(GameScene);

        // After a passed story level: play its closing cutscene, then return to the level list.
        public static void Continue()
        {
            MenuPanel = Mode == GameMode.Story ? "story" : "free";
            var result = LastResult;
            if (Mode == GameMode.Story && result != null && result.score.passed && LevelCatalog.Load().Cutscene(Level.after) != null)
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

    // Saved between sessions with PlayerPrefs (works in WebGL too).
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

        public static bool HintsOn
        {
            get => PlayerPrefs.GetInt(Prefix + "hints", 1) == 1;
            set { PlayerPrefs.SetInt(Prefix + "hints", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static void Record(MissionResult result)
        {
            string id = result.mode == GameMode.Story ? result.levelId : "free";
            if (result.score.total > Best(id)) PlayerPrefs.SetInt(Prefix + "best." + id, result.score.total);
            if (result.score.passed) PlayerPrefs.SetInt(Prefix + "passed." + id, 1);
            PlayerPrefs.Save();
        }

        public static void SaveBase(string levelId, SavedBase saved)
        {
            PlayerPrefs.SetString(Prefix + "base." + levelId, JsonUtility.ToJson(saved));
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
            }
            PlayerPrefs.DeleteKey(Prefix + "best.free");
            PlayerPrefs.Save();
        }
    }
}
