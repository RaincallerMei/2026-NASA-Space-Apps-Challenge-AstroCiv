using System;
using Mapmakers.Sim;

namespace Mapmakers.Data
{
    // Turns real maps into game terrain. The mappings are simplifications and the thresholds are game choices.
    public static class TerrainFactory
    {
        // Moon (LOLA): sunFraction = floor + (1 - floor) * min(1, percentLit / fullAtPercent).
        // Tiles that are almost never lit are blocked dark crater floor, where ice can stay frozen: the rover's water source.
        public static TerrainGrid FromIllumination(IlluminationGrid grid, TerrainRules rules)
        {
            var terrain = new TerrainGrid(grid.width, grid.height);
            for (int row = 0; row < grid.height; row++)
            {
                int y = grid.height - 1 - row;
                for (int x = 0; x < grid.width; x++)
                {
                    double percent = grid.percent[row, x];
                    double lit = Math.Min(1.0, percent / rules.sunFullAtPercent);
                    bool dark = percent < rules.blockedBelowPercent;
                    terrain.SetSun(x, y, rules.sunFloor + (1 - rules.sunFloor) * lit);
                    terrain.SetBlocked(x, y, dark);
                    terrain.SetWater(x, y, dark);
                }
            }
            for (int i = 0; i + 1 < rules.rockTiles.Length; i += 2)
                if (terrain.InBounds(rules.rockTiles[i], rules.rockTiles[i + 1]))
                    terrain.SetBlocked(rules.rockTiles[i], rules.rockTiles[i + 1], true);
            MarkSoil(terrain, rules.soilTiles);
            return terrain;
        }

        public static TerrainGrid Load(Balance balance)
        {
            return FromIllumination(FixtureLoader.Illumination(), balance.terrain);
        }

        // Mars (MOLA heights, Jezero crater): steep crater walls are blocked, the old lake floor has soft mud (soil),
        // the hand-picked delta tiles hold wet clay (water). Higher ground gets a little more sun (less dusty air above).
        public static TerrainGrid FromElevation(ElevationGrid grid, MarsTerrainRules rules)
        {
            var terrain = new TerrainGrid(grid.width, grid.height);
            double min = double.MaxValue;
            foreach (double h in grid.metres) min = Math.Min(min, h);
            for (int row = 0; row < grid.height; row++)
            {
                int y = grid.height - 1 - row;
                for (int x = 0; x < grid.width; x++)
                {
                    double h = grid.metres[row, x];
                    bool steep = Slope(grid, row, x) > rules.slopeBlockedM;
                    terrain.SetSun(x, y, Math.Round(rules.sunLow + (1 - rules.sunLow) * Math.Min(1, (h - min) / rules.fullSunAboveFloorM), 3));
                    terrain.SetBlocked(x, y, steep);
                    terrain.SetSoil(x, y, !steep && h < rules.soilBelowM);
                }
            }
            for (int i = 0; i + 1 < rules.waterTiles.Length; i += 2)
                if (terrain.InBounds(rules.waterTiles[i], rules.waterTiles[i + 1]))
                    terrain.SetWater(rules.waterTiles[i], rules.waterTiles[i + 1], true);
            return terrain;
        }

        public static TerrainGrid LoadMars(Balance balance)
        {
            return FromElevation(FixtureLoader.Jezero(), balance.mars);
        }

        // Largest height step to a neighbour (metres per 3.7 km tile).
        public static double Slope(ElevationGrid g, int row, int col)
        {
            double h = g.metres[row, col], worst = 0;
            int[] dr = { -1, 1, 0, 0 }, dc = { 0, 0, -1, 1 };
            for (int k = 0; k < 4; k++)
            {
                int r = row + dr[k], c = col + dc[k];
                if (r < 0 || c < 0 || r >= g.height || c >= g.width) continue;
                worst = Math.Max(worst, Math.Abs(h - g.metres[r, c]));
            }
            return worst;
        }

        static void MarkSoil(TerrainGrid terrain, int[] tiles)
        {
            for (int i = 0; i + 1 < tiles.Length; i += 2)
                if (terrain.InBounds(tiles[i], tiles[i + 1]) && !terrain.IsBlocked(tiles[i], tiles[i + 1]))
                    terrain.SetSoil(tiles[i], tiles[i + 1], true);
        }
    }
}
