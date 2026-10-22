using System;
using System.Collections.Generic;
using ShorelineDefense.Economy;
using UnityEngine;

namespace ShorelineDefense.Waves
{
    public sealed class ActiveEventContext
    {
        public EndlessEventDefinition Definition { get; internal set; }
        public int Wave { get; internal set; }
        public LandingSector Sector { get; internal set; }
        public bool Active { get; internal set; }
        public float SecondsRemaining { get; internal set; }
        public float WarningRemaining { get; internal set; }
        public int WavesRemaining { get; internal set; }
        public int SupplyReceived { get; internal set; }
    }

    public readonly struct EventHistoryEntry
    {
        public readonly int Wave;
        public readonly string Id;
        public EventHistoryEntry(int wave, string id) { Wave = wave; Id = id; }
        public override string ToString() => "Wave " + Wave + " — " + Id;
    }

    /// <summary>Run-owned event policy/lifecycle. Never writes permanent tuning or owns wave scheduling.</summary>
    public sealed class EndlessEventManager
    {
        private static readonly EventModifiers normal = new EventModifiers();
        private readonly EndlessEventSettings settings;
        private readonly int seed;
        private readonly Dictionary<string, int> lastWave = new Dictionary<string, int>();
        private readonly List<string> recent = new List<string>();
        private readonly List<EventHistoryEntry> history = new List<EventHistoryEntry>();
        private readonly Dictionary<string, int> counts = new Dictionary<string, int>();
        private bool preparation;
        private int nextEventWave;
        public ActiveEventContext Current { get; private set; }
        public EventModifiers Modifiers => Current != null && Current.Active ? Current.Definition.modifiers : normal;
        public IReadOnlyList<EventHistoryEntry> History => history;
        public long EventsExperienced { get; private set; }
        public string MostRecentEvent { get; private set; }
        public int Count(string id) => counts.TryGetValue(id, out int value) ? value : 0;
        public bool CanStart => Current == null || Current.WarningRemaining <= 0;
        public event Action Changed;
        public EndlessEventManager(EndlessEventSettings tuning, int runSeed)
        { settings = tuning; seed = runSeed; nextEventWave = tuning != null ? Mathf.Max(5, tuning.firstEventWave) : int.MaxValue; }

        public EndlessEventDefinition Select(int wave, bool boss, EndlessEventDefinition existing = null)
        {
            if (settings == null || wave < nextEventWave || existing != null) return null; // one major event
            var eligible = new List<EndlessEventDefinition>();
            var totals = new float[3];
            foreach (var e in settings.events)
            {
                if (e == null || !e.Eligible(wave, boss) || !e.Compatible(existing) || recent.Contains(e.id) ||
                    lastWave.TryGetValue(e.id, out int last) && (long)wave - last < Mathf.Max(1, e.cooldown)) continue;
                eligible.Add(e); totals[(int)e.category] += e.weight;
            }
            float[] categories = { settings.challengeWeight, settings.reliefWeight, settings.patternWeight };
            float total = 0;
            for (int i = 0; i < 3; i++) if (totals[i] > 0) total += Mathf.Max(0, categories[i]);
            if (total <= 0) return null;
            var random = RandomFor(wave);
            double roll = random.NextDouble() * total; int category = 0;
            for (int i = 0; i < 3; i++) if (totals[i] > 0) { category = i; roll -= Mathf.Max(0, categories[i]); if (roll < 0) break; }
            roll = random.NextDouble() * totals[category];
            foreach (var e in eligible) if ((int)e.category == category) { roll -= e.weight; if (roll < 0) return e; }
            return null;
        }

        public void Prepare(int wave, bool boss, EconomyManager economy)
        {
            preparation = true;
            if (Current != null && Current.WavesRemaining > 0 && (!boss || Current.Definition.allowBossWave))
            { Current.Wave = wave; Current.Active = Current.Definition.preparationEffects; Changed?.Invoke(); return; }
            Cancel();
            Queue(Select(wave, boss), wave, economy);
        }
        private System.Random RandomFor(int wave) => new System.Random(unchecked(seed * 7919 ^ wave * 104729 ^ 0x5b));
        private void Queue(EndlessEventDefinition definition, int wave, EconomyManager economy)
        {
            if (definition == null) return;
            var random = RandomFor(wave);
            Current = new ActiveEventContext { Definition = definition, Wave = wave, Sector = (LandingSector)random.Next(3),
                Active = definition.preparationEffects || definition.durationType == EventDuration.Preparation,
                WavesRemaining = definition.durationType == EventDuration.MultiWave ? Mathf.Max(1, Mathf.CeilToInt(definition.duration)) : 1,
                SecondsRemaining = definition.duration, WarningRemaining = definition.preparationWarningSeconds };
            lastWave[definition.id] = wave; recent.Add(definition.id);
            while (recent.Count > Mathf.Max(0, settings != null ? settings.recentExclusion : 0)) recent.RemoveAt(0);
            MostRecentEvent = definition.id; EventsExperienced++; counts[definition.id] = Count(definition.id) + 1;
            history.Add(new EventHistoryEntry(wave, definition.id)); if (history.Count > 128) history.RemoveAt(0);
            bool late = settings != null && wave >= settings.laterFrequencyWave;
            int min = Mathf.Max(2, settings == null ? 2 : late ? settings.laterMinimumGap : settings.minimumGap);
            int max = Mathf.Max(min, settings == null ? 4 : late ? settings.laterMaximumGap : settings.maximumGap);
            nextEventWave = (int)Math.Min(int.MaxValue, (long)wave + random.Next(min, max + 1));
            if (definition.supplyBase > 0 || definition.supplyPerWave > 0)
            {
                int grant = (int)Math.Min(Mathf.Max(0, definition.supplyMaximum), Math.Max(0, (long)definition.supplyBase + (long)wave * definition.supplyPerWave));
                var context = Current;
                int before = economy != null ? economy.Money : 0; economy?.AddMoney(grant);
                context.SupplyReceived = economy != null ? economy.Money - before : 0;
                if (Current != context) return; // A money observer may end the run and cancel the event.
            }
            Changed?.Invoke();
        }
        public void BeginWave()
        {
            preparation = false;
            if (Current == null) return;
            if (Current.Definition.durationType == EventDuration.Preparation) { Cancel(); return; }
            Current.Active = true; Changed?.Invoke();
        }
        public void CompleteWave()
        {
            if (Current == null) return;
            Current.Active = false; Current.WavesRemaining--;
            if (Current.WavesRemaining <= 0) Current = null;
            Changed?.Invoke();
        }
        public void Tick(float dt)
        {
            if (dt <= 0 || Current == null) return;
            if (preparation && Current.WarningRemaining > 0)
            { Current.WarningRemaining = Mathf.Max(0, Current.WarningRemaining - dt); if (Current.WarningRemaining == 0) Changed?.Invoke(); }
            if (!preparation && Current.Active && Current.Definition.durationType == EventDuration.Timed)
            { Current.SecondsRemaining -= dt; if (Current.SecondsRemaining <= 0) Cancel(); }
        }
        public void Cancel() { if (Current == null) return; Current = null; Changed?.Invoke(); }
        public Persistence.EventSaveData Capture()
        {
            var d = new Persistence.EventSaveData { nextWave = nextEventWave, experienced = EventsExperienced, mostRecent = MostRecentEvent, recent = new List<string>(recent) };
            foreach (var pair in lastWave) d.cooldowns.Add(new Persistence.EventCounter(pair.Key, pair.Value));
            foreach (var pair in counts) d.counts.Add(new Persistence.EventCounter(pair.Key, pair.Value));
            foreach (var item in history) d.history.Add(new Persistence.EventCounter(item.Id, item.Wave));
            if (Current != null) { d.currentId = Current.Definition.id; d.wave = Current.Wave; d.sector = (int)Current.Sector; d.active = Current.Active;
                d.wavesRemaining = Current.WavesRemaining; d.secondsRemaining = Current.SecondsRemaining; d.warningRemaining = Current.WarningRemaining; d.supplyReceived = Current.SupplyReceived; }
            return d;
        }
        public void Restore(Persistence.EventSaveData d)
        {
            preparation = true; nextEventWave = d.nextWave; EventsExperienced = d.experienced; MostRecentEvent = d.mostRecent;
            lastWave.Clear(); counts.Clear(); recent.Clear(); history.Clear();
            foreach (var pair in d.cooldowns) lastWave[pair.id] = pair.value;
            foreach (var pair in d.counts) counts[pair.id] = pair.value;
            recent.AddRange(d.recent);
            foreach (var item in d.history) history.Add(new EventHistoryEntry(item.value, item.id));
            Current = null;
            if (!string.IsNullOrEmpty(d.currentId)) {
                var definition = settings == null ? null : Array.Find(settings.events, e => e != null && e.id == d.currentId);
                if (definition == null) throw new InvalidOperationException("Saved event is no longer available.");
                Current = new ActiveEventContext { Definition = definition, Wave = d.wave, Sector = (LandingSector)d.sector, Active = d.active,
                    WavesRemaining = d.wavesRemaining, SecondsRemaining = d.secondsRemaining, WarningRemaining = d.warningRemaining, SupplyReceived = d.supplyReceived };
            }
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool Force(EndlessEventDefinition definition, int wave, bool boss, EconomyManager economy)
        {
            if (!preparation || definition == null || !definition.Eligible(wave, boss)) return false;
            Cancel(); Queue(definition, wave, economy); return true;
        }
#endif
    }
}
