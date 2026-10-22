using UnityEngine;

namespace ShorelineDefense.Waves
{
    public enum AssaultPressure { Normal, Accelerated, Throttled }

    /// <summary>Pure pacing policy; WaveManager alone owns deployment and pending counts.</summary>
    public sealed class AssaultPressureDirector
    {
        private float untilEvaluation, elapsed;
        public int Target { get; private set; }
        public AssaultPressure Mode { get; private set; }
        public bool Surging { get; private set; }
        public float ArrivalInterval { get; private set; }
        public int BoatLimit { get; private set; }
        public bool DeploymentPaused { get; private set; }
        public void Reset() { untilEvaluation = elapsed = 0; Mode = AssaultPressure.Normal; Surging = DeploymentPaused = false; }
        public bool Tick(float dt, EndlessWaveSettings settings, int wave, int activeEnemies,
            int pendingEnemies, int activeBoats, int incomingPassengers, bool bossPendingOrAlive)
        {
            elapsed += dt; untilEvaluation -= dt;
            if (untilEvaluation > 0) return false;
            untilEvaluation = Mathf.Clamp(settings.directorEvaluationInterval, .1f, .25f);
            Target = settings.PressureTarget(wave);
            float band = Mathf.Clamp(settings.pressureHysteresis, .05f, .4f);
            // Cargo committed to the river is pressure in flight: do not overfill a full battlefield.
            int committed = activeEnemies + incomingPassengers;
            DeploymentPaused = committed >= Mathf.Min(settings.maximumActiveEnemies, Mathf.CeilToInt(Target * (1 + band)));
            if (pendingEnemies <= incomingPassengers) Mode = AssaultPressure.Normal;
            else if (committed < Target * (1 - band)) Mode = AssaultPressure.Accelerated;
            else if (activeEnemies > Target * (1 + band)) Mode = AssaultPressure.Throttled;
            else if (Mode == AssaultPressure.Accelerated && committed >= Target ||
                     Mode == AssaultPressure.Throttled && activeEnemies <= Target) Mode = AssaultPressure.Normal;
            float period = Mathf.Max(1, settings.surgePeriod);
            Surging = settings.assaultSurgesEnabled && wave >= 5 && pendingEnemies > incomingPassengers &&
                elapsed >= period && elapsed % period < Mathf.Min(period * .5f, Mathf.Max(1, settings.surgeDuration));
            BoatLimit = Mathf.Clamp(settings.BoatConcurrency(wave) + (Surging ? settings.surgeExtraBoats : 0), 1, 32);
            ArrivalInterval = settings.ArrivalInterval(wave);
            if (bossPendingOrAlive) ArrivalInterval *= .8f;
            if ((Mode == AssaultPressure.Accelerated || Surging) && activeBoats < BoatLimit)
                ArrivalInterval = Mathf.Min(ArrivalInterval, Mathf.Max(.05f, settings.acceleratedArrivalInterval));
            else if (Mode == AssaultPressure.Throttled) ArrivalInterval *= 1.5f;
            return true;
        }
    }
}
