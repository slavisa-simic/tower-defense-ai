using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    /// <summary>Observes existing health/arming state; never changes the defense lifetime or rules.</summary>
    [RequireComponent(typeof(DefenseBase))]
    public sealed class DefenseSpritePresentation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer baseRenderer;
        [SerializeField] private Sprite idle;
        [SerializeField] private Sprite damaged;
        [SerializeField] private Sprite destroyed;
        [SerializeField] private Sprite armed;
        [SerializeField] private Sprite preview;
        private DefenseBase defense;
        private LandMine mine;
        public Sprite PreviewSprite => preview != null ? preview : idle;
        public void Configure(SpriteRenderer renderer, Sprite intact, Sprite damage, Sprite wreck, Sprite armedMine, Sprite placement)
        { baseRenderer = renderer; idle = intact; damaged = damage; destroyed = wreck; armed = armedMine; preview = placement; }
        private void Awake() { defense = GetComponent<DefenseBase>(); mine = GetComponent<LandMine>(); }
        private void OnEnable() { defense.Died += OnDied; Refresh(); }
        private void OnDisable() { if (defense != null) defense.Died -= OnDied; }
        // Two cached state reads, no allocations or Animator on static defenses.
        private void LateUpdate() => Refresh();
        private void Refresh()
        {
            if (baseRenderer == null || defense.Definition == null) return;
            Sprite next = !defense.IsAlive && destroyed != null ? destroyed :
                mine != null && mine.Armed && armed != null ? armed :
                damaged != null && defense.CurrentHealth <= defense.Definition.MaxHealth / 2 ? damaged : idle;
            if (next != null && baseRenderer.sprite != next) baseRenderer.sprite = next;
        }
        private void OnDied()
        {
            if (baseRenderer != null && destroyed != null) baseRenderer.sprite = destroyed;
        }
    }
}
