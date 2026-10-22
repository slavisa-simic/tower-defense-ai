using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShorelineDefense.Presentation;

public static class VfxAssetValidation
{
    public static string Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode");
        var entries=JArray.Parse(File.ReadAllText("Docs/Art/Batches/p3b_manifest.json"));
        var catalog=AssetDatabase.LoadAssetAtPath<VfxCatalog>("Assets/Game/Art/Effects/CombatVfxCatalog.asset");
        var rows=new List<object>();int clips=0;
        var rig=new GameObject("Temporary VFX clip audit");var renderer=rig.AddComponent<SpriteRenderer>();rig.AddComponent<Animator>();
        try
        {
            foreach(JObject e in entries)
            {
                string path=(string)e["path"];var t=(TextureImporter)AssetImporter.GetAtPath(path);
                var seq=catalog.Get((VfxId)Enum.Parse(typeof(VfxId),(string)e["id"]));
                if(seq==null||seq.frames.Length!=(int)e["count"]||seq.frames.Any(s=>s==null))throw new Exception("Catalog frames "+path);
                if(t.filterMode!=FilterMode.Point||t.mipmapEnabled||t.isReadable||t.textureCompression!=TextureImporterCompression.Uncompressed||Math.Abs(t.spritePixelsPerUnit-51.2f)>.001f)throw new Exception("Importer "+path);
                if(!SortingLayer.layers.Any(s=>s.name==seq.sortingLayer))throw new Exception("Sorting "+path);
                int sampled=0;
                if(seq.frames.Length>1)
                {
                    var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Game/Animations/Effects/bd_fx_"+(string)e["name"]+"_v001.anim");
                    if(clip==null||AnimationUtility.GetAnimationEvents(clip).Length!=0||AnimationUtility.GetCurveBindings(clip).Length!=0)throw new Exception("Unsafe clip "+path);
                    var bindings=AnimationUtility.GetObjectReferenceCurveBindings(clip);
                    if(bindings.Length!=1||bindings[0].type!=typeof(SpriteRenderer)||bindings[0].propertyName!="m_Sprite")throw new Exception("Clip bindings "+path);
                    for(int i=0;i<seq.frames.Length;i++)
                    {clip.SampleAnimation(rig,(i+.01f)/seq.fps);if(renderer.sprite!=seq.frames[i])throw new Exception("Clip sample "+path+" "+i);sampled++;}
                    clips++;
                }
                rows.Add(new{path,frames=seq.frames.Length,sampled,pass=true});
            }
        }
        finally{UnityEngine.Object.DestroyImmediate(rig);}
        File.WriteAllText("Docs/Validation/Phase3B/asset-validation.json",JsonConvert.SerializeObject(new{sequences=entries.Count,clips,pass=true,rows},Formatting.Indented));
        return "Validated "+entries.Count+" sequences and sampled every frame of "+clips+" sprite-only clips";
    }
    public static string Cleanup()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode");
        string root=Path.GetFullPath("Assets/Docs");
        string expected=Path.Combine(Path.GetFullPath("Assets"),"Docs");
        if(root!=expected)throw new Exception("Unexpected cleanup root");
        if(Directory.Exists(root))
        {
            var names=new[]{"death-reward-hud.png","wave-start-hud.png","wave-complete-hud.png","game-over-hud.png","stress-300-hud.png"};
            foreach(var f in Directory.GetFiles(root,"*",SearchOption.AllDirectories))
            {
                if(f.EndsWith(".meta"))continue;
                if(!names.Contains(Path.GetFileName(f)))throw new Exception("Unrelated file in screenshot staging: "+f);
                string destination="Docs/Validation/Phase3B/"+Path.GetFileName(f);
                if(!File.Exists(destination)||!File.ReadAllBytes(f).SequenceEqual(File.ReadAllBytes(destination)))throw new Exception("Screenshot not preserved: "+f);
            }
            if(!AssetDatabase.DeleteAsset("Assets/Docs"))throw new Exception("Screenshot staging cleanup failed");
        }
        Time.timeScale=1;Time.maximumDeltaTime=.33333334f;Application.runInBackground=false;
        return "Verified screenshot copies and removed only temporary Assets/Docs staging through AssetDatabase";
    }
}
