using System;
using System.Collections.Generic;

namespace ShorelineDefense.Enemies
{
    /// <summary>Runtime, source-owned modifiers. Removing one source never removes another's effect.</summary>
    public sealed class MovementModifiers
    {
        private readonly Dictionary<object, float> factors = new Dictionary<object, float>();
        public float Multiplier { get; private set; } = 1f;
        public void Set(object source, float multiplier)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (float.IsNaN(multiplier) || float.IsInfinity(multiplier) || multiplier <= 0 || multiplier > 1)
                throw new ArgumentOutOfRangeException(nameof(multiplier));
            factors[source] = multiplier;
            Refresh();
        }
        public void Remove(object source) { if (source != null && factors.Remove(source)) Refresh(); }
        public void Clear() { factors.Clear(); Multiplier = 1f; }
        private void Refresh()
        {
            Multiplier = 1;
            foreach (var factor in factors.Values) Multiplier *= factor;
            Multiplier = Math.Max(0.05f, Multiplier);
        }
    }
}
