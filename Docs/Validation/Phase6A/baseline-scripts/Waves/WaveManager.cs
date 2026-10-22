using System;
using System.Collections.Generic;
using ShorelineDefense.Core;
using ShorelineDefense.Economy;
using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Waves
{
    public enum WaveState { Preparation, Spawning, Combat, WaveComplete, AllWavesComplete, GameOver }

    public sealed class WaveManager : MonoBehaviour
    {
        [SerializeField] private WaveDefinition[] waves = Array.Empty<WaveDefinition>();
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private EnemyRegistry registry;
        [SerializeField] private EconomyManager economy;
        [SerializeField] private GameStateController game;
        [SerializeField] private BoatLandingSystem landings;
        [SerializeField] private EndlessWaveSettings endless;
        private EndlessWaveGenerator generator; private RunStatistics statistics;
        private WaveDefinition generated;
        public int RunSeed { get; private set; }
        public bool IsEndless => endless != null;
        public EndlessWaveSettings EndlessSettings => endless;
        public EndlessEventManager Events { get; private set; }
        public event Action EventChanged;
        private void NotifyEventChanged() {
            // An unstarted assault is only a preview. Canceling it must restore the seeded
            // baseline plan as well as live modifiers, or its extra cargo becomes hidden pressure.
            if (generated != null && generator != null && State == WaveState.Preparation && Events?.Current == null) {
                var replacement = generator.Generate(waveIndex + 1);
                ReleaseGenerated(); generated = replacement;
            }
            if (generated != null && endless != null) {
                var modifiers = Events?.Modifiers;
                landings?.ConfigureEvent(modifiers, Events?.Current?.Sector ?? LandingSector.Center);
                landings?.ConfigurePressure(endless.ArrivalInterval(generated.WaveNumber) * (generated.Encounter != null ? generated.Encounter.ArrivalMultiplier : 1) * (modifiers?.boatInterval ?? 1),
                    Mathf.CeilToInt(endless.BoatConcurrency(generated.WaveNumber) * (modifiers?.boatConcurrency ?? 1)));
            }
            EventChanged?.Invoke(); if (generated != null) Changed?.Invoke();
        }
        private void ReleaseEvents() { if (Events != null) { Events.Cancel(); Events.Changed -= NotifyEventChanged; Events = null; } }
        public void ConfigureEndless(EndlessWaveSettings settings)
        { ReleaseEvents(); ReleaseGenerated(); endless = settings; generator = null; waveIndex = 0; EnsureGenerated(); Changed?.Invoke(); }
        public void RestorePreparation(Persistence.RunSaveData data)
        {
            if (!IsEndless || State != WaveState.Preparation) throw new InvalidOperationException("Only fresh endless Preparation can be restored.");
            ReleaseEvents(); ReleaseGenerated(); waveIndex = data.wave - 1; CompletedWaves = data.completedWaves; RunSeed = data.seed;
            generator = new EndlessWaveGenerator(endless, RunSeed);
            Events = new EndlessEventManager(endless.eventSettings, RunSeed); Events.Restore(data.events); Events.Changed += NotifyEventChanged;
            // The announced event is already committed; never call Prepare or credit its supply again.
            generated = generator.Generate(data.wave, Events.Current?.Definition.modifiers);
            NotifyEventChanged(); Changed?.Invoke();
        }
        private void EnsureGenerated()
        {
            if (endless == null || generated != null) return;
            if (generator == null) {
                RunSeed = endless.randomizeSeed ? Guid.NewGuid().GetHashCode() : endless.seed; generator = new EndlessWaveGenerator(endless, RunSeed);
                Events = new EndlessEventManager(endless.eventSettings, RunSeed); Events.Changed += NotifyEventChanged;
            }
            bool boss = endless.bossEncounters != null && endless.bossEncounters.ForWave(waveIndex + 1) != null;
            Events.Prepare(waveIndex + 1, boss, economy);
            generated = generator.Generate(waveIndex + 1, Events.Current?.Definition.modifiers);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool ForceEvent(string id)
        {
            if (State != WaveState.Preparation || endless?.eventSettings == null) return false;
            var definition = Array.Find(endless.eventSettings.events, e => e != null && e.id == id);
            if (!Events.Force(definition, CurrentWaveNumber, CurrentWave.Encounter != null, economy)) return false;
            ReleaseGenerated(); generated = generator.Generate(waveIndex + 1, definition.modifiers);
            Changed?.Invoke(); return true;
        }
#endif
        private void ReleaseGenerated() { if (generated != null) { if (Application.isPlaying) Destroy(generated); else DestroyImmediate(generated); generated = null; } }
        private void OnDestroy() { ReleaseEvents(); ReleaseGenerated(); }
        [SerializeField, Min(0)] private float completionDisplaySeconds = 1.5f;
        private readonly HashSet<EnemyController> alive = new HashSet<EnemyController>();
        private int waveIndex;
        private int groupIndex;
        private int spawnedInGroup;
        private int pending;
        private float timer;
        private bool subscribed;
        private readonly List<EnemyController> cargo = new List<EnemyController>(8);
        private readonly AssaultPressureDirector director = new AssaultPressureDirector();
        private bool bossAwaitingDeployment;
        private EnemyController deployedBoss;
        public AssaultPressureDirector PressureDirector => director;
        public int PendingBoats => landings == null ? 0 : Mathf.CeilToInt(Mathf.Max(0, pending - landings.IncomingPassengers) / (float)landings.Capacity);
        public BoatLandingSystem Landings => landings;
        public WaveState State { get; private set; } = WaveState.Preparation;
        public IReadOnlyList<WaveDefinition> Waves => waves;
        public WaveDefinition CurrentWave { get { EnsureGenerated(); return IsEndless ? generated : waves != null && waveIndex < waves.Length ? waves[waveIndex] : null; } }
        public int CurrentWaveNumber => CurrentWave != null ? CurrentWave.WaveNumber : 0;
        // Cheap index for boundary observers; CurrentWave may generate and emit event callbacks.
        public int PreparationWaveNumber => waveIndex + 1;
        public int EnemiesRemaining => State == WaveState.Preparation ? CurrentWave?.ScheduledCount ?? 0 : pending + alive.Count;
        public int AliveCount => alive.Count;
        public int PendingCount => pending;
        public int CompletedWaves { get; private set; }
        public bool CanStart => isActiveAndEnabled && game != null && game.IsPlaying && State == WaveState.Preparation && CurrentWave != null && CurrentWave.IsValid && (Events?.CanStart ?? true);
        public event Action Changed;
        public event Action PreparationEnding;
        public void ConfigureLandings(BoatLandingSystem transport)
        {
            if (landings != null) landings.Unloaded -= OnUnloaded;
            landings = transport;
            if (subscribed && landings != null) landings.Unloaded += OnUnloaded;
        }

        public void Configure(WaveDefinition[] definitions, EnemySpawner executor, EnemyRegistry enemies, EconomyManager money, GameStateController state)
        {
            Unsubscribe();
            waves = definitions;
            spawner = executor;
            registry = enemies;
            economy = money;
            game = state;
            if (isActiveAndEnabled) Subscribe();
        }
        private void OnEnable() => Subscribe();
        private void Subscribe()
        {
            if (subscribed || game == null || registry == null) return;
            game.StateChanged += OnGameState;
            registry.EnemyRemoved += OnEnemyRemoved;
            if (landings != null) landings.Unloaded += OnUnloaded;
            subscribed = true;
            OnGameState(game.CurrentState);
        }
        private void OnDisable() { Unsubscribe(); Events?.Cancel(); landings?.ConfigureEvent(null); landings?.Cancel(); }
        private void Unsubscribe()
        {
            if (!subscribed) return;
            game.StateChanged -= OnGameState;
            registry.EnemyRemoved -= OnEnemyRemoved;
            if (landings != null) landings.Unloaded -= OnUnloaded;
            subscribed = false;
        }
        private void Start()
        {
            statistics = FindFirstObjectByType<RunStatistics>();
            bool valid = spawner != null && registry != null && economy != null && game != null;
            if (IsEndless) { EnsureGenerated(); valid &= generated != null && generated.IsValid; }
            else { valid &= waves != null && waves.Length > 0; if (valid) foreach (var wave in waves) valid &= wave != null && wave.IsValid; }
            if (!valid) { Debug.LogError("WaveManager requires valid waves and scene dependencies.", this); enabled = false; }
            Changed?.Invoke();
        }
        public bool TryStartWave()
        {
            if (!CanStart) return false;
            PreparationEnding?.Invoke();
            if (!CanStart) return false;
            alive.Clear();
            Events?.BeginWave();
            landings?.ConfigureEvent(Events?.Modifiers, Events?.Current?.Sector ?? LandingSector.Center);
            if (IsEndless)
            {
                spawner.ConfigureWave(endless.maximumActiveEnemies, endless.RuntimeHealthMultiplier(CurrentWave.HealthMultiplier, false), CurrentWave.SpeedMultiplier,
                    endless.RuntimeHealthMultiplier(CurrentWave.HealthMultiplier, true));
                landings?.ConfigurePressure(endless.ArrivalInterval(CurrentWaveNumber) * (generated.Encounter != null ? generated.Encounter.ArrivalMultiplier : 1) * (Events?.Modifiers.boatInterval ?? 1), Mathf.CeilToInt(endless.BoatConcurrency(CurrentWaveNumber) * (Events?.Modifiers.boatConcurrency ?? 1)));
                if (endless.pressureDirectorEnabled) {
                    landings?.ConfigureMotion(endless.boatCapacity, endless.boatSpeed, endless.unloadInterval, endless.departureDelay);
                    spawner.ConfigureDeployment(endless.disembarkSeparation);
                }
            }
            director.Reset();
            bossAwaitingDeployment = CurrentWave.Encounter != null; deployedBoss = null;
            pending = CurrentWave.ScheduledCount;
            groupIndex = spawnedInGroup = 0;
            timer = CurrentWave.Groups[0].DelayBeforeGroup;
            if (IsEndless && endless.pressureDirectorEnabled) timer = Mathf.Max(.05f, endless.firstAttackDelay);
            SetState(WaveState.Spawning);
            return true;
        }
        private void Update() { Tick(Time.deltaTime); if (game != null && game.IsPlaying && (State == WaveState.Spawning || State == WaveState.Combat)) statistics?.SamplePressure(registry.Count, Time.unscaledDeltaTime); }
        public void Tick(float deltaTime)
        {
            if (game == null || !game.IsPlaying || deltaTime <= 0) return;
            Events?.Tick(deltaTime);
            landings?.Tick(deltaTime);
            statistics?.RecordPressure(registry.Count, landings != null ? landings.ActiveBoats : 0);
            if (!game.IsPlaying) return;
            if (State == WaveState.WaveComplete)
            {
                timer -= deltaTime;
                if (timer > 0) return;
                if (IsEndless) { if (waveIndex < int.MaxValue - 1) waveIndex++; ReleaseGenerated(); EnsureGenerated(); SetState(WaveState.Preparation); }
                else if (waveIndex + 1 < waves.Length) { waveIndex++; SetState(WaveState.Preparation); }
                else SetState(WaveState.AllWavesComplete);
                return;
            }
            if (State != WaveState.Spawning) return;
            if (IsEndless && endless.pressureDirectorEnabled && landings != null && director.Tick(deltaTime, endless, CurrentWaveNumber,
                registry.Count, pending, landings.ActiveBoats, landings.IncomingPassengers, bossAwaitingDeployment || deployedBoss != null && deployedBoss.IsTargetable, Events?.Modifiers))
            {
                landings.ConfigurePressure(director.ArrivalInterval, director.BoatLimit);
                if (landings.TotalArrivals > 0 && director.Mode == AssaultPressure.Accelerated)
                    timer = Mathf.Min(timer, director.ArrivalInterval);
            }
            if (groupIndex >= CurrentWave.Groups.Count) return;
            if (IsEndless && endless.pressureDirectorEnabled && landings != null && director.DeploymentPaused) return;
            timer -= deltaTime;
            if (timer > 0) return;
            var group = CurrentWave.Groups[groupIndex];
            if (landings != null)
            {
                if (IsEndless)
                {
                    cargo.Clear();
                    int units = 0, nextGroup = groupIndex, nextCount = spawnedInGroup;
                    while (nextGroup < CurrentWave.Groups.Count)
                    {
                        var candidate = CurrentWave.Groups[nextGroup];
                        int weight = candidate.Prefab.Definition.TransportCapacityUnits;
                        if (cargo.Count > 0 && candidate.Prefab.Definition is BossDefinition) break;
                        if (units + weight > landings.Capacity) break;
                        cargo.Add(candidate.Prefab); units += weight; nextCount++;
                        if (nextCount == candidate.Count) { nextGroup++; nextCount = 0; }
                    }
                    if (!landings.TryDispatch(cargo)) { timer = .1f; return; }
                    groupIndex = nextGroup; spawnedInGroup = nextCount;
                    timer = landings.ArrivalInterval; Changed?.Invoke(); return;
                }
                int count = Mathf.Min(group.Count - spawnedInGroup, landings.PassengerLimit(group.Prefab));
                if (!landings.TryDispatch(group.Prefab, count)) { timer = .1f; return; }
                spawnedInGroup += count;
                timer = landings.ArrivalInterval;
                if (spawnedInGroup == group.Count)
                {
                    groupIndex++; spawnedInGroup = 0;
                    if (groupIndex == CurrentWave.Groups.Count) { if (pending == 0) SetState(WaveState.Combat); }
                    else timer += CurrentWave.Groups[groupIndex].DelayBeforeGroup;
                }
                Changed?.Invoke();
                return;
            }
            if (!spawner.TrySpawn(group.Prefab, out var enemy)) { timer = 0.1f; return; }
            alive.Add(enemy);
            pending--;
            spawnedInGroup++;
            timer = group.SpawnInterval;
            if (spawnedInGroup == group.Count)
            {
                groupIndex++;
                spawnedInGroup = 0;
                if (groupIndex == CurrentWave.Groups.Count) SetState(WaveState.Combat);
                else timer += CurrentWave.Groups[groupIndex].DelayBeforeGroup;
            }
            Changed?.Invoke();
            CheckCompletion();
        }
        private void OnUnloaded(EnemyController enemy)
        {
            if (State != WaveState.Spawning || !game.IsPlaying) return;
            alive.Add(enemy); pending--;
            if (enemy.Definition is BossDefinition) { bossAwaitingDeployment = false; deployedBoss = enemy; }
            if (pending == 0 && groupIndex == CurrentWave.Groups.Count) SetState(WaveState.Combat);
            Changed?.Invoke(); CheckCompletion();
        }
        private void OnEnemyRemoved(EnemyController enemy)
        {
            if (deployedBoss == enemy) deployedBoss = null;
            if (!alive.Remove(enemy)) return;
            Changed?.Invoke();
            CheckCompletion();
        }
        private void CheckCompletion()
        {
            if (State != WaveState.Combat || pending != 0 || alive.Count != 0 || !game.IsPlaying) return;
            // Commit state before income or observers can reenter this manager.
            timer = completionDisplaySeconds;
            if (CompletedWaves < int.MaxValue) CompletedWaves++;
            SetState(WaveState.WaveComplete);
            if (game.IsPlaying && State == WaveState.WaveComplete) economy.AddMoney(CurrentWave.CompletionReward);
        }
        private void OnGameState(GameState state)
        {
            if (state == GameState.GameOver) SetState(WaveState.GameOver);
            else Changed?.Invoke();
        }
        private void SetState(WaveState state) {
            State = state;
            if (state == WaveState.WaveComplete) Events?.CompleteWave();
            if (state == WaveState.GameOver) Events?.Cancel();
            if (state == WaveState.WaveComplete || state == WaveState.GameOver) landings?.ConfigureEvent(null);
            Changed?.Invoke();
        }
    }
}
