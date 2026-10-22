using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class EndlessEventHud : MonoBehaviour
    {
        [SerializeField] private WaveManager waves;
        private VisualElement panel, icon, wavePanel;
        private Label title, effect, waveStatus;
        public void Configure(WaveManager manager) => waves = manager;
        private void OnEnable()
        {
            if (waves == null) return;
            var root = GetComponent<UIDocument>().rootVisualElement;
            panel = root.Q("event-panel"); icon = root.Q("event-icon"); title = root.Q<Label>("event-title"); effect = root.Q<Label>("event-effect");
            wavePanel = root.Q("wave-panel"); waveStatus = root.Q<Label>("wave-status");
            if (panel == null) return;
            waves.EventChanged += Refresh; waves.Changed += WaveChanged; Refresh();
        }
        private WaveState previousState;
        private void WaveChanged() { if (previousState == waves.State) return; previousState = waves.State; Refresh(); }
        private void OnDisable() { if (waves != null) { waves.EventChanged -= Refresh; waves.Changed -= WaveChanged; } }
        private void Refresh()
        {
            if (panel == null) return;
            var current = waves.Events?.Current;
            panel.style.display = current != null ? DisplayStyle.Flex : DisplayStyle.None;
            wavePanel.EnableInClassList("has-event", current != null);
            waveStatus.style.display = current != null ? DisplayStyle.None : DisplayStyle.Flex;
            if (current == null) return;
            bool preparation = waves.State == WaveState.Preparation;
            title.text = (preparation ? "EVENT: " : "ACTIVE: ") + current.Definition.displayName;
            effect.text = current.Definition.description;
            if (current.Definition.modifiers.landingPattern == LandingPattern.Concentrated) effect.text += " / " + current.Sector.ToString().ToUpperInvariant() + " SECTOR";
            if (current.SupplyReceived > 0) effect.text += " / +" + current.SupplyReceived + " RSD RECEIVED";
            if (current.WarningRemaining > 0) effect.text += " / PREPARATION WARNING";
            icon.style.backgroundImage = current.Definition.uiIcon != null ? new StyleBackground(current.Definition.uiIcon) : StyleKeyword.None;
        }
    }
}
