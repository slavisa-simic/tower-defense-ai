using System.Collections.Generic;

namespace ShorelineDefense.Tutorial
{
    public enum TutorialCondition { RunStarted, Preparation, DefenseSelected, DefensePlaced, CreatureEncountered, StructureDamaged, StructureAttacked, StructureDestroyed, EventQueued, BossStarted, Breach, RunCompleted, MainMenu, ResearchVisited, ResearchPurchased, MapSelect }
    public enum TutorialContext { Gameplay, Preparation, Placement, SelectedStructure, DamagedStructure, UpgradeAvailable, DismantleAvailable, Summary, MainMenu, Research, MapSelect }
    public enum TutorialDismissRule { Continue, Timed }
    public enum TutorialPauseBehavior { KeepRunning }

    public sealed class TutorialDefinition
    {
        public string Id { get; }
        public string Title { get; }
        public string Body { get; }
        public TutorialCondition Trigger { get; }
        public int Priority { get; }
        public string HighlightTarget { get; }
        public TutorialContext MinimumContext { get; }
        public TutorialDismissRule DismissRule { get; }
        public TutorialPauseBehavior PauseBehavior => TutorialPauseBehavior.KeepRunning;
        public float Duration => 9;
        public TutorialDefinition(string id, string title, string body, TutorialCondition trigger, int priority, string target, TutorialContext context, TutorialDismissRule dismiss = TutorialDismissRule.Timed)
        { Id = id; Title = title; Body = body; Trigger = trigger; Priority = priority; HighlightTarget = target; MinimumContext = context; DismissRule = dismiss; }
    }

    /// <summary>Central text layer and stable localization keys. No tutorial copy in simulation code.</summary>
    public static class TutorialCatalog
    {
        private static TutorialDefinition D(string id, string title, string body, TutorialCondition trigger, int priority = 40, string target = null, TutorialContext context = TutorialContext.Gameplay, TutorialDismissRule dismiss = TutorialDismissRule.Timed) =>
            new TutorialDefinition(id, title, body, trigger, priority, target, context, dismiss);
        public static readonly TutorialDefinition[] All = {
            D("intro", "QUARANTINE LINE", "Aquatic organisms are emerging from the Danube. Stop them before they cross the inland containment line.", TutorialCondition.RunStarted, 120, null, TutorialContext.Preparation, TutorialDismissRule.Continue),
            D("objective", "HOLD THE QUARANTINE LINE", "If organisms cross the quarantine line, civilian casualties increase. Keep HOMICIDES below the limit.", TutorialCondition.RunStarted, 119, "health-panel"),
            D("rsd", "RSD / FIELD BUDGET", "RSD is used to build, upgrade and repair defenses.", TutorialCondition.RunStarted, 118, "money-panel"),
            D("build", "BUILD YOUR FIRST DEFENSE", "Select a Machine Gun Nest, then place it on land near the river. Spread defenses across the approaches.", TutorialCondition.Preparation, 110, "defense-buttons", TutorialContext.Preparation),
            D("placement", "GREEN = VALID / RED = BLOCKED", "Structures snap to the defense grid. Choose a green footprint to build; right click cancels placement.", TutorialCondition.DefenseSelected, 109, "build-hint", TutorialContext.Placement),
            D("defense-machine-gun-nest", "MACHINE GUN NEST", "Sustained rapid fire is best against Crawlers, Skitters and Broodlings.", TutorialCondition.DefensePlaced, 90, "defense-buttons"),
            D("start-wave", "START WAVE", "Begin the next assault when ready. You may keep building during combat.", TutorialCondition.DefensePlaced, 89, "start-wave", TutorialContext.Preparation),
            D("emergence", "BIOLOGICAL ACTIVITY DETECTED", "Creatures are emerging from the river. Stop them before they reach the inland line.", TutorialCondition.CreatureEncountered, 75),
            D("creature-Crawler", "CRAWLER", "Standard swarm organism. Sustained fire holds back the swarm.", TutorialCondition.CreatureEncountered),
            D("creature-Skitter", "SKITTER", "Fast and fragile. Use slowing defenses and rapid fire.", TutorialCondition.CreatureEncountered, 45, "defense-buttons"),
            D("creature-Bulwark", "BULWARK / ARMORED", "Heavily armored. Heavy weapons and Snipers are more effective.", TutorialCondition.CreatureEncountered, 50),
            D("creature-Spitter", "RANGED ORGANISM DETECTED", "Spitters can damage defensive structures from range. Watch your structures' HP.", TutorialCondition.CreatureEncountered, 85),
            D("creature-Burrower", "SUBSURFACE MOVEMENT DETECTED", "Burrowers can bypass part of the first line underground. Watch the moving-ground trail; they must surface.", TutorialCondition.CreatureEncountered, 70),
            D("creature-Broodling", "BROODLING SWARM", "Individually weak; dangerous in large numbers. Area damage is effective.", TutorialCondition.CreatureEncountered, 50),
            D("blockade", "ROUTE BLOCKED", "Creatures will attack defensive structures if no open route remains.", TutorialCondition.DefensePlaced, 130),
            D("structure-attack", "DEFENSE UNDER ATTACK", "If all routes are blocked, creatures destroy defenses to reopen a path.", TutorialCondition.StructureAttacked, 140),
            D("damage", "STRUCTURE HP", "Structures can be damaged and destroyed. Select a damaged defense to inspect its HP and repair controls.", TutorialCondition.StructureDamaged, 84, "structure-stats"),
            D("repair", "REPAIR", "Repairing during Preparation is cheaper than during an active Wave. Select REPAIR to restore full HP.", TutorialCondition.StructureDamaged, 105, "structure-repair", TutorialContext.DamagedStructure),
            D("upgrade", "LEVEL II / LEVEL III", "Defenses can be upgraded to Level II and Level III. The preview below shows the next level.", TutorialCondition.Preparation, 100, "structure-upgrade", TutorialContext.UpgradeAvailable),
            D("dismantle", "DISMANTLE", "Remove a structure to recover part of the invested RSD. The refund is shown below; dismantling is optional.", TutorialCondition.Preparation, 35, "structure-dismantle", TutorialContext.DismantleAvailable),
            D("destroyed", "DEFENSE DESTROYED", "Destroyed defenses provide no refund. Repair damaged positions before they collapse.", TutorialCondition.StructureDestroyed, 86),
            D("defense-barbed-wire", "BARBED WIRE", "Slows creatures without fully blocking their path. Connected sections automatically join.", TutorialCondition.DefenseSelected, 65, "build-hint", TutorialContext.Placement),
            D("defense-land-mine", "LAND MINE", "Creatures can walk over mines. The mine detonates when triggered.", TutorialCondition.DefenseSelected, 65, "build-hint", TutorialContext.Placement),
            D("defense-sniper-tower", "SNIPER TOWER", "Long-range precision fire. Useful against armored, high-value targets.", TutorialCondition.DefenseSelected, 65, "build-hint", TutorialContext.Placement),
            D("defense-mortar", "MORTAR", "Long-range area damage. Cannot attack targets inside its minimum range.", TutorialCondition.DefenseSelected, 65, "build-hint", TutorialContext.Placement),
            D("defense-flamethrower", "FLAMETHROWER", "Short-range crowd control. Works especially well when enemies are slowed.", TutorialCondition.DefenseSelected, 65, "build-hint", TutorialContext.Placement),
            D("defense-autocannon", "AUTO CANNON", "Heavy sustained fire. Effective against armored creatures.", TutorialCondition.DefenseSelected, 65, "build-hint", TutorialContext.Placement),
            D("defense-rocket", "ROCKET LAUNCHER", "Expensive heavy area damage. Best against dense groups and high-value targets.", TutorialCondition.DefenseSelected, 65, "build-hint", TutorialContext.Placement),
            D("event", "UPCOMING EVENT", "Some waves have temporary modifiers. The upcoming effect is shown in the Preparation event panel.", TutorialCondition.EventQueued, 80, "event-panel", TutorialContext.Preparation),
            D("fog", "FOG", "Defense range is temporarily reduced. The river fog marks the active effect.", TutorialCondition.EventQueued, 78, "event-panel"),
            D("flood", "FLOODED AREA", "Some cells are temporarily unavailable for new construction. Existing defenses remain.", TutorialCondition.EventQueued, 78, "event-panel"),
            D("boss", "MASSIVE BIOLOGICAL SIGNATURE DETECTED", "Boss organisms have their own health bar and large breach penalties.", TutorialCondition.BossStarted, 160),
            D("creature-Dredger", "THE DREDGER", "Extremely durable armored organism. Heavy weapons recommended.", TutorialCondition.BossStarted, 150),
            D("creature-Broodmother", "THE BROODMOTHER", "Produces additional Broodlings during the assault. Control the swarm while damaging the boss.", TutorialCondition.BossStarted, 150),
            D("speed", "PAUSE / x1 / x2", "Use x2 to accelerate gameplay. PAUSE lets you take a breath and review the field.", TutorialCondition.Preparation, 10, "speed-controls", TutorialContext.Preparation),
            D("breach", "QUARANTINE BREACH", "Creatures that escape increase projected civilian casualties.", TutorialCondition.Breach, 125, "health-panel"),
            D("rp", "RESEARCH EARNED", "Research Points persist between runs. Review this run's RP award in the summary.", TutorialCondition.RunCompleted, 120, "run-research-earned", TutorialContext.Summary),
            D("research-intro", "QUARANTINE RESEARCH", "Spend Research Points on permanent upgrades for future runs.", TutorialCondition.MainMenu, 110, "menu-research", TutorialContext.MainMenu),
            D("research-tree", "TECH TREE", "Nodes cost RP. Prerequisites unlock deeper research. Purchases apply to future runs.", TutorialCondition.ResearchVisited, 100, "research-tree", TutorialContext.Research),
            D("research-applied", "RESEARCH COMPLETE", "Permanent research applies to NEW runs. An existing Continue run keeps the research state it started with.", TutorialCondition.ResearchPurchased, 110, "research-detail", TutorialContext.Research),
            D("intel", "PREPARATION INTELLIGENCE", "Research can reveal upcoming threat information before the Wave begins.", TutorialCondition.Preparation, 60, "preparation-intelligence", TutorialContext.Preparation),
            D("port", "INDUSTRIAL PORT", "Containers and industrial structures are permanent obstacles. Use their natural chokepoints to shape your defense.", TutorialCondition.RunStarted, 88),
            D("maps", "CHOOSE YOUR LINE", "Danube Border offers open-field defense. Industrial Port has tighter routes and chokepoints.", TutorialCondition.MapSelect, 30, null, TutorialContext.MapSelect)
        };
        private static readonly Dictionary<string, TutorialDefinition> byId = CreateIndex();
        private static Dictionary<string, TutorialDefinition> CreateIndex()
        { var result = new Dictionary<string, TutorialDefinition>(); foreach (var d in All) result.Add(d.Id, d); return result; }
        public static TutorialDefinition Find(string id) => byId.TryGetValue(id, out var d) ? d : null;
        public static string BranchHint(string branch)
        {
            switch (branch) {
                case "Logistics": return "Economy and resource management.";
                case "Engineering": return "Structure durability and repair.";
                case "Weapons": return "Offensive defense improvements.";
                default: return "More information before assaults.";
            }
        }
    }
}
