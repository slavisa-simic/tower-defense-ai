using ShorelineDefense.Progression;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.UIElements;
namespace ShorelineDefense.UI
{
    [DefaultExecutionOrder(1200), RequireComponent(typeof(UIDocument))]
    public sealed class PreparationIntelHud : MonoBehaviour
    {
        private WaveManager waves;
        private Label label;
        private WaveState lastState;
        private int lastWave;
        private void Start()
        {
            waves = FindFirstObjectByType<WaveManager>();
            label = new Label { name = "preparation-intelligence", pickingMode = PickingMode.Ignore };
            label.AddToClassList("preparation-intelligence");
            GetComponent<UIDocument>().rootVisualElement.Add(label);
            waves.Changed += WaveChanged; waves.EventChanged += Refresh; Refresh();
        }
        private void WaveChanged() { if (lastState != waves.State || lastWave != waves.PreparationWaveNumber) Refresh(); }
        private void Refresh()
        {
            lastState = waves.State; lastWave = waves.PreparationWaveNumber;
            label.text = PreparationIntelligence.Describe(waves, waves.Meta);
            label.style.display = label.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
        private void OnDestroy() { if (waves != null) { waves.Changed -= WaveChanged; waves.EventChanged -= Refresh; } }
    }
}
