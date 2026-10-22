using System;
using System.Collections.Generic;
using NUnit.Framework;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Tests
{
    public sealed class GridAndPathTests
    {
        [Test]
        public void CellCentersRoundTripWithNonZeroOriginAndCellSize()
        {
            var grid = new BattlefieldGrid(4, 7, 1.5f, new Vector3(-7, 2, 3));
            for (int y = 0; y < grid.Height; y++)
            for (int x = 0; x < grid.Width; x++)
            {
                var expected = new Vector2Int(x, y);
                Assert.That(grid.TryWorldToCell(grid.CellToWorld(expected), out var actual), Is.True);
                Assert.That(actual, Is.EqualTo(expected));
            }
        }

        [TestCase(-0.01f, 0.5f, false)]
        [TestCase(0f, 0f, true)]
        [TestCase(3.999f, 2.999f, true)]
        [TestCase(4f, 1f, false)]
        [TestCase(1f, 3f, false)]
        public void BoundsAreInclusiveAtMinimumAndExclusiveAtMaximum(float x, float y, bool valid)
        {
            var grid = new BattlefieldGrid(4, 3, 1, Vector3.zero);
            Assert.That(grid.TryWorldToCell(new Vector3(x, y, 100), out _), Is.EqualTo(valid));
        }

        [Test]
        public void InvalidCellAndCostAreRejected()
        {
            var grid = new BattlefieldGrid(3, 3, 1, Vector3.zero);
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetCell(new Vector2Int(3, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.CellToWorld(new Vector2Int(-1, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridCell(TerrainType.Ground, true, true, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridCell(TerrainType.Ground, true, true, float.NaN));
            Assert.Throws<ArgumentException>(() => grid.SetCell(Vector2Int.zero, default));
        }

        [Test]
        public void AStarProducesShortestAdjacentPathAndAvoidsBlockedCells()
        {
            var grid = new BattlefieldGrid(5, 5, 1, Vector3.zero);
            for (int y = 0; y < 4; y++) grid.SetCell(new Vector2Int(2, y), new GridCell(TerrainType.Blocked, false, false));
            var path = new List<Vector2Int>();
            Assert.That(new PathfindingService(grid).TryFindPath(new Vector2Int(0, 0), new Vector2Int(4, 0), path), Is.True);
            Assert.That(path.Count, Is.EqualTo(13));
            Assert.That(path[0], Is.EqualTo(new Vector2Int(0, 0)));
            Assert.That(path[path.Count - 1], Is.EqualTo(new Vector2Int(4, 0)));
            for (int i = 0; i < path.Count; i++)
            {
                Assert.That(grid.GetCell(path[i]).Walkable, Is.True);
                if (i > 0) Assert.That(Mathf.Abs(path[i].x - path[i - 1].x) + Mathf.Abs(path[i].y - path[i - 1].y), Is.EqualTo(1));
            }
        }

        [Test]
        public void AStarChoosesCheaperDetourAndUsesUpdatedGrid()
        {
            var grid = new BattlefieldGrid(5, 3, 1, Vector3.zero);
            var expensive = new Vector2Int(2, 1);
            grid.SetCell(expensive, new GridCell(TerrainType.Ground, true, true, 20));
            var paths = new PathfindingService(grid);
            var result = new List<Vector2Int>();
            Assert.That(paths.TryFindPath(new Vector2Int(0, 1), new Vector2Int(4, 1), result), Is.True);
            Assert.That(result, Has.No.Member(expensive));
            Assert.That(result.Count, Is.EqualTo(7));
            int changes = 0;
            int oldRevision = grid.Revision;
            grid.Changed += () => changes++;
            grid.SetCell(expensive, new GridCell(TerrainType.Ground, true, true));
            Assert.That(grid.Revision, Is.EqualTo(oldRevision + 1));
            Assert.That(changes, Is.EqualTo(1));
            Assert.That(paths.TryFindPath(new Vector2Int(0, 1), new Vector2Int(4, 1), result), Is.True);
            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result, Has.Member(expensive));
        }

        [Test]
        public void UnreachableOrInvalidEndpointsReturnEmptyPathAndSearchCanRecover()
        {
            var grid = new BattlefieldGrid(3, 3, 1, Vector3.zero);
            var paths = new PathfindingService(grid);
            var result = new List<Vector2Int> { Vector2Int.zero };
            for (int y = 0; y < 3; y++) grid.SetCell(new Vector2Int(1, y), new GridCell(TerrainType.Blocked, false, false));
            Assert.That(paths.TryFindPath(Vector2Int.zero, new Vector2Int(2, 0), result), Is.False);
            Assert.That(result, Is.Empty);
            Assert.That(paths.TryFindPath(new Vector2Int(-1, 0), Vector2Int.zero, result), Is.False);
            Assert.That(paths.TryFindPath(Vector2Int.zero, new Vector2Int(1, 0), result), Is.False);
            grid.SetCell(new Vector2Int(1, 0), new GridCell(TerrainType.Ground, true, true));
            Assert.That(paths.TryFindPath(Vector2Int.zero, new Vector2Int(2, 0), result), Is.True);
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void SameStartAndGoalProducesOneCell()
        {
            var grid = new BattlefieldGrid(2, 2, 1, Vector3.zero);
            var result = new List<Vector2Int>();
            Assert.That(new PathfindingService(grid).TryFindPath(Vector2Int.one, Vector2Int.one, result), Is.True);
            Assert.That(result, Is.EqualTo(new[] { Vector2Int.one }));
        }

        [Test]
        public void BattlefieldDefinitionMakesWaterBeachGroundAndNonBuildableBase()
        {
            var definition = ScriptableObject.CreateInstance<BattlefieldDefinition>();
            try
            {
                var grid = definition.CreateGrid(Vector3.zero);
                Assert.That(definition.Plane, Is.EqualTo(GameplayPlane.XY), "New battlefield definitions must default to top-down.");
                Assert.That(grid.GetCell(Vector2Int.zero).Terrain, Is.EqualTo(TerrainType.Water));
                Assert.That(grid.GetCell(new Vector2Int(definition.WaterBandCells, 0)).Terrain, Is.EqualTo(TerrainType.Beach));
                Assert.That(grid.GetCell(new Vector2Int(definition.WaterBandCells + definition.BeachBandCells, 0)).Buildable, Is.True);
                Assert.That(grid.GetCell(definition.BaseCell).Terrain, Is.EqualTo(TerrainType.Base));
                Assert.That(grid.GetCell(definition.BaseCell).Buildable, Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(definition); }
        }
    }
}
