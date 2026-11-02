using System;
using System.Text;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;

namespace ShorelineDefense.Progression
{
    /// <summary>Player-facing estimates from the committed plan. Never advances generation or event state.</summary>
    public static class PreparationIntelligence
    {
        public static string Describe(WaveManager waves, MetaModifierService meta)
        {
            if (waves.State != WaveState.Preparation) return "";
            if (!meta.CountIntel && !meta.EventForecast && !meta.DeepWaterIntel) return "";
            var wave = waves.CurrentWave;
            if (wave == null) return "";
            var text = new StringBuilder("RIVER INTELLIGENCE  /  WAVE " + wave.WaveNumber);
            var counts = new int[9];
            foreach (var g in wave.Groups) counts[(int)g.Prefab.Definition.Kind] += g.Count;
            if (meta.CountIntel) text.Append("\nINCOMING: ~").Append(Math.Max(10, (int)Math.Round(wave.ScheduledCount / 10.0) * 10)).Append(" organisms");
            int fast = counts[(int)EnemyKind.Skitter] + counts[(int)EnemyKind.Burrower];
            int armor = counts[(int)EnemyKind.Bulwark];
            if (meta.ThreatIntel)
                text.Append("\n").Append(armor * 5 >= wave.ScheduledCount ? "ARMORED ACTIVITY" : fast * 3 >= wave.ScheduledCount ? "HIGH-SPEED CONTACTS" : "SWARM ACTIVITY");
            if (meta.CompositionIntel)
            {
                for (int i = 0; i < 6; i++) if (counts[i] > 0)
                    text.Append("\n").Append(((EnemyKind)i).ToString()).Append(": ").Append(counts[i] * 100L / wave.ScheduledCount >= 40 ? "High" : counts[i] * 100L / wave.ScheduledCount >= 15 ? "Medium" : "Low");
            }
            if (meta.DeepWaterIntel)
            {
                var active = waves.Events?.Current;
                bool focused = active?.Definition.modifiers.landingPattern == LandingPattern.Concentrated;
                text.Append("\nSECTORS: ").Append(focused ? active.Sector.ToString().ToUpperInvariant() + " concentrated / other sectors possible" : "Northern / Central / Southern approaches");
                bool heavy = counts[(int)EnemyKind.Bulwark] > 0 || wave.Encounter != null;
                text.Append("\nCARRIER PRESSURE: ").Append(heavy ? "Heavy organisms / living carriers likely" : wave.WaveNumber >= 7 ? "Pods / brood carriers likely" : "Direct emergence / drift pods likely");
                text.Append(wave.Encounter != null ? "\nMASSIVE SIGNATURE EXPECTED" : "\nNo massive signature detected");
            }
            if (meta.EventForecast)
            {
                var next = waves.ForecastFollowingEvent();
                text.Append("\nFOLLOWING WAVE ").Append(wave.WaveNumber + 1).Append(": ").Append(next == null ? "No major event detected" : next.displayName.ToUpperInvariant());
            }
            return text.ToString();
        }
    }
}
