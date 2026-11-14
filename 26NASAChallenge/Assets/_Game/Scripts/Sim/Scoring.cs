using System;
using System.Collections.Generic;

namespace Mapmakers.Sim
{
    public sealed class ScoreRow
    {
        public string label;
        public int points;
        public int max;
    }

    public sealed class LevelScore
    {
        public List<ScoreRow> rows = new List<ScoreRow>();
        public int total;
        public string letter;
        public int stars;
        public bool passed;
    }

    // Score out of 100. S = 81-100, A = 61-80, B = 31-60, C = 0-30. Stars: S 3, A 2, B 1, C 0.
    public static class Grader
    {
        public const int FailCap = 30;

        public static string Letter(int score) => score >= 81 ? "S" : score >= 61 ? "A" : score >= 31 ? "B" : "C";

        public static int Stars(string letter)
        {
            switch (letter)
            {
                case "S": return 3;
                case "A": return 2;
                case "B": return 1;
                default: return 0;
            }
        }

        public static LevelScore Mission(Balance balance, SimState s, int goalsDone, int goalsTotal)
        {
            int daysPlayed = Math.Max(1, s.completed ? s.day - 1 : s.day);
            double supplies = (Clamp01(s.airHours / balance.airCapHours)
                + Clamp01(s.waterL / balance.startWaterL)
                + Clamp01(s.meals / balance.startMeals)) / 3;

            var score = new LevelScore();
            Add(score, "Crew safety", 25, Clamp01((s.minSafety - 50) / 50));
            Add(score, "Steady power", 25, 1 - Clamp01((double)s.brownoutDays / daysPlayed));
            Add(score, "Air, water, food", 25, supplies);
            Add(score, "Goals", 25, Fraction(goalsDone, goalsTotal));
            return Finish(score, s.failed, s.completed && !s.failed);
        }

        public static LevelScore Drawing(double powerInKW, double powerOutKW, double solarSun, int goalsDone, int goalsTotal)
        {
            var score = new LevelScore();
            Add(score, "Power in beats power out", 25, powerOutKW <= 0 ? 0 : Clamp01(powerInKW / powerOutKW));
            Add(score, "Sunny solar spots", 25, Clamp01((solarSun - 0.5) / 0.5));
            Add(score, "Goals", 50, Fraction(goalsDone, goalsTotal));
            return Finish(score, false, goalsDone >= goalsTotal);
        }

        static LevelScore Finish(LevelScore score, bool failed, bool passed)
        {
            int total = 0;
            foreach (var row in score.rows) total += row.points;
            if (failed) total = Math.Min(total, FailCap);
            score.total = total;
            score.letter = Letter(total);
            score.stars = Stars(score.letter);
            score.passed = passed;
            return score;
        }

        static void Add(LevelScore score, string label, int max, double fraction)
        {
            score.rows.Add(new ScoreRow { label = label, max = max, points = (int)Math.Round(max * Clamp01(fraction)) });
        }

        static double Fraction(int done, int total) => total <= 0 ? 1 : (double)done / total;
        static double Clamp01(double v) => double.IsNaN(v) ? 0 : Math.Max(0, Math.Min(1, v));
    }

    // Level goals by id. Kept in the simulation so tests and the UI agree.
    public static class Goals
    {
        public static bool IsMet(string id, Simulator sim, SimState s)
        {
            switch (id)
            {
                case "habitat": return s.Count(BuildingType.Habitat) > 0;
                case "solar": return s.Count(BuildingType.SolarArray) > 0;
                case "greenhouse": return s.Count(BuildingType.Greenhouse) > 0;
                case "power_positive": return PowerPositive(sim, s);
                case "core_base": return Simulator.HasCoreBase(s);
                case "harvest": return s.harvestDone;
                case "two_batteries": return s.Count(BuildingType.Battery) >= 2;
                case "recall_early": return s.stormsStarted > 0 && s.earlyRecalls >= s.stormsStarted;
                case "shelter_used": return s.shelterHours > 0;
                case "storm_power": return s.stormsStarted > 0 && !s.StormActive && s.stormBrownouts == 0;
                case "all_days": return s.completed && !s.failed;
                default: return false;
            }
        }

        public static bool PowerPositive(Simulator sim, SimState s)
        {
            var r = sim.Preview(s);
            return r.demandKW > 0 && r.generationKW + 1e-6 >= r.demandKW;
        }

        public static int CountMet(IEnumerable<string> ids, Simulator sim, SimState s)
        {
            int n = 0;
            foreach (var id in ids)
                if (IsMet(id, sim, s)) n++;
            return n;
        }
    }
}
