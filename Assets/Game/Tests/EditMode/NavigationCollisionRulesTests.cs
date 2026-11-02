using System.Collections.Generic;
using NUnit.Framework;
using ShorelineDefense.Grid;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using UnityEngine;
using UnityEditor;

namespace ShorelineDefense.Tests
{
    public sealed class NavigationCollisionRulesTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        [TearDown] public void Cleanup() { foreach (var go in objects) if (go != null) Object.DestroyImmediate(go); objects.Clear(); }
        private DefenseBase Defense(BattlefieldGrid grid, int x, int y)
        {
            var data = AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/MachineGunNest.asset");
            var go = new GameObject("Test wall"); objects.Add(go);
            var d = go.AddComponent<DefenseBase>(); d.Initialize(data,new Vector2Int(x,y),grid,null,null);
            Assert.IsTrue(grid.TryOccupy(d.Anchor,data.Footprint,d)); Assert.IsTrue(d.ActivateNavigation()); return d;
        }
        [Test] public void ContainerRectangleForcesDetourAndPathNeverContainsStaticCells()
        {
            var grid = new BattlefieldGrid(9,7,1,Vector3.zero);
            for (int x=3;x<6;x++) for(int y=2;y<5;y++) grid.SetCell(new Vector2Int(x,y),new GridCell(TerrainType.Blocked,false,false));
            var route = new List<Vector2Int>(); Assert.IsTrue(new PathfindingService(grid).TryFindPath(new Vector2Int(0,3),new Vector2Int(8,3),route));
            Assert.That(route.Count,Is.GreaterThan(9)); foreach(var cell in route) Assert.IsTrue(grid.IsTraversable(cell),cell.ToString());
        }
        [Test] public void NavigationOwnershipIsAtomicAndRemovalDoesNotOverwriteStaticTerrain()
        {
            var grid=new BattlefieldGrid(5,5,1,Vector3.zero); var owner=new object(); var other=new object(); var at=new Vector2Int(2,2);
            int revision=grid.Revision; Assert.IsTrue(grid.TryOccupy(at,Vector2Int.one,owner)); Assert.AreEqual(revision,grid.Revision);
            Assert.IsFalse(grid.TrySetNavigationBlocker(at,Vector2Int.one,other)); Assert.IsTrue(grid.TrySetNavigationBlocker(at,Vector2Int.one,owner));
            Assert.AreEqual(revision+1,grid.Revision); Assert.IsFalse(grid.IsTraversable(at));
            grid.ClearNavigationBlocker(at,Vector2Int.one,other); Assert.IsFalse(grid.IsTraversable(at));
            grid.SetCell(at,new GridCell(TerrainType.Blocked,false,false)); grid.ClearNavigationBlocker(at,Vector2Int.one,owner);
            Assert.IsFalse(grid.IsTraversable(at)); Assert.IsNull(grid.NavigationBlockerAt(at));
        }
        [Test] public void CompleteWallResolvesFirstUsefulDefenseAndSharesReverseField()
        {
            var grid=new BattlefieldGrid(9,5,1,Vector3.zero);
            var wall=new List<DefenseBase>(); for(int y=0;y<5;y++)wall.Add(Defense(grid,4,y));
            Defense(grid,1,4); var paths=new PathfindingService(grid); var route=new List<Vector2Int>(); var goals=new[]{new Vector2Int(8,2)};
            Assert.IsFalse(paths.TryFindPath(new Vector2Int(0,2),goals[0],route));
            Assert.IsTrue(paths.Obstructions.TryResolve(new Vector2Int(0,2),goals,route,out var target,out _));
            Assert.Contains(target,wall); Assert.AreEqual(4,target.Anchor.x);
            foreach(var cell in route) Assert.IsTrue(grid.IsTraversable(cell));
            Assert.AreEqual(3,route[route.Count-1].x);
            for(int y=0;y<5;y++) Assert.IsTrue(paths.Obstructions.CanReachByBreaching(new Vector2Int(0,y),goals[0]));
            Assert.AreEqual(1,paths.Obstructions.FieldBuilds);
        }
        [Test] public void PermanentWallHasNoAttackableFallback()
        {
            var grid=new BattlefieldGrid(9,5,1,Vector3.zero); for(int y=0;y<5;y++)grid.SetCell(new Vector2Int(4,y),new GridCell(TerrainType.Blocked,false,false));
            Defense(grid,2,4); var paths=new PathfindingService(grid); var route=new List<Vector2Int>();
            Assert.IsFalse(paths.Obstructions.TryResolve(new Vector2Int(0,2),new[]{new Vector2Int(8,2)},route,out var target,out _)); Assert.IsNull(target);
        }
        [Test] public void StaleLongMovementTickCannotCrossNewSolidFootprint()
        {
            var grid=new BattlefieldGrid(9,3,1,Vector3.zero); var paths=new PathfindingService(grid);
            var go=new GameObject("Movement contact");objects.Add(go);go.transform.position=grid.CellToWorld(new Vector2Int(0,1));
            var movement=go.AddComponent<EnemyMovement>();movement.Initialize(grid,paths,new Vector2Int(8,1),2,0);
            for(int i=0;i<PathfindingService.RepathsPerFrame;i++) paths.TryReserveRepath();
            for(int y=0;y<3;y++)Defense(grid,4,y);
            movement.Tick(100);Assert.Less(go.transform.position.x,4);Assert.IsTrue(grid.TryWorldToCell(go.transform.position,out var cell));Assert.IsTrue(grid.IsTraversable(cell));
            Assert.LessOrEqual(paths.PeakRepathsPerFrame,PathfindingService.RepathsPerFrame);
        }
        [TestCase("barbed-wire",false)] [TestCase("land-mine",false)]
        [TestCase("machine-gun-nest",true)] [TestCase("sniper-tower",true)] [TestCase("flamethrower",true)]
        [TestCase("mortar",true)] [TestCase("autocannon",true)] [TestCase("rocket",true)]
        public void DefenseNavigationPolicyIsAuthored(string id,bool blocking)
        {
            DefenseDefinition found=null;foreach(var guid in AssetDatabase.FindAssets("t:DefenseDefinition",new[]{"Assets/Game/Data/Defenses"}))
            {var d=AssetDatabase.LoadAssetAtPath<DefenseDefinition>(AssetDatabase.GUIDToAssetPath(guid));if(d.Id==id)found=d;}
            Assert.NotNull(found,id);Assert.AreEqual(blocking,found.BlocksMovement);
        }
    }
}
