using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Grid;
using ShorelineDefense.Core;
using ShorelineDefense.Waves;
using ShorelineDefense.Enemies;
using ShorelineDefense.UI;

public static class Phase6AAuthor
{
    const string Original = "Assets/Game/Scenes/CoastalDefense_TopDown.unity";
    const string Port = "Assets/Game/Scenes/DanubeIndustrialPort.unity";
    const string Folder = "Assets/Game/Data/Maps/";
    static T Asset<T>(string name) where T : ScriptableObject
    {
        var path = Folder + name + ".asset";
        var a = AssetDatabase.LoadAssetAtPath<T>(path);
        if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a,path); }
        return a;
    }
    static T Find<T>() where T : UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>();
    public static string Run()
    {
        if (Application.isPlaying) throw new Exception("Edit Mode required.");
        System.IO.Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
        var scene = EditorSceneManager.OpenScene(Original);
        var river = Asset<MapDefinition>("DanubeBorder");
        river.id = MapCatalog.OriginalId; river.displayName = "DANUBE BORDER"; river.sceneName = "CoastalDefense_TopDown";
        river.description = "An open quarantine line along the Danube."; river.gameplayStyle = "OPEN FIELD DEFENSE — broad building freedom";
        river.battlefield = Find<GridManager>().Definition;
        var port = Asset<MapDefinition>("IndustrialPort");
        port.id = "industrial_port"; port.displayName = "DANUBE INDUSTRIAL PORT"; port.sceneName = "DanubeIndustrialPort";
        port.description = "Abandoned cargo docks converted into a quarantine checkpoint."; port.gameplayStyle = "CORRIDOR DEFENSE — split routes and cleared concrete pockets";
        var catalog = Asset<MapCatalog>("MapCatalog"); catalog.maps = new[] { river,port };
        foreach (var a in new ScriptableObject[] {river,port,catalog}) EditorUtility.SetDirty(a); AssetDatabase.SaveAssets();
        Context(river); Find<ProductMenuController>().ConfigureMaps(catalog);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Port) == null) AssetDatabase.CopyAsset(Original,Port);
        scene = EditorSceneManager.OpenScene(Port);
        var data = Asset<BattlefieldDefinition>("BattlefieldIndustrialPort");
        EditorUtility.CopySerialized(river.battlefield,data); data.name = "BattlefieldIndustrialPort";
        data.authoredWater = true;
        var water = new List<Vector2Int>(); var ground = new List<Vector2Int>(); var blocked = new List<Vector2Int>(); var noBuild = new List<Vector2Int>(); var flood = new List<Vector2Int>();
        bool Pier(int y) => y >= 2 && y <= 4 || y >= 7 && y <= 9 || y >= 13 && y <= 15;
        for (int x = 0; x < data.Width; x++) for (int y = 0; y < data.Height; y++)
        {
            var c = new Vector2Int(x,y);
            bool riverCell = x < 6 || y < 2 && x < 12;
            if (x >= 4 && Pier(y)) riverCell = false;
            if (riverCell) water.Add(c); else if (x < 6) { ground.Add(c); noBuild.Add(c); }
            if (!riverCell && x >= 6 && x < 9) flood.Add(c);
        }
        var blocks = new[] {
            new RectInt(10,2,5,3), new RectInt(18,2,5,3), new RectInt(24,1,3,4),
            new RectInt(9,7,4,4), new RectInt(16,7,5,3), new RectInt(24,7,3,4),
            new RectInt(11,13,5,3), new RectInt(19,13,4,3), new RectInt(24,14,3,3),
            new RectInt(7,16,3,2), new RectInt(28,0,4,3), new RectInt(28,7,4,3),new RectInt(28,15,4,3) };
        foreach (var rect in blocks) foreach (var c in rect.allPositionsWithin) blocked.Add(c);
        data.waterCells = water.ToArray(); data.groundCells = ground.ToArray(); data.blockedCells = blocked.Distinct().ToArray(); data.noBuildCells = noBuild.ToArray(); data.floodableCells = flood.ToArray();
        port.battlefield = data; EditorUtility.SetDirty(data); EditorUtility.SetDirty(port); AssetDatabase.SaveAssets(); Find<GridManager>().Configure(data);
        Context(port); Find<ProductMenuController>().ConfigureMaps(catalog);
        var grid = data.CreateGrid(Find<GridManager>().transform.position);
        var border = Find<BorderBreachService>(); border.Configure(Find<RunStatistics>(),Find<GameStateController>(),Find<GridManager>(),new[] {new Vector2Int(31,5),new Vector2Int(31,12)});
        var boats = Find<BoatLandingSystem>(); var so = new SerializedObject(boats); var prefab = (GameObject)so.FindProperty("boatPrefab").objectReferenceValue;
        float[] ys = {2.8f,4.1f,7.8f,9.1f,11.5f,13.8f,15.1f,17.1f};
        var points = ys.Select((y,i) => new RiverLandingPoint {
            riverEntry = grid.Origin + new Vector3(-5,y*data.CellSize,0),
            berth = grid.Origin + new Vector3((i==4 || i==7 ? 4.25f : 2.25f)*data.CellSize,y*data.CellSize,0),
            authoredSector = true, sector = i < 2 ? LandingSector.South : i < 5 ? LandingSector.Center : LandingSector.North,
            unloadingPositions = new[] { grid.CellToWorld(new Vector2Int(i==4 || i==7 ? 6 : 4,Mathf.FloorToInt(y))), grid.CellToWorld(new Vector2Int(i==7 ? 6 : i==4 ? 7 : 5,i==7 ? 16 : Mathf.FloorToInt(y))) }
        }).ToArray();
        boats.Configure(Find<EnemySpawner>(),Find<GameStateController>(),prefab,points);
        var spawner = Find<EnemySpawner>(); var sp = new SerializedObject(spawner); var spawnArray = sp.FindProperty("spawnPoints");
        for (int i = 0; i < spawnArray.arraySize; i++) ((Transform)spawnArray.GetArrayElementAtIndex(i).objectReferenceValue).position = points[i % points.Length].unloadingPositions[0];
        foreach (string name in new[] {"Top-Down Coastal Graybox","Danube Quarantine Dressing","Riverbank Vegetation","Final Border Checkpoint"})
        { var go = GameObject.Find(name); if (go != null) UnityEngine.Object.DestroyImmediate(go); }
        // The gameplay base root/components remain; its presentation will be replaced by checkpoint dressing.
        var baseRoot = GameObject.Find("Player Base"); foreach (var r in baseRoot.GetComponentsInChildren<SpriteRenderer>()) r.enabled = false;
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        scene = EditorSceneManager.OpenScene("Assets/Game/Scenes/MainMenu.unity"); Find<ProductMenuController>().ConfigureMaps(catalog); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);

        AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes = new[] {new EditorBuildSettingsScene("Assets/Game/Scenes/MainMenu.unity",true),new EditorBuildSettingsScene(Original,true),new EditorBuildSettingsScene(Port,true),new EditorBuildSettingsScene("Assets/Game/Scenes/CoastalDefense_Prototype.unity",true)};
        EditorSceneManager.OpenScene(Port);
        return "Two definitions, catalog, port masks/8 landings/2 exits, both gameplay contexts and all menu bindings saved.";
    }
    static void Context(MapDefinition definition)
    {
        var context = Find<MapRuntimeContext>() ?? new GameObject("Map Runtime Context").AddComponent<MapRuntimeContext>();
        context.definition = definition; context.grid = Find<GridManager>(); context.boats = Find<BoatLandingSystem>(); context.border = Find<BorderBreachService>(); EditorUtility.SetDirty(context);
    }
}
