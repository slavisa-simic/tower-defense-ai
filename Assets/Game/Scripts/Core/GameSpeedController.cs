using System;
using UnityEngine;

namespace ShorelineDefense.Core
{
    public enum GameSpeed { Pause, Normal, Double }
    public sealed class GameSpeedController : MonoBehaviour
    {
        [SerializeField] private GameStateController game;
        public GameSpeed Speed { get; private set; } = GameSpeed.Normal;
        public GameSpeed LastRunningSpeed { get; private set; } = GameSpeed.Normal;
        public bool Resume() => SetSpeed(LastRunningSpeed);
        public event Action Changed;
        public void Configure(GameStateController state) => game = state;
        private void OnEnable() { game.StateChanged += StateChanged; }
        private void OnDisable() { game.StateChanged -= StateChanged; Time.timeScale = 1; }
        public bool SetSpeed(GameSpeed value)
        {
            if (game == null || !game.IsPlaying || !Enum.IsDefined(typeof(GameSpeed), value)) return false;
            Speed = value;
            if (value != GameSpeed.Pause) LastRunningSpeed = value;
            Time.timeScale = value == GameSpeed.Pause ? 0 : value == GameSpeed.Double ? 2 : 1;
            Changed?.Invoke();
            return true;
        }
        private void StateChanged(GameState state)
        {
            if (state != GameState.GameOver) return;
            // Existing Game Over UI/effects may complete normally; simulation checks IsPlaying.
            Speed = GameSpeed.Normal; Time.timeScale = 1; Changed?.Invoke();
        }
    }
}
