using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class Phase3A2Import
{
    const string Manifest = "Docs/Art/Batches/p3a2_animation_manifest.json";
    static void Folder(string path)
    {
        if(AssetDatabase.IsValidFolder(path)) return;
        string parent=Path.GetDirectoryName(path).Replace('\\','/');
        Folder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
    }
    static TextureImporter Settings(string path,bool runtime)
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
            var settings=new TextureImporterSettings();t.ReadTextureSettings(settings);
            settings.spriteMode=(int)SpriteImportMode.Multiple;settings.spritePixelsPerUnit=51.2f;
            settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.Center;
            settings.spritePivot=new Vector2(.5f,.5f);settings.spriteGenerateFallbackPhysicsShape=false;
            t.SetTextureSettings(settings);
        }
        t.SaveAndReimport();return t;
    }
    public static string Run()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before importing");
        var entries=JArray.Parse(File.ReadAllText(Manifest));var rows=new List<object>();
        foreach(JObject e in entries)
        {
            string path=(string)e["output"];
            if(!path.StartsWith("Assets/Game/Art/Enemies/")||path.Contains(".."))throw new InvalidOperationException(path);
            int n=(int)e["frame"],count=(int)e["count"];float fps=(float)e["fps"];bool loop=(bool)e["loop"];
            var t=Settings(path,true);
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(t);provider.InitSpriteEditorDataProvider();
            var existing=provider.GetSpriteRects().ToDictionary(s=>s.name,s=>s.spriteID);
            string stem="bd_enemy_"+(string)e["id"]+"_"+(string)e["action"]+"_e";
            var rects=Enumerable.Range(0,count).Select(i=>{
                string name=stem+"__f"+i.ToString("D3");
                return new SpriteRect{name=name,rect=new Rect(i*n,0,n,n),alignment=SpriteAlignment.Center,
                    pivot=new Vector2(.5f,.5f),spriteID=existing.ContainsKey(name)?existing[name]:GUID.Generate()};
            }).ToArray();
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(
                rects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
            provider.Apply();t.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name,StringComparer.Ordinal).ToArray();
            if(texture.width!=n*count||texture.height!=n||sprites.Length!=count)throw new InvalidOperationException("Wrong frame geometry: "+path);
            for(int i=0;i<count;i++)
                if(sprites[i].name!=rects[i].name||sprites[i].rect!=rects[i].rect||sprites[i].pivot!=new Vector2(n/2f,n/2f)||Mathf.Abs(sprites[i].pixelsPerUnit-51.2f)>.001f)
                    throw new InvalidOperationException("Wrong sprite metadata: "+sprites[i].name);
            string folder="Assets/Game/Animations/Enemies/"+(string)e["folder"];Folder(folder);
            string clipPath=folder+"/"+stem+"_v001.anim";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,clipPath);}
            clip.frameRate=fps;clip.wrapMode=loop?WrapMode.Loop:WrapMode.ClampForever;
            var binding=new EditorCurveBinding{path="Visual/Body",type=typeof(SpriteRenderer),propertyName="m_Sprite"};
            // Unity includes one frame of duration after the final discrete sprite key.
            var keys=new ObjectReferenceKeyframe[count];
            for(int i=0;i<count;i++)keys[i]=new ObjectReferenceKeyframe{time=i/fps,value=sprites[i]};
            AnimationUtility.SetObjectReferenceCurve(clip,binding,keys);
            var cs=AnimationUtility.GetAnimationClipSettings(clip);cs.loopTime=loop;cs.startTime=0;cs.stopTime=count/fps;
            AnimationUtility.SetAnimationClipSettings(clip,cs);AnimationUtility.SetAnimationEvents(clip,Array.Empty<AnimationEvent>());
            EditorUtility.SetDirty(clip);
            if(AnimationUtility.GetCurveBindings(clip).Length!=0||AnimationUtility.GetObjectReferenceCurveBindings(clip).Length!=1)
                throw new InvalidOperationException("Unexpected animation bindings: "+clipPath);
            var frameRows=sprites.Select(s=>{
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s,out string guid,out long fileId);
                return new {s.name,rect=new[]{s.rect.x,s.rect.y,s.rect.width,s.rect.height},pivot=new[]{s.pivot.x,s.pivot.y},s.pixelsPerUnit,guid,fileId};
            }).ToArray();
            rows.Add(new{path,texture.width,texture.height,guid=AssetDatabase.AssetPathToGUID(path),
                filter=t.filterMode.ToString(),compression=t.textureCompression.ToString(),t.mipmapEnabled,
                t.spritePixelsPerUnit,t.alphaIsTransparency,t.isReadable,t.sRGBTexture,t.maxTextureSize,
                wrap=t.wrapMode.ToString(),npot=t.npotScale.ToString(),frames=frameRows,
                clipPath,clipGuid=AssetDatabase.AssetPathToGUID(clipPath),fps,loop,duration=clip.length,
                keys=keys.Length,binding="Visual/Body:SpriteRenderer.m_Sprite",events=0,floatCurves=0});
        }
        var archivePaths=Directory.GetFiles("Assets/Game/Art/SourceSheets/p3a2_core_enemies_001","*.png")
            .Concat(Directory.GetFiles("Assets/Game/Art/MasterReferences","bd_ref_enemy_*.png"));
        var archives=new List<object>();
        foreach(var p in archivePaths)
        {
            string path=p.Replace('\\','/');var t=Settings(path,false);var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            archives.Add(new{path,type=t.textureType.ToString(),tex.width,tex.height,filter=t.filterMode.ToString(),t.mipmapEnabled});
        }
        AssetDatabase.SaveAssets();
        File.WriteAllText("Docs/Validation/Phase3A2/unity-imports.json",JsonConvert.SerializeObject(new{runtime=rows,archives},Formatting.Indented));
        return "Validated "+rows.Count+" sheets and unbound sprite-only clips; "+archives.Count+" non-runtime textures";
    }
}
