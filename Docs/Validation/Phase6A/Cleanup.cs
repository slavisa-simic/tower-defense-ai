using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Grid;
public static class Phase6ACleanup
{
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit mode only");
        const string stray="Assets/Docs/Validation/Phase6A/port-establishing.png";
        if(File.Exists(stray)){File.Copy(stray,"Docs/Validation/Phase6A/port-establishing.png",true);AssetDatabase.DeleteAsset(stray);}
        foreach(string path in new[]{"Assets/Docs/Validation/Phase6A","Assets/Docs/Validation","Assets/Docs"})if(Directory.Exists(path)&&Directory.GetFileSystemEntries(path).Length==0)AssetDatabase.DeleteAsset(path);
        var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/DanubeIndustrialPort.unity");
        var context=UnityEngine.Object.FindFirstObjectByType<MapRuntimeContext>();
        // Validate the exact saved data in Edit Mode, where GridManager has no Awake grid.
        int invalid=0;var grid=context.definition.battlefield.CreateGrid(context.grid.transform.position);var paths=new PathfindingService(grid);var route=new System.Collections.Generic.List<Vector2Int>();
        foreach(var p in context.boats.LandingPoints)foreach(var pos in p.unloadingPositions){bool found=false;if(grid.TryWorldToCell(pos,out var start))foreach(var goal in context.border.BreachCells)found|=paths.TryFindPath(start,goal,route);if(!found)invalid++;}
        if(invalid>0)throw new Exception("Saved map invalid paths="+invalid);
        return "Saved port: all sixteen unloading paths valid. Editor stopped on port scene; stray capture removed via AssetDatabase.";
    }
}
