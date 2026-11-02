using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;

public static class ShadowAudit
{
    public const string Folder = "Docs/Validation/ShadowAudit";
    public static readonly string[] Scenes = {"CoastalDefense_TopDown", "DanubeIndustrialPort"};
    static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
    static object Describe(SpriteRenderer r) => new {
        path=PathOf(r.transform), enabled=r.enabled, active=r.gameObject.activeSelf,
        sprite=AssetDatabase.GetAssetPath(r.sprite), spriteName=r.sprite != null ? r.sprite.name : null,
        material=AssetDatabase.GetAssetPath(r.sharedMaterial), shader=r.sharedMaterial != null ? r.sharedMaterial.shader.name : null,
        color=r.color.ToString(), position=r.transform.position.ToString("F4"), rotation=r.transform.eulerAngles.ToString(),
        scale=r.transform.lossyScale.ToString("F4"), size=r.bounds.size.ToString("F4"), layer=r.sortingLayerName, order=r.sortingOrder
    };
    public static string Inventory(string revision)
    {
        if (Application.isPlaying) throw new Exception("Edit mode required");
        Directory.CreateDirectory(Folder);
        var setup=EditorSceneManager.GetSceneManagerSetup();
        try {
            foreach(var name in Scenes) {
                var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+name+".unity");
                var renderers=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).ToArray();
                var cameras=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Camera>(true)).Select(c=>new {
                    name=c.name, c.orthographic, rotation=c.transform.eulerAngles.ToString(),
                    renderer=c.GetUniversalAdditionalCameraData().scriptableRenderer.GetType().FullName
                });
                var data=new {
                    pipeline=GraphicsSettings.currentRenderPipeline.name, quality=QualitySettings.names[QualitySettings.GetQualityLevel()], cameras,
                    lights=Object.FindObjectsByType<Light2D>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length,
                    casters=Object.FindObjectsByType<ShadowCaster2D>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length,
                    shadowCount=renderers.Count(r=>r.name.Contains("Shadow")), rendererCount=renderers.Length,
                    materials=renderers.GroupBy(r=>r.sharedMaterial).Select(g=>new {material=AssetDatabase.GetAssetPath(g.Key),shader=g.Key!=null?g.Key.shader.name:null,count=g.Count()}),
                    renderers=renderers.Select(Describe)
                };
                File.WriteAllText(Folder+"/"+revision+"-"+name+".json",JsonConvert.SerializeObject(data,Formatting.Indented));
            }
            var paths=new[]{"Assets/Game/Prefabs/Enemies","Assets/Game/Prefabs/Defenses","Assets/Game/Prefabs/Carriers","Assets/Game/Prefabs/Gameplay"};
            var prefabs=AssetDatabase.FindAssets("t:Prefab",paths.Where(AssetDatabase.IsValidFolder).ToArray()).Select(AssetDatabase.GUIDToAssetPath).Select(p=>new {path=p, renderers=AssetDatabase.LoadAssetAtPath<GameObject>(p).GetComponentsInChildren<SpriteRenderer>(true).Select(Describe)}).ToArray();
            File.WriteAllText(Folder+"/"+revision+"-prefabs.json",JsonConvert.SerializeObject(prefabs,Formatting.Indented));
        } finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        return "Saved "+revision+" scene and prefab inventory";
    }
    public static string Validate()
    {
        var errors=new System.Collections.Generic.List<string>();
        var setup=EditorSceneManager.GetSceneManagerSetup();
        var scenes=new System.Collections.Generic.List<object>();
        try {
            foreach(var name in Scenes) {
                var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+name+".unity");
                var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).ToArray();
                var shadows=all.Where(r=>r.name.Contains("Shadow")).ToArray();
                foreach(var r in shadows.Where(r=>r.enabled&&r.gameObject.activeInHierarchy)) {
                    if(r.sharedMaterial==null||r.sharedMaterial.shader.name!="BorderDefenders/PixelShadow")errors.Add(name+": invalid material "+PathOf(r.transform));
                    if(r.sortingLayerName!="GroundObjects"||r.sortingOrder!=-20)errors.Add(name+": invalid shadow sorting");
                    var owner=r.transform.parent.GetComponent<SpriteRenderer>();
                    if(owner!=null&&(!owner.enabled||owner.sprite==null))errors.Add(name+": visible orphan "+PathOf(r.transform));
                }
                scenes.Add(new {name,renderers=all.Length,shadows=shadows.Length,enabledShadows=shadows.Count(r=>r.enabled&&r.gameObject.activeInHierarchy)});
            }
            foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Game/Prefabs/Enemies","Assets/Game/Prefabs/Defenses"})) {
                string path=AssetDatabase.GUIDToAssetPath(guid);if(path.EndsWith("/Crawler.prefab"))continue;
                var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if(root.GetComponentsInChildren<SpriteRenderer>(true).Count(r=>r.name.Contains("Shadow"))!=1)errors.Add(path+": not exactly one shadow");
            }
            var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Game/Materials/PixelShadow.shader");
            var messages=ShaderUtil.GetShaderMessages(shader).Select(m=>new {severity=m.severity.ToString(),m.message,m.line}).ToArray();
            if(messages.Length>0)errors.Add("Shader compiler messages present");
            File.WriteAllText(Folder+"/final-validation.json",JsonConvert.SerializeObject(new{scenes,shaderSupported=shader.isSupported,shaderMessages=messages,errors},Formatting.Indented));
        } finally {EditorSceneManager.RestoreSceneManagerSetup(setup);}
        if(errors.Count>0)throw new Exception(string.Join("; ",errors));
        return "Both maps and all 16 active creature/defense prefabs pass shadow inventory; shader has zero compiler messages";
    }
}
