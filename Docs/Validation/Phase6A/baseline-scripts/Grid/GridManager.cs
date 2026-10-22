using UnityEngine;

namespace ShorelineDefense.Grid
{
    [DefaultExecutionOrder(-200)]
    public sealed class GridManager : MonoBehaviour
    {
        [SerializeField] private BattlefieldDefinition definition;
        [SerializeField] private bool showCells = true;
        public BattlefieldDefinition Definition => definition;
        public BattlefieldGrid Grid { get; private set; }
        public PathfindingService Paths { get; private set; }

        public void Configure(BattlefieldDefinition data) => definition = data;

        private void Awake()
        {
            if (definition == null)
            {
                Debug.LogError("GridManager needs a BattlefieldDefinition.", this);
                enabled = false;
                return;
            }
            Grid = definition.CreateGrid(transform.position);
            Paths = new PathfindingService(Grid);
        }

        private void OnDrawGizmosSelected()
        {
            if (definition == null || !showCells) return;
            bool xy = definition.Plane == GameplayPlane.XY;
            var size = xy ? new Vector3(definition.CellSize, definition.CellSize, 0.03f)
                : new Vector3(definition.CellSize, 0.03f, definition.CellSize);
            for (int y = 0; y < definition.Height; y++)
            for (int x = 0; x < definition.Width; x++)
            {
                var cell = new Vector2Int(x, y);
                bool blocked = Grid != null && !Grid.GetCell(cell).Walkable;
                bool occupied = Grid != null && Grid.OccupantAt(cell) != null;
                Gizmos.color = occupied ? Color.cyan : blocked ? Color.red : cell == definition.BaseCell ? Color.yellow : new Color(1, 1, 1, 0.35f);
                var center = transform.position + (xy
                    ? new Vector3((x + 0.5f) * size.x, (y + 0.5f) * size.y, -0.04f)
                    : new Vector3((x + 0.5f) * size.x, 0.04f, (y + 0.5f) * size.z));
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
