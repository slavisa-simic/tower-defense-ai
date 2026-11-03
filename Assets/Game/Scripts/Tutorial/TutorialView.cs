using System;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Persistence;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.Tutorial
{
    /// <summary>One UI Toolkit card and temporary outlines. Never handles placement or advances simulation.</summary>
    public sealed class TutorialView : IDisposable
    {
        public static bool ConsumesSpace { get; private set; }
        private readonly VisualElement root, panel, actions, frame;
        private readonly Label title, copy, detail;
        private readonly TutorialManager manager;
        private readonly ProductMenuController menu;
        private readonly Button skip;
        private TutorialPrompt shown;
        private float elapsed, suspended;
        private VisualElement highlighted;
        public Rect Bounds => panel.worldBound;
        public bool Visible => panel.resolvedStyle.display != DisplayStyle.None && shown != null;
        public TutorialView(VisualElement document, TutorialManager queue, ProductMenuController menus)
        {
            root = document; manager = queue; menu = menus;
            panel = new VisualElement { name = "tutorial-panel" }; panel.AddToClassList("tutorial-panel");
            title = new Label { name = "tutorial-title", pickingMode = PickingMode.Ignore }; title.AddToClassList("tutorial-title"); panel.Add(title);
            copy = new Label { name = "tutorial-copy", pickingMode = PickingMode.Ignore }; copy.AddToClassList("tutorial-copy"); panel.Add(copy);
            detail = new Label { name = "tutorial-detail", pickingMode = PickingMode.Ignore }; detail.AddToClassList("tutorial-detail"); panel.Add(detail);
            actions = new VisualElement(); actions.AddToClassList("tutorial-actions"); panel.Add(actions);
            var dismiss = new Button(Dismiss) { name = "tutorial-continue", text = "CONTINUE  [ENTER]" }; dismiss.AddToClassList("management-button"); actions.Add(dismiss);
            skip = new Button(Skip) { name = "tutorial-skip", text = "SKIP TUTORIAL" }; skip.AddToClassList("management-button"); actions.Add(skip);
            // Consumes clicks only inside the card. Clicking the world keeps normal placement behavior.
            panel.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());
            frame = new VisualElement { name = "tutorial-world-highlight", pickingMode = PickingMode.Ignore }; frame.AddToClassList("tutorial-world-highlight"); root.Add(frame);
            root.Add(panel); Hide();
        }
        public void Tick(float delta, Func<TutorialPrompt, bool> eligible)
        {
            if (suspended > 0) { suspended -= delta; return; }
            var confirmation = panel.Q("tutorial-skip-confirm");
            if (confirmation != null) {
                if (LocalGameData.Tutorial.Enabled && manager.Active != null && eligible(manager.Active)) { elapsed = 0; return; }
                confirmation.RemoveFromHierarchy(); actions.style.display = DisplayStyle.Flex;
            }
            var next = manager.Pump(eligible);
            if (next != shown) { shown = next; elapsed = 0; ClearHighlight(); }
            if (next == null) { Hide(); return; }
            bool inMenu = menu.ModalOpen;
            var host = inMenu ? root.Q("product-content") : root;
            if (panel.parent != host) { panel.RemoveFromHierarchy(); if (inMenu) host.Insert(Math.Min(1, host.childCount), panel); else host.Add(panel); }
            panel.EnableInClassList("tutorial-inline", inMenu);
            panel.style.display = DisplayStyle.Flex;
            if (!inMenu) panel.BringToFront();
            title.text = next.Definition.Title; copy.text = next.Definition.Body;
            detail.text = TutorialCopy.Detail(next); detail.style.display = detail.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            skip.style.display = !LocalGameData.Tutorial.BasicsCompleted ? DisplayStyle.Flex : DisplayStyle.None;
            elapsed += delta;
            Highlight(next);
            ConsumesSpace = true;
            // Leave Space for ordinary pause while a modal menu is already in use.
            if (elapsed > .25f && (Input.GetKeyDown(KeyCode.Return) || !inMenu && Input.GetKeyDown(KeyCode.Space))) Dismiss();
            // Inline cards change menu layout. Keep them stable until an explicit dismissal or navigation.
            else if (!inMenu && next.Definition.DismissRule == TutorialDismissRule.Timed && elapsed >= next.Definition.Duration) Dismiss();
        }
        public void Dismiss() { manager.Dismiss(); shown = null; Hide(); suspended = .35f; }
        private void Skip()
        {
            // A small inline confirmation preserves the current run and the player's speed/pause choice.
            copy.text = "Disable tutorial guidance for this profile?"; detail.style.display = DisplayStyle.None;
            actions.style.display = DisplayStyle.None;
            var confirm = new VisualElement { name = "tutorial-skip-confirm" }; confirm.AddToClassList("tutorial-actions");
            var yes = new Button(() => {
                if (LocalGameData.Tutorial.SetEnabled(false)) { confirm.RemoveFromHierarchy(); actions.style.display = DisplayStyle.Flex; shown = null; Hide(); }
                else { detail.text = LocalGameData.Tutorial.Error; detail.style.display = DisplayStyle.Flex; }
            }) { name = "tutorial-skip-yes", text = "YES / DISABLE" };
            var no = new Button(() => { confirm.RemoveFromHierarchy(); actions.style.display = DisplayStyle.Flex; }) { name = "tutorial-skip-no", text = "NO / KEEP GUIDANCE" };
            yes.AddToClassList("management-button"); no.AddToClassList("management-button"); confirm.Add(no); confirm.Add(yes); panel.Add(confirm);
        }
        private void Highlight(TutorialPrompt prompt)
        {
            bool confirming = panel.Q("tutorial-skip-confirm") != null;
            // Confirmation uses the same card; it must not expire while a choice is pending.
            if (confirming) { elapsed = 0; copy.text = "Disable tutorial guidance for this profile?"; }
            var target = prompt.Definition.HighlightTarget == null ? null : root.Q(prompt.Definition.HighlightTarget);
            if (target != highlighted) { ClearHighlight(); highlighted = target; }
            if (highlighted != null) {
                highlighted.AddToClassList("tutorial-highlight"); highlighted.EnableInClassList("tutorial-pulse", elapsed % 1.2f < .6f);
            }
            Component world = prompt.Subject as DefenseBase;
            if (world == null && prompt.Subject is EnemyController enemy && enemy.IsCombatActive) world = enemy;
            frame.style.display = DisplayStyle.None;
            if (world == null || !world.gameObject.activeInHierarchy || menu.ModalOpen || Camera.main == null) return;
            var at = RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, world.transform.position, Camera.main);
            float size = prompt.Subject is DefenseBase d ? Mathf.Max(30, d.Definition.Footprint.x * 18) : 32;
            frame.style.left = at.x - size / 2; frame.style.top = at.y - size / 2; frame.style.width = size; frame.style.height = size;
            frame.style.display = DisplayStyle.Flex;
        }
        private void ClearHighlight()
        {
            highlighted?.RemoveFromClassList("tutorial-highlight"); highlighted?.RemoveFromClassList("tutorial-pulse"); highlighted = null;
            frame.style.display = DisplayStyle.None;
        }
        private void Hide() { panel.style.display = DisplayStyle.None; ConsumesSpace = false; ClearHighlight(); }
        public void Dispose() { Hide(); panel.RemoveFromHierarchy(); frame.RemoveFromHierarchy(); }
    }

    internal static class TutorialCopy
    {
        public static string Detail(TutorialPrompt prompt)
        {
            if (prompt.Definition.Id == "upgrade" && prompt.Subject is DefenseBase d && d.IsAlive && d.Level < 3)
                return $"HP {d.MaxHealth} → {d.Meta.MaxHealth(d.Definition, d.Level + 1)}  /  {d.Owner.UpgradeCost(d):N0} RSD" +
                    (d.Definition.Id == "barbed-wire" ? $"\nSlow {(1 - d.MovementMultiplier) * 100:0}% → {(1 - d.Definition.MovementMultiplier * (d.Definition.LevelData(d.Level + 1)?.slow ?? 1)) * 100:0}%" : $"\nDamage {d.Damage} → {d.DamageAtLevel(d.Level + 1)}");
            if (prompt.Definition.Id == "dismantle" && prompt.Subject is DefenseBase refund && refund.IsAlive) return $"ACTUAL REFUND  +{refund.Owner.RefundFor(refund):N0} RSD";
            if (prompt.Definition.Id == "repair" && prompt.Subject is DefenseBase repair && repair.IsAlive) return $"HP {repair.CurrentHealth} / {repair.MaxHealth}  /  REPAIR {repair.Owner.RepairCost(repair):N0} RSD";
            if (prompt.Subject is EndlessEventDefinition e) return e.displayName.ToUpperInvariant() + "\n" + e.description;
            if (prompt.Subject is BossDefinition boss) return boss.DisplayName.ToUpperInvariant();
            if (prompt.Subject is ShorelineDefense.Progression.RunAwardReceipt award) return $"+{award.Total} RP / PERSISTS BETWEEN RUNS";
            return "";
        }
    }
}
