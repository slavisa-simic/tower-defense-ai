using System.Collections.Generic;
using NUnit.Framework;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Presentation;
using UnityEditor;
using UnityEngine;

namespace ShorelineDefense.Tests
{
    public sealed class TopDownGridTests
    {
        private static BattlefieldDefinition TopDownDefinition => AssetDatabase.LoadAssetAtPath<BattlefieldDefinition>(
            "Assets/Game/Data/Game/Battlefield_TopDown.asset");

        [Test]
        public void XYConversionUsesYAndIgnoresPresentationDepth()
        {
            var grid = new BattlefieldGrid(8, 5, 1.25f, new Vector3(-5, -3, 7));
            Assert.That(grid.CellToWorld(new Vector2Int(2, 3)), Is.EqualTo(new Vector3(-1.875f, 1.375f, 7)));
            Assert.That(grid.TryWorldToCell(new Vector3(-1.875f, 1.375f, -123), out var cell), Is.True);
            Assert.That(cell, Is.EqualTo(new Vector2Int(2, 3)));
            Assert.That(grid.TryWorldToCell(new Vector3(5, 0, 7), out _), Is.False);
            Assert.That(grid.TryWorldToCell(new Vector3(0, 3.25f, 7), out _), Is.False);
        }

        [Test]
        public void LegacySerializedDefinitionKeepsOriginalXZMap()
        {
            var data = AssetDatabase.LoadAssetAtPath<BattlefieldDefinition>("Assets/Game/Data/Game/Battlefield.asset");
            Assert.That(data.Plane, Is.EqualTo(GameplayPlane.LegacyXZ));
            var grid = data.CreateGrid(new Vector3(2, 5, 9));
            Assert.That(grid.CellToWorld(Vector2Int.zero), Is.EqualTo(new Vector3(2.75f, 5, 9.75f)));
            Assert.That(grid.TryWorldToCell(new Vector3(2.75f, 100, 9.75f), out var cell), Is.True);
            Assert.That(cell, Is.EqualTo(Vector2Int.zero));
            Assert.That(data.WaterBandCells, Is.EqualTo(4));
            Assert.That(data.BeachBandCells, Is.EqualTo(4));
        }

        [Test]
        public void EveryCoastRowHasAPathAcrossWaterBeachAndGroundToRightHandBase()
        {
            var data = TopDownDefinition;
            Assert.That(data, Is.Not.Null);
            Assert.That(data.Plane, Is.EqualTo(GameplayPlane.XY));
            var grid = data.CreateGrid(Vector3.zero);
            var paths = new PathfindingService(grid);
            var result = new List<Vector2Int>();
            int ground = 0;
            for (int y = 0; y < grid.Height; y++)
            {
                Assert.That(paths.TryFindPath(new Vector2Int(1, y), data.BaseCell, result), Is.True);
                var terrain = new HashSet<TerrainType>();
                foreach (var cell in result) terrain.Add(grid.GetCell(cell).Terrain);
                Assert.That(terrain, Is.SupersetOf(new[] { TerrainType.Water, TerrainType.Beach, TerrainType.Ground, TerrainType.Base }));
                Assert.That(result[result.Count - 1], Is.EqualTo(data.BaseCell));
                for (int x = 0; x < grid.Width; x++)
                {
                    var cell = grid.GetCell(new Vector2Int(x, y));
                    if (cell.Buildable) ground++;
                    if (x < data.WaterBandCells) Assert.That(cell.Terrain, Is.EqualTo(TerrainType.Water));
                    else if (x < data.WaterBandCells + data.BeachBandCells) Assert.That(cell.Terrain, Is.EqualTo(TerrainType.Beach));
                }
            }
            Assert.That(ground, Is.GreaterThan(grid.CellCount * 0.7f));
            Assert.That(data.BaseCell.x, Is.GreaterThan(grid.Width * 0.8f));
        }

        [TestCase(1920, 1080)]
        [TestCase(2560, 1440)]
        [TestCase(1280, 720)]
        [TestCase(1024, 768)]
        [TestCase(2560, 1080)]
        public void CameraFramesEntireBattlefieldAndFacesXYPlane(int width, int height)
        {
            var root = new GameObject("Camera framing test");
            root.SetActive(false);
            var cameraRoot = new GameObject("Test camera");
            cameraRoot.SetActive(false);
            try
            {
                var grid = root.AddComponent<GridManager>();
                grid.Configure(TopDownDefinition);
                root.transform.position = new Vector3(-20, -11.25f, 2);
                var camera = cameraRoot.AddComponent<Camera>();
                camera.aspect = (float)width / height;
                cameraRoot.AddComponent<TopDownCamera>().Configure(grid);
                Assert.That(camera.orthographic, Is.True);
                Assert.That(camera.transform.rotation, Is.EqualTo(Quaternion.identity));
                var min = camera.WorldToViewportPoint(root.transform.position);
                var max = camera.WorldToViewportPoint(root.transform.position + new Vector3(40, 22.5f, 0));
                Assert.That(min.x, Is.InRange(0f, 1f));
                Assert.That(min.y, Is.InRange(0f, 1f));
                Assert.That(max.x, Is.InRange(0f, 1f));
                Assert.That(max.y, Is.InRange(0f, 1f));
                Assert.That(min.z, Is.GreaterThan(camera.nearClipPlane));
                Assert.That(max.z, Is.LessThan(camera.farClipPlane));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(cameraRoot); }
        }

        [Test]
        public void XYMovementIsFrameIndependentThroughTurnsAndClampsAtAttackRange()
        {
            var grid = new BattlefieldGrid(6, 4, 1, new Vector3(0, 0, 3));
            grid.SetCell(new Vector2Int(2, 0), new GridCell(TerrainType.Blocked, false, false));
            var paths = new PathfindingService(grid);
            var longFrame = new GameObject("Long frame").AddComponent<EnemyMovement>();
            var shortFrames = new GameObject("Short frames").AddComponent<EnemyMovement>();
            try
            {
                foreach (var enemy in new[] { longFrame, shortFrames })
                {
                    enemy.transform.position = grid.CellToWorld(Vector2Int.zero) + Vector3.forward * 100;
                    enemy.Initialize(grid, paths, new Vector2Int(5, 2), 1.25f, 0.6f);
                }
                longFrame.Tick(4);
                for (int i = 0; i < 40; i++) shortFrames.Tick(0.1f);
                Assert.That(Vector3.Distance(longFrame.transform.position, shortFrames.transform.position), Is.LessThan(0.001f));
                longFrame.Tick(100);
                Assert.That(longFrame.ReachedDestination, Is.True);
                Assert.That(longFrame.transform.position.z, Is.EqualTo(3));
                Assert.That(Vector3.Distance(longFrame.transform.position, grid.CellToWorld(new Vector2Int(5, 2))), Is.EqualTo(0.6f).Within(0.001f));
                Assert.That(paths.Requests, Is.EqualTo(2), "Movement ticks must not run A* every frame.");
            }
            finally { Object.DestroyImmediate(longFrame.gameObject); Object.DestroyImmediate(shortFrames.gameObject); }
        }
    }
}
