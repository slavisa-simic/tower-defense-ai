using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using ShorelineDefense.Core;
namespace ShorelineDefense.Tests
{
    public sealed class AssaultPoolTests
    {
        [UnityTest] public IEnumerator ReusedEnemyResetsHealthBurnPresentationRegistrationAndBreach()
        {
            Time.timeScale=0;yield return SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");yield return null;
            var spawn=Object.FindFirstObjectByType<EnemySpawner>();var registry=Object.FindFirstObjectByType<EnemyRegistry>();
            var waves=Object.FindFirstObjectByType<WaveManager>();var border=Object.FindFirstObjectByType<BorderBreachService>();border.ConfigureThreshold(0);
            spawn.ConfigureWave(400,3,1,4);var point=spawn.Battlefield.Grid.CellToWorld(new Vector2Int(27,4));
            Assert.IsTrue(spawn.TrySpawnAt(spawn.DefaultPrefab,point,out var first));Assert.AreEqual(spawn.DefaultPrefab.Definition.MaxHealth*3,first.Health.MaxHealth);
            var burn=first.GetComponent<EnemyDamageOverTime>();burn.ApplyBurn(10,10);first.Health.TakeDefenseDamage(int.MaxValue);yield return null;
            Assert.AreEqual(0,registry.Count);Assert.AreEqual(1,spawn.PooledEnemies);
            Assert.IsTrue(spawn.TrySpawnAt(spawn.DefaultPrefab,point,out var second));Assert.AreSame(first,second);
            Assert.IsTrue(second.IsTargetable);Assert.IsFalse(burn.Burning);Assert.IsFalse(second.Health.KilledByDefense);
            Assert.IsTrue(second.transform.Find("Visual/Body").GetComponent<SpriteRenderer>().enabled);Assert.AreEqual(1,registry.Count);
            second.Tick(100);Assert.IsFalse(border.TryBreach(second));yield return null;Assert.AreEqual(0,registry.Count);
            foreach(var boss in waves.EndlessSettings.bossEncounters.bosses) {
                Assert.IsTrue(spawn.TrySpawnAt(boss,point,out var enemy));Assert.AreEqual(boss.Definition.MaxHealth*4,enemy.Health.MaxHealth);
                enemy.Health.TakeDefenseDamage(int.MaxValue);yield return null;
            }
            Time.timeScale=1;
        }
    }
}
