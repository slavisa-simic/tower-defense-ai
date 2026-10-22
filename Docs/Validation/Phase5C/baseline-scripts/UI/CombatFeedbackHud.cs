using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class CombatFeedbackHud : MonoBehaviour
    {
        [SerializeField] private EnemyRegistry enemies;
        [SerializeField] private WaveManager waves;
        [SerializeField] private GameStateController game;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Font pixelFont;
        [SerializeField] private BorderBreachService border;
        [SerializeField, Min(1)] private int popupCapacity = 24;
        private sealed class Popup { public Label label; public Vector3 world; public float age; public bool active, breach; public int line; public long reward; }
        private Popup[] popups;
        private VisualElement layer, dimmer;
        private Label banner;
        private WaveState previousWave;
        private float bannerRemaining;
        private bool subscribed;
        public int ActivePopups { get; private set; }
        public int PopupCapacity => popupCapacity;
        public string LastRewardText { get; private set; }
        public string BannerText => banner?.text;
        public void Configure(EnemyRegistry registry, WaveManager manager, GameStateController state, Camera camera, Font font)
        { enemies = registry; waves = manager; game = state; worldCamera = camera; pixelFont = font; }
        private void OnEnable()
        {
            if (enemies == null || waves == null || game == null) return;
            var root = GetComponent<UIDocument>().rootVisualElement;
            dimmer = new VisualElement { name = "combat-game-over-dimmer", pickingMode = PickingMode.Ignore };
            Fullscreen(dimmer); dimmer.style.backgroundColor = new Color(.03f, .05f, .04f, .5f);
            dimmer.style.display = DisplayStyle.None; root.Insert(0, dimmer);
            layer = new VisualElement { name = "combat-feedback", pickingMode = PickingMode.Ignore };
            Fullscreen(layer); root.Add(layer);
            banner = new Label { name = "combat-wave-banner", pickingMode = PickingMode.Ignore };
            banner.AddToClassList("wave-banner");
            banner.style.position = Position.Absolute; banner.style.top = Length.Percent(18);
            banner.style.left = Length.Percent(30); banner.style.right = Length.Percent(30);
            banner.style.unityTextAlign = TextAnchor.MiddleCenter; banner.style.fontSize = 26;
            banner.style.color = new Color(.76f, .84f, .64f); banner.style.backgroundColor = new Color(.08f, .11f, .09f, .8f);
            banner.style.paddingTop = 10; banner.style.paddingBottom = 10;
            banner.style.display = DisplayStyle.None; layer.Add(banner);
            popups = new Popup[popupCapacity];
            for (int i = 0; i < popups.Length; i++)
            {
                var label = new Label { pickingMode = PickingMode.Ignore };
                label.style.position = Position.Absolute; label.style.fontSize = 18;
                label.style.color = new Color(.72f, .88f, .6f);
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                if (pixelFont != null) { label.style.unityFontDefinition = FontDefinition.FromFont(pixelFont); label.style.unityFontStyleAndWeight = FontStyle.Normal; }
                label.style.display = DisplayStyle.None; layer.Add(label);
                popups[i] = new Popup { label = label };
            }
            var gameOver = root.Q<Label>("game-over");
            if (gameOver != null && border == null) gameOver.text = "GAME OVER\nBASE DESTROYED — DANUBE LINE LOST";
            previousWave = waves.State;
            enemies.EnemyKilled += OnKilled; waves.Changed += OnWave; game.StateChanged += OnState;
            if (border != null) border.Breached += OnBreach;
            subscribed = true; OnState(game.CurrentState);
        }
        private static void Fullscreen(VisualElement e)
        { e.style.position = Position.Absolute; e.style.left = 0; e.style.right = 0; e.style.top = 0; e.style.bottom = 0; }
        private void OnDisable()
        {
            if (subscribed)
            { enemies.EnemyKilled -= OnKilled; waves.Changed -= OnWave; game.StateChanged -= OnState; subscribed = false; }
            if (border != null) border.Breached -= OnBreach;
            layer?.RemoveFromHierarchy(); dimmer?.RemoveFromHierarchy();
            popups = null; ActivePopups = 0;
        }
        private void OnKilled(EnemyController enemy)
        {
            if (!game.IsPlaying || !enemy.Health.KilledByDefense || enemy.Definition.KillReward <= 0) return;
            LastRewardText = "+" + enemy.Definition.KillReward + " RSD";
            foreach (var popup in popups)
                if (popup.active && !popup.breach && popup.age < .2f && (popup.world - enemy.transform.position).sqrMagnitude < 25)
                { popup.reward += enemy.Definition.KillReward; popup.label.text = "+" + popup.reward + " RSD"; return; }
            ShowPopup(enemy.transform.position, LastRewardText, new Color(.72f, .88f, .6f)).reward = enemy.Definition.KillReward;
        }
        private void OnBreach(Vector3 position, int penalty)
        { if (game.IsPlaying) ShowPopup(position, "+" + penalty + " HOMICIDES", new Color(.95f, .35f, .3f), true); }
        private Popup ShowPopup(Vector3 position, string text, Color color, bool breach = false)
        {
            int usedLines = 0, line = 0;
            Popup oldestAtLane = null;
            if (breach)
                foreach (var p in popups)
                    if (p.active && p.breach && (p.world - position).sqrMagnitude < 1)
                    { usedLines |= 1 << p.line; if (oldestAtLane == null || p.age > oldestAtLane.age) oldestAtLane = p; }
            while (line < 3 && (usedLines & (1 << line)) != 0) line++;
            Popup slot = popups[0];
            for (int i = 0; i < popups.Length; i++)
            {
                if (!popups[i].active) { slot = popups[i]; break; }
                if (popups[i].age > slot.age) slot = popups[i];
            }
            // At most three readable rows per breach lane; recycle the oldest there under a burst.
            if (breach && line == 3) { slot = oldestAtLane; line = slot.line; }
            if (!slot.active) ActivePopups++;
            slot.active = true; slot.age = 0; slot.world = position;
            slot.reward = 0;
            slot.breach = breach; slot.line = line;
            slot.label.text = text; slot.label.style.color = color; slot.label.style.opacity = 1;
            slot.label.style.display = DisplayStyle.Flex;
            PositionPopup(slot);
            return slot;
        }
        private void OnWave()
        {
            if (waves.State == previousWave) return;
            previousWave = waves.State;
            if (!game.IsPlaying) return;
            if (waves.State == WaveState.Spawning) ShowBanner("WAVE " + waves.CurrentWaveNumber + " INCOMING");
            else if (waves.State == WaveState.WaveComplete) ShowBanner("WAVE COMPLETE   +" + waves.CurrentWave.CompletionReward + " RSD");
        }
        private void ShowBanner(string message)
        { banner.text = message; bannerRemaining = 1.5f; banner.style.opacity = 1; banner.style.display = DisplayStyle.Flex; }
        private void OnState(GameState state)
        {
            dimmer.style.display = state == GameState.GameOver ? DisplayStyle.Flex : DisplayStyle.None;
            if (state != GameState.GameOver) return;
            bannerRemaining = 0; banner.style.display = DisplayStyle.None;
            foreach (var p in popups) { p.active = false; p.label.style.display = DisplayStyle.None; }
            ActivePopups = 0;
        }
        private void PositionPopup(Popup p)
        {
            if (worldCamera == null || layer.panel == null) return;
            Vector2 screen = RuntimePanelUtils.CameraTransformWorldToPanel(layer.panel, p.world, worldCamera);
            float width = p.label.resolvedStyle.width;
            if (float.IsNaN(width)) width = 220;
            p.label.style.left = Mathf.Round(Mathf.Clamp(screen.x - 10, 0, Mathf.Max(0, layer.resolvedStyle.width - width - 8)));
            p.label.style.top = Mathf.Round(screen.y - 18 - p.age * 32 - (p.breach ? p.line * 26 : 0));
        }
        private void Update()
        {
            if (popups == null) return;
            if (bannerRemaining > 0)
            {
                bannerRemaining -= Time.deltaTime;
                banner.style.opacity = Mathf.Clamp01(bannerRemaining / .35f);
                if (bannerRemaining <= 0) banner.style.display = DisplayStyle.None;
            }
            foreach (var p in popups)
            {
                if (!p.active) continue;
                p.age += Time.deltaTime;
                if (p.age >= .85f)
                { p.active = false; p.label.style.display = DisplayStyle.None; ActivePopups--; continue; }
                PositionPopup(p); p.label.style.opacity = Mathf.Clamp01((.85f - p.age) / .3f);
            }
        }
    }
}
