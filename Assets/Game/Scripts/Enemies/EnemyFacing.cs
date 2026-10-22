using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Enemies
{
    /// <summary>Presentation only. Sprites face local +X; legacy meshes face local +Z.</summary>
    [RequireComponent(typeof(EnemyMovement))]
    public sealed class EnemyFacing : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        private EnemyMovement movement;

        public void Configure(Transform representation) => visual = representation;
        private void Awake() => movement = GetComponent<EnemyMovement>();

        private void LateUpdate()
        {
            if (visual == null || movement.FacingDirection.sqrMagnitude < 0.001f) return;
            Vector3 direction = movement.FacingDirection;
            visual.rotation = movement.Plane == GameplayPlane.XY
                ? Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg)
                : Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
