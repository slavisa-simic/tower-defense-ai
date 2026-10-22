using System;

namespace ShorelineDefense.Grid
{
    public enum TerrainType { Water, Beach, Ground, Base, Blocked }

    public readonly struct GridCell
    {
        public TerrainType Terrain { get; }
        public bool Walkable { get; }
        public bool Buildable { get; }
        public float TraversalCost { get; }

        public GridCell(TerrainType terrain, bool walkable, bool buildable, float traversalCost = 1f)
        {
            if (float.IsNaN(traversalCost) || float.IsInfinity(traversalCost) || traversalCost < 1f)
                throw new ArgumentOutOfRangeException(nameof(traversalCost), "Traversal cost must be finite and at least one.");
            Terrain = terrain;
            Walkable = walkable;
            Buildable = buildable;
            TraversalCost = traversalCost;
        }
    }
}
