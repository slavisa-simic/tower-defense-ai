using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ShorelineDefense.Waves;
using ShorelineDefense.Enemies;
namespace ShorelineDefense.Tests {
 public sealed class Phase4CRulesTests {
  EndlessWaveSettings Settings=>AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
  [TestCase(1,60,90)] [TestCase(3,100,150)] [TestCase(5,150,225)] [TestCase(10,225,375)] [TestCase(20,375,650)]
  public void PressureEstablishesScaleEarly(int number,int minimum,int maximum){var w=new EndlessWaveGenerator(Settings,1977).Generate(number);try{Assert.That(w.ScheduledCount,Is.InRange(minimum,maximum));}finally{UnityEngine.Object.DestroyImmediate(w);}}
  [Test] public void BoatCadenceAndConcurrencyGrowWithoutOverflow(){var s=Settings;Assert.That(s.BoatConcurrency(1),Is.EqualTo(6));Assert.That(s.BoatConcurrency(5),Is.EqualTo(8));Assert.That(s.BoatConcurrency(10),Is.EqualTo(10));Assert.That(s.BoatConcurrency(20),Is.EqualTo(15));Assert.That(s.BoatConcurrency(int.MaxValue),Is.EqualTo(28));Assert.Less(s.ArrivalInterval(25),s.ArrivalInterval(1));Assert.That(s.ArrivalInterval(int.MaxValue),Is.GreaterThanOrEqualTo(.3f));}
  [Test] public void BossScheduleAlternatesAndAddsEscortsWithoutRemovingNormalPressure(){var s=Settings;Assert.IsNull(s.bossEncounters.ForWave(24));Assert.IsNull(s.bossEncounters.ForWave(26));Assert.AreEqual(EnemyKind.UndeadOsama,s.bossEncounters.ForWave(25).Definition.Kind);Assert.AreEqual(EnemyKind.PirateWarlord,s.bossEncounters.ForWave(50).Definition.Kind);Assert.AreEqual(EnemyKind.UndeadOsama,s.bossEncounters.ForWave(75).Definition.Kind);var w=new EndlessWaveGenerator(s,1977).Generate(25);try{Assert.GreaterOrEqual(w.ScheduledCount,120);Assert.AreEqual(1,w.Groups.Count(g=>g.Prefab.Definition is BossDefinition));Assert.GreaterOrEqual(w.Encounter.Escorts.Sum(g=>g.Count),30);Assert.Greater(w.EncounterCost,0);}finally{UnityEngine.Object.DestroyImmediate(w);}}
  [Test] public void AllSpecialEnemiesAreConfiguredAndFactionBiasRetainsBasicCrowds(){var s=Settings;Assert.AreEqual(7,s.enemies.Length);foreach(var prefab in s.enemies){Assert.NotNull(prefab.Definition);Assert.LessOrEqual(prefab.Definition.TransportCapacityUnits,7);}foreach(int n in new[]{24,26,31}){var w=new EndlessWaveGenerator(s,1977).Generate(n);try{Assert.Greater(w.Groups.Count(g=>g.Prefab.Definition.Kind==EnemyKind.Basic),25);Assert.IsTrue(w.Groups.Any(g=>(int)g.Prefab.Definition.Kind>=3));}finally{UnityEngine.Object.DestroyImmediate(w);}}}
  [TestCase(0,100)] [TestCase(.35f,65)] [TestCase(.2f,80)]
  public void ArmorMitigatesDamageAndExtremeHitsStillKill(float armor,int expected){var go=new GameObject("Armor rule");try{var health=go.AddComponent<EnemyHealth>();health.ConfigureArmor(armor);health.Initialize(1000);health.TakeDefenseDamage(100);Assert.AreEqual(1000-expected,health.CurrentHealth);health.TakeDefenseDamage(int.MaxValue);Assert.IsFalse(health.IsAlive);Assert.IsTrue(health.KilledByDefense);}finally{UnityEngine.Object.DestroyImmediate(go);}} }
}
