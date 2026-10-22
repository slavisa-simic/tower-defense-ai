using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    [RequireComponent(typeof(DefenseBase))]
    public sealed class DefenseUpgradePresentation : MonoBehaviour
    {
        private DefenseBase defense;
        private SpriteRenderer overlay;
        private void Awake()
        {
            defense = GetComponent<DefenseBase>();
            var go = new GameObject("Upgrade Reinforcement"); go.transform.SetParent(transform, false);
            overlay = go.AddComponent<SpriteRenderer>(); overlay.sortingLayerName = "Defenses"; overlay.sortingOrder = 1;
        }
        private void OnEnable() { defense.Changed += Refresh; Refresh(); }
        private void OnDisable() { defense.Changed -= Refresh; }
        private void Refresh()
        {
            if (defense.Definition != null)
            {
                overlay.transform.localScale = Vector3.one * Mathf.Max(defense.Definition.Footprint.x, defense.Definition.Footprint.y);
                bool wire = defense.GetComponent<BarbedWire>() != null, mine = defense.GetComponent<LandMine>() != null;
                overlay.sortingLayerName = wire || mine ? "GroundObjects" : "Defenses";
                overlay.sortingOrder = mine ? 3 : 1;
            }
            overlay.sprite = defense.Definition?.LevelData(defense.Level)?.overlay;
            overlay.enabled = defense.IsAlive && overlay.sprite != null;
        }
    }
}
