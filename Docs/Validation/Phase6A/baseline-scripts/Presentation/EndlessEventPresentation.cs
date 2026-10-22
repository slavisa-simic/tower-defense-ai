using ShorelineDefense.Grid;
using ShorelineDefense.Waves;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    /// <summary>Two bounded tiled overlays. No terrain, navigation or gameplay ownership.</summary>
    public sealed class EndlessEventPresentation : MonoBehaviour
    {
        [SerializeField] private WaveManager waves;
        [SerializeField] private GridManager battlefield;
        [SerializeField] private Sprite fogSprite, wetSprite;
        private SpriteRenderer fog, wet;
        public bool FogVisible => fog != null && fog.enabled;
        public bool FloodVisible => wet != null && wet.enabled;
        public void Configure(WaveManager manager, GridManager grid, Sprite mist, Sprite bank)
        { waves = manager; battlefield = grid; fogSprite = mist; wetSprite = bank; }
        private void Start()
        {
            if (waves == null || battlefield?.Grid == null) return;
            fog = Make("Event Mist", fogSprite, "Effects", 18, false);
            wet = Make("Flooded No Build Strip", wetSprite, "GroundObjects", 1, true);
            waves.EventChanged += Refresh; Refresh();
        }
        private void OnEnable() { if (fog != null && waves != null) { waves.EventChanged += Refresh; Refresh(); } }
        private SpriteRenderer Make(string label, Sprite sprite, string layer, int order, bool tiled)
        {
            var go = new GameObject(label); go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite;
            renderer.drawMode = tiled ? SpriteDrawMode.Tiled : SpriteDrawMode.Simple; renderer.sortingLayerName = layer; renderer.sortingOrder = order;
            renderer.enabled = false; return renderer;
        }
        private void Refresh()
        {
            if (fog == null) return;
            var grid = battlefield.Grid; var modifiers = waves.Events?.Modifiers;
            fog.enabled = modifiers?.fog ?? false;
            fog.size = new Vector2(grid.Width * grid.CellSize, grid.Height * grid.CellSize);
            fog.transform.localScale = new Vector3(fog.size.x / fogSprite.bounds.size.x, fog.size.y / fogSprite.bounds.size.y, 1);
            fog.transform.position = grid.Origin + new Vector3(fog.size.x / 2, fog.size.y / 2, -.1f);
            fog.color = new Color(.76f, .82f, .81f, .24f);
            wet.enabled = modifiers?.floodedColumns > 0;
            wet.size = new Vector2((modifiers?.floodedColumns ?? 0) * grid.CellSize, grid.Height * grid.CellSize);
            wet.transform.position = grid.Origin + new Vector3((battlefield.Definition.WaterBandCells + battlefield.Definition.BeachBandCells) * grid.CellSize + wet.size.x / 2, wet.size.y / 2, -.02f);
            wet.color = new Color(.22f, .52f, .57f, .64f);
        }
        private void OnDisable() { if (waves != null) waves.EventChanged -= Refresh; if (fog != null) fog.enabled = false; if (wet != null) wet.enabled = false; }
    }
}
