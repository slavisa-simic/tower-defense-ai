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
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Core;
using ShorelineDefense.Waves;
using ShorelineDefense.UI;

public static class Phase3BImport
{
    const string CatalogPath="Assets/Game/Art/Effects/CombatVfxCatalog.asset";
    static void Folder(string path)
    {
        if(AssetDatabase.IsValidFolder(path))return;
        Folder(Path.GetDirectoryName(path).Replace('\\','/'));
        AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\','/'),Path.GetFileName(path));
    }
    static TextureImporter Settings(string path,bool runtime,bool multiple,Vector2 pivot)
    {
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var t=(TextureImporter)AssetImporter.GetAtPath(path);
        t.textureType=runtime?TextureImporterType.Sprite:TextureImporterType.Default;
        t.filterMode=FilterMode.Point;t.textureCompression=TextureImporterCompression.Uncompressed;
        t.crunchedCompression=false;t.mipmapEnabled=false;t.wrapMode=TextureWrapMode.Clamp;
        t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;t.isReadable=false;t.sRGBTexture=true;
        t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=runtime;
        foreach(var platform in new[]{"Standalone","Android","iPhone","WebGL"})t.ClearPlatformTextureSettings(platform);
        if(runtime)
        {
            var s=new TextureImporterSettings();t.ReadTextureSettings(s);
            s.spriteMode=(int)(multiple?SpriteImportMode.Multiple:SpriteImportMode.Single);
            s.spritePixelsPerUnit=51.2f;s.spriteMeshType=SpriteMeshType.FullRect;
            s.spriteAlignment=(int)SpriteAlignment.Custom;s.spritePivot=pivot;s.spriteGenerateFallbackPhysicsShape=false;t.SetTextureSettings(s);
        }
        t.SaveAndReimport();return t;
    }
    public static string Import()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first");
        AssetDatabase.Refresh();var entries=JArray.Parse(File.ReadAllText("Docs/Art/Batches/p3b_manifest.json"));
        var sequences=new List<VfxSequence>();var evidence=new List<object>();
        foreach(JObject e in entries)
        {
            string path=(string)e["path"];int count=(int)e["count"],n=(int)e["frame"];
            var pivot=new Vector2((float)e["pivot"][0],(float)e["pivot"][1]);
            var t=Settings(path,true,count>1,pivot);
            if(count>1)
            {
                var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(t);provider.InitSpriteEditorDataProvider();
                var existing=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
                var rects=Enumerable.Range(0,count).Select(i=>{
                    string name="bd_fx_"+(string)e["name"]+"__f"+i.ToString("D3");
                    return new SpriteRect{name=name,rect=new Rect(i*n,0,n,n),alignment=SpriteAlignment.Custom,pivot=pivot,spriteID=existing.ContainsKey(name)?existing[name]:GUID.Generate()};
                }).ToArray();
                provider.SetSpriteRects(rects);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
                provider.Apply();t.SaveAndReimport();
            }
            var frames=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            if(frames.Length!=count)throw new Exception("Bad slices "+path);
            var sequence=new VfxSequence{id=(VfxId)Enum.Parse(typeof(VfxId),(string)e["id"]),frames=frames,fps=(float)e["fps"],
                channel=(VfxChannel)Enum.Parse(typeof(VfxChannel),(string)e["channel"]),sortingLayer=(string)e["layer"],sortingOrder=(int)e["order"],
                lifetime=(float)e["lifetime"],opacity=(float)e["opacity"],fadePortion=(float)e["fade"]};
            if(sequence.channel==VfxChannel.Smoke)sequence.drift=new Vector2(.08f,.06f);
            sequences.Add(sequence);
            string clipPath=null;
            if(count>1)
            {
                Folder("Assets/Game/Animations/Effects");clipPath="Assets/Game/Animations/Effects/bd_fx_"+(string)e["name"]+"_v001.anim";
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,clipPath);}
                clip.frameRate=sequence.fps;clip.wrapMode=(bool)e["loop"]?WrapMode.Loop:WrapMode.ClampForever;
                var keys=frames.Select((s,i)=>new ObjectReferenceKeyframe{time=i/sequence.fps,value=s}).ToArray();
                AnimationUtility.SetObjectReferenceCurve(clip,new EditorCurveBinding{path="",type=typeof(SpriteRenderer),propertyName="m_Sprite"},keys);
                var cs=AnimationUtility.GetAnimationClipSettings(clip);cs.loopTime=(bool)e["loop"];cs.startTime=0;cs.stopTime=count/sequence.fps;AnimationUtility.SetAnimationClipSettings(clip,cs);
                AnimationUtility.SetAnimationEvents(clip,Array.Empty<AnimationEvent>());EditorUtility.SetDirty(clip);
            }
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(texture.width!=(int)e["dimensions"][0]||texture.height!=(int)e["dimensions"][1])throw new Exception("Resized import "+path);
            evidence.Add(new{path,texture.width,texture.height,filter=t.filterMode.ToString(),compression=t.textureCompression.ToString(),t.mipmapEnabled,t.spritePixelsPerUnit,
                clipPath,frames=frames.Select(s=>new{s.name,rect=s.rect.ToString(),pivot=s.pivot.ToString(),s.pixelsPerUnit}).ToArray()});
        }
        foreach(var p in Directory.GetFiles("Assets/Game/Art/Effects/SourceSheets/p3b_core_001","*.png"))Settings(p.Replace('\\','/'),false,false,Vector2.zero);
        Settings("Assets/Game/Art/MasterReferences/ref_vfx_core_01.png",false,false,Vector2.zero);
        var catalog=AssetDatabase.LoadAssetAtPath<VfxCatalog>(CatalogPath);
        if(catalog==null){catalog=ScriptableObject.CreateInstance<VfxCatalog>();AssetDatabase.CreateAsset(catalog,CatalogPath);}
        catalog.sequences=sequences.ToArray();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
        File.WriteAllText("Docs/Validation/Phase3B/unity-imports.json",JsonConvert.SerializeObject(evidence,Formatting.Indented));
        return "Imported "+sequences.Count+" effect sequences; sprite-only clips and catalog saved";
    }
    static Sprite[] EnemyFrames(string action)=>AssetDatabase.LoadAllAssetsAtPath("Assets/Game/Art/Enemies/BasicInfected/bd_enemy_basic_infected_"+action+"_e_sheet_v001.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
    public static string Integrate()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!="Assets/Game/Scenes/CoastalDefense_TopDown.unity")throw new Exception("Expected primary scene");
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/TopDownPlaceholder.mat");
        var service=UnityEngine.Object.FindFirstObjectByType<VfxService>();
        if(service==null)service=new GameObject("Combat and World VFX").AddComponent<VfxService>();
        service.Configure(AssetDatabase.LoadAssetAtPath<VfxCatalog>(CatalogPath),UnityEngine.Object.FindFirstObjectByType<GameStateController>(),UnityEngine.Object.FindFirstObjectByType<GridManager>(),material);
        if(service.GetComponent<WorldWaterPresentation>()==null)service.gameObject.AddComponent<WorldWaterPresentation>();
        foreach(string name in new[]{"MachineGunNest","SniperTower"})
        {
            string path="Assets/Game/Prefabs/Defenses/"+name+".prefab";var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                root.GetComponent<TurretPresentation>().ConfigureEffects(name=="SniperTower");
                if(root.GetComponent<DefenseDamageSmoke>()==null)root.AddComponent<DefenseDamageSmoke>();
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        string enemyPath="Assets/Game/Prefabs/Enemies/Crawler_TopDown.prefab";var enemy=PrefabUtility.LoadPrefabContents(enemyPath);
        try
        {
            var visual=enemy.transform.Find("Visual");var body=visual.Find("Body").GetComponent<SpriteRenderer>();
            foreach(var r in visual.GetComponentsInChildren<SpriteRenderer>())r.enabled=false;
            body.enabled=true;body.color=Color.white;body.sprite=EnemyFrames("idle")[0];body.transform.localScale=Vector3.one;
            body.transform.localPosition=Vector3.zero;body.transform.localRotation=Quaternion.identity;
            body.sortingLayerName="Enemies";body.sortingOrder=1;
            var wakeTransform=visual.Find("Water Wake");
            if(wakeTransform==null){wakeTransform=new GameObject("Water Wake").transform;wakeTransform.SetParent(visual,false);}
            wakeTransform.localPosition=new Vector3(-.16f,0,.02f);wakeTransform.localScale=Vector3.one;
            var wake=wakeTransform.GetComponent<SpriteRenderer>();if(wake==null)wake=wakeTransform.gameObject.AddComponent<SpriteRenderer>();
            wake.sharedMaterial=material;wake.sortingLayerName="GroundObjects";wake.sortingOrder=2;wake.enabled=false;
            var presentation=enemy.GetComponent<EnemySpritePresentation>();if(presentation==null)presentation=enemy.AddComponent<EnemySpritePresentation>();
            presentation.Configure(body,wake,EnemyFrames("idle"),EnemyFrames("walk"),EnemyFrames("attack"),EnemyFrames("death"));
            PrefabUtility.SaveAsPrefabAsset(enemy,enemyPath);
        }finally{PrefabUtility.UnloadPrefabContents(enemy);}
        var hud=UnityEngine.Object.FindFirstObjectByType<WaveHud>();
        var feedback=hud.GetComponent<CombatFeedbackHud>();if(feedback==null)feedback=hud.gameObject.AddComponent<CombatFeedbackHud>();
        feedback.Configure(UnityEngine.Object.FindFirstObjectByType<EnemyRegistry>(),UnityEngine.Object.FindFirstObjectByType<WaveManager>(),UnityEngine.Object.FindFirstObjectByType<GameStateController>(),Camera.main,
            AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Art/Effects/UIFeedback/bd_reward_digits_v001.ttf"));
        EditorUtility.SetDirty(service);EditorUtility.SetDirty(feedback);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "Primary scene, two turret prefabs and Crawler presentation integrated; roots/data preserved";
    }
}
