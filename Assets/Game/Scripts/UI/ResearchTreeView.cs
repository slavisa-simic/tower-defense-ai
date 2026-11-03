using System;
using System.Collections.Generic;
using System.Linq;
using ShorelineDefense.Progression;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.UI
{
    /// <summary>Presentation-only board. The service validates every transaction and saves immediately.</summary>
    public sealed class ResearchTreeView : VisualElement
    {
        private readonly ResearchCatalog catalog;
        private readonly PlayerProfileService profile;
        private readonly ResearchTreeService tree;
        private readonly Label balance, detailTitle, detailCopy, result;
        private readonly Image detailIcon;
        private readonly Button purchase;
        private readonly Dictionary<ResearchNodeDefinition, Button> buttons = new Dictionary<ResearchNodeDefinition, Button>();
        private readonly List<ResearchBranchBoard> boards = new List<ResearchBranchBoard>();
        private ResearchNodeDefinition selected;
        private readonly Action onPurchased;
        public ResearchTreeView(ResearchCatalog definitions, PlayerProfileService player, Func<bool> betweenRuns, Action purchased = null)
        {
            onPurchased = purchased;
            catalog = definitions; profile = player; tree = new ResearchTreeService(player, definitions, betweenRuns);
            name = "research-tree";
            var header = new VisualElement(); header.AddToClassList("research-balance-row"); Add(header);
            if (catalog?.researchPointIcon != null) { var icon = new Image { sprite = catalog.researchPointIcon }; icon.AddToClassList("research-rp-icon"); header.Add(icon); }
            balance = new Label { name = "research-balance" }; balance.AddToClassList("research-balance"); header.Add(balance);
            var note = new Label("Applies to new runs. Continue keeps its original research."); note.AddToClassList("research-note"); Add(note);
            var body = new VisualElement(); body.AddToClassList("research-body"); Add(body);
            var branches = new VisualElement { name = "research-branches" }; branches.AddToClassList("research-branches"); body.Add(branches);
            foreach (ResearchBranch branch in Enum.GetValues(typeof(ResearchBranch)))
            {
                var column = new VisualElement(); column.AddToClassList("research-column"); branches.Add(column);
                var title = new Label(branch == ResearchBranch.Weapons ? "WEAPONS" : branch.ToString().ToUpperInvariant()); title.AddToClassList("research-branch-title"); column.Add(title);
                title.tooltip = ShorelineDefense.Tutorial.TutorialCatalog.BranchHint(branch.ToString());
                var board = new ResearchBranchBoard(definitions.nodes.Where(n => n.branch == branch).ToArray(), n => tree.Status(n));
                boards.Add(board); column.Add(board);
                foreach (var node in board.Nodes)
                {
                    var n = node;
                    var button = new Button(() => Select(n)) { name = "research-node-" + n.id };
                    button.AddToClassList("research-node");
                    button.style.position = Position.Absolute; button.style.width = 86; button.style.height = 86;
                    button.style.left = new Length(n.boardPosition.x * 100, LengthUnit.Percent); button.style.marginLeft = -43;
                    button.style.top = n.boardPosition.y;
                    var icon = new Image { sprite = n.icon, pickingMode = PickingMode.Ignore }; icon.AddToClassList("research-node-icon"); button.Add(icon);
                    var label = new Label(n.displayName.ToUpperInvariant()) { pickingMode = PickingMode.Ignore }; label.AddToClassList("research-node-name"); button.Add(label);
                    var state = new Label { name = "node-state", pickingMode = PickingMode.Ignore }; state.AddToClassList("research-node-state"); button.Add(state);
                    button.tooltip = n.displayName.ToUpperInvariant() + "\n" + n.description + "\n" + n.costRP + " RP\nRequires: " + Requirements(n);
                    board.Add(button); buttons[n] = button;
                }
            }
            var detail = new VisualElement { name = "research-detail" }; detail.AddToClassList("research-detail"); body.Add(detail);
            detailIcon = new Image(); detailIcon.AddToClassList("research-detail-icon"); detail.Add(detailIcon);
            detailTitle = new Label(); detailTitle.AddToClassList("research-detail-title"); detail.Add(detailTitle);
            detailCopy = new Label(); detailCopy.AddToClassList("research-detail-copy"); detail.Add(detailCopy);
            purchase = new Button(Purchase) { name = "research-purchase" }; purchase.AddToClassList("product-button"); detail.Add(purchase);
            result = new Label { name = "research-result" }; result.AddToClassList("research-detail-copy"); detail.Add(result);
            selected = definitions.nodes.FirstOrDefault(); Refresh();
        }
        private static string Requirements(ResearchNodeDefinition node) => node.prerequisites.Length == 0 ? "None" : string.Join(" + ", node.prerequisites.Select(n => n.displayName));
        private void Select(ResearchNodeDefinition node) { selected = node; result.text = ""; Refresh(); }
        private void Purchase()
        {
            if (selected == null) return;
            string name = selected.displayName;
            bool ok = tree.TryPurchase(selected.id, out string error);
            Refresh(); result.text = ok ? "RESEARCH COMPLETE\n" + name + "\nApplies to new runs." : error;
            if (ok) onPurchased?.Invoke();
        }
        public void Refresh()
        {
            balance.text = $"AVAILABLE  {profile.ResearchPoints:N0} RP";
            foreach (var pair in buttons)
            {
                var state = tree.Status(pair.Key); var b = pair.Value;
                b.EnableInClassList("node-locked", state == ResearchNodeStatus.Locked);
                b.EnableInClassList("node-purchased", state == ResearchNodeStatus.Purchased);
                b.EnableInClassList("node-insufficient", state == ResearchNodeStatus.InsufficientRP);
                b.EnableInClassList("node-selected", pair.Key == selected);
                b.Q<Label>("node-state").text = state == ResearchNodeStatus.Purchased ? "[OK] RESEARCHED" : state == ResearchNodeStatus.Locked ? "[X] LOCKED" : state == ResearchNodeStatus.InsufficientRP ? pair.Key.costRP + " RP / NEED RP" : pair.Key.costRP + " RP / READY";
            }
            foreach (var b in boards) b.MarkDirtyRepaint();
            if (selected == null) return;
            var status = tree.Status(selected);
            detailIcon.sprite = selected.icon; detailTitle.text = selected.displayName.ToUpperInvariant();
            detailCopy.text = selected.branch.ToString().ToUpperInvariant() + " / TIER " + selected.tier + "\n\nPERMANENT EFFECT\n" + selected.description + "\n\nCOST  " + selected.costRP + " RP\n\nREQUIRES\n" + Requirements(selected) + "\n\n" +
                (status == ResearchNodeStatus.Locked ? "Missing prerequisite research." : status == ResearchNodeStatus.InsufficientRP ? "Insufficient RP. Complete runs to earn research." : status == ResearchNodeStatus.Purchased ? "Permanently researched." : "Available for research.");
            purchase.text = status == ResearchNodeStatus.Purchased ? "RESEARCHED" : "RESEARCH / " + selected.costRP + " RP";
            purchase.SetEnabled(profile.Available && status == ResearchNodeStatus.Available);
            if (profile.Error != null) result.text = profile.Error;
        }
    }
    internal sealed class ResearchBranchBoard : VisualElement
    {
        public ResearchNodeDefinition[] Nodes { get; }
        public ResearchBranchBoard(ResearchNodeDefinition[] nodes, Func<ResearchNodeDefinition, ResearchNodeStatus> status)
        {
            Nodes = nodes; AddToClassList("research-branch-board");
            generateVisualContent += context => {
                var painter = context.painter2D; float w = contentRect.width;
                // Low-contrast technical grid, then dependency elbows. Node children render above it.
                painter.lineWidth = 1; painter.strokeColor = new Color(.15f, .25f, .26f, .45f);
                for (float y = 0; y < contentRect.height; y += 20) { painter.BeginPath(); painter.MoveTo(new Vector2(0, y)); painter.LineTo(new Vector2(w, y)); painter.Stroke(); }
                foreach (var node in nodes) foreach (var parent in node.prerequisites)
                {
                    if (!nodes.Contains(parent)) continue;
                    var from = new Vector2(parent.boardPosition.x * w, parent.boardPosition.y + 86);
                    var to = new Vector2(node.boardPosition.x * w, node.boardPosition.y);
                    float mid = (from.y + to.y) * .5f;
                    painter.lineWidth = 2; painter.strokeColor = status(parent) == ResearchNodeStatus.Purchased ? new Color(.48f, .75f, .7f) : new Color(.28f, .4f, .42f);
                    painter.BeginPath(); painter.MoveTo(from); painter.LineTo(new Vector2(from.x, mid)); painter.LineTo(new Vector2(to.x, mid)); painter.LineTo(to); painter.Stroke();
                }
            };
        }
    }
}
