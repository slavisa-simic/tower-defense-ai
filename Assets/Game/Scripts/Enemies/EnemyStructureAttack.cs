using System.Collections.Generic;
using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Enemies
{
    public enum StructureAttackState { Moving, PathBlocked, Approaching, Attacking }

    /// <summary>Obstruction acquisition/contact combat. EnemyController remains the simulation clock owner.</summary>
    public sealed class EnemyStructureAttack : MonoBehaviour
    {
        private EnemyController enemy;
        private EnemyMovement movement;
        private SpitterAttack spitter;
        private readonly List<Vector2Int> approach = new List<Vector2Int>(64);
        private float elapsed;
        private int resolvedRevision = -1;
        private int contactSlot;
        public DefenseBase Target { get; private set; }
        public StructureAttackState State { get; private set; }
        public int AttacksPerformed { get; private set; }
        public void Initialize(EnemyController owner)
        {
            ClearTarget(false);
            enemy = owner; movement = owner.Movement; spitter = GetComponent<SpitterAttack>();
            contactSlot = movement.Navigation.AllocateContactSlot();
            elapsed = 0; resolvedRevision = -1; AttacksPerformed = 0; State = StructureAttackState.Moving;
        }

        public bool Tick(float dt)
        {
            if (dt <= 0 || enemy == null || !enemy.IsCombatActive) return false;
            movement.PrepareNavigation(dt);
            if (Target != null && (!Target.IsAlive || !Target.gameObject.activeInHierarchy)) ClearTarget(true);
            if (movement.HasPath && !movement.FollowingObstructionRoute)
            { ClearTarget(false); State = StructureAttackState.Moving; return false; }
            if (movement.RepathPending) { State = StructureAttackState.PathBlocked; return Target != null; }
            // A revision discarded the old approach. Re-resolve from the current cell even if
            // that defense survived; otherwise attackers would retain a target with no route.
            if (Target != null && !movement.FollowingObstructionRoute)
            { ClearTarget(false); resolvedRevision = -1; }
            if (Target == null)
            {
                State = StructureAttackState.PathBlocked;
                if (resolvedRevision == movement.Grid.Revision) return false;
                resolvedRevision = movement.Grid.Revision;
                if (!movement.Grid.TryWorldToCell(transform.position, out var cell) ||
                    !movement.Navigation.Obstructions.TryResolve(cell, movement.Goals, approach, out var defense, out _, out float tailDistance)) return false;
                Target = defense; Target.Changed += TargetChanged;
                // Spread contact positions inside the last reachable cell; never offset into a solid footprint.
                int slot = contactSlot;
                var offset = new Vector2((slot % 4 - 1.5f) * .12f, (slot / 4 - 1.5f) * .12f) * movement.Grid.CellSize;
                var center = movement.Grid.CellToWorld(approach[approach.Count - 1]);
                var towards = defense.ClosestFootprintPoint(center) - center;
                float vertical = movement.Plane == ShorelineDefense.Grid.GameplayPlane.XY ? towards.y : towards.z;
                if (Mathf.Abs(towards.x) > Mathf.Abs(vertical)) offset.x = Mathf.Sign(towards.x) * (.08f + .06f * (slot % 4)) * movement.Grid.CellSize;
                else offset.y = Mathf.Sign(vertical) * (.08f + .06f * (slot / 4)) * movement.Grid.CellSize;
                movement.FollowObstructionRoute(approach, offset, tailDistance);
                elapsed = 0;
            }
            float range = enemy.Definition.StructureRangeCells * movement.Grid.CellSize;
            // Melee approaches the contact slot before attacking; ranged spit can assist from farther away.
            bool inRange = Vector3.Distance(transform.position, Target.ClosestFootprintPoint(transform.position)) <= range + .001f;
            if (!inRange || spitter == null && !movement.ReachedDestination)
            {
                State = StructureAttackState.Approaching; elapsed = 0;
                movement.Advance(dt);
                return true;
            }
            State = StructureAttackState.Attacking;
            movement.FaceTowards(Target.ClosestFootprintPoint(transform.position));
            if (spitter != null) { spitter.Tick(dt, Target); return true; }
            elapsed += dt;
            float interval = enemy.Definition.StructureAttackInterval;
            while (elapsed >= interval && Target != null && Target.IsAlive)
            {
                elapsed -= interval; AttacksPerformed++; enemy.NotifyAttack();
                Target.TakeDamage(enemy.Definition.StructureDamage);
            }
            return true;
        }
        private void TargetChanged() { if (Target == null || !Target.IsAlive) ClearTarget(true); }
        private void ClearTarget(bool resume)
        {
            if (!ReferenceEquals(Target, null)) Target.Changed -= TargetChanged;
            Target = null; elapsed = 0;
            if (resume && movement != null) { resolvedRevision = -1; movement.ResumeBorderRoute(); }
        }
        private void OnDisable() { ClearTarget(false); State = StructureAttackState.Moving; }
    }
}
