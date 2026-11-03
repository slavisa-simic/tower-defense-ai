using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ShorelineDefense.Persistence;
using ShorelineDefense.Progression;
using ShorelineDefense.Defenses;
using UnityEditor;
using UnityEngine;

namespace ShorelineDefense.Tests
{
    public sealed class MetaProgressionTests
    {
        private string directory;
        private LocalJsonStore files;
        private PlayerProfileService profile;
        private ResearchCatalog catalog;
        [SetUp] public void Setup()
        {
            directory = Path.Combine(Path.GetTempPath(), "BorderDefendersMetaTests", Guid.NewGuid().ToString());
            files = new LocalJsonStore(directory); profile = new PlayerProfileService(files); catalog = ResearchCatalog.Load();
            Assert.That(catalog, Is.Not.Null);
        }
        [TearDown] public void Cleanup() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        private RunRecord Run(int wave = 20, long bosses = 0, long homicides = 400) => new RunRecord { runId = Guid.NewGuid().ToString(), mapId = "river_border", wave = wave, completedWaves = Math.Max(0,wave-1), bosses = bosses, homicides = homicides, kills = wave*30 };
        private ResearchTreeService Tree(bool menu = true) => new ResearchTreeService(profile,catalog,()=>menu);
        private DefenseDefinition Defense(string filename) => AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/"+filename+".asset");
        [Test] public void CatalogHasTwentyUniqueBoundedNodesAndAcyclicPrerequisites()
        {
            Assert.That(catalog.nodes.Length, Is.EqualTo(20)); Assert.That(catalog.nodes.Select(n=>n.id).Distinct().Count(),Is.EqualTo(20));
            Assert.That(catalog.nodes.Sum(n=>n.costRP),Is.EqualTo(610));
            foreach(ResearchBranch b in Enum.GetValues(typeof(ResearchBranch))) Assert.That(catalog.nodes.Count(n=>n.branch==b),Is.EqualTo(5));
            foreach(var n in catalog.nodes){Assert.That(n.maxRank,Is.EqualTo(1));Assert.That(n.icon,Is.Not.Null);var seen=new System.Collections.Generic.HashSet<string>();
                void Visit(ResearchNodeDefinition p){Assert.That(p,Is.Not.Null);Assert.IsTrue(seen.Add(p.id),"Cycle");foreach(var r in p.prerequisites)Visit(r);seen.Remove(p.id);} Visit(n);}
        }
        [Test] public void RewardsAreConfigurableBoundedAndDoNotPayOrdinaryKills()
        {
            var r=Run(20,1,200);var a=catalog.rewards.Calculate(r,Array.Empty<string>(),out _);
            Assert.That(a.waveRP,Is.EqualTo(10));Assert.That(a.bossRP,Is.EqualTo(10));Assert.That(a.qualityRP,Is.EqualTo(5));
            r.kills=long.MaxValue;Assert.That(catalog.rewards.Calculate(r,Array.Empty<string>(),out _).Total,Is.EqualTo(a.Total));
            r.wave=1000000;r.bosses=1000000;Assert.That(catalog.rewards.Calculate(r,Array.Empty<string>(),out _).Total,Is.LessThan(21000));
        }
        [Test] public void EarlyDeathsCannotFarmAndFirstDecentRunBuysUsefulResearch()
        {
            for(int i=0;i<3;i++) Assert.IsTrue(profile.CompleteRun(Run(2),catalog.rewards,out var a,out _)&&a.Total==0);
            Assert.IsTrue(profile.CompleteRun(Run(5),catalog.rewards,out var first,out _));Assert.That(first.Total,Is.EqualTo(10));
            Assert.IsTrue(Tree().TryPurchase("emergency-fund",out _));Assert.That(profile.ResearchPoints,Is.Zero);
        }
        [Test] public void LongFailedRunCanEarnQualityAtTheNormalHomicidesLossThreshold()
        {
            var shortRun=catalog.rewards.Calculate(Run(20,0,1000),Array.Empty<string>(),out _);
            var longRun=catalog.rewards.Calculate(Run(40,1,1000),Array.Empty<string>(),out _);
            Assert.That(shortRun.qualityRP,Is.Zero);Assert.That(longRun.qualityRP,Is.EqualTo(10));
        }
        [Test] public void AwardAndMilestonesCommitOnceAcrossSessionsAndHistoryEviction()
        {
            var r=Run(50,2,100);r.dredgers=1;r.broodmothers=1;
            Assert.IsTrue(profile.CompleteRun(r,catalog.rewards,out var a,out _));long total=profile.ResearchPoints;
            Assert.IsTrue(profile.CompleteRun(r,catalog.rewards,out var again,out _));Assert.That(again.Total,Is.EqualTo(a.Total));Assert.That(profile.ResearchPoints,Is.EqualTo(total));
            for(int i=0;i<25;i++)profile.CompleteRun(Run(0),catalog.rewards,out _,out _);
            profile=new PlayerProfileService(files);Assert.IsTrue(profile.CompleteRun(r,catalog.rewards,out _,out _));Assert.That(profile.ResearchPoints,Is.EqualTo(total));
            Assert.That(profile.Current.lifetimeStatistics.totalRuns,Is.EqualTo(26));
            Assert.IsTrue(profile.CompleteRun(Run(50,2,100),catalog.rewards,out var second,out _));Assert.That(second.milestoneRP,Is.Zero);
        }
        [Test] public void PurchasesEnforceCurrencyPrerequisitesAndRankAndPersistImmediately()
        {
            Assert.IsFalse(Tree().TryPurchase("emergency-fund",out _));profile.DebugGrantRP(200,out _);
            Assert.IsFalse(Tree().TryPurchase("supply-contracts",out _));Assert.IsTrue(Tree().TryPurchase("emergency-fund",out _));
            Assert.That(profile.ResearchPoints,Is.EqualTo(190));Assert.IsFalse(Tree().TryPurchase("emergency-fund",out _));
            Assert.IsTrue(Tree().TryPurchase("supply-contracts",out _));profile=new PlayerProfileService(files);
            Assert.That(profile.Rank("supply-contracts"),Is.EqualTo(1));Assert.That(profile.ResearchPoints,Is.EqualTo(170));Assert.That(profile.Current.lifetimeStatistics.rpSpent,Is.EqualTo(30));
        }
        [Test] public void ResearchDuringGameplayIsRejectedByService()
        {profile.DebugGrantRP(100,out _);Assert.IsFalse(Tree(false).TryPurchase("emergency-fund",out _));Assert.That(profile.ResearchPoints,Is.EqualTo(100));}
        [Test] public void FailedAtomicPurchaseDoesNotSpendOrUnlock()
        {
            profile.DebugGrantRP(100,out _);Directory.CreateDirectory(files.PathFor("profile_save")+".tmp");
            Assert.IsFalse(Tree().TryPurchase("emergency-fund",out _));Assert.That(profile.ResearchPoints,Is.EqualTo(100));Assert.That(profile.Rank("emergency-fund"),Is.Zero);
        }
        [Test] public void ProfileCorruptionRecoversBackupAndPreservesReceipts()
        {
            var r=Run(25,1);profile.CompleteRun(r,catalog.rewards,out _,out _);long balance=profile.ResearchPoints;
            File.WriteAllText(files.PathFor("profile_save"),"{broken");profile=new PlayerProfileService(files);
            Assert.IsTrue(profile.Available);Assert.That(profile.Error,Does.Contain("backup"));Assert.That(profile.ResearchPoints,Is.EqualTo(balance));
            Assert.IsTrue(profile.CompleteRun(r,catalog.rewards,out _,out _));Assert.That(profile.ResearchPoints,Is.EqualTo(balance));
        }
        [Test] public void UnrecoverableProfileFailsGracefullyWithoutOverwritingOrDeletingRun()
        {
            Directory.CreateDirectory(directory);File.WriteAllText(files.PathFor("profile_save"),"{}");File.WriteAllText(files.PathFor("active_run"),"preserve");
            profile=new PlayerProfileService(files);Assert.IsFalse(profile.Available);Assert.IsFalse(profile.CompleteRun(Run(),catalog.rewards,out _,out _));
            Assert.That(File.ReadAllText(files.PathFor("profile_save")),Is.EqualTo("{}"));Assert.That(File.ReadAllText(files.PathFor("active_run")),Is.EqualTo("preserve"));
        }
        [Test] public void ProfileMapBestsOutliveRunDeletionAndHistory()
        {
            profile.CompleteRun(Run(25),catalog.rewards,out _,out _);var port=Run(30);port.mapId="industrial_port";profile.CompleteRun(port,catalog.rewards,out _,out _);
            new SaveGameService(files).Delete(out _);files.Delete("run_history",out _);profile=new PlayerProfileService(files);
            Assert.That(profile.BestWave("river_border"),Is.EqualTo(25));Assert.That(profile.BestWave("industrial_port"),Is.EqualTo(30));Assert.That(profile.Current.lifetimeStatistics.wavesSurvived,Is.EqualTo(53));
        }
        [Test] public void NeutralLegacySnapshotCannotAcquireCurrentResearch()
        {
            profile.DebugUnlock(catalog,true,null,out _);
            var save=new RunSaveData{saveVersion=3,metaSnapshot=null,runId=Guid.NewGuid().ToString(),wave=1,completedWaves=0};
            Assert.IsTrue(AquaticSaveMigration.Upgrade(save));Assert.That(save.saveVersion,Is.EqualTo(4));Assert.That(save.metaSnapshot.startingRsd,Is.Zero);
            Assert.That(save.metaSnapshot.blockingHealth,Is.EqualTo(1));Assert.IsFalse(save.metaSnapshot.countIntel);
        }
        [Test] public void FrozenSnapshotRemainsIndependentOfProfileAndSerializedSource()
        {
            var initial=MetaModifierService.FromProfile(profile,catalog);var run=new MetaModifierService(initial);
            profile.DebugUnlock(catalog,true,null,out _);initial.startingRsd=900;
            Assert.That(run.StartingRsd,Is.Zero);var later=new MetaModifierService(MetaModifierService.FromProfile(profile,catalog));Assert.That(later.StartingRsd,Is.EqualTo(250));
            var clone=later.Capture();clone.blockingHealth=1;Assert.That(later.HealthMultiplier(Defense("MachineGunNest")),Is.EqualTo(1.2f).Within(.001));
        }
        [Test] public void FullTreeEffectsTargetOnlyIntendedWeaponsAndRemainModerate()
        {
            profile.DebugUnlock(catalog,true,null,out _);var m=new MetaModifierService(MetaModifierService.FromProfile(profile,catalog));
            foreach(string name in new[]{"MachineGunNest","SniperTower","MortarPosition","FlamethrowerPosition","AutoCannon","RocketLauncherPosition","BarbedWire","LandMine"})
            {
                var d=Defense(name);Assert.That(m.DamageMultiplier(d),Is.EqualTo(name=="MachineGunNest"||name=="AutoCannon"?1.05f:1).Within(.001));
                Assert.That(m.RangeMultiplier(d),Is.EqualTo(name=="SniperTower"?1.08f:1).Within(.001));
                Assert.That(m.SplashMultiplier(d),Is.EqualTo(name=="MortarPosition"||name=="RocketLauncherPosition"?1.1f:1).Within(.001));
                Assert.That(m.BurnMultiplier(d),Is.EqualTo(name=="FlamethrowerPosition"?1.1f:1).Within(.001));
                Assert.That(m.FireRateMultiplier(d),Is.EqualTo(name=="BarbedWire"||name=="LandMine"?1:1.05f).Within(.001));
                Assert.That(m.HealthMultiplier(d),Is.EqualTo(name=="LandMine"||name=="BarbedWire"?1:1.2f).Within(.001));
            }
            Assert.That(m.MaxHealth(Defense("MachineGunNest"),1),Is.EqualTo(360));Assert.That(m.MaxHealth(Defense("MachineGunNest"),3),Is.EqualTo(540));
            Assert.That(m.RefundBonus,Is.EqualTo(.05));Assert.That(m.RepairMultiplier(false),Is.EqualTo(.9));
        }
        [Test] public void InvalidSnapshotNumbersAreRejected()
        {Assert.IsFalse(RunMetaSnapshot.Valid(new RunMetaSnapshot{blockingHealth=float.NaN}));Assert.IsFalse(RunMetaSnapshot.Valid(new RunMetaSnapshot{preparationRepair=0}));}
    }
}
