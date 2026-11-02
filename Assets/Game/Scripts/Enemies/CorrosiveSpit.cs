using System.Collections.Generic;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Enemies
{
    public sealed class CorrosiveSpit : MonoBehaviour
    {
        private Vector3 start, impact;
        private BattlefieldGrid grid;
        private GameStateController game;
        private float elapsed, duration;
        private int damage;
        private SpriteRenderer visual;
        private bool resolved;
        private ShorelineDefense.Presentation.VfxService effects;
        public static void Launch(Vector3 from, Vector3 to, BattlefieldGrid battlefield, GameStateController state, Sprite sprite, int amount, float seconds, ShorelineDefense.Presentation.VfxService vfx = null)
        {
            var shot = new GameObject("Corrosive biological glob").AddComponent<CorrosiveSpit>();
            shot.start = from; shot.impact = to; shot.grid = battlefield; shot.game = state;
            shot.damage = amount; shot.duration = Mathf.Max(.1f, seconds); shot.transform.position = from;
            shot.effects = vfx;
            shot.visual = shot.gameObject.AddComponent<SpriteRenderer>(); shot.visual.sprite = sprite;
            shot.visual.sortingLayerName = "Effects"; shot.visual.sortingOrder = 10;
            shot.transform.localScale = Vector3.one * .4f;
        }
        private void Update() => Tick(Time.deltaTime);
        public void Tick(float dt)
        {
            if (resolved || dt <= 0) return;
            if (game == null || !game.IsPlaying) { resolved = true; Destroy(gameObject); return; }
            elapsed += dt;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, impact, t) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * .35f);
            if (t < 1) return;
            resolved = true;
            effects?.Play(ShorelineDefense.Presentation.VfxId.SlimeImpact, impact, 0, .7f);
            // At most nine cells, deduplicated for 2x2 defenses. Locked impact permits evasion/dismantle.
            var hit = new HashSet<DefenseBase>();
            if (grid.TryWorldToCell(impact, out var center))
                for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++)
                {
                    var cell = center + new Vector2Int(x, y);
                    if (!grid.Contains(cell)) continue;
                    var defense = grid.OccupantAt(cell) as DefenseBase;
                    if (defense != null && defense.IsAlive && defense.gameObject.activeInHierarchy &&
                        ((Vector2)(grid.CellToWorld(cell) - impact)).sqrMagnitude <= grid.CellSize * grid.CellSize * .8f && hit.Add(defense)) defense.TakeDamage(damage);
                }
            // A short, bounded splash reuses this renderer; gameplay has already resolved.
            transform.position = impact; transform.localScale = Vector3.one * .85f;
            visual.color = new Color(.5f, .75f, .32f, .65f); Destroy(gameObject, .18f);
        }
    }
}
