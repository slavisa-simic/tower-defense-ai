using NUnit.Framework;
using ShorelineDefense.Presentation;
using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Tests
{
    public sealed class VfxPoolTests
    {
        private GameObject root;
        private Texture2D texture;
        private Sprite sprite;
        private Sprite[] frames;
        [SetUp] public void SetUp()
        {
            root = new GameObject("VFX test"); texture = new Texture2D(4, 4);
            sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.one * .5f, 4);
            frames = new[] { sprite };
        }
        [TearDown] public void TearDown()
        { Object.DestroyImmediate(root); Object.DestroyImmediate(sprite); Object.DestroyImmediate(texture); }
        [Test] public void CapacityRecyclesOldestWithoutCreatingMoreRenderers()
        {
            var pool = new SpriteEffectPool(root.transform, "Test", 2, null);
            var first = pool.Play(frames, 10, Vector3.zero, Quaternion.identity, Vector3.one, "Effects", 2, 1, 1, 0, Vector2.zero);
            pool.Play(frames, 10, Vector3.right, Quaternion.identity, Vector3.one, "Effects", 2, 1, 1, 0, Vector2.zero);
            var replacement = pool.Play(frames, 10, Vector3.up, Quaternion.identity, Vector3.one, "Effects", 2, 1, 1, 0, Vector2.zero);
            Assert.AreSame(first, replacement); Assert.AreEqual(2, pool.ActiveCount);
            Assert.AreEqual(2, root.GetComponentsInChildren<SpriteRenderer>().Length); Assert.AreEqual(1, pool.RecycledCount);
        }
        [Test] public void ReuseResetsFadeFollowTransformAndSorting()
        {
            var pool = new SpriteEffectPool(root.transform, "Test", 1, null);
            var follower = new GameObject("Follow"); follower.transform.SetParent(root.transform); follower.transform.position = Vector3.one * 4;
            var renderer = pool.Play(frames, 10, Vector3.zero, Quaternion.Euler(0,0,70), Vector3.one*3, "Effects", 8, 1, .4f, 1, Vector2.one, follower.transform);
            pool.Tick(.8f); Assert.Less(renderer.color.a, .1f); Assert.AreEqual(follower.transform.position, renderer.transform.position);
            pool.Play(frames, 10, Vector3.left, Quaternion.identity, Vector3.one, "GroundObjects", 0, 1, 1, 0, Vector2.zero);
            pool.Tick(.1f); Assert.AreEqual(Vector3.left, renderer.transform.position); Assert.AreEqual(1, renderer.color.a);
            Assert.AreEqual(Vector3.one, renderer.transform.localScale); Assert.AreEqual(Quaternion.identity, renderer.transform.rotation);
            Assert.AreEqual("GroundObjects", renderer.sortingLayerName); Assert.AreEqual(0, renderer.sortingOrder);
            pool.Tick(2); Assert.AreEqual(0, pool.ActiveCount); Assert.IsFalse(renderer.enabled); Assert.IsNull(renderer.sprite);
        }
        [Test] public void ClearReleasesEveryActiveEffectAndAllowsReuse()
        {
            var pool = new SpriteEffectPool(root.transform, "Test", 2, null);
            pool.Play(frames, 10, Vector3.zero, Quaternion.identity, Vector3.one, "Effects", 2, 1, 1, 0, Vector2.zero);
            pool.Clear(); pool.Clear(); Assert.AreEqual(0, pool.ActiveCount);
            pool.Play(frames, 10, Vector3.zero, Quaternion.identity, Vector3.one, "Effects", 2, 1, 1, 0, Vector2.zero);
            Assert.AreEqual(1, pool.ActiveCount);
        }
        [Test] public void DamageNotificationReportsCommittedDamageOnceAndIgnoresInvalidHits()
        {
            var health = root.AddComponent<EnemyHealth>(); health.Initialize(25);
            int damage = 0, deaths = 0, calls = 0;
            health.Damaged += amount => { damage += amount; calls++; Assert.Less(health.CurrentHealth,25); };
            health.Died += () => deaths++;
            health.TakeDamage(0); health.TakeDamage(-3); health.TakeDefenseDamage(10); health.TakeDefenseDamage(100); health.TakeDamage(10);
            Assert.AreEqual(25, damage); Assert.AreEqual(2,calls); Assert.AreEqual(1,deaths); Assert.IsTrue(health.KilledByDefense);
        }
    }
}
