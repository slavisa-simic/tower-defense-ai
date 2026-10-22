using ShorelineDefense.Defenses;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    /// <summary>Four local occupancy reads, independent of traversal rules.</summary>
    public static class WireConnectivity
    {
        public static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        public static int Mask(bool north, bool east, bool south, bool west) =>
            (north ? 1 : 0) | (east ? 2 : 0) | (south ? 4 : 0) | (west ? 8 : 0);
        public static int At(BattlefieldGrid grid, Vector2Int cell)
        {
            int mask = 0;
            if (grid == null) return mask;
            for (int i = 0; i < Directions.Length; i++)
            {
                var adjacent = cell + Directions[i];
                if (grid.Contains(adjacent) && grid.OccupantAt(adjacent) is DefenseBase defense &&
                    defense != null && defense.IsAlive && defense.GetComponent<BarbedWire>() != null && defense.isActiveAndEnabled)
                    mask |= 1 << i;
            }
            return mask;
        }
    }
}
