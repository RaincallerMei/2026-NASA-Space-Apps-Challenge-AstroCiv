namespace Mapmakers.Sim
{
    // Per tile: sunlight, blocked (rocks, craters, steep walls), soft soil and a water source (ice or wet clay).
    public sealed class TerrainGrid
    {
        public readonly int width;
        public readonly int height;
        readonly bool[] blocked;
        readonly bool[] soil;
        readonly bool[] water;
        readonly double[] sun;

        public TerrainGrid(int width, int height, double sunFraction = 1.0)
        {
            this.width = width;
            this.height = height;
            blocked = new bool[width * height];
            soil = new bool[width * height];
            water = new bool[width * height];
            sun = new double[width * height];
            for (int i = 0; i < sun.Length; i++) sun[i] = sunFraction;
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;
        public bool IsBlocked(int x, int y) => blocked[y * width + x];
        public bool IsSoil(int x, int y) => soil[y * width + x];
        public bool IsWater(int x, int y) => water[y * width + x];
        public double Sun(int x, int y) => sun[y * width + x];
        public void SetBlocked(int x, int y, bool value) => blocked[y * width + x] = value;
        public void SetSoil(int x, int y, bool value) => soil[y * width + x] = value;
        public void SetWater(int x, int y, bool value) => water[y * width + x] = value;
        public void SetSun(int x, int y, double value) => sun[y * width + x] = value;

        public double AverageSun(int x, int y, int w, int h)
        {
            double total = 0;
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    total += Sun(x + dx, y + dy);
            return total / (w * h);
        }

        public bool AnySoil()
        {
            foreach (bool s in soil) if (s) return true;
            return false;
        }

        public bool AnyWater()
        {
            foreach (bool w in water) if (w) return true;
            return false;
        }
    }
}
