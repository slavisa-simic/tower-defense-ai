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
        public bool authoredSector;
        public LandingSector sector;
        [Min(1)] public int minimumWave = 1;
        public Vector3[] unloadingPositions = Array.Empty<Vector3>();
        public Vector3[] lateUnloadingPositions = Array.Empty<Vector3>();
    }

    /// <summary>Bounded transport presentation. The spawner alone creates/initializes passengers.</summary>
    public class AssaultCarrierSystem : MonoBehaviour
    {
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private GameStateController game;
        [SerializeField] private GameObject boatPrefab;
        [SerializeField] private CarrierDefinition[] carriers = Array.Empty<CarrierDefinition>();
        [SerializeField, Range(0, 1)] private float earlyDirectRatio = .6f;
        [SerializeField, Range(0, 1)] private float lateDirectRatio = .3f;
        [SerializeField, Range(0, 1)] private float broodRatio = .25f;
        [SerializeField, Min(0)] private int distributedPressureFromWave;
        [SerializeField, Range(1, 4)] private float lateReleaseRate = 1;
        [SerializeField, Range(1, 3)] private float latePressureMultiplier = 1;
        [SerializeField, Range(.25f, 1)] private float lateArrivalMultiplier = 1;
        [SerializeField, Range(0, 8)] private int lateExtraCarriers;
        [SerializeField, Range(1, 4)] private float lateSpitterWeight = 1;
        [SerializeField, Range(1, 4)] private float lateBurrowerWeight = 1;
        [SerializeField, Range(1, 3)] private float lateFormationMultiplier = 1;
        private float SplitPressureAgeAt(int wave) => distributedPressureFromWave > 0 ? Mathf.Clamp01((wave - distributedPressureFromWave) / 20f) : 0;
        private float SplitPressureAge => SplitPressureAgeAt(waveNumber);
        public float CargoWeight(EnemyKind kind, int wave) => Mathf.Lerp(1,
            kind == EnemyKind.Spitter ? lateSpitterWeight : kind == EnemyKind.Burrower ? lateBurrowerWeight : 1, SplitPressureAgeAt(wave));
        public int FormationQuantity(int wave, int baseline) => Mathf.Clamp(Mathf.RoundToInt(baseline *
            Mathf.Lerp(1, lateFormationMultiplier, SplitPressureAgeAt(wave))), 1, 20);
        public float PressureMultiplier => Mathf.Lerp(1, latePressureMultiplier, SplitPressureAge);
        public float ArrivalMultiplier => Mathf.Lerp(1, lateArrivalMultiplier, SplitPressureAge);
        public int ExtraCarriers => Mathf.RoundToInt(lateExtraCarriers * SplitPressureAge);
        public Vector3[] PositionsFor(RiverLandingPoint point) => distributedPressureFromWave > 0 && waveNumber >= distributedPressureFromWave &&
            point.lateUnloadingPositions != null && point.lateUnloadingPositions.Length > 0 ? point.lateUnloadingPositions : point.unloadingPositions;
        public bool IntersectsReleaseCells(Vector2Int anchor, Vector2Int footprint)
        {
            var grid = spawner?.Battlefield?.Grid;
            if (distributedPressureFromWave <= 0 || grid == null) return false;
            foreach (var point in landingPoints)
            {
                if (point == null) continue;
                if (Intersects(point.unloadingPositions) || Intersects(point.lateUnloadingPositions)) return true;
            }
            return false;
            bool Intersects(Vector3[] positions)
            {
                if (positions == null) return false;
                foreach (var position in positions)
                    if (grid.TryWorldToCell(position, out var cell) && cell.x >= anchor.x && cell.y >= anchor.y &&
                        cell.x < anchor.x + footprint.x && cell.y < anchor.y + footprint.y) return true;
                return false;
            }
        }
        private bool HasWalkableRelease(RiverLandingPoint point)
        {
            var grid = spawner.Battlefield.Grid;
            foreach (var position in PositionsFor(point))
                if (grid.TryWorldToCell(position, out var cell) && grid.IsTraversable(cell)) return true;
            return false;
        }
        private int waveNumber = 1;
        public IReadOnlyList<CarrierDefinition> Carriers => carriers;
        public void ConfigureWave(int number) => waveNumber = Mathf.Max(1, number);
        public event Action<ArrivalMethod> CarrierApproach;
        public event Action<ArrivalMethod> CarrierRupture;
        public event Action BossEmergence;
        private readonly int[] arrivalCounts = new int[4];
        public IReadOnlyList<int> ArrivalCounts => arrivalCounts;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public ArrivalMethod? ForcedArrival { get; set; }
#endif
        public int ActiveCarriers => boats.Count;
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
            public CarrierDefinition definition;
            public Vector3 berth;
            public AquaticCarrierPresentation presentation;
        }
        private readonly List<Transport> boats = new List<Transport>(6);
        private readonly Dictionary<GameObject, Stack<GameObject>> visualPools = new Dictionary<GameObject, Stack<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> visualSources = new Dictionary<GameObject, GameObject>();
        private int[] recentLandings;
        private int landingSequence;
        private EventModifiers eventModifiers;
        private LandingSector targetSector;
        private readonly int[] sectorArrivals = new int[3];
        public IReadOnlyList<int> SectorArrivals => sectorArrivals;
        public void ConfigureEvent(EventModifiers modifiers, LandingSector sector = LandingSector.Center)
        { eventModifiers = modifiers; targetSector = sector; }
        public LandingSector SectorFor(RiverLandingPoint point)
        {
            if (point.authoredSector) return point.sector;
            var grid = spawner.Battlefield.Grid;
            float fraction = (point.berth.y - grid.Origin.y) / (grid.Height * grid.CellSize);
            return fraction < 1f / 3 ? LandingSector.South : fraction > 2f / 3 ? LandingSector.North : LandingSector.Center;
        }
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
        protected virtual void OnEnable() { if (game != null) game.StateChanged += OnState; effects = FindFirstObjectByType<VfxService>(); }
        protected virtual void OnDisable() { if (game != null) game.StateChanged -= OnState; Cancel(); }
        private void OnState(GameState state) { if (state == GameState.GameOver) Cancel(); }
        public CarrierDefinition SelectCarrier(EnemyController first)
        {
            if (carriers == null || carriers.Length == 0 || first == null) return null;
            ArrivalMethod method;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (ForcedArrival.HasValue) return Array.Find(carriers, c => c != null && c.method == ForcedArrival.Value);
#endif
            if (first.Definition.Kind == EnemyKind.Dredger) method = ArrivalMethod.DirectEmergence;
            else if (first.Definition is BossDefinition) method = ArrivalMethod.LeviathanCarrier;
            else
            {
                float roll = ((landingSequence * 37 + waveNumber * 11) % 100) / 100f;
                float direct = eventModifiers?.directEmergenceRatio >= 0 ? eventModifiers.directEmergenceRatio : waveNumber < 7 ? earlyDirectRatio : lateDirectRatio;
                bool eligible = first.Definition.Kind == EnemyKind.Crawler || first.Definition.Kind == EnemyKind.Skitter || first.Definition.Kind == EnemyKind.Burrower;
                float brood = eventModifiers?.broodCarrierRatio >= 0 ? eventModifiers.broodCarrierRatio : broodRatio;
                method = eligible && roll < direct ? ArrivalMethod.DirectEmergence : waveNumber >= 7 && roll > 1 - brood ? ArrivalMethod.BroodCarrier : ArrivalMethod.DriftPod;
                if (first.Definition.Kind == EnemyKind.Bulwark && waveNumber >= 15 && roll > .85f) method = ArrivalMethod.LeviathanCarrier;
            }
            return Array.Find(carriers, c => c != null && c.method == method);
        }
        public int RequestCapacityHalfUnits(EnemyController first) => SelectCarrier(first)?.capacityHalfUnits ?? capacity * 2;
        public bool CanCarry(EnemyController first, EnemyController candidate) => SelectCarrier(first)?.Accepts(candidate.Definition) ?? true;
        public int PassengerLimit(EnemyController prefab) => Mathf.Max(1, RequestCapacityHalfUnits(prefab) / prefab.Definition.CarrierHalfUnits);
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
            var carrier = SelectCarrier(cargo[0]);
            int units = 0;
            foreach (var p in cargo) { if (p == null || p.Definition == null || !(carrier?.Accepts(p.Definition) ?? true)) return false; units += p.Definition.CarrierHalfUnits; }
            if (units > (carrier?.capacityHalfUnits ?? capacity * 2)) return false;
            if (recentLandings == null || recentLandings.Length != landingPoints.Length) recentLandings = new int[landingPoints.Length];
            // Least occupied/recent sector with seeded-style jitter; no predictable round-robin.
            int chosen = -1; float best = float.PositiveInfinity;
            for (int index = 0; index < landingPoints.Length; index++)
            {
                var point = landingPoints[index];
                if (point == null || waveNumber < point.minimumWave) continue;
                if (!IsRiverLane(point, carrier) || !HasWalkableRelease(point)) continue;
                // Four of five dispatches wait for the announced sector instead of silently spilling
                // into every free lane. The fifth allows supporting landings elsewhere.
                if (eventModifiers?.landingPattern == LandingPattern.Concentrated && landingSequence % 5 != 4 && SectorFor(point) != targetSector) continue;
                bool entryOccupied = false; int laneCount = 0;
                foreach (var active in boats) {
                    if (active.point == index) laneCount++;
                    if ((active.point == index || spawner.Battlefield.Definition.authoredWater) && (active.visual.transform.position - point.riverEntry).sqrMagnitude < ClearanceSquared(active.definition, carrier)) entryOccupied = true;
                }
                if (entryOccupied) continue;
                float score = laneCount * 4 + recentLandings[index] / (float)Mathf.Max(1, landingSequence) +
                    ((index * 17 + landingSequence * 13) % 23) * .025f;
                if (eventModifiers?.landingPattern == LandingPattern.Distributed ||
                    distributedPressureFromWave > 0 && waveNumber >= distributedPressureFromWave && eventModifiers?.landingPattern != LandingPattern.Concentrated)
                    score += (int)SectorFor(point) == landingSequence % 3 ? -20 : 0;
                if (eventModifiers?.landingPattern == LandingPattern.Concentrated)
                    score += SectorFor(point) == targetSector ? -20 : 0;
                if (score < best) { best = score; chosen = index; }
            }
            if (chosen >= 0)
            {
                int index = chosen; var point = landingPoints[index]; recentLandings[index] = ++landingSequence;
                sectorArrivals[(int)SectorFor(point)]++;
                BossDefinition boss = null;
                foreach (var passenger in cargo) if (passenger.Definition is BossDefinition special) { boss = special; break; }
                var source = carrier != null ? carrier.visualPrefab : boss != null && boss.BossBoat != null ? boss.BossBoat : eventModifiers?.boatVisual != null ? eventModifiers.boatVisual : boatPrefab;
                if (source == null) return false;
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
                var presentation = visual.GetComponent<AquaticCarrierPresentation>();
                presentation?.Begin(carrier, cargo[0]);
                boats.Add(new Transport { visual = visual, cargo = stored, seatOffsets = offsets, point = index, remaining = cargo.Count, passengers = seats, speedMultiplier = carrier != null ? carrier.speedMultiplier : boss != null ? boss.BoatSpeedMultiplier : 1,
                    definition = carrier, berth = Berth(point, carrier), presentation = presentation });
                IncomingPassengers += cargo.Count; TotalArrivals++;
                if (carrier != null) { arrivalCounts[(int)carrier.method]++; CarrierApproach?.Invoke(carrier.method); }
                PeakConcurrentBoats = Mathf.Max(PeakConcurrentBoats, boats.Count);
                return true;
            }
            return false;
        }
        public bool IsValidApproach(RiverLandingPoint point) => IsRiverLane(point, carriers != null && carriers.Length > 0 ? Array.Find(carriers, c => c != null && c.method == ArrivalMethod.DriftPod) : null);
        public bool IsValidApproach(RiverLandingPoint point, CarrierDefinition carrier) => IsRiverLane(point, carrier);
        private static float ClearanceSquared(CarrierDefinition a, CarrierDefinition b)
        { float clearance = (a != null ? a.halfExtents.x : 1.875f) + (b != null ? b.halfExtents.x : 1.875f) + .25f; return clearance * clearance; }
        private Vector3 Berth(RiverLandingPoint point, CarrierDefinition carrier) => carrier == null ? point.berth : point.berth - Vector3.right * Mathf.Max(0, carrier.halfExtents.x - 1.875f);
        private bool IsRiverLane(RiverLandingPoint point, CarrierDefinition carrier = null)
        {
            var battlefield = spawner != null ? spawner.Battlefield : null;
            if (point == null || battlefield?.Grid == null || battlefield.Grid.Plane != GameplayPlane.XY ||
                point.unloadingPositions == null || point.unloadingPositions.Length == 0) return false;
            var sprite = boatPrefab.GetComponentInChildren<SpriteRenderer>()?.sprite;
            if (sprite == null) return false;
            var grid = battlefield.Grid;
            Vector2 extent = carrier != null ? carrier.halfExtents : (Vector2)sprite.bounds.extents;
            var berth = Berth(point, carrier);
            if (battlefield.Definition.authoredWater)
            {
                if (!Mathf.Approximately(point.berth.z, grid.Origin.z) || !Mathf.Approximately(point.riverEntry.z, grid.Origin.z)) return false;
                if (point.riverEntry == point.berth) return false;
                // Sample the swept hull conservatively. Off-grid river is allowed on left/bottom only.
                int steps = Mathf.CeilToInt(Vector3.Distance(point.riverEntry, point.berth) / (grid.CellSize * .25f));
                for (int i = 0; i <= steps; i++)
                {
                    var center = Vector3.Lerp(point.riverEntry, berth, (float)i / steps);
                    for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++)
                    {
                        var sample = center + new Vector3(x * extent.x, y * extent.y, 0);
                        if (grid.TryWorldToCell(sample, out var cell)) { if (grid.GetCell(cell).Terrain != TerrainType.Water) return false; }
                        else if (sample.x >= grid.Origin.x && sample.y >= grid.Origin.y) return false;
                    }
                }
                return true;
            }
            float shore = grid.Origin.x + battlefield.Definition.WaterBandCells * grid.CellSize;
            float halfWidth = extent.x, halfHeight = extent.y;
            // Straight horizontal lanes only: both the hull and its full swept path stay in river space.
            return Mathf.Approximately(point.riverEntry.y, point.berth.y) && point.riverEntry.x < point.berth.x &&
                berth.x + halfWidth <= shore + .001f && berth.y - halfHeight >= grid.Origin.y &&
                berth.y + halfHeight <= grid.Origin.y + grid.Height * grid.CellSize &&
                Mathf.Approximately(point.berth.z, grid.Origin.z) && Mathf.Approximately(point.riverEntry.z, grid.Origin.z);
        }
        // WaveManager owns ticking so deterministic tests can advance the whole arrival sequence.
        public void Tick(float deltaTime)
        {
            if (game == null || !game.IsPlaying || deltaTime <= 0 || !isActiveAndEnabled) return;
            for (int i = boats.Count - 1; i >= 0; i--)
            {
                var boat = boats[i]; var point = landingPoints[boat.point];
                boat.presentation?.Tick(boat.stage, deltaTime);
                if (boat.stage == 0 || boat.stage == 3)
                {
                    // Incoming hulls queue behind an occupied berth/approach; unloading remains per boat.
                    bool waiting = false;
                    if (boat.stage == 0) foreach (var ahead in boats)
                        if (ahead != boat && (ahead.point == boat.point || spawner.Battlefield.Definition.authoredWater) && ahead.stage != 3 &&
                            ahead.visual.transform.position.x > boat.visual.transform.position.x &&
                            (ahead.visual.transform.position - boat.visual.transform.position).sqrMagnitude < ClearanceSquared(ahead.definition, boat.definition)) { waiting = true; break; }
                    if (waiting) continue;
                    var destination = boat.stage == 0 ? boat.berth : point.riverEntry;
                    var direction = destination - boat.visual.transform.position;
                    if (direction.sqrMagnitude > .0001f)
                        boat.visual.transform.GetChild(0).rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                    boat.visual.transform.position = Vector3.MoveTowards(boat.visual.transform.position, destination, moveSpeed * boat.speedMultiplier * (eventModifiers?.boatSpeed ?? 1) * deltaTime);
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
                    boat.timer = boat.definition?.ruptureSeconds ?? 0;
                    if (boat.definition?.method == ArrivalMethod.DriftPod) effects?.Play(VfxId.PodRupture, boat.berth, 0, .8f);
                    if (boat.definition?.method == ArrivalMethod.BroodCarrier || boat.definition?.method == ArrivalMethod.LeviathanCarrier) effects?.Play(VfxId.BroodRelease, boat.berth, 0, 1);
                    if (boat.definition != null) CarrierRupture?.Invoke(boat.definition.method);
                    if (boat.cargo[0].Definition is BossDefinition) BossEmergence?.Invoke();
                    effects?.Play(VfxId.SplashMedium, point.berth + Vector3.right * 1.4f, 0, .6f);
                }
                if (boat.stage == 1)
                {
                    boat.timer -= deltaTime;
                    if (boat.timer > 0) continue;
                    float rate = distributedPressureFromWave > 0 && waveNumber >= distributedPressureFromWave ? lateReleaseRate : 1;
                    // Late split-route pressure can release more than one passenger per frame.
                    // Retain the interval remainder; every release still uses spawner admission.
                    do
                    {
                        bool unloaded = false;
                        var positions = PositionsFor(point);
                        for (int attempt = 0; attempt < positions.Length; attempt++)
                        {
                            var position = positions[boat.nextOffset];
                            boat.nextOffset = (boat.nextOffset + 1) % positions.Length;
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
                        float interval = (boat.definition?.releaseInterval ?? disembarkDelay) / Mathf.Max(1, rate);
                        boat.timer = unloaded ? rate > 1 ? boat.timer + interval : interval : .1f;
                        if (boat.remaining == 0) { boat.stage = 2; boat.timer = boat.definition?.remainsSeconds ?? departureDelay; }
                        if (!unloaded || rate <= 1) break;
                    } while (boat.stage == 1 && boat.timer <= 0);
                }
                else if (boat.stage == 2)
                {
                    boat.timer -= deltaTime;
                    if (boat.timer <= 0) {
                        if (boat.definition != null && (boat.definition.method == ArrivalMethod.DriftPod || boat.definition.method == ArrivalMethod.DirectEmergence)) { ReturnVisual(boat.visual); boats.RemoveAt(i); }
                        else boat.stage = 3;
                    }
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
