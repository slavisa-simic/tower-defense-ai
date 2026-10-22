using ShorelineDefense.Defenses;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Building
{
    public enum PlacementResult { Valid, NotPlaying, InvalidDefinition, OutsideGrid, NotBuildable, Occupied, InsufficientMoney, CreationFailed, Busy }

    public static class PlacementRules
    {
        public static PlacementResult Validate(BattlefieldGrid grid, Vector2Int anchor, Vector2Int footprint, int cost, int money)
        {
            if (grid == null || !grid.ContainsFootprint(anchor, footprint)) return PlacementResult.OutsideGrid;
            for (int y = 0; y < footprint.y; y++)
            for (int x = 0; x < footprint.x; x++)
            {
                var cell = grid.GetCell(anchor + new Vector2Int(x, y));
                if (cell.Terrain != TerrainType.Ground || !cell.Buildable) return PlacementResult.NotBuildable;
            }
            if (!grid.CanOccupy(anchor, footprint)) return PlacementResult.Occupied;
            if (cost < 0 || money < cost) return PlacementResult.InsufficientMoney;
            return PlacementResult.Valid;
        }
    }
}
