using NUnit.Framework;
using ShorelineDefense.Combat;

namespace ShorelineDefense.Tests
{
    public sealed class StructureHealthTests
    {
        [Test] public void CapacityUpgradePreservesMissingHealthAndRepairIsIdempotent()
        {
            var hp=new Health(300);hp.TakeDamage(120);int events=0;hp.Changed+=()=>events++;
            hp.IncreaseMaximum(450);Assert.That(hp.CurrentHealth,Is.EqualTo(330));Assert.That(hp.MaxHealth,Is.EqualTo(450));
            hp.RestoreFull();Assert.That(hp.CurrentHealth,Is.EqualTo(450));hp.RestoreFull();Assert.That(events,Is.EqualTo(2));
        }
        [Test] public void RepairsAndCapacityChangesCannotResurrectDestroyedStructures()
        {
            var hp=new Health(100);hp.TakeDamage(100);hp.RestoreFull();hp.IncreaseMaximum(150);
            Assert.That(hp.CurrentHealth,Is.Zero);Assert.That(hp.MaxHealth,Is.EqualTo(100));
        }
    }
}
