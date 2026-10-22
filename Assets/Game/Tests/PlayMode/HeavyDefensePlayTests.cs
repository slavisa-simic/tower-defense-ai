using System.Collections;
using System.Linq;
using NUnit.Framework;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace ShorelineDefense.Tests
{
    public sealed class HeavyDefensePlayTests
    {
        private BuildManager build;
        private BuildHud hud;
        private EnemySpawner spawner;
        private EnemyRegistry registry;
        [UnitySetUp] public IEnumerator Setup()
        {
            Time.timeScale=0; yield return SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity"); yield return null;
            build=Object.FindFirstObjectByType<BuildManager>(); hud=Object.FindFirstObjectByType<BuildHud>();
            spawner=Object.FindFirstObjectByType<EnemySpawner>(); registry=Object.FindFirstObjectByType<EnemyRegistry>();
            typeof(EnemySpawner).GetField("minimumSpawnSeparation",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(spawner,0f);
            build.Economy.AddMoney(10000);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale=1; var previous=SceneManager.GetActiveScene(); SceneManager.SetActiveScene(SceneManager.CreateScene("Heavy cleanup"));
            yield return SceneManager.UnloadSceneAsync(previous); LogAssert.NoUnexpectedReceived();
        }
        private DefenseBase Place(string id, int x=18, int y=8)
        {
            var data=hud.Defenses.Single(d=>d.Id==id);
            Assert.That(build.TryPlaceDefense(data,new Vector2Int(x,y),out var defense),Is.EqualTo(PlacementResult.Valid)); return defense;
        }
        private EnemyController Spawn(Vector3 position, bool brute=false)
        {
            var prefab=brute?Object.FindFirstObjectByType<WaveManager>().EndlessSettings.enemies.Single(e=>e.Definition.Kind==EnemyKind.Brute):spawner.DefaultPrefab;
            Assert.IsTrue(spawner.TrySpawnAt(prefab,position,out var enemy)); return enemy;
        }
        [UnityTest] public IEnumerator AllPurchasesUseCatalogFootprintsPreviewsAndRsd()
        {
            Assert.That(hud.Defenses.Count,Is.EqualTo(8)); int x=8;
            var placement=Object.FindFirstObjectByType<PlacementController>();
            foreach(string id in new[]{"mortar","flamethrower","autocannon","rocket"})
            {
                var data=hud.Defenses.Single(d=>d.Id==id); int balance=build.Economy.Money;
                var button=hud.GetComponent<UIDocument>().rootVisualElement.Q<Button>("build-"+id);
                Assert.That(button.text,Does.Contain(data.Cost+" RSD"));
                using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=button;button.SendEvent(submit);}
                Assert.That(placement.Selected,Is.SameAs(data)); placement.HoverCell(new Vector2Int(x,3)); Assert.That(placement.Result,Is.EqualTo(PlacementResult.Valid));
                Assert.That(placement.TryPlaceCurrent(out var d),Is.EqualTo(PlacementResult.Valid)); Assert.That(build.Economy.Money,Is.EqualTo(balance-data.Cost));
                Assert.That(build.Validate(data,new Vector2Int(x,3)),Is.EqualTo(PlacementResult.Occupied));
                Assert.That(build.Battlefield.Grid.OccupantAt(new Vector2Int(x,3)),Is.SameAs(d));
                placement.HoverCell(new Vector2Int(0,3));Assert.That(placement.Result,Is.Not.EqualTo(PlacementResult.Valid)); x+=4;
            }
            yield return null;
        }
        [UnityTest] public IEnumerator MortarEnforcesAnnulusAndChoosesClusterThenImpactsOnce()
        {
            var defense=Place("mortar");var weapon=defense.GetComponent<AreaProjectileWeapon>();
            var close=Spawn(defense.transform.position+Vector3.left*.5f);var outside=Spawn(defense.transform.position+Vector3.left*16);
            weapon.Tick(.1f);Assert.That(weapon.ShotsFired,Is.Zero);
            var a=Spawn(defense.transform.position+Vector3.left*6);var b=Spawn(a.transform.position+Vector3.up*.4f);var c=Spawn(a.transform.position+Vector3.down*.4f);
            Spawn(defense.transform.position+Vector3.right*5);
            weapon.Tick(.3f);Assert.That(weapon.ShotsFired,Is.EqualTo(1));
            Assert.That(Vector3.Distance(weapon.AimPoint,a.transform.position+a.Movement.FacingDirection*a.Movement.CurrentSpeed*.8f),Is.LessThan(1),"Cluster solution includes movement prediction");
            var shot=Object.FindFirstObjectByType<TimedAreaProjectile>();Vector3 start=shot.transform.position;
            shot.Tick(.4f);Assert.That(shot.transform.position.y,Is.GreaterThan(start.y+1)); Assert.IsTrue(a.IsTargetable);
            shot.Tick(.5f);Assert.IsFalse(a.IsTargetable);Assert.IsFalse(b.IsTargetable);Assert.IsFalse(c.IsTargetable);
            int balance=build.Economy.Money;shot.Tick(2);Assert.That(build.Economy.Money,Is.EqualTo(balance));
            Assert.That(close.Health.CurrentHealth,Is.EqualTo(100));Assert.That(outside.Health.CurrentHealth,Is.EqualTo(100));Assert.That(defense.CurrentHealth,Is.EqualTo(250));yield return null;
        }
        [UnityTest] public IEnumerator RocketRetainsPointAfterTargetDeathAndDamageIgnoresVisuals()
        {
            var defense=Place("rocket");var a=Spawn(defense.transform.position+Vector3.left*6);var b=Spawn(a.transform.position+Vector3.up*.5f,true);
            defense.GetComponent<AreaProjectileWeapon>().Tick(.1f);var shot=Object.FindFirstObjectByType<TimedAreaProjectile>();var point=shot.ImpactPoint;
            a.Health.TakeDefenseDamage(100);shot.Tick(.3f);Assert.IsFalse(shot.Resolved);Assert.That(shot.ImpactPoint,Is.EqualTo(point));
            shot.Tick(.4f);Assert.That(b.Health.CurrentHealth,Is.EqualTo(350));shot.Tick(1);Assert.That(b.Health.CurrentHealth,Is.EqualTo(350));yield return null;
        }
        [UnityTest] public IEnumerator FlameConeMultiHitBurnRefreshExpiryAndReward()
        {
            var defense=Place("flamethrower");var flame=defense.GetComponent<FlameWeapon>();
            var a=Spawn(defense.transform.position+Vector3.left*2);var b=Spawn(a.transform.position+Vector3.up*.3f);var outside=Spawn(defense.transform.position+Vector3.right*3);
            flame.Tick(.2f);Assert.That(a.Health.CurrentHealth,Is.EqualTo(88));Assert.That(b.Health.CurrentHealth,Is.EqualTo(88));Assert.That(outside.Health.CurrentHealth,Is.EqualTo(100));
            var burn=a.GetComponent<EnemyDamageOverTime>();for(int i=0;i<100;i++)burn.ApplyBurn(2,15);Assert.That(burn.Dps,Is.EqualTo(15));
            burn.Tick(1);Assert.That(a.Health.CurrentHealth,Is.EqualTo(73));burn.Tick(5);Assert.That(a.Health.CurrentHealth,Is.EqualTo(58));Assert.IsFalse(burn.Burning);
            int money=build.Economy.Money;a.Health.TakeDamage(50);burn.ApplyBurn(2,15);burn.Tick(1);Assert.IsFalse(a.IsTargetable);Assert.That(build.Economy.Money,Is.EqualTo(money+25));
            burn.Tick(2);Assert.That(build.Economy.Money,Is.EqualTo(money+25));yield return null;
        }
        [UnityTest] public IEnumerator CannonPrioritizesHealthCooldownAndReacquiresAfterLoss()
        {
            var defense=Place("autocannon");var gun=defense.GetComponent<TurretCombat>();
            var weak=Spawn(defense.transform.position+Vector3.left*2);var heavy=Spawn(defense.transform.position+Vector3.left*4,true);
            gun.Tick(.01f);Assert.That(gun.Target,Is.SameAs(heavy));Assert.That(heavy.Health.CurrentHealth,Is.EqualTo(545));Assert.That(weak.Health.CurrentHealth,Is.EqualTo(100));
            gun.Tick(.2f);Assert.That(gun.ShotsFired,Is.EqualTo(1));gun.Tick(.3f);Assert.That(gun.ShotsFired,Is.EqualTo(2));
            heavy.Health.TakeDefenseDamage(1000);gun.Tick(.2f);Assert.That(gun.Target,Is.SameAs(weak));gun.Tick(.3f);Assert.That(weak.Health.CurrentHealth,Is.EqualTo(45));yield return null;
        }
        [UnityTest] public IEnumerator AreaAndBurnCancelOnGameOverWithoutDamageOrRewards()
        {
            var defense=Place("rocket");var enemy=Spawn(defense.transform.position+Vector3.left*4,true);
            defense.GetComponent<AreaProjectileWeapon>().Tick(.1f);var shot=Object.FindFirstObjectByType<TimedAreaProjectile>();
            var burn=enemy.GetComponent<EnemyDamageOverTime>();burn.ApplyBurn(2,15);
            Object.FindFirstObjectByType<GameStateController>().EndRun();shot.Tick(1);burn.Tick(2);
            Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(600));Assert.IsFalse(burn.Burning);Assert.IsTrue(shot.Resolved);yield return null;
        }
    }
}
