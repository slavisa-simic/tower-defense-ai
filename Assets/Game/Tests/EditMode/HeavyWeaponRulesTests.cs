using NUnit.Framework;
using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Tests
{
    public sealed class HeavyWeaponRulesTests
    {
        [TestCase(-2, 0, true)]
        [TestCase(-2, .8f, true)]
        [TestCase(-2, 2, false)]
        [TestCase(2, 0, false)]
        [TestCase(-5, 0, false)]
        public void ConeUsesRangeAndAngle(float x, float y, bool inside)
        { Assert.That(FlameWeapon.InCone(new Vector2(x,y),Vector2.left,4,55),Is.EqualTo(inside)); }

        [Test] public void InvalidBehaviorDataRejectsDeadZoneAndNonfiniteTimings()
        {
            var data=ScriptableObject.CreateInstance<HeavyWeaponData>();
            try { Assert.IsTrue(data.IsValid(12));data.minimumRangeInCells=12;Assert.IsFalse(data.IsValid(12));data.minimumRangeInCells=3;data.travelTime=float.NaN;Assert.IsFalse(data.IsValid(12)); }
            finally { Object.DestroyImmediate(data); }
        }
    }
}
