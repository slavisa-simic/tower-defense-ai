using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.UI
{
    /// <summary>UI Toolkit's editor tooltip property is rendered explicitly for the Player.</summary>
    public sealed class RuntimeTooltips : MonoBehaviour
    {
        private VisualElement root, target;
        private Label tooltip;
        private void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            tooltip = new Label { name = "runtime-tooltip", pickingMode = PickingMode.Ignore };
            tooltip.AddToClassList("runtime-tooltip"); tooltip.style.display = DisplayStyle.None; root.Add(tooltip);
            root.RegisterCallback<PointerOverEvent>(Over); root.RegisterCallback<PointerOutEvent>(Out);
        }
        private void Over(PointerOverEvent e)
        {
            target = e.target as VisualElement;
            while (target != null && string.IsNullOrEmpty(target.tooltip)) target = target.parent;
            if (target == null) return;
            tooltip.text = target.tooltip; tooltip.style.display = DisplayStyle.Flex; tooltip.BringToFront();
            float width = root.resolvedStyle.width, height = root.resolvedStyle.height;
            tooltip.style.left = Mathf.Clamp(target.worldBound.xMin, 12, Mathf.Max(12, width - 370));
            tooltip.style.top = Mathf.Clamp(target.worldBound.yMin - 160, 12, Mathf.Max(12, height - 170));
        }
        private void Out(PointerOutEvent e) { target = null; tooltip.style.display = DisplayStyle.None; }
        private void LateUpdate() { if (target != null && tooltip != null) tooltip.text = target.tooltip; }
        private void OnDestroy() { root?.UnregisterCallback<PointerOverEvent>(Over); root?.UnregisterCallback<PointerOutEvent>(Out); tooltip?.RemoveFromHierarchy(); }
    }
}
