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
        private void Awake() => defense = GetComponent<DefenseBase>();
        private void OnEnable() { effects = FindFirstObjectByType<VfxService>(); nextPuff = 0; }
        private void Update()
        {
            if (Time.time < nextPuff || effects == null || !effects.CanPlay) return;
            nextPuff = Time.time + 1.2f;
            if (defense.IsAlive && defense.Definition != null && defense.CurrentHealth <= defense.Definition.MaxHealth * .35f)
                effects.Play(VfxId.SmokeSmall, transform.position, 0, .7f);
        }
    }
}
