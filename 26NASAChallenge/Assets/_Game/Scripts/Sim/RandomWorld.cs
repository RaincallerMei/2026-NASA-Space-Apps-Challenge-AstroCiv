using System;
using System.Collections.Generic;

namespace Mapmakers.Sim
{
    // Made-up maps for Free Build. Same seed, same map. Crater floors hold water (ice or wet clay); soft soil lies in patches.
    public static class TerrainGenerator
    {
        public const int MinFreeTiles = 200;

        public static TerrainGrid Generate(int seed, int width = 20, int height = 15)
        {
            TerrainGrid best = null;
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var terrain = Build(new Random(seed + attempt * 7919), width, height);
                best = terrain;
                if (FreeTiles(terrain) >= MinFreeTiles && HasFreeRect(terrain, 3, 3) && terrain.AnySoil() && terrain.AnyWater()) return terrain;
            }
            return best;
        }

        static TerrainGrid Build(Random rng, int width, int height)
        {
            var terrain = new TerrainGrid(width, height);
            const int cw = 6, ch = 5;
            var coarse = new double[cw, ch];
            for (int i = 0; i < cw; i++)
                for (int j = 0; j < ch; j++)
                    coarse[i, j] = rng.NextDouble();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double u = (double)x / (width - 1) * (cw - 1), v = (double)y / (height - 1) * (ch - 1);
                    int i = Math.Min((int)u, cw - 2), j = Math.Min((int)v, ch - 2);
                    double fu = Smooth(u - i), fv = Smooth(v - j);
                    double n = Lerp(Lerp(coarse[i, j], coarse[i + 1, j], fu), Lerp(coarse[i, j + 1], coarse[i + 1, j + 1], fu), fv);
                    terrain.SetSun(x, y, Math.Round(0.5 + 0.5 * n, 3));
                }
            }

            int craters = 3 + rng.Next(3);
            for (int c = 0; c < craters; c++)
            {
                double cx = rng.Next(width), cy = rng.Next(height), r = 1.0 + rng.NextDouble() * 1.4;
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                        {
                            terrain.SetBlocked(x, y, true);
                            terrain.SetWater(x, y, true);
                        }
            }

            int rocks = 4 + rng.Next(5);
            for (int k = 0; k < rocks; k++)
            {
                int x = rng.Next(width), y = rng.Next(height);
                terrain.SetBlocked(x, y, true);
                terrain.SetWater(x, y, false);
            }

            int patches = 2 + rng.Next(2);
            for (int p = 0; p < patches; p++)
            {
                int cx = rng.Next(width), cy = rng.Next(height);
                for (int y = cy - 1; y <= cy + 1; y++)
                    for (int x = cx - 1; x <= cx + 1; x++)
                        if (terrain.InBounds(x, y) && !terrain.IsBlocked(x, y) && rng.NextDouble() < 0.8) terrain.SetSoil(x, y, true);
            }
            return terrain;
        }

        public static int FreeTiles(TerrainGrid t)
        {
            int n = 0;
            for (int y = 0; y < t.height; y++)
                for (int x = 0; x < t.width; x++)
                    if (!t.IsBlocked(x, y)) n++;
            return n;
        }

        static bool HasFreeRect(TerrainGrid t, int w, int h)
        {
            for (int y = 0; y + h <= t.height; y++)
                for (int x = 0; x + w <= t.width; x++)
                    if (RectFree(t, x, y, w, h)) return true;
            return false;
        }

        static bool RectFree(TerrainGrid t, int x, int y, int w, int h)
        {
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    if (t.IsBlocked(x + dx, y + dy)) return false;
            return true;
        }

        static double Smooth(double t) => t * t * (3 - 2 * t);
        static double Lerp(double a, double b, double t) => a + (b - a) * t;
    }

    public enum Difficulty { Easy, Normal, Hard }

    // Random event schedule for Free Build, in game hours. Storms and dim spells never overlap,
    // and every storm is over well before the evaluation so the player can recover.
    public static class ScenarioFactory
    {
        public const double FirstEventHour = 20;
        public const double RecoveryHours = 8;
        public const double FirstDropHour = 12;
        public const double DropEveryHours = 20;

        public static Scenario Random(int seed, double durationHours, Difficulty difficulty, Balance balance)
        {
            var rng = new Random(seed);
            var scenario = new Scenario { durationHours = durationHours, shelterUnlocked = true, maxBatteries = 2 };
            var busy = new List<(double from, double to)>();

            int storms = difficulty == Difficulty.Easy ? 1 : difficulty == Difficulty.Normal ? 1 + rng.Next(2) : 2;
            int dims = difficulty == Difficulty.Easy ? 1 : difficulty == Difficulty.Normal ? 2 : 3;
            int faults = difficulty == Difficulty.Easy ? 0 : difficulty == Difficulty.Normal ? 1 : 2;

            double stormLength = balance.storm.endHour;
            for (int i = 0; i < storms; i++)
            {
                double hour = Pick(rng, FirstEventHour, durationHours - stormLength - RecoveryHours, stormLength, busy);
                if (hour < 0) continue;
                double strength = Math.Round((0.7 + 0.3 * rng.NextDouble()) * 20) / 20;
                scenario.events.Add(ScheduledEvent.Make(hour, EventKind.SolarStorm, strength));
            }
            double dimLength = balance.shadow.durationHours;
            for (int i = 0; i < dims; i++)
            {
                double hour = Pick(rng, FirstEventHour, durationHours - dimLength - 2, dimLength, busy);
                if (hour >= 0) scenario.events.Add(ScheduledEvent.Make(hour, EventKind.Shadow));
            }
            for (int i = 0; i < faults; i++)
                scenario.events.Add(ScheduledEvent.Make(Whole(FirstEventHour + rng.NextDouble() * (durationHours - FirstEventHour - 10)), EventKind.Fault, 1, rng.Next(100)));
            int bonus = difficulty == Difficulty.Easy ? 10 : difficulty == Difficulty.Normal ? 5 : 0;
            for (double hour = FirstDropHour; hour < durationHours - 12; hour += DropEveryHours)
                scenario.events.Add(ScheduledEvent.Make(Whole(hour + rng.NextDouble() * 4), EventKind.SupplyDrop, 1, 20 + bonus + 5 * rng.Next(3)));

            scenario.events.Sort((a, b) => a.hour.CompareTo(b.hour));
            return scenario;
        }

        static double Pick(Random rng, double min, double max, double length, List<(double from, double to)> busy)
        {
            if (max < min) return -1;
            for (int tries = 0; tries < 50; tries++)
            {
                double hour = Whole(min + rng.NextDouble() * (max - min));
                bool clash = busy.Exists(b => hour < b.to + 2 && hour + length + 2 > b.from);
                if (clash) continue;
                busy.Add((hour, hour + length));
                return hour;
            }
            return -1;
        }

        static double Whole(double hour) => Math.Round(hour);
    }
}
