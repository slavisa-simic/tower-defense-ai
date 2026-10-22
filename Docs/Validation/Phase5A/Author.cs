using System;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;

public static class Phase5AAuthor
{
    public static string Integrate()
    {
        if (EditorApplication.isPlaying) throw new Exception("Author only in Edit Mode");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Game/Scenes/CoastalDefense_TopDown.unity" || scene.isDirty) throw new Exception("Requires clean primary scene");
        var build = UnityEngine.Object.FindFirstObjectByType<BuildManager>();
        var placement = UnityEngine.Object.FindFirstObjectByType<PlacementController>();
        var game = build.Game;
        var wave = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
        var selection = Get<StructureSelection>(placement.gameObject); selection.Configure(build, placement);
        UnityEngine.Object.FindFirstObjectByType<PlacementInput>().ConfigureSelection(selection);
        var speed = Get<GameSpeedController>(game.gameObject); speed.Configure(game);
        var ui = UnityEngine.Object.FindFirstObjectByType<BuildHud>();
        Get<StructureHud>(ui.gameObject).Configure(build, selection, speed, wave);
        Get<StructureSelectionPresentation>(placement.gameObject).Configure(selection);
        build.ConfigureWaves(wave);
        foreach (var definition in ui.Defenses)
        {
            var ii = definition.LevelData(2); var iii = definition.LevelData(3);
            ii.cost = Mathf.CeilToInt(definition.Cost * .75f); iii.cost = Mathf.CeilToInt(definition.Cost * 1.25f);
            ii.health=1.25f;iii.health=1.5f;ii.damage=1.25f;iii.damage=1.6f;ii.fireRate=1;iii.fireRate=1;
            ii.range=1;iii.range=1;ii.splash=1;iii.splash=1;ii.slow=1;iii.slow=1;
            if(definition.Id=="machine-gun-nest") { ii.fireRate=1.1f;iii.fireRate=1.2f;iii.range=1.15f; }
            if(definition.Id=="sniper-tower") { ii.range=1.1f;iii.range=1.15f;iii.fireRate=1.2f; }
            if(definition.Id=="barbed-wire") { ii.health=1.5f;iii.health=2;ii.slow=.8f;iii.slow=.6f; }
            if(definition.Id=="land-mine") { ii.health=1;iii.health=1;iii.splash=1.2f;iii.range=1.2f; }
            if(definition.Id=="mortar" || definition.Id=="rocket") { ii.damage=1;ii.fireRate=1.25f;iii.fireRate=1.25f;iii.splash=1.2f; }
            if(definition.Id=="autocannon") { iii.fireRate=1.2f;iii.range=1.1f; }
            if(definition.Id=="flamethrower") { ii.range=1.1f;iii.range=1.15f;ii.burnDps=1.25f;iii.burnDps=1.6f;iii.burnDuration=1.5f; }
            EditorUtility.SetDirty(definition);
            string path=AssetDatabase.GetAssetPath(definition.Prefab);
            var root=PrefabUtility.LoadPrefabContents(path);
            Get<DefenseUpgradePresentation>(root);
            PrefabUtility.SaveAsPrefabAsset(root,path); PrefabUtility.UnloadPrefabContents(root);
        }
        EditorUtility.SetDirty(build);EditorUtility.SetDirty(placement);EditorUtility.SetDirty(game);EditorUtility.SetDirty(ui);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        return "Selection, HUD, speed, eight upgrade paths/prefabs saved";
    }
    private static T Get<T>(GameObject go) where T:Component => go.GetComponent<T>() ?? go.AddComponent<T>();
    public static string ExportArt()
    {
        // Same nearest-neighbor, hard-alpha runtime export convention as Phase 4B.
        const string source="Assets/Game/Art/SourceSheets/p5a_upgrades_001/bd_src_upgrade_overlays_v002.png";
        var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);tex.LoadImage(File.ReadAllBytes(source));
        var pixels=tex.GetPixels32();var output=new Texture2D(256,256,TextureFormat.RGBA32,false);var result=new Color32[256*256];
        for(int i=0;i<16;i++)
        {
            int cw=tex.width/4,ch=tex.height/4,ox=(i%4)*cw,oy=(3-i/4)*ch;
            int minX=cw,minY=ch,maxX=0,maxY=0;
            for(int y=0;y<ch;y++)for(int x=0;x<cw;x++)if(pixels[(oy+y)*tex.width+ox+x].a>=220){minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);}
            if(minX>maxX)throw new Exception("Empty art cell "+i);
            int w=maxX-minX+1,h=maxY-minY+1;float scale=Math.Min(60f/w,60f/h);int dw=Mathf.RoundToInt(w*scale),dh=Mathf.RoundToInt(h*scale);
            for(int y=0;y<dh;y++)for(int x=0;x<dw;x++)
            {
                int sx=minX+Math.Min(w-1,Mathf.FloorToInt((x+.5f)*w/dw)),sy=minY+Math.Min(h-1,Mathf.FloorToInt((y+.5f)*h/dh));
                var p=pixels[(oy+sy)*tex.width+ox+sx];if(p.a<220)continue;
                result[((3-i/4)*64+(64-dh)/2+y)*256+(i%4)*64+(64-dw)/2+x]=new Color32((byte)(p.r/8*8),(byte)(p.g/8*8),(byte)(p.b/8*8),255);
            }
        }
        output.SetPixels32(result);output.Apply();File.WriteAllBytes("Assets/Game/Art/Defenses/bd_defense_upgrade_overlays_sheet_v001.png",output.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(output);return "256x256 atlas, 16 centered 64px cells exported";
    }
    public static string BindArt()
    {
        if(EditorApplication.isPlaying)throw new Exception("Not authorable during play");
        const string path="Assets/Game/Art/Defenses/bd_defense_upgrade_overlays_sheet_v001.png";
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit=51.2f;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=2048;
        importer.alphaIsTransparency=true;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteGenerateFallbackPhysicsShape=false;importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        var factories=new SpriteDataProviderFactories();factories.Init();
        var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        var old=provider.GetSpriteRects();
        var cells=new SpriteRect[16];
        for(int i=0;i<16;i++)cells[i]=new SpriteRect{name="upgrade_"+i,rect=new Rect((i%4)*64,(3-i/4)*64,64,64),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f),spriteID=old.FirstOrDefault(r=>r.name=="upgrade_"+i)?.spriteID ?? GUID.Generate()};
        provider.SetSpriteRects(cells);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(cells.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();importer.SaveAndReimport();
        var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        string[] ids={"machine-gun-nest","sniper-tower","barbed-wire","land-mine","mortar","flamethrower","autocannon","rocket"};
        foreach(string guid in AssetDatabase.FindAssets("t:DefenseDefinition",new[]{"Assets/Game/Data/Defenses"}))
        {
            var d=AssetDatabase.LoadAssetAtPath<DefenseDefinition>(AssetDatabase.GUIDToAssetPath(guid));int index=Array.IndexOf(ids,d.Id);
            if(index<0)continue;
            d.LevelData(2).overlay=sprites.Single(s=>s.name=="upgrade_"+(index*2));d.LevelData(3).overlay=sprites.Single(s=>s.name=="upgrade_"+(index*2+1));EditorUtility.SetDirty(d);
        }
        AssetDatabase.SaveAssets();return "16 overlay cells bound";
    }
}
