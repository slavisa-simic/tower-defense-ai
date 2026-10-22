using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    /// <summary>At most one small sprite per burning enemy, no per-hit visual objects.</summary>
    [RequireComponent(typeof(EnemyDamageOverTime))]
    public sealed class BurnPresentation : MonoBehaviour
    {
        private EnemyDamageOverTime burn;
        private VfxSequence sequence;
        private SpriteRenderer overlay;
        private void Awake()
        {
            burn = GetComponent<EnemyDamageOverTime>();
        }
        private void CreateOverlay()
        {
            var effects = FindFirstObjectByType<VfxService>();
            sequence = effects?.Catalog.Get(VfxId.BurningOverlay);
            var go = new GameObject("Burn overlay"); go.transform.SetParent(transform, false);
            overlay = go.AddComponent<SpriteRenderer>(); overlay.sharedMaterial = effects?.Material;
            overlay.sortingLayerName = "Effects"; overlay.sortingOrder = 3;
            go.transform.localScale = Vector3.one * .6f;
        }
        private void LateUpdate()
        {
            if (!burn.Burning) { if (overlay != null) overlay.enabled = false; return; }
            if (overlay == null) CreateOverlay();
            overlay.enabled = burn.Burning && sequence?.frames != null && sequence.frames.Length > 0;
            if (overlay.enabled) overlay.sprite = sequence.frames[(int)(Time.time * sequence.fps) % sequence.frames.Length];
        }
        private void OnDisable() { if (overlay != null) overlay.enabled = false; }
    }
}
