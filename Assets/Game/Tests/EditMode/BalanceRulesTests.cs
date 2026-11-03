using NUnit.Framework;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using UnityEngine;

namespace ShorelineDefense.Tests
{
    public sealed class BalanceRulesTests
    {
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
