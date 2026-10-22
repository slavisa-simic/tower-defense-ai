using System;
using System.Collections.Generic;
using NUnit.Framework;
using ShorelineDefense.Grid;
using ShorelineDefense.Persistence;
using UnityEngine;
using UnityEditor;

namespace ShorelineDefense.Tests
{
    public sealed class MapRulesTests
    {
        [Test] public void PortMasksReduceFreedomAndHaveNoUniversalSingleCellCut()
        {
            var map = AssetDatabase.LoadAssetAtPath<MapDefinition>("Assets/Game/Data/Maps/IndustrialPort.asset");
            var grid = map.battlefield.CreateGrid(Vector3.zero); var paths = new PathfindingService(grid); var route = new List<Vector2Int>();
            var starts = new[] {new Vector2Int(4,3),new Vector2Int(4,8),new Vector2Int(4,14)};
            var goals = new[] {new Vector2Int(31,5),new Vector2Int(31,12)};
            Assert.That(grid.GetCell(new Vector2Int(10,2)).Buildable,Is.False); Assert.That(grid.GetCell(new Vector2Int(10,2)).Walkable,Is.False);
            Assert.That(grid.GetCell(new Vector2Int(0,3)).Walkable,Is.False);
            foreach (var s in starts) Assert.That(paths.TryFindPath(s,goals[0],route) || paths.TryFindPath(s,goals[1],route),Is.True);
            // Removing any one buildable cell must not disconnect all route families.
            for (int x = 6; x < 31; x++) for (int y = 0; y < 18; y++)
            {
                var c = new Vector2Int(x,y); var original = grid.GetCell(c); if (!original.Buildable) continue;
                grid.SetCell(c,new GridCell(TerrainType.Blocked,false,false));
                bool reachable = false; foreach (var s in starts) foreach (var g in goals) reachable |= paths.TryFindPath(s,g,route);
                Assert.IsTrue(reachable,"Universal cut at " + c); grid.SetCell(c,original);
            }
        }
        [Test] public void VersionOneDefaultsOnlyToOriginalAndUnknownMapIsUnresolvable()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MapCatalog>("Assets/Game/Data/Maps/MapCatalog.asset");
            Assert.That(catalog.Find(null).id,Is.EqualTo("river_border")); Assert.IsNull(catalog.Find("unknown_map"));
            var d = new RunSaveData {runId = Guid.NewGuid().ToString(),wave=1,maxHomicides=1000,saveVersion=1,mapId=null};
            Assert.IsTrue(SaveGameService.Valid(d)); d.saveVersion=2; Assert.IsFalse(SaveGameService.Valid(d));
        }
    }
}
