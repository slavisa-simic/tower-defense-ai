using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Persistence;
using ShorelineDefense.UI;

public static class Phase5CAuthor
{
    public static string Run()
    {
        if (Application.isPlaying) throw new Exception("Author in Edit Mode only.");
        var gameplay = EditorSceneManager.OpenScene("Assets/Game/Scenes/CoastalDefense_TopDown.unity");
        var hud = UnityEngine.Object.FindFirstObjectByType<BuildHud>();
        var document = hud.GetComponent<UIDocument>();
        var settings = document.panelSettings;
        var art = new SerializedObject(hud.GetComponent<HudArtPresentation>());
        var font = (Font)art.FindProperty("pixelFont").objectReferenceValue;
        var style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Game/UI/ProductMenus.uss");
        var catalog = hud.Defenses.ToArray(); var map = UnityEngine.Object.FindFirstObjectByType<ShorelineDefense.Grid.GridManager>().Definition;
        var events = UnityEngine.Object.FindFirstObjectByType<ShorelineDefense.Waves.WaveManager>().EndlessSettings.eventSettings;
        var session = UnityEngine.Object.FindFirstObjectByType<RunSession>();
        if (session == null) new GameObject("Run Persistence").AddComponent<RunSession>();
        var controller = hud.GetComponent<ProductMenuController>() ?? hud.gameObject.AddComponent<ProductMenuController>();
        controller.Configure(false, style, font); EditorUtility.SetDirty(controller);
        controller.ConfigureCatalog(catalog, map, events);
        EditorSceneManager.MarkSceneDirty(gameplay); EditorSceneManager.SaveScene(gameplay);

        var renderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
            .Where(r => r.enabled && r.gameObject.activeInHierarchy && r.sprite != null && r.gameObject.name != "Placement Preview").ToArray();
        var sourceCamera = UnityEngine.Object.FindFirstObjectByType<Camera>();
        var menu = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(menu);
        var environment = new GameObject("Danube Menu Environment");
        foreach (var source in renderers)
        {
            var obj = new GameObject(source.name); obj.transform.SetParent(environment.transform);
            obj.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation); obj.transform.localScale = source.transform.lossyScale;
            EditorUtility.CopySerialized(source, obj.AddComponent<SpriteRenderer>());
        }
        var camera = new GameObject("Menu Camera").AddComponent<Camera>(); EditorUtility.CopySerialized(sourceCamera, camera);
        camera.transform.SetPositionAndRotation(sourceCamera.transform.position, sourceCamera.transform.rotation);
        camera.rect = new Rect(0, 0, 1, 1); camera.orthographic = true;
        camera.tag = "MainCamera";
        var ui = new GameObject("Main Menu UI"); var uiDoc = ui.AddComponent<UIDocument>(); uiDoc.panelSettings = settings;
        var menuController = ui.AddComponent<ProductMenuController>(); menuController.Configure(true, style, font); menuController.ConfigureCatalog(catalog, map, events);
        EditorSceneManager.SaveScene(menu, "Assets/Game/Scenes/MainMenu.unity");
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene("Assets/Game/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Game/Scenes/CoastalDefense_TopDown.unity", true),
            new EditorBuildSettingsScene("Assets/Game/Scenes/CoastalDefense_Prototype.unity", true) };
        EditorSceneManager.CloseScene(gameplay, true); AssetDatabase.SaveAssets();
        return "Main Menu saved, build index zero; static reused renderers=" + renderers.Length;
    }
}
