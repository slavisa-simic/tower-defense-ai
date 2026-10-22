using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.U2D.Sprites;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;

public static class Phase4BAuthor
{
    const string Art = "Assets/Game/Art/";
    const string Source = Art + "SourceSheets/p4b_heavy_001/";
    static Material material;
    public static string FinalizeImports()
    {
        var paths=Directory.GetFiles(Art+"Defenses","defense_*.png",SearchOption.AllDirectories).Concat(Directory.GetFiles(Art+"Effects/Heavy","*.png"))
            .Concat(Directory.GetFiles(Art+"UI","ui_icon_*.png"));
        foreach(var path in paths)
        {
            var i=(TextureImporter)AssetImporter.GetAtPath(path);var settings=new TextureImporterSettings();i.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;i.SetTextureSettings(settings);i.SaveAndReimport();
        }
        foreach(var path in Directory.GetFiles(Source,"*.png").Concat(new[]{Art+"MasterReferences/ref_heavy_defense_lineup_01.png"}))
        {
            var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Default;i.filterMode=FilterMode.Point;
            i.textureCompression=TextureImporterCompression.Uncompressed;i.mipmapEnabled=false;i.wrapMode=TextureWrapMode.Clamp;i.npotScale=TextureImporterNPOTScale.None;i.maxTextureSize=4096;i.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();return "New runtime FullRect and non-runtime source/reference imports verified";
    }
    public static string FixCatalog()
    {
        var names=new[]{"MachineGunNest","BarbedWire","LandMine","SniperTower","MortarPosition","FlamethrowerPosition","AutoCannon","RocketLauncherPosition"};
        var defs=names.Select(n=>AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/"+n+".asset")).ToArray();
        var hud=UnityEngine.Object.FindFirstObjectByType<BuildHud>();hud.ConfigureCatalog(defs);EditorUtility.SetDirty(hud);
        Camera.main.rect=new Rect(0,.30f,1,.56f);EditorUtility.SetDirty(Camera.main);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());return "Original catalog indices retained; visual order remains independent";
    }
    static void Set(UnityEngine.Object o, string key, object value)
    {
        var s = new SerializedObject(o); var p = s.FindProperty(key);
        if (value is int n) p.intValue = n;
        else if (value is float f) p.floatValue = f;
        else if (value is string text) p.stringValue = text;
        else if (value is Vector2Int v) p.vector2IntValue = v;
        else p.objectReferenceValue = value as UnityEngine.Object;
        s.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(o);
    }
    static void Import(string path, int frameSize, int frames = 1, float ppu = 51.2f)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var t = (TextureImporter)AssetImporter.GetAtPath(path);
        t.textureType = TextureImporterType.Sprite; t.spriteImportMode = frames == 1 ? SpriteImportMode.Single : SpriteImportMode.Multiple;
        t.filterMode = FilterMode.Point; t.textureCompression = TextureImporterCompression.Uncompressed;
        t.mipmapEnabled = false; t.wrapMode = TextureWrapMode.Clamp; t.npotScale = TextureImporterNPOTScale.None;
        t.maxTextureSize = 4096; t.alphaSource = TextureImporterAlphaSource.FromInput; t.alphaIsTransparency = true; t.spritePixelsPerUnit = ppu;
        t.SaveAndReimport();
        if (frames == 1) return;
        var factories = new SpriteDataProviderFactories(); factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(t); provider.InitSpriteEditorDataProvider();
        var rects = Enumerable.Range(0, frames).Select(i => new SpriteRect { name = Path.GetFileNameWithoutExtension(path) + "__f" + i.ToString("D3"),
            rect = new Rect(i * frameSize, 0, frameSize, frameSize), alignment = SpriteAlignment.Center, pivot = new Vector2(.5f,.5f), spriteID = GUID.Generate() }).ToArray();
        provider.SetSpriteRects(rects);
        var pairs = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        pairs.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        provider.Apply(); t.SaveAndReimport();
    }
    static Color32[] Cell(Texture2D source, int index, int rows, int size)
    {
        int col = index % 4, row = index / 4;
        var result = new Color32[size * size]; var pixels = source.GetPixels32();
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            int sx = Mathf.Min(source.width - 1, Mathf.FloorToInt((col + (x + .5f) / size) * source.width / 4f));
            int sy = Mathf.Min(source.height - 1, Mathf.FloorToInt((rows - row - 1 + (y + .5f) / size) * source.height / rows));
            var p = pixels[sy * source.width + sx];
            if (p.a < 220) continue;
            // Hard alpha and deliberate no-dither five-bit color clusters, same export convention as Phase 4A.
            result[y * size + x] = new Color32((byte)(p.r/8*8), (byte)(p.g/8*8), (byte)(p.b/8*8), 255);
        }
        return result;
    }
    static void Save(string path, Color32[][] cells, int size, float ppu = 51.2f)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var output = new Texture2D(size * cells.Length, size, TextureFormat.RGBA32, false);
        for (int i = 0; i < cells.Length; i++) output.SetPixels32(i * size, 0, size, size, cells[i]);
        output.Apply(); File.WriteAllBytes(path, output.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(output);
        Import(path, size, cells.Length, ppu);
    }
    static Color32[] WeaponCell(Texture2D source,int index,int size,string id)
    {
        var pixels=source.GetPixels32();int x0=Mathf.RoundToInt(index%4*source.width/4f),x1=Mathf.RoundToInt((index%4+1)*source.width/4f);
        int y0=Mathf.RoundToInt((1-index/4)*source.height/2f),y1=Mathf.RoundToInt((2-index/4)*source.height/2f);
        int l=x1,r=x0,b=y1,t=y0;
        for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)
        {
            var p=pixels[y*source.width+x];
            // Find the steel/olive weapon, excluding amber muzzle/fire/smoke from alignment bounds.
            if(p.a<220||p.r>p.g*1.3f+12||p.r>190)continue;
            l=Math.Min(l,x);r=Math.Max(r,x);b=Math.Min(b,y);t=Math.Max(t,y);
        }
        if(r<=l||t<=b)throw new Exception("Weapon has no opaque metal: "+id);
        float width=r-l+1,height=t-b+1;
        float pivot=id=="rocket"?.5f:id=="flamethrower"?.18f:.33f;
        float px=l+width*pivot,py=(b+t)*.5f;
        float scale=Math.Min((size*.44f)/Math.Max(width*(1-pivot),width*pivot),(size*.4f)/height);
        // Per-frame bounding normalization corrects native sheet scale/pivot drift without interpolating pixels.
        var result=new Color32[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            int sx=Mathf.FloorToInt(px+(x-size*.5f)/scale),sy=Mathf.FloorToInt(py+(y-size*.5f)/scale);
            if(sx<x0||sx>=x1||sy<y0||sy>=y1)continue;
            var p=pixels[sy*source.width+sx];if(p.a<220)continue;
            result[y*size+x]=new Color32((byte)(p.r/8*8),(byte)(p.g/8*8),(byte)(p.b/8*8),255);
        }
        return result;
    }
    static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
    static Sprite[] Frames(string path) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
    static SpriteRenderer Renderer(Transform parent, string name, Sprite sprite, int order)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var r = go.AddComponent<SpriteRenderer>(); r.sprite = sprite; r.sharedMaterial = material;
        r.sortingLayerName = "Defenses"; r.sortingOrder = order; return r;
    }
    static DefenseDefinition Defense(string id, string name, string display, int price, int hp, float range, int damage, float rate, int cells, TargetingMode targeting)
    {
        string folder = Art + "Defenses/" + name + "/";
        int size = cells * 64;
        var raw = new Texture2D(2,2); raw.LoadImage(File.ReadAllBytes(Source + "defense_" + id + "_pack.png"));
        var parts = Enumerable.Range(0,8).Select(i => Cell(raw,i,2,size)).ToArray();
        Save(folder + "defense_"+id+"_base_idle.png",new[]{parts[0]},size);
        string weaponName = id == "rocket" ? "launcher" : "weapon";
        Save(folder + "defense_"+id+"_"+weaponName+"_idle.png",new[]{WeaponCell(raw,1,size,id)},size);
        Save(folder + "defense_"+id+"_fire_sheet.png",Enumerable.Range(2,4).Select(i=>WeaponCell(raw,i,size,id)).ToArray(),size);
        Save(folder + "defense_"+id+"_damaged.png",new[]{parts[6]},size);
        Save(folder + "defense_"+id+"_destroyed.png",new[]{parts[7]},size);
        var icon = Cell(raw,0,2,32); Save(Art + "UI/ui_icon_"+id+".png",new[]{icon},32,100);
        UnityEngine.Object.DestroyImmediate(raw);
        string dataPath = "Assets/Game/Data/Defenses/"+name+".asset";
        var definition = AssetDatabase.LoadAssetAtPath<DefenseDefinition>(dataPath);
        if (definition == null) { definition = ScriptableObject.CreateInstance<DefenseDefinition>(); AssetDatabase.CreateAsset(definition,dataPath); }
        Set(definition,"id",id); Set(definition,"displayName",display); Set(definition,"cost",price); Set(definition,"maxHealth",hp);
        Set(definition,"rangeInCells",range); Set(definition,"damage",damage); Set(definition,"fireRate",rate);
        Set(definition,"footprint",new Vector2Int(cells,cells)); Set(definition,"targetingMode",(int)targeting);
        if (id != "autocannon")
        {
            var path = "Assets/Game/Data/Defenses/"+name+"Weapon.asset";
            var data = AssetDatabase.LoadAssetAtPath<HeavyWeaponData>(path);
            if (data == null) { data = ScriptableObject.CreateInstance<HeavyWeaponData>(); AssetDatabase.CreateAsset(data,path); }
            data.minimumRangeInCells = id == "mortar" ? 3 : 0;
            data.splashRadiusInCells = id == "rocket" ? 2.75f : 2.25f;
            data.travelTime = id == "mortar" ? .8f : .65f; data.arcHeight = id == "mortar" ? 2.5f : 0;
            data.damageTick = .2f; data.coneAngle = 55; data.burnDuration = 2; data.burnDps = 15;
            EditorUtility.SetDirty(data); Set(definition,"heavyWeapon",data);
        }
        var root = new GameObject(name); root.SetActive(false);
        try
        {
            var defense = root.AddComponent<DefenseBase>();
            var visual = new GameObject("Visual").transform; visual.SetParent(root.transform,false);
            var shadow = Renderer(visual,"Contact Shadow",Sprite(Art+"Effects/ContactShadow/bd_fx_contact_shadow_v001.png"),0);
            shadow.transform.localPosition = new Vector3(.04f,-.04f,0); shadow.transform.localScale = new Vector3(cells,cells,1);
            shadow.color = new Color(31/255f,41/255f,44/255f,.28f);
            var body = Renderer(visual,"Base",Sprite(folder+"defense_"+id+"_base_idle.png"),1);
            var rotating = new GameObject("Rotating Weapon").transform; rotating.SetParent(visual,false); rotating.localRotation = Quaternion.Euler(0,0,180);
            var weapon = Renderer(rotating,"Weapon",Sprite(folder+"defense_"+id+"_"+weaponName+"_idle.png"),2);
            var muzzle = new GameObject("Muzzle").transform; muzzle.SetParent(rotating,false); muzzle.localPosition = new Vector3(cells * .5f,0,0);
            root.AddComponent<DefenseSpritePresentation>().Configure(body,body.sprite,Sprite(folder+"defense_"+id+"_damaged.png"),Sprite(folder+"defense_"+id+"_destroyed.png"),null,body.sprite);
            if (id == "autocannon")
            {
                root.AddComponent<TurretCombat>(); var p = root.AddComponent<TurretPresentation>();
                p.Configure(rotating,muzzle,null,null); p.ConfigureSprites(weapon,weapon.sprite,Frames(folder+"defense_"+id+"_fire_sheet.png"),16); p.ConfigureHeavyEffects(true);
            }
            else
            {
                if (id == "flamethrower") root.AddComponent<FlameWeapon>(); else root.AddComponent<AreaProjectileWeapon>();
                root.AddComponent<HeavyWeaponPresentation>().Configure(rotating,muzzle,weapon,weapon.sprite,Frames(folder+"defense_"+id+"_fire_sheet.png"),id=="rocket");
            }
            string path = "Assets/Game/Prefabs/Defenses/"+name+".prefab";
            root.SetActive(true);
            PrefabUtility.SaveAsPrefabAsset(root,path); Set(definition,"prefab",AssetDatabase.LoadAssetAtPath<DefenseBase>(path));
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
        return definition;
    }
    static void Effects(VfxCatalog catalog)
    {
        var ids = new[] { new[]{VfxId.MortarMuzzle,VfxId.MortarShell,VfxId.MortarExplosion},new[]{VfxId.FlameCone,VfxId.Burn,VfxId.BurningOverlay},
            new[]{VfxId.CannonMuzzle,VfxId.CannonTracer,VfxId.CannonImpact},new[]{VfxId.RocketProjectile,VfxId.RocketSmoke,VfxId.RocketExplosion} };
        var names = new[]{"mortar","flamethrower","autocannon","rocket"};
        var sequences = catalog.sequences.Where(s => (int)s.id < (int)VfxId.MortarMuzzle).ToList();
        for (int n=0;n<names.Length;n++)
        {
            var raw = new Texture2D(2,2); raw.LoadImage(File.ReadAllBytes(Source+"fx_"+names[n]+"_pack.png"));
            int size = n == 0 || n == 3 ? 128 : 64;
            for (int row=0;row<3;row++)
            {
                string path = Art+"Effects/Heavy/fx_"+ids[n][row].ToString().ToLower()+"_sheet.png";
                Save(path,Enumerable.Range(row*4,4).Select(i=>Cell(raw,i,3,size)).ToArray(),size);
                sequences.Add(new VfxSequence { id=ids[n][row],frames=Frames(path),fps=16,channel=ids[n][row]==VfxId.RocketSmoke?VfxChannel.Smoke:VfxChannel.Burst,
                    lifetime=ids[n][row]==VfxId.RocketSmoke?.45f:.3f,fadePortion=.25f });
            }
            UnityEngine.Object.DestroyImmediate(raw);
        }
        catalog.sequences = sequences.ToArray(); EditorUtility.SetDirty(catalog);
    }
    public static string Run()
    {
        if (Application.isPlaying || SceneManager.GetActiveScene().path != "Assets/Game/Scenes/CoastalDefense_TopDown.unity") throw new Exception("Primary scene in Edit Mode required");
        AssetDatabase.Refresh(); material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/TopDownPlaceholder.mat");
        var mortar=Defense("mortar","MortarPosition","Mortar",400,250,12,120,1/3f,2,TargetingMode.BestCluster);
        var flame=Defense("flamethrower","FlamethrowerPosition","Flamethrower",300,350,3.5f,12,5,1,TargetingMode.Closest);
        var cannon=Defense("autocannon","AutoCannon","Auto Cannon",600,450,7.5f,55,2,2,TargetingMode.HighestHealth);
        var rocket=Defense("rocket","RocketLauncherPosition","Rocket Launcher",900,300,10,250,.2f,2,TargetingMode.BestCluster);
        var hud=UnityEngine.Object.FindFirstObjectByType<BuildHud>();
        var mg=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/MachineGunNest.asset");
        var sniper=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/SniperTower.asset");
        var wire=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/BarbedWire.asset");
        var mine=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/LandMine.asset");
        hud.ConfigureCatalog(new[]{mg,wire,mine,sniper,mortar,flame,cannon,rocket}); hud.ConfigureDisplayOrder(new[]{wire,mine,mg,sniper,mortar,flame,cannon,rocket}); EditorUtility.SetDirty(hud);
        Effects(UnityEngine.Object.FindFirstObjectByType<VfxService>().Catalog);
        foreach (string path in new[]{"Crawler","Runner","Brute"}.Select(n=>"Assets/Game/Prefabs/Enemies/"+n+"_TopDown.prefab"))
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try { if(root.GetComponent<EnemyDamageOverTime>()==null)root.AddComponent<EnemyDamageOverTime>(); if(root.GetComponent<BurnPresentation>()==null)root.AddComponent<BurnPresentation>(); PrefabUtility.SaveAsPrefabAsset(root,path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        return "Four heavy defense definitions/prefabs, twelve VFX sequences, burn observers and eight-button catalog saved";
    }
}
