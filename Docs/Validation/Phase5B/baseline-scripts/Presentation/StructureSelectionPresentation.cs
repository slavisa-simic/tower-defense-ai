using ShorelineDefense.Building;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    public sealed class StructureSelectionPresentation : MonoBehaviour
    {
        [SerializeField] private StructureSelection selection;
        private LineRenderer[] corners;
        private Material material;
        public void Configure(StructureSelection controller) => selection = controller;
        private void Awake()
        {
            material = new Material(Shader.Find("Sprites/Default"));
            corners = new LineRenderer[4];
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject("Selection Corner"); go.transform.SetParent(transform, false);
                var line = go.AddComponent<LineRenderer>(); corners[i] = line;
                line.sharedMaterial = material; line.useWorldSpace = true; line.positionCount = 3;
                line.startWidth = line.endWidth = .04f; line.startColor = line.endColor = new Color(.88f, .68f, .31f);
                line.sortingLayerName = "Effects"; line.sortingOrder = 20; line.enabled = false;
            }
        }
        private void OnEnable() { selection.Changed += Refresh; Refresh(); }
        private void OnDisable() { selection.Changed -= Refresh; if (corners != null) foreach (var line in corners) line.enabled = false; }
        private void OnDestroy() { if (material != null) Destroy(material); }
        private void Refresh()
        {
            var d = selection.Selected;
            for (int i = 0; i < 4; i++)
            {
                var line = corners[i]; line.enabled = d != null;
                if (d == null) continue;
                float sx = i % 2 == 0 ? -1 : 1, sy = i < 2 ? -1 : 1;
                Vector3 corner = d.transform.position + new Vector3(sx * d.Definition.Footprint.x * d.Grid.CellSize * .52f, sy * d.Definition.Footprint.y * d.Grid.CellSize * .52f, -.05f);
                line.SetPosition(0, corner + Vector3.right * -sx * .22f); line.SetPosition(1, corner); line.SetPosition(2, corner + Vector3.up * -sy * .22f);
            }
        }
    }
}
