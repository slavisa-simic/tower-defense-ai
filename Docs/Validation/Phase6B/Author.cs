using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using ShorelineDefense.Presentation;
using ShorelineDefense.Grid;
using Object = UnityEngine.Object;

public static class Phase6BAuthor
{
    const string Art = "Assets/Game/Art/", Data = "Assets/Game/Data/Enemies/", Prefabs = "Assets/Game/Prefabs/Enemies/";
    static readonly List<object> receipt = new List<object>();
    static readonly Dictionary<string, Sprite[][]> sequences = new Dictionary<string, Sprite[][]>();
    static readonly string[] actions = { "idle", "move", "attack", "hit", "death", "emergence", "special", "underground", "emerge" };
    static void Set(Object o, string key, object value)
    {
        var s = new SerializedObject(o); var p = s.FindProperty(key); if (p == null) throw new Exception(o.name+" missing "+key);
        if (value is int i) p.intValue=i; else if (value is float f) p.floatValue=f; else if(value is string t)p.stringValue=t; else p.objectReferenceValue=value as Object;
        s.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(o);
    }
    static void Refs(Object o,string key,IEnumerable<Object> values)
    { var s=new SerializedObject(o);var p=s.FindProperty(key);var a=values.ToArray();p.arraySize=a.Length;for(int i=0;i<a.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=a[i];s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o); }
    static void Move(string from,string to)
    { if(from==to || AssetDatabase.LoadMainAssetAtPath(from)==null)return;Directory.CreateDirectory(Path.GetDirectoryName(to));AssetDatabase.Refresh();string error=AssetDatabase.MoveAsset(from,to);if(!string.IsNullOrEmpty(error))throw new Exception(error); }
    static Sprite[] Export(string path,Color32[][] frames,int width,int height)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));var texture=new Texture2D(width*frames.Length,height,TextureFormat.RGBA32,false);
        for(int i=0;i<frames.Length;i++)texture.SetPixels32(i*width,0,width,height,frames[i]);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Multiple;imp.spritePixelsPerUnit=51.2f;imp.filterMode=FilterMode.Point;
        imp.textureCompression=TextureImporterCompression.Uncompressed;imp.mipmapEnabled=false;imp.wrapMode=TextureWrapMode.Clamp;imp.npotScale=TextureImporterNPOTScale.None;imp.alphaIsTransparency=true;imp.alphaSource=TextureImporterAlphaSource.FromInput;imp.maxTextureSize=4096;
        var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;imp.SetTextureSettings(settings);imp.SaveAndReimport();
        var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(imp);provider.InitSpriteEditorDataProvider();var previous=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
        var rects=Enumerable.Range(0,frames.Length).Select(i=> {string name=Path.GetFileNameWithoutExtension(path)+"__f"+i.ToString("D3");return new SpriteRect{name=name,rect=new Rect(i*width,0,width,height),pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,spriteID=previous.TryGetValue(name,out var id)?id:GUID.Generate()};}).ToArray();
        provider.SetSpriteRects(rects);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();imp.SaveAndReimport();
        receipt.Add(new {path,width=width*frames.Length,height,ppu=51.2f,frames=frames.Length});return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
    }
    static Sprite[][] Sheet(string name,int rows,int size,bool carrier=false)
    {
        string raw=Art+"SourceSheets/p6b_aquatic_001/"+name+"_raw_v001.png";
        if(!File.Exists(raw))throw new Exception("Missing generated source "+raw);
        var source=new Texture2D(2,2);source.LoadImage(File.ReadAllBytes(raw));var pixels=source.GetPixels32();
        var bounds=new RectInt[rows*4];
        for(int i=0;i<bounds.Length;i++)
        {
            int left=i%4*source.width/4,right=(i%4+1)*source.width/4,bottom=(rows-i/4-1)*source.height/rows,top=(rows-i/4)*source.height/rows;
            int l=right,r=left,b=top,u=bottom;
            for(int y=bottom;y<top;y++)for(int x=left;x<right;x++)if(pixels[y*source.width+x].a>=220){l=Math.Min(l,x);r=Math.Max(r,x);b=Math.Min(b,y);u=Math.Max(u,y);}
            if(r<l)throw new Exception("Empty "+name+" cell "+i);bounds[i]=new RectInt(l,b,r-l+1,u-b+1);
        }
        int width=carrier?192:size,height=carrier?96:size;
        float scale=Math.Min((width-8f)/bounds.Max(b=>b.width),(height-8f)/bounds.Max(b=>b.height));
        var result=new Sprite[rows][];
        for(int row=0;row<rows;row++)
        {
            string action=carrier?new[]{"float","move","arrival","release","retreat"}[row]:name=="broodling"?new[]{"move","attack","death"}[row]:actions[row];
            var frames=new Color32[4][];
            for(int col=0;col<4;col++)
            {
                var rect=bounds[row*4+col];frames[col]=new Color32[width*height];
                float cx=rect.center.x,cy=rect.center.y;
                for(int y=0;y<height;y++)for(int x=0;x<width;x++)
                {int sx=Mathf.FloorToInt(cx+(x+.5f-width*.5f)/scale),sy=Mathf.FloorToInt(cy+(y+.5f-height*.5f)/scale);if(sx<rect.x||sx>=rect.xMax||sy<rect.y||sy>=rect.yMax)continue;var p=pixels[sy*source.width+sx];if(p.a<220)continue;frames[col][y*width+x]=new Color32((byte)(p.r/8*8),(byte)(p.g/8*8),(byte)(p.b/8*8),255);}
            }
            string category=name=="biovfx"?"Effects/Aquatic":carrier?"Carriers":"Enemies/Aquatic";
            result[row]=Export(Art+category+"/bd_"+(carrier?"carrier_":"enemy_")+name+"_"+action+"_sheet_v001.png",frames,width,height);
            string clipPath="Assets/Game/Animations/Aquatic/"+name+"_"+action+".anim";Directory.CreateDirectory(Path.GetDirectoryName(clipPath));AssetDatabase.Refresh();
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,clipPath);}clip.frameRate=name=="skitter"?12:name=="bulwark"||carrier?6:8;
            var clipSettings=AnimationUtility.GetAnimationClipSettings(clip);clipSettings.loopTime=row<3;AnimationUtility.SetAnimationClipSettings(clip,clipSettings);
            AnimationUtility.SetObjectReferenceCurve(clip,new EditorCurveBinding{path="Visual/Body",type=typeof(SpriteRenderer),propertyName="m_Sprite"},result[row].Select((s,i)=>new ObjectReferenceKeyframe{time=i/clip.frameRate,value=s}).ToArray());EditorUtility.SetDirty(clip);
        }
        receipt.Add(new {raw,width=source.width,height=source.height,columns=4,rows,alphaCutoff=220,scale,exportWidth=width,exportHeight=height});Object.DestroyImmediate(source);sequences[name]=result;return result;
    }
    static EnemyController Creature(string old,string name,EnemyKind kind,int hp,float speed,int reward,int penalty,int cost,int unlock,int units,float armor=0,bool boss=false)
    {
        Move(Data+old+".asset",Data+name+".asset");Move(Prefabs+old+"_TopDown.prefab",Prefabs+name+"_TopDown.prefab");
        var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(Data+name+".asset");
        Set(definition,"displayName",boss?"THE "+name.ToUpperInvariant():name.ToUpperInvariant());definition.name=name;
        Set(definition,"kind",(int)kind);Set(definition,"maxHealth",hp);Set(definition,"movementSpeed",speed);Set(definition,"killReward",reward);Set(definition,"breachPenalty",penalty);Set(definition,"difficultyCost",cost);Set(definition,"unlockWave",unlock);Set(definition,"transportCapacityUnits",units);Set(definition,"armor",armor);
        string path=Prefabs+name+"_TopDown.prefab";var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            root.name=name;root.GetComponent<EnemyController>().Configure(definition);var frames=sequences[name.ToLowerInvariant()];
            var body=root.transform.Find("Visual/Body").GetComponent<SpriteRenderer>();body.sprite=frames[0][0];body.color=Color.white;body.transform.localScale=Vector3.one;
            var presentation=root.GetComponent<EnemySpritePresentation>();
            if(name=="Broodling") {Refs(presentation,"idle",frames[0]);Refs(presentation,"walk",frames[0]);Refs(presentation,"attack",frames[1]);Refs(presentation,"death",frames[2]);Refs(presentation,"hit",Array.Empty<Object>());var burn=root.GetComponent<BurnPresentation>();if(burn!=null)Object.DestroyImmediate(burn);}
            else {foreach(var pair in new[]{("idle",0),("walk",1),("attack",2),("hit",3),("death",4),("emergence",5)})Refs(presentation,pair.Item1,frames[pair.Item2]);}
            Set(presentation,"walkFps",name=="Skitter"?12f:name=="Bulwark"?6f:8f);Set(presentation,"effectScale",boss?2f:name=="Broodling"?.4f:1f);
            var shadow=root.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(r=>r.name.Contains("Shadow"));
            if(shadow!=null){shadow.sprite=frames[0][0];shadow.transform.localScale=Vector3.one;Set(presentation,"contactShadow",shadow);}
            if(name=="Spitter") {var ability=root.GetComponent<SpitterAttack>()??root.AddComponent<SpitterAttack>();Set(ability,"glob",sequences["biovfx"][0][0]);}
            if(name=="Burrower"){if(root.GetComponent<BurrowerTraversal>()==null)root.AddComponent<BurrowerTraversal>();Refs(presentation,"burrow",frames[6]);Refs(presentation,"underground",frames[7]);Refs(presentation,"emerge",frames[8]);}
            if(name=="Broodmother"){var ability=root.GetComponent<BroodmotherSpawn>()??root.AddComponent<BroodmotherSpawn>();Set(ability,"broodling",AssetDatabase.LoadAssetAtPath<EnemyController>(Prefabs+"Broodling_TopDown.prefab"));}
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        if(boss)Set(definition,"bossArt",sequences[name.ToLowerInvariant()][0][0]);
        return AssetDatabase.LoadAssetAtPath<EnemyController>(path);
    }
    static CarrierDefinition Carrier(string name,ArrivalMethod method,int halfUnits,Vector2 extent)
    {
        Directory.CreateDirectory("Assets/Game/Prefabs/Carriers");Directory.CreateDirectory("Assets/Game/Data/Carriers");AssetDatabase.Refresh();
        string path="Assets/Game/Prefabs/Carriers/"+name+".prefab";
        var root=new GameObject(name);var visual=new GameObject("Visual");visual.transform.SetParent(root.transform,false);var bodyGo=new GameObject("Body");bodyGo.transform.SetParent(visual.transform,false);
        var body=bodyGo.AddComponent<SpriteRenderer>();body.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/TopDownPlaceholder.mat");body.sortingLayerName="Enemies";body.sortingOrder=-2;
        var presentation=root.AddComponent<AquaticCarrierPresentation>();Set(presentation,"body",body);
        if(method!=ArrivalMethod.DirectEmergence){var frames=sequences[name];body.sprite=frames[0][0];foreach(var pair in new[]{("floating",0),("moving",1),("arrival",2),("release",3),("retreat",4)})Refs(presentation,pair.Item1,frames[pair.Item2]);
            float width=body.sprite.bounds.size.x;visual.transform.localScale=new Vector3(extent.x*2/width,extent.x*2/width,1);
        }else body.sprite=sequences["crawler"][5][0];
        var prefab=PrefabUtility.SaveAsPrefabAsset(root,path);Object.DestroyImmediate(root);
        string dataPath="Assets/Game/Data/Carriers/"+name+".asset";var definition=AssetDatabase.LoadAssetAtPath<CarrierDefinition>(dataPath);if(definition==null){definition=ScriptableObject.CreateInstance<CarrierDefinition>();AssetDatabase.CreateAsset(definition,dataPath);}
        definition.method=method;definition.capacityHalfUnits=halfUnits;definition.visualPrefab=prefab;definition.halfExtents=extent;definition.ruptureSeconds=method==ArrivalMethod.DirectEmergence?.5f:.4f;definition.remainsSeconds=method==ArrivalMethod.DriftPod?1.1f:.3f;definition.releaseInterval=method==ArrivalMethod.BroodCarrier?.04f:.07f;EditorUtility.SetDirty(definition);return definition;
    }
    static void Escorts(BossDefinition boss,params WaveEnemyGroup[] groups)
    {var so=new SerializedObject(boss);var p=so.FindProperty("escorts");p.arraySize=groups.Length;for(int i=0;i<groups.Length;i++){var q=p.GetArrayElementAtIndex(i);q.FindPropertyRelative("prefab").objectReferenceValue=groups[i].Prefab;q.FindPropertyRelative("count").intValue=groups[i].Count;q.FindPropertyRelative("spawnInterval").floatValue=.1f;}so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(boss);}
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit mode required");
        foreach(var entry in new[]{("crawler",6,64),("skitter",6,64),("bulwark",6,64),("spitter",6,64),("broodling",3,32),("burrower",9,64),("dredger",7,128),("broodmother",7,128)})Sheet(entry.Item1,entry.Item2,entry.Item3);
        foreach(string carrier in new[]{"drift_pod","brood_carrier","leviathan_carrier"})Sheet(carrier,5,192,true);
        // VFX use eight action rows, generated separately; these are inventory plus bounded runtime consumers.
        Sheet("biovfx",6,32);
        var crawler=Creature("Crawler","Crawler",EnemyKind.Crawler,100,2.5f,25,25,1,1,1);
        var skitter=Creature("Runner","Skitter",EnemyKind.Skitter,60,5f,35,50,2,3,1);
        var bulwark=Creature("Brute","Bulwark",EnemyKind.Bulwark,600,1.35f,75,100,5,7,4,.25f);
        var broodling=Creature("IsisHeavy","Broodling",EnemyKind.Broodling,18,3.8f,2,5,1,5,1);
        var spitter=Creature("IsisBreacher","Spitter",EnemyKind.Spitter,180,2.2f,55,80,4,10,2);
        var burrower=Creature("PirateRunner","Burrower",EnemyKind.Burrower,140,3.2f,50,90,4,15,2);
        var dredger=Creature("UndeadOsama","Dredger",EnemyKind.Dredger,8000,1.15f,2500,1000,20,25,4,.35f,true);
        var mother=Creature("PirateWarlord","Broodmother",EnemyKind.Broodmother,6000,1.65f,2200,900,18,25,4,.15f,true);
        var carriers=new[]{Carrier("direct_emergence",ArrivalMethod.DirectEmergence,12,new Vector2(1.5f,.8f)),Carrier("drift_pod",ArrivalMethod.DriftPod,24,new Vector2(1.65f,.85f)),Carrier("brood_carrier",ArrivalMethod.BroodCarrier,48,new Vector2(1.85f,.92f)),Carrier("leviathan_carrier",ArrivalMethod.LeviathanCarrier,64,new Vector2(2.4f,1.2f))};
        Set(dredger.Definition,"bossBoat",carriers[0].visualPrefab);Set(mother.Definition,"bossBoat",carriers[3].visualPrefab);
        Set(mother.Definition,"boatSpeedMultiplier",1f);Set(mother.Definition,"arrivalMultiplier",.8f);
        Escorts((BossDefinition)dredger.Definition,new WaveEnemyGroup(bulwark,8),new WaveEnemyGroup(crawler,20),new WaveEnemyGroup(spitter,6));
        Escorts((BossDefinition)mother.Definition,new WaveEnemyGroup(broodling,24),new WaveEnemyGroup(skitter,12),new WaveEnemyGroup(crawler,12));
        var settings=AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");settings.enemies=new[]{crawler,skitter,bulwark,spitter,broodling,burrower};settings.bossEncounters.bosses=new[]{dredger,mother};EditorUtility.SetDirty(settings);EditorUtility.SetDirty(settings.bossEncounters);
        foreach(var e in settings.eventSettings.events)
        {
            e.id=ShorelineDefense.Persistence.AquaticSaveMigration.EventId(e.id);e.modifiers.boatVisual=null;e.modifiers.composition=Array.Empty<CompositionBias>();e.modifiers.directEmergenceRatio=-1;e.modifiers.broodCarrierRatio=-1;
            void Bias(params (EnemyKind,float)[] entries)=>e.modifiers.composition=entries.Select(x=>new CompositionBias{kind=x.Item1,multiplier=x.Item2}).ToArray();
            switch(e.id)
            {
                case "mass-emergence":e.displayName="MASS EMERGENCE";e.description="High biological activity. More organisms approach across multiple sectors.";break;
                case "swarm-surge":e.displayName="SWARM SURGE";e.description="Fast river organisms surge toward the quarantine line.";e.minimumWave=8;e.modifiers.directEmergenceRatio=.8f;Bias((EnemyKind.Skitter,5),(EnemyKind.Broodling,4),(EnemyKind.Crawler,1.5f),(EnemyKind.Bulwark,.3f));break;
                case "armored-migration":e.displayName="ARMORED MIGRATION";e.description="Armored bottom-dwellers approach with corrosive support.";e.minimumWave=10;Bias((EnemyKind.Bulwark,7),(EnemyKind.Spitter,3),(EnemyKind.Skitter,.4f),(EnemyKind.Broodling,.2f));break;
                case "brood-tide":e.displayName="BROOD TIDE";e.description="Brood organisms release dense larval swarms.";e.minimumWave=7;e.modifiers.broodCarrierRatio=.8f;e.modifiers.directEmergenceRatio=.05f;Bias((EnemyKind.Broodling,9),(EnemyKind.Crawler,.4f),(EnemyKind.Bulwark,.2f));break;
                case "deep-water-disturbance":e.displayName="DEEP WATER DISTURBANCE";e.description="Unusual biological activity detected in deep water.";e.minimumWave=15;e.modifiers.broodCarrierRatio=.65f;e.modifiers.directEmergenceRatio=.1f;Bias((EnemyKind.Bulwark,5),(EnemyKind.Spitter,2),(EnemyKind.Burrower,2),(EnemyKind.Broodling,.3f));break;
                case "concentrated-emergence":e.displayName="CONCENTRATED EMERGENCE";e.description="River contacts concentrate on one announced emergence sector.";break;
                case "multi-sector":e.description="Biological activity detected across multiple emergence sectors.";break;
                default:e.description=e.description.Replace("boats","organisms").Replace("boat","river contact").Replace("landing","emergence").Replace("infected","creatures");break;
            }
            e.incompatibleEvents=e.incompatibleEvents.Select(ShorelineDefense.Persistence.AquaticSaveMigration.EventId).ToArray();EditorUtility.SetDirty(e);
        }
        foreach(string scene in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            var opened=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+scene+".unity");var system=Object.FindFirstObjectByType<AssaultCarrierSystem>();Refs(system,"carriers",carriers);Set(system,"boatPrefab",carriers[1].visualPrefab);
            foreach(var r in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))if(r.sprite!=null && (r.sprite.name.Contains("infected") || r.sprite.name.Contains("rubber_boat"))) {r.enabled=false;r.sprite=null;EditorUtility.SetDirty(r);}
            var map=Object.FindFirstObjectByType<MapRuntimeContext>().definition;map.description=scene=="CoastalDefense_TopDown"?"Hold the quarantine line against organisms emerging from the Danube.":"Defend narrow cargo lanes as aquatic creatures breach the docks.";EditorUtility.SetDirty(map);
            EditorSceneManager.MarkSceneDirty(opened);EditorSceneManager.SaveScene(opened);
        }
        Move(Data+"PirateRaider.asset","Assets/Game/Legacy/Phase4C/PirateRaider.asset");Move(Prefabs+"PirateRaider_TopDown.prefab","Assets/Game/Legacy/Phase4C/PirateRaider_TopDown.prefab");
        foreach(string raw in Directory.GetFiles(Art+"SourceSheets/p6b_aquatic_001","*.png").Concat(Directory.GetFiles(Art+"MasterReferences","ref_aquatic*.png")))
        {AssetDatabase.ImportAsset(raw);var imp=(TextureImporter)AssetImporter.GetAtPath(raw);imp.textureType=TextureImporterType.Default;imp.filterMode=FilterMode.Point;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.mipmapEnabled=false;imp.maxTextureSize=4096;imp.SaveAndReimport();}
        AssetDatabase.SaveAssets();File.WriteAllText("Docs/Validation/Phase6B/import-receipt.json",JsonConvert.SerializeObject(receipt,Formatting.Indented));return "Aquatic roster, carriers, bosses, events and both saved maps integrated through Unity APIs.";
    }
    public static string BindMaps()
    {
        if(Application.isPlaying)throw new Exception("Edit mode required");
        foreach(string name in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+name+".unity");
            // Opening a scene can unload newly authored assets; resolve durable references afresh.
            var definitions=new[]{"direct_emergence","drift_pod","brood_carrier","leviathan_carrier"}.Select(n=>AssetDatabase.LoadAssetAtPath<CarrierDefinition>("Assets/Game/Data/Carriers/"+n+".asset")).ToArray();
            if(definitions.Any(d=>d==null))throw new Exception("Missing durable carrier definition");
            var system=Object.FindFirstObjectByType<AssaultCarrierSystem>();Refs(system,"carriers",definitions);Set(system,"boatPrefab",definitions[1].visualPrefab);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();return "Both maps rebound to durable carrier definitions.";
    }
}
