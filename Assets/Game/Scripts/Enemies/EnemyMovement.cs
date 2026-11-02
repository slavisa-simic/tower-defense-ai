using System.Collections.Generic;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Enemies
{
    public sealed class EnemyMovement : MonoBehaviour
    {
        private readonly List<Vector2Int> path = new List<Vector2Int>(64);
        private BattlefieldGrid grid;
        private PathfindingService paths;
        private Vector2Int destination;
        private Vector3 targetPosition;
        private float speed;
        private float stoppingDistance;
        private int waypoint;
        private bool pathDirty;
        private int pathRevision = -1;
        private float repathDelay;
        private float obstructionTailDistance;
        private IReadOnlyList<Vector2Int> goals;
        private readonly List<Vector2Int> candidatePath = new List<Vector2Int>(64);
        public bool FollowingObstructionRoute { get; private set; }
        public bool RepathPending => pathDirty;
        public IReadOnlyList<Vector2Int> Goals => goals;
        public BattlefieldGrid Grid => grid;
        public PathfindingService Navigation => paths;

        public bool HasPath { get; private set; }
        public float DistanceTravelled { get; private set; }
        public bool ReachedDestination { get; private set; }
        public IReadOnlyList<Vector2Int> Path => path;
        public Vector3 FacingDirection { get; private set; } = Vector3.right;
        public GameplayPlane Plane => grid?.Plane ?? GameplayPlane.XY;
        public MovementModifiers Modifiers { get; } = new MovementModifiers();
        public float BaseSpeed => speed;
        public bool IgnoreWireSlow { get; set; }
        public float CurrentSpeed => speed * Modifiers.Multiplier * (IgnoreWireSlow ? 1 : grid?.MovementMultiplierAt(transform.position) ?? 1f);
        // Four-way route suffixes have equal-length cell edges; query is O(1), without another A*.
        public float RemainingPathDistance => !HasPath || pathDirty ? float.PositiveInfinity :
            (FollowingObstructionRoute ? obstructionTailDistance : 0) + (ReachedDestination || waypoint >= path.Count ? 0 : Mathf.Max(0,
                Vector3.Distance(transform.position, grid.CellToWorld(path[waypoint])) +
                (path.Count - waypoint - 1) * grid.CellSize - stoppingDistance));

        public void Initialize(BattlefieldGrid battlefield, PathfindingService navigation, Vector2Int goal,
            float movementSpeed, float stopDistance, IReadOnlyList<Vector2Int> destinations = null)
        {
            Unsubscribe();
            grid = battlefield;
            paths = navigation;
            destination = goal;
            goals = destinations != null && destinations.Count > 0 ? destinations : new[] { goal };
            targetPosition = grid.CellToWorld(goal);
            transform.position = grid.ProjectToPlane(transform.position);
            speed = movementSpeed;
            Modifiers.Clear();
            DistanceTravelled = 0;
            IgnoreWireSlow = false;
            stoppingDistance = stopDistance;
            borderStoppingDistance = stopDistance;
            if (isActiveAndEnabled) grid.Changed += InvalidatePath;
            RecalculatePath();
        }

        public void Tick(float deltaTime)
        {
            if (grid == null || deltaTime <= 0) return;
            PrepareNavigation(deltaTime);
            Advance(deltaTime);
        }

        // EnemyController prepares navigation once, then abilities/combat advance the same clock.
        public void Advance(float deltaTime)
        {
            if (grid == null || deltaTime <= 0) return;
            if (!HasPath || ReachedDestination) return;

            float remaining = deltaTime;
            while (remaining > 0 && waypoint < path.Count)
            {
                if (WithinRange()) { ReachedDestination = true; return; }
                if (!grid.IsTraversable(path[waypoint])) { StopForObstruction(); return; }
                Vector3 next = waypoint == path.Count - 1 ? targetPosition : grid.CellToWorld(path[waypoint]);
                Vector3 offset = next - transform.position;
                float distance = offset.magnitude;
                if (distance <= 0.0001f) { waypoint++; continue; }
                Vector3 direction = offset / distance;
                // Split at cell boundaries so large ticks cannot skip slow areas or retain an exited slow.
                float effectiveSpeed = speed * Modifiers.Multiplier * (IgnoreWireSlow ? 1 : grid.MovementMultiplierAt(transform.position + direction * 0.0001f));
                if (effectiveSpeed <= 0) return;
                float step = Mathf.Min(Mathf.Min(remaining * effectiveSpeed, distance), DistanceToCellEdge(direction));
                // Clamp the last segment at attack range, even on a long frame.
                if (path[waypoint] == destination) step = Mathf.Min(step, Mathf.Max(0, distance - stoppingDistance));
                FacingDirection = offset.normalized;
                // Check the section being entered, including a boundary crossed during a large tick.
                if (!grid.TryWorldToCell(transform.position + direction * (step + .0001f), out var entering) || !grid.IsTraversable(entering))
                { StopForObstruction(); return; }
                transform.position += offset.normalized * step;
                DistanceTravelled += step;
                remaining -= step / effectiveSpeed;
                if (WithinRange()) { ReachedDestination = true; return; }
                if (step >= distance) waypoint++;
                else if (step <= 0.000001f) break;
            }
            ReachedDestination = WithinRange();
        }

        private float borderStoppingDistance;
        public void PrepareNavigation(float deltaTime)
        {
            if (!pathDirty || grid == null || deltaTime <= 0) return;
            repathDelay -= deltaTime;
            if (repathDelay <= 0 && paths.TryReserveRepath()) RecalculatePath();
        }

        private void StopForObstruction()
        {
            HasPath = false; ReachedDestination = false;
            if (!pathDirty) { pathDirty = true; repathDelay = 0; }
        }

        public void FollowObstructionRoute(List<Vector2Int> approach, Vector2 contactOffset, float remainingBeyondApproach = 0)
        {
            obstructionTailDistance = remainingBeyondApproach;
            path.Clear(); path.AddRange(approach); waypoint = 0;
            destination = path[path.Count - 1];
            targetPosition = grid.CellToWorld(destination) + (Plane == GameplayPlane.XY
                ? new Vector3(contactOffset.x, contactOffset.y, 0) : new Vector3(contactOffset.x, 0, contactOffset.y));
            stoppingDistance = 0;
            HasPath = true; ReachedDestination = false; FollowingObstructionRoute = true;
            pathDirty = false; pathRevision = grid.Revision;
        }

        public void ResumeBorderRoute()
        {
            FollowingObstructionRoute = false; HasPath = false; ReachedDestination = false;
            pathDirty = true; repathDelay = 0;
        }

        private float DistanceToCellEdge(Vector3 direction)
        {
            Vector3 probe = transform.position + direction * 0.0001f;
            if (!grid.TryWorldToCell(probe, out var cell)) return grid.CellSize;
            Vector3 center = grid.CellToWorld(cell);
            float half = grid.CellSize * 0.5f;
            float dx = Mathf.Abs(direction.x) < 0.00001f ? float.PositiveInfinity :
                (center.x + Mathf.Sign(direction.x) * half - transform.position.x) / direction.x;
            float axis = Plane == GameplayPlane.XY ? direction.y : direction.z;
            float position = Plane == GameplayPlane.XY ? transform.position.y : transform.position.z;
            float centerAxis = Plane == GameplayPlane.XY ? center.y : center.z;
            float dy = Mathf.Abs(axis) < 0.00001f ? float.PositiveInfinity :
                (centerAxis + Mathf.Sign(axis) * half - position) / axis;
            return Mathf.Max(0.0001f, Mathf.Min(dx, dy));
        }

        public void FaceTowards(Vector3 position)
        {
            if (grid == null) return;
            Vector3 direction = grid.ProjectToPlane(position) - transform.position;
            if (direction.sqrMagnitude > 0.001f) FacingDirection = direction.normalized;
        }

        private bool WithinRange() => (transform.position - targetPosition).sqrMagnitude <=
            (stoppingDistance + 0.001f) * (stoppingDistance + 0.001f);

        private void RecalculatePath()
        {
            pathRevision = grid.Revision;
            pathDirty = false;
            waypoint = 0;
            ReachedDestination = false;
            path.Clear();
            FollowingObstructionRoute = false; stoppingDistance = borderStoppingDistance;
            HasPath = false;
            if (!grid.TryWorldToCell(transform.position, out var start)) return;
            float best = float.PositiveInfinity;
            foreach (var goal in goals)
            {
                if (!paths.TryFindPath(start, goal, candidatePath)) continue;
                float cost = 0;
                foreach (var cell in candidatePath) cost += grid.GetCell(cell).TraversalCost;
                if (cost >= best) continue;
                best = cost; destination = goal; path.Clear(); path.AddRange(candidatePath); HasPath = true;
            }
            if (HasPath) targetPosition = grid.CellToWorld(destination);
            // On a grid edit, return through the current cell center before following the new route.
            // A blocked/unreachable start waits for another grid change; it never walks through blockers.
        }

        private void InvalidatePath()
        {
            if (!pathDirty) repathDelay = .02f * ((GetInstanceID() & int.MaxValue) % 16);
            pathDirty = true; ReachedDestination = false;
        }
        private void OnEnable() { if (grid != null) { grid.Changed += InvalidatePath;
            pathDirty = pathRevision != grid.Revision || path.Count == 0 || !grid.TryWorldToCell(transform.position, out var start) || path[0] != start;
        } }
        private void OnDisable() => Unsubscribe();
        private void Unsubscribe() { if (grid != null) grid.Changed -= InvalidatePath; }

        private void OnDrawGizmosSelected()
        {
            if (grid == null) return;
            Gizmos.color = HasPath ? Color.cyan : Color.red;
            Vector3 lift = Plane == GameplayPlane.XY ? Vector3.back * 0.15f : Vector3.up * 0.15f;
            Vector3 previous = transform.position + lift;
            for (int i = waypoint; i < path.Count; i++)
            {
                Vector3 next = grid.CellToWorld(path[i]) + lift;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, stoppingDistance);
        }
    }
}
