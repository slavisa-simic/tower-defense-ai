using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    [RequireComponent(typeof(DefenseBase), typeof(BarbedWire))]
    public sealed class ConnectedWirePresentation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private SpriteRenderer shadow;
        [SerializeField] private Sprite[] variants = new Sprite[16];
        private DefenseBase defense;
        private bool connected;
        public int Mask { get; private set; }
        public Sprite SpriteFor(int mask) => variants != null && mask >= 0 && mask < variants.Length ? variants[mask] : null;
        public void Configure(SpriteRenderer renderer, Sprite[] sprites) { body = renderer; variants = sprites; }
        public void ConfigureShadow(SpriteRenderer renderer) => shadow = renderer;
        private void Awake() => defense = GetComponent<DefenseBase>();
        private void OnEnable()
        {
            if (defense.Grid == null) return;
            connected = true;
            defense.Died += Disconnect;
            Refresh(); RefreshNeighbors();
        }
        private void OnDisable() { if (defense != null) defense.Died -= Disconnect; Disconnect(); }
        private void Disconnect()
        {
            if (!connected) return;
            connected = false;
            RefreshNeighbors();
        }
        public void Refresh()
        {
            if (!connected || body == null) return;
            Mask = WireConnectivity.At(defense.Grid, defense.Anchor);
            var sprite = SpriteFor(Mask);
            if (sprite != null) body.sprite = sprite;
            if (shadow != null) shadow.sprite = sprite;
        }
        private void RefreshNeighbors()
        {
            if (defense.Grid == null) return;
            foreach (var direction in WireConnectivity.Directions)
            {
                var cell = defense.Anchor + direction;
                if (defense.Grid.Contains(cell) && defense.Grid.OccupantAt(cell) is DefenseBase neighbor && neighbor != null)
                    neighbor.GetComponent<ConnectedWirePresentation>()?.Refresh();
            }
        }
    }
}
