using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShorelineDefense.Presentation;
using ShorelineDefense.Building;

public static class DefenseArtImport
{
    const string Art="Assets/Game/Art/Defenses/";
    static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
    static string PathFor(string folder,string id,string part) => Art+folder+"/bd_defense_"+id+"_"+part+"_v001.png";
    static TextureImporter Settings(string path,bool runtime,bool sheet=false)
    {
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var t=(TextureImporter)AssetImporter.GetAtPath(path);
        t.textureType=runtime?TextureImporterType.Sprite:TextureImporterType.Default;
        t.filterMode=FilterMode.Point;t.textureCompression=TextureImporterCompression.Uncompressed;
        t.crunchedCompression=false;t.mipmapEnabled=false;t.wrapMode=TextureWrapMode.Clamp;
        t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=runtime?512:4096;t.isReadable=false;t.sRGBTexture=true;
        t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=runtime;
        foreach(var p in new[]{"Standalone","Android","iPhone","WebGL"})t.ClearPlatformTextureSettings(p);
        if(runtime)
        {
            var s=new TextureImporterSettings();t.ReadTextureSettings(s);
            s.spriteMode=(int)(sheet?SpriteImportMode.Multiple:SpriteImportMode.Single);
            s.spritePixelsPerUnit=path.Contains("/UI/")?100:51.2f;s.spriteMeshType=SpriteMeshType.FullRect;
            s.spriteAlignment=(int)SpriteAlignment.Center;s.spritePivot=new Vector2(.5f,.5f);
            s.spriteGenerateFallbackPhysicsShape=false;t.SetTextureSettings(s);
        }
        t.SaveAndReimport();return t;
    }
    public static string Import()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first");
        var manifest=JArray.Parse(File.ReadAllText("Docs/Art/Batches/p3a3_export_manifest.json"));
        var runtime=Directory.GetFiles(Art,"*.png",SearchOption.AllDirectories).Select(p=>p.Replace('\\','/')).ToList();
        runtime.Add("Assets/Game/Art/UI/bd_ui_sniper_tower_icon_v002.png");
        var rows=new List<object>();
        foreach(var path in runtime)
        {
            var e=manifest.OfType<JObject>().FirstOrDefault(x=>(string)x["output"]==path);
            int count=e==null?1:(int)e["count"];var t=Settings(path,true,count>1);
            if(count>1)
            {
                int n=(int)e["frame"];var factory=new SpriteDataProviderFactories();factory.Init();
                var provider=factory.GetSpriteEditorDataProviderFromObject(t);provider.InitSpriteEditorDataProvider();
                var existing=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
                string stem=System.IO.Path.GetFileNameWithoutExtension(path).Replace("_sheet_v001","");
                var rects=Enumerable.Range(0,count).Select(i=>{
                    string name=stem+"__f"+i.ToString("D3");
                    return new SpriteRect{name=name,rect=new Rect(i*n,0,n,n),alignment=SpriteAlignment.Center,
                        pivot=new Vector2(.5f,.5f),spriteID=existing.ContainsKey(name)?existing[name]:GUID.Generate()};
                }).ToArray();
                provider.SetSpriteRects(rects);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
                provider.Apply();t.SaveAndReimport();
            }
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            if(sprites.Length!=count)throw new Exception("Wrong slice count: "+path);
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            rows.Add(new{path,tex.width,tex.height,t.spritePixelsPerUnit,filter=t.filterMode.ToString(),compression=t.textureCompression.ToString(),t.mipmapEnabled,
                t.alphaIsTransparency,t.isReadable,sprites=sprites.Select(s=>new{s.name,rect=s.rect.ToString(),pivot=s.pivot.ToString(),s.pixelsPerUnit}).ToArray()});
        }
        var archives=Directory.GetFiles("Assets/Game/Art/SourceSheets/p3a3_defenses_001","*.png").Concat(Directory.GetFiles("Assets/Game/Art/MasterReferences","*reference_v*.png"))
            .Concat(new[]{"Assets/Game/Art/MasterReferences/ref_defense_lineup_core_01.png"});
        foreach(var path in archives)Settings(path.Replace('\\','/'),false);
        AssetDatabase.SaveAssets();
        File.WriteAllText("Docs/Validation/Phase3A3/unity-imports.json",JsonConvert.SerializeObject(rows,Formatting.Indented));
        return "Imported "+rows.Count+" runtime PNGs; sliced 4 MG and 3 sniper frames.";
    }
    static void Bind(SpriteRenderer r,Sprite sprite,string layer,int order)
    {
        r.sprite=sprite;r.enabled=true;r.color=Color.white;r.drawMode=SpriteDrawMode.Simple;
        r.transform.localScale=Vector3.one;r.transform.localPosition=Vector3.zero;r.transform.localRotation=Quaternion.identity;
        r.sortingLayerName=layer;r.sortingOrder=order;
    }
    static void Retire(SpriteRenderer r) {r.enabled=false;r.sprite=null;}
    static void Turret(string prefab,string folder,string id,string layer,float fps,float muzzleX)
    {
        string path="Assets/Game/Prefabs/Defenses/"+prefab+".prefab";var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var body=root.transform.Find("Sandbag Base").GetComponent<SpriteRenderer>();
            var rotating=root.transform.Find("Rotating Weapon");var weapon=rotating.Find("Receiver").GetComponent<SpriteRenderer>();
            var idle=Sprite(PathFor(folder,id,"base_idle"));Bind(body,idle,"Defenses",0);
            Retire(root.transform.Find("Inset").GetComponent<SpriteRenderer>());Retire(rotating.Find("Barrel").GetComponent<SpriteRenderer>());
            var aim=Sprite(PathFor(folder,id,layer+"_idle"));Bind(weapon,aim,"Defenses",2);
            rotating.Find("Muzzle").localPosition=new Vector3(muzzleX/51.2f,0,0);
            var frames=AssetDatabase.LoadAllAssetsAtPath(PathFor(folder,id,layer+"_fire_sheet")).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            root.GetComponent<TurretPresentation>().ConfigureSprites(weapon,aim,frames,fps);
            var art=root.GetComponent<DefenseSpritePresentation>()??root.AddComponent<DefenseSpritePresentation>();
            art.Configure(body,idle,Sprite(PathFor(folder,id,"base_damaged")),Sprite(PathFor(folder,id,"base_destroyed")),null,Sprite(PathFor(folder,id,"preview")));
            PrefabUtility.SaveAsPrefabAsset(root,path);
        } finally {PrefabUtility.UnloadPrefabContents(root);}
    }
    static void Static(string prefab,string folder,string id,bool mine)
    {
        string path="Assets/Game/Prefabs/Defenses/"+prefab+".prefab";var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var renderers=root.GetComponentsInChildren<SpriteRenderer>(true);
            var body=renderers.First(r=>r.name==(mine?"Mine body":"Wire strand"));
            foreach(var r in renderers)if(r!=body&&r.sortingLayerName!="Effects")Retire(r);
            var idle=Sprite(PathFor(folder,id,mine?"idle":"single"));Bind(body,idle,"GroundObjects",mine?2:1);
            var art=root.GetComponent<DefenseSpritePresentation>()??root.AddComponent<DefenseSpritePresentation>();
            art.Configure(body,idle,mine?null:Sprite(PathFor(folder,id,"damaged")),mine?null:Sprite(PathFor(folder,id,"destroyed")),mine?Sprite(PathFor(folder,id,"armed")):null,idle);
            PrefabUtility.SaveAsPrefabAsset(root,path);
        } finally {PrefabUtility.UnloadPrefabContents(root);}
    }
    public static string Integrate()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!="Assets/Game/Scenes/CoastalDefense_TopDown.unity"||scene.isDirty)throw new InvalidOperationException("Expected clean primary scene");
        Turret("MachineGunNest","MachineGunNest","mg","weapon",20,26);
        Turret("SniperTower","SniperTower","sniper","aim",12,34);
        Static("BarbedWire","BarbedWire","wire",false);Static("LandMine","LandMine","mine",true);
        var placement=UnityEngine.Object.FindFirstObjectByType<PlacementController>();
        var serialized=new SerializedObject(placement);var preview=(SpriteRenderer)serialized.FindProperty("preview").objectReferenceValue;
        var presentation=placement.GetComponent<DefensePreviewPresentation>()??placement.gameObject.AddComponent<DefensePreviewPresentation>();
        presentation.Configure(placement,preview);EditorUtility.SetDirty(presentation);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "Updated four existing prefabs and added observer to existing placement ghost; gameplay roots/data preserved.";
    }
}
