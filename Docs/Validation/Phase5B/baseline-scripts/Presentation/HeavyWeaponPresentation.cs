using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    [RequireComponent(typeof(DefenseBase))]
    public sealed class HeavyWeaponPresentation : MonoBehaviour
    {
        [SerializeField] private Transform weapon;
        [SerializeField] private Transform muzzle;
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private Sprite idle;
        [SerializeField] private Sprite[] firingFrames;
        [SerializeField] private bool rocket;
        private AreaProjectileWeapon area;
        private FlameWeapon flame;
        private DefenseBase defense;
        private VfxService effects;
        private SpriteRenderer cone;
        private float shotTime = float.NegativeInfinity;
        public void Configure(Transform rotation, Transform tip, SpriteRenderer renderer, Sprite rest, Sprite[] frames, bool isRocket)
        { weapon = rotation; muzzle = tip; weaponRenderer = renderer; idle = rest; firingFrames = frames; rocket = isRocket; }
        private void Awake() { defense = GetComponent<DefenseBase>(); area = GetComponent<AreaProjectileWeapon>(); flame = GetComponent<FlameWeapon>(); }
        private void OnEnable()
        {
            effects = FindFirstObjectByType<VfxService>();
            if (area != null) area.Fired += OnFired;
            weapon.rotation = Quaternion.Euler(0, 0, 180);
            if (flame != null && cone == null)
            {
                var go = new GameObject("Flame cone visual"); go.transform.SetParent(muzzle, false);
                cone = go.AddComponent<SpriteRenderer>(); cone.sharedMaterial = effects?.Material;
                cone.sortingLayerName = "Effects"; cone.sortingOrder = 8;
            }
        }
        private void OnDisable() { if (area != null) area.Fired -= OnFired; if (cone != null) cone.enabled = false; }
        private void LateUpdate()
        {
            bool active = defense.IsAlive && defense.Game != null && defense.Game.IsPlaying;
            Vector2 direction = Vector2.left;
            if (active && flame != null && flame.Firing) direction = flame.AimDirection;
            else if (active && area != null && area.HasSolution && Time.time - shotTime < .8f) direction = area.AimPoint - transform.position;
            weapon.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            float frame = (Time.time - shotTime) * 16;
            if (active && flame != null && flame.Firing && firingFrames != null && firingFrames.Length > 0)
                frame = (int)(Time.time * 16) % firingFrames.Length;
            weaponRenderer.sprite = firingFrames != null && frame >= 0 && frame < firingFrames.Length ? firingFrames[(int)frame] : idle;
            if (cone == null) return;
            cone.enabled = active && flame.Firing;
            var sequence = effects?.Catalog.Get(VfxId.FlameCone);
            if (!cone.enabled || sequence?.frames == null || sequence.frames.Length == 0) return;
            cone.sprite = sequence.frames[(int)(Time.time * sequence.fps) % sequence.frames.Length];
            float reach = Mathf.Max(.1f, defense.Range - Vector3.Distance(muzzle.position, transform.position));
            cone.transform.localPosition = new Vector3(reach * .5f, 0, 0);
            var size = cone.sprite.bounds.size;
            cone.transform.localScale = new Vector3(reach / size.x,
                reach * 2 * Mathf.Tan(defense.Definition.HeavyWeapon.coneAngle * .5f * Mathf.Deg2Rad) / size.y, 1);
        }
        private void OnFired(Vector3 point, TimedAreaProjectile projectile)
        {
            shotTime = Time.time;
            if (effects == null) return;
            float angle = Mathf.Atan2(point.y - transform.position.y, point.x - transform.position.x) * Mathf.Rad2Deg;
            weapon.rotation = Quaternion.Euler(0, 0, angle);
            effects.Play(rocket ? VfxId.RocketSmoke : VfxId.MortarMuzzle, muzzle.position, angle, .35f);
            projectile.gameObject.AddComponent<AreaProjectilePresentation>().Configure(effects, rocket, angle);
        }
    }
}
