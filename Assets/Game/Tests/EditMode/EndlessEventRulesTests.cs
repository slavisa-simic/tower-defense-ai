using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using EventModifiers = ShorelineDefense.Waves.EventModifiers;

namespace ShorelineDefense.Tests
{
    public sealed class EndlessEventRulesTests
    {
        private EndlessWaveSettings Waves => AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
        private EndlessEventSettings Settings => Waves.eventSettings;
        [Test] public void CatalogIsCompleteAndEligibleSelectionIsSeededAndNeverStacks()
        {
            Assert.That(Settings.events.Length, Is.EqualTo(13));
            Assert.That(Settings.events.Select(e => e.id).Distinct().Count(), Is.EqualTo(13));
            for (int seed = 0; seed < 100; seed++)
            {
                var a = new EndlessEventManager(Settings, seed); var b = new EndlessEventManager(Settings, seed);
                for (int wave = 1; wave <= 50; wave++)
                {
                    var e = a.Select(wave, false); Assert.That(b.Select(wave, false), Is.SameAs(e));
                    if (wave < 5) Assert.That(e, Is.Null);
                    if (e == null) continue;
                    Assert.That(e.Eligible(wave, false), Is.True); Assert.That(e.uiIcon, Is.Not.Null);
                    Assert.That(a.Select(wave, false, e), Is.Null);
                }
                Assert.That(a.Select(25, true), Is.Null);
            }
        }
        [Test] public void NaturalPacingRespectsGapCooldownRecentExclusionAndBosses()
        {
            var manager = new EndlessEventManager(Settings, 1977);
            for (int wave = 1; wave <= 150; wave++) { manager.Prepare(wave, wave % 25 == 0, null); manager.BeginWave(); manager.CompleteWave(); }
            var h = manager.History;
            Assert.That(h.Count, Is.InRange(25, 65));
            for (int i = 0; i < h.Count; i++)
            {
                Assert.That(h[i].Wave % 25, Is.Not.Zero);
                var e = Settings.events.Single(d => d.id == h[i].Id); Assert.That(h[i].Wave, Is.GreaterThanOrEqualTo(e.minimumWave));
                if (i > 0) Assert.That(h[i].Wave - h[i - 1].Wave, Is.GreaterThanOrEqualTo(2));
                for (int n = 0; n < i; n++) if (h[n].Id == h[i].Id) Assert.That(h[i].Wave - h[n].Wave, Is.GreaterThanOrEqualTo(e.cooldown));
                for (int n = Mathf.Max(0, i - Settings.recentExclusion); n < i; n++) Assert.That(h[i].Id, Is.Not.EqualTo(h[n].Id));
            }
        }
        [Test] public void CategoryWeightsProduceChallengeReliefAndPatternVariety()
        {
            int[] counts = new int[3];
            for (int seed = 0; seed < 1000; seed++) counts[(int)new EndlessEventManager(Settings, seed).Select(30, false).category]++;
            Assert.That(counts[0], Is.InRange(620, 780)); Assert.That(counts[1], Is.InRange(140, 260)); Assert.That(counts[2], Is.InRange(60, 150));
        }
        [Test] public void CompatibilityIsSymmetricAndMaximumWaveIsHonored()
        {
            var a = ScriptableObject.CreateInstance<EndlessEventDefinition>(); var b = ScriptableObject.CreateInstance<EndlessEventDefinition>();
            try { a.id = "a"; b.id = "b"; a.incompatibleEvents = new[] { "b" }; a.maximumWave = 10;
                Assert.IsFalse(a.Compatible(b)); Assert.IsFalse(b.Compatible(a)); Assert.IsFalse(a.Compatible(a));
                Assert.IsTrue(a.Eligible(10, false)); Assert.IsFalse(a.Eligible(11, false)); Assert.IsFalse(a.Eligible(4, false));
            } finally { Object.DestroyImmediate(a); Object.DestroyImmediate(b); }
        }
        [TestCase(EventDuration.NextWave)] [TestCase(EventDuration.EntireWave)] [TestCase(EventDuration.Timed)]
        [TestCase(EventDuration.Preparation)] [TestCase(EventDuration.MultiWave)]
        public void AllDurationTypesCleanUpDeterministically(EventDuration duration)
        {
            var e = ScriptableObject.CreateInstance<EndlessEventDefinition>();
            try {
                e.id = "fixture"; e.durationType = duration; e.duration = 2; e.modifiers.defenseRange = .8f;
                var manager = new EndlessEventManager(Settings, 1); manager.Prepare(20, false, null); Assert.IsTrue(manager.Force(e, 20, false, null));
                manager.Tick(0); manager.BeginWave();
                if (duration == EventDuration.Preparation) Assert.That(manager.Current, Is.Null);
                else {
                    Assert.That(manager.Modifiers.defenseRange, Is.EqualTo(.8f));
                    if (duration == EventDuration.Timed) { manager.Tick(0); Assert.That(manager.Current.SecondsRemaining, Is.EqualTo(2)); manager.Tick(2); Assert.That(manager.Current, Is.Null); }
                    else { manager.CompleteWave(); Assert.That(manager.Modifiers.defenseRange, Is.EqualTo(1));
                        if (duration == EventDuration.MultiWave) { manager.Prepare(21, false, null); manager.BeginWave(); Assert.That(manager.Modifiers.defenseRange, Is.EqualTo(.8f)); manager.CompleteWave(); }
                        Assert.That(manager.Current, Is.Null);
                    }
                }
                manager.Cancel(); Assert.That(manager.Modifiers.defenseRange, Is.EqualTo(1));
            } finally { Object.DestroyImmediate(e); }
        }
        [Test] public void MassLandingChangesQuantityAndDirectorWithoutMutatingBaseData()
        {
            var s = Waves; var m = Settings.events.Single(e => e.id == "mass-landing").modifiers;
            var generator = new EndlessWaveGenerator(s, 1977); var normal = generator.Generate(20); var assault = generator.Generate(20, m);
            try {
                Assert.That(assault.ScheduledCount, Is.EqualTo(Mathf.RoundToInt(normal.ScheduledCount * 1.5f)));
                Assert.That(assault.HealthMultiplier, Is.EqualTo(normal.HealthMultiplier));
                var d = new AssaultPressureDirector(); d.Tick(.2f, s, 20, 0, 500, 0, 0, false, m);
                Assert.That(d.BoatLimit, Is.EqualTo(Mathf.CeilToInt(s.BoatConcurrency(20) * 1.5f)));
                Assert.That(d.Target, Is.EqualTo(Mathf.CeilToInt(s.PressureTarget(20) * 1.5f)));
                d.Reset(); d.Tick(.2f, s, 21, 0, 500, 0, 0, false);
                Assert.That(d.Target, Is.EqualTo(s.PressureTarget(21))); Assert.That(d.BoatLimit, Is.EqualTo(s.BoatConcurrency(21)));
            } finally { Object.DestroyImmediate(normal); Object.DestroyImmediate(assault); }
        }
        [TestCase("pirate-raid", EnemyKind.PirateRunner)] [TestCase("militant-assault", EnemyKind.IsisBreacher)]
        [TestCase("heavy-assault", EnemyKind.Brute)] [TestCase("runner-swarm", EnemyKind.Runner)]
        public void CompositionBiasChangesActualGeneratedRoster(string id, EnemyKind kind)
        {
            int baseline = 0, biased = 0;
            var m = Settings.events.Single(e => e.id == id).modifiers;
            for (int seed = 0; seed < 25; seed++) {
                var g = new EndlessWaveGenerator(Waves, seed); var a = g.Generate(24); var b = g.Generate(24, m);
                baseline += a.Groups.Where(x => x.Prefab.Definition.Kind == kind).Sum(x => x.Count);
                biased += b.Groups.Where(x => x.Prefab.Definition.Kind == kind).Sum(x => x.Count);
                Object.DestroyImmediate(a); Object.DestroyImmediate(b);
            }
            Assert.That(biased, Is.GreaterThan(baseline * 1.5f));
        }
    }
}
