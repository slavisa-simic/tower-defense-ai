using System;
using System.Collections.Generic;
using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Presentation;
using UnityEngine;

namespace ShorelineDefense.Waves
{
    [Serializable]
    public sealed class RiverLandingPoint
    {
        public Vector3 riverEntry;
        public Vector3 berth;
        public Vector3[] unloadingPositions = Array.Empty<Vector3>();
    }

    /// <summary>Bounded transport presentation. The spawner alone creates/initializes passengers.</summary>
    public sealed class BoatLandingSystem : MonoBehaviour
    {
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private GameStateController game;
        [SerializeField] private GameObject boatPrefab;
        [SerializeField] private RiverLandingPoint[] landingPoints = Array.Empty<RiverLandingPoint>();
        [SerializeField, Min(1)] private int capacity = 5;
        [SerializeField, Min(1)] private int maximumActiveBoats = 3;
        [SerializeField, Min(.1f)] private float moveSpeed = 2.5f;
        [SerializeField, Min(.05f)] private float disembarkDelay = .25f;
        [SerializeField, Min(0)] private float departureDelay = 1;
        [SerializeField, Min(.05f)] private float arrivalInterval = 2;
        private sealed class Transport
        {
            public GameObject visual;
            public EnemyController[] cargo;
            public int[] seatOffsets;
            public int point, remaining, nextOffset, stage;
            public float timer, wakeTimer;
            public float speedMultiplier = 1;
            public SpriteRenderer[] passengers;
        }
        private readonly List<Transport> boats = new List<Transport>(6);
        private int nextPoint;
        private readonly Dictionary<GameObject, Stack<GameObject>> visualPools = new Dictionary<GameObject, Stack<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> visualSources = new Dictionary<GameObject, GameObject>();
        private int[] recentLandings;
        private int landingSequence;
        public int CreatedBoatVisuals { get; private set; }
        public int ReusedBoatVisuals { get; private set; }
        private VfxService effects;
        public int Capacity => capacity;
        public float MoveSpeed => moveSpeed;
        public int ConcurrencyLimit => maximumActiveBoats;
        public int PeakConcurrentBoats { get; private set; }
        public void ConfigureMotion(int cargoUnits, float speed, float unloadDelay, float returnDelay) { capacity = Mathf.Clamp(cargoUnits, 3, 18); moveSpeed = Mathf.Max(.1f,speed); disembarkDelay = Mathf.Max(.05f,unloadDelay); departureDelay = Mathf.Max(0,returnDelay); }
        public float ArrivalInterval => arrivalInterval;
        public int ActiveBoats => boats.Count;
        public int IncomingPassengers { get; private set; }
        public int TotalArrivals { get; private set; }
        public IReadOnlyList<RiverLandingPoint> LandingPoints => landingPoints;
        public event Action<EnemyController> Unloaded;

        public void Configure(EnemySpawner executor, GameStateController state, GameObject prefab, RiverLandingPoint[] points)
        {
            if (isActiveAndEnabled && game != null) game.StateChanged -= OnState;
            Cancel(); spawner = executor; game = state; boatPrefab = prefab; landingPoints = points;
            if (isActiveAndEnabled && game != null) game.StateChanged += OnState;
        }
        private void OnEnable() { if (game != null) game.StateChanged += OnState; effects = FindFirstObjectByType<VfxService>(); }
        private void OnDisable() { if (game != null) game.StateChanged -= OnState; Cancel(); }
        private void OnState(GameState state) { if (state == GameState.GameOver) Cancel(); }
        public int PassengerLimit(EnemyController prefab) => Mathf.Max(1, capacity / Mathf.Max(1, prefab.Definition.TransportCapacityUnits));
        public void ConfigurePressure(float interval, int concurrency)
        { arrivalInterval = Mathf.Clamp(interval, .05f, 30); maximumActiveBoats = Mathf.Clamp(concurrency, 1, 32); }
        public bool TryDispatch(EnemyController passenger, int count)
        {
            if (passenger == null || passenger.Definition == null || count <= 0 || count > PassengerLimit(passenger)) return false;
            var cargo = new EnemyController[count]; for (int i = 0; i < count; i++) cargo[i] = passenger;
            return TryDispatch(cargo);
        }
        public bool TryDispatch(IReadOnlyList<EnemyController> cargo)
        {
            if (!isActiveAndEnabled || game == null || !game.IsPlaying || boatPrefab == null || cargo == null || cargo.Count == 0 ||
                landingPoints == null || landingPoints.Length == 0 || boats.Count >= maximumActiveBoats) return false;
            int units = 0;
            foreach (var p in cargo) { if (p == null || p.Definition == null) return false; units += Mathf.Max(1, p.Definition.TransportCapacityUnits); }
            if (units > capacity) return false;
            if (recentLandings == null || recentLandings.Length != landingPoints.Length) recentLandings = new int[landingPoints.Length];
            // Least occupied/recent sector with seeded-style jitter; no predictable round-robin.
            int chosen = -1; float best = float.PositiveInfinity;
            for (int index = 0; index < landingPoints.Length; index++)
            {
                var point = landingPoints[index];
                if (!IsRiverLane(point)) continue;
                bool entryOccupied = false; int laneCount = 0;
                foreach (var active in boats) if (active.point == index) {
                    laneCount++;
                    if ((active.visual.transform.position - point.riverEntry).sqrMagnitude < 16) entryOccupied = true;
                }
                if (entryOccupied) continue;
                float score = laneCount * 4 + recentLandings[index] / (float)Mathf.Max(1, landingSequence) +
                    ((index * 17 + landingSequence * 13) % 23) * .025f;
                if (score < best) { best = score; chosen = index; }
            }
            if (chosen >= 0)
            {
                int index = chosen; var point = landingPoints[index]; recentLandings[index] = ++landingSequence;
                BossDefinition boss = null;
                foreach (var passenger in cargo) if (passenger.Definition is BossDefinition special) { boss = special; break; }
                var source = boss != null && boss.BossBoat != null ? boss.BossBoat : boatPrefab;
                if (!visualPools.TryGetValue(source, out var pool)) visualPools[source] = pool = new Stack<GameObject>();
                GameObject visual;
                if (pool.Count > 0) { visual = pool.Pop(); ReusedBoatVisuals++; }
                else { visual = Instantiate(source, point.riverEntry, Quaternion.identity, transform); visualSources[visual] = source; CreatedBoatVisuals++; }
                visual.transform.SetPositionAndRotation(point.riverEntry, Quaternion.identity); visual.SetActive(true);
                var seats = visual.GetComponentsInChildren<SpriteRenderer>(true);
                var offsets = new int[cargo.Count]; int nextSeat = 0;
                for (int p = 0; p < cargo.Count; p++)
                {
                    int weight = Mathf.Max(1, cargo[p].Definition.TransportCapacityUnits);
                    offsets[p] = nextSeat + weight / 2; nextSeat += weight;
                }
                foreach (var seat in seats)
                    if (seat.name.StartsWith("Passenger ") && int.TryParse(seat.name.Substring(10), out int seatIndex))
                    {
                        int passengerIndex = Array.IndexOf(offsets, seatIndex);
                        seat.enabled = passengerIndex >= 0;
                        if (seat.enabled)
                        {
                            var body = cargo[passengerIndex].transform.Find("Visual/Body");
                            var sprite = body != null ? body.GetComponent<SpriteRenderer>() : null;
                            if (sprite != null) seat.sprite = sprite.sprite;
                        }
                    }
                var stored = new EnemyController[cargo.Count]; for (int i = 0; i < cargo.Count; i++) stored[i] = cargo[i];
                boats.Add(new Transport { visual = visual, cargo = stored, seatOffsets = offsets, point = index, remaining = cargo.Count, passengers = seats, speedMultiplier = boss != null ? boss.BoatSpeedMultiplier : 1 });
                IncomingPassengers += cargo.Count; TotalArrivals++;
                PeakConcurrentBoats = Mathf.Max(PeakConcurrentBoats, boats.Count);
                return true;
            }
            return false;
        }
        private bool IsRiverLane(RiverLandingPoint point)
        {
            var battlefield = spawner != null ? spawner.Battlefield : null;
            if (point == null || battlefield?.Grid == null || battlefield.Grid.Plane != GameplayPlane.XY ||
                point.unloadingPositions == null || point.unloadingPositions.Length == 0) return false;
            var sprite = boatPrefab.GetComponentInChildren<SpriteRenderer>()?.sprite;
            if (sprite == null) return false;
            var grid = battlefield.Grid;
            float shore = grid.Origin.x + battlefield.Definition.WaterBandCells * grid.CellSize;
            float halfWidth = sprite.bounds.extents.x, halfHeight = sprite.bounds.extents.y;
            // Straight horizontal lanes only: both the hull and its full swept path stay in river space.
            return Mathf.Approximately(point.riverEntry.y, point.berth.y) && point.riverEntry.x < point.berth.x &&
                point.berth.x + halfWidth <= shore + .001f && point.berth.y - halfHeight >= grid.Origin.y &&
                point.berth.y + halfHeight <= grid.Origin.y + grid.Height * grid.CellSize &&
                Mathf.Approximately(point.berth.z, grid.Origin.z) && Mathf.Approximately(point.riverEntry.z, grid.Origin.z);
        }
        // WaveManager owns ticking so deterministic tests can advance the whole arrival sequence.
        public void Tick(float deltaTime)
        {
            if (game == null || !game.IsPlaying || deltaTime <= 0 || !isActiveAndEnabled) return;
            for (int i = boats.Count - 1; i >= 0; i--)
            {
                var boat = boats[i]; var point = landingPoints[boat.point];
                if (boat.stage == 0 || boat.stage == 3)
                {
                    // Incoming hulls queue behind an occupied berth/approach; unloading remains per boat.
                    bool waiting = false;
                    if (boat.stage == 0) foreach (var ahead in boats)
                        if (ahead != boat && ahead.point == boat.point && ahead.stage != 3 &&
                            ahead.visual.transform.position.x > boat.visual.transform.position.x &&
                            (ahead.visual.transform.position - boat.visual.transform.position).sqrMagnitude < 16) { waiting = true; break; }
                    if (waiting) continue;
                    var destination = boat.stage == 0 ? point.berth : point.riverEntry;
                    var direction = destination - boat.visual.transform.position;
                    if (direction.sqrMagnitude > .0001f)
                        boat.visual.transform.GetChild(0).rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                    boat.visual.transform.position = Vector3.MoveTowards(boat.visual.transform.position, destination, moveSpeed * boat.speedMultiplier * deltaTime);
                    boat.wakeTimer -= deltaTime;
                    if (boat.wakeTimer <= 0 && effects != null)
                    {
                        effects.Play(VfxId.WaterWake, boat.visual.transform.position - direction.normalized * 1.2f,
                            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, 1.2f);
                        boat.wakeTimer = .3f;
                    }
                    if ((boat.visual.transform.position - destination).sqrMagnitude > .0001f) continue;
                    if (boat.stage == 3) { ReturnVisual(boat.visual); boats.RemoveAt(i); continue; }
                    boat.stage = 1;
                    effects?.Play(VfxId.SplashMedium, point.berth + Vector3.right * 1.4f, 0, .6f);
                }
                if (boat.stage == 1)
                {
                    boat.timer -= deltaTime;
                    if (boat.timer > 0) continue;
                    bool unloaded = false;
                    for (int attempt = 0; attempt < point.unloadingPositions.Length; attempt++)
                    {
                        var position = point.unloadingPositions[boat.nextOffset];
                        boat.nextOffset = (boat.nextOffset + 1) % point.unloadingPositions.Length;
                        if (!spawner.TrySpawnAt(boat.cargo[boat.cargo.Length - boat.remaining], position, out var enemy)) continue;
                        boat.remaining--; IncomingPassengers--; unloaded = true;
                        foreach (var seat in boat.passengers)
                            if (seat.name == "Passenger " + boat.seatOffsets[boat.cargo.Length - boat.remaining - 1]) seat.enabled = false;
                        effects?.Play(VfxId.SplashSmall, position, 0, .4f);
                        Unloaded?.Invoke(enemy);
                        // Observers may cause Game Over and clear the entire transport collection.
                        if (!game.IsPlaying || !isActiveAndEnabled) return;
                        break;
                    }
                    boat.timer = unloaded ? disembarkDelay : .1f;
                    if (boat.remaining == 0) { boat.stage = 2; boat.timer = departureDelay; }
                }
                else if (boat.stage == 2)
                {
                    boat.timer -= deltaTime;
                    if (boat.timer <= 0) boat.stage = 3;
                }
            }
        }
        public void Cancel()
        {
            foreach (var boat in boats) if (boat.visual != null) ReturnVisual(boat.visual);
            boats.Clear(); IncomingPassengers = 0;
        }
        private void ReturnVisual(GameObject visual)
        { visual.SetActive(false); if (visualSources.TryGetValue(visual, out var source)) visualPools[source].Push(visual); else Destroy(visual); }
    }
}
