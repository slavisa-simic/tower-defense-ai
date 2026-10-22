using System;
using ShorelineDefense.Combat;
using UnityEngine;

namespace ShorelineDefense.Base
{
    [DefaultExecutionOrder(-150)]
    public sealed class BaseHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maxHealth = 100;
        private Health health;
        public int CurrentHealth => State.CurrentHealth;
        public int MaxHealth => State.MaxHealth;
        public bool IsAlive => State.IsAlive;
        public event Action Changed { add => State.Changed += value; remove { if (health != null) health.Changed -= value; } }
        public event Action Died { add => State.Died += value; remove { if (health != null) health.Died -= value; } }
        private Health State => health ?? (health = new Health(maxHealth));
        private void Awake() => _ = State;
        public void TakeDamage(int amount) => State.TakeDamage(amount);
    }
}
