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
            public EnemyController passenger;
            public int point, remaining, nextOffset, stage;
            public float timer, wakeTimer;
            public SpriteRenderer[] passengers;
        }
        private readonly List<Transport> boats = new List<Transport>(6);
        private int nextPoint;
        private VfxService effects;
        public int Capacity => capacity;
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
        public bool TryDispatch(EnemyController passenger, int count)
        {
            if (!isActiveAndEnabled || game == null || !game.IsPlaying || boatPrefab == null || passenger == null ||
                passenger.Definition == null || count <= 0 || count > PassengerLimit(passenger) ||
                landingPoints == null || landingPoints.Length == 0 || boats.Count >= maximumActiveBoats) return false;
            for (int attempt = 0; attempt < landingPoints.Length; attempt++)
            {
                int index = nextPoint; nextPoint = (nextPoint + 1) % landingPoints.Length;
                bool occupied = false; foreach (var boat in boats) occupied |= boat.point == index;
                var point = landingPoints[index];
                if (occupied || !IsRiverLane(point)) continue;
                var visual = Instantiate(boatPrefab, point.riverEntry, Quaternion.identity, transform);
                var seats = visual.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var seat in seats)
                    if (seat.name.StartsWith("Passenger ") && int.TryParse(seat.name.Substring(10), out int seatIndex))
                        seat.enabled = seatIndex < count;
                boats.Add(new Transport { visual = visual, passenger = passenger, point = index, remaining = count, passengers = seats });
                IncomingPassengers += count; TotalArrivals++;
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
                    var destination = boat.stage == 0 ? point.berth : point.riverEntry;
                    var direction = destination - boat.visual.transform.position;
                    if (direction.sqrMagnitude > .0001f)
                        boat.visual.transform.GetChild(0).rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                    boat.visual.transform.position = Vector3.MoveTowards(boat.visual.transform.position, destination, moveSpeed * deltaTime);
                    boat.wakeTimer -= deltaTime;
                    if (boat.wakeTimer <= 0 && effects != null)
                    {
                        effects.Play(VfxId.WaterWake, boat.visual.transform.position - direction.normalized * 1.2f,
                            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, 1.2f);
                        boat.wakeTimer = .3f;
                    }
                    if ((boat.visual.transform.position - destination).sqrMagnitude > .0001f) continue;
                    if (boat.stage == 3) { Destroy(boat.visual); boats.RemoveAt(i); continue; }
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
                        if (!spawner.TrySpawnAt(boat.passenger, position, out var enemy)) continue;
                        boat.remaining--; IncomingPassengers--; unloaded = true;
                        foreach (var seat in boat.passengers)
                            if (seat.name == "Passenger " + boat.remaining) seat.enabled = false;
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
            foreach (var boat in boats) if (boat.visual != null) Destroy(boat.visual);
            boats.Clear(); IncomingPassengers = 0;
        }
    }
}
