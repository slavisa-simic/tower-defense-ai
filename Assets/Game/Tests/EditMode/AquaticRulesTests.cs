using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using ShorelineDefense.Enemies;
using ShorelineDefense.Persistence;
using ShorelineDefense.Waves;
namespace ShorelineDefense.Tests
{
    public sealed class AquaticRulesTests
    {
        [TestCase(1)] [TestCase(2)]
        public void LegacySnapshotsMigrateDeterministicallyWithoutLosingTotals(int version)
        {
            var d=new RunSaveData{saveVersion=version,mapId=version==1?null:"industrial_port",runId=Guid.NewGuid().ToString(),wave=24,completedWaves=23,money=1577,maxHomicides=1000};
            d.statistics.kills=new long[]{1,2,3,4,5,6,7,8,9};d.events.currentId="pirate-raid";d.events.mostRecent="militant-assault";d.events.wave=24;d.events.wavesRemaining=1;d.events.nextWave=27;d.events.supplyReceived=292;
            d.events.recent.Add("mass-landing");d.events.cooldowns.Add(new EventCounter("militant-assault",20));d.events.history.Add(new EventCounter("pirate-raid",24));
            Assert.IsTrue(AquaticSaveMigration.Upgrade(d));Assert.AreEqual(4,d.saveVersion);Assert.AreEqual(version==1?"river_border":"industrial_port",d.mapId);
            CollectionAssert.AreEqual(new long[]{12,8,8,0,0,0,0,8,9},d.statistics.kills);Assert.AreEqual(45,d.statistics.kills.Sum());Assert.AreEqual(1577,d.money);
            Assert.AreEqual("swarm-surge",d.events.currentId);Assert.AreEqual("armored-migration",d.events.mostRecent);Assert.AreEqual("mass-emergence",d.events.recent[0]);Assert.AreEqual(292,d.events.supplyReceived);Assert.AreEqual(27,d.events.nextWave);
            Assert.IsTrue(AquaticSaveMigration.Upgrade(d));Assert.AreEqual(45,d.statistics.kills.Sum());
        }
        [Test]
        public void RosterUnlocksHalfCostsFamiliesAndCarrierCompatibilityAreExplicit()
        {
            var s=AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
            Assert.AreEqual(6,s.enemies.Length);Assert.AreEqual(6,s.enemies.Select(e=>e.Definition.Kind).Distinct().Count());
            var unlocks=new[]{1,3,7,10,5,15};for(int i=0;i<6;i++)Assert.AreEqual(unlocks[i],s.enemies[i].Definition.UnlockWave);
            var larva=s.enemies.Single(e=>e.Definition.Kind==EnemyKind.Broodling).Definition;Assert.AreEqual(1,larva.WaveCostHalfUnits);Assert.AreEqual(1,larva.CarrierHalfUnits);Assert.LessOrEqual(larva.KillReward,2);
            var direct=AssetDatabase.LoadAssetAtPath<CarrierDefinition>("Assets/Game/Data/Carriers/direct_emergence.asset");Assert.IsFalse(direct.Accepts(s.enemies.Single(e=>e.Definition.Kind==EnemyKind.Bulwark).Definition));Assert.IsTrue(direct.Accepts(s.enemies[0].Definition));
            foreach(var p in s.enemies)Assert.IsFalse(p.Definition.DisplayName.ToLowerInvariant().Contains("infected"));
            Assert.AreEqual(EnemyKind.Dredger,s.bossEncounters.ForWave(25).Definition.Kind);Assert.AreEqual(EnemyKind.Broodmother,s.bossEncounters.ForWave(50).Definition.Kind);
        }
        [TestCase(25)] [TestCase(50)]
        public void EncounterCostAccountsForLightweightBroodlingEscorts(int number)
        {
            var s=AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");var wave=new EndlessWaveGenerator(s,1977).Generate(number);
            try { int expected=wave.Encounter.DifficultyCost+wave.Encounter.Escorts.Sum(g=>UnityEngine.Mathf.CeilToInt(g.Count*s.enemyQuantityMultiplier*g.Prefab.Definition.WaveCostHalfUnits/2f));Assert.AreEqual(expected,wave.EncounterCost); }
            finally { UnityEngine.Object.DestroyImmediate(wave); }
        }
    }
}
