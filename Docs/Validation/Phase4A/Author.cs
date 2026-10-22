using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using ShorelineDefense.Economy;
using ShorelineDefense.Grid;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;

public static class Phase4AAuthor
{
    const string Art = "Assets/Game/Art/";
    static void Set(UnityEngine.Object o, string key, UnityEngine.Object value)
    { var s = new SerializedObject(o); s.FindProperty(key).objectReferenceValue = value; s.ApplyModifiedPropertiesWithoutUndo(); }
    static void Number(UnityEngine.Object o, string key, int value)
    { var s = new SerializedObject(o); s.FindProperty(key).intValue = value; s.ApplyModifiedPropertiesWithoutUndo(); }
    static void Float(UnityEngine.Object o, string key, float value)
    { var s = new SerializedObject(o); s.FindProperty(key).floatValue = value; s.ApplyModifiedPropertiesWithoutUndo(); }
    static Sprite[] Frames(string type, string action)
    { return AssetDatabase.LoadAllAssetsAtPath(Art+"Enemies/"+type+"/bd_enemy_"+(type=="BasicInfected"?"basic_infected":type.ToLower())+"_"+action+"_e_sheet_v001.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray(); }
    static EnemyController Enemy(string name, EnemyKind kind, int hp, float speed, int reward, int penalty, int cost, int unlock, int capacity)
    {
        string path="Assets/Game/Data/Enemies/"+name+".asset";
        var data=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
        if(data==null){data=ScriptableObject.CreateInstance<EnemyDefinition>();AssetDatabase.CreateAsset(data,path);}
        Number(data,"kind",(int)kind);Number(data,"maxHealth",hp);Float(data,"movementSpeed",speed);Number(data,"killReward",reward);
        Number(data,"breachPenalty",penalty);Number(data,"difficultyCost",cost);Number(data,"unlockWave",unlock);Number(data,"transportCapacityUnits",capacity);
        var s=new SerializedObject(data);s.FindProperty("displayName").stringValue=name;s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(data);
        string prefab="Assets/Game/Prefabs/Enemies/"+name+"_TopDown.prefab";
        if(!File.Exists(prefab))AssetDatabase.CopyAsset("Assets/Game/Prefabs/Enemies/Crawler_TopDown.prefab",prefab);
        var root=PrefabUtility.LoadPrefabContents(prefab);
        try
        {
            root.name=name+"_TopDown";var controller=root.GetComponent<EnemyController>();controller.Configure(data);
            var presentation=root.GetComponent<EnemySpritePresentation>();var body=root.transform.Find("Visual/Body").GetComponent<SpriteRenderer>();
            string type=name;var walk=Frames(type,name=="Runner"?"run":"walk");
            if(walk.Length==0)throw new Exception("Missing "+name+" frames");
            body.sprite=walk[0];presentation.Configure(body,root.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(r=>r.name=="Water Wake"),Frames(type,"idle"),walk,Frames(type,"attack"),Frames(type,"death"));
            var serialized=new SerializedObject(presentation);var hit=Frames(type,"hit");var a=serialized.FindProperty("hit");a.arraySize=hit.Length;for(int i=0;i<hit.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=hit[i];serialized.ApplyModifiedPropertiesWithoutUndo();
            Float(presentation,"walkFps",name=="Runner"?12:6);Float(presentation,"effectScale",name=="Brute"?1.4f:1);
            PrefabUtility.SaveAsPrefabAsset(root,prefab);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
        return AssetDatabase.LoadAssetAtPath<EnemyController>(prefab);
    }
    static void Import(string path, bool sprite)
    {
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var t=(TextureImporter)AssetImporter.GetAtPath(path);
        t.textureType=sprite?TextureImporterType.Sprite:TextureImporterType.Default;t.filterMode=FilterMode.Point;t.textureCompression=TextureImporterCompression.Uncompressed;
        t.mipmapEnabled=false;t.wrapMode=TextureWrapMode.Clamp;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=sprite;
        if(sprite){t.spriteImportMode=SpriteImportMode.Single;t.spritePixelsPerUnit=100;}
        t.SaveAndReimport();
    }
    static void Export(string source,string target,int w,int h,bool fit)
    {
        var t=new Texture2D(2,2);t.LoadImage(File.ReadAllBytes(source));var pixels=t.GetPixels32();int l=t.width,b=t.height,r=0,top=0;
        for(int y=0;y<t.height;y++)for(int x=0;x<t.width;x++)if(pixels[y*t.width+x].a>=240){l=Math.Min(l,x);r=Math.Max(r,x);b=Math.Min(b,y);top=Math.Max(top,y);}
        int sw=r-l+1,sh=top-b+1;float scale=Math.Min((w-4)/(float)sw,(h-4)/(float)sh);
        int dw=fit?Mathf.RoundToInt(sw*scale):w-4,dh=fit?Mathf.RoundToInt(sh*scale):h-4;
        var output=new Color32[w*h];
        for(int y=0;y<dh;y++)for(int x=0;x<dw;x++)
        {
            int sx=l+Math.Min(sw-1,(int)((x+.5f)*sw/dw)),sy=b+Math.Min(sh-1,(int)((y+.5f)*sh/dh));var p=pixels[sy*t.width+sx];
            if(p.a<240)continue;output[(y+(h-dh)/2)*w+x+(w-dw)/2]=new Color32((byte)(p.r/8*8),(byte)(p.g/8*8),(byte)(p.b/8*8),255);
        }
        var result=new Texture2D(w,h,TextureFormat.RGBA32,false);result.SetPixels32(output);result.Apply();File.WriteAllBytes(target,result.EncodeToPNG());UnityEngine.Object.DestroyImmediate(result);UnityEngine.Object.DestroyImmediate(t);Import(source,false);Import(target,true);
    }
    static SpriteRenderer Visual(Transform parent,string name,string path,Vector3 position,float angle=0)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=position;go.transform.rotation=Quaternion.Euler(0,0,angle);
        var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+path);if(renderer.sprite==null)throw new Exception(path);
        renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/TopDownPlaceholder.mat");renderer.sortingLayerName="GroundObjects";renderer.sortingOrder=9;return renderer;
    }
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode required");
        if(SceneManager.GetActiveScene().path!="Assets/Game/Scenes/CoastalDefense_TopDown.unity")throw new Exception("Primary scene required");
        AssetDatabase.Refresh();
        foreach(var spec in new[]{new[]{"money_rsd","ui_icon_money_rsd"},new[]{"money_bundle_rsd","ui_icon_money_bundle_rsd"},new[]{"money_panel_rsd","ui_money_panel_rsd"}})
            Export(Art+"SourceSheets/p4a_money_001/bd_src_"+spec[0]+"_v001.png",Art+"UI/"+spec[1]+".png",spec[0]=="money_panel_rsd"?160:32,spec[0]=="money_panel_rsd"?64:32,spec[0]!="money_panel_rsd");
        if(!AssetDatabase.IsValidFolder(Art+"UI/Archive"))AssetDatabase.CreateFolder(Art+"UI","Archive");
        if(File.Exists(Art+"UI/bd_ui_money_icon_v001.png")) {string error=AssetDatabase.MoveAsset(Art+"UI/bd_ui_money_icon_v001.png",Art+"UI/Archive/bd_ui_money_icon_v001.png");if(error!="")throw new Exception(error);}
        var basic=AssetDatabase.LoadAssetAtPath<EnemyController>("Assets/Game/Prefabs/Enemies/Crawler_TopDown.prefab");
        Number(basic.Definition,"breachPenalty",25);Number(basic.Definition,"difficultyCost",1);Number(basic.Definition,"unlockWave",1);EditorUtility.SetDirty(basic.Definition);
        var runner=Enemy("Runner",EnemyKind.Runner,60,4.5f,35,50,2,3,1);var brute=Enemy("Brute",EnemyKind.Brute,600,1.35f,75,100,5,6,3);
        string tuningPath="Assets/Game/Data/Waves/EndlessSurvival.asset";var tuning=AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>(tuningPath);
        if(tuning==null){tuning=ScriptableObject.CreateInstance<EndlessWaveSettings>();AssetDatabase.CreateAsset(tuning,tuningPath);}tuning.enemies=new[]{basic,runner,brute};EditorUtility.SetDirty(tuning);
        var waves=UnityEngine.Object.FindFirstObjectByType<WaveManager>();var spawner=UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();var registry=UnityEngine.Object.FindFirstObjectByType<EnemyRegistry>();
        var grid=UnityEngine.Object.FindFirstObjectByType<GridManager>();var game=UnityEngine.Object.FindFirstObjectByType<GameStateController>();var economy=UnityEngine.Object.FindFirstObjectByType<EconomyManager>();
        var services=GameObject.Find("Survival Services")??new GameObject("Survival Services");
        var stats=services.GetComponent<RunStatistics>()??services.AddComponent<RunStatistics>();stats.Configure(registry,waves,economy);
        var border=services.GetComponent<BorderBreachService>()??services.AddComponent<BorderBreachService>();border.Configure(stats,game,grid,new[]{new Vector2Int(31,4),new Vector2Int(31,9),new Vector2Int(31,14)});
        Set(waves,"endless",tuning);Set(spawner,"border",border);
        var hud=UnityEngine.Object.FindFirstObjectByType<PrototypeHud>();Set(hud,"border",border);Set(hud,"statistics",stats);
        var artHud=UnityEngine.Object.FindFirstObjectByType<HudArtPresentation>();Set(artHud,"border",border);Set(artHud,"statistics",stats);
        Set(UnityEngine.Object.FindFirstObjectByType<CombatFeedbackHud>(),"border",border);
        var old=GameObject.Find("Final Border Checkpoint");if(old!=null)UnityEngine.Object.DestroyImmediate(old);
        var root=new GameObject("Final Border Checkpoint").transform;Vector3 origin=grid.transform.position;
        Func<float,float,Vector3> cell=(x,y)=>origin+new Vector3((x+.5f)*1.25f,(y+.5f)*1.25f,0);
        var dressing=GameObject.Find("Danube Quarantine Dressing");
        if(dressing!=null)foreach(var part in dressing.GetComponentsInChildren<Transform>())
            if(part.name=="base_military_tent_topdown")part.position=cell(29.3f,16);
        // Move the command-post presentation aside; BaseHealth root remains reusable but receives no attacks in this mode.
        var baseRoot=UnityEngine.Object.FindFirstObjectByType<ShorelineDefense.Base.BaseHealth>().transform;
        var visual=baseRoot.Find("Visual");if(visual!=null)visual.localPosition=new Vector3(-1.25f,-2.5f,0);
        for(int y=0;y<18;y++)if(y!=4&&y!=9&&y!=14)
            Visual(root,"Reinforced border segment "+y,"Props/bd_prop_quarantine_fence_segment_v001.png",cell(30.4f,y),90);
        foreach(int lane in new[]{4,9,14})
        {
            var road=Visual(root,"Road exit "+lane,"Environment/bd_env_road_tile_v001.png",cell(29.5f,lane));road.sortingLayerName="Environment";road.sortingOrder=5;road.drawMode=SpriteDrawMode.Tiled;road.size=new Vector2(5,1.25f);
            Visual(root,"Gate barrier "+lane,"Base/bd_base_bunker_gate_piece_v001.png",cell(30.5f,lane+.6f),90);
            Visual(root,"Border floodlight "+lane,"Props/bd_prop_floodlight_v001.png",cell(29.7f,lane+.85f));
            Visual(root,"Warning marker "+lane,"Props/bd_prop_warning_sign_v001.png",cell(28.5f,lane+.85f));
        }
        AssetDatabase.ImportAsset("Assets/Game/UI/TopDownHud.uss",ImportAssetOptions.ForceUpdate);AssetDatabase.ImportAsset("Assets/Game/UI/TopDownHud.uxml",ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        return "Endless tuning, three enemy prefabs, services, three border lanes and RSD art saved";
    }
}



