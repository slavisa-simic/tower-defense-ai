using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Economy;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class BuildHud : MonoBehaviour
    {
        [SerializeField] private EconomyManager economy;
        [SerializeField] private GameStateController game;
        [SerializeField] private PlacementController placement;
        [SerializeField] private DefenseDefinition machineGun;
        [SerializeField] private DefenseDefinition[] defenses;
        [SerializeField] private DefenseDefinition[] displayOrder;
        private Label moneyLabel;
        private Label hint;
        private Button buildButton;
        private bool subscribed;
        private readonly List<Button> buttons = new List<Button>();
        private readonly List<Action> selections = new List<Action>();
        public string MoneyText => moneyLabel?.text;
        public bool BuildAvailable => buildButton != null && buildButton.enabledSelf;
        public DefenseDefinition MachineGun => machineGun;
        public IReadOnlyList<DefenseDefinition> Defenses => defenses;
        public void Configure(EconomyManager money, GameStateController state, PlacementController controller, DefenseDefinition definition)
        { economy = money; game = state; placement = controller; machineGun = definition; }
        public void ConfigureCatalog(DefenseDefinition[] definitions) => defenses = definitions;
        public void ConfigureDisplayOrder(DefenseDefinition[] definitions) => displayOrder = definitions;
        private void OnEnable()
        {
            if (economy == null || placement == null || game == null || machineGun == null) return;
            var root = GetComponent<UIDocument>().rootVisualElement;
            moneyLabel = root.Q<Label>("money");
            hint = root.Q<Label>("build-hint");
            buildButton = root.Q<Button>("build-mg");
            if (moneyLabel == null || hint == null || buildButton == null)
            { Debug.LogError("Build HUD is missing money, build-hint or build-mg in its UXML.", this); return; }
            if (defenses == null || defenses.Length == 0) defenses = new[] { machineGun };
            var bar = buildButton.parent;
            for (int i = 0; i < defenses.Length; i++)
            {
                var definition = defenses[i];
                var button = i == 0 ? buildButton : new Button { name = "build-" + definition.Id };
                button.AddToClassList("defense-button");
                button.AddToClassList("defense-" + definition.Id);
                button.text = $"{definition.DisplayName}  ${definition.Cost}";
                var icon = new VisualElement { pickingMode = PickingMode.Ignore };
                icon.AddToClassList("build-icon");
                button.Add(icon);
                if (i > 0) bar.Add(button);
                Action select = () => placement.Select(definition);
                button.clicked += select;
                buttons.Add(button);
                selections.Add(select);
            }
            // Visual order is independent of the existing catalog and named MG control.
            if (displayOrder != null)
                foreach (var definition in displayOrder)
                    for (int i = 0; i < defenses.Length; i++)
                        if (defenses[i] == definition) buttons[i].BringToFront();
            economy.MoneyChanged += OnMoneyChanged;
            game.StateChanged += OnStateChanged;
            placement.Changed += Refresh;
            subscribed = true;
            Refresh();
        }
        private void OnDisable()
        {
            if (!subscribed) return;
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].clicked -= selections[i];
                buttons[i].Q(className: "build-icon")?.RemoveFromHierarchy();
                if (i > 0) buttons[i].RemoveFromHierarchy();
            }
            buttons.Clear();
            selections.Clear();
            economy.MoneyChanged -= OnMoneyChanged;
            game.StateChanged -= OnStateChanged;
            placement.Changed -= Refresh;
            subscribed = false;
        }
        private void OnMoneyChanged(int money) => Refresh();
        private void OnStateChanged(GameState state) => Refresh();
        private void Refresh()
        {
            moneyLabel.text = $"Money: ${economy.Money}";
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].SetEnabled(game.IsPlaying && economy.CanAfford(defenses[i].Cost));
                buttons[i].style.opacity = buttons[i].enabledSelf ? 1f : 0.4f;
                buttons[i].EnableInClassList("selected", placement.IsPlacing && placement.Selected == defenses[i]);
            }
            if (!game.IsPlaying) hint.text = game.CurrentState == GameState.GameOver ? "Game Over — building closed" : "Preparing battlefield";
            else if (!placement.IsPlacing) hint.text = "Select a defense to build";
            else if (!placement.PreviewVisible) hint.text = $"{placement.Selected.DisplayName} selected — point at ground | RMB / Esc cancels";
            else hint.text = placement.Result == PlacementResult.Valid ? "Left click to build | RMB / Esc cancels" :
                placement.Result == PlacementResult.Occupied ? "Cell occupied | RMB / Esc cancels" :
                placement.Result == PlacementResult.OutsideGrid ? "Outside battlefield | RMB / Esc cancels" :
                "Build on free grass cells | RMB / Esc cancels";
        }
    }
}
