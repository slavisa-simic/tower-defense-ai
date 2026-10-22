using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Economy
{
    public sealed class EnemyRewardService : MonoBehaviour
    {
        [SerializeField] private EnemyRegistry enemies;
        [SerializeField] private EconomyManager economy;
        [SerializeField] private GameStateController game;
        public void Configure(EnemyRegistry registry, EconomyManager money, GameStateController state)
        { enemies = registry; economy = money; game = state; }
        private void OnEnable() { if (enemies != null) enemies.EnemyKilled += OnKilled; }
        private void OnDisable() { if (enemies != null) enemies.EnemyKilled -= OnKilled; }
        private void OnKilled(EnemyController enemy)
        {
            if (game != null && game.IsPlaying && enemy.Health.KilledByDefense)
                economy.AddMoney(enemy.Definition.KillReward);
        }
    }
}
