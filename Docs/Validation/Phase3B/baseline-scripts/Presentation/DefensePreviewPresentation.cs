using ShorelineDefense.Building;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    /// <summary>Replaces the existing tinted ghost texture after placement updates its state.</summary>
    public sealed class DefensePreviewPresentation : MonoBehaviour
    {
        [SerializeField] private PlacementController placement;
        [SerializeField] private SpriteRenderer preview;
        private Sprite fallback;
        public void Configure(PlacementController controller, SpriteRenderer renderer)
        { placement = controller; preview = renderer; }
        private void Awake() { if (preview != null) fallback = preview.sprite; }
        private void OnEnable()
        {
            if (placement != null) { placement.Changed += Refresh; Refresh(); }
        }
        private void OnDisable() { if (placement != null) placement.Changed -= Refresh; }
        private void Refresh()
        {
            if (preview == null || placement == null) return;
            var definition = placement.Selected;
            var art = definition != null && definition.Prefab != null
                ? definition.Prefab.GetComponent<DefenseSpritePresentation>() : null;
            if (art != null && art.PreviewSprite != null)
            {
                preview.sprite = art.PreviewSprite;
                preview.transform.localScale = Vector3.one;
            }
            else preview.sprite = fallback;
        }
    }
}
