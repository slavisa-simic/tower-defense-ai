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
        public bool IsDismantling { get; private set; }
        public bool IsInteracting => IsPlacing || IsDismantling;
        public DefenseBase DismantleTarget => hasCell && Grid != null && Grid.Contains(Cell) ? Grid.OccupantAt(Cell) as DefenseBase : null;
        public int ExpectedRefund => builder.RefundFor(DismantleTarget);
        private string refundFeedback;
        private float feedbackUntil;
        public string RefundFeedback => Time.unscaledTime < feedbackUntil ? refundFeedback : null;
        public bool SelectDismantle() { if(!builder.Game.IsPlaying)return false;Selected=null;IsDismantling=true;hasCell=false;Refresh();return true; }
        public bool TryDismantleCurrent() { if(!IsDismantling || !hasCell || !builder.TryDismantle(DismantleTarget,out int refund))return false;refundFeedback="REFUND +"+refund+" RSD";feedbackUntil=Time.unscaledTime+1.5f;Refresh();return true; }
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
            IsDismantling = false;
            hasCell = false;
            Refresh();
            return true;
        }
        public void HoverCell(Vector2Int cell)
        {
            if (!IsInteracting || (hasCell && cell == Cell)) return;
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
            Selected = null; IsDismantling = false;
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
            if (IsDismantling) {
                var target=DismantleTarget;bool valid=builder.CanDismantle(target);
                if(preview!=null){preview.gameObject.SetActive(valid);if(valid){preview.transform.position=Grid.FootprintCenter(target.Anchor,target.Definition.Footprint);preview.transform.localScale=new Vector3(target.Definition.Footprint.x*Grid.CellSize,target.Definition.Footprint.y*Grid.CellSize,1);preview.color=new Color(1,.3f,.1f,.55f);}}
                Changed?.Invoke();return;
            }
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
