using System;
using System.IO;
using System.Linq;
using ShorelineDefense.Base;
using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ShorelineDefense.Editor
{
    /// <summary>Editor authoring only. Saves ordinary assets; runtime never builds the map.</summary>
    public static class TopDownSceneBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/CoastalDefense_TopDown.unity";
        public const string BattlefieldPath = "Assets/Game/Data/Game/Battlefield_TopDown.asset";
        private const string ArtPath = "Assets/Game/Art/Placeholders";
        private const string CrawlerPrefabPath = "Assets/Game/Prefabs/Enemies/Crawler_TopDown.prefab";
        private const string BasePrefabPath = "Assets/Game/Prefabs/Gameplay/PlayerBase_TopDown.prefab";

        [MenuItem("Tools/Shoreline Defense/Open or Create Top-Down Scene")]
        public static void OpenOrCreate()
        {
            if (Application.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath);
            else BuildScene();
        }

        [MenuItem("Tools/Shoreline Defense/Rebuild Top-Down Scene From Definition")]
        public static void Rebuild()
        {
            if (Application.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildScene();
        }

        public static void RegisterScenes()
        {
            // Separate scenes, never loaded together. Keep the reference available to regression tests.
            var rest = EditorBuildSettings.scenes.Where(s => s.path != ScenePath && s.path != PrototypeSceneBuilder.ScenePath);
            EditorBuildSettings.scenes = new[] {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(PrototypeSceneBuilder.ScenePath, true)
            }.Concat(rest).ToArray();
        }

        private static void BuildScene()
        {
            EnsureFolder(ArtPath);
            var data = GetDefinition();
            if (data.Plane != GameplayPlane.XY) throw new InvalidOperationException("Top-down maps must use the XY plane.");
            EnsureSortingLayers();
            Sprite square = GetShape("Square", false);
            Sprite triangle = GetShape("Triangle", true);
            const string materialPath = "Assets/Game/Materials/TopDownPlaceholder.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
                AssetDatabase.CreateAsset(material, materialPath);
            }
            var crawler = GetCrawler(square, triangle, material);
            var basePrefab = GetBase(square, material);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            float width = data.Width * data.CellSize;
            float height = data.Height * data.CellSize;
            var origin = new Vector3(-width * 0.5f, -height * 0.5f, 0);
            var grid = new GameObject("Battlefield Grid").AddComponent<GridManager>();
            grid.transform.position = origin;
            grid.Configure(data);
            var model = data.CreateGrid(origin);
            var environment = new GameObject("Top-Down Coastal Graybox").transform;
            float waterWidth = data.WaterBandCells * data.CellSize;
            float beachWidth = data.BeachBandCells * data.CellSize;
            float coast = origin.x + waterWidth;
            float inland = coast + beachWidth;
            Shape("Ocean", environment, square, material, new Color(0.13f, 0.39f, 0.55f),
                new Vector2(origin.x + waterWidth * 0.5f, 0), new Vector2(waterWidth, height), "Environment", 0);
            Shape("Beach", environment, square, material, new Color(0.78f, 0.69f, 0.45f),
                new Vector2(coast + beachWidth * 0.5f, 0), new Vector2(beachWidth, height), "Environment", 0);
            Shape("Grass Combat Field", environment, square, material, new Color(0.31f, 0.43f, 0.27f),
                new Vector2(inland + (width - waterWidth - beachWidth) * 0.5f, 0),
                new Vector2(width - waterWidth - beachWidth, height), "Environment", 0);
            Shape("Shallow Water", environment, square, material, new Color(0.24f, 0.53f, 0.64f),
                new Vector2(coast - 0.3f, 0), new Vector2(0.6f, height), "Environment", 1);
            Shape("Shoreline", environment, square, material, new Color(0.72f, 0.84f, 0.76f),
                new Vector2(coast - 0.055f, 0), new Vector2(0.11f, height), "Environment", 2);
            // Faint graybox cell guides reveal the large future build area without adding placement.
            var guides = new GameObject("Ground Cell Guides").transform;
            guides.SetParent(environment, false);
            var lineColor = new Color(0.74f, 0.82f, 0.57f, 0.09f);
            for (int x = data.WaterBandCells + data.BeachBandCells; x <= data.Width; x++)
                Shape("Column " + x, guides, square, material, lineColor,
                    new Vector2(origin.x + x * data.CellSize, 0), new Vector2(0.018f, height), "Environment", 3);
            for (int y = 0; y <= data.Height; y++)
                Shape("Row " + y, guides, square, material, lineColor,
                    new Vector2((inland + width * 0.5f) * 0.5f, origin.y + y * data.CellSize),
                    new Vector2(width - waterWidth - beachWidth, 0.018f), "Environment", 3);

            var playerBase = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            playerBase.name = "Player Base";
            playerBase.transform.position = model.CellToWorld(data.BaseCell);
            PrefabUtility.RecordPrefabInstancePropertyModifications(playerBase.transform);
            var health = playerBase.GetComponent<BaseHealth>();
            var state = new GameObject("Game State").AddComponent<GameStateController>();
            state.Configure(health);
            var spawnRoot = new GameObject("Coastline Spawn Points").transform;
            var points = new Transform[5];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new GameObject("Sea Spawn " + (i + 1)).transform;
                points[i].SetParent(spawnRoot, false);
                int row = Mathf.Clamp(Mathf.RoundToInt((i + 1) * (data.Height - 1) / 6f), 0, data.Height - 1);
                points[i].position = model.CellToWorld(new Vector2Int(Mathf.Min(1, data.WaterBandCells - 1), row));
            }
            var enemies = new GameObject("Enemies").transform;
            var spawner = new GameObject("Prototype Enemy Spawner").AddComponent<EnemySpawner>();
            spawner.Configure(crawler.GetComponent<EnemyController>(), grid, health, state, points, enemies);
            var camera = new GameObject("Top-Down Camera", typeof(Camera)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.075f, 0.11f, 0.13f);
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>().SetRenderer(Ensure2DRenderer());
            camera.gameObject.AddComponent<TopDownCamera>().Configure(grid);

            var hud = new GameObject("Prototype HUD");
            var document = hud.AddComponent<UIDocument>();
            document.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/Game/UI/PrototypePanelSettings.asset");
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/TopDownHud.uxml");
            hud.AddComponent<PrototypeHud>().Configure(health, state);
            Phase2BSceneSetup.ConfigureScene();
            Phase2CSceneSetup.ConfigureScene();
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterScenes();
            AssetDatabase.SaveAssets();
            Selection.activeObject = grid.gameObject;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.in2DMode = true;
                SceneView.lastActiveSceneView.LookAt(Vector3.zero, Quaternion.identity, height * 0.6f, true);
            }
            Debug.Log("Top-down scene saved: " + ScenePath);
        }

        private static BattlefieldDefinition GetDefinition()
        {
            var data = AssetDatabase.LoadAssetAtPath<BattlefieldDefinition>(BattlefieldPath);
            if (data != null) return data;
            data = ScriptableObject.CreateInstance<BattlefieldDefinition>();
            var fields = new SerializedObject(data);
            fields.FindProperty("plane").enumValueIndex = (int)GameplayPlane.XY;
            fields.FindProperty("width").intValue = 32;
            fields.FindProperty("height").intValue = 18;
            fields.FindProperty("cellSize").floatValue = 1.25f;
            fields.FindProperty("waterBandCells").intValue = 4;
            fields.FindProperty("beachBandCells").intValue = 2;
            fields.FindProperty("baseCell").vector2IntValue = new Vector2Int(29, 9);
            fields.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(data, BattlefieldPath);
            return data;
        }

        private static GameObject GetCrawler(Sprite square, Sprite triangle, Material material)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CrawlerPrefabPath);
            if (prefab != null) return prefab;
            var root = new GameObject("Crawler TopDown");
            root.AddComponent<EnemyController>().Configure(AssetDatabase.LoadAssetAtPath<EnemyDefinition>(PrototypeSceneBuilder.CrawlerPath));
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            root.AddComponent<EnemyFacing>().Configure(visual);
            Shape("Body Outline", visual, square, material, new Color(0.24f, 0.09f, 0.09f),
                Vector2.zero, new Vector2(0.88f, 0.64f), "Enemies", 0);
            Shape("Body", visual, square, material, new Color(0.9f, 0.25f, 0.18f),
                new Vector2(-0.05f, 0), new Vector2(0.65f, 0.46f), "Enemies", 1);
            Shape("Facing Tip", visual, triangle, material, new Color(1f, 0.8f, 0.47f),
                new Vector2(0.43f, 0), new Vector2(0.4f, 0.6f), "Enemies", 2);
            prefab = PrefabUtility.SaveAsPrefabAsset(root, CrawlerPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject GetBase(Sprite square, Material material)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefabPath);
            if (prefab != null) return prefab;
            var root = new GameObject("PlayerBase TopDown");
            root.AddComponent<BaseHealth>();
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            Shape("Footprint", visual, square, material, new Color(0.1f, 0.2f, 0.22f),
                Vector2.zero, Vector2.one * 2.5f, "GroundObjects", 0);
            Shape("Base", visual, square, material, new Color(0.8f, 0.85f, 0.77f),
                Vector2.zero, Vector2.one * 2.16f, "GroundObjects", 1);
            Shape("Roof", visual, square, material, new Color(0.3f, 0.45f, 0.49f),
                Vector2.zero, Vector2.one * 1.5f, "GroundObjects", 2);
            Shape("Roof Stripe", visual, square, material, new Color(0.9f, 0.9f, 0.7f),
                Vector2.zero, new Vector2(0.22f, 1.5f), "GroundObjects", 3);
            prefab = PrefabUtility.SaveAsPrefabAsset(root, BasePrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void Shape(string name, Transform parent, Sprite sprite, Material material, Color color,
            Vector2 position, Vector2 size, string layer, int order)
        {
            var visual = new GameObject(name).AddComponent<SpriteRenderer>();
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = position;
            visual.transform.localScale = new Vector3(size.x, size.y, 1);
            visual.sprite = sprite;
            visual.sharedMaterial = material;
            visual.color = color;
            visual.sortingLayerName = layer;
            visual.sortingOrder = order;
        }

        private static Sprite GetShape(string name, bool triangle)
        {
            string path = ArtPath + "/" + name + ".png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    texture.SetPixel(x, y, !triangle || x <= 15 - Mathf.Abs(2 * y - 15) ? Color.white : Color.clear);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureSortingLayers()
        {
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tags.FindProperty("m_SortingLayers");
            string[] names = { "Environment", "GroundObjects", "Enemies", "Defenses", "Effects" };
            for (int i = 0; i < names.Length; i++)
            {
                if (SortingLayer.layers.Any(layer => layer.name == names[i])) continue;
                int index = layers.arraySize;
                layers.InsertArrayElementAtIndex(index);
                var layer = layers.GetArrayElementAtIndex(index);
                layer.FindPropertyRelative("name").stringValue = names[i];
                layer.FindPropertyRelative("uniqueID").intValue = 2001 + i;
                layer.FindPropertyRelative("locked").boolValue = false;
            }
            tags.ApplyModifiedPropertiesWithoutUndo();
        }

        private static int Ensure2DRenderer()
        {
            const string path = "Assets/Settings/TopDown_Renderer.asset";
            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(path);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(renderer, path);
            }
            // Both existing quality pipelines retain the 3D renderer at index zero for Phase 1.
            // The top-down camera explicitly selects the 2D renderer at the same index in each.
            int rendererIndex = -1;
            foreach (string pipelinePath in new[] { "Assets/Settings/PC_RPAsset.asset", "Assets/Settings/Mobile_RPAsset.asset" })
            {
                var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
                var fields = new SerializedObject(pipeline);
                var renderers = fields.FindProperty("m_RendererDataList");
                int index = 0;
                while (index < renderers.arraySize && renderers.GetArrayElementAtIndex(index).objectReferenceValue != renderer) index++;
                if (index == renderers.arraySize)
                {
                    renderers.InsertArrayElementAtIndex(index);
                    renderers.GetArrayElementAtIndex(index).objectReferenceValue = renderer;
                    fields.ApplyModifiedPropertiesWithoutUndo();
                }
                if (rendererIndex >= 0 && rendererIndex != index)
                    throw new InvalidOperationException("Quality pipelines must share the top-down renderer index.");
                rendererIndex = index;
            }
            return rendererIndex;
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
