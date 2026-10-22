using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json;
using ShorelineDefense.Base;
using ShorelineDefense.Core;
using ShorelineDefense.Grid;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using ShorelineDefense.Presentation;

public static class Phase3C1Author
{
    const string Art="Assets/Game/Art/";
    static Material material;
    static Sprite shadow;
    static readonly List<object> exports=new List<object>();
    static void Folder(string path)
    {
        if(AssetDatabase.IsValidFolder(path))return;
        string parent=Path.GetDirectoryName(path).Replace('\\','/');Folder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
    }
    static void Import(string path,bool sprite)
    {
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var t=(TextureImporter)AssetImporter.GetAtPath(path);
        t.textureType=sprite?TextureImporterType.Sprite:TextureImporterType.Default;t.filterMode=FilterMode.Point;
        t.textureCompression=TextureImporterCompression.Uncompressed;t.crunchedCompression=false;t.mipmapEnabled=false;
        t.wrapMode=TextureWrapMode.Clamp;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;
        t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=sprite;t.isReadable=false;
        foreach(var platform in new[]{"Standalone","Android","iPhone","WebGL"})t.ClearPlatformTextureSettings(platform);
        if(sprite){var s=new TextureImporterSettings();t.ReadTextureSettings(s);s.spriteMode=1;s.spritePixelsPerUnit=51.2f;s.spriteMeshType=SpriteMeshType.FullRect;s.spritePivot=Vector2.one*.5f;s.spriteAlignment=(int)SpriteAlignment.Center;s.spriteGenerateFallbackPhysicsShape=false;t.SetTextureSettings(s);}
        t.SaveAndReimport();
    }
    static Sprite Save(string relative,int n,Color32[] pixels,string source)
    {
        string path=Art+relative;Folder(Path.GetDirectoryName(path).Replace('\\','/'));
        var t=new Texture2D(n,n,TextureFormat.RGBA32,false);t.SetPixels32(pixels);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
        Import(path,true);exports.Add(new{path,source,width=n,height=n,ppu=51.2,pivot="center",filter="Point",alpha=pixels.Any(p=>p.a!=0&&p.a!=255)?"clustered 0/100/180":"binary",colors=pixels.Where(p=>p.a>0).Distinct().Count()});
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static Color32[] Sample(string source,int n,int margin)
    {
        var t=new Texture2D(2,2);t.LoadImage(File.ReadAllBytes(source));var input=t.GetPixels32();
        int left=t.width,bottom=t.height,right=0,top=0;
        for(int y=0;y<t.height;y++)for(int x=0;x<t.width;x++)if(input[y*t.width+x].a>=240){left=Math.Min(left,x);right=Math.Max(right,x);bottom=Math.Min(bottom,y);top=Math.Max(top,y);}
        if(left>=right)throw new Exception("No solid alpha foreground "+source);
        int span=Math.Max(right-left+1,top-bottom+1),inside=n-margin*2;
        float cx=(left+right)*.5f,cy=(bottom+top)*.5f;
        var pixels=new Color32[n*n];
        for(int y=margin;y<n-margin;y++)for(int x=margin;x<n-margin;x++)
        {
            int sx=Mathf.Clamp(Mathf.RoundToInt(cx+(x-(n-1)*.5f)*span/inside),0,t.width-1);
            int sy=Mathf.Clamp(Mathf.RoundToInt(cy+(y-(n-1)*.5f)*span/inside),0,t.height-1);
            var p=input[sy*t.width+sx];if(p.a<240)continue;
            // Recorded no-dither 5-bit channels, hard alpha, nearest sampling only.
            pixels[y*n+x]=new Color32((byte)((p.r/8)*8),(byte)((p.g/8)*8),(byte)((p.b/8)*8),255);
        }
        UnityEngine.Object.DestroyImmediate(t);return pixels;
    }
    public static string Export()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode required");
        foreach(string path in Directory.GetFiles(Art+"SourceSheets/p3c1_polish_001","*.png"))Import(path,false);
        Save("Environment/Vegetation/bd_env_reeds_v001.png",48,Sample(Art+"SourceSheets/p3c1_polish_001/bd_src_reeds_v002.png",48,3),"reeds v002");
        Save("Environment/Vegetation/bd_env_shrub_v001.png",48,Sample(Art+"SourceSheets/p3c1_polish_001/bd_src_shrub_v001.png",48,3),"shrub v001");
        Save("Environment/Vegetation/bd_env_grass_tuft_v001.png",32,Sample(Art+"SourceSheets/p3c1_polish_001/bd_src_grass_v001.png",32,3),"grass v001");
        var cross=Sample(Art+"SourceSheets/p3c1_polish_001/bd_src_wire_cross_v001.png",64,0);
        // Match every opposing edge exactly using the east half-arm as canonical source.
        var canonical=new Color32[32*16];
        for(int u=0;u<32;u++)for(int v=0;v<16;v++)canonical[u*16+v]=cross[(v+24)*64+u+32];
        for(int v=0;v<16;v++)canonical[31*16+v]=canonical[30*16+v];
        for(int mask=0;mask<16;mask++)
        {
            var pixels=new Color32[64*64];
            for(int y=24;y<40;y++)for(int x=24;x<40;x++)pixels[y*64+x]=cross[y*64+x];
            for(int u=0;u<32;u++)for(int v=0;v<16;v++)
            {
                var p=canonical[u*16+v];
                if((mask&2)!=0)pixels[(v+24)*64+u+32]=p;
                if((mask&8)!=0)pixels[(v+24)*64+31-u]=p;
                if((mask&1)!=0)pixels[(u+32)*64+v+24]=p;
                if((mask&4)!=0)pixels[(31-u)*64+v+24]=p;
            }
            Save("Defenses/BarbedWire/bd_defense_wire_mask_"+mask.ToString("D2")+"_v002.png",64,pixels,"wire cross v001 canonical arm/center");
        }
        var contact=new Color32[64*64];
        for(int y=0;y<64;y++)for(int x=0;x<64;x++)
        {
            float d=(x-31.5f)*(x-31.5f)/(29*29f)+(y-31.5f)*(y-31.5f)/(29*29f);
            if(d<=1)contact[y*64+x]=new Color32(31,41,44,(byte)(d>.8f?100:180));
        }
        Save("Effects/ContactShadow/bd_fx_contact_shadow_v001.png",64,contact,"shared code-native pixel contact ellipse; no generated source");
        File.WriteAllText("Docs/Art/Batches/p3c1_exports.json",JsonConvert.SerializeObject(exports,Formatting.Indented));
        return "Exported vegetation, 16 edge-matched wire masks and shared shadow";
    }
    static Sprite Load(string path)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+path);
    static SpriteRenderer Renderer(Transform parent,string name,Sprite sprite,Vector3 position,string layer,int order)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;
        var r=go.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.sortingLayerName=layer;r.sortingOrder=order;return r;
    }
    static SpriteRenderer Shadow(Transform parent,Vector2 size,float opacity,string layer,int order)
    {
        var existing=parent.Find("Contact Shadow");if(existing!=null)UnityEngine.Object.DestroyImmediate(existing.gameObject);
        var r=Renderer(parent,"Contact Shadow",shadow,new Vector3(.04f,-.04f,0),layer,order);
        r.transform.position=parent.position+new Vector3(.04f,-.04f,0);r.transform.rotation=Quaternion.identity;
        r.transform.localScale=new Vector3(size.x/1.25f,size.y/1.25f,1);r.color=new Color(1,1,1,opacity);return r;
    }
    public static string Integrate()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode required");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!="Assets/Game/Scenes/CoastalDefense_TopDown.unity")throw new Exception("Primary scene required");
        material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/TopDownPlaceholder.mat");shadow=Load("Effects/ContactShadow/bd_fx_contact_shadow_v001.png");
        var grid=UnityEngine.Object.FindFirstObjectByType<GridManager>();var data=grid.Definition;
        var origin=grid.transform.position;
        Func<float,float,Vector3> cell=(x,y)=>origin+new Vector3((x+.5f)*data.CellSize,(y+.5f)*data.CellSize,0);
        foreach(string name in new[]{"Crawler_TopDown","MachineGunNest","SniperTower","BarbedWire","LandMine","PlayerBase_TopDown"})
        {
            string path="Assets/Game/Prefabs/"+(name=="Crawler_TopDown"?"Enemies/":name=="PlayerBase_TopDown"?"Gameplay/":"Defenses/")+name+".prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                Vector2 size=name=="Crawler_TopDown"?new Vector2(.65f,.48f):name=="LandMine"?new Vector2(.44f,.38f):name=="PlayerBase_TopDown"?new Vector2(3.6f,3.6f):Vector2.one*1.15f;
                var s=Shadow(root.transform,size,name=="LandMine"?.12f:.3f,"GroundObjects",name=="PlayerBase_TopDown"?7:0);
                if(name=="BarbedWire")
                {
                    var body=root.GetComponentsInChildren<SpriteRenderer>().First(r=>r!=s&&r.enabled);
                    var variants=Enumerable.Range(0,16).Select(m=>Load("Defenses/BarbedWire/bd_defense_wire_mask_"+m.ToString("D2")+"_v002.png")).ToArray();
                    body.sprite=variants[0];
                    var wire=root.GetComponent<ConnectedWirePresentation>()??root.AddComponent<ConnectedWirePresentation>();wire.Configure(body,variants);
                    s.transform.localScale=Vector3.one;s.sprite=variants[0];s.color=new Color(31/255f,41/255f,44/255f,.2f);wire.ConfigureShadow(s);
                }
                var weapon=root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="Rotating Weapon");if(weapon!=null)weapon.localRotation=Quaternion.Euler(0,0,180);
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        var dressing=GameObject.Find("Danube Quarantine Dressing");
        var extension=dressing.transform.Find("Danube approach water");if(extension!=null)UnityEngine.Object.DestroyImmediate(extension.gameObject);
        var riverBackdrop=Renderer(dressing.transform,"Danube approach water",Load("Environment/bd_env_deep_danube_water_tile_v001.png"),cell(-5.5f,8.5f),"Environment",0);
        riverBackdrop.drawMode=SpriteDrawMode.Tiled;riverBackdrop.size=new Vector2(12.5f,22.5f);
        foreach(var r in dressing.GetComponentsInChildren<SpriteRenderer>())
        {
            if(r==null)continue;
            if(r.sortingLayerName=="Environment"||r.name=="Contact Shadow")continue;
            if(r.name=="prop_rubber_boat") {UnityEngine.Object.DestroyImmediate(r.gameObject);continue;}
            if(r.name=="prop_destroyed_rubber_boat")r.transform.position=cell(1.6f,16f);
            var size=r.sprite.bounds.size;var s=Shadow(r.transform,new Vector2(size.x*.82f,size.y*.82f),.25f,r.sortingLayerName,r.sortingOrder-1);
            // Offset is world-aligned even for in-plane rotated static props.
            s.transform.position=r.transform.position+new Vector3(.04f,-.04f,0);s.transform.rotation=Quaternion.identity;
        }
        var old=GameObject.Find("Riverbank Vegetation");if(old!=null)UnityEngine.Object.DestroyImmediate(old);
        var vegetation=new GameObject("Riverbank Vegetation").transform;
        var random=new System.Random(3101);int count=0;
        foreach(int y in Enumerable.Range(0,18))
        {
            if(new[]{2,5,8,11,14}.Any(l=>Math.Abs(l-y)<=0))continue;
            for(int k=0;k<2;k++)
            {
                var r=Renderer(vegetation,"Reeds "+count++,Load("Environment/Vegetation/bd_env_reeds_v001.png"),cell(3.9f+(float)random.NextDouble()*.7f,y+(float)random.NextDouble()*.4f-.2f),"GroundObjects",-9);
                r.transform.localRotation=Quaternion.Euler(0,0,random.Next(4)*90);Shadow(r.transform,new Vector2(.65f,.6f),.15f,"GroundObjects",-10);
            }
        }
        for(int i=0;i<100;i++)
        {
            int x=random.Next(6,26),y=random.Next(18);
            if((y==4||y==9||y==14)&&x%3==0)continue;
            bool bush=i%9==0;
            var r=Renderer(vegetation,(bush?"Shrub ":"Grass ")+count++,Load("Environment/Vegetation/bd_env_"+(bush?"shrub":"grass_tuft")+"_v001.png"),cell(x+(float)random.NextDouble()*.5f-.25f,y+(float)random.NextDouble()*.5f-.25f),"GroundObjects",-9);
            r.transform.localRotation=Quaternion.Euler(0,0,random.Next(4)*90);
            if(bush)Shadow(r.transform,new Vector2(.7f,.65f),.22f,"GroundObjects",-10);
        }
        Folder("Assets/Game/Prefabs/Presentation");string boatPath="Assets/Game/Prefabs/Presentation/RubberLandingBoat.prefab";
        var boat=new GameObject("Rubber Landing Boat");var visual=new GameObject("Visual").transform;visual.SetParent(boat.transform,false);
        Renderer(visual,"Rubber Boat",Load("Props/bd_prop_rubber_boat_v001.png"),Vector3.zero,"GroundObjects",-4);
        var passengerSprite=AssetDatabase.LoadAllAssetsAtPath(Art+"Enemies/BasicInfected/bd_enemy_basic_infected_idle_e_sheet_v001.png").OfType<Sprite>().OrderBy(s=>s.name).First();
        var seats=new[]{new Vector2(-.8f,.23f),new Vector2(-.8f,-.23f),new Vector2(0,.23f),new Vector2(0,-.23f),new Vector2(.75f,0)};
        for(int i=0;i<seats.Length;i++)
        {var seat=Renderer(visual,"Passenger "+i,passengerSprite,seats[i],"GroundObjects",-3);seat.transform.localScale=Vector3.one*.85f;}
        Shadow(boat.transform,new Vector2(3.5f,1.6f),.18f,"GroundObjects",-5);
        var prefab=PrefabUtility.SaveAsPrefabAsset(boat,boatPath);UnityEngine.Object.DestroyImmediate(boat);
        var wave=UnityEngine.Object.FindFirstObjectByType<WaveManager>();var executor=UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
        var transport=wave.GetComponent<BoatLandingSystem>()??wave.gameObject.AddComponent<BoatLandingSystem>();
        var points=new[]{2,5,8,11,14}.Select(y=>new RiverLandingPoint{riverEntry=cell(-12f,y),berth=cell(2.0f,y),unloadingPositions=new[]{cell(4,y)+new Vector3(-.25f,0,0),cell(4,y)+new Vector3(-.25f,.38f,0),cell(4,y)+new Vector3(-.25f,-.38f,0)}}).ToArray();
        transport.Configure(executor,UnityEngine.Object.FindFirstObjectByType<GameStateController>(),prefab,points);wave.ConfigureLandings(transport);
        Camera.main.GetComponent<TopDownCamera>().FrameBattlefield();
        if(File.Exists("Assets/Docs/Validation/Phase3C1/baseline.png")) {File.Copy("Assets/Docs/Validation/Phase3C1/baseline.png","Docs/Validation/Phase3C1/baseline.png",true);AssetDatabase.DeleteAsset("Assets/Docs");}
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "Saved shadows, "+count+" deterministic vegetation props, 5 boat landings and connected wire prefabs";
    }
}
