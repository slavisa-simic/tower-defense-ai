using UnityEngine;

namespace ShorelineDefense.Defenses
{
    /// <summary>Walkable grid overlay. Enemies sample it locally; wires never scan the scene.</summary>
    [RequireComponent(typeof(DefenseBase))]
    public sealed class BarbedWire : MonoBehaviour
    {
        private DefenseBase defense;
        private int appliedLevel;
        private void Awake() => defense = GetComponent<DefenseBase>();
        private void OnEnable()
        {
            if (defense.Definition == null) return;
            defense.Died += Release;
            defense.Changed += Refresh;
            Refresh();
        }
        private void Refresh()
        {
            if (!defense.IsAlive || appliedLevel == defense.Level) return;
            appliedLevel = defense.Level;
            defense.Grid.TrySetTraversalModifier(defense.Anchor, defense.Definition.Footprint, defense,
                defense.Definition.TraversalMultiplier, defense.MovementMultiplier);
        }
        private void Release() => defense.Grid?.ClearTraversalModifier(defense.Anchor, defense.Definition.Footprint, defense);
        private void OnDisable()
        {
            if (defense == null || defense.Definition == null) return;
            defense.Died -= Release;
            defense.Changed -= Refresh;
            Release();
            appliedLevel = 0;
        }
    }
}
