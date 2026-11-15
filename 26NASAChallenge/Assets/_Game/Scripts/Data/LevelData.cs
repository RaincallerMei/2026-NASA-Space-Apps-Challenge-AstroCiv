using System;
using System.Collections.Generic;
using Mapmakers.Sim;
using UnityEngine;

namespace Mapmakers.Data
{
    [Serializable]
    public class LevelDef
    {
        public string id, tag, title, subtitle, planet, mode, before, after, startBase, lesson;
        public double durationHours;
        public int seed = -1;
        public int startSupplies = -1;
        public int maxBatteries = 1;
        public bool shelterUnlocked, saveBase;
        public string[] build = new string[0];
        public string[] needs = new string[0];
        public string[] goals = new string[0];
        public ScheduledEvent[] events = new ScheduledEvent[0];

        public bool IsDrawing => mode == "drawing";
        public bool IsLesson => mode == "lesson";
        public string Label => string.IsNullOrEmpty(tag) ? id : tag;
        public PlanetProfile Planet => PlanetProfile.Get(planet);

        public Scenario MakeScenario()
        {
            return new Scenario
            {
                durationHours = durationHours, shelterUnlocked = shelterUnlocked, maxBatteries = maxBatteries,
                events = new List<ScheduledEvent>(events)
            };
        }
    }

    [Serializable] public class Slide { public string image; public string[] lines; }
    [Serializable] public class CutsceneDef { public string id; public Slide[] slides; }
    [Serializable] public class GoalText { public string id, text; }

    [Serializable]
    public class LevelCatalog
    {
        public string note;
        public LevelDef[] levels;
        public CutsceneDef[] cutscenes;
        public GoalText[] goals;

        public const string ResourcePath = "Levels";

        static LevelCatalog cached;
        public static LevelCatalog Load()
        {
            if (cached == null) cached = JsonUtility.FromJson<LevelCatalog>(Resources.Load<TextAsset>(ResourcePath).text);
            return cached;
        }

        public LevelDef Level(string id) => Array.Find(levels, l => l.id == id);

        // A cutscene from this file or a lesson from Help.json.
        public bool HasScene(string id) => Cutscene(id) != null || HelpData.Load().Lesson(id) != null;
        public CutsceneDef Cutscene(string id) => string.IsNullOrEmpty(id) ? null : Array.Find(cutscenes, c => c.id == id);

        public string GoalLabel(string id)
        {
            var goal = Array.Find(goals, g => g.id == id);
            return goal == null ? id : goal.text;
        }

        public int IndexOf(string id) => Array.FindIndex(levels, l => l.id == id);

        public LevelDef Next(string id)
        {
            int i = IndexOf(id);
            return i >= 0 && i + 1 < levels.Length ? levels[i + 1] : null;
        }
    }

    // A saved building, used to carry a base from one level to the next.
    [Serializable] public class SavedBuilding { public string type; public int x, y; }
    [Serializable] public class SavedBase { public List<SavedBuilding> buildings = new List<SavedBuilding>(); }

    // Builds the simulator and first state for a level or a Free Build run.
    public static class LevelSetup
    {
        public static Simulator CreateSimulator(LevelDef level)
        {
            var balance = BalanceLoader.Load();
            if (level.startSupplies >= 0) balance.startSupplies = level.startSupplies;
            return new Simulator(balance, Terrain(level.planet, level.seed, balance), level.MakeScenario())
            {
                planet = level.Planet,
                missionEnabled = !level.IsDrawing,
                layoutRules = !level.IsDrawing
            };
        }

        public static Simulator FreeBuild(string planet, int seed, Difficulty difficulty, double durationHours)
        {
            var balance = BalanceLoader.Load();
            var scenario = ScenarioFactory.Random(seed, durationHours, difficulty, balance);
            return new Simulator(balance, TerrainGenerator.Generate(seed), scenario) { planet = PlanetProfile.Get(planet) };
        }

        public static TerrainGrid Terrain(string planet, int seed, Balance balance)
        {
            if (seed >= 0) return TerrainGenerator.Generate(seed);
            return planet == "Mars" ? TerrainFactory.LoadMars(balance) : TerrainFactory.Load(balance);
        }

        // Rebuilds a saved base for free. Returns false (and builds a ready-made base) when nothing valid was saved,
        // so the game can say so instead of silently swapping bases.
        public static bool LoadBase(Simulator sim, SimState s, SavedBase saved)
        {
            bool valid = saved != null;
            if (saved != null)
                foreach (var b in saved.buildings)
                    valid &= Enum.TryParse(b.type, out BuildingType type) && sim.PlaceFree(s, type, b.x, b.y).Ok;
            if (!valid || !Simulator.HasCoreBase(s))
            {
                s.buildings.Clear();
                s.nextId = 1;
                StarterBase.Build(sim, s);
                valid = false;
            }
            s.events.Clear();
            s.batteryKWh = sim.BatteryCapacity(s);
            s.harvestDone = true;
            return valid;
        }

        public static SavedBase Save(SimState s)
        {
            var saved = new SavedBase();
            foreach (var b in s.buildings)
                saved.buildings.Add(new SavedBuilding { type = b.type.ToString(), x = b.x, y = b.y });
            return saved;
        }
    }
}
