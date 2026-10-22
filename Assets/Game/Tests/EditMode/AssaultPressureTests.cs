using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ShorelineDefense.Waves;
namespace ShorelineDefense.Tests
{
    public sealed class AssaultPressureTests
    {
        [TestCase(1,16)] [TestCase(5,38)] [TestCase(10,52)] [TestCase(20,87)] [TestCase(25,139)] [TestCase(50,289)]
        public void SeededQuantityIsFiveTimesBaselineExceptSingleBoss(int number, int baseline)
        {
            var settings = AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
            var wave = new EndlessWaveGenerator(settings,1977).Generate(number);
            try { Assert.AreEqual(baseline * 5 - (wave.Encounter == null ? 0 : 4), wave.ScheduledCount); }
            finally { Object.DestroyImmediate(wave); }
        }
        [Test] public void HealthFactorsDoNotStackForBosses()
        {
            var s = ScriptableObject.CreateInstance<EndlessWaveSettings>();
            try { s.globalEnemyHealthMultiplier=3;s.bossHealthMultiplier=4;Assert.AreEqual(6,s.RuntimeHealthMultiplier(2,false));Assert.AreEqual(8,s.RuntimeHealthMultiplier(2,true)); }
            finally { Object.DestroyImmediate(s); }
        }
        [Test] public void RouteCacheSharesResultsAndInvalidatesOnNavigationChange()
        {
            var grid = new ShorelineDefense.Grid.BattlefieldGrid(5,5,1,Vector3.zero);
            var navigation = new ShorelineDefense.Grid.PathfindingService(grid);
            var path = new System.Collections.Generic.List<Vector2Int>();var end = new Vector2Int(4,0);
            Assert.IsTrue(navigation.TryFindPath(Vector2Int.zero,end,path));int searches=navigation.Searches;
            path.Clear();Assert.IsTrue(navigation.TryFindPath(Vector2Int.zero,end,path));Assert.AreEqual(searches,navigation.Searches);Assert.AreEqual(1,navigation.CacheHits);
            grid.SetCell(new Vector2Int(2,0),new ShorelineDefense.Grid.GridCell(ShorelineDefense.Grid.TerrainType.Blocked,false,false));
            Assert.IsTrue(navigation.TryFindPath(Vector2Int.zero,end,path));Assert.AreEqual(searches+1,navigation.Searches);Assert.IsFalse(path.Contains(new Vector2Int(2,0)));
        }
        [Test] public void DirectorAcceleratesLowPopulationAndHoldsHysteresisAndAccountsForCargo()
        {
            var s = ScriptableObject.CreateInstance<EndlessWaveSettings>();
            try {
                s.earlyPressureTarget=100;var d=new AssaultPressureDirector();d.Reset();
                Assert.IsTrue(d.Tick(.2f,s,1,20,500,1,10,false));Assert.AreEqual(AssaultPressure.Accelerated,d.Mode);
                Assert.IsFalse(d.Tick(.01f,s,1,130,500,1,10,false));
                d.Tick(.2f,s,1,85,500,1,10,false);Assert.AreEqual(AssaultPressure.Accelerated,d.Mode);
                d.Tick(.2f,s,1,85,500,1,30,false);Assert.AreEqual(AssaultPressure.Normal,d.Mode);
                d.Tick(.2f,s,1,85,500,1,40,false);Assert.IsTrue(d.DeploymentPaused);
                d.Tick(.2f,s,1,130,500,1,0,false);Assert.AreEqual(AssaultPressure.Throttled,d.Mode);
                d.Tick(.2f,s,1,110,500,1,0,false);Assert.AreEqual(AssaultPressure.Throttled,d.Mode);
                d.Tick(.2f,s,1,100,500,1,0,false);Assert.AreEqual(AssaultPressure.Normal,d.Mode);
                d.Tick(.2f,s,1,0,10,1,10,false);Assert.AreEqual(AssaultPressure.Normal,d.Mode);
            } finally { Object.DestroyImmediate(s); }
        }
    }
}
