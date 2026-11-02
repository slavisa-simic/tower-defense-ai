using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Grid;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;

public static class NavigationAuthor
{
    private static void Set(UnityEngine.Object target, string field, int value)
    { var so = new SerializedObject(target); so.FindProperty(field).intValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    private static void SetFloat(UnityEngine.Object target, string field, float value)
    { var so = new SerializedObject(target); so.FindProperty(field).floatValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    public static string Run()
    {
        if (Application.isPlaying) throw new Exception("Edit mode only");
        var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tags.FindProperty("layers");
        foreach (var pair in new[] { (8,"EnvironmentObstacle"), (9,"PlayerDefense"), (10,"Enemy") })
        {
            var field = layers.GetArrayElementAtIndex(pair.Item1);
            if (!string.IsNullOrEmpty(field.stringValue) && field.stringValue != pair.Item2) throw new Exception("Layer already in use");
            field.stringValue = pair.Item2;
        }
        tags.ApplyModifiedPropertiesWithoutUndo();
        // These are query/interaction colliders. Grid Transform movement has no dynamic Rigidbody2D pushing.
        Physics2D.IgnoreLayerCollision(10,10,true);
        Physics2D.IgnoreLayerCollision(10,8,false); Physics2D.IgnoreLayerCollision(10,9,false);
        foreach (string guid in AssetDatabase.FindAssets("t:DefenseDefinition", new[] {"Assets/Game/Data/Defenses"}))
        {
            var data = AssetDatabase.LoadAssetAtPath<DefenseDefinition>(AssetDatabase.GUIDToAssetPath(guid));
            var so = new SerializedObject(data); so.FindProperty("blocksMovement").boolValue = data.Id != "barbed-wire" && data.Id != "land-mine"; so.ApplyModifiedPropertiesWithoutUndo();
            string path = AssetDatabase.GetAssetPath(data.Prefab);
            var prefab = PrefabUtility.LoadPrefabContents(path);
            prefab.layer = 9;
            if (data.BlocksMovement)
            {
                var c = prefab.GetComponent<BoxCollider2D>();
                if (c == null) c = prefab.AddComponent<BoxCollider2D>();
                c.size = new Vector2(data.Footprint.x,data.Footprint.y)*1.25f; c.offset = Vector2.zero; c.isTrigger = false;
                if (prefab.GetComponent<DefenseDamageSmoke>() == null) prefab.AddComponent<DefenseDamageSmoke>();
            }
            PrefabUtility.SaveAsPrefabAsset(prefab,path); PrefabUtility.UnloadPrefabContents(prefab);
        }
        foreach (string guid in AssetDatabase.FindAssets("t:EnemyDefinition", new[] {"Assets/Game/Data/Enemies"}))
        {
            var data = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(guid));
            int damage = data.Kind == EnemyKind.Skitter ? 6 : data.Kind == EnemyKind.Bulwark ? 45 : data.Kind == EnemyKind.Broodling ? 3 : data.Kind == EnemyKind.Spitter ? 24 : data.Kind == EnemyKind.Burrower ? 18 : data.Kind == EnemyKind.Dredger ? 120 : data.Kind == EnemyKind.Broodmother ? 45 : 12;
            Set(data,"structureDamage",damage); SetFloat(data,"structureAttackInterval",data.Kind == EnemyKind.Spitter ? 5 : data.Kind == EnemyKind.Dredger ? 1.2f : 1);
            SetFloat(data,"structureRangeCells",data.Kind == EnemyKind.Spitter ? 5 : .6f);
        }
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Game/Prefabs/Enemies"}))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid); var prefab = PrefabUtility.LoadPrefabContents(path);
            prefab.layer = 10;
            if (prefab.GetComponent<EnemyStructureAttack>() == null) prefab.AddComponent<EnemyStructureAttack>();
            PrefabUtility.SaveAsPrefabAsset(prefab,path); PrefabUtility.UnloadPrefabContents(prefab);
        }
        var result = new System.Text.StringBuilder();
        foreach (string map in new[] {"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            var scene = EditorSceneManager.OpenScene("Assets/Game/Scenes/"+map+".unity");
            var manager = UnityEngine.Object.FindFirstObjectByType<GridManager>(); var data = manager.Definition;
            var footprints = new List<StaticObstacleFootprint>();
            var old = GameObject.Find("Navigation Obstacles"); if (old != null) UnityEngine.Object.DestroyImmediate(old);
            var root = new GameObject("Navigation Obstacles");
            void Add(string id, StaticObstacleCategory category, int x,int y,int w,int h)
            { footprints.Add(new StaticObstacleFootprint {id=id,category=category,anchor=new Vector2Int(x,y),size=new Vector2Int(w,h)}); }
            if (map == "DanubeIndustrialPort")
            {
                int i=0;
                foreach(var r in new[]{new RectInt(10,2,5,3),new RectInt(18,2,5,3),new RectInt(9,7,4,4),new RectInt(16,7,5,3),new RectInt(11,13,5,3),new RectInt(19,13,4,3)})
                    Add("container-group-"+(++i),StaticObstacleCategory.Container,r.x,r.y,r.width,r.height);
                Add("warehouse",StaticObstacleCategory.Building,24,1,3,4); Add("maintenance",StaticObstacleCategory.Building,24,7,3,4); Add("loading-shed",StaticObstacleCategory.Building,24,14,3,3);
                Add("crane-landmark",StaticObstacleCategory.Crane,7,16,3,2);
                // The right-hand chassis/supports are solid. The projecting boom is overhead;
                // retain the authored quay access column at x=6 for northern emergence points.
                Add("dock-crane-south",StaticObstacleCategory.Crane,7,3,2,2);
                Add("dock-crane-center",StaticObstacleCategory.Crane,7,8,2,2);
                Add("dock-crane-north",StaticObstacleCategory.Crane,7,14,2,2);
                foreach (int y in new[]{0,7,15}) Add("checkpoint-compound-"+y,StaticObstacleCategory.Building,28,y,4,3);
                Add("forklift",StaticObstacleCategory.Equipment,26,14,2,2);
                Add("logistics-truck",StaticObstacleCategory.Vehicle,29,8,3,2);
            }
            else
            {
                Add("command-post",StaticObstacleCategory.Building,27,6,3,3);
                Add("bunker-wall-south",StaticObstacleCategory.Fortification,26,1,2,5);
                Add("bunker-wall-north",StaticObstacleCategory.Fortification,26,13,2,4);
                Add("bunker-gate-south-support",StaticObstacleCategory.Fortification,26,6,1,1);
                Add("bunker-gate-north-support",StaticObstacleCategory.Fortification,26,8,1,1);
                Add("fence-south",StaticObstacleCategory.Fortification,27,1,5,1);
                Add("fence-north",StaticObstacleCategory.Fortification,27,17,5,1);
                foreach (int y in new[]{4,9,14}) Add("checkpoint-gate-support-"+y,StaticObstacleCategory.Fortification,30,y+1,2,1);
                Add("sandbags-south",StaticObstacleCategory.Fortification,24,3,2,1);
                Add("sandbags-north",StaticObstacleCategory.Fortification,24,14,2,2);
            }
            data.navigationObstacles = footprints.ToArray(); EditorUtility.SetDirty(data);
            var grid = data.CreateGrid(manager.transform.position);
            foreach (var f in footprints)
            {
                var go = new GameObject(f.id); go.transform.SetParent(root.transform); go.layer=8; go.isStatic=true;
                go.transform.position=grid.FootprintCenter(f.anchor,f.size);
                go.AddComponent<StaticNavigationObstacle>().obstacleId=f.id;
                var collider=go.AddComponent<BoxCollider2D>(); collider.size=new Vector2(f.size.x,f.size.y)*grid.CellSize;
            }
            // Explicit bindings allow development validation to catch missing known geometry.
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            result.AppendLine(map+": "+footprints.Count+" authored footprints / BoxCollider2Ds.");
        }
        AssetDatabase.SaveAssets();
        File.WriteAllText("Docs/Validation/NavigationCollision/authoring.txt",result.ToString());
        return result.ToString();
    }
}
