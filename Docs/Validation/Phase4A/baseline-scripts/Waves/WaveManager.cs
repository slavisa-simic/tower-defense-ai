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
        [SerializeField, Min(0)] private float completionDisplaySeconds = 1.5f;
        private readonly HashSet<EnemyController> alive = new HashSet<EnemyController>();
        private int waveIndex;
        private int groupIndex;
        private int spawnedInGroup;
        private int pending;
        private float timer;
        private bool subscribed;
        public WaveState State { get; private set; } = WaveState.Preparation;
        public IReadOnlyList<WaveDefinition> Waves => waves;
        public WaveDefinition CurrentWave => waves != null && waveIndex < waves.Length ? waves[waveIndex] : null;
        public int CurrentWaveNumber => CurrentWave != null ? CurrentWave.WaveNumber : 0;
        public int EnemiesRemaining => State == WaveState.Preparation ? CurrentWave?.ScheduledCount ?? 0 : pending + alive.Count;
        public int AliveCount => alive.Count;
        public int PendingCount => pending;
        public int CompletedWaves { get; private set; }
        public bool CanStart => isActiveAndEnabled && game != null && game.IsPlaying && State == WaveState.Preparation && CurrentWave != null && CurrentWave.IsValid;
        public event Action Changed;
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
        private void OnDisable() { Unsubscribe(); landings?.Cancel(); }
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
            bool valid = waves != null && waves.Length > 0 && spawner != null && registry != null && economy != null && game != null;
            if (valid) foreach (var wave in waves) valid &= wave != null && wave.IsValid;
            if (!valid) { Debug.LogError("WaveManager requires valid waves and scene dependencies.", this); enabled = false; }
            Changed?.Invoke();
        }
        public bool TryStartWave()
        {
            if (!CanStart) return false;
            alive.Clear();
            pending = CurrentWave.ScheduledCount;
            groupIndex = spawnedInGroup = 0;
            timer = CurrentWave.Groups[0].DelayBeforeGroup;
            SetState(WaveState.Spawning);
            return true;
        }
        private void Update() => Tick(Time.deltaTime);
        public void Tick(float deltaTime)
        {
            if (game == null || !game.IsPlaying || deltaTime <= 0) return;
            landings?.Tick(deltaTime);
            if (!game.IsPlaying) return;
            if (State == WaveState.WaveComplete)
            {
                timer -= deltaTime;
                if (timer > 0) return;
                if (waveIndex + 1 < waves.Length) { waveIndex++; SetState(WaveState.Preparation); }
                else SetState(WaveState.AllWavesComplete);
                return;
            }
            if (State != WaveState.Spawning) return;
            if (groupIndex >= CurrentWave.Groups.Count) return;
            timer -= deltaTime;
            if (timer > 0) return;
            var group = CurrentWave.Groups[groupIndex];
            if (landings != null)
            {
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
            if (pending == 0 && groupIndex == CurrentWave.Groups.Count) SetState(WaveState.Combat);
            Changed?.Invoke(); CheckCompletion();
        }
        private void OnEnemyRemoved(EnemyController enemy)
        {
            if (!alive.Remove(enemy)) return;
            Changed?.Invoke();
            CheckCompletion();
        }
        private void CheckCompletion()
        {
            if (State != WaveState.Combat || pending != 0 || alive.Count != 0 || !game.IsPlaying) return;
            // Commit state before income or observers can reenter this manager.
            timer = completionDisplaySeconds;
            CompletedWaves++;
            SetState(WaveState.WaveComplete);
            if (game.IsPlaying && State == WaveState.WaveComplete) economy.AddMoney(CurrentWave.CompletionReward);
        }
        private void OnGameState(GameState state)
        {
            if (state == GameState.GameOver) SetState(WaveState.GameOver);
            else Changed?.Invoke();
        }
        private void SetState(WaveState state) { State = state; Changed?.Invoke(); }
    }
}
