using System;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Building
{
    /// <summary>Selection and preview state; accepts grid commands independently of input devices.</summary>
    public sealed class PlacementController : MonoBehaviour
    {
        [SerializeField] private BuildManager builder;
        [SerializeField] private SpriteRenderer preview;
        private bool hasCell;
        private bool subscribed;
        public DefenseDefinition Selected { get; private set; }
        public Vector2Int Cell { get; private set; }
        public ShorelineDefense.Grid.BattlefieldGrid Grid => builder?.Battlefield.Grid;
        public PlacementResult Result { get; private set; }
        public bool IsPlacing => Selected != null;
        public bool PreviewVisible => preview != null && preview.gameObject.activeSelf;
        public event Action Changed;
        public void Configure(BuildManager buildManager, SpriteRenderer ghost) { builder = buildManager; preview = ghost; }
        private void Start() => Subscribe();
        private void OnEnable() { if (builder != null && builder.Battlefield.Grid != null) Subscribe(); }
        private void Subscribe()
        {
            if (subscribed) return;
            builder.Economy.MoneyChanged += MoneyChanged;
            builder.Game.StateChanged += StateChanged;
            builder.Battlefield.Grid.OccupancyChanged += Refresh;
            builder.Battlefield.Grid.Changed += Refresh;
            subscribed = true;
            Refresh();
        }
        private void OnDisable()
        {
            Cancel();
            if (!subscribed) return;
            builder.Economy.MoneyChanged -= MoneyChanged;
            builder.Game.StateChanged -= StateChanged;
            builder.Battlefield.Grid.OccupancyChanged -= Refresh;
            builder.Battlefield.Grid.Changed -= Refresh;
            subscribed = false;
        }
        public bool Select(DefenseDefinition definition)
        {
            if (definition == null || !definition.IsValid || !builder.Game.IsPlaying || !builder.Economy.CanAfford(definition.Cost)) return false;
            Selected = definition;
            hasCell = false;
            Refresh();
            return true;
        }
        public void HoverCell(Vector2Int cell)
        {
            if (!IsPlacing || (hasCell && cell == Cell)) return;
            Cell = cell;
            hasCell = true;
            Refresh();
        }
        public void HidePreview()
        {
            if (!hasCell) return;
            hasCell = false;
            Refresh();
        }
        public PlacementResult TryPlaceCurrent(out DefenseBase defense)
        {
            defense = null;
            if (!IsPlacing || !hasCell) return PlacementResult.InvalidDefinition;
            var result = builder.TryPlaceDefense(Selected, Cell, out defense);
            Refresh();
            return result;
        }
        public void Cancel()
        {
            Selected = null;
            hasCell = false;
            Refresh();
        }
        private void MoneyChanged(int money)
        {
            if (IsPlacing && !builder.Economy.CanAfford(Selected.Cost)) Cancel();
            else Refresh();
        }
        private void StateChanged(GameState state) { if (state != GameState.Playing) Cancel(); else Refresh(); }
        private void Refresh()
        {
            bool visible = IsPlacing && hasCell;
            if (preview != null) preview.gameObject.SetActive(visible);
            if (visible)
            {
                Result = builder.Validate(Selected, Cell);
                var grid = builder.Battlefield.Grid;
                preview.transform.position = grid.FootprintCenter(Cell, Selected.Footprint);
                preview.transform.localScale = new Vector3(Selected.Footprint.x * grid.CellSize, Selected.Footprint.y * grid.CellSize, 1);
                preview.color = Result == PlacementResult.Valid ? new Color(0.2f, 1, 0.35f, 0.6f) : new Color(1, 0.15f, 0.15f, 0.6f);
            }
            Changed?.Invoke();
        }
    }
}
