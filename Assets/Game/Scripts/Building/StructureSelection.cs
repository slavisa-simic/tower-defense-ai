using System;
using ShorelineDefense.Defenses;
using ShorelineDefense.Core;
using UnityEngine;

namespace ShorelineDefense.Building
{
    public sealed class StructureSelection : MonoBehaviour
    {
        [SerializeField] private BuildManager builder;
        [SerializeField] private PlacementController placement;
        public DefenseBase Selected { get; private set; }
        public event Action Changed;
        public void Configure(BuildManager manager, PlacementController controller) { builder = manager; placement = controller; }
        private void OnEnable() { placement.Changed += ModeChanged; builder.Game.StateChanged += StateChanged; }
        private void OnDisable() { placement.Changed -= ModeChanged; builder.Game.StateChanged -= StateChanged; Clear(); }
        private void ModeChanged() { if (placement.IsInteracting) Clear(); }
        private void StateChanged(GameState state) { if (state != GameState.Playing) Clear(); }
        public bool SelectCell(Vector2Int cell)
        {
            if (placement.IsInteracting) return false;
            return Select(builder.Battlefield.Grid.Contains(cell) ? builder.Battlefield.Grid.OccupantAt(cell) as DefenseBase : null);
        }
        public bool Select(DefenseBase defense)
        {
            if (placement.IsInteracting) return false;
            Clear();
            if (!builder.CanDismantle(defense)) return false;
            Selected = defense;
            Selected.Changed += SelectionChanged;
            Changed?.Invoke();
            return true;
        }
        public void Clear()
        {
            if (ReferenceEquals(Selected, null)) return;
            Selected.Changed -= SelectionChanged;
            Selected = null;
            Changed?.Invoke();
        }
        private void SelectionChanged()
        {
            if (!builder.CanDismantle(Selected)) Clear();
            else Changed?.Invoke();
        }
    }
}
