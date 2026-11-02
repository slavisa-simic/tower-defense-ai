using System;
using UnityEngine;

namespace ShorelineDefense.Grid
{
    // The Phase 1 asset explicitly selects LegacyXZ; new definitions and grids default to XY.
    public enum GameplayPlane { LegacyXZ = 0, XY = 1 }

    /// <summary>Axis-aligned grid. Origin is the minimum outer corner; positions are cell centers.</summary>
    public sealed class BattlefieldGrid
    {
        private readonly GridCell[] cells;
        private readonly object[] occupants;
        private readonly object[] traversalOwners;
        private readonly float[] traversalMultipliers;
        private readonly float[] movementMultipliers;
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public Vector3 Origin { get; }
        public GameplayPlane Plane { get; }
        public int Revision { get; private set; }
        public int CellCount => cells.Length;
        public event Action Changed;
        // Build occupancy is deliberately separate from navigation revisions.
        public event Action OccupancyChanged;

        public BattlefieldGrid(int width, int height, float cellSize, Vector3 origin, GameplayPlane plane = GameplayPlane.XY)
        {
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (float.IsNaN(cellSize) || float.IsInfinity(cellSize) || cellSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(cellSize));
            Width = width;
            Height = height;
            CellSize = cellSize;
            Origin = origin;
            Plane = plane;
            cells = new GridCell[checked(width * height)];
            occupants = new object[cells.Length];
            traversalOwners = new object[cells.Length];
            traversalMultipliers = new float[cells.Length];
            movementMultipliers = new float[cells.Length];
            for (int i = 0; i < cells.Length; i++)
                cells[i] = new GridCell(TerrainType.Ground, true, true);
        }

        public bool Contains(Vector2Int cell) => cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;

        public bool ContainsFootprint(Vector2Int anchor, Vector2Int footprint) =>
            footprint.x > 0 && footprint.y > 0 && Contains(anchor) &&
            footprint.x <= Width - anchor.x && footprint.y <= Height - anchor.y;

        public object OccupantAt(Vector2Int cell)
        {
            RequireBounds(cell);
            return occupants[cell.y * Width + cell.x];
        }

        public bool CanOccupy(Vector2Int anchor, Vector2Int footprint)
        {
            if (!ContainsFootprint(anchor, footprint)) return false;
            for (int y = 0; y < footprint.y; y++)
            for (int x = 0; x < footprint.x; x++)
                if (OccupantAt(anchor + new Vector2Int(x, y)) != null) return false;
            return true;
        }

        public bool TryOccupy(Vector2Int anchor, Vector2Int footprint, object owner)
        {
            if (owner == null || !CanOccupy(anchor, footprint)) return false;
            for (int y = 0; y < footprint.y; y++)
            for (int x = 0; x < footprint.x; x++)
                occupants[(anchor.y + y) * Width + anchor.x + x] = owner;
            OccupancyChanged?.Invoke();
            return true;
        }

        public void ClearOccupied(Vector2Int anchor, Vector2Int footprint, object owner)
        {
            if (owner == null || !ContainsFootprint(anchor, footprint)) return;
            bool changed = false;
            for (int y = 0; y < footprint.y; y++)
            for (int x = 0; x < footprint.x; x++)
            {
                int index = (anchor.y + y) * Width + anchor.x + x;
                if (!ReferenceEquals(occupants[index], owner)) continue;
                occupants[index] = null;
                changed = true;
            }
            if (changed) OccupancyChanged?.Invoke();
        }

        // Also supports out-of-bounds cursor previews. Anchor is the lower-left cell.
        public Vector3 FootprintCenter(Vector2Int anchor, Vector2Int footprint) => Origin +
            (Plane == GameplayPlane.XY
                ? new Vector3((anchor.x + footprint.x * 0.5f) * CellSize, (anchor.y + footprint.y * 0.5f) * CellSize, 0)
                : new Vector3((anchor.x + footprint.x * 0.5f) * CellSize, 0, (anchor.y + footprint.y * 0.5f) * CellSize));

        public bool TryWorldToCell(Vector3 world, out Vector2Int cell)
        {
            cell = new Vector2Int(Mathf.FloorToInt((world.x - Origin.x) / CellSize),
                Mathf.FloorToInt((Plane == GameplayPlane.XY ? world.y - Origin.y : world.z - Origin.z) / CellSize));
            return Contains(cell);
        }

        public Vector3 CellToWorld(Vector2Int cell)
        {
            RequireBounds(cell);
            float x = (cell.x + 0.5f) * CellSize;
            float y = (cell.y + 0.5f) * CellSize;
            return Origin + (Plane == GameplayPlane.XY ? new Vector3(x, y, 0) : new Vector3(x, 0, y));
        }

        public Vector3 ProjectToPlane(Vector3 position) => Plane == GameplayPlane.XY
            ? new Vector3(position.x, position.y, Origin.z)
            : new Vector3(position.x, Origin.y, position.z);

        public GridCell GetCell(Vector2Int cell)
        {
            RequireBounds(cell);
            int index = cell.y * Width + cell.x;
            var value = cells[index];
            return traversalOwners[index] == null ? value : new GridCell(value.Terrain, value.Walkable,
                value.Buildable, value.TraversalCost * traversalMultipliers[index]);
        }

        public float MovementMultiplierAt(Vector3 position) => TryWorldToCell(position, out var cell) &&
            traversalOwners[cell.y * Width + cell.x] != null ? movementMultipliers[cell.y * Width + cell.x] : 1f;

        // A placed area owns its overlay. Terrain edits and removal never overwrite each other.
        // Build occupancy prevents overlapping area footprints; status effects can still stack on enemies.
        public bool TrySetTraversalModifier(Vector2Int anchor, Vector2Int footprint, object owner, float cost, float movement)
        {
            if (owner == null || !ContainsFootprint(anchor, footprint)) return false;
            if (float.IsNaN(cost) || float.IsInfinity(cost) || cost < 1 ||
                float.IsNaN(movement) || float.IsInfinity(movement) || movement <= 0 || movement > 1)
                throw new ArgumentOutOfRangeException(nameof(cost));
            for (int y = 0; y < footprint.y; y++)
            for (int x = 0; x < footprint.x; x++)
            {
                int i = (anchor.y + y) * Width + anchor.x + x;
                if (!ReferenceEquals(occupants[i], owner) ||
                    (traversalOwners[i] != null && !ReferenceEquals(traversalOwners[i], owner))) return false;
            }
            for (int y = 0; y < footprint.y; y++)
            for (int x = 0; x < footprint.x; x++)
            {
                int i = (anchor.y + y) * Width + anchor.x + x;
                traversalOwners[i] = owner;
                traversalMultipliers[i] = cost;
                movementMultipliers[i] = movement;
            }
            Revision++;
            Changed?.Invoke();
            return true;
        }

        public void ClearTraversalModifier(Vector2Int anchor, Vector2Int footprint, object owner)
        {
            if (owner == null || !ContainsFootprint(anchor, footprint)) return;
            bool changed = false;
            for (int y = 0; y < footprint.y; y++)
            for (int x = 0; x < footprint.x; x++)
            {
                int i = (anchor.y + y) * Width + anchor.x + x;
                if (!ReferenceEquals(traversalOwners[i], owner)) continue;
                traversalOwners[i] = null;
                changed = true;
            }
            if (changed) { Revision++; Changed?.Invoke(); }
        }

        public void SetCell(Vector2Int cell, GridCell value)
        {
            RequireBounds(cell);
            if (value.TraversalCost < 1f) throw new ArgumentException("An initialized GridCell is required.", nameof(value));
            cells[cell.y * Width + cell.x] = value;
            Revision++;
            Changed?.Invoke();
        }

        private void RequireBounds(Vector2Int cell)
        {
            if (!Contains(cell)) throw new ArgumentOutOfRangeException(nameof(cell), cell, "Outside grid bounds.");
        }
    }
}
