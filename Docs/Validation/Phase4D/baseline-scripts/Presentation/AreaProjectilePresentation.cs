using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    [RequireComponent(typeof(TimedAreaProjectile))]
    public sealed class AreaProjectilePresentation : MonoBehaviour
    {
        private TimedAreaProjectile projectile;
        private VfxService effects;
        private bool rocket;
        private float smoke;
        public void Configure(VfxService service, bool isRocket, float angle)
        {
            effects = service; rocket = isRocket; projectile = GetComponent<TimedAreaProjectile>();
            projectile.Impacted += OnImpact;
            var sequence = service.Catalog.Get(rocket ? VfxId.RocketProjectile : VfxId.MortarShell);
            if (sequence?.frames == null || sequence.frames.Length == 0) return;
            var go = new GameObject("Projectile visual"); go.transform.SetParent(transform, false);
            go.transform.rotation = Quaternion.Euler(0, 0, angle);
            go.transform.localScale = Vector3.one * (rocket ? .28f : .18f);
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sequence.frames[0];
            renderer.sharedMaterial = service.Material; renderer.sortingLayerName = "Effects"; renderer.sortingOrder = 12;
        }
        private void LateUpdate()
        {
            if (!rocket || projectile == null || projectile.Resolved) return;
            smoke -= Time.deltaTime;
            if (smoke <= 0) { smoke = .12f; effects.Play(VfxId.RocketSmoke, transform.position, 0, .45f); }
        }
        private void OnImpact(Vector3 point, float radius)
        {
            var id = rocket ? VfxId.RocketExplosion : VfxId.MortarExplosion;
            var seq = effects.Catalog.Get(id);
            float scale = seq?.frames != null && seq.frames.Length > 0 ? radius * 2 / seq.frames[0].bounds.size.x : 1;
            effects.Play(id, point, 0, scale);
        }
        private void OnDestroy() { if (projectile != null) projectile.Impacted -= OnImpact; }
    }
}
