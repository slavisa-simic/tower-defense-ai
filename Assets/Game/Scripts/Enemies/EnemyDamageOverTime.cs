using UnityEngine;

namespace ShorelineDefense.Enemies
{
    /// <summary>One burn slot per enemy: refresh duration, strongest DPS wins; no source stacking.</summary>
    [RequireComponent(typeof(EnemyController))]
    public sealed class EnemyDamageOverTime : MonoBehaviour
    {
        private EnemyController enemy;
        private float remaining, dps;
        private double fractional;
        public bool Burning => remaining > 0;
        public float Remaining => remaining;
        public float Dps => dps;
        private void Awake() => enemy = GetComponent<EnemyController>();
        public void ApplyBurn(float duration, float damagePerSecond)
        {
            if (!enemy.IsTargetable || duration <= 0 || damagePerSecond <= 0) return;
            remaining = Mathf.Max(remaining, duration); dps = Mathf.Max(dps, damagePerSecond);
        }
        private void Update() => Tick(Time.deltaTime);
        public void Tick(float dt)
        {
            // A burn committed on the surface continues during the finite burrow state.
            if (!enemy.IsCombatActive) { Clear(); return; }
            if (!Burning || dt <= 0) return;
            float active = Mathf.Min(dt, remaining); remaining = Mathf.Max(0, remaining - dt);
            // Mitigate before integer accumulation. Rounding every tiny frame's
            // damage up to one otherwise lets high-frame-rate burns bypass armor.
            fractional += active * dps * enemy.Health.ArmorDamageMultiplier;
            int amount = (int)System.Math.Floor(fractional + .00001);
            fractional -= amount;
            if (amount > 0) enemy.Health.TakeDefenseDamage(amount, 1); // Armor already applied above.
            if (!Burning) Clear();
        }
        private void OnDisable() => Clear();
        public void Clear() { remaining = dps = 0; fractional = 0; }
    }
}
