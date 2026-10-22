using ShorelineDefense.Core;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    /// <summary>Scene-owned, bounded visual playback. No damage, rewards or simulation commands.</summary>
    public sealed class VfxService : MonoBehaviour
    {
        [SerializeField] private VfxCatalog catalog;
        [SerializeField] private GameStateController game;
        [SerializeField] private GridManager grid;
        [SerializeField] private Material material;
        [SerializeField, Min(1)] private int burstCapacity = 96;
        [SerializeField, Min(1)] private int tracerCapacity = 64;
        [SerializeField, Min(1)] private int smokeCapacity = 16;
        [SerializeField, Min(1)] private int decalCapacity = 48;
        [SerializeField, Min(1)] private int corpseCapacity = 32;
        private SpriteEffectPool[] pools;
        public bool CanPlay => isActiveAndEnabled && game != null && game.IsPlaying;
        public GridManager Grid => grid;
        public Material Material => material;
        public VfxCatalog Catalog => catalog;
        public int TotalCapacity => burstCapacity + tracerCapacity + smokeCapacity + decalCapacity + corpseCapacity;
        public SpriteEffectPool Pool(VfxChannel channel) => pools?[(int)channel];
        public void Configure(VfxCatalog data, GameStateController state, GridManager battlefield, Material spriteMaterial)
        { catalog = data; game = state; grid = battlefield; material = spriteMaterial; }

        private void Awake()
        {
            pools = new[] {
                new SpriteEffectPool(transform, "Burst", burstCapacity, material),
                new SpriteEffectPool(transform, "Tracer", tracerCapacity, material),
                new SpriteEffectPool(transform, "Smoke", smokeCapacity, material),
                new SpriteEffectPool(transform, "Decal", decalCapacity, material),
                new SpriteEffectPool(transform, "Death", corpseCapacity, material) };
        }
        private void OnEnable() { if (game != null) game.StateChanged += OnState; }
        private void OnDisable()
        {
            if (game != null) game.StateChanged -= OnState;
            Clear();
        }
        private void OnState(GameState state) { if (state == GameState.GameOver) Clear(); }
        private void LateUpdate()
        {
            if (pools == null) return;
            for (int i = 0; i < pools.Length; i++) pools[i].Tick(Time.deltaTime);
        }
        public void Clear()
        {
            if (pools == null) return;
            for (int i = 0; i < pools.Length; i++) pools[i].Clear();
        }
        public SpriteRenderer Play(VfxId id, Vector3 point, float angle = 0, float scale = 1, Transform follow = null)
        {
            if (!CanPlay || catalog == null || pools == null) return null;
            var sequence = catalog.Get(id);
            if (sequence == null) return null;
            return pools[(int)sequence.channel].Play(sequence.frames, sequence.fps, point,
                Quaternion.Euler(0, 0, angle), Vector3.one * scale, sequence.sortingLayer,
                sequence.sortingOrder, sequence.lifetime, sequence.opacity, sequence.fadePortion, sequence.drift, follow);
        }
        public void Shot(bool sniper, Transform muzzle, Vector3 impact)
        {
            if (muzzle == null || !CanPlay) return;
            Vector3 direction = impact - muzzle.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Play(sniper ? VfxId.SniperMuzzle : VfxId.MgMuzzle, muzzle.position, angle, 1, muzzle);
            var sequence = catalog.Get(sniper ? VfxId.SniperTracer : VfxId.MgTracer);
            if (sequence == null || sequence.frames == null || sequence.frames.Length == 0) return;
            var size = sequence.frames[0].bounds.size;
            pools[(int)VfxChannel.Tracer].Play(sequence.frames, sequence.fps, (impact + muzzle.position) * .5f,
                Quaternion.Euler(0, 0, angle), new Vector3(direction.magnitude / size.x, 1, 1),
                sequence.sortingLayer, sequence.sortingOrder, sequence.lifetime, sequence.opacity, 0, Vector2.zero);
            Play(VfxId.ImpactFlesh, impact, angle);
        }
        public void Mine(Vector3 position, float radius)
        {
            var sequence = catalog != null ? catalog.Get(VfxId.MineExplosion) : null;
            if (sequence?.frames == null || sequence.frames.Length == 0) return;
            // The largest exported debris ring spans ~80% of its canvas; represent the existing AOE.
            float scale = radius * 2 / (sequence.frames[0].bounds.size.x * .8f);
            Play(VfxId.MineExplosion, position, 0, scale);
            Play(VfxId.SmokeMedium, position, 0, Mathf.Min(1.5f, scale));
        }
        public void Death(Sprite[] frames, float fps, Vector3 position, Quaternion rotation, float scale = 1)
        {
            if (!CanPlay || pools == null) return;
            pools[(int)VfxChannel.Corpse].Play(frames, fps, position, rotation, Vector3.one * scale,
                "GroundObjects", 4, frames.Length / Mathf.Max(1, fps) + .6f, 1, .3f, Vector2.zero);
            Play(VfxId.BloodHit, position, rotation.eulerAngles.z, scale);
            Play(scale > 1 ? VfxId.BloodDecalMedium : VfxId.BloodDecal, position, rotation.eulerAngles.z, scale);
        }
    }
}
