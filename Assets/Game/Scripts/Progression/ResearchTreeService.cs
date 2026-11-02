using System;

namespace ShorelineDefense.Progression
{
    public enum ResearchNodeStatus { Locked, Available, InsufficientRP, Purchased }
    public sealed class ResearchTreeService
    {
        private readonly PlayerProfileService profile;
        private readonly ResearchCatalog catalog;
        private readonly Func<bool> canPurchase;
        private bool purchasing;
        public ResearchTreeService(PlayerProfileService player, ResearchCatalog definitions, Func<bool> betweenRuns)
        { profile = player; catalog = definitions; canPurchase = betweenRuns; }
        public ResearchNodeStatus Status(ResearchNodeDefinition n)
        {
            if (profile.Rank(n.id) >= n.maxRank) return ResearchNodeStatus.Purchased;
            foreach (var required in n.prerequisites) if (required == null || profile.Rank(required.id) < 1) return ResearchNodeStatus.Locked;
            return profile.ResearchPoints >= n.costRP ? ResearchNodeStatus.Available : ResearchNodeStatus.InsufficientRP;
        }
        public bool TryPurchase(string id, out string error)
        {
            error = null;
            if (purchasing || !canPurchase()) { error = "Research applies to new runs. Return to the Main Menu to purchase."; return false; }
            var node = catalog?.Find(id);
            if (node == null || Status(node) != ResearchNodeStatus.Available) { error = "Research unavailable: check RP and prerequisites."; return false; }
            purchasing = true;
            try { return profile.Purchase(node, out error); }
            finally { purchasing = false; }
        }
    }
}
