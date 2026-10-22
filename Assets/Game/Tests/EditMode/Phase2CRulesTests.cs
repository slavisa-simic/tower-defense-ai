using System.Collections.Generic;
using NUnit.Framework;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Waves;
using UnityEditor;
using UnityEngine;

namespace ShorelineDefense.Tests
{
    public sealed class Phase2CRulesTests
    {
        [Test]
        public void RuntimeSlowSourcesStackReplaceAndRemoveIndependently()
        {
            var modifiers = new MovementModifiers();
            var a = new object(); var b = new object();
            modifiers.Set(a, 0.5f); modifiers.Set(b, 0.8f);
            Assert.That(modifiers.Multiplier, Is.EqualTo(0.4f).Within(0.0001));
            modifiers.Set(a, 0.6f);
            Assert.That(modifiers.Multiplier, Is.EqualTo(0.48f).Within(0.0001));
            modifiers.Remove(b); modifiers.Remove(b);
            Assert.That(modifiers.Multiplier, Is.EqualTo(0.6f));
            modifiers.Remove(a);
            Assert.That(modifiers.Multiplier, Is.EqualTo(1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => modifiers.Set(a, float.NaN));
        }

        [Test]
        public void WireChangesRouteCostWithoutBlockingAndRestoresOnlyItsOwnOverlay()
        {
            var grid = new BattlefieldGrid(5, 3, 1, Vector3.zero);
            var paths = new PathfindingService(grid);
            var route = new List<Vector2Int>();
            var anchor = new Vector2Int(1, 1); var footprint = new Vector2Int(3, 1); var owner = new object();
            grid.TryOccupy(anchor, footprint, owner);
            int revision = grid.Revision;
            Assert.That(grid.TrySetTraversalModifier(anchor, footprint, owner, 4, 0.5f), Is.True);
            Assert.That(grid.Revision, Is.EqualTo(revision + 1), "One footprint change is one navigation revision.");
            Assert.That(grid.GetCell(anchor).Walkable, Is.True);
            Assert.That(grid.CanOccupy(anchor, Vector2Int.one), Is.False);
            Assert.That(paths.TryFindPath(new Vector2Int(0, 1), new Vector2Int(4, 1), route), Is.True);
            Assert.That(route, Has.No.Member(new Vector2Int(2, 1)), "A* should take the cheaper detour.");
            grid.ClearTraversalModifier(anchor, footprint, new object());
            Assert.That(grid.GetCell(anchor).TraversalCost, Is.EqualTo(4));
            grid.SetCell(anchor, new GridCell(TerrainType.Ground, true, true, 3));
            grid.ClearTraversalModifier(anchor, footprint, owner);
            Assert.That(grid.GetCell(anchor).TraversalCost, Is.EqualTo(3), "Removal must preserve later terrain edits.");
            Assert.That(grid.MovementMultiplierAt(grid.CellToWorld(anchor)), Is.EqualTo(1));
        }

        [Test]
        public void MovementEntersCrossesAndLeavesAdjacentWireEvenInLongFrames()
        {
            var grid = new BattlefieldGrid(8, 1, 1, Vector3.zero);
            var owner = new object(); var anchor = new Vector2Int(2, 0); var footprint = new Vector2Int(2, 1);
            grid.TryOccupy(anchor, footprint, owner);
            grid.TrySetTraversalModifier(anchor, footprint, owner, 2, 0.5f);
            var paths = new PathfindingService(grid);
            var movement = new GameObject("Slow traversal").AddComponent<EnemyMovement>();
            var smallTicks = new GameObject("Small ticks").AddComponent<EnemyMovement>();
            try
            {
                foreach (var item in new[] { movement, smallTicks })
                {
                    item.transform.position = grid.CellToWorld(Vector2Int.zero);
                    item.Initialize(grid, paths, new Vector2Int(7, 0), 2, 0);
                }
                movement.Tick(1);
                Assert.That(movement.transform.position.x, Is.EqualTo(2.25f).Within(0.001));
                Assert.That(movement.CurrentSpeed, Is.EqualTo(1));
                var status = new object(); movement.Modifiers.Set(status, 0.5f);
                Assert.That(movement.CurrentSpeed, Is.EqualTo(0.5f));
                movement.Modifiers.Remove(status);
                movement.Tick(2);
                for (int i = 0; i < 300; i++) smallTicks.Tick(0.01f);
                Assert.That(movement.transform.position.x, Is.EqualTo(4.5f).Within(0.001));
                Assert.That(Vector3.Distance(movement.transform.position, smallTicks.transform.position), Is.LessThan(0.001));
                Assert.That(movement.CurrentSpeed, Is.EqualTo(2));
                Assert.That(movement.BaseSpeed, Is.EqualTo(2));
                Assert.That(paths.Requests, Is.EqualTo(2));
            }
            finally { Object.DestroyImmediate(movement.gameObject); Object.DestroyImmediate(smallTicks.gameObject); }
        }

        [Test]
        public void RouteProgressOrdersDetoursInsteadOfStraightLineDistance()
        {
            var grid = new BattlefieldGrid(7, 5, 1, Vector3.zero);
            for (int y = 0; y < 4; y++) grid.SetCell(new Vector2Int(5, y), new GridCell(TerrainType.Blocked, false, false));
            var paths = new PathfindingService(grid);
            var near = new GameObject("Near but detoured").AddComponent<EnemyMovement>();
            var advanced = new GameObject("Longer straight distance but shorter route").AddComponent<EnemyMovement>();
            try
            {
                var goal = new Vector2Int(6, 2);
                near.transform.position = grid.CellToWorld(new Vector2Int(4, 2));
                advanced.transform.position = grid.CellToWorld(new Vector2Int(3, 4));
                near.Initialize(grid, paths, goal, 1, 0);
                advanced.Initialize(grid, paths, goal, 1, 0);
                Assert.That(Vector3.Distance(near.transform.position, grid.CellToWorld(goal)), Is.LessThan(Vector3.Distance(advanced.transform.position, grid.CellToWorld(goal))));
                Assert.That(near.RemainingPathDistance, Is.GreaterThan(advanced.RemainingPathDistance));
                float previous = advanced.RemainingPathDistance;
                advanced.Tick(0.5f);
                Assert.That(advanced.RemainingPathDistance, Is.EqualTo(previous - 0.5f).Within(0.001));
                grid.SetCell(new Vector2Int(5, 2), new GridCell(TerrainType.Ground, true, true));
                near.Tick(0.01f);
                Assert.That(near.RemainingPathDistance, Is.LessThan(advanced.RemainingPathDistance));
            }
            finally { Object.DestroyImmediate(near.gameObject); Object.DestroyImmediate(advanced.gameObject); }
        }

        [Test]
        public void PersistedCatalogAndWavesHaveTheRequestedPrototypeValues()
        {
            string[] names = { "MachineGunNest", "BarbedWire", "LandMine", "SniperTower" };
            int[] costs = { 150, 50, 75, 300 };
            for (int i = 0; i < names.Length; i++)
            {
                var data = AssetDatabase.LoadAssetAtPath<DefenseDefinition>($"Assets/Game/Data/Defenses/{names[i]}.asset");
                Assert.That(data.IsValid, Is.True, names[i]);
                Assert.That(data.Cost, Is.EqualTo(costs[i]));
                Assert.That(data.Footprint, Is.EqualTo(Vector2Int.one));
            }
            var sniper = AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/SniperTower.asset");
            Assert.That(sniper.Targeting, Is.EqualTo(TargetingMode.ClosestToBase));
            Assert.That(sniper.RangeInCells, Is.InRange(10, 12));
            Assert.That(sniper.Damage, Is.EqualTo(100));
            Assert.That(sniper.FireRate, Is.EqualTo(0.5f));
            for (int i = 1; i <= 5; i++)
            {
                var wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>($"Assets/Game/Data/Waves/Wave{i}.asset");
                Assert.That(wave.IsValid, Is.True);
                Assert.That(wave.WaveNumber, Is.EqualTo(i));
                Assert.That(wave.ScheduledCount, Is.EqualTo(5 + 5 * i));
                Assert.That(wave.CompletionReward, Is.EqualTo(100));
            }
        }
    }
}
