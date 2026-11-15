namespace Mapmakers.Sim
{
    public enum PlacementReason { Ok, OutOfBounds, BlockedTerrain, Overlap, TooExpensive, NeedSoil, MaxCount, Locked, HabitatFirst, OutsideBase }

    public struct PlacementResult
    {
        public PlacementReason reason;
        public string message;
        public PlacedBuilding building;
        public bool Ok => reason == PlacementReason.Ok;

        public static PlacementResult Fail(PlacementReason reason, string message)
        {
            return new PlacementResult { reason = reason, message = message };
        }
    }

    public static class Placement
    {
        public static bool IsUnlocked(SimState s, BuildingType type)
        {
            return type != BuildingType.Shelter || s.shelterUnlocked;
        }

        public static int MaxCount(Simulator sim, SimState s, BuildingType type)
        {
            if (type == BuildingType.Battery) return s.maxBatteries;
            int max = sim.balance.Def(type).maxCount;
            return type == BuildingType.SolarArray ? max + sim.planet.extraSolar : max;
        }

        public static PlacementResult Check(Simulator sim, SimState s, BuildingType type, int x, int y)
        {
            var balance = sim.balance;
            var terrain = sim.terrain;
            var def = balance.Def(type);
            if (!IsUnlocked(s, type))
                return PlacementResult.Fail(PlacementReason.Locked, def.displayName + " is locked for now.");
            if (s.Count(type) >= MaxCount(sim, s, type))
                return PlacementResult.Fail(PlacementReason.MaxCount, "You already have the most " + def.displayName + " allowed.");
            if (def.cost > s.supplies)
                return PlacementResult.Fail(PlacementReason.TooExpensive, "Not enough supplies: costs " + def.cost + ", you have " + s.supplies + ".");
            if (sim.layoutRules && def.soilCost > s.soil + 1e-6)
                return PlacementResult.Fail(PlacementReason.NeedSoil, "Needs " + def.soilCost + " bags of soil. A rover brings soil.");
            if (sim.layoutRules && type != BuildingType.Habitat && s.Count(BuildingType.Habitat) == 0)
                return PlacementResult.Fail(PlacementReason.HabitatFirst, "Place the Habitat first. The base grows around it.");

            for (int dy = 0; dy < def.height; dy++)
            {
                for (int dx = 0; dx < def.width; dx++)
                {
                    int tx = x + dx, ty = y + dy;
                    if (!terrain.InBounds(tx, ty))
                        return PlacementResult.Fail(PlacementReason.OutOfBounds, "That is off the map.");
                    if (terrain.IsBlocked(tx, ty))
                        return PlacementResult.Fail(PlacementReason.BlockedTerrain, "Rocks or a crater are in the way.");
                    if (BuildingAt(balance, s, tx, ty) != null)
                        return PlacementResult.Fail(PlacementReason.Overlap, "Another building is already there.");
                }
            }
            if (sim.layoutRules && type != BuildingType.Habitat && !InsideBase(sim, s, x, y, def.width, def.height))
                return PlacementResult.Fail(PlacementReason.OutsideBase, "Too far from the Habitat. Stay inside the base area.");
            return new PlacementResult { reason = PlacementReason.Ok, message = "" };
        }

        public static bool InsideBase(Simulator sim, SimState s, int x, int y, int w, int h)
        {
            return Layout.GapTo(sim.balance, s, BuildingType.Habitat, x, y, w, h) <= sim.balance.layout.baseRadius;
        }

        public static bool InsideBaseTile(Simulator sim, SimState s, int x, int y)
        {
            if (!sim.layoutRules) return true;
            var habitat = s.First(BuildingType.Habitat);
            if (habitat == null) return true;
            return InsideBase(sim, s, x, y, 1, 1);
        }

        public static PlacedBuilding BuildingAt(Balance balance, SimState s, int x, int y)
        {
            foreach (var b in s.buildings)
            {
                var def = balance.Def(b.type);
                if (x >= b.x && x < b.x + def.width && y >= b.y && y < b.y + def.height) return b;
            }
            return null;
        }
    }
}
