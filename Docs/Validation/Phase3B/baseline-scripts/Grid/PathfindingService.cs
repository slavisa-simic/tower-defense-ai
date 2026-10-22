using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShorelineDefense.Grid
{
    /// <summary>Reusable synchronous four-way A*. Costs are paid when entering a cell.</summary>
    public sealed class PathfindingService
    {
        private readonly BattlefieldGrid grid;
        private readonly float[] costs;
        private readonly int[] parents;
        private readonly byte[] states;
        private readonly int[] open;
        public int Requests { get; private set; }

        public PathfindingService(BattlefieldGrid grid)
        {
            this.grid = grid ?? throw new ArgumentNullException(nameof(grid));
            costs = new float[grid.CellCount];
            parents = new int[grid.CellCount];
            states = new byte[grid.CellCount];
            open = new int[grid.CellCount];
        }

        // Caller owns the result buffer. Workspace is reused; call on one simulation thread.
        public bool TryFindPath(Vector2Int start, Vector2Int goal, List<Vector2Int> result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            result.Clear();
            Requests++;
            if (!grid.Contains(start) || !grid.Contains(goal) ||
                !grid.GetCell(start).Walkable || !grid.GetCell(goal).Walkable) return false;

            Array.Clear(states, 0, states.Length);
            int startIndex = Index(start);
            int goalIndex = Index(goal);
            int openCount = 1;
            open[0] = startIndex;
            costs[startIndex] = 0;
            parents[startIndex] = -1;
            states[startIndex] = 1;

            while (openCount > 0)
            {
                int bestSlot = 0;
                float bestScore = float.PositiveInfinity;
                float bestHeuristic = float.PositiveInfinity;
                for (int i = 0; i < openCount; i++)
                {
                    float h = Heuristic(Cell(open[i]), goal);
                    float score = costs[open[i]] + h;
                    if (score < bestScore || (score == bestScore && h < bestHeuristic))
                    {
                        bestSlot = i;
                        bestScore = score;
                        bestHeuristic = h;
                    }
                }

                int current = open[bestSlot];
                open[bestSlot] = open[--openCount];
                if (current == goalIndex)
                {
                    for (int node = current; node != -1; node = parents[node]) result.Add(Cell(node));
                    result.Reverse();
                    return true;
                }
                states[current] = 2;
                Vector2Int currentCell = Cell(current);
                for (int direction = 0; direction < 4; direction++)
                {
                    var next = currentCell + (direction == 0 ? Vector2Int.down : direction == 1 ? Vector2Int.left :
                        direction == 2 ? Vector2Int.right : Vector2Int.up);
                    if (!grid.Contains(next)) continue;
                    GridCell terrain = grid.GetCell(next);
                    int nextIndex = Index(next);
                    if (!terrain.Walkable || states[nextIndex] == 2) continue;
                    float candidate = costs[current] + terrain.TraversalCost;
                    if (states[nextIndex] == 1 && candidate >= costs[nextIndex]) continue;
                    costs[nextIndex] = candidate;
                    parents[nextIndex] = current;
                    if (states[nextIndex] == 0)
                    {
                        states[nextIndex] = 1;
                        open[openCount++] = nextIndex;
                    }
                }
            }
            return false;
        }

        private int Index(Vector2Int cell) => cell.y * grid.Width + cell.x;
        private Vector2Int Cell(int index) => new Vector2Int(index % grid.Width, index / grid.Width);
        private static float Heuristic(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
