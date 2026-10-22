using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class StructureHud : MonoBehaviour
    {
        [SerializeField] private BuildManager builder;
        [SerializeField] private StructureSelection selection;
        [SerializeField] private GameSpeedController speed;
        [SerializeField] private WaveManager waves;
        private VisualElement panel;
        private Label title, stats, status;
        private Button upgrade, repair, dismantle, pause, normal, fast;
        private WaveState previousWaveState;
        public void Configure(BuildManager manager, StructureSelection controller, GameSpeedController clock, WaveManager waveManager)
        { builder = manager; selection = controller; speed = clock; waves = waveManager; }
        private void OnEnable()
        {
            if (builder == null || selection == null || speed == null) return;
            var root = GetComponent<UIDocument>().rootVisualElement;
            panel = root.Q("structure-panel"); title = root.Q<Label>("structure-title"); stats = root.Q<Label>("structure-stats"); status = root.Q<Label>("structure-status");
            upgrade = root.Q<Button>("structure-upgrade"); repair = root.Q<Button>("structure-repair"); dismantle = root.Q<Button>("structure-dismantle");
            pause = root.Q<Button>("speed-pause"); normal = root.Q<Button>("speed-normal"); fast = root.Q<Button>("speed-double");
            upgrade.clicked += Upgrade; repair.clicked += Repair; dismantle.clicked += Dismantle;
            pause.clicked += Pause; normal.clicked += Normal; fast.clicked += Fast;
            selection.Changed += Refresh; builder.Economy.MoneyChanged += MoneyChanged; builder.Game.StateChanged += GameChanged; speed.Changed += Refresh;
            previousWaveState = waves.State;
            Refresh();
        }
        private void OnDisable()
        {
            if (panel == null) return;
            upgrade.clicked -= Upgrade; repair.clicked -= Repair; dismantle.clicked -= Dismantle;
            pause.clicked -= Pause; normal.clicked -= Normal; fast.clicked -= Fast;
            selection.Changed -= Refresh; builder.Economy.MoneyChanged -= MoneyChanged; builder.Game.StateChanged -= GameChanged; speed.Changed -= Refresh;
            panel = null;
        }
        // WaveManager.Changed also fires for every passenger. Observe only its cheap state value.
        private void Update() { if (panel != null && previousWaveState != waves.State) { previousWaveState = waves.State; Refresh(); } }
        private void MoneyChanged(int money) => Refresh();
        private void GameChanged(GameState state) => Refresh();
        private void Upgrade() => builder.TryUpgrade(selection.Selected);
        private void Repair() => builder.TryRepair(selection.Selected);
        private void Dismantle() => builder.TryDismantle(selection.Selected, out _);
        private void Pause() => speed.SetSpeed(GameSpeed.Pause);
        private void Normal() => speed.SetSpeed(GameSpeed.Normal);
        private void Fast() => speed.SetSpeed(GameSpeed.Double);
        private void Refresh()
        {
            bool playing = builder.Game.IsPlaying;
            pause.SetEnabled(playing); normal.SetEnabled(playing); fast.SetEnabled(playing);
            pause.EnableInClassList("selected", speed.Speed == GameSpeed.Pause);
            normal.EnableInClassList("selected", speed.Speed == GameSpeed.Normal);
            fast.EnableInClassList("selected", speed.Speed == GameSpeed.Double);
            var d = selection.Selected;
            panel.style.display = d != null ? DisplayStyle.Flex : DisplayStyle.None;
            if (d == null) return;
            title.text = d.Definition.DisplayName.ToUpperInvariant() + " / " + (d.Level == 1 ? "I" : d.Level == 2 ? "II" : "III");
            string core = $"HP {d.CurrentHealth:N0} / {d.MaxHealth:N0}";
            if (d.GetComponent<ShorelineDefense.Defenses.BarbedWire>() != null) core += $"\nSlow {(1 - d.MovementMultiplier) * 100:0}%";
            else if (d.GetComponent<ShorelineDefense.Defenses.LandMine>() != null) core += $"\nDamage {d.Damage} / Blast {d.Range / d.Grid.CellSize:0.0} cells";
            else
            {
                var heavy = d.Definition.HeavyWeapon;
                core += d.GetComponent<ShorelineDefense.Defenses.FlameWeapon>() != null ? $"\nDPS {d.Damage / heavy.damageTick:0} / Burn {d.BurnDuration:0.0}s" : $"\nDamage {d.Damage} / Reload {1 / d.FireRate:0.00}s";
                core += $"\nRange {d.Range / d.Grid.CellSize:0.0} cells";
                if (d.GetComponent<ShorelineDefense.Defenses.AreaProjectileWeapon>() != null) core += $"\nBlast {d.SplashRadiusInCells:0.0} / Min {heavy.minimumRangeInCells:0.0} cells";
            }
            stats.text = core;
            int cost = builder.UpgradeCost(d), repairCost = builder.RepairCost(d);
            upgrade.text = d.Level == 3 ? "MAX LEVEL III" : $"UPGRADE / {cost:N0} RSD";
            repair.text = repairCost == 0 ? "HP FULL" : $"REPAIR / {repairCost:N0} RSD";
            dismantle.text = $"DISMANTLE / +{builder.RefundFor(d):N0} RSD";
            upgrade.SetEnabled(builder.CanUpgrade(d)); repair.SetEnabled(builder.CanRepair(d)); dismantle.SetEnabled(builder.CanDismantle(d));
            status.text = $"Repair rate x{builder.RepairMultiplier:0.00}" + ((cost > builder.Economy.Money && d.Level < 3) || repairCost > builder.Economy.Money ? " / Insufficient RSD" : "");
        }
    }
}
