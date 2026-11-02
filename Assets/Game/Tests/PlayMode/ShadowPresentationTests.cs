using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ShorelineDefense.Enemies;
using ShorelineDefense.Persistence;
using ShorelineDefense.Waves;
using Object = UnityEngine.Object;

namespace ShorelineDefense.Tests
{
    public sealed class ShadowPresentationTests
    {
        static T Find<T>() where T : Object => Object.FindFirstObjectByType<T>();
        static SpriteRenderer Shadow(EnemyController enemy) => enemy.GetComponentsInChildren<SpriteRenderer>(true).Single(r => r.name == "Contact Shadow");
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale = 1;
            yield return SceneManager.LoadSceneAsync("CoastalDefense_TopDown");
        }

        [UnityTest] public IEnumerator BothMapsHaveOneNeutralAnimatedShadowPerCreatureAndReuseRestoresIt()
        {
            foreach (string map in new[] { "CoastalDefense_TopDown", "DanubeIndustrialPort" })
            {
                RunSession.Pending = null;
                yield return SceneManager.LoadSceneAsync(map); yield return null;
                Time.timeScale = 0;
                var spawner = Find<EnemySpawner>(); var waves = Find<WaveManager>();
                var prefabs = waves.EndlessSettings.enemies.Concat(waves.EndlessSettings.bossEncounters.bosses).ToArray();
                var position = spawner.Battlefield.Grid.CellToWorld(new Vector2Int(8, 5));
                foreach (var prefab in prefabs)
                {
                    Assert.IsTrue(spawner.TrySpawnAt(prefab, position, out var enemy)); yield return null;
                    var body = enemy.transform.Find("Visual/Body").GetComponent<SpriteRenderer>();
                    var shadow = Shadow(enemy);
                    Assert.IsTrue(shadow.enabled, prefab.name);
                    Assert.AreSame(body.sprite, shadow.sprite, "Shadow follows the actual animation frame");
                    Assert.AreEqual("BorderDefenders/PixelShadow", shadow.sharedMaterial.shader.name);
                    Assert.IsTrue(shadow.sharedMaterial.shader.isSupported);
                    var offset = shadow.sharedMaterial.GetVector("_OffsetPixels");
                    Assert.That(offset.x, Is.InRange(4f, 12f), "Visible cast instead of a hidden under-body contact");
                    Assert.AreEqual(-offset.x * .75f, offset.y, .001f, "Global lower-right light direction");
                    var tone = shadow.sharedMaterial.GetColor("_Color");
                    Assert.That(tone.a, Is.InRange(.35f, .40f)); Assert.Greater(tone.r, 0);
                    enemy.transform.position += Vector3.right * .1f; yield return null;
                    Assert.AreSame(body.sprite, shadow.sprite);
                    enemy.Health.TakeDefenseDamage(int.MaxValue);
                    Assert.IsFalse(shadow.enabled, "Death immediately hides the shadow"); yield return null;
                    Assert.IsTrue(spawner.TrySpawnAt(prefab, position, out var reused)); yield return null;
                    Assert.AreSame(enemy, reused, "Exercise the actual pool");
                    Assert.IsTrue(Shadow(reused).enabled, "Pooled activation restores land contact");
                    reused.Health.TakeDefenseDamage(int.MaxValue); yield return null;
                }
            }
        }

        [UnityTest] public IEnumerator LandContactDisappearsInWaterAndWhileBurrowedThenReturnsOnLand()
        {
            foreach (string map in new[] { "CoastalDefense_TopDown", "DanubeIndustrialPort" })
            {
                RunSession.Pending = null;
                yield return SceneManager.LoadSceneAsync(map); yield return null; Time.timeScale = 0;
                var spawner = Find<EnemySpawner>();
                var prefab = Find<WaveManager>().EndlessSettings.enemies.Single(e => e.Definition.Kind == EnemyKind.Burrower);
                var land = spawner.Battlefield.Grid.CellToWorld(new Vector2Int(8, 5));
                Assert.IsTrue(spawner.TrySpawnAt(prefab, land, out var enemy)); yield return null;
                Assert.IsTrue(Shadow(enemy).enabled);
                enemy.transform.position = spawner.Battlefield.Grid.CellToWorld(new Vector2Int(1, 5)); yield return null;
                Assert.IsFalse(Shadow(enemy).enabled, "Water must not show land contact");
                enemy.transform.position = land; yield return null; Assert.IsTrue(Shadow(enemy).enabled);
                enemy.GetComponent<BurrowerTraversal>().Move(1.1f); yield return null;
                Assert.IsTrue(enemy.IsBurrowed); Assert.IsFalse(Shadow(enemy).enabled, "No surface body shadow underground");
            }
        }
    }
}
