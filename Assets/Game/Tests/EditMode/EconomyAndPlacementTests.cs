using NUnit.Framework;
using ShorelineDefense.Building;
using ShorelineDefense.Economy;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Tests
{
    public sealed class EconomyAndPlacementTests
    {
        [Test]
        public void BrokenObserverCannotTurnCommittedPaymentIntoFailure()
        {
            var economy = new GameObject("Observer test").AddComponent<EconomyManager>();
            try
            {
                int observed = -1;
                economy.MoneyChanged += _ => throw new System.InvalidOperationException("Expected observer failure");
                economy.MoneyChanged += amount => observed = amount;
                UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, "InvalidOperationException: Expected observer failure");
                Assert.That(economy.TrySpend(150), Is.True);
                Assert.That(economy.Money, Is.EqualTo(350));
                Assert.That(observed, Is.EqualTo(350));
            }
            finally { Object.DestroyImmediate(economy.gameObject); }
        }

        [Test]
        public void EconomyOwnsBalanceAndEmitsOnlyRealChanges()
        {
            var economy = new GameObject("Economy test").AddComponent<EconomyManager>();
            try
            {
                int changes = 0;
                int observed = -1;
                economy.MoneyChanged += amount => { changes++; observed = amount; };
                Assert.That(economy.Money, Is.EqualTo(500));
                Assert.That(economy.CanAfford(150), Is.True);
                Assert.That(economy.TrySpend(150), Is.True);
                Assert.That(observed, Is.EqualTo(350));
                Assert.That(economy.TrySpend(351), Is.False);
                Assert.That(economy.TrySpend(-1), Is.False);
                Assert.That(economy.Money, Is.EqualTo(350));
                economy.AddMoney(25);
                Assert.That(economy.Money, Is.EqualTo(375));
                economy.AddMoney(-25);
                economy.AddMoney(0);
                economy.TrySpend(0);
                Assert.That(changes, Is.EqualTo(2));
                Assert.That(observed, Is.EqualTo(375));
                economy.AddMoney(int.MaxValue);
                Assert.That(economy.Money, Is.EqualTo(int.MaxValue), "Income must never overflow into negative money.");
            }
            finally { Object.DestroyImmediate(economy.gameObject); }
        }

        [TestCase(TerrainType.Water, false, PlacementResult.NotBuildable)]
        [TestCase(TerrainType.Beach, false, PlacementResult.NotBuildable)]
        [TestCase(TerrainType.Base, false, PlacementResult.NotBuildable)]
        [TestCase(TerrainType.Blocked, false, PlacementResult.NotBuildable)]
        [TestCase(TerrainType.Ground, false, PlacementResult.NotBuildable)]
        [TestCase(TerrainType.Ground, true, PlacementResult.Valid)]
        public void PlacementHonorsTerrainAndBuildability(TerrainType terrain, bool buildable, PlacementResult expected)
        {
            var grid = new BattlefieldGrid(4, 4, 1, Vector3.zero);
            grid.SetCell(Vector2Int.one, new GridCell(terrain, true, buildable));
            Assert.That(PlacementRules.Validate(grid, Vector2Int.one, Vector2Int.one, 150, 500), Is.EqualTo(expected));
        }

        [Test]
        public void MultiCellOccupancyIsAtomicAndOnlyItsOwnerCanReleaseIt()
        {
            var grid = new BattlefieldGrid(5, 5, 1, Vector3.zero);
            var owner = new object();
            var other = new object();
            int navigation = 0;
            int occupancy = 0;
            grid.Changed += () => navigation++;
            grid.OccupancyChanged += () => occupancy++;
            var footprint = new Vector2Int(2, 3);
            Assert.That(grid.TryOccupy(Vector2Int.one, footprint, owner), Is.True);
            Assert.That(grid.TryOccupy(new Vector2Int(0, 2), new Vector2Int(2, 2), other), Is.False);
            Assert.That(grid.OccupantAt(new Vector2Int(0, 2)), Is.Null, "A rejected footprint must not partially occupy cells.");
            Assert.That(PlacementRules.Validate(grid, new Vector2Int(2, 3), Vector2Int.one, 150, 500), Is.EqualTo(PlacementResult.Occupied));
            grid.ClearOccupied(Vector2Int.one, footprint, other);
            Assert.That(grid.CanOccupy(Vector2Int.one, footprint), Is.False);
            grid.ClearOccupied(Vector2Int.one, footprint, owner);
            grid.ClearOccupied(Vector2Int.one, footprint, owner);
            Assert.That(grid.CanOccupy(Vector2Int.one, footprint), Is.True);
            Assert.That(occupancy, Is.EqualTo(2));
            Assert.That(navigation, Is.Zero, "Non-blocking structures must not invalidate A* routes.");
        }

        [Test]
        public void EntireFootprintMustFitAndBeBuildableAndAffordable()
        {
            var grid = new BattlefieldGrid(5, 5, 1.25f, new Vector3(-4, -3, 8));
            Assert.That(PlacementRules.Validate(grid, new Vector2Int(-1, 0), Vector2Int.one, 150, 500), Is.EqualTo(PlacementResult.OutsideGrid));
            Assert.That(PlacementRules.Validate(grid, new Vector2Int(4, 4), new Vector2Int(2, 2), 150, 500), Is.EqualTo(PlacementResult.OutsideGrid));
            Assert.That(PlacementRules.Validate(grid, Vector2Int.zero, Vector2Int.zero, 150, 500), Is.EqualTo(PlacementResult.OutsideGrid));
            Assert.That(PlacementRules.Validate(grid, Vector2Int.one, new Vector2Int(2, 3), 150, 149), Is.EqualTo(PlacementResult.InsufficientMoney));
            grid.SetCell(new Vector2Int(2, 3), new GridCell(TerrainType.Beach, true, false));
            Assert.That(PlacementRules.Validate(grid, Vector2Int.one, new Vector2Int(2, 3), 150, 500), Is.EqualTo(PlacementResult.NotBuildable));
            Assert.That(grid.FootprintCenter(Vector2Int.one, new Vector2Int(2, 3)), Is.EqualTo(new Vector3(-1.5f, 0.125f, 8)));
        }

        [Test]
        public void OccupiedNonBlockingCellStillHasAValidPath()
        {
            var grid = new BattlefieldGrid(3, 1, 1, Vector3.zero);
            grid.TryOccupy(new Vector2Int(1, 0), Vector2Int.one, new object());
            var path = new System.Collections.Generic.List<Vector2Int>();
            Assert.That(new PathfindingService(grid).TryFindPath(Vector2Int.zero, new Vector2Int(2, 0), path), Is.True);
        }
    }
}
