using ShorelineDefense.Base;
using ShorelineDefense.Economy;
using ShorelineDefense.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.UI
{
    /// <summary>Art and short feedback only; existing HUD controllers own values and controls.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class HudArtPresentation : MonoBehaviour
    {
        [SerializeField] private EconomyManager economy;
        [SerializeField] private BaseHealth playerBase;
        [SerializeField] private Font pixelFont;
        [SerializeField] private RunStatistics statistics;
        [SerializeField] private BorderBreachService border;
        private VisualElement moneyPanel, healthPanel, healthFill;
        private float moneyUntil, healthUntil;
        private int previousMoney, previousHealth;
        private bool subscribed;
        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            if (pixelFont != null) root.style.unityFontDefinition = FontDefinition.FromFont(pixelFont);
            moneyPanel = root.Q("money-panel"); healthPanel = root.Q("health-panel"); healthFill = root.Q("health-fill");
            if (economy == null || playerBase == null) return;
            previousMoney = economy.Money; previousHealth = playerBase.CurrentHealth;
            economy.MoneyChanged += OnMoney; playerBase.Changed += OnHealth; subscribed = true; OnHealth();
            if (statistics != null) { statistics.OnHomicidesChanged += OnHomicides; OnHomicides(statistics.Homicides); }
        }
        private void OnDisable()
        {
            if (subscribed) { economy.MoneyChanged -= OnMoney; playerBase.Changed -= OnHealth; subscribed = false; }
            if (statistics != null) statistics.OnHomicidesChanged -= OnHomicides;
            moneyPanel?.RemoveFromClassList("money-gain"); moneyPanel?.RemoveFromClassList("money-spent");
            healthPanel?.RemoveFromClassList("health-hit"); moneyUntil = healthUntil = 0;
        }
        private void OnMoney(int amount)
        {
            if (amount == previousMoney || moneyPanel == null) return;
            moneyPanel.EnableInClassList("money-gain", amount > previousMoney);
            moneyPanel.EnableInClassList("money-spent", amount < previousMoney);
            previousMoney = amount; moneyUntil = Time.unscaledTime + .25f;
        }
        private void OnHealth()
        {
            if (border != null) return;
            if (healthFill != null) healthFill.style.width = Length.Percent(100f * playerBase.CurrentHealth / Mathf.Max(1, playerBase.MaxHealth));
            if (playerBase.CurrentHealth < previousHealth && healthPanel != null)
            { healthPanel.AddToClassList("health-hit"); healthUntil = Time.unscaledTime + .2f; }
            previousHealth = playerBase.CurrentHealth;
        }
        private void OnHomicides(long value)
        {
            if (healthFill != null) healthFill.style.width = Length.Percent(border != null && border.MaxHomicides > 0 ?
                Mathf.Clamp01((float)((double)value / border.MaxHomicides)) * 100 : 0);
            healthPanel?.AddToClassList("health-hit"); healthUntil = Time.unscaledTime + .2f;
        }
        private void Update()
        {
            if (moneyUntil > 0 && Time.unscaledTime >= moneyUntil)
            { moneyPanel?.RemoveFromClassList("money-gain"); moneyPanel?.RemoveFromClassList("money-spent"); moneyUntil = 0; }
            if (healthUntil > 0 && Time.unscaledTime >= healthUntil)
            { healthPanel?.RemoveFromClassList("health-hit"); healthUntil = 0; }
        }
    }
}
