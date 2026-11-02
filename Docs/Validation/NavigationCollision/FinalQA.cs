using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Persistence;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using Object=UnityEngine.Object;

public static class NavigationFinalQA
{
    const string Folder="Docs/Validation/NavigationCollision/";
    static T Find<T>()where T:Object=>Object.FindFirstObjectByType<T>();
    public static string SavedAssets()
    {
        if(Application.isPlaying)throw new Exception("Edit mode required");var report=new List<string>();
        foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+map+".unity");
            int missing=scene.GetRootGameObjects().Sum(r=>r.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
            if(missing!=0)throw new Exception("Missing scripts "+map);
            var manager=Find<GridManager>();var grid=manager.Definition.CreateGrid(manager.transform.position);
            foreach(var f in manager.Definition.navigationObstacles)
            {
                var marker=Object.FindObjectsByType<StaticNavigationObstacle>(FindObjectsSortMode.None).Single(m=>m.obstacleId==f.id);
                var box=marker.GetComponent<BoxCollider2D>();
                if(box==null||!box.enabled||Vector3.Distance(marker.transform.position,grid.FootprintCenter(f.anchor,f.size))>.001f||Vector2.Distance(box.size,new Vector2(f.size.x,f.size.y)*grid.CellSize)>.001f)
                    throw new Exception("Collider/footprint mismatch "+map+" "+f.id);
            }
            report.Add(map+": zero missing scripts, all static colliders match authored cell centers/sizes.");
        }
        foreach(string guid in AssetDatabase.FindAssets("t:DefenseDefinition",new[]{"Assets/Game/Data/Defenses"}))
        {
            var d=AssetDatabase.LoadAssetAtPath<DefenseDefinition>(AssetDatabase.GUIDToAssetPath(guid));var box=d.Prefab.GetComponent<BoxCollider2D>();
            if(d.BlocksMovement&&(box==null||Vector2.Distance(box.size,new Vector2(d.Footprint.x,d.Footprint.y)*1.25f)>.001f))throw new Exception("Defense collider "+d.Id);
            if(!d.BlocksMovement&&box!=null&&box.enabled)throw new Exception("Walkable hazard has a solid collider "+d.Id);
        }
        if(Physics2D.GetIgnoreLayerCollision(10,8)||Physics2D.GetIgnoreLayerCollision(10,9)||!Physics2D.GetIgnoreLayerCollision(10,10))throw new Exception("Layer matrix");
        report.Add("Six solid defense colliders match footprints; Wire/Mine nonblocking; physics layer matrix correct.");
        // Display the existing grid debug Gizmos against the saved port geometry.
        Selection.activeGameObject=Find<GridManager>().gameObject;
        var view=SceneView.lastActiveSceneView;
        if(view!=null){var g=Find<GridManager>();view.LookAt(g.transform.position+new Vector3(g.Definition.Width*g.Definition.CellSize*.5f,g.Definition.Height*g.Definition.CellSize*.5f,0),Quaternion.identity,13,true);view.drawGizmos=true;view.Repaint();}
        File.WriteAllLines(Folder+"saved-assets-final.txt",report);return string.Join("\n",report);
    }
    public static async Task<string> Hud()
    {
        if(!Application.isPlaying)throw new Exception("Play mode required");Time.timeScale=0;
        var build=Find<BuildManager>();build.Economy.AddMoney(50000);Find<BorderBreachService>().ConfigureThreshold(0);
        var grid=build.Battlefield.Grid;var mg=Find<BuildHud>().Defenses.Single(d=>d.Id=="machine-gun-nest");
        for(int y=0;y<grid.Height;y++)
        {
            var cell=new Vector2Int(14,y);if(!grid.IsTraversable(cell))continue;
            if(build.TryPlaceDefense(mg,cell,out var d)!=PlacementResult.Valid)throw new Exception("HUD wall placement");
            d.GetComponent<TurretCombat>().enabled=false;
        }
        var spawn=Find<EnemySpawner>();var crawler=Find<WaveManager>().EndlessSettings.enemies.Single(e=>e.Definition.Kind==EnemyKind.Crawler);
        if(!spawn.TrySpawnAt(crawler,grid.CellToWorld(new Vector2Int(12,5)),out var enemy))throw new Exception("HUD spawn");
        Time.timeScale=1;
        for(int i=0;i<150&&enemy.StructureAttack.AttacksPerformed<2;i++)await Task.Delay(30);
        Time.timeScale=0;var target=enemy.StructureAttack.Target;
        if(target==null||target.CurrentHealth>=target.MaxHealth||!Find<StructureSelection>().Select(target))throw new Exception("HUD target damage");
        await Task.Delay(150);
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);
        var encode=type.GetMethod("EncodeScreenToPng",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
        File.WriteAllBytes(Folder+"selected-damaged-defense-hud.png",(byte[])encode.Invoke(null,new object[]{Screen.width,Screen.height}));
        string result=$"Update-driven HUD: target HP {target.CurrentHealth}/{target.MaxHealth}, repair {build.RepairCost(target)} RSD, attacks {enemy.StructureAttack.AttacksPerformed}, render {Screen.width}x{Screen.height}.";
        File.WriteAllText(Folder+"hud-final.txt",result);return result;
    }

    // Camera.Render omits Editor Gizmos. This transient export displays the same cell data
    // and a real A* route without saving any debug object/material into the game scene.
    public static string GridOverlay()
    {
        if(Application.isPlaying)throw new Exception("Edit mode required");
        var manager=Find<GridManager>();var grid=manager.Definition.CreateGrid(manager.transform.position);
        var root=new GameObject("Transient navigation debug export");root.hideFlags=HideFlags.HideAndDontSave;
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/TopDownPlaceholder.mat");
        var cameraObject=new GameObject("Transient debug camera");cameraObject.hideFlags=HideFlags.HideAndDontSave;
        var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=12;
        camera.transform.position=grid.Origin+new Vector3(grid.Width*grid.CellSize*.5f,grid.Height*grid.CellSize*.5f,-10);
        camera.backgroundColor=new Color(.06f,.09f,.1f);camera.clearFlags=CameraClearFlags.SolidColor;
        var target=new RenderTexture(1920,1080,24);var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);var active=RenderTexture.active;
        void Line(string name,Vector3[] points,Color color,float width,bool loop)
        {
            var go=new GameObject(name);go.transform.SetParent(root.transform);
            var line=go.AddComponent<LineRenderer>();line.sharedMaterial=material;line.startColor=line.endColor=color;
            line.widthMultiplier=width;line.loop=loop;line.positionCount=points.Length;line.SetPositions(points);line.sortingLayerName="Effects";line.sortingOrder=200;
        }
        try
        {
            for(int x=0;x<grid.Width;x++)for(int y=0;y<grid.Height;y++)
            {
                var cell=new Vector2Int(x,y);var center=grid.CellToWorld(cell)+Vector3.back*.2f;float h=grid.CellSize*.49f;
                var color=grid.IsTraversable(cell)?new Color(.45f,.55f,.6f,.35f):new Color(1,.2f,.12f,.9f);
                Line("Cell",new[]{center+new Vector3(-h,-h),center+new Vector3(h,-h),center+new Vector3(h,h),center+new Vector3(-h,h)},color,.025f,true);
            }
            var route=new List<Vector2Int>();if(!new PathfindingService(grid).TryFindPath(new Vector2Int(6,3),new Vector2Int(31,5),route))throw new Exception("Debug route missing");
            if(route.Any(c=>!grid.IsTraversable(c)))throw new Exception("Debug route crosses blocker");
            Line("Actual A* route",route.Select(c=>grid.CellToWorld(c)+Vector3.back*.3f).ToArray(),Color.cyan,.09f,false);
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(Folder+"port-navigation-debug.png",image.EncodeToPNG());
            return "Exported authoritative blocked-cell overlay and actual crane/container detour path; all "+route.Count+" path cells traversable.";
        }
        finally{RenderTexture.active=active;Object.DestroyImmediate(root);Object.DestroyImmediate(cameraObject);Object.DestroyImmediate(image);Object.DestroyImmediate(target);}
    }
}
