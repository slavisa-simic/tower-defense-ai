using System;
using NUnit.Framework;
using ShorelineDefense.Combat;

namespace ShorelineDefense.Tests
{
    public sealed class HealthTests
    {
        [Test]
        public void DamageReducesHealthAndNotifiesObservers()
        {
            var health = new Health(100);
            int changes = 0;
            health.Changed += () => changes++;
            health.TakeDamage(10);
            Assert.That(health.CurrentHealth, Is.EqualTo(90));
            Assert.That(health.MaxHealth, Is.EqualTo(100));
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void LethalDamageClampsAtZeroAndDeathOnlyOccursOnce()
        {
            var health = new Health(100);
            int deaths = 0;
            int changes = 0;
            health.Died += () => deaths++;
            health.Changed += () => changes++;
            health.TakeDamage(int.MaxValue);
            health.TakeDamage(10);
            Assert.That(health.CurrentHealth, Is.Zero);
            Assert.That(health.IsAlive, Is.False);
            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void NonPositiveDamageDoesNotHealOrEmitEvents()
        {
            var health = new Health(100);
            int changes = 0;
            health.Changed += () => changes++;
            health.TakeDamage(-20);
            health.TakeDamage(0);
            Assert.That(health.CurrentHealth, Is.EqualTo(100));
            Assert.That(changes, Is.Zero);
        }

        [Test]
        public void HealthInstancesAreIndependentAndRequirePositiveMaximum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Health(0));
            var first = new Health(100);
            var second = new Health(100);
            first.TakeDamage(10);
            Assert.That(second.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void ReentrantDamageFromObserverDoesNotDuplicateDeath()
        {
            var health = new Health(100);
            int deaths = 0;
            health.Died += () => deaths++;
            health.Changed += () => { if (health.CurrentHealth == 90) health.TakeDamage(90); };
            health.TakeDamage(10);
            Assert.That(deaths, Is.EqualTo(1));
        }
    }
}
