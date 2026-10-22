using UnityEngine;
using UnityEngine.Serialization;

namespace ShorelineDefense.Grid
{
    [CreateAssetMenu(menuName = "Shoreline Defense/Battlefield Definition")]
    public sealed class BattlefieldDefinition : ScriptableObject
    {
        [SerializeField, Min(5)] private int width = 32;
        [SerializeField, Min(8)] private int height = 18;
        [SerializeField, Min(0.5f)] private float cellSize = 1.25f;
        [Tooltip("XY is the primary top-down convention. LegacyXZ is only for the Phase 1 reference scene.")]
        [SerializeField] private GameplayPlane plane = GameplayPlane.XY;
        [FormerlySerializedAs("waterRows"), SerializeField, Min(1)] private int waterBandCells = 4;
        [FormerlySerializedAs("beachRows"), SerializeField, Min(1)] private int beachBandCells = 2;
        [SerializeField] private Vector2Int baseCell = new Vector2Int(29, 9);
        [Tooltip("Centered reserved base area; remains walkable, never buildable.")]
        [SerializeField] private Vector2Int baseFootprint = Vector2Int.one;

        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public GameplayPlane Plane => plane;
        public int WaterBandCells => waterBandCells;
        public int BeachBandCells => beachBandCells;
        public int GroundBandCells => (plane == GameplayPlane.XY ? width : height) - waterBandCells - beachBandCells;
        public Vector2Int BaseCell => baseCell;

        public BattlefieldGrid CreateGrid(Vector3 origin)
        {
            var grid = new BattlefieldGrid(width, height, cellSize, origin, plane);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var type = TerrainAt(new Vector2Int(x, y));
                grid.SetCell(new Vector2Int(x, y), new GridCell(type, true, type == TerrainType.Ground));
            }
            return grid;
        }

        public TerrainType TerrainAt(Vector2Int cell)
        {
            Vector2Int baseMin = baseCell - new Vector2Int(baseFootprint.x / 2, baseFootprint.y / 2);
            if (cell.x >= baseMin.x && cell.y >= baseMin.y &&
                cell.x < baseMin.x + baseFootprint.x && cell.y < baseMin.y + baseFootprint.y) return TerrainType.Base;
            int inland = plane == GameplayPlane.XY ? cell.x : height - 1 - cell.y;
            return inland < waterBandCells ? TerrainType.Water :
                inland < waterBandCells + beachBandCells ? TerrainType.Beach : TerrainType.Ground;
        }

        private void OnValidate()
        {
            width = Mathf.Max(5, width);
            height = Mathf.Max(8, height);
            cellSize = Mathf.Max(0.5f, cellSize);
            baseFootprint = new Vector2Int(Mathf.Max(1, baseFootprint.x), Mathf.Max(1, baseFootprint.y));
            int bandAxis = plane == GameplayPlane.XY ? width : height;
            waterBandCells = Mathf.Clamp(waterBandCells, 1, bandAxis - 4);
            beachBandCells = Mathf.Clamp(beachBandCells, 1, bandAxis - waterBandCells - 3);
            baseCell = plane == GameplayPlane.XY
                ? new Vector2Int(Mathf.Clamp(baseCell.x, waterBandCells + beachBandCells, width - 1), Mathf.Clamp(baseCell.y, 0, height - 1))
                : new Vector2Int(Mathf.Clamp(baseCell.x, 0, width - 1), Mathf.Clamp(baseCell.y, 0, GroundBandCells - 1));
        }
    }
}
