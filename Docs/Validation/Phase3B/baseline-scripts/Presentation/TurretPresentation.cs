using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    [RequireComponent(typeof(TurretCombat))]
    public class TurretPresentation : MonoBehaviour
    {
        [SerializeField] private Transform weapon;
        [SerializeField] private Transform muzzle;
        [SerializeField] private SpriteRenderer flash;
        [SerializeField] private SpriteRenderer tracer;
        [SerializeField, Min(0.01f)] private float flashDuration = 0.075f;
        [SerializeField, Min(0.01f)] private float tracerWidth = 0.055f;
        [Header("Weapon sprite recoil (presentation only)")]
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite[] firingFrames;
        [SerializeField, Min(1)] private float firingFramesPerSecond = 20;
        private TurretCombat combat;
        private float visibleUntil;
        private float shotStarted = float.NegativeInfinity;
        public void Configure(Transform rotatingWeapon, Transform muzzlePoint, SpriteRenderer muzzleFlash, SpriteRenderer shotTracer)
        { weapon = rotatingWeapon; muzzle = muzzlePoint; flash = muzzleFlash; tracer = shotTracer; }
        public void ConfigureSprites(SpriteRenderer renderer, Sprite idle, Sprite[] frames, float framesPerSecond)
        { weaponRenderer = renderer; idleSprite = idle; firingFrames = frames; firingFramesPerSecond = Mathf.Max(1, framesPerSecond); }
        protected void Awake() => combat = GetComponent<TurretCombat>();
        protected void OnEnable() { combat.Fired += OnFired; ResetSprite(); Hide(); }
        protected void OnDisable() { if (combat != null) combat.Fired -= OnFired; ResetSprite(); Hide(); }
        protected void LateUpdate()
        {
            if (combat.Target != null) Aim(combat.Target.transform.position);
            if (Time.time >= visibleUntil) Hide();
            UpdateSprite();
        }
        private void ResetSprite()
        {
            shotStarted = float.NegativeInfinity;
            if (weaponRenderer != null && idleSprite != null) weaponRenderer.sprite = idleSprite;
        }
        private void UpdateSprite()
        {
            if (weaponRenderer == null || idleSprite == null) return;
            float frame = (Time.time - shotStarted) * firingFramesPerSecond;
            Sprite sprite = firingFrames != null && frame >= 0 && frame < firingFrames.Length
                ? firingFrames[(int)frame] : idleSprite;
            if (sprite != null && weaponRenderer.sprite != sprite) weaponRenderer.sprite = sprite;
        }
        private void Aim(Vector3 point)
        {
            Vector3 direction = point - weapon.position;
            weapon.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }
        private void OnFired(Vector3 impact)
        {
            shotStarted = Time.time;
            Aim(impact);
            Vector3 direction = impact - muzzle.position;
            tracer.transform.position = (impact + muzzle.position) * 0.5f;
            tracer.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            tracer.transform.localScale = new Vector3(direction.magnitude, tracerWidth, 1);
            tracer.enabled = flash.enabled = true;
            visibleUntil = Time.time + flashDuration;
        }
        private void Hide()
        {
            if (flash != null) flash.enabled = false;
            if (tracer != null) tracer.enabled = false;
        }
    }
}
