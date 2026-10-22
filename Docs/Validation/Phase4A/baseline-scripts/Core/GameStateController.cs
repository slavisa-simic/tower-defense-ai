using System;
using ShorelineDefense.Base;
using UnityEngine;

namespace ShorelineDefense.Core
{
    public enum GameState { Initializing, Playing, GameOver }

    [DefaultExecutionOrder(-100)]
    public sealed class GameStateController : MonoBehaviour
    {
        [SerializeField] private BaseHealth playerBase;
        public GameState CurrentState { get; private set; } = GameState.Initializing;
        public bool IsPlaying => CurrentState == GameState.Playing;
        public event Action<GameState> StateChanged;

        public void Configure(BaseHealth target)
        {
            if (playerBase != null) playerBase.Died -= OnBaseDestroyed;
            playerBase = target;
            if (isActiveAndEnabled && playerBase != null) playerBase.Died += OnBaseDestroyed;
        }

        private void OnEnable()
        {
            if (playerBase == null) return;
            playerBase.Died += OnBaseDestroyed;
            if (!playerBase.IsAlive) OnBaseDestroyed();
        }

        private void Start()
        {
            if (playerBase == null)
            {
                Debug.LogError("GameStateController needs a player base.", this);
                enabled = false;
                return;
            }
            SetState(playerBase.IsAlive ? GameState.Playing : GameState.GameOver);
        }

        private void OnDisable()
        {
            if (playerBase != null) playerBase.Died -= OnBaseDestroyed;
        }

        private void OnBaseDestroyed() => SetState(GameState.GameOver);

        private void SetState(GameState state)
        {
            if (state == CurrentState) return;
            CurrentState = state;
            StateChanged?.Invoke(state);
        }
    }
}
