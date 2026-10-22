using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class Phase3A2ValidateClips
{
    public static string Run()
    {
        var active=SceneManager.GetActiveScene();bool dirty=active.isDirty;
        var preview=EditorSceneManager.NewPreviewScene();var results=new List<object>();int frames=0;
        try {
            var root=new GameObject("Enemy clip validation");SceneManager.MoveGameObjectToScene(root,preview);
            var visual=new GameObject("Visual");visual.transform.SetParent(root.transform,false);
            var body=new GameObject("Body");body.transform.SetParent(visual.transform,false);
            var renderer=body.AddComponent<SpriteRenderer>();
            root.AddComponent<Animator>();
            foreach(JObject e in JArray.Parse(File.ReadAllText("Docs/Art/Batches/p3a2_animation_manifest.json"))) {
                string clipPath="Assets/Game/Animations/Enemies/"+(string)e["folder"]+"/bd_enemy_"+(string)e["id"]+"_"+(string)e["action"]+"_e_v001.anim";
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                var sprites=AssetDatabase.LoadAllAssetsAtPath((string)e["output"]).OfType<Sprite>().OrderBy(s=>s.name,StringComparer.Ordinal).ToArray();
                int count=(int)e["count"];float fps=(float)e["fps"];
                for(int i=0;i<count;i++){
                    renderer.sprite=null;clip.SampleAnimation(root,(i+.25f)/fps);
                    if(renderer.sprite!=sprites[i])throw new InvalidOperationException("Wrong sampled frame: "+clipPath+" #"+i+" expected="+sprites[i].name+" actual="+(renderer.sprite==null?"null":renderer.sprite.name));
                    if(root.transform.position!=Vector3.zero||root.transform.rotation!=Quaternion.identity||root.transform.localScale!=Vector3.one||
                       body.transform.localPosition!=Vector3.zero||body.transform.localScale!=Vector3.one)
                        throw new InvalidOperationException("Clip changed transform: "+clipPath);
                    frames++;
                }
                var bindings=AnimationUtility.GetObjectReferenceCurveBindings(clip);
                if(bindings.Length!=1||bindings[0].path!="Visual/Body"||bindings[0].propertyName!="m_Sprite"||
                   AnimationUtility.GetCurveBindings(clip).Length!=0||AnimationUtility.GetAnimationEvents(clip).Length!=0)
                    throw new InvalidOperationException("Unexpected clip content: "+clipPath);
                bool loop=AnimationUtility.GetAnimationClipSettings(clip).loopTime;
                if(loop!=(bool)e["loop"]||Mathf.Abs(clip.length-count/fps)>.001f)throw new InvalidOperationException("Timing mismatch: "+clipPath);
                results.Add(new{clipPath,count,fps,loop,duration=clip.length,samplingPassed=true,transformsUnchanged=true});
            }
        } finally {EditorSceneManager.ClosePreviewScene(preview);}
        if(SceneManager.GetActiveScene()!=active||active.isDirty!=dirty)throw new InvalidOperationException("Changed active scene state");
        File.WriteAllText("Docs/Validation/Phase3A2/clip-playback-validation.json",JsonConvert.SerializeObject(new{clips=results,sampledFrames=frames,activeSceneUnchanged=true},Formatting.Indented));
        return frames+" sprite keys sampled successfully across "+results.Count+" clips; active scene unchanged";
    }
}
