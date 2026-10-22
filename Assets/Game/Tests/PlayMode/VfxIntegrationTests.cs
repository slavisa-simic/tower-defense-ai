using System.Collections;
using NUnit.Framework;
using ShorelineDefense.Base;
using ShorelineDefense.Building;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ShorelineDefense.Tests
{
    public sealed class VfxIntegrationTests
    {
        private BuildManager builder;
        private EnemySpawner spawner;
        private EnemyRegistry enemies;
        private VfxService effects;
        private CombatFeedbackHud hud;
        [UnitySetUp] public IEnumerator SetUp()
        {
            Time.timeScale = 0;
            yield return SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");
            yield return null;
            builder = Object.FindFirstObjectByType<BuildManager>();
            spawner = Object.FindFirstObjectByType<EnemySpawner>();
            enemies = Object.FindFirstObjectByType<EnemyRegistry>();
            effects = Object.FindFirstObjectByType<VfxService>();
            hud = Object.FindFirstObjectByType<CombatFeedbackHud>();
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            Time.timeScale = 1;
            var previous = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("VFX cleanup " + System.Guid.NewGuid());
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(previous);
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ReenabledObserversPayOnceAndDeathOutlivesTarget()
        {
            Assert.IsTrue(spawner.TrySpawn(spawner.DefaultPrefab, out var enemy));
            var art = enemy.GetComponent<EnemySpritePresentation>(); art.enabled = false; art.enabled = true;
            hud.enabled = false; hud.enabled = true;
            var body = enemy.transform.Find("Visual/Body").GetComponent<SpriteRenderer>();
            var material = body.sharedMaterial;
            int before = builder.Economy.Money;
            enemy.Health.TakeDefenseDamage(1);
            Assert.AreNotEqual(Color.white, body.color);
            Assert.AreSame(material, body.sharedMaterial);
            int reward = enemy.Definition.KillReward;
            enemy.Health.TakeDefenseDamage(100000);
            Assert.AreEqual(0, enemies.Count);
            Assert.AreEqual(before + reward, builder.Economy.Money);
            Assert.AreEqual("+" + reward + " RSD", hud.LastRewardText);
            Assert.AreEqual(1, hud.ActivePopups);
            Assert.AreEqual(1, effects.Pool(VfxChannel.Corpse).ActiveCount);
            yield return null;
            Assert.IsTrue(enemy == null || !enemy.gameObject.activeInHierarchy);
            Assert.AreEqual(1, effects.Pool(VfxChannel.Corpse).ActiveCount);
            effects.Pool(VfxChannel.Corpse).Tick(3);
            Assert.AreEqual(0, effects.Pool(VfxChannel.Corpse).ActiveCount);
            Assert.AreEqual(before + reward, builder.Economy.Money);
        }
        [UnityTest] public IEnumerator WaterExitStopsWakeAndEmitsOneSplash()
        {
            Assert.IsTrue(spawner.TrySpawn(spawner.DefaultPrefab, out var enemy));
            enemy.transform.position = builder.Battlefield.Grid.CellToWorld(new Vector2Int(3, 8));
            yield return null; yield return null;
            var art = enemy.GetComponent<EnemySpritePresentation>(); Assert.IsTrue(art.WakeVisible);
            int before = effects.Pool(VfxChannel.Burst).ActiveCount;
            enemy.transform.position = builder.Battlefield.Grid.CellToWorld(new Vector2Int(4, 8));
            yield return null; yield return null;
            Assert.IsFalse(art.WakeVisible);
            Assert.AreEqual(before + 1, effects.Pool(VfxChannel.Burst).ActiveCount);
            yield return null;
            Assert.AreEqual(before + 1, effects.Pool(VfxChannel.Burst).ActiveCount);
        }
        [UnityTest] public IEnumerator GameOverClearsPresentationAndRejectsFurtherEffects()
        {
            effects.Play(VfxId.BloodHit, Vector3.zero);
            effects.Play(VfxId.SmokeSmall, Vector3.zero);
            effects.Play(VfxId.BloodDecal, Vector3.zero);
            Assert.IsTrue(Object.FindFirstObjectByType<WaveManager>().TryStartWave());
            Assert.That(hud.BannerText, Does.Contain("INCOMING"));
            Object.FindFirstObjectByType<BaseHealth>().TakeDamage(100000);
            int money = builder.Economy.Money;
            foreach (VfxChannel channel in System.Enum.GetValues(typeof(VfxChannel)))
                Assert.AreEqual(0, effects.Pool(channel).ActiveCount);
            Assert.IsFalse(effects.CanPlay); Assert.IsNull(effects.Play(VfxId.ExplosionSmall,Vector3.zero));
            Assert.AreEqual(0, hud.ActivePopups);
            yield return null;
            Assert.AreEqual(money, builder.Economy.Money);
        }
    }
}
