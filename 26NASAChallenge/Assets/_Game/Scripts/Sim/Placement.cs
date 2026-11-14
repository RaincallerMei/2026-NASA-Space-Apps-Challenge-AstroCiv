namespace Mapmakers.Sim
{
    public enum PlacementReason { Ok, OutOfBounds, BlockedTerrain, Overlap, TooExpensive, MaxCount, Locked }

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

        public static int MaxCount(Balance balance, SimState s, BuildingType type)
        {
            return type == BuildingType.Battery ? s.maxBatteries : balance.Def(type).maxCount;
        }

        public static PlacementResult Check(Balance balance, TerrainGrid terrain, SimState s, BuildingType type, int x, int y)
        {
            var def = balance.Def(type);
            if (!IsUnlocked(s, type))
                return PlacementResult.Fail(PlacementReason.Locked, def.displayName + " is locked for now.");
            if (s.Count(type) >= MaxCount(balance, s, type))
                return PlacementResult.Fail(PlacementReason.MaxCount, "You already have the most " + def.displayName + " allowed.");
            if (def.cost > s.supplies)
                return PlacementResult.Fail(PlacementReason.TooExpensive, "Not enough supplies: costs " + def.cost + ", you have " + s.supplies + ".");

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
            return new PlacementResult { reason = PlacementReason.Ok, message = "" };
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
