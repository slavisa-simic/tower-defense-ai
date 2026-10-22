using System;
using ShorelineDefense.Combat;
using UnityEngine;

namespace ShorelineDefense.Enemies
{
    public sealed class EnemyHealth : MonoBehaviour, IDamageable
    {
        private Health health;
        private float armor;
        public void ConfigureArmor(float fraction) => armor = Mathf.Clamp(fraction, 0, .9f);
        private int Mitigate(int amount) => amount <= 0 ? 0 : (int)Math.Min(int.MaxValue, Math.Max(1, Math.Ceiling(amount * (1.0 - Math.Round(armor, 6)))));
        public bool IsAlive => health != null && health.IsAlive;
        public int CurrentHealth => health?.CurrentHealth ?? 0;
        public int MaxHealth => health?.MaxHealth ?? 0;
        public event Action Died;
        public event Action<int> Damaged;
        public bool KilledByDefense { get; private set; }

        public void Initialize(int maximum)
        {
            if (health != null) health.Died -= OnDied;
            health = new Health(maximum);
            KilledByDefense = false;
            health.Died += OnDied;
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            amount = Mitigate(amount);
            int applied = Math.Min(amount, CurrentHealth);
            health.TakeDamage(amount);
            Damaged?.Invoke(applied);
        }
        // Attribution belongs to the lethal damage operation, never to a specific turret type.
        public void TakeDefenseDamage(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            if (Mitigate(amount) >= CurrentHealth) KilledByDefense = true;
            TakeDamage(amount);
        }
        private void OnDied() => Died?.Invoke();
    }
}
