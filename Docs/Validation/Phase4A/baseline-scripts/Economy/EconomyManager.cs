using System;
using ShorelineDefense.Core;
using UnityEngine;

namespace ShorelineDefense.Economy
{
    [DefaultExecutionOrder(-90)]
    public sealed class EconomyManager : MonoBehaviour
    {
        [SerializeField, Min(0)] private int startingMoney = 500;
        [SerializeField] private GameStateController game;
        private int? balance;
        public int Money => balance ?? (balance = Mathf.Max(0, startingMoney)).Value;
        public event Action<int> MoneyChanged;
        public void Configure(GameStateController state) => game = state;
        private void Awake() => _ = Money;
        public bool CanAfford(int cost) => cost >= 0 && Money >= cost;

        public bool TrySpend(int cost)
        {
            if ((game != null && !game.IsPlaying) || !CanAfford(cost)) return false;
            if (cost == 0) return true;
            balance = Money - cost;
            NotifyMoneyChanged();
            return true;
        }

        public void AddMoney(int amount)
        {
            if (amount <= 0 || (game != null && !game.IsPlaying)) return;
            int next = (int)Math.Min(int.MaxValue, (long)Money + amount);
            if (next == Money) return;
            balance = next;
            NotifyMoneyChanged();
        }

        private void NotifyMoneyChanged()
        {
            // Observation failures must not turn a committed payment into a failed build command.
            if (MoneyChanged == null) return;
            foreach (Action<int> observer in MoneyChanged.GetInvocationList())
            {
                try { observer(Money); }
                catch (Exception exception) { Debug.LogException(exception, this); }
            }
        }
    }
}
