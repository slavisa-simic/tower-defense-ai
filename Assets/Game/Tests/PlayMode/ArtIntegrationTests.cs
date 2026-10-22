using System.Collections;
using NUnit.Framework;
using ShorelineDefense.Building;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace ShorelineDefense.Tests
{
    public sealed class ArtIntegrationTests
    {
        [UnitySetUp] public IEnumerator SetUp()
        {
            Time.timeScale = 0;
            yield return SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");
            yield return null; yield return null;
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            Time.timeScale = 1;
            var previous = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(SceneManager.CreateScene("Art cleanup " + System.Guid.NewGuid()));
            yield return SceneManager.UnloadSceneAsync(previous);
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator HudReenableKeepsNamedMgBindingAndAllCellsAccessible()
        {
            var hud = Object.FindFirstObjectByType<BuildHud>();
            hud.enabled = false; hud.enabled = true;
            yield return null;
            var root = hud.GetComponent<UIDocument>().rootVisualElement;
            Assert.AreEqual(hud.Defenses.Count + 1, root.Query<VisualElement>(className: "build-icon").ToList().Count);
            var mg = root.Q<Button>("build-mg");
            using (var click = NavigationSubmitEvent.GetPooled()) { click.target = mg; mg.SendEvent(click); }
            var placement = Object.FindFirstObjectByType<PlacementController>();
            Assert.AreSame(hud.MachineGun, placement.Selected);
            Assert.IsTrue(mg.ClassListContains("selected"));
            var input = Object.FindFirstObjectByType<PlacementInput>();
            var grid = Object.FindFirstObjectByType<BuildManager>().Battlefield.Grid;
            for (int y = 0; y < grid.Height; y++) for (int x = 0; x < grid.Width; x++)
            {
                var cell = new Vector2Int(x, y); var screen = Camera.main.WorldToScreenPoint(grid.CellToWorld(cell));
                Assert.IsTrue(input.TryScreenToCell(screen, out var actual), "Cell must be on camera: " + cell);
                Assert.AreEqual(cell, actual); Assert.IsFalse(input.IsOverHud(screen), "HUD covers cell: " + cell);
            }
        }
        [UnityTest] public IEnumerator HitPoseEndsWithoutInterruptingAuthoritativeMovement()
        {
            var spawner = Object.FindFirstObjectByType<EnemySpawner>();
            Assert.IsTrue(spawner.TrySpawn(spawner.DefaultPrefab, out var enemy));
            var body = enemy.transform.Find("Visual/Body").GetComponent<SpriteRenderer>();
            enemy.Health.TakeDefenseDamage(1);
            yield return null;
            StringAssert.Contains("hit", body.sprite.name);
            var start = enemy.transform.position;
            enemy.Movement.Tick(.1f); yield return null;
            Assert.Greater((enemy.transform.position - start).sqrMagnitude, .0001f);
            Time.timeScale = 1; yield return new WaitForSeconds(.25f); Time.timeScale = 0;
            Assert.IsTrue(enemy.Health.IsAlive); Assert.IsTrue(enemy.IsTargetable);
            StringAssert.DoesNotContain("hit", body.sprite.name);
            Assert.AreEqual(Color.white, body.color);
        }
    }
}
