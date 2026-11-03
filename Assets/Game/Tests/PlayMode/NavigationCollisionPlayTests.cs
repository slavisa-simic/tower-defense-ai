using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NUnit.Framework;
using ShorelineDefense.Base;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Persistence;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;

namespace ShorelineDefense.Tests
{
    public sealed class NavigationCollisionPlayTests
    {
        private static T Find<T>() where T:Object => Object.FindFirstObjectByType<T>();
        private BuildManager Build => Find<BuildManager>();
        private EnemySpawner Spawn => Find<EnemySpawner>();
        private BattlefieldGrid Grid => Build.Battlefield.Grid;
        private EnemyRegistry Enemies => Find<EnemyRegistry>();
        private EnemyController Prefab(EnemyKind kind) => Find<WaveManager>().EndlessSettings.enemies.Concat(Find<WaveManager>().EndlessSettings.bossEncounters.bosses).Single(e=>e.Definition.Kind==kind);
        private DefenseDefinition Data(string id) => Find<BuildHud>().Defenses.Single(d=>d.Id==id);
        private Vector3 At(int x,int y)=>Grid.CellToWorld(new Vector2Int(x,y));
        private IEnumerator Load(string map)
        {
            RunSession.Pending=null; yield return SceneManager.LoadSceneAsync(map);yield return null;Time.timeScale=0;
            Find<BorderBreachService>().ConfigureThreshold(0); Build.Economy.AddMoney(50000);
            Spawn.ConfigureDeployment(.1f);Spawn.ConfigureWave(400,1,1);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        { Time.timeScale=1; yield return SceneManager.LoadSceneAsync("CoastalDefense_TopDown");yield return null;LogAssert.NoUnexpectedReceived(); }
        private static void DisableWeapons(DefenseBase defense)
        { foreach(var component in defense.GetComponents<MonoBehaviour>()) if(component is TurretCombat || component is FlameWeapon || component is AreaProjectileWeapon)component.enabled=false; }
        private List<DefenseBase> Wall()
        {
            var wall=new List<DefenseBase>();
            for(int y=0;y<Grid.Height;y++)
            {
                var cell=new Vector2Int(14,y);if(!Grid.GetCell(cell).Walkable)continue;
                Assert.AreEqual(PlacementResult.Valid,Build.TryPlaceDefense(Data(y%3==0?"sniper-tower":y%3==1?"flamethrower":"machine-gun-nest"),cell,out var d));
                DisableWeapons(d);wall.Add(d);
            }
            return wall;
        }
        private void Safe(EnemyController enemy, BattlefieldGrid battlefield=null)
        {
            if(enemy==null||!enemy.gameObject.activeInHierarchy)return;
            var map=battlefield??Grid;
            Assert.IsTrue(map.TryWorldToCell(enemy.transform.position,out var cell));Assert.IsTrue(map.IsTraversable(cell),enemy.Definition.Kind+" ghosted into "+cell);
        }
        private void Tick(EnemyController enemy,float dt)
        {enemy.Tick(dt);foreach(var shot in Object.FindObjectsByType<CorrosiveSpit>(FindObjectsSortMode.None))shot.Tick(dt);Safe(enemy);}

        [UnityTest] public IEnumerator BothMapsHaveBoundBlockersAndEveryStaticLandingHasAnExit()
        {
            foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
            {
                yield return Load(map);string report=Find<MapRuntimeContext>().ValidateStaticObstacles(out int invalid);Assert.Zero(invalid,report);
                Assert.Greater(Build.Battlefield.Definition.navigationObstacles.Length,0);
                foreach(var marker in Object.FindObjectsByType<StaticNavigationObstacle>(FindObjectsSortMode.None))
                {Assert.NotNull(marker.GetComponent<BoxCollider2D>());Assert.AreEqual("EnvironmentObstacle",LayerMask.LayerToName(marker.gameObject.layer));}
                File.AppendAllText("Docs/Validation/NavigationCollision/static-validation.txt",report+Environment.NewLine);
            }
        }
        [UnityTest] public IEnumerator AllSolidDefenseTypesRepathAroundWithoutAttackingWhenAlternateRouteExists()
        {
            foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
            foreach(string id in new[]{"machine-gun-nest","sniper-tower","flamethrower","mortar","autocannon","rocket"})
            {
                yield return Load(map);Assert.IsTrue(Spawn.TrySpawnAt(Prefab(EnemyKind.Crawler),At(8,5),out var enemy));
                int revision=Grid.Revision;Assert.AreEqual(PlacementResult.Valid,Build.TryPlaceDefense(Data(id),new Vector2Int(13,5),out var defense));DisableWeapons(defense);
                Assert.Greater(Grid.Revision,revision);Assert.NotNull(defense.GetComponent<BoxCollider2D>());
                for(int i=0;i<200&&enemy.gameObject.activeInHierarchy;i++) {Tick(enemy,.1f);yield return null;}
                Assert.Zero(enemy.StructureAttack.AttacksPerformed,id);Assert.AreEqual(defense.MaxHealth,defense.CurrentHealth);
                Assert.IsFalse(enemy.gameObject.activeInHierarchy,"Must reach border around "+id);
            }
        }
        [UnityTest] public IEnumerator FullWallReceivesRealDamageRepairUpgradeAndDismantleClearTargetAndSelection()
        {
            foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
            {
                yield return Load(map);Wall();Assert.IsTrue(Spawn.TrySpawnAt(Prefab(EnemyKind.Crawler),At(11,5),out var enemy),"Walls must not prevent deployment");
                for(int i=0;i<100&&enemy.StructureAttack.AttacksPerformed==0;i++){Tick(enemy,.1f);yield return null;}
                var target=enemy.StructureAttack.Target;Assert.NotNull(target);Assert.Less(target.CurrentHealth,target.MaxHealth);
                int attacks=enemy.StructureAttack.AttacksPerformed;
                Assert.AreEqual(PlacementResult.Valid,Build.TryPlaceDefense(Data("machine-gun-nest"),new Vector2Int(20,5),out var extra));DisableWeapons(extra);
                for(int i=0;i<25;i++){Tick(enemy,.1f);yield return null;}
                Assert.Greater(enemy.StructureAttack.AttacksPerformed,attacks,"A revision must rebuild the surviving target's approach");
                target=enemy.StructureAttack.Target;Assert.NotNull(target);
                Assert.IsTrue(Find<StructureSelection>().Select(target));
                int hp=target.CurrentHealth;for(int i=0;i<12;i++){Tick(enemy,.1f);yield return null;}Assert.Less(target.CurrentHealth,hp);
                var label=Find<StructureHud>().GetComponent<UIDocument>().rootVisualElement.Q<Label>("structure-stats");Assert.That(label.text,Does.Contain(target.CurrentHealth.ToString("N0")));
                float preparation=Build.RepairMultiplier;Assert.IsTrue(Find<WaveManager>().TryStartWave());Assert.Greater(Build.RepairMultiplier,preparation);
                Assert.IsTrue(Build.TryRepair(target));Assert.AreEqual(target.MaxHealth,target.CurrentHealth);
                Assert.IsTrue(Build.TryUpgrade(target));Assert.Greater(target.MaxHealth,hp);Assert.LessOrEqual(target.CurrentHealth,target.MaxHealth);
                for(int i=0;i<12;i++){Tick(enemy,.1f);yield return null;}Assert.Less(target.CurrentHealth,target.MaxHealth,"Repair must not pause attacks");
                int money=Build.Economy.Money;int revision=Grid.Revision;var anchor=target.Anchor;
                Assert.IsTrue(Build.TryDismantle(target,out int refund));Assert.AreEqual(money+refund,Build.Economy.Money);Assert.IsFalse(Build.TryDismantle(target,out _));
                Assert.IsNull(Find<StructureSelection>().Selected);Assert.IsNull(enemy.StructureAttack.Target);Assert.IsNull(Grid.OccupantAt(anchor));Assert.IsTrue(Grid.IsTraversable(anchor));Assert.Greater(Grid.Revision,revision);
                for(int i=0;i<220&&enemy.gameObject.activeInHierarchy;i++){Tick(enemy,.1f);yield return null;}Assert.IsFalse(enemy.gameObject.activeInHierarchy);
            }
        }
        [UnityTest] public IEnumerator EveryArchetypeIncludingBossesBreaksWallWithoutRefundOrGhosting()
        {
            foreach(var kind in new[]{EnemyKind.Crawler,EnemyKind.Skitter,EnemyKind.Bulwark,EnemyKind.Spitter,EnemyKind.Broodling,EnemyKind.Burrower,EnemyKind.Dredger,EnemyKind.Broodmother})
            {
                yield return Load("DanubeIndustrialPort");Wall();Assert.IsTrue(Spawn.TrySpawnAt(Prefab(kind),At(11,5),out var enemy));
                for(int i=0;i<100&&enemy.StructureAttack.Target==null;i++){Tick(enemy,.1f);yield return null;}
                var target=enemy.StructureAttack.Target;Assert.NotNull(target,kind.ToString());Assert.IsTrue(Find<StructureSelection>().Select(target));
                int deaths=0;target.Died+=()=>deaths++;int money=Build.Economy.Money;int revision=Grid.Revision;var anchor=target.Anchor;
                for(int i=0;i<1300&&target!=null&&target.IsAlive;i++){Tick(enemy,.1f);yield return null;}
                Assert.AreEqual(1,deaths,kind.ToString());Assert.AreEqual(money,Build.Economy.Money,"Enemy destruction must not refund");Assert.Greater(Grid.Revision,revision);
                Assert.IsNull(Grid.OccupantAt(anchor));Assert.IsNull(Grid.NavigationBlockerAt(anchor));Assert.IsNull(Find<StructureSelection>().Selected);Assert.IsNull(enemy.StructureAttack.Target);
                Assert.IsFalse(enemy.IsBurrowed);Assert.IsFalse(Build.TryDismantle(target,out _));
                // A ranged Spitter now holds position against other surviving wall sections.
                // Remove those valid ranged targets before checking that its route resumes.
                if(kind==EnemyKind.Spitter)
                {
                    foreach(var remaining in Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None))remaining.TakeDamage(int.MaxValue);
                    Assert.AreEqual(money,Build.Economy.Money,"Clearing ranged targets must not refund");
                }
                for(int i=0;i<400&&enemy.gameObject.activeInHierarchy;i++){Tick(enemy,.1f);yield return null;}Assert.IsFalse(enemy.gameObject.activeInHierarchy,kind+" must continue after wall");
            }
        }
        [UnityTest] public IEnumerator SeveralAttackersShareFrontierWithDistinctSafeContactPositions()
        {
            yield return Load("CoastalDefense_TopDown");Wall();var group=new List<EnemyController>();
            for(int i=0;i<12;i++)
            {
                var position=At(11,5)+new Vector3((i%4-1.5f)*.15f,(i/4-1)*.15f,0);
                Assert.IsTrue(Spawn.TrySpawnAt(Prefab(EnemyKind.Crawler),position,out var enemy));group.Add(enemy);
            }
            for(int i=0;i<100&&!group.All(e=>e.StructureAttack.State==StructureAttackState.Attacking);i++)
            {foreach(var e in group)Tick(e,.05f);yield return null;}
            Assert.IsTrue(group.All(e=>e.StructureAttack.State==StructureAttackState.Attacking));
            Assert.AreEqual(1,group.Select(e=>e.StructureAttack.Target).Distinct().Count());
            Assert.AreEqual(12,group.Select(e=>e.transform.position).Distinct().Count(),"Shared wall contact must spread across safe slots");
            Assert.IsTrue(group.All(e=>e.Movement.RemainingPathDistance>10*Grid.CellSize),"Wall contact must not count as border arrival for ClosestToBase targeting");
        }
        [UnityTest] public IEnumerator WireAndMineRemainWalkableSlowAndTriggerAndPreventBuildingOnCreature()
        {
            yield return Load("CoastalDefense_TopDown");Assert.IsTrue(Spawn.TrySpawnAt(Prefab(EnemyKind.Crawler),At(10,5),out var enemy));
            Assert.AreEqual(PlacementResult.EnemyInFootprint,Build.TryPlaceDefense(Data("machine-gun-nest"),new Vector2Int(10,5),out _));
            Assert.AreEqual(PlacementResult.Valid,Build.TryPlaceDefense(Data("barbed-wire"),new Vector2Int(10,5),out var wire));Assert.IsTrue(Grid.IsTraversable(wire.Anchor));
            Assert.Less(enemy.Movement.CurrentSpeed,enemy.Movement.BaseSpeed);Tick(enemy,.1f);yield return null;
            Assert.IsTrue(Build.TryDismantle(wire,out _));Assert.AreEqual(enemy.Movement.BaseSpeed,enemy.Movement.CurrentSpeed);
            Assert.AreEqual(PlacementResult.Valid,Build.TryPlaceDefense(Data("land-mine"),new Vector2Int(10,5),out var mine));Assert.IsTrue(Grid.IsTraversable(mine.Anchor));
            var trigger=mine.GetComponent<LandMine>();trigger.Tick(1);Assert.IsTrue(trigger.Exploded);Assert.IsNull(Grid.OccupantAt(new Vector2Int(10,5)));yield return null;
        }
        [UnityTest] public IEnumerator BurrowerRoutesAroundPermanentCraneContainersAndWarehouse()
        {
            yield return Load("DanubeIndustrialPort");Assert.IsTrue(Spawn.TrySpawnAt(Prefab(EnemyKind.Burrower),At(6,3),out var enemy));
            enemy.Movement.Initialize(Grid,Build.Battlefield.Paths,new Vector2Int(29,3),enemy.Definition.MovementSpeed,.05f);
            var ability=enemy.GetComponent<BurrowerTraversal>();bool sawBurrowed=false;
            for(int i=0;i<500&&!enemy.Movement.ReachedDestination;i++)
            {ability.Move(.1f);sawBurrowed|=ability.IsBurrowed;Safe(enemy);yield return null;}
            Assert.IsTrue(enemy.Movement.ReachedDestination);Assert.IsTrue(sawBurrowed);Assert.IsFalse(ability.IsBurrowed);
            Assert.LessOrEqual(ability.DistanceBurrowed,5*Grid.CellSize+.01f);Assert.IsFalse(enemy.Movement.IgnoreWireSlow);
        }
        [UnityTest] public IEnumerator HundredAndTwoHundredEnemiesRepathAndBreachBothMapsWithoutSolidCellEntry()
        {
            foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})foreach(int count in new[]{100,200})
            {
                yield return Load(map);Wall();var prefab=Prefab(EnemyKind.Crawler);int spawned=0;
                for(int x=6;x<13&&spawned<count;x++)for(int y=0;y<Grid.Height&&spawned<count;y++)
                {
                    if(!Grid.IsTraversable(new Vector2Int(x,y)))continue;
                    for(int slot=0;slot<4&&spawned<count;slot++)
                        if(Spawn.TrySpawnAt(prefab,At(x,y)+new Vector3((slot%2-.5f)*.3f,(slot/2-.5f)*.3f,0),out _))spawned++;
                }
                Assert.AreEqual(count,spawned);var active=new List<EnemyController>(count);var clock=new System.Diagnostics.Stopwatch();double peak=0,total=0;int ticks=0;
                var battlefield=Grid;
                int searches=Build.Battlefield.Paths.Searches, fields=Build.Battlefield.Paths.Obstructions.FieldBuilds;
                for(int i=0;i<900&&Enemies.Count>0;i++)
                {
                    active.Clear();active.AddRange(Enemies.Active);clock.Restart();foreach(var enemy in active)enemy.Tick(.1f);clock.Stop();
                    double ms=clock.Elapsed.TotalMilliseconds;peak=Math.Max(peak,ms);total+=ms;ticks++;
                    foreach(var enemy in active)Safe(enemy,battlefield);
                    if(i==10&&Build.TryPlaceDefense(Data("machine-gun-nest"),new Vector2Int(20,5),out var added)==PlacementResult.Valid)DisableWeapons(added);
                    yield return null;
                }
                if(Enemies.Count>0) File.WriteAllLines("Docs/Validation/NavigationCollision/stuck-crowd.txt",new[]{"game="+Build.Game.CurrentState+" revision="+Grid.Revision}.Concat(Enemies.Active.Select(e=>$"{e.name}: active={e.IsCombatActive}, pos={e.transform.position}, path={e.Movement.HasPath}, reached={e.Movement.ReachedDestination}, pending={e.Movement.RepathPending}, obstruction={e.Movement.FollowingObstructionRoute}, state={e.StructureAttack.State}, target={e.StructureAttack.Target}, pathcells={e.Movement.Path.Count}")));
                Assert.Zero(Enemies.Count,"Crowd must eventually reach border");Assert.LessOrEqual(Build.Battlefield.Paths.PeakRepathsPerFrame,PathfindingService.RepathsPerFrame);
                File.AppendAllText("Docs/Validation/NavigationCollision/density-results-v2.txt",FormattableString.Invariant($"{map}: count={count}, ticks={ticks}, simulatedSeconds={ticks*.1:0.0}, meanEnemyTickMs={total/ticks:0.000}, peakEnemyTickMs={peak:0.000}, AStarSearches={Build.Battlefield.Paths.Searches-searches}, obstructionFields={Build.Battlefield.Paths.Obstructions.FieldBuilds-fields}, peakRepaths={Build.Battlefield.Paths.PeakRepathsPerFrame}, remaining={Enemies.Count}")+Environment.NewLine);
            }
        }
    }
}
