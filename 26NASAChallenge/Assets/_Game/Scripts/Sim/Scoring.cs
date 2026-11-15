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

    // A grade only. Whether the campaign may continue is decided separately (MissionResult.canContinue).
    public sealed class LevelScore
    {
        public List<ScoreRow> rows = new List<ScoreRow>();
        public int rawTotal;
        public int total;
        public int cap = 100;
        public string capNote = "";
        public string letter;
        public int stars;
        public bool goalsComplete;
    }

    // Score out of 100. S = 81-100, A = 61-80, B = 31-60, C = 0-30. Stars: S 3, A 2, B 1, C 0.
    public static class Grader
    {
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

        public static LevelScore Mission(Simulator sim, SimState s, IList<string> goals)
        {
            var b = sim.balance;
            var g = b.grade;
            int goalsDone = Goals.CountMet(goals, sim, s);
            var score = new LevelScore { goalsComplete = goalsDone == goals.Count };

            Add(score, "Base and goals", g.baseAndGoals, 0.5 * CoreFraction(s) + 0.5 * Fraction(goalsDone, goals.Count));
            Add(score, "Smart layout", g.layout, LayoutQuality(sim, s));
            Add(score, "Crew safety", g.crewSafety, Math.Min(Clamp01((s.minSafety - 50) / 50), Clamp01(s.minAirHours / b.warnHours)));
            Add(score, "Air, water, food left", g.reserves, (Clamp01(s.airHours / b.airCapHours)
                + Clamp01(s.waterL / b.startWaterL) + Clamp01(s.meals / b.startMeals)) / 3);
            Add(score, "Steady power", g.steadyPower, PowerReliability(b, s));

            if (s.Failed) Cap(score, g.failCap, "Mission stopped early: capped at " + g.failCap + ".");
            else if (goals.Contains("core_base") && !Simulator.HasCoreBase(s))
                Cap(score, g.missingCoreCap, "A core building is missing: capped at " + g.missingCoreCap + " (B).");
            else if (!score.goalsComplete)
                Cap(score, g.missingGoalCap, "A goal is missing: capped at " + g.missingGoalCap + " (no S).");
            return Finish(score);
        }

        // Every core building counts when it is present and working.
        static double CoreFraction(SimState s)
        {
            int ok = 0;
            foreach (var type in Simulator.CoreTypes)
            {
                var b = s.First(type);
                if (b != null && !b.broken && (b.powered || type == BuildingType.SolarArray || type == BuildingType.Battery || !b.enabled)) ok++;
            }
            return (double)ok / Simulator.CoreTypes.Length;
        }

        // Sunny solar spots (40%) and met links (60%).
        public static double LayoutQuality(Simulator sim, SimState s)
        {
            double sun = 0;
            int panels = 0;
            foreach (var b in s.buildings)
            {
                if (b.type != BuildingType.SolarArray) continue;
                var def = sim.balance.Def(b.type);
                sun += Clamp01((sim.terrain.AverageSun(b.x, b.y, def.width, def.height) - 0.5) / 0.45);
                panels++;
            }
            double sunScore = panels == 0 ? 0 : sun / panels;
            double links = Layout.Score(sim.balance, sim.terrain, s, out _, out _);
            return 0.4 * sunScore + 0.6 * links;
        }

        // No points without critical systems to serve. Time with loads switched off costs points; critical outages cost more.
        static double PowerReliability(Balance b, SimState s)
        {
            if (s.criticalServedHours <= 0) return 0;
            double shed = s.shedHours / s.criticalServedHours;
            double lost = s.criticalLostHours / s.criticalServedHours;
            return 1 - b.grade.shedPenalty * shed - b.grade.criticalPenalty * lost;
        }

        public static LevelScore Drawing(double powerInKW, double powerOutKW, double solarSun, int goalsDone, int goalsTotal)
        {
            var score = new LevelScore { goalsComplete = goalsDone >= goalsTotal };
            Add(score, "Power in beats power out", 25, powerOutKW <= 0 ? 0 : Clamp01(powerInKW / powerOutKW));
            Add(score, "Sunny solar spots", 25, Clamp01((solarSun - 0.5) / 0.5));
            Add(score, "Goals", 50, Fraction(goalsDone, goalsTotal));
            return Finish(score);
        }

        static void Cap(LevelScore score, int cap, string note)
        {
            score.cap = cap;
            score.capNote = note;
        }

        static LevelScore Finish(LevelScore score)
        {
            int total = 0;
            foreach (var row in score.rows) total += row.points;
            score.rawTotal = total;
            score.total = Math.Min(total, score.cap);
            if (score.total == score.rawTotal) score.capNote = "";
            score.letter = Letter(score.total);
            score.stars = Stars(score.letter);
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
        public const string SafeToEnd = "safe_to_end";

        public static bool IsMet(string id, Simulator sim, SimState s)
        {
            switch (id)
            {
                case "habitat": return s.Count(BuildingType.Habitat) > 0;
                case "solar": return s.Count(BuildingType.SolarArray) > 0;
                case "greenhouse": return s.Count(BuildingType.Greenhouse) > 0;
                case "power_positive": return PowerPositive(sim, s);
                case "core_base": return Simulator.HasCoreBase(s);
                case "rover": return s.Count(BuildingType.RoverGarage) > 0;
                case "harvest": return s.harvestDone;
                case "two_batteries": return s.Count(BuildingType.Battery) >= 2;
                case "recall_early": return s.stormsStarted > 0 && s.earlyRecalls >= s.stormsStarted;
                case "shelter_used": return s.shelterHours > 0;
                case "storm_power": return s.stormsSeenParticles > 0 && !s.StormActive && s.stormBrownouts == 0;
                case "dim_power": return s.shadowPassed && s.dimBrownouts == 0;
                case SafeToEnd: return s.Evaluated;
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
