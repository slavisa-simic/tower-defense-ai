using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class WaveHud : MonoBehaviour
    {
        [SerializeField] private WaveManager waves;
        private Label waveLabel;
        private Label remaining;
        private Label status;
        private Button start;
        private bool subscribed;
        public string RemainingText => remaining?.text;
        public bool StartAvailable => start != null && start.enabledSelf;
        public void Configure(WaveManager manager) => waves = manager;
        private void OnEnable()
        {
            if (waves == null) return;
            var root = GetComponent<UIDocument>().rootVisualElement;
            waveLabel = root.Q<Label>("wave-number");
            remaining = root.Q<Label>("enemies-remaining");
            status = root.Q<Label>("wave-status");
            start = root.Q<Button>("start-wave");
            if (waveLabel == null || remaining == null || status == null || start == null)
            { Debug.LogError("Wave HUD is missing its named UXML elements.", this); return; }
            waves.Changed += Refresh;
            start.clicked += StartWave;
            subscribed = true;
            Refresh();
        }
        private void OnDisable()
        {
            if (!subscribed) return;
            waves.Changed -= Refresh;
            start.clicked -= StartWave;
            subscribed = false;
        }
        private void StartWave() => waves.TryStartWave();
        private void Refresh()
        {
            waveLabel.text = $"WAVE {waves.CurrentWaveNumber} / {waves.Waves.Count}";
            remaining.text = $"Enemies Remaining: {waves.EnemiesRemaining}";
            start.SetEnabled(waves.CanStart);
            start.style.opacity = start.enabledSelf ? 1 : 0.4f;
            switch (waves.State)
            {
                case WaveState.Preparation: status.text = "WAVE READY — prepare defenses"; break;
                case WaveState.Spawning: status.text = "Enemies approaching"; break;
                case WaveState.Combat: status.text = "Clear the remaining enemies"; break;
                case WaveState.WaveComplete: status.text = $"WAVE COMPLETE  +${waves.CurrentWave.CompletionReward}"; break;
                case WaveState.AllWavesComplete: status.text = "ALL WAVES COMPLETE"; break;
                case WaveState.GameOver: status.text = "GAME OVER"; break;
            }
        }
    }
}
