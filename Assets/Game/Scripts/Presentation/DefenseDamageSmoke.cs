using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    [RequireComponent(typeof(DefenseBase))]
    public sealed class DefenseDamageSmoke : MonoBehaviour
    {
        private DefenseBase defense;
        private VfxService effects;
        private float nextPuff;
        private float flashUntil;
        private SpriteRenderer[] bodies;
        private Color[] colors;
        private void Awake() => defense = GetComponent<DefenseBase>();
        private void OnEnable()
        {
            effects = FindFirstObjectByType<VfxService>(); nextPuff = 0;
            defense.Damaged += Hit; defense.Died += Destroyed;
            bodies = System.Array.FindAll(GetComponentsInChildren<SpriteRenderer>(), r => r.sortingLayerName == "Defenses");
            colors = new Color[bodies.Length];
            for (int i = 0; i < bodies.Length; i++) colors[i] = bodies[i].color;
        }
        private void Hit(int damage)
        {
            flashUntil = Time.time + .12f;
            for (int i = 0; i < bodies.Length; i++) if (bodies[i] != null) bodies[i].color = new Color(1, .55f, .4f, colors[i].a);
            effects?.Play(VfxId.ImpactMetal, transform.position, 0, .7f);
        }
        private void Destroyed() => effects?.Play(VfxId.ExplosionSmall, transform.position, 0, .9f);
        private void RestoreColors() { if (bodies == null) return; for (int i = 0; i < bodies.Length; i++) if (bodies[i] != null) bodies[i].color = colors[i]; }
        private void OnDisable() { defense.Damaged -= Hit; defense.Died -= Destroyed; RestoreColors(); }
        private void Update()
        {
            if (flashUntil > 0 && Time.time >= flashUntil) { flashUntil = 0; RestoreColors(); }
            if (Time.time < nextPuff || effects == null || !effects.CanPlay) return;
            nextPuff = Time.time + 1.2f;
            if (defense.IsAlive && defense.Definition != null && defense.CurrentHealth <= defense.MaxHealth * .35f)
                effects.Play(VfxId.SmokeSmall, transform.position, 0, .7f);
        }
    }
}
