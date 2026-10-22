using System.IO;
using ShorelineDefense.Base;
using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace ShorelineDefense.Editor
{
    /// <summary>Persists normal Unity assets. Never runs when the game starts.</summary>
    public static class PrototypeSceneBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/CoastalDefense_Prototype.unity";
        public const string BattlefieldPath = "Assets/Game/Data/Game/Battlefield.asset";
        public const string CrawlerPath = "Assets/Game/Data/Enemies/Crawler.asset";

        [MenuItem("Tools/Shoreline Defense/Build Phase 1 Prototype")]
        public static void BuildPrototype()
        {
            if (File.Exists(ScenePath))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
                return;
            }
            BuildScene();
        }

        [MenuItem("Tools/Shoreline Defense/Rebuild Prototype Scene From Definition")]
        public static void RebuildPrototype()
        {
            if (Application.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildScene();
        }

        private static void BuildScene()
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Stop Play Mode before building the scene.");
            EnsureFolder("Assets/Game/Scenes");
            EnsureFolder("Assets/Game/Materials");
            EnsureFolder("Assets/Game/Data/Game");
            EnsureFolder("Assets/Game/Data/Enemies");
            EnsureFolder("Assets/Game/Prefabs/Enemies");
            EnsureFolder("Assets/Game/Prefabs/Gameplay");

            bool createLegacyDefinition = !File.Exists(BattlefieldPath);
            var definition = LoadOrCreate<BattlefieldDefinition>(BattlefieldPath);
            if (createLegacyDefinition)
            {
                // New gameplay definitions default to XY. Only this historical builder authors XZ.
                var legacy = new SerializedObject(definition);
                legacy.FindProperty("plane").enumValueIndex = (int)GameplayPlane.LegacyXZ;
                legacy.FindProperty("width").intValue = 14;
                legacy.FindProperty("height").intValue = 20;
                legacy.FindProperty("cellSize").floatValue = 1.5f;
                legacy.FindProperty("waterBandCells").intValue = 4;
                legacy.FindProperty("beachBandCells").intValue = 4;
                legacy.FindProperty("baseCell").vector2IntValue = new Vector2Int(7, 2);
                legacy.ApplyModifiedPropertiesWithoutUndo();
            }
            var crawlerData = LoadOrCreate<EnemyDefinition>(CrawlerPath);
            var water = Material("Water", new Color(0.12f, 0.4f, 0.57f));
            var beach = Material("Beach", new Color(0.72f, 0.62f, 0.4f));
            var ground = Material("Ground", new Color(0.31f, 0.4f, 0.28f));
            var baseMaterial = Material("PlayerBase", new Color(0.74f, 0.79f, 0.83f));
            var enemyMaterial = Material("Crawler", new Color(0.73f, 0.16f, 0.16f));
            var crawler = CreateCrawler(crawlerData, enemyMaterial);
            var basePrefab = CreateBase(baseMaterial);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            float width = definition.Width * definition.CellSize;
            float depth = definition.Height * definition.CellSize;
            var origin = new Vector3(-width / 2, 0, -depth / 2);
            var gridObject = new GameObject("Battlefield Grid");
            gridObject.transform.position = origin;
            var grid = gridObject.AddComponent<GridManager>();
            grid.Configure(definition);
            var model = definition.CreateGrid(origin);

            var environment = new GameObject("Coastal Graybox").transform;
            Zone("Defensive Land", 0, definition.GroundBandCells, definition, origin, ground, environment);
            Zone("Beach", definition.GroundBandCells, definition.BeachBandCells, definition, origin, beach, environment);
            Zone("Sea", definition.Height - definition.WaterBandCells, definition.WaterBandCells, definition, origin, water, environment);

            var playerBase = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            playerBase.name = "Player Base";
            playerBase.transform.position = model.CellToWorld(definition.BaseCell);
            playerBase.transform.GetChild(0).localScale = new Vector3(definition.CellSize * 0.85f, 1.8f, definition.CellSize * 0.85f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(playerBase.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(playerBase.transform.GetChild(0));
            var baseHealth = playerBase.GetComponent<BaseHealth>();
            var state = new GameObject("Game State").AddComponent<GameStateController>();
            state.Configure(baseHealth);

            var spawnRoot = new GameObject("Sea Spawn Points").transform;
            var points = new Transform[3];
            int[] spawnColumns = { definition.Width / 5, definition.Width / 2, definition.Width - 1 - definition.Width / 5 };
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new GameObject($"Sea Spawn {i + 1}").transform;
                points[i].SetParent(spawnRoot);
                points[i].position = model.CellToWorld(new Vector2Int(spawnColumns[i], definition.Height - 2));
            }
            var enemies = new GameObject("Enemies").transform;
            var spawner = new GameObject("Prototype Enemy Spawner").AddComponent<EnemySpawner>();
            spawner.Configure(crawler.GetComponent<EnemyController>(), grid, baseHealth, state, points, enemies);
            spawner.gameObject.AddComponent<PrototypeSpawnLoop>();

            var camera = new GameObject("Isometric Camera", typeof(Camera)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(12f, (width + depth) * 0.29f);
            camera.transform.rotation = Quaternion.Euler(35.264f, 45f, 0);
            camera.transform.position = -camera.transform.forward * 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 120f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.14f, 0.18f);
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>();

            var light = new GameObject("Sun").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.58f, 0.62f, 0.68f);
            RenderSettings.skybox = null;
            RenderSettings.sun = light;

            CreateHud(baseHealth, state);
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");
            if (pipeline == null) throw new System.InvalidOperationException("The URP template PC pipeline asset is missing.");
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            PlayerSettings.productName = "Shoreline Defense";
            PlayerSettings.companyName = "Border Defenders";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            // UI Toolkit uses the built-in input backend for this passive HUD.
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            settings.FindProperty("activeInputHandler").intValue = 0;
            settings.ApplyModifiedPropertiesWithoutUndo();
            EditorSettings.serializationMode = SerializationMode.ForceText;

            EditorSceneManager.SaveScene(scene, ScenePath);
            // Rebuilding the reference must not replace the primary top-down build scene.
            if (File.Exists(TopDownSceneBuilder.ScenePath)) TopDownSceneBuilder.RegisterScenes();
            else EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Selection.activeObject = gridObject;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.LookAt(Vector3.zero, camera.transform.rotation, camera.orthographicSize * 1.4f, true);
            Debug.Log($"Phase 1 prototype saved: {ScenePath}");
        }

        private static GameObject CreateCrawler(EnemyDefinition data, Material material)
        {
            const string path = "Assets/Game/Prefabs/Enemies/Crawler.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            var root = new GameObject("Crawler");
            root.AddComponent<EnemyController>().Configure(data);
            var visual = Primitive("Capsule Placeholder", PrimitiveType.Capsule, material, root.transform);
            visual.transform.localPosition = new Vector3(0, 0.7f, 0);
            visual.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateBase(Material material)
        {
            const string path = "Assets/Game/Prefabs/Gameplay/PlayerBase.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            var root = new GameObject("PlayerBase");
            root.AddComponent<BaseHealth>();
            var visual = Primitive("Cube Placeholder", PrimitiveType.Cube, material, root.transform);
            visual.transform.localPosition = new Vector3(0, 0.9f, 0);
            visual.transform.localScale = new Vector3(1.275f, 1.8f, 1.275f);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreateHud(BaseHealth playerBase, GameStateController state)
        {
            var panel = LoadOrCreate<PanelSettings>("Assets/Game/UI/PrototypePanelSettings.asset");
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1280, 720);
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/Game/UI/PrototypeTheme.tss");
            if (theme == null)
            {
                File.WriteAllText("Assets/Game/UI/PrototypeTheme.tss", "@import url(\"unity-theme://default\");\n");
                AssetDatabase.ImportAsset("Assets/Game/UI/PrototypeTheme.tss");
                theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/Game/UI/PrototypeTheme.tss");
            }
            panel.themeStyleSheet = theme;
            EditorUtility.SetDirty(panel);
            var root = new GameObject("Prototype HUD");
            var document = root.AddComponent<UIDocument>();
            document.panelSettings = panel;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/PrototypeHud.uxml");
            root.AddComponent<PrototypeHud>().Configure(playerBase, state);
        }

        private static void Zone(string name, int firstRow, int rowCount, BattlefieldDefinition data,
            Vector3 origin, Material material, Transform parent)
        {
            var area = Primitive(name, PrimitiveType.Cube, material, parent);
            area.transform.position = origin + new Vector3(data.Width * data.CellSize / 2, -0.2f,
                (firstRow + rowCount / 2f) * data.CellSize);
            area.transform.localScale = new Vector3(data.Width * data.CellSize, 0.4f, rowCount * data.CellSize);
        }

        private static GameObject Primitive(string name, PrimitiveType type, Material material, Transform parent)
        {
            var result = GameObject.CreatePrimitive(type);
            result.name = name;
            result.transform.SetParent(parent, false);
            result.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(result.GetComponent<Collider>());
            return result;
        }

        private static Material Material(string name, Color color)
        {
            string path = $"Assets/Game/Materials/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.05f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
