using ShorelineDefense.Grid;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.Building
{
    public sealed class PlacementInput : MonoBehaviour
    {
        [SerializeField] private Camera view;
        [SerializeField] private GridManager grid;
        [SerializeField] private PlacementController placement;
        [SerializeField] private UIDocument hud;
        [SerializeField] private StructureSelection selection;
        public void ConfigureSelection(StructureSelection controller) => selection = controller;
        public void Configure(Camera camera, GridManager battlefield, PlacementController controller, UIDocument document)
        { view = camera; grid = battlefield; placement = controller; hud = document; }
        private void Update()
        {
            var menus = hud != null ? hud.GetComponent<ShorelineDefense.UI.ProductMenuController>() : null;
            if (menus != null && menus.ModalOpen) { placement.HidePreview(); return; }
            // ProductMenuController owns Escape priority; legacy scenes retain cancellation.
            if ((menus == null && Input.GetKeyDown(KeyCode.Escape)) || Input.GetMouseButtonDown(1)) { placement.Cancel(); selection?.Clear(); return; }
            if (IsOverHud(Input.mousePosition) || !TryScreenToCell(Input.mousePosition, out var cell))
            { placement.HidePreview(); return; }
            if (!placement.IsInteracting) { if (Input.GetMouseButtonDown(0)) selection?.SelectCell(cell); return; }
            placement.HoverCell(cell);
            if (Input.GetMouseButtonDown(0)) { if(placement.IsDismantling)placement.TryDismantleCurrent();else placement.TryPlaceCurrent(out _); }
        }
        public bool TryScreenToCell(Vector2 screen, out Vector2Int cell)
        {
            cell = default;
            if (view == null || grid.Grid == null || !view.pixelRect.Contains(screen)) return false;
            var ray = view.ScreenPointToRay(screen);
            var plane = new Plane(Vector3.forward, grid.Grid.Origin);
            if (!plane.Raycast(ray, out float distance)) return false;
            // Out-of-grid cells are returned too, so the preview remains red at map edges.
            grid.Grid.TryWorldToCell(ray.GetPoint(distance), out cell);
            return true;
        }
        public bool IsOverHud(Vector2 screen)
        {
            var root = hud != null ? hud.rootVisualElement : null;
            if (root?.panel == null) return false;
            Vector2 point = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(screen.x, Screen.height - screen.y));
            var picked = root.panel.Pick(point);
            return picked != null && picked != root;
        }
    }
}
