using ShorelineDefense.Core;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class AssaultPressureDebugHud : MonoBehaviour
    {
        [SerializeField] private WaveManager waves;
        [SerializeField] private RunStatistics statistics;
        private Label label;
        private float nextRefresh;
        public void Configure(WaveManager manager, RunStatistics counters) { waves = manager; statistics = counters; }
        private void OnEnable()
        {
            label = new Label { name = "assault-pressure-debug", pickingMode = PickingMode.Ignore };
            label.style.position = Position.Absolute; label.style.left = 12; label.style.top = 160;
            label.style.color = Color.white; label.style.backgroundColor = new Color(0, 0, 0, .8f);
            GetComponent<UIDocument>().rootVisualElement.Add(label);
        }
        private void OnDisable() => label?.RemoveFromHierarchy();
        private void Update()
        {
            bool visible = (Application.isEditor || Debug.isDebugBuild) && waves != null && waves.EndlessSettings != null && waves.EndlessSettings.showPressureDebug;
            label.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!visible || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .25f;
            var boats = waves.Landings; var director = waves.PressureDirector;
            label.text = $"Wave {waves.CurrentWaveNumber}  Active {waves.AliveCount}  Pending {waves.PendingCount}\n" +
                $"Boats {boats?.ActiveBoats ?? 0}  Pending boats ~{waves.PendingBoats}  Target {director.Target}\n" +
                $"{director.Mode}  Surge {director.Surging}  Peaks {statistics.PeakActiveEnemies} / {statistics.PeakConcurrentBoats}\n" +
                $"Avg {statistics.AverageFrameTimeMs:F2} ms  Income {statistics.CurrentWaveIncome:N0} RSD";
        }
    }
}
