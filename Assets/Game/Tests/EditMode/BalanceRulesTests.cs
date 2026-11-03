using NUnit.Framework;
using System.Linq;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using UnityEngine;

namespace ShorelineDefense.Tests
{
    public sealed class BalanceRulesTests
    {
        [Test] public void PortCargoPreservesOpeningAndHealthAndRespectsItsScaledBudget()
        {
            var s=UnityEditor.AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
            var go=new GameObject("Port cargo tuning");
            try
            {
                var arrivals=go.AddComponent<AssaultCarrierSystem>();var data=new UnityEditor.SerializedObject(arrivals);
                data.FindProperty("distributedPressureFromWave").intValue=11;data.FindProperty("lateSpitterWeight").floatValue=3;data.FindProperty("lateBurrowerWeight").floatValue=4;data.FindProperty("lateFormationMultiplier").floatValue=2.4f;data.ApplyModifiedPropertiesWithoutUndo();
                int ordinaryFlankers=0,portFlankers=0;
                for(int wave=1;wave<=50;wave++)
                {
                    var generator=new EndlessWaveGenerator(s,1977);
                    int quantity=arrivals.FormationQuantity(wave,s.enemyQuantityMultiplier);
                    var ordinary=generator.Generate(wave);var port=generator.Generate(wave,null,arrivals.CargoWeight(EnemyKind.Spitter,wave),arrivals.CargoWeight(EnemyKind.Burrower,wave),quantity);
                    try
                    {
                        Assert.That(port.HealthMultiplier,Is.EqualTo(ordinary.HealthMultiplier));Assert.That(port.SpeedMultiplier,Is.EqualTo(ordinary.SpeedMultiplier));
                        Assert.That(port.Groups.Sum(g=>g.Count*g.Prefab.Definition.WaveCostHalfUnits)/2,Is.LessThanOrEqualTo(generator.Budget(wave,quantity)+port.EncounterCost));
                        if(wave<=10)CollectionAssert.AreEqual(ordinary.Groups.Select(g=>(g.Prefab,g.Count)).ToArray(),port.Groups.Select(g=>(g.Prefab,g.Count)).ToArray());
                        if(wave>=31)
                        {
                            ordinaryFlankers+=ordinary.Groups.Where(g=>g.Prefab.Definition.Kind==EnemyKind.Spitter||g.Prefab.Definition.Kind==EnemyKind.Burrower).Sum(g=>g.Count);
                            portFlankers+=port.Groups.Where(g=>g.Prefab.Definition.Kind==EnemyKind.Spitter||g.Prefab.Definition.Kind==EnemyKind.Burrower).Sum(g=>g.Count);
                        }
                    }
                    finally{Object.DestroyImmediate(ordinary);Object.DestroyImmediate(port);}
                }
                Assert.That(portFlankers,Is.GreaterThan(ordinaryFlankers*1.5f));
            }
            finally{Object.DestroyImmediate(go);}
        }
        [Test] public void LateIncomePreservesOpeningAndCapsGrantGrowth()
        {
            var s=UnityEditor.AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
            foreach(var enemy in s.enemies)
            {
                for(int wave=1;wave<=10;wave++)Assert.That(s.KillReward(enemy.Definition,wave),Is.EqualTo(enemy.Definition.KillReward));
                int previous=enemy.Definition.KillReward;
                for(int wave=11;wave<=100;wave++){int reward=s.KillReward(enemy.Definition,wave);Assert.That(reward,Is.InRange(0,previous));previous=reward;}
            }
            for(int wave=1;wave<=10;wave++)Assert.That(s.CompletionReward(wave),Is.EqualTo(s.baseReward+(wave-1)*s.rewardGrowth));
            Assert.That(s.CompletionReward(50),Is.EqualTo(s.CompletionReward(30)));
            Assert.That(s.CompletionReward(int.MaxValue),Is.EqualTo(s.CompletionReward(30)));
            foreach(var boss in s.bossEncounters.bosses)Assert.That(s.KillReward(boss.Definition,50),Is.EqualTo(boss.Definition.KillReward));
        }
        [Test] public void SplitRoutePressureStillAdmitsOnlyTheRemainingHardCap()
        {
            var s=UnityEditor.AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
            var director=new AssaultPressureDirector();director.Tick(.2f,s,50,350,1000,4,49,false,null,2.25f,.5f,6);
            Assert.That(director.Target,Is.EqualTo(400));Assert.That(director.RemainingAdmission(s,350,49),Is.EqualTo(1));
            Assert.That(director.RemainingAdmission(s,350,50),Is.Zero);
            Assert.That(director.BoatLimit,Is.EqualTo(Mathf.Min(s.BoatConcurrency(50)+6,32)));
        }
        [TestCase(0f, 50)]
        [TestCase(.75f, 88)]
        [TestCase(1f, 100)]
        public void PenetrationIgnoresOnlyTheSpecifiedArmorFraction(float penetration, int expected)
        {
            var go = new GameObject("Armor rule");
            try
            {
                var health = go.AddComponent<EnemyHealth>(); health.Initialize(1000); health.ConfigureArmor(.5f);
                health.TakeDefenseDamage(100, penetration);
                Assert.That(1000-health.CurrentHealth, Is.EqualTo(expected));
                health.ConfigureArmor(0); health.TakeDefenseDamage(100, penetration);
                Assert.That(1000-health.CurrentHealth, Is.EqualTo(expected+100));
            }
            finally { Object.DestroyImmediate(go); }
        }
        [Test] public void PenetratingLethalDamageCommitsAttributionAndDeathOnce()
        {
            var go = new GameObject("Lethal rule");
            try
            {
                var health = go.AddComponent<EnemyHealth>(); health.Initialize(88); health.ConfigureArmor(.5f);
                int deaths=0,damage=0;health.Died+=()=>deaths++;health.Damaged+=n=>damage+=n;
                health.TakeDefenseDamage(100,.75f);health.TakeDefenseDamage(int.MaxValue,1);
                Assert.That(health.KilledByDefense,Is.True);Assert.That(deaths,Is.EqualTo(1));Assert.That(damage,Is.EqualTo(88));
            }
            finally { Object.DestroyImmediate(go); }
        }
        [Test] public void AdmissionUsesCurrentIncomingCargoBetweenPolicyEvaluations()
        {
            var settings=ScriptableObject.CreateInstance<EndlessWaveSettings>();
            try
            {
                settings.earlyPressureTarget=8;settings.maximumActiveEnemies=400;settings.pressureHysteresis=.2f;
                var director=new AssaultPressureDirector();director.Tick(.2f,settings,1,0,80,0,0,false);
                Assert.That(director.RemainingAdmission(settings,0,0),Is.EqualTo(10));
                Assert.That(director.RemainingAdmission(settings,2,8),Is.Zero);
                Assert.That(director.RemainingAdmission(settings,2,7),Is.EqualTo(1));
                Assert.That(director.RemainingAdmission(settings,400,32),Is.Zero);
            }
            finally { Object.DestroyImmediate(settings); }
        }
    }
}
