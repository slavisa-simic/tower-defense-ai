using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    [RequireComponent(typeof(LandMine))]
    public sealed class MineExplosionPresentation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer flash;
        private LandMine mine;
        public void Configure(SpriteRenderer renderer) => flash = renderer;
        private void Awake() => mine = GetComponent<LandMine>();
        private void OnEnable() { mine.Detonated += Show; }
        private void OnDisable() { if (mine != null) mine.Detonated -= Show; }
        private void Show(float radius)
        {
            if (flash == null) return;
            flash.transform.SetParent(null, true);
            flash.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
            flash.enabled = true;
            Destroy(flash.gameObject, 0.22f);
        }
    }
}
