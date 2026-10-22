using System.IO;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Economy;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ShorelineDefense.Editor
{
    /// <summary>Authors persisted Phase 2B assets/references. Not required during normal play.</summary>
    public static class Phase2BSceneSetup
    {
        public const string DefinitionPath = "Assets/Game/Data/Defenses/MachineGunNest.asset";
        public const string PrefabPath = "Assets/Game/Prefabs/Defenses/MachineGunNest.prefab";

        [MenuItem("Tools/Shoreline Defense/Install Phase 2B In Top-Down Scene")]
        public static void Install()
        {
            if (Application.isPlaying) return;
            EditorSceneManager.OpenScene(TopDownSceneBuilder.ScenePath);
            ConfigureScene();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }

        public static void ConfigureScene()
        {
            var grid = Object.FindFirstObjectByType<GridManager>();
            var game = Object.FindFirstObjectByType<GameStateController>();
            var spawner = Object.FindFirstObjectByType<EnemySpawner>();
            var hud = Object.FindFirstObjectByType<PrototypeHud>().gameObject;
            var square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Placeholders/Square.png");
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/TopDownPlaceholder.mat");
            var definition = GetDefense(square, material);
            var economy = GetOrCreate<EconomyManager>("Economy");
            economy.Configure(game);
            var registry = GetOrCreate<EnemyRegistry>("Enemy Registry");
            var rewards = GetOrCreate<EnemyRewardService>("Enemy Rewards");
            rewards.Configure(registry, economy, game);
            spawner.ConfigureRegistry(registry);
            var parent = GameObject.Find("Defenses") ?? new GameObject("Defenses");
            var builder = GetOrCreate<BuildManager>("Defense Placement");
            builder.Configure(grid, economy, game, registry, parent.transform);
            var controller = builder.GetComponent<PlacementController>() ?? builder.gameObject.AddComponent<PlacementController>();
            var ghostObject = GameObject.Find("Placement Preview");
            // Find inactive authoring objects too; the preview is saved hidden.
            if (ghostObject == null)
                foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                    if (root.name == "Placement Preview") ghostObject = root;
            var ghost = ghostObject != null ? ghostObject.GetComponent<SpriteRenderer>() :
                Shape("Placement Preview", null, square, material, Color.green, Vector2.zero, Vector2.one, 20);
            ghost.sortingLayerName = "Effects";
            ghost.gameObject.SetActive(false);
            controller.Configure(builder, ghost);
            var input = builder.GetComponent<PlacementInput>() ?? builder.gameObject.AddComponent<PlacementInput>();
            input.Configure(Camera.main, grid, controller, hud.GetComponent<UIDocument>());
            var buildHud = hud.GetComponent<BuildHud>() ?? hud.AddComponent<BuildHud>();
            buildHud.Configure(economy, game, controller, definition);
            var mapFields = new SerializedObject(grid.Definition);
            mapFields.FindProperty("baseFootprint").vector2IntValue = new Vector2Int(3, 3);
            mapFields.ApplyModifiedPropertiesWithoutUndo();
            var crawler = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(PrototypeSceneBuilder.CrawlerPath);
            EditorUtility.SetDirty(crawler); // Persist the new default killReward field.
            foreach (var component in new Object[] { economy, registry, rewards, spawner, builder, controller, input, buildHud })
                EditorUtility.SetDirty(component);
        }

        private static T GetOrCreate<T>(string name) where T : Component =>
            Object.FindFirstObjectByType<T>() ?? new GameObject(name).AddComponent<T>();

        private static DefenseDefinition GetDefense(Sprite square, Material material)
        {
            EnsureFolder("Assets/Game/Data/Defenses");
            EnsureFolder("Assets/Game/Prefabs/Defenses");
            var prefab = AssetDatabase.LoadAssetAtPath<DefenseBase>(PrefabPath);
            if (prefab == null)
            {
                var root = new GameObject("Machine Gun Nest");
                root.AddComponent<DefenseBase>();
                root.AddComponent<MachineGunCombat>();
                Shape("Sandbag Base", root.transform, square, material, new Color(0.76f, 0.69f, 0.44f), Vector2.zero, new Vector2(1.08f, 0.95f), 0);
                Shape("Inset", root.transform, square, material, new Color(0.18f, 0.25f, 0.21f), Vector2.zero, new Vector2(0.82f, 0.68f), 1);
                var weapon = new GameObject("Rotating Weapon").transform;
                weapon.SetParent(root.transform, false);
                Shape("Receiver", weapon, square, material, new Color(0.55f, 0.64f, 0.58f), Vector2.zero, new Vector2(0.48f, 0.35f), 2);
                Shape("Barrel", weapon, square, material, new Color(0.12f, 0.16f, 0.17f), new Vector2(0.4f, 0), new Vector2(0.7f, 0.14f), 3);
                var muzzle = new GameObject("Muzzle").transform;
                muzzle.SetParent(weapon, false);
                muzzle.localPosition = new Vector3(0.78f, 0, 0);
                var flash = Shape("Muzzle Flash", muzzle, square, material, new Color(1, 0.89f, 0.35f), Vector2.zero, new Vector2(0.26f, 0.26f), 5);
                var tracer = Shape("Reusable Tracer", root.transform, square, material, new Color(1, 0.9f, 0.5f), Vector2.zero, Vector2.one, 4);
                flash.sortingLayerName = tracer.sortingLayerName = "Effects";
                flash.enabled = tracer.enabled = false;
                root.AddComponent<MachineGunPresentation>().Configure(weapon, muzzle, flash, tracer);
                prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath).GetComponent<DefenseBase>();
                Object.DestroyImmediate(root);
            }
            var data = AssetDatabase.LoadAssetAtPath<DefenseDefinition>(DefinitionPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<DefenseDefinition>();
                var fields = new SerializedObject(data);
                fields.FindProperty("prefab").objectReferenceValue = prefab;
                fields.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(data, DefinitionPath);
            }
            return data;
        }

        private static SpriteRenderer Shape(string name, Transform parent, Sprite sprite, Material material, Color color,
            Vector2 position, Vector2 size, int order)
        {
            var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(parent, false);
            renderer.transform.localPosition = position;
            renderer.transform.localScale = new Vector3(size.x, size.y, 1);
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.color = color;
            renderer.sortingLayerName = "Defenses";
            renderer.sortingOrder = order;
            return renderer;
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
