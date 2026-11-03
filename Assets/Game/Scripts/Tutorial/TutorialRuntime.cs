using System;
using System.Collections.Generic;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Persistence;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;

namespace ShorelineDefense.Tutorial
{
    /// <summary>Scene event adapter. Owns no gameplay state, currency, scheduling or research modifiers.</summary>
    [DefaultExecutionOrder(1400)]
    public sealed class TutorialRuntime : MonoBehaviour
    {
        public TutorialManager Manager { get; private set; }
        public TutorialView View { get; private set; }
        private ProductMenuController menu;
        private BuildManager builder;
        private PlacementController placement;
        private StructureSelection selection;
        private WaveManager waves;
        private EnemyRegistry enemies;
        private BorderBreachService border;
        private RunSession session;
        private TutorialProfileService profile;
        private WaveState lastState;
        private int lastWave, broodlings;
        private DefenseDefinition lastPlacement;
        private readonly Dictionary<DefenseBase, Action[]> defenses = new Dictionary<DefenseBase, Action[]>();
        private readonly Dictionary<DefenseBase, Action<int>> damageHandlers = new Dictionary<DefenseBase, Action<int>>();
        private readonly Dictionary<EnemyController, Action> attacks = new Dictionary<EnemyController, Action>();
        private readonly List<Vector2Int> path = new List<Vector2Int>();
        private void Start()
        {
            profile = LocalGameData.Tutorial; Manager = new TutorialManager(profile);
            menu = GetComponent<ProductMenuController>();
            builder = FindFirstObjectByType<BuildManager>(); waves = FindFirstObjectByType<WaveManager>();
            placement = FindFirstObjectByType<PlacementController>(); selection = FindFirstObjectByType<StructureSelection>();
            enemies = FindFirstObjectByType<EnemyRegistry>(); border = FindFirstObjectByType<BorderBreachService>(); session = FindFirstObjectByType<RunSession>();
            View = new TutorialView(GetComponent<UnityEngine.UIElements.UIDocument>().rootVisualElement, Manager, menu);
            menu.PageChanged += PageChanged; menu.ResearchPurchased += ResearchPurchased;
            if (session != null && session.Ready && waves != null && builder != null)
            {
                builder.DefensePlaced += DefensePlaced; builder.StableActionCommitted += StructureContext;
                builder.Economy.MoneyChanged += MoneyChanged; placement.Changed += PlacementChanged; selection.Changed += StructureContext;
                waves.Changed += WaveChanged; waves.EventChanged += EventChanged;
                enemies.EnemyRegistered += Creature; enemies.EnemyRemoved += CreatureRemoved;
                border.Breached += Breached; session.FeedbackChanged += Summary;
                foreach (var d in FindObjectsByType<DefenseBase>(FindObjectsSortMode.None)) Observe(d);
                foreach (var enemy in enemies.Active) Creature(enemy);
                SeedRun(); WaveChanged();
            }
            PageChanged(menu.CurrentPage);
        }
        private void SeedRun()
        {
            if (session == null || !session.Ready) return;
            if (!session.RestoredRun) Manager.Enqueue("intro");
            Manager.Enqueue("objective"); Manager.Enqueue("rsd");
            if (session.MapId == "industrial_port") Manager.Enqueue("port");
            Preparation();
        }
        private void Preparation()
        {
            if (waves.State != WaveState.Preparation) return;
            if (defenses.Count == 0) Manager.Enqueue("build"); else Manager.Enqueue("start-wave");
            if (waves.CompletedWaves >= 2) Manager.Enqueue("speed");
            if (waves.Meta.CountIntel || waves.Meta.EventForecast || waves.Meta.DeepWaterIntel) Manager.Enqueue("intel");
            StructureContext(); EventChanged();
        }
        private void WaveChanged()
        {
            if (waves.State == lastState && waves.PreparationWaveNumber == lastWave) return;
            lastState = waves.State; lastWave = waves.PreparationWaveNumber;
            if (waves.State == WaveState.Preparation) { broodlings = 0; Preparation(); }
            if (waves.State == WaveState.Spawning && waves.CurrentWave.Encounter != null)
            { Manager.Enqueue("boss", waves.CurrentWave.Encounter); Manager.Enqueue("creature-" + waves.CurrentWave.Encounter.Kind, waves.CurrentWave.Encounter); }
        }
        private void PlacementChanged()
        {
            if (placement.Selected == lastPlacement) return;
            lastPlacement = placement.Selected;
            if (lastPlacement == null) return;
            Manager.Enqueue("placement");
            if (lastPlacement.Id != "machine-gun-nest") Manager.Enqueue("defense-" + lastPlacement.Id, lastPlacement);
        }
        private void DefensePlaced(DefenseBase defense)
        {
            Observe(defense);
            if (defense.Definition.Id == "machine-gun-nest") Manager.Enqueue("defense-machine-gun-nest", defense);
            Manager.Enqueue("start-wave");
            if (defense.Definition.BlocksMovement && !profile.HasSeen("blockade")) CheckBlockade(defense);
            StructureContext();
        }
        private void CheckBlockade(DefenseBase defense)
        {
            if (waves.Landings == null) return;
            foreach (var landing in waves.Landings.LandingPoints)
            {
                if (!builder.Battlefield.Grid.TryWorldToCell(landing.berth, out var from)) continue;
                bool open = false;
                foreach (var goal in border.BreachCells) if (builder.Battlefield.Paths.TryFindPath(from, goal, path)) { open = true; break; }
                if (!open) { Manager.Enqueue("blockade", defense); return; }
            }
        }
        private void Observe(DefenseBase defense)
        {
            if (defense == null || defense.Owner != builder || !defense.IsAlive || defenses.ContainsKey(defense)) return;
            Action changed = () => { if (defense.Dismantled) Forget(defense); else StructureContext(); };
            Action died = () => { if (!(defense.GetComponent<LandMine>()?.Exploded ?? false)) Manager.Enqueue("destroyed"); Forget(defense); };
            Action<int> damaged = amount => {
                Manager.Enqueue("damage", defense);
                if (defense.CurrentHealth <= defense.MaxHealth * .8f) Manager.Enqueue("repair", defense);
            };
            defenses.Add(defense, new[] { changed, died }); damageHandlers.Add(defense, damaged);
            defense.Changed += changed; defense.Died += died; defense.Damaged += damaged;
        }
        private void Forget(DefenseBase d)
        {
            if (!defenses.TryGetValue(d, out var handlers)) return;
            d.Changed -= handlers[0]; d.Died -= handlers[1]; d.Damaged -= damageHandlers[d]; defenses.Remove(d); damageHandlers.Remove(d);
        }
        private void StructureContext()
        {
            var d = selection.Selected;
            if (d == null) return;
            if (d.CurrentHealth < d.MaxHealth) { Manager.Enqueue("damage", d); if (d.CurrentHealth <= d.MaxHealth * .8f) Manager.Enqueue("repair", d); }
            if (builder.CanUpgrade(d)) Manager.Enqueue("upgrade", d);
            if (waves.State == WaveState.Preparation && waves.CompletedWaves >= 1 && defenses.Count >= 3) Manager.Enqueue("dismantle", d);
        }
        private void MoneyChanged(int money) => StructureContext();
        private void Creature(EnemyController enemy)
        {
            if (attacks.ContainsKey(enemy)) return;
            Action attack = () => {
                var obstruction = enemy.StructureAttack;
                if (obstruction.State == StructureAttackState.Attacking && obstruction.Target != null) Manager.Enqueue("structure-attack", obstruction.Target);
            };
            attacks.Add(enemy, attack); enemy.CreatureAttack += attack;
            Manager.Enqueue("emergence");
            if (enemy.Definition.Kind != EnemyKind.Broodling || ++broodlings >= 6) Manager.Enqueue("creature-" + enemy.Definition.Kind, enemy);
        }
        private void CreatureRemoved(EnemyController enemy)
        {
            if (!attacks.TryGetValue(enemy, out var handler)) return;
            enemy.CreatureAttack -= handler; attacks.Remove(enemy);
        }
        private void EventChanged()
        {
            var e = waves.Events?.Current?.Definition;
            if (e == null) return;
            Manager.Enqueue("event", e);
            if (e.modifiers.fog) Manager.Enqueue("fog", e);
            if (e.modifiers.floodedColumns > 0) Manager.Enqueue("flood", e);
        }
        private void Breached(Vector3 position, int amount) => Manager.Enqueue("breach");
        private void ResearchPurchased() => Manager.Enqueue("research-applied");
        private void Summary()
        {
            if (menu.CurrentPage != "over" || session?.ResearchAward == null) return;
            profile.RecordRunCompleted(); Manager.Enqueue("rp", session.ResearchAward);
        }
        private void PageChanged(string page)
        {
            switch (page) {
                case "main": if (profile.RunCompleted) Manager.Enqueue("research-intro"); break;
                case "maps": Manager.Enqueue("maps"); break;
                case "research": if (profile.RunCompleted) Manager.Enqueue("research-tree"); break;
                case "over": Summary(); break;
                case null: SeedRun(); break;
            }
        }
        public bool Eligible(TutorialPrompt p)
        {
            if (menu.IsLoading) return false;
            var d = p.Definition;
            switch (d.MinimumContext) {
                case TutorialContext.MainMenu: return menu.CurrentPage == "main" && profile.RunCompleted;
                case TutorialContext.MapSelect: return menu.CurrentPage == "maps";
                case TutorialContext.Research: return menu.CurrentPage == "research";
                case TutorialContext.Summary: return menu.CurrentPage == "over";
            }
            if (builder == null || !builder.Game.IsPlaying || menu.ModalOpen) return false;
            var structure = p.Subject as DefenseBase;
            switch (d.MinimumContext) {
                case TutorialContext.Preparation:
                    if (waves.State != WaveState.Preparation) return false;
                    if (d.Id == "build" && defenses.Count > 0 || d.Id == "start-wave" && defenses.Count == 0) return false;
                    break;
                case TutorialContext.Placement:
                    if (!placement.IsPlacing) return false;
                    if (p.Subject is DefenseDefinition selected && placement.Selected != selected) return false;
                    break;
                case TutorialContext.DamagedStructure:
                    return structure != null && selection.Selected == structure && structure.IsAlive && structure.CurrentHealth < structure.MaxHealth;
                case TutorialContext.UpgradeAvailable:
                    return structure != null && selection.Selected == structure && builder.CanUpgrade(structure);
                case TutorialContext.DismantleAvailable:
                    return waves.State == WaveState.Preparation && structure != null && selection.Selected == structure && defenses.Count >= 3 && builder.CanDismantle(structure);
            }
            if (p.Subject is EndlessEventDefinition e && waves.Events?.Current?.Definition != e) return false;
            if (p.Subject is BossDefinition && (waves.State == WaveState.Preparation || waves.State == WaveState.WaveComplete)) return false;
            if (d.Id == "damage" && (structure == null || !structure.IsAlive || structure.CurrentHealth >= structure.MaxHealth)) return false;
            return true;
        }
        private void LateUpdate()
        {
            if (Manager == null) return;
            View.Tick(Time.unscaledDeltaTime, Eligible);
        }
        private void OnDestroy()
        {
            if (Manager == null) return;
            menu.PageChanged -= PageChanged; menu.ResearchPurchased -= ResearchPurchased;
            if (builder != null) {
                builder.DefensePlaced -= DefensePlaced; builder.StableActionCommitted -= StructureContext; builder.Economy.MoneyChanged -= MoneyChanged;
                placement.Changed -= PlacementChanged; selection.Changed -= StructureContext;
                waves.Changed -= WaveChanged; waves.EventChanged -= EventChanged;
                enemies.EnemyRegistered -= Creature; enemies.EnemyRemoved -= CreatureRemoved; border.Breached -= Breached; session.FeedbackChanged -= Summary;
            }
            foreach (var d in new List<DefenseBase>(defenses.Keys)) Forget(d);
            foreach (var enemy in new List<EnemyController>(attacks.Keys)) CreatureRemoved(enemy);
            View.Dispose(); Manager.Dispose();
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private string debugTutorialId = "objective";
        [ContextMenu("Tutorial/Trigger lesson by ID")] private void DebugTriggerSelected() => DebugTrigger(debugTutorialId);
        [ContextMenu("Tutorial/Mark lesson complete")] private void DebugMarkSelected() => DebugMarkComplete(debugTutorialId);
        [ContextMenu("Tutorial/Reset progress")] private void DebugReset() { profile.ResetProgress(); Manager.Clear(); SeedRun(); PageChanged(menu.CurrentPage); }
        public bool DebugTrigger(string id) => Manager.Enqueue(id);
        public void DebugMarkComplete(string id) => profile.MarkShown(id);
        [ContextMenu("Tutorial/Complete all lessons")] public void DebugCompleteAll() { foreach (var d in TutorialCatalog.All) profile.MarkShown(d.Id); Manager.Clear(); }
        [ContextMenu("Tutorial/Show queue")] private void DebugQueue() { foreach (var p in Manager.Queue) Debug.Log(p.Definition.Priority + " / " + p.Definition.Id); }
#endif
    }
}
