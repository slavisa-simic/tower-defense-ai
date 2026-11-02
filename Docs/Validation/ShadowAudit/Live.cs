using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using ShorelineDefense.Building;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Persistence;
using ShorelineDefense.UI;
using Object=UnityEngine.Object;

// Deterministic inspection fixture only. Currency/units exist in Play Mode and are never saved.
public static class ShadowLive
{
    const string Folder="Docs/Validation/ShadowAudit";
    static T Find<T>() where T:Object => Object.FindFirstObjectByType<T>();
    public static string Isolate() { LocalGameData.UseDirectory(Path.Combine(Path.GetTempPath(),"BorderDefendersShadowAudit",Guid.NewGuid().ToString()));return "Isolated QA persistence"; }
    public static async Task<string> Capture(string revision, string map)
    {
        Isolate();
        RunSession.Pending=null;
        var window=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));window.Focus();
        await SceneManager.LoadSceneAsync(map);await Task.Delay(250);Time.timeScale=0;
        var build=Find<BuildManager>();build.Economy.AddMoney(20000);
        var defenses=Find<BuildHud>().Defenses;
        int cursor=0;
        foreach(var definition in defenses) {
            bool placed=false;
            var targetCell=new Vector2Int(22,1+cursor*2);
            var candidates=Enumerable.Range(10,17).SelectMany(x=>Enumerable.Range(1,16).Select(y=>new Vector2Int(x,y))).OrderBy(c=>(c-targetCell).sqrMagnitude);
            foreach(var cell in candidates) {
                if(build.TryPlaceDefense(definition,cell,out _)==PlacementResult.Valid){placed=true;cursor++;break;}
            }
            if(!placed)throw new Exception("Unable to place "+definition.name);
        }
        var spawner=Find<EnemySpawner>();var grid=Find<GridManager>().Grid;
        var names=new[]{"Crawler","Skitter","Bulwark","Spitter","Broodling","Burrower","Dredger","Broodmother"};
        int count=0;
        for(int i=0;i<names.Length;i++) {
            var prefab=AssetDatabase.LoadAssetAtPath<EnemyController>("Assets/Game/Prefabs/Enemies/"+names[i]+"_TopDown.prefab");
            bool spawned=false;
            for(int y=1+i*2;y<grid.Height&&!spawned;y++)for(int x=7;x<10&&!spawned;x++)
                if(spawner.TrySpawnAt(prefab,grid.CellToWorld(new Vector2Int(x,y)),out var enemy)){
                    // Fixed facing exhibits rotation-independent shadow direction in comparisons.
                    enemy.transform.Find("Visual").rotation=Quaternion.Euler(0,0,45*(i%4));spawned=true;count++;
                }
            if(!spawned)throw new Exception("Unable to spawn "+names[i]);
        }
        await Task.Delay(150);
        // Camera-only render gives identical 1920x1080 framing regardless of desktop window size.
        var camera=Camera.main;var target=RenderTexture.GetTemporary(1920,1080,24);
        var old=camera.targetTexture;var active=RenderTexture.active;
        try {
            RenderTexture.active=target;GL.Clear(true,true,Color.black);
            camera.targetTexture=target;camera.GetComponent<ShorelineDefense.Presentation.TopDownCamera>().FrameBattlefield();camera.Render();RenderTexture.active=target;
            var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(Folder+"/"+revision+"-"+map+".png",image.EncodeToPNG());Object.DestroyImmediate(image);
        } finally {camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);}
        File.WriteAllText(Folder+"/"+revision+"-"+map+"-fixture.json",JsonConvert.SerializeObject(new {map,units=count,defenses=cursor,currencyGrant=20000,resolution="1920x1080",cameraOnly=true,cameraPosition=camera.transform.position.ToString("F4"),cameraRotation=camera.transform.eulerAngles.ToString("F4"),orthographicSize=camera.orthographicSize,viewport=camera.rect.ToString()},Formatting.Indented));
        return "Captured "+revision+" "+map+" with "+count+" units / "+cursor+" defenses";
    }
}
