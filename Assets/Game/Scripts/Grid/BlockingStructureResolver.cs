using System.Collections.Generic;
using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Grid
{
    /// <summary>One reverse field per exit/revision, shared across the crowd. Only player walls can be breached.</summary>
    public sealed class BlockingStructureResolver
    {
        private sealed class Field
        {
            public float[] cost;
            public int[] next;
        }
        private readonly BattlefieldGrid grid;
        private readonly Dictionary<Vector2Int, Field> fields = new Dictionary<Vector2Int, Field>();
        private readonly int[] open;
        private readonly bool[] closed;
        private int revision = -1;
        public int FieldBuilds { get; private set; }
        public BlockingStructureResolver(BattlefieldGrid battlefield)
        { grid = battlefield; open = new int[grid.CellCount]; closed = new bool[grid.CellCount]; }
        private int Index(Vector2Int cell) => cell.y * grid.Width + cell.x;
        private Vector2Int Cell(int index) => new Vector2Int(index % grid.Width, index / grid.Width);
        private bool CanBreach(Vector2Int cell)
        {
            var blocker = grid.NavigationBlockerAt(cell);
            return blocker == null || blocker is DefenseBase d && d.IsAlive && d.Definition.BlocksMovement;
        }
        private Field GetField(Vector2Int goal)
        {
            if (revision != grid.Revision) { fields.Clear(); revision = grid.Revision; }
            if (fields.TryGetValue(goal, out var field)) return field;
            field = new Field { cost = new float[grid.CellCount], next = new int[grid.CellCount] };
            fields.Add(goal, field); FieldBuilds++;
            System.Array.Clear(closed, 0, closed.Length);
            for (int i = 0; i < grid.CellCount; i++) { field.cost[i] = float.PositiveInfinity; field.next[i] = -1; }
            if (!grid.IsStaticWalkable(goal) || !CanBreach(goal)) return field;
            int count = 1; open[0] = Index(goal); field.cost[open[0]] = 0;
            // Map grids are 576 cells. Reused linear workspace avoids per-enemy searches/heap allocations.
            while (count > 0)
            {
                int best = 0;
                for (int i = 1; i < count; i++) if (field.cost[open[i]] < field.cost[open[best]]) best = i;
                int current = open[best]; open[best] = open[--count]; closed[current] = true;
                var cell = Cell(current);
                float step = grid.GetCell(cell).TraversalCost + (grid.NavigationBlockerAt(cell) == null ? 0 : grid.CellCount * 16f);
                for (int d = 0; d < 4; d++)
                {
                    var neighbor = cell + (d == 0 ? Vector2Int.left : d == 1 ? Vector2Int.down : d == 2 ? Vector2Int.up : Vector2Int.right);
                    if (!grid.IsStaticWalkable(neighbor) || !CanBreach(neighbor)) continue;
                    int index = Index(neighbor);
                    if (closed[index] || field.cost[current] + step >= field.cost[index]) continue;
                    if (float.IsPositiveInfinity(field.cost[index])) open[count++] = index;
                    field.cost[index] = field.cost[current] + step; field.next[index] = current;
                }
            }
            return field;
        }

        public bool CanReachByBreaching(Vector2Int start, Vector2Int goal) => grid.IsTraversable(start) &&
            grid.Contains(goal) && !float.IsPositiveInfinity(GetField(goal).cost[Index(start)]);

        public bool TryResolve(Vector2Int start, IReadOnlyList<Vector2Int> goals, List<Vector2Int> approach,
            out DefenseBase target, out Vector2Int goal)
            => TryResolve(start, goals, approach, out target, out goal, out _);

        public bool TryResolve(Vector2Int start, IReadOnlyList<Vector2Int> goals, List<Vector2Int> approach,
            out DefenseBase target, out Vector2Int goal, out float distanceAfterApproach)
        {
            target = null; goal = default; distanceAfterApproach = 0; approach.Clear();
            if (!grid.IsTraversable(start)) return false;
            Field chosen = null; float best = float.PositiveInfinity;
            foreach (var destination in goals)
            {
                if (!grid.Contains(destination)) continue;
                var field = GetField(destination);
                if (field.cost[Index(start)] >= best) continue;
                chosen = field; best = field.cost[Index(start)]; goal = destination;
            }
            if (chosen == null) return false;
            int node = Index(start);
            for (int steps = 0; steps < grid.CellCount && node >= 0; steps++)
            {
                var cell = Cell(node);
                if (grid.NavigationBlockerAt(cell) is DefenseBase defense)
                {
                    // Keep objective progress meaningful for ClosestToBase targeting while attacking.
                    // Include the first edge from the contact cell into this wall, then its route tail.
                    int edges = 1;
                    for (int tail = node, n = 0; chosen.next[tail] >= 0 && n < grid.CellCount; n++)
                    { tail = chosen.next[tail]; edges++; }
                    distanceAfterApproach = edges * grid.CellSize;
                    target = defense; return approach.Count > 0 && defense.IsAlive;
                }
                if (!grid.IsTraversable(cell)) break;
                approach.Add(cell); node = chosen.next[node];
            }
            approach.Clear(); return false;
        }
    }
}
