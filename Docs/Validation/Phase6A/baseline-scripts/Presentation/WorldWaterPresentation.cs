using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    [RequireComponent(typeof(VfxService))]
    public sealed class WorldWaterPresentation : MonoBehaviour
    {
        private VfxService effects;
        private float nextRipple;
        private int cursor;
        private SpriteRenderer[] foam;
        private void Awake() => effects = GetComponent<VfxService>();
        private void Start()
        {
            var grid = effects.Grid?.Grid;
            if (grid == null) return;
            // Existing Water -> Beach boundary, at most one narrow strip per row.
            foam = new SpriteRenderer[grid.Height];
            for (int y = 0; y < grid.Height; y++)
            for (int x = 1; x < grid.Width; x++)
            {
                if (grid.GetCell(new Vector2Int(x - 1, y)).Terrain != TerrainType.Water ||
                    grid.GetCell(new Vector2Int(x, y)).Terrain == TerrainType.Water) continue;
                var obj = new GameObject("Shore foam " + y); obj.transform.SetParent(transform, false);
                obj.transform.position = grid.CellToWorld(new Vector2Int(x, y)) - Vector3.right * grid.CellSize * .5f;
                var r = obj.AddComponent<SpriteRenderer>(); r.sharedMaterial = effects.Material;
                r.sortingLayerName = "Environment"; r.sortingOrder = 8;
                r.color = new Color(1, 1, 1, .2f); foam[y] = r; break;
            }
        }
        private void Update()
        {
            if (!effects.CanPlay) { SetFoamVisible(false); return; }
            var sequence = effects.Catalog.Get(VfxId.ShoreFoam);
            if (foam != null && sequence?.frames != null && sequence.frames.Length > 0)
                for (int i = 0; i < foam.Length; i++) if (foam[i] != null)
                { foam[i].enabled = true; foam[i].sprite = sequence.frames[((int)(Time.time * sequence.fps) + i) % sequence.frames.Length]; }
            if (Time.time < nextRipple) return;
            nextRipple = Time.time + .65f;
            var grid = effects.Grid.Grid;
            // Local deterministic visual sequence never consumes UnityEngine.Random's gameplay state.
            int y = (cursor * 7 + 3) % grid.Height;
            int x = (cursor++ % 3);
            var cell = new Vector2Int(x, y);
            if (x < grid.Width && grid.GetCell(cell).Terrain == TerrainType.Water)
                effects.Play(VfxId.WaterRipple, grid.CellToWorld(cell));
        }
        private void SetFoamVisible(bool visible)
        { if (foam != null) for (int i = 0; i < foam.Length; i++) if (foam[i] != null) foam[i].enabled = visible; }
        private void OnDisable() => SetFoamVisible(false);
    }
}
