using ShorelineDefense.Base;
using ShorelineDefense.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] private BaseHealth playerBase;
        [SerializeField] private GameStateController game;
        private Label healthLabel;
        private Label gameOverLabel;
        private bool subscribed;
        public string HealthText => healthLabel?.text;
        public bool GameOverVisible => gameOverLabel != null && gameOverLabel.style.display.value == DisplayStyle.Flex;

        public void Configure(BaseHealth target, GameStateController state) { playerBase = target; game = state; }

        private void OnEnable()
        {
            if (playerBase == null || game == null) return;
            var root = GetComponent<UIDocument>().rootVisualElement;
            healthLabel = root.Q<Label>("base-hp");
            gameOverLabel = root.Q<Label>("game-over");
            if (healthLabel == null || gameOverLabel == null)
            {
                Debug.LogError("Prototype HUD needs its UXML with base-hp and game-over labels.", this);
                return;
            }
            playerBase.Changed += RefreshHealth;
            game.StateChanged += RefreshState;
            subscribed = true;
            RefreshHealth();
            RefreshState(game.CurrentState);
        }

        private void OnDisable()
        {
            if (!subscribed) return;
            playerBase.Changed -= RefreshHealth;
            game.StateChanged -= RefreshState;
            subscribed = false;
        }

        private void RefreshHealth() => healthLabel.text = $"BASE HP: {playerBase.CurrentHealth} / {playerBase.MaxHealth}";
        private void RefreshState(GameState state) => gameOverLabel.style.display = state == GameState.GameOver ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
