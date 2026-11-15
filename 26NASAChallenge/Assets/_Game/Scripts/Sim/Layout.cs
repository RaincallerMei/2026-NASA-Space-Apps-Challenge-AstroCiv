using System;
using System.Collections.Generic;

namespace Mapmakers.Sim
{
    public sealed class LinkCheck
    {
        public string id;
        public bool met;
        public string good;
        public string bad;
        public string Text => met ? good : bad;
    }

    // Where a building sits changes how well it works (like districts in a strategy game).
    // All ranges and effects are fictional balance from Balance.layout.
    public static class Layout
    {
        public const int NoPath = 999;

        public static int Gap(int ax, int ay, int aw, int ah, int bx, int by, int bw, int bh)
        {
            int gx = Math.Max(0, Math.Max(ax, bx) - Math.Min(ax + aw, bx + bw));
            int gy = Math.Max(0, Math.Max(ay, by) - Math.Min(ay + ah, by + bh));
            return Math.Max(gx, gy);
        }

        public static int Gap(Balance balance, PlacedBuilding a, PlacedBuilding b)
        {
            var da = balance.Def(a.type);
            var db = balance.Def(b.type);
            return Gap(a.x, a.y, da.width, da.height, b.x, b.y, db.width, db.height);
        }

        // Smallest gap from a footprint to any building of `type` (NoPath if none).
        public static int GapTo(Balance balance, SimState s, BuildingType type, int x, int y, int w, int h, int ignoreId = 0)
        {
            int best = NoPath;
            foreach (var b in s.buildings)
            {
                if (b.type != type || b.id == ignoreId) continue;
                var d = balance.Def(b.type);
                best = Math.Min(best, Gap(x, y, w, h, b.x, b.y, d.width, d.height));
            }
            return best;
        }

        public static int GapTo(Balance balance, SimState s, PlacedBuilding from, BuildingType type)
        {
            var d = balance.Def(from.type);
            return GapTo(balance, s, type, from.x, from.y, d.width, d.height, from.id);
        }

        // Gap from a footprint to the nearest terrain tile that matches.
        public static int GapToTile(TerrainGrid t, int x, int y, int w, int h, Func<int, int, bool> match)
        {
            int best = NoPath;
            for (int ty = 0; ty < t.height; ty++)
                for (int tx = 0; tx < t.width; tx++)
                    if (match(tx, ty)) best = Math.Min(best, Gap(x, y, w, h, tx, ty, 1, 1));
            return best;
        }

        public static int SoilGap(TerrainGrid t, int x, int y, int w, int h) => GapToTile(t, x, y, w, h, t.IsSoil);
        public static int WaterGap(TerrainGrid t, int x, int y, int w, int h) => GapToTile(t, x, y, w, h, t.IsWater);

        public static double TripHours(LayoutRules r, int gap) => gap > r.roverRange ? double.PositiveInfinity : r.roverBaseTripHours + gap * r.roverHoursPerTile;

        // Checks for one building at a position (used by the map ghost, the inspect panel and the grade).
        public static List<LinkCheck> Checks(Balance balance, TerrainGrid terrain, SimState s, BuildingType type, int x, int y, int ignoreId = 0)
        {
            var r = balance.layout;
            var def = balance.Def(type);
            int w = def.width, h = def.height;
            var list = new List<LinkCheck>();
            switch (type)
            {
                case BuildingType.SolarArray:
                    list.Add(Link("cable", GapTo(balance, s, BuildingType.Battery, x, y, w, h, ignoreId) <= r.cableGap,
                        "Short cable to a Battery.", "No Battery close by: long cable loses power."));
                    break;
                case BuildingType.Battery:
                    list.Add(Link("cable", GapTo(balance, s, BuildingType.SolarArray, x, y, w, h, ignoreId) <= r.cableGap,
                        "Close to a Solar Array.", "Put it close to a Solar Array."));
                    break;
                case BuildingType.LifeSupport:
                    list.Add(Link("duct", GapTo(balance, s, BuildingType.Habitat, x, y, w, h, ignoreId) <= r.ductGap,
                        "Short air pipe to the Habitat.", "Far from the Habitat: long air pipe, less air."));
                    list.Add(Link("plants", GapTo(balance, s, BuildingType.Greenhouse, x, y, w, h, ignoreId) <= r.plantLoopGap,
                        "Plants next door help clean the air.", "A Greenhouse next door would save power."));
                    break;
                case BuildingType.Greenhouse:
                    list.Add(Link("plants", GapTo(balance, s, BuildingType.LifeSupport, x, y, w, h, ignoreId) <= r.plantLoopGap,
                        "Shares air with Air Support.", "Put it next to Air Support to share air."));
                    break;
                case BuildingType.Shelter:
                    list.Add(Link("shelter", GapTo(balance, s, BuildingType.Habitat, x, y, w, h, ignoreId) <= r.shelterGap,
                        "Close to the Habitat: quick to reach.", "Far from the Habitat: slow to reach."));
                    break;
                case BuildingType.RoverGarage:
                    if (terrain.AnySoil())
                        list.Add(Link("soil", SoilGap(terrain, x, y, w, h) <= 2, "Soft soil is close.", "Soft soil is far: slow trips."));
                    if (terrain.AnyWater())
                        list.Add(Link("water", WaterGap(terrain, x, y, w, h) <= 3, "Water is close.", "Water is far: slow trips."));
                    break;
            }
            return list;
        }

        public static List<LinkCheck> Checks(Balance balance, TerrainGrid terrain, SimState s, PlacedBuilding b)
        {
            return Checks(balance, terrain, s, b.type, b.x, b.y, b.id);
        }

        // Optional tips: allowed spots where every link is met. Solar Arrays also want the brightest ground,
        // and ignore the Battery link until a Battery exists. Never a rule: any allowed spot can still be used.
        public static List<(int x, int y)> Suggest(Simulator sim, SimState s, BuildingType type)
        {
            var result = new List<(int x, int y)>();
            var balance = sim.balance;
            var terrain = sim.terrain;
            var def = balance.Def(type);
            bool solar = type == BuildingType.SolarArray;
            if (!solar && (!sim.layoutRules || Checks(balance, terrain, s, type, 0, 0).Count == 0)) return result;
            if (sim.layoutRules && type != BuildingType.Habitat && s.Count(BuildingType.Habitat) == 0) return result;
            bool hasBattery = s.Count(BuildingType.Battery) > 0;
            var spots = new List<(int x, int y, double sun)>();
            double bestSun = 0;
            for (int y = 0; y < terrain.height; y++)
                for (int x = 0; x < terrain.width; x++)
                {
                    if (!Placement.Fits(sim, s, type, x, y)) continue;
                    if (sim.layoutRules && !LinksMet(Checks(balance, terrain, s, type, x, y), solar && !hasBattery)) continue;
                    double sun = terrain.AverageSun(x, y, def.width, def.height);
                    bestSun = Math.Max(bestSun, sun);
                    spots.Add((x, y, sun));
                }
            foreach (var spot in spots)
                if (!solar || spot.sun >= bestSun - BrightMargin) result.Add((spot.x, spot.y));
            return result;
        }

        const double BrightMargin = 0.08;

        static bool LinksMet(List<LinkCheck> checks, bool ignoreCable)
        {
            foreach (var c in checks)
                if (!c.met && !(ignoreCable && c.id == "cable")) return false;
            return true;
        }

        static LinkCheck Link(string id, bool met, string good, string bad) => new LinkCheck { id = id, met = met, good = good, bad = bad };

        // 0..1 for the grade: how well the base uses its links. Only buildings that are present count.
        public static double Score(Balance balance, TerrainGrid terrain, SimState s, out int met, out int total)
        {
            met = total = 0;
            foreach (var b in s.buildings)
            {
                if (b.type == BuildingType.Battery) continue;
                foreach (var c in Checks(balance, terrain, s, b))
                {
                    total++;
                    if (c.met) met++;
                }
            }
            return total == 0 ? 0 : (double)met / total;
        }
    }
}
