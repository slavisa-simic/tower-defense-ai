using UnityEngine;

namespace ShorelineDefense.Enemies
{
    /// <summary>One limited route-following bypass. Never crosses blocked cells or teleports.</summary>
    public sealed class BurrowerTraversal : MonoBehaviour
    {
        [SerializeField, Range(1, 8)] private float bypassCells = 5;
        [SerializeField, Min(3)] private float finalSafetyCells = 8;
        [SerializeField, Min(.2f)] private float surfaceSeconds = 1;
        private EnemyController enemy;
        private float surface, remaining, cellSize;
        private bool used;
        public bool IsBurrowed { get; private set; }
        public float DistanceBurrowed { get; private set; }
        public void SurfaceForObstruction()
        {
            used = true; remaining = 0; IsBurrowed = false;
            if (enemy != null) enemy.Movement.IgnoreWireSlow = false;
        }
        public void Initialize(EnemyController owner, float size)
        { enemy = owner; cellSize = size; surface = surfaceSeconds; remaining = 0; used = false; IsBurrowed = false; DistanceBurrowed = 0; }
        public void Move(float dt, bool prepareNavigation = true)
        {
            if (dt <= 0) return;
            var movement = enemy.Movement;
            if (prepareNavigation) movement.PrepareNavigation(dt);
            if (!used)
            {
                float above = Mathf.Min(surface, dt); movement.Advance(above); surface -= above; dt -= above;
                if (dt <= 0) return;
                used = true;
                remaining = Mathf.Min(bypassCells * cellSize, Mathf.Max(0, movement.RemainingPathDistance - finalSafetyCells * cellSize));
                IsBurrowed = remaining > .01f && float.IsFinite(remaining);
            }
            if (IsBurrowed)
            {
                remaining = Mathf.Min(remaining, Mathf.Max(0, movement.RemainingPathDistance - finalSafetyCells * cellSize));
                movement.IgnoreWireSlow = true;
                float underground = Mathf.Min(dt, remaining / Mathf.Max(.01f, movement.CurrentSpeed));
                float before = movement.DistanceTravelled; movement.Advance(underground);
                // Actual route distance includes turns and excludes time stopped by a stale path.
                float traveled = movement.DistanceTravelled - before;
                remaining -= traveled; DistanceBurrowed += traveled; dt -= underground;
                if (!movement.HasPath || remaining <= .001f || movement.RemainingPathDistance <= finalSafetyCells * cellSize + .001f)
                { IsBurrowed = false; movement.IgnoreWireSlow = false; }
                if (IsBurrowed) return;
            }
            movement.Advance(dt);
        }
        private void OnDisable() { IsBurrowed = false; if (enemy != null) enemy.Movement.IgnoreWireSlow = false; }
    }
}
