using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Persistence;
using ShorelineDefense.Progression;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;

namespace ShorelineDefense.Tests
{
    public sealed class MetaProgressionPlayTests
    {
        private string directory;
        private string previousDirectory;
        private static T Find<T>()where T:Object=>Object.FindFirstObjectByType<T>();
        private static VisualElement Root=>Find<ProductMenuController>().GetComponent<UIDocument>().rootVisualElement;
        private static ResearchCatalog Catalog=>ResearchCatalog.Load();
        private static void Click(string id)
        {var b=Root.Q<Button>(id);Assert.That(b,Is.Not.Null,id);Assert.IsTrue(b.enabledSelf,id);using(var e=NavigationSubmitEvent.GetPooled()){e.target=b;b.SendEvent(e);}}
        private static IEnumerator Scene(string scene)
        {yield return SceneManager.LoadSceneAsync(scene);yield return null;yield return new WaitForSecondsRealtime(.15f);Time.timeScale=0;}
        private static IEnumerator Arrive(string scene)
        {float end=Time.realtimeSinceStartup+10;while(Time.realtimeSinceStartup<end&&(SceneManager.GetActiveScene().name!=scene||Find<ProductMenuController>()==null||Find<ProductMenuController>().IsLoading))yield return null;yield return new WaitForSecondsRealtime(.15f);Time.timeScale=0;Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo(scene));}
        private static DefenseBase Place(string id,Vector2Int cell)
        {var b=Find<BuildManager>();Assert.That(b.TryPlaceDefense(Find<BuildHud>().Defenses.Single(d=>d.Id==id),cell,out var d),Is.EqualTo(PlacementResult.Valid),id+" "+cell);return d;}
        [UnitySetUp] public IEnumerator Setup()
        {previousDirectory=LocalGameData.Files.Root;directory=Path.Combine(Path.GetTempPath(),"BorderDefendersMetaPlay",Guid.NewGuid().ToString());LocalGameData.UseDirectory(directory);RunSession.Pending=null;yield return Scene("MainMenu");}
        [UnityTearDown] public IEnumerator Cleanup()
        {var scene=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("Meta cleanup"));yield return SceneManager.UnloadSceneAsync(scene);Time.timeScale=1;LocalGameData.UseDirectory(previousDirectory);if(Directory.Exists(directory))Directory.Delete(directory,true);LogAssert.NoUnexpectedReceived();}
        [UnityTest] public IEnumerator ContinueFreezesOldResearchAndNewRunReceivesLaterPurchases()
        {
            yield return Scene("CoastalDefense_TopDown");var mg=Place("machine-gun-nest",new Vector2Int(12,8));mg.TakeDamage(60);
            Assert.That(mg.MaxHealth,Is.EqualTo(300));var saved=Find<RunSession>().Capture();Assert.IsTrue(Find<RunSession>().SaveNow());
            Find<ProductMenuController>().ReturnToMenu();yield return Arrive("MainMenu");
            LocalGameData.Profile.DebugGrantRP(100,out _);Click("menu-research");Click("research-node-emergency-fund");Click("research-purchase");Click("research-node-reinforced-emplacements");Click("research-purchase");
            Assert.That(new PlayerProfileService(LocalGameData.Files).Rank("reinforced-emplacements"),Is.EqualTo(1));Click("research-back");Click("menu-continue");yield return Arrive("CoastalDefense_TopDown");
            Assert.That(Find<RunSession>().RunId,Is.EqualTo(saved.runId));Assert.That(Find<BuildManager>().Economy.Money,Is.EqualTo(saved.money));
            mg=Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).Single();Assert.That(mg.MaxHealth,Is.EqualTo(300));Assert.That(mg.CurrentHealth,Is.EqualTo(240));
            Assert.That(Place("machine-gun-nest",new Vector2Int(14,8)).MaxHealth,Is.EqualTo(300));
            Find<ProductMenuController>().ReturnToMenu();yield return Arrive("MainMenu");Click("menu-new");Click("map-river_border");Click("map-start");Click("confirm-accept");yield return Arrive("CoastalDefense_TopDown");
            Assert.That(Find<BuildManager>().Economy.Money,Is.EqualTo(600));Assert.That(Place("machine-gun-nest",new Vector2Int(12,8)).MaxHealth,Is.EqualTo(330));
            Assert.That(Find<RunSession>().RunId,Is.Not.EqualTo(saved.runId));
        }
        [UnityTest] public IEnumerator FullResearchSaveRestoresHealthMoneyAndEffectiveWeaponStats()
        {
            LocalGameData.Profile.DebugUnlock(Catalog,true,null,out _);yield return Scene("DanubeIndustrialPort");var b=Find<BuildManager>();Assert.That(b.Economy.Money,Is.EqualTo(750));
            b.Economy.AddMoney(3000);var mg=Place("machine-gun-nest",new Vector2Int(14,5));Assert.That(mg.MaxHealth,Is.EqualTo(360));Assert.That(mg.Damage,Is.EqualTo(21));Assert.That(mg.FireRate,Is.EqualTo(4.2f).Within(.001));
            Assert.IsTrue(b.TryUpgrade(mg));Assert.IsTrue(b.TryUpgrade(mg));mg.TakeDamage(70);Assert.That(mg.MaxHealth,Is.EqualTo(540));var snapshot=Find<RunSession>().Capture();
            Assert.IsTrue(Find<RunSession>().SaveNow());yield return Scene("MainMenu");Click("menu-continue");yield return Arrive("DanubeIndustrialPort");
            mg=Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).Single();Assert.That(mg.MaxHealth,Is.EqualTo(540));Assert.That(mg.CurrentHealth,Is.EqualTo(470));Assert.That(Find<BuildManager>().Economy.Money,Is.EqualTo(snapshot.money));
            Assert.That(mg.TotalInvestment,Is.EqualTo(snapshot.structures[0].investment));Assert.That(mg.Damage,Is.EqualTo(Mathf.RoundToInt(mg.Definition.Damage*1.05f*mg.Definition.LevelData(3).damage)));
        }
        [UnityTest] public IEnumerator RepairUpgradeAndDismantleUseExactDisplayedPricesWithEvents()
        {
            LocalGameData.Profile.DebugUnlock(Catalog,true,null,out _);yield return Scene("CoastalDefense_TopDown");var b=Find<BuildManager>();b.Economy.AddMoney(10000);
            var mg=Place("machine-gun-nest",new Vector2Int(14,8));mg.TakeDamage(180);
            var w=Find<WaveManager>();var snapshot=Find<RunSession>().Capture();snapshot.wave=10;snapshot.completedWaves=9;w.RestorePreparation(snapshot);Assert.IsTrue(w.ForceEvent("field-engineers"));
            Find<StructureSelection>().Select(mg);yield return null;
            int expected=(int)Math.Ceiling(mg.TotalInvestment*.5*.5*.75*.9*Math.Round(w.Events.Modifiers.preparationRepair,6));
            Assert.That(b.RepairCost(mg),Is.EqualTo(expected));Assert.That(Root.Q<Button>("structure-repair").text,Does.Contain(expected.ToString()));
            int money=b.Economy.Money;Assert.IsTrue(b.TryRepair(mg));Assert.That(money-b.Economy.Money,Is.EqualTo(expected));Assert.That(mg.CurrentHealth,Is.EqualTo(mg.MaxHealth));
            int upgrade=b.UpgradeCost(mg);Assert.That(upgrade,Is.EqualTo((int)Math.Ceiling(mg.Definition.LevelData(2).cost*.9*Math.Round(w.Events.Modifiers.upgradeCost,6))));
            Assert.IsTrue(b.TryUpgrade(mg));Assert.That(mg.TotalInvestment,Is.EqualTo(150+upgrade));int refund=b.RefundFor(mg);Assert.That(refund,Is.EqualTo((int)Math.Floor(mg.TotalInvestment*.55)));
            money=b.Economy.Money;Assert.IsTrue(b.TryDismantle(mg,out int actual));Assert.That(actual,Is.EqualTo(refund));Assert.That(b.Economy.Money-money,Is.EqualTo(refund));Assert.IsFalse(b.TryDismantle(mg,out _));
            mg=Place("machine-gun-nest",new Vector2Int(14,8));mg.TakeDamage(180);w.Events.Tick(100);Assert.IsTrue(w.TryStartWave());Assert.That(b.RepairCost(mg),Is.EqualTo((int)Math.Ceiling(150*.5*.5*1.25*.9)));
        }
        [UnityTest] public IEnumerator IntelRevealsProgressivelyAndForecastMatchesCommittedEventWithoutSideEffects()
        {
            yield return Scene("CoastalDefense_TopDown");var w=Find<WaveManager>();Assert.That(PreparationIntelligence.Describe(w,MetaModifierService.Neutral),Is.Empty);
            var m=new RunMetaSnapshot{countIntel=true};string text=PreparationIntelligence.Describe(w,new MetaModifierService(m));Assert.That(text,Does.Contain("INCOMING"));Assert.That(text,Does.Not.Contain("SWARM ACTIVITY"));
            m.threatIntel=true;m.compositionIntel=true;text=PreparationIntelligence.Describe(w,new MetaModifierService(m));Assert.That(text,Does.Contain("SWARM ACTIVITY"));Assert.That(text,Does.Contain("Crawler: High"));
            m.deepWaterIntel=true;m.eventForecast=true;var data=Find<RunSession>().Capture();data.wave=24;data.completedWaves=23;w.RestorePreparation(data);
            string before=JsonUtility.ToJson(w.Events.Capture());int money=Find<BuildManager>().Economy.Money;
            text=PreparationIntelligence.Describe(w,new MetaModifierService(m));Assert.That(text,Does.Contain("CARRIER PRESSURE"));Assert.That(text,Does.Contain("SECTORS"));
            var forecast=w.ForecastFollowingEvent();Assert.That(JsonUtility.ToJson(w.Events.Capture()),Is.EqualTo(before));Assert.That(Find<BuildManager>().Economy.Money,Is.EqualTo(money));
            var preview=new EndlessEventManager(w.EndlessSettings.eventSettings,w.RunSeed);preview.Restore(w.Events.Capture());preview.BeginWave();preview.CompleteWave();preview.Prepare(25,true,null);Assert.That(preview.Current?.Definition,Is.SameAs(forecast));
            data.wave=25;data.completedWaves=24;w.RestorePreparation(data);Assert.That(PreparationIntelligence.Describe(w,new MetaModifierService(m)),Does.Contain("MASSIVE SIGNATURE EXPECTED"));
        }
        [UnityTest] public IEnumerator FogReducesResearchedSniperRangeAndRestoresWithoutMutatingTuning()
        {
            LocalGameData.Profile.DebugUnlock(Catalog,true,null,out _);yield return Scene("CoastalDefense_TopDown");var b=Find<BuildManager>();var sniper=Place("sniper-tower",new Vector2Int(14,8));var w=Find<WaveManager>();float normal=sniper.Range;float baseRange=sniper.Definition.RangeInCells;
            var d=Find<RunSession>().Capture();d.wave=10;d.completedWaves=9;w.RestorePreparation(d);Assert.IsTrue(w.ForceEvent("fog"));w.Events.Tick(100);Assert.IsTrue(w.TryStartWave());
            Assert.That(sniper.Range,Is.EqualTo(normal*w.Events.Modifiers.defenseRange).Within(.001));w.Events.Cancel();Assert.That(sniper.Range,Is.EqualTo(normal).Within(.001));Assert.That(sniper.Definition.RangeInCells,Is.EqualTo(baseRange));
        }
        [UnityTest] public IEnumerator DuplicateGameOverPaysOnceAndShowsPersistentRewardInSummaryAndHistory()
        {
            yield return Scene("CoastalDefense_TopDown");var stats=Find<RunStatistics>();var data=stats.Capture();data.highestWave=5;stats.Restore(data);
            Find<GameStateController>().EndRun();yield return null;var session=Find<RunSession>();Assert.That(session.ResearchAward.Total,Is.EqualTo(10));Assert.That(LocalGameData.Profile.ResearchPoints,Is.EqualTo(10));
            Assert.IsTrue(session.FinalizeRun());Find<GameStateController>().EndRun();Assert.That(LocalGameData.Profile.ResearchPoints,Is.EqualTo(10));Assert.That(LocalGameData.Profile.Current.lifetimeStatistics.totalRuns,Is.EqualTo(1));
            Assert.That(Root.Q<Label>("run-research-earned").text,Does.Contain("+10 RP"));Assert.That(new RunHistoryService(LocalGameData.Files).Load().runs.Single().researchEarned,Is.EqualTo(10));Assert.IsFalse(new SaveGameService(LocalGameData.Files).Load(out _,out _));
            Find<ProductMenuController>().ReturnToMenu();yield return Arrive("MainMenu");Click("menu-research");Click("research-node-emergency-fund");Click("research-purchase");Assert.That(LocalGameData.Profile.ResearchPoints,Is.Zero);Assert.That(new PlayerProfileService(LocalGameData.Files).Rank("emergency-fund"),Is.EqualTo(1));
        }
    }
}
