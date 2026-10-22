using System;
using ShorelineDefense.Combat;
using UnityEngine;

namespace ShorelineDefense.Enemies
{
    public sealed class EnemyHealth : MonoBehaviour, IDamageable
    {
        private Health health;
        public bool IsAlive => health != null && health.IsAlive;
        public int CurrentHealth => health?.CurrentHealth ?? 0;
        public int MaxHealth => health?.MaxHealth ?? 0;
        public event Action Died;
        public bool KilledByDefense { get; private set; }

        public void Initialize(int maximum)
        {
            if (health != null) health.Died -= OnDied;
            health = new Health(maximum);
            KilledByDefense = false;
            health.Died += OnDied;
        }

        public void TakeDamage(int amount) => health?.TakeDamage(amount);
        // Attribution belongs to the lethal damage operation, never to a specific turret type.
        public void TakeDefenseDamage(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            if (amount >= CurrentHealth) KilledByDefense = true;
            TakeDamage(amount);
        }
        private void OnDied() => Died?.Invoke();
    }
}
