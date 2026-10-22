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
        public long TotalEarned { get; private set; }
        public long TotalSpent { get; private set; }
        public long RefundIncome { get; private set; }
        public int StructuresDismantled { get; private set; }
        public void RefundConstruction(int amount) {
            if (amount < 0 || (game != null && !game.IsPlaying)) return;
            int credited = (int)Math.Min(amount, (long)int.MaxValue - Money);
            balance = Money + credited;
            RefundIncome = RefundIncome > long.MaxValue - credited ? long.MaxValue : RefundIncome + credited;
            if (StructuresDismantled < int.MaxValue) StructuresDismantled++;
            NotifyMoneyChanged();
        }
        public void Configure(GameStateController state) => game = state;
        public void Restore(int money, long earned, long spent, long refunds, int dismantled)
        { balance = money; TotalEarned = earned; TotalSpent = spent; RefundIncome = refunds; StructuresDismantled = dismantled; NotifyMoneyChanged(); }
        private void Awake() => _ = Money;
        public bool CanAfford(int cost) => cost >= 0 && Money >= cost;

        public bool TrySpend(int cost)
        {
            if ((game != null && !game.IsPlaying) || !CanAfford(cost)) return false;
            if (cost == 0) return true;
            balance = Money - cost;
            TotalSpent = TotalSpent > long.MaxValue - cost ? long.MaxValue : TotalSpent + cost;
            NotifyMoneyChanged();
            return true;
        }

        public void AddMoney(int amount)
        {
            if (amount <= 0 || (game != null && !game.IsPlaying)) return;
            int next = (int)Math.Min(int.MaxValue, (long)Money + amount);
            if (next == Money) return;
            int credited = next - Money;
            TotalEarned = TotalEarned > long.MaxValue - credited ? long.MaxValue : TotalEarned + credited;
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
