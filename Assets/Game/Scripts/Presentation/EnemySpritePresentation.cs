using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    [RequireComponent(typeof(EnemyController))]
    public sealed class EnemySpritePresentation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private SpriteRenderer wake;
        [SerializeField] private Sprite[] idle, walk, attack, death;
        [SerializeField] private Sprite[] hit;
        [SerializeField] private Sprite[] emergence, burrow, underground, emerge, special;
        [SerializeField] private SpriteRenderer contactShadow;
        public Sprite[] EmergenceFrames => emergence;
        [SerializeField] private float walkFps = 8;
        [SerializeField] private float deathFps = 9;
        [SerializeField] private VfxId arrival = VfxId.SplashSmall;
        [SerializeField] private float effectScale = 1;
        private EnemyController enemy;
        private EnemyHealth health;
        private VfxService effects;
        private Color originalColor;
        private float flashUntil;
        private float hitStarted, hitUntil;
        private bool terrainKnown, wasWater;
        private Vector3 previousPosition;
        private float animationClock;
        private Sprite[] lastSequence;
        private bool wasBurrowed;
        private float burrowClock, emergedAt = float.NegativeInfinity;
        public bool WakeVisible => wake != null && wake.enabled;
        public void Configure(SpriteRenderer renderer, SpriteRenderer waterWake, Sprite[] idleFrames,
            Sprite[] walkFrames, Sprite[] attackFrames, Sprite[] deathFrames)
        { body = renderer; wake = waterWake; idle = idleFrames; walk = walkFrames; attack = attackFrames; death = deathFrames; }
        private void Awake()
        {
            enemy = GetComponent<EnemyController>();
            health = GetComponent<EnemyHealth>();
            if (body != null) originalColor = body.color;
        }
        private void OnEnable()
        {
            // A single scene lookup on activation; never a per-frame search or global mutable singleton.
            effects = FindFirstObjectByType<VfxService>();
            health.Damaged += OnDamaged;
            health.Died += OnDied;
            previousPosition = transform.position;
            if (body != null) body.enabled = true;
            terrainKnown = false; flashUntil = 0; hitUntil = 0; animationClock = 0; lastSequence = null;
            wasBurrowed = false; burrowClock = 0; emergedAt = float.NegativeInfinity;
            if (contactShadow != null) contactShadow.enabled = false;
        }
        private void OnDisable()
        {
            if (health != null)
            { health.Damaged -= OnDamaged; health.Died -= OnDied; }
            if (body != null) body.color = originalColor;
            if (wake != null) wake.enabled = false;
            if (contactShadow != null) contactShadow.enabled = false;
        }
        private void OnDamaged(int amount)
        {
            if (!enemy.Health.IsAlive || !enemy.IsTargetable) return;
            if (enemy.Definition.Kind == EnemyKind.Broodling) return;
            flashUntil = Time.time + .075f;
            // Brief pose override only; navigation and attack clocks continue in gameplay.
            // Repeated rapid hits do not keep restarting the two-frame reaction.
            if (Time.time >= hitUntil && hit != null && hit.Length > 0)
            { hitStarted = Time.time; hitUntil = hitStarted + .2f; }
            if (body != null) body.color = new Color(1, .6f, .6f, originalColor.a);
            if (effects != null && effects.CanPlay)
                effects.Play(VfxId.CreatureFluids, transform.position, body != null ? body.transform.eulerAngles.z : 0, effectScale * .6f);
        }
        private void OnDied()
        {
            if (body != null && effects != null && death != null && death.Length > 0 &&
                (enemy.Definition.Kind != EnemyKind.Broodling || (GetInstanceID() & 3) == 0))
                effects.Death(death, deathFps, body.transform.position, body.transform.rotation, effectScale);
            if (body != null) body.enabled = false;
            if (wake != null) wake.enabled = false;
            if (contactShadow != null) contactShadow.enabled = false;
        }
        private void LateUpdate()
        {
            if (body == null || !enemy.Health.IsAlive) return;
            if (wasBurrowed != enemy.IsBurrowed) { if (!enemy.IsBurrowed) emergedAt = Time.time; burrowClock = 0; wasBurrowed = enemy.IsBurrowed; }
            if (contactShadow != null) contactShadow.enabled = false;
            if (enemy.IsBurrowed)
            {
                var trail = burrowClock < .35f && burrow != null && burrow.Length > 0 ? burrow : underground;
                if (trail != null && trail.Length > 0) body.sprite = trail[(int)(burrowClock * 10) % trail.Length];
                burrowClock += Time.deltaTime;
                body.color = Color.white; if (wake != null) wake.enabled = false;
                previousPosition = transform.position; return;
            }
            if (Time.time >= flashUntil) body.color = originalColor;
            if (!enemy.IsTargetable)
            { if (wake != null) wake.enabled = false; return; }
            bool moving = (transform.position - previousPosition).sqrMagnitude > .000001f;
            previousPosition = transform.position;
            var frames = enemy.Movement.ReachedDestination || enemy.IsAttacking ? attack : moving ? walk : idle;
            if (enemy.Definition.Kind == EnemyKind.Broodmother && enemy.IsAttacking && special != null && special.Length > 0) frames = special;
            float fps = frames == walk ? walkFps : frames == attack ? 10 : frames == special ? 8 : 5;
            if (frames != lastSequence) { animationClock = 0; lastSequence = frames; }
            else animationClock += Time.deltaTime;
            if (frames != null && frames.Length > 0) body.sprite = frames[(int)(animationClock * fps) % frames.Length];
            if (Time.time < hitUntil && hit != null && hit.Length > 0)
                body.sprite = hit[Mathf.Min(hit.Length - 1, (int)((Time.time - hitStarted) * hit.Length / .2f))];
            if (Time.time - emergedAt < .4f && emerge != null && emerge.Length > 0) body.sprite = emerge[Mathf.Min(emerge.Length - 1, (int)((Time.time - emergedAt) * 10))];
            UpdateWater();
            UpdateContactShadow();
        }
        private void UpdateContactShadow()
        {
            if (contactShadow == null) return;
            // The sibling renderer inherits Visual facing; its material offsets in world XY.
            // Keep the exact animated body silhouette and suppress land contact underwater.
            contactShadow.sprite = body.sprite;
            contactShadow.enabled = body.enabled && !enemy.IsBurrowed && !wasWater;
        }
        private void UpdateWater()
        {
            var grid = effects != null ? effects.Grid?.Grid : null;
            if (grid == null || !grid.TryWorldToCell(transform.position, out var cell))
            { if (wake != null) wake.enabled = false; return; }
            bool water = grid.GetCell(cell).Terrain == TerrainType.Water;
            if (terrainKnown && wasWater && !water) effects.Play(arrival, transform.position, 0, effectScale);
            terrainKnown = true; wasWater = water;
            if (wake == null) return;
            var sequence = effects.Catalog.Get(VfxId.WaterWake);
            wake.enabled = water && sequence?.frames != null && sequence.frames.Length > 0 && effects.CanPlay;
            if (!wake.enabled) return;
            wake.sprite = sequence.frames[(int)(Time.time * sequence.fps) % sequence.frames.Length];
            wake.color = new Color(1, 1, 1, sequence.opacity);
        }
    }
}
