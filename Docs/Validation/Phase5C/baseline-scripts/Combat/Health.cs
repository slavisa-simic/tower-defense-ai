using System;

namespace ShorelineDefense.Combat
{
    /// <summary>Per-instance mutable health. Configuration assets never own this state.</summary>
    public sealed class Health : IDamageable
    {
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public void IncreaseMaximum(int maximum)
        {
            if (!IsAlive || maximum <= MaxHealth) return;
            // Preserve missing HP; upgrading adds capacity without replacing paid repairs.
            CurrentHealth += maximum - MaxHealth;
            MaxHealth = maximum;
            Changed?.Invoke();
        }
        public void RestoreFull()
        {
            if (!IsAlive || CurrentHealth == MaxHealth) return;
            CurrentHealth = MaxHealth;
            Changed?.Invoke();
        }
        public bool IsAlive => CurrentHealth > 0;
        public event Action Changed;
        public event Action Died;

        public Health(int maximum)
        {
            if (maximum <= 0) throw new ArgumentOutOfRangeException(nameof(maximum));
            CurrentHealth = MaxHealth = maximum;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || !IsAlive) return;
            CurrentHealth = Math.Max(0, CurrentHealth - amount);
            bool died = !IsAlive;
            Changed?.Invoke();
            if (died) Died?.Invoke();
        }
    }
}
