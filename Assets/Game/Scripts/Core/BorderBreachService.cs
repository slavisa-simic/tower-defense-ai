using System;
using System.Collections.Generic;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Core
{
    /// <summary>Owns the final border destinations and breach command; kills/rewards remain separate.</summary>
    public sealed class BorderBreachService : MonoBehaviour
    {
        [SerializeField] private RunStatistics statistics;
        [SerializeField] private GameStateController game;
        [SerializeField] private GridManager grid;
        [SerializeField] private Vector2Int[] breachCells = Array.Empty<Vector2Int>();
        [SerializeField] private int maxHomicides = 1000;
        public int MaxHomicides => maxHomicides;
        public IReadOnlyList<Vector2Int> BreachCells => breachCells;
        private readonly List<Vector2Int> route = new List<Vector2Int>(64);
        public event Action<Vector3, int> Breached;
        public event Action<EnemyController, int> BossBreached;
        public void Configure(RunStatistics stats, GameStateController state, GridManager battlefield, Vector2Int[] cells)
        { statistics = stats; game = state; grid = battlefield; breachCells = cells; }
        public void ConfigureThreshold(int maximum) => maxHomicides = maximum;
        public bool TryGetDestination(Vector2Int start, out Vector2Int goal)
        {
            goal = default; float best = float.PositiveInfinity; bool found = false;
            if (grid?.Grid == null || breachCells == null) return false;
            foreach (var cell in breachCells)
            {
                if (!grid.Grid.Contains(cell) || !grid.Grid.GetCell(cell).Walkable || !grid.Paths.TryFindPath(start, cell, route)) continue;
                float cost = 0; foreach (var step in route) cost += grid.Grid.GetCell(step).TraversalCost;
                if (cost >= best) continue;
                best = cost; goal = cell; found = true;
            }
            return found;
        }
        public bool TryBreach(EnemyController enemy)
        {
            if (game == null || !game.IsPlaying || statistics == null || enemy == null || !enemy.IsTargetable || !enemy.Movement.ReachedDestination) return false;
            bool atBoundary = false;
            foreach (var cell in breachCells)
                atBoundary |= (enemy.transform.position - grid.Grid.CellToWorld(cell)).sqrMagnitude <= .15f * .15f;
            if (!atBoundary || !enemy.TryResolveBreach()) return false;
            int penalty = enemy.Definition.BreachPenalty;
            // Resolve once before observers; commit loss before registry removal can complete/reward the wave.
            try
            {
                statistics.RecordBreach(penalty);
                if (enemy.Definition is ShorelineDefense.Waves.BossDefinition) {
                    statistics.RecordBossBreach();
                    if (BossBreached != null) foreach (Action<EnemyController,int> observer in BossBreached.GetInvocationList())
                        try { observer(enemy, penalty); } catch (Exception exception) { Debug.LogException(exception, this); }
                }
                if (maxHomicides > 0 && statistics.Homicides >= maxHomicides) game.EndRun();
                if (Breached != null)
                    foreach (Action<Vector3, int> observer in Breached.GetInvocationList())
                    { try { observer(enemy.transform.position, penalty); } catch (Exception exception) { Debug.LogException(exception, this); } }
            }
            finally { enemy.RemoveBreached(); }
            return true;
        }
    }
}
