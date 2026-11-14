using Mapmakers.Sim;

namespace Mapmakers.Data
{
    // Turns the real illumination grid into game terrain. The mapping is a simplification:
    // sunFraction = floor + (1 - floor) * min(1, percentLit / fullAtPercent).
    public static class TerrainFactory
    {
        public static TerrainGrid FromIllumination(IlluminationGrid grid, TerrainRules rules)
        {
            var terrain = new TerrainGrid(grid.width, grid.height);
            for (int row = 0; row < grid.height; row++)
            {
                int y = grid.height - 1 - row;
                for (int x = 0; x < grid.width; x++)
                {
                    double percent = grid.percent[row, x];
                    double lit = System.Math.Min(1.0, percent / rules.sunFullAtPercent);
                    terrain.SetSun(x, y, rules.sunFloor + (1 - rules.sunFloor) * lit);
                    terrain.SetBlocked(x, y, percent < rules.blockedBelowPercent);
                }
            }
            for (int i = 0; i + 1 < rules.rockTiles.Length; i += 2)
                if (terrain.InBounds(rules.rockTiles[i], rules.rockTiles[i + 1]))
                    terrain.SetBlocked(rules.rockTiles[i], rules.rockTiles[i + 1], true);
            return terrain;
        }

        public static TerrainGrid Load(Balance balance)
        {
            return FromIllumination(FixtureLoader.Illumination(), balance.terrain);
        }
    }
}
