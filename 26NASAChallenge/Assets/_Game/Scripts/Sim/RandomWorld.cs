using System;

namespace Mapmakers.Sim
{
    // Made-up maps for Free Build and Mars. Same seed, same map.
    public static class TerrainGenerator
    {
        public const int MinFreeTiles = 210;

        public static TerrainGrid Generate(int seed, int width = 20, int height = 15)
        {
            TerrainGrid best = null;
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var terrain = Build(new Random(seed + attempt * 7919), width, height);
                best = terrain;
                if (FreeTiles(terrain) >= MinFreeTiles && HasFreeRect(terrain, 3, 3)) return terrain;
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
                        if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r) terrain.SetBlocked(x, y, true);
            }

            int rocks = 4 + rng.Next(5);
            for (int k = 0; k < rocks; k++) terrain.SetBlocked(rng.Next(width), rng.Next(height), true);
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

    // Random event schedule for Free Build. Storms and dim days never overlap, so each one can be handled.
    public static class ScenarioFactory
    {
        public static Scenario Random(int seed, int days, Difficulty difficulty)
        {
            var rng = new Random(seed);
            var scenario = new Scenario { days = days, shelterUnlocked = true, maxBatteries = 2 };
            var busy = new bool[days + 3];

            int storms = difficulty == Difficulty.Easy ? 1 : difficulty == Difficulty.Normal ? 1 + rng.Next(2) : 2;
            int dims = difficulty == Difficulty.Easy ? 1 : difficulty == Difficulty.Normal ? 2 : 3;
            int faults = difficulty == Difficulty.Easy ? 0 : difficulty == Difficulty.Normal ? 1 : 2;
            int drops = difficulty == Difficulty.Easy ? 2 : 1;

            for (int i = 0; i < storms; i++)
            {
                int day = Pick(rng, 3, days - 1, d => !busy[d - 1] && !busy[d] && !busy[d + 1]);
                if (day < 0) continue;
                busy[day - 1] = busy[day] = busy[day + 1] = true;
                double strength = Math.Round((0.7 + 0.3 * rng.NextDouble()) * 20) / 20;
                scenario.events.Add(ScheduledEvent.Make(day, EventKind.SolarStorm, strength));
            }
            for (int i = 0; i < dims; i++)
            {
                int day = Pick(rng, 2, days, d => !busy[d]);
                if (day < 0) continue;
                busy[day] = true;
                scenario.events.Add(ScheduledEvent.Make(day, EventKind.Shadow));
            }
            for (int i = 0; i < faults; i++)
                scenario.events.Add(ScheduledEvent.Make(2 + rng.Next(Math.Max(1, days - 1)), EventKind.Fault, 1, rng.Next(100)));
            for (int i = 0; i < drops; i++)
                scenario.events.Add(ScheduledEvent.Make(2 + rng.Next(Math.Max(1, days - 1)), EventKind.SupplyDrop, 1, 20 + 5 * rng.Next(3)));

            scenario.events.Sort((a, b) => a.day.CompareTo(b.day));
            return scenario;
        }

        static int Pick(Random rng, int min, int max, Func<int, bool> ok)
        {
            if (max < min) return -1;
            for (int tries = 0; tries < 50; tries++)
            {
                int day = min + rng.Next(max - min + 1);
                if (ok(day)) return day;
            }
            return -1;
        }
    }
}
