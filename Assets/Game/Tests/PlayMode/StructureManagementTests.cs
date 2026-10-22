using System.Collections;
using System.Linq;
using NUnit.Framework;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace ShorelineDefense.Tests
{
    public sealed class StructureManagementTests
    {
        private BuildManager build;
        private BuildHud hud;
        private StructureSelection selection;
        private PlacementController placement;
        private GameSpeedController speed;
        private WaveManager waves;
        [UnitySetUp] public IEnumerator Setup()
        {
            Time.timeScale=0;
            yield return SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");yield return null;
            build=Object.FindFirstObjectByType<BuildManager>();hud=Object.FindFirstObjectByType<BuildHud>();selection=Object.FindFirstObjectByType<StructureSelection>();
            placement=Object.FindFirstObjectByType<PlacementController>();speed=Object.FindFirstObjectByType<GameSpeedController>();waves=Object.FindFirstObjectByType<WaveManager>();
            build.Economy.AddMoney(20000);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            var previous=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("Management cleanup"));
            yield return SceneManager.UnloadSceneAsync(previous);Time.timeScale=1;LogAssert.NoUnexpectedReceived();
        }
        private DefenseBase Place(string id,int x=18,int y=8)
        {
            Assert.That(build.TryPlaceDefense(hud.Defenses.Single(d=>d.Id==id),new Vector2Int(x,y),out var defense),Is.EqualTo(PlacementResult.Valid));return defense;
        }
        [UnityTest] public IEnumerator AllEightUpgradeRepairAndRefundPathsPreserveDefinitionsAndOccupancy()
        {
            int x=7;
            foreach(var definition in hud.Defenses)
            {
                var d=Place(definition.Id,x,5);x+=3;int baseDamage=definition.Damage;int balance=build.Economy.Money;
                Assert.That(selection.SelectCell(d.Anchor),Is.True);Assert.That(selection.Selected,Is.SameAs(d));
                Assert.That(build.CanRepair(d),Is.False);
                for(int level=2;level<=3;level++)
                {
                    int cost=build.UpgradeCost(d);Assert.That(cost,Is.GreaterThan(0));
                    Assert.That(build.TryUpgrade(d),Is.True);balance-=cost;
                    Assert.That(d.Level,Is.EqualTo(level));Assert.That(build.Economy.Money,Is.EqualTo(balance));
                    Assert.That(d.Damage,Is.EqualTo(Mathf.RoundToInt(baseDamage*definition.LevelData(level).damage)));
                    Assert.That(d.Range,Is.EqualTo(definition.RangeInCells*definition.LevelData(level).range*d.Grid.CellSize).Within(.001));
                    Assert.That(definition.Damage,Is.EqualTo(baseDamage));
                    Assert.That(d.GetComponent<ShorelineDefense.Presentation.DefenseUpgradePresentation>(),Is.Not.Null);
                    Assert.That(definition.LevelData(level).overlay,Is.Not.Null);
                }
                Assert.That(build.TryUpgrade(d),Is.False);Assert.That(build.UpgradeCost(d),Is.Zero);
                if(d.MaxHealth>1)
                {
                    d.TakeDamage(d.MaxHealth/2);int missing=d.MaxHealth-d.CurrentHealth;
                    int expected=(int)System.Math.Ceiling(d.TotalInvestment*.5*missing/d.MaxHealth*.75);
                    Assert.That(build.RepairCost(d),Is.EqualTo(expected));
                    Assert.That(build.TryRepair(d),Is.True);Assert.That(d.CurrentHealth,Is.EqualTo(d.MaxHealth));
                    Assert.That(build.TryRepair(d),Is.False);
                }
                int refund=build.RefundFor(d);Assert.That(refund,Is.EqualTo((int)System.Math.Floor(d.TotalInvestment*.5)));
                int before=build.Economy.Money;Assert.That(build.TryDismantle(d,out int actual),Is.True);
                Assert.That(actual,Is.EqualTo(refund));Assert.That(build.Economy.Money,Is.EqualTo(before+refund));
                Assert.That(selection.Selected,Is.Null);Assert.That(build.Battlefield.Grid.OccupantAt(d.Anchor),Is.Null);
                Assert.That(build.TryDismantle(d,out _),Is.False);Assert.That(build.TryRepair(d),Is.False);Assert.That(build.TryUpgrade(d),Is.False);
            }
            yield return null;
        }
        [UnityTest] public IEnumerator SelectionPanelActionsAndInputModesStayDistinct()
        {
            var d=Place("machine-gun-nest");var root=hud.GetComponent<UIDocument>().rootVisualElement;var panel=root.Q("structure-panel");
            Assert.That(selection.SelectCell(d.Anchor),Is.True);Assert.That(panel.style.display.value,Is.EqualTo(DisplayStyle.Flex));
            Assert.That(root.Q<Label>("structure-title").text,Does.Contain("MG NEST"));
            Submit(root.Q<Button>("structure-upgrade"));Assert.That(d.Level,Is.EqualTo(2));
            d.TakeDamage(30);Submit(root.Q<Button>("structure-repair"));Assert.That(d.CurrentHealth,Is.EqualTo(d.MaxHealth));
            Assert.That(selection.SelectCell(new Vector2Int(20,10)),Is.False);Assert.That(panel.style.display.value,Is.EqualTo(DisplayStyle.None));
            selection.Select(d);placement.Select(d.Definition);Assert.That(selection.Selected,Is.Null);Assert.That(selection.SelectCell(d.Anchor),Is.False);
            placement.Cancel();selection.Select(d);placement.SelectDismantle();Assert.That(selection.Selected,Is.Null);Assert.That(selection.SelectCell(d.Anchor),Is.False);
            placement.Cancel();selection.Select(d);Submit(root.Q<Button>("structure-dismantle"));Assert.That(selection.Selected,Is.Null);Assert.That(d.Dismantled,Is.True);
            yield return null;
        }
        private static void Submit(Button button) { using(var e=NavigationSubmitEvent.GetPooled()){e.target=button;button.SendEvent(e);} }
        [UnityTest] public IEnumerator InsufficientFundsAndForeignDeadTargetsCannotMutate()
        {
            var d=Place("sniper-tower");d.TakeDamage(100);build.Economy.TrySpend(build.Economy.Money);int hp=d.CurrentHealth;
            Assert.That(build.TryUpgrade(d),Is.False);Assert.That(build.TryRepair(d),Is.False);Assert.That(d.Level,Is.EqualTo(1));Assert.That(d.CurrentHealth,Is.EqualTo(hp));
            selection.Select(d);var root=hud.GetComponent<UIDocument>().rootVisualElement;
            Assert.That(root.Q<Button>("structure-upgrade").enabledSelf,Is.False);Assert.That(root.Q<Button>("structure-repair").enabledSelf,Is.False);
            Assert.That(build.TryRepair(null),Is.False);Assert.That(selection.Select(null),Is.False);
            d.TakeDamage(int.MaxValue);Assert.That(selection.Select(d),Is.False);Assert.That(build.TryUpgrade(d),Is.False);yield return null;
        }
        [UnityTest] public IEnumerator WireUpgradeKeepsAdjacencyAndRefreshesSlowAndRemoval()
        {
            var a=Place("barbed-wire",16,7);var b=Place("barbed-wire",17,7);var wire=a.GetComponent<ShorelineDefense.Presentation.ConnectedWirePresentation>();
            int mask=wire.Mask;float initial=a.Grid.MovementMultiplierAt(a.transform.position);
            build.TryUpgrade(a);Assert.That(wire.Mask,Is.EqualTo(mask));Assert.That(a.Grid.MovementMultiplierAt(a.transform.position),Is.LessThan(initial));
            build.TryUpgrade(a);Assert.That(a.Grid.MovementMultiplierAt(a.transform.position),Is.EqualTo(a.MovementMultiplier));
            build.TryDismantle(b,out _);Assert.That(wire.Mask,Is.Zero);yield return null;
        }
        [UnityTest] public IEnumerator PreparationAndWaveRepairCostsDifferAndMoneyIsExact()
        {
            var d=Place("machine-gun-nest");d.TakeDamage(150);int prep=build.RepairCost(d);Assert.That(build.RepairMultiplier,Is.EqualTo(.75f));
            Assert.That(waves.TryStartWave(),Is.True);Assert.That(build.RepairMultiplier,Is.EqualTo(1.25f));int combat=build.RepairCost(d);Assert.That(combat,Is.GreaterThan(prep));
            int money=build.Economy.Money;Assert.That(build.TryRepair(d),Is.True);Assert.That(build.Economy.Money,Is.EqualTo(money-combat));yield return null;
        }
        [UnityTest] public IEnumerator SpeedButtonsPauseRealBoatWaveAndUiRemainsUsable()
        {
            Assert.That(waves.TryStartWave(),Is.True);speed.SetSpeed(GameSpeed.Normal);yield return new WaitForSecondsRealtime(.6f);
            var boat=waves.Landings.transform.Cast<Transform>().FirstOrDefault(b=>b.gameObject.activeInHierarchy);
            Assert.That(boat,Is.Not.Null,"A real dispatched boat is required for pause validation");
            float before=Time.time;speed.SetSpeed(GameSpeed.Pause);Vector3 position=boat!=null?boat.transform.position:Vector3.zero;int pending=waves.PendingCount;
            yield return new WaitForSecondsRealtime(.15f);Assert.That(Time.time,Is.EqualTo(before).Within(.03));Assert.That(waves.PendingCount,Is.EqualTo(pending));
            if(boat!=null)Assert.That(boat.transform.position,Is.EqualTo(position));
            var d=Place("machine-gun-nest");selection.Select(d);Assert.That(build.TryUpgrade(d),Is.True);d.TakeDamage(50);Assert.That(build.TryRepair(d),Is.True);
            var root=hud.GetComponent<UIDocument>().rootVisualElement;Submit(root.Q<Button>("speed-double"));Assert.That(Time.timeScale,Is.EqualTo(2));
            float clock=Time.time;yield return new WaitForSecondsRealtime(.2f);Assert.That(Time.time-clock,Is.GreaterThan(.3f));
            Submit(root.Q<Button>("speed-normal"));Assert.That(Time.timeScale,Is.EqualTo(1));
            build.Game.EndRun();Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(selection.Selected,Is.Null);Assert.That(speed.SetSpeed(GameSpeed.Double),Is.False);
        }
        [UnityTest] public IEnumerator UpgradedWeaponsApplyActualDamageAndConsumedMinesClearSelection()
        {
            // Frozen Update: each command is isolated from natural combat and cooldown ticking.
            Time.timeScale=0;
            var spawner=Object.FindFirstObjectByType<EnemySpawner>();
            foreach(string id in new[]{"machine-gun-nest","sniper-tower","autocannon","mortar","rocket","flamethrower","land-mine"})
            {
                var d=Place(id,24,4);build.TryUpgrade(d);build.TryUpgrade(d);
                var turret=d.GetComponent<TurretCombat>();var area=d.GetComponent<AreaProjectileWeapon>();var flame=d.GetComponent<FlameWeapon>();var mine=d.GetComponent<LandMine>();
                float distance=turret!=null?(d.Definition.RangeInCells*d.Grid.CellSize+d.Range)/2:mine!=null?.1f:flame!=null?2:6;
                Assert.That(spawner.TrySpawnAt(spawner.DefaultPrefab,d.transform.position+Vector3.left*distance,out var enemy),Is.True);
                enemy.Health.Initialize(10000);int hp=enemy.Health.CurrentHealth;
                if(turret!=null){turret.Tick(.01f);Assert.That(turret.ShotsFired,Is.EqualTo(1));}
                if(area!=null){TimedAreaProjectile shot=null;area.Fired+=(_,p)=>shot=p;area.Tick(.01f);Assert.That(shot,Is.Not.Null);shot.Tick(2);}
                if(flame!=null)flame.Tick(.01f);
                if(mine!=null){selection.Select(d);mine.Tick(1);Assert.That(mine.Exploded,Is.True);Assert.That(selection.Selected,Is.Null);Assert.That(build.TryUpgrade(d),Is.False);}
                Assert.That(hp-enemy.Health.CurrentHealth,Is.EqualTo(d.Damage),id+" must use Level III damage");
                enemy.Health.TakeDamage(int.MaxValue);if(mine==null)build.TryDismantle(d,out _);
                yield return null;
            }
        }
        [UnityTest] public IEnumerator ExternalRemovalClosesPanelWithoutCurrencyOrInputRefresh()
        {
            var d=Place("machine-gun-nest");selection.Select(d);Object.Destroy(d.gameObject);yield return null;
            Assert.That(selection.Selected,Is.Null);Assert.That(hud.GetComponent<UIDocument>().rootVisualElement.Q("structure-panel").style.display.value,Is.EqualTo(DisplayStyle.None));
        }
    }
}
