using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using ShorelineDefense.Grid;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Persistence;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ShorelineDefense.Tests
{
    public sealed class IndustrialPortPlayTests
    {
        static T Find<T>() where T:Object => Object.FindFirstObjectByType<T>();
        [UnitySetUp] public IEnumerator Setup() { RunSession.Pending=null; new SaveGameService(LocalGameData.Files).Delete(out _); yield return SceneManager.LoadSceneAsync("DanubeIndustrialPort"); yield return null; Time.timeScale=0; Find<BorderBreachService>().ConfigureThreshold(0); }
        [UnityTearDown] public IEnumerator Cleanup() { Time.timeScale=1; yield return SceneManager.LoadSceneAsync("CoastalDefense_TopDown"); yield return null; }
        [UnityTest] public IEnumerator AllLandingPathsAndBothBossCargoBreachThroughSharedService()
        {
            var context=Find<MapRuntimeContext>(); string report=context.Validate(out int invalid); Assert.Zero(invalid,report);
            var boats=Find<BoatLandingSystem>(); var waves=Find<WaveManager>(); var registry=Find<EnemyRegistry>();
            Assert.That(boats.LandingPoints.Count,Is.EqualTo(8)); Assert.That(boats.LandingPoints.Select(p=>boats.SectorFor(p)).Distinct().Count(),Is.EqualTo(3));
            foreach(var prefab in waves.EndlessSettings.bossEncounters.bosses)
            {
                long before=Find<RunStatistics>().BossesBreached;
                Assert.IsTrue(boats.TryDispatch(prefab,1));
                for(int i=0;i<500&&(boats.ActiveBoats>0||registry.Count>0);i++) { boats.Tick(.5f); foreach(var e in registry.Active.ToArray())e.Tick(100); yield return null; }
                Assert.That(Find<RunStatistics>().BossesBreached,Is.EqualTo(before+1)); Assert.Zero(boats.IncomingPassengers);
            }
        }
        [UnityTest] public IEnumerator AllDefensesManageSaveAndContinueOnSelectedMap()
        {
            var builder=Find<BuildManager>(); builder.Economy.AddMoney(50000); var catalog=Find<BuildHud>().Defenses;
            int x=7;
            foreach(var d in catalog)
            {
                while(builder.Validate(d,new Vector2Int(x,5))!=PlacementResult.Valid)x++;
                Assert.That(builder.TryPlaceDefense(d,new Vector2Int(x++,5),out var defense),Is.EqualTo(PlacementResult.Valid));
                Assert.IsTrue(builder.TryUpgrade(defense)); Assert.IsTrue(builder.TryUpgrade(defense));
                if(defense.MaxHealth>1) { defense.TakeDamage(1); Assert.IsTrue(builder.TryRepair(defense)); }
            }
            var removed=builder.Battlefield.Grid.OccupantAt(new Vector2Int(7,5)) as DefenseBase; Assert.IsTrue(builder.TryDismantle(removed,out _));
            var session=Find<RunSession>(); var expected=session.Capture(); Assert.That(expected.mapId,Is.EqualTo("industrial_port")); Assert.IsTrue(session.SaveNow());
            yield return SceneManager.LoadSceneAsync("MainMenu"); yield return null; Find<ProductMenuController>().Continue();
            for(int i=0;i<120 && SceneManager.GetActiveScene().name!="DanubeIndustrialPort";i++)yield return null;
            yield return null; yield return null; Time.timeScale=0;
            var actual=Find<RunSession>().Capture(); Assert.That(actual.mapId,Is.EqualTo(expected.mapId)); Assert.That(actual.structures.Count,Is.EqualTo(7)); Assert.That(actual.money,Is.EqualTo(expected.money));
            foreach(var item in expected.structures) { var d=Find<BuildManager>().Battlefield.Grid.OccupantAt(new Vector2Int(item.x,item.y)) as DefenseBase; Assert.That(d.Level,Is.EqualTo(3)); Assert.That(d.CurrentHealth,Is.EqualTo(item.health)); }
        }
        [UnityTest] public IEnumerator EarlyAndMidgameWavesCompleteWithRealBoatsAndSharedBreach()
        {
            var waves=Find<WaveManager>(); var registry=Find<EnemyRegistry>();
            foreach(int start in new[]{1,10})
            {
                if(start==10) { var d=Find<RunSession>().Capture(); d.wave=10;d.completedWaves=9;waves.RestorePreparation(d); }
                for(int w=start;w<= (start==1 ? 5 : 20);w++)
                {
                    waves.Events.Tick(100); Assert.IsTrue(waves.TryStartWave(),"Start wave "+w);
                    for(int t=0;t<4000&&waves.State!=WaveState.WaveComplete;t++) { waves.Tick(.5f);foreach(var e in registry.Active.ToArray())e.Tick(100); yield return null; }
                    Assert.That(waves.State,Is.EqualTo(WaveState.WaveComplete),"Wave "+w); waves.Tick(2);
                    for(int t=0;t<100&&waves.Landings.ActiveBoats>0;t++)waves.Landings.Tick(1); yield return null;
                }
            }
        }
        [UnityTest] public IEnumerator EveryEventWorksAndFloodUsesAuthoredDockCells()
        {
            var waves=Find<WaveManager>(); var boats=Find<BoatLandingSystem>(); var builder=Find<BuildManager>();
            var snapshot=Find<RunSession>().Capture();snapshot.wave=30;snapshot.completedWaves=29;waves.RestorePreparation(snapshot);
            foreach(var ev in waves.EndlessSettings.eventSettings.events)
            {
                waves.RestorePreparation(snapshot); Assert.IsTrue(waves.ForceEvent(ev.id),ev.id); waves.Events.Tick(100);
                if(ev.modifiers.floodedColumns>0)
                {
                    Assert.That(builder.Validate(Find<BuildHud>().Defenses.First(),new Vector2Int(6,5)),Is.EqualTo(PlacementResult.NotBuildable));
                    Assert.IsFalse(builder.Battlefield.Definition.IsFloodable(new Vector2Int(15,5),2));
                }
                waves.Events.BeginWave();
                if(ev.modifiers.fog) Assert.IsTrue(Find<ShorelineDefense.Presentation.EndlessEventPresentation>().FogVisible);
                boats.ConfigurePressure(.1f,8);
                for(int i=0;i<100;i++) { boats.TryDispatch(waves.EndlessSettings.enemies[0],1);boats.Tick(.5f);foreach(var e in Find<EnemyRegistry>().Active.ToArray())e.Tick(100);yield return null; }
                boats.Cancel(); waves.Events.Cancel(); Assert.IsFalse(Find<ShorelineDefense.Presentation.EndlessEventPresentation>().FogVisible);
            }
            Assert.That(boats.SectorArrivals.All(n=>n>0),Is.True);
        }
    }
}
