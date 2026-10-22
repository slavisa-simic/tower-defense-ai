using UnityEngine;

namespace ShorelineDefense.Enemies
{
    /// <summary>One burn slot per enemy: refresh duration, strongest DPS wins; no source stacking.</summary>
    [RequireComponent(typeof(EnemyController))]
    public sealed class EnemyDamageOverTime : MonoBehaviour
    {
        private EnemyController enemy;
        private float remaining, dps, fractional;
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
            if (!enemy.IsTargetable) { Clear(); return; }
            if (!Burning || dt <= 0) return;
            float active = Mathf.Min(dt, remaining); remaining = Mathf.Max(0, remaining - dt);
            fractional += active * dps;
            int amount = Mathf.FloorToInt(fractional + .00001f);
            fractional -= amount;
            if (amount > 0) enemy.Health.TakeDefenseDamage(amount);
            if (!Burning) Clear();
        }
        private void OnDisable() => Clear();
        public void Clear() { remaining = dps = fractional = 0; }
    }
}
