using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
namespace ShorelineDefense.Tests
{
    public sealed class EndlessWaveTests
    {
        private EndlessWaveSettings Settings => AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
        [TestCase(1)] [TestCase(10)] [TestCase(25)] [TestCase(50)] [TestCase(100)] [TestCase(1000)] [TestCase(int.MaxValue)]
        public void CompositionIsValidBudgetedBoundedAndNumericallySafe(int number)
        {
            var settings=Settings;var generator=new EndlessWaveGenerator(settings,123);
            for(int seed=0;seed<10;seed++)
            {
                var g=new EndlessWaveGenerator(settings,seed);var wave=g.Generate(number);
                try
                {
                    Assert.IsTrue(wave.IsValid);Assert.That(wave.ScheduledCount,Is.InRange(1,settings.maximumEnemiesPerWave + (wave.Encounter != null ? 1 + wave.Encounter.Escorts.Sum(e=>e.Count)*settings.enemyQuantityMultiplier : 0)));
                    int cost=wave.Groups.Sum(group=>group.Count*group.Prefab.Definition.DifficultyCost);Assert.LessOrEqual(cost,g.Budget(number)+wave.EncounterCost);
                    if(wave.ScheduledCount - (wave.Encounter != null ? 1 + wave.Encounter.Escorts.Sum(e=>e.Count)*settings.enemyQuantityMultiplier : 0)<settings.maximumEnemiesPerWave)Assert.Less(g.Budget(number)+wave.EncounterCost-cost,settings.enemies.Min(p=>p.Definition.DifficultyCost)*settings.enemyQuantityMultiplier);
                    Assert.That(wave.HealthMultiplier,Is.InRange(1,settings.maximumHealthMultiplier));Assert.That(wave.SpeedMultiplier,Is.InRange(1,settings.maximumSpeedMultiplier));
                    Assert.GreaterOrEqual(wave.CompletionReward,100);
                    foreach(var group in wave.Groups){Assert.Greater(group.Count,0);Assert.LessOrEqual(group.Prefab.Definition.UnlockWave,number);}
                }
                finally{UnityEngine.Object.DestroyImmediate(wave);}
            }
        }
        [Test]
        public void SeedReproducesCompositionAndOtherSeedsVary()
        {
            var a=new EndlessWaveGenerator(Settings,123).Generate(12);var b=new EndlessWaveGenerator(Settings,123).Generate(12);var c=new EndlessWaveGenerator(Settings,321).Generate(12);
            try {CollectionAssert.AreEqual(a.Groups.Select(g=>g.Prefab).ToArray(),b.Groups.Select(g=>g.Prefab).ToArray());CollectionAssert.AreNotEqual(a.Groups.Select(g=>g.Prefab).ToArray(),c.Groups.Select(g=>g.Prefab).ToArray());}
            finally{UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);UnityEngine.Object.DestroyImmediate(c);}
        }
        [Test]
        public void UnlocksEnemyStatsAndDifficultyGrowthMatchRoles()
        {
            var s=Settings;Assert.Greater(new EndlessWaveGenerator(s,0).Budget(100),new EndlessWaveGenerator(s,0).Budget(25));
            var basic=s.enemies.First(p=>p.Definition.Kind==EnemyKind.Basic).Definition;var runner=s.enemies.First(p=>p.Definition.Kind==EnemyKind.Runner).Definition;var brute=s.enemies.First(p=>p.Definition.Kind==EnemyKind.Brute).Definition;
            Assert.That(runner.UnlockWave,Is.EqualTo(3));Assert.That(brute.UnlockWave,Is.EqualTo(6));Assert.Less(runner.MaxHealth,basic.MaxHealth);Assert.Greater(runner.MovementSpeed,basic.MovementSpeed);Assert.Greater(brute.MaxHealth,basic.MaxHealth*5);Assert.Less(brute.MovementSpeed,basic.MovementSpeed);Assert.That(brute.TransportCapacityUnits,Is.EqualTo(3));
            for(int wave=1;wave<6;wave++) {var generated=new EndlessWaveGenerator(s,42).Generate(wave);try{foreach(var g in generated.Groups)Assert.LessOrEqual(g.Prefab.Definition.UnlockWave,wave);}finally{UnityEngine.Object.DestroyImmediate(generated);}}
        }
    }
}
