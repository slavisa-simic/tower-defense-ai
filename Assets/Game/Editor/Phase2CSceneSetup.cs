using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Economy;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ShorelineDefense.Editor
{
    /// <summary>Idempotent authoring of ordinary saved assets. No runtime setup dependency.</summary>
    public static class Phase2CSceneSetup
    {
        [MenuItem("Tools/Shoreline Defense/Install Phase 2C In Top-Down Scene")]
        public static void Install()
        {
            if (Application.isPlaying) return;
            // Preserve the historical scene's repeating spawn regression explicitly.
            EditorSceneManager.OpenScene("Assets/Game/Scenes/CoastalDefense_Prototype.unity");
            var oldSpawner = Object.FindFirstObjectByType<EnemySpawner>();
            if (oldSpawner.GetComponent<PrototypeSpawnLoop>() == null) oldSpawner.gameObject.AddComponent<PrototypeSpawnLoop>();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorSceneManager.OpenScene(TopDownSceneBuilder.ScenePath);
            Phase2BSceneSetup.ConfigureScene();
            ConfigureScene();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }

        public static void ConfigureScene()
        {
            var mg = AssetDatabase.LoadAssetAtPath<DefenseDefinition>(Phase2BSceneSetup.DefinitionPath);
            var wire = GetDefense("BarbedWire", "barbed-wire", "Barbed Wire", 50, 200, 1, 0, 1, TargetingMode.Closest);
            var mine = GetDefense("LandMine", "land-mine", "Land Mine", 75, 1, 1.75f, 150, 1, TargetingMode.Closest);
            var sniper = GetDefense("SniperTower", "sniper-tower", "Sniper Tower", 300, 250, 11, 100, 0.5f, TargetingMode.ClosestToBase);
            var hud = Object.FindFirstObjectByType<BuildHud>();
            hud.ConfigureCatalog(new[] { mg, wire, mine, sniper });
            var spawner = Object.FindFirstObjectByType<EnemySpawner>();
            spawner.gameObject.name = "Enemy Spawner";
            foreach (var oldLoop in Object.FindObjectsByType<PrototypeSpawnLoop>(FindObjectsSortMode.None)) Object.DestroyImmediate(oldLoop);
            var waveData = new WaveDefinition[5];
            if (!AssetDatabase.IsValidFolder("Assets/Game/Data/Waves")) AssetDatabase.CreateFolder("Assets/Game/Data", "Waves");
            for (int i = 0; i < waveData.Length; i++)
            {
                string path = $"Assets/Game/Data/Waves/Wave{i + 1}.asset";
                waveData[i] = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);
                if (waveData[i] != null) continue;
                var data = ScriptableObject.CreateInstance<WaveDefinition>();
                var fields = new SerializedObject(data);
                fields.FindProperty("waveNumber").intValue = i + 1;
                fields.FindProperty("completionReward").intValue = 100;
                var groups = fields.FindProperty("groups");
                groups.arraySize = 1;
                var group = groups.GetArrayElementAtIndex(0);
                group.FindPropertyRelative("prefab").objectReferenceValue = spawner.DefaultPrefab;
                group.FindPropertyRelative("count").intValue = 10 + i * 5;
                group.FindPropertyRelative("spawnInterval").floatValue = 0.75f;
                group.FindPropertyRelative("delayBeforeGroup").floatValue = 0;
                fields.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(data, path);
                waveData[i] = data;
            }
            var waves = Object.FindFirstObjectByType<WaveManager>() ?? new GameObject("Wave Manager").AddComponent<WaveManager>();
            waves.Configure(waveData, spawner, Object.FindFirstObjectByType<EnemyRegistry>(),
                Object.FindFirstObjectByType<EconomyManager>(), Object.FindFirstObjectByType<GameStateController>());
            var waveHud = hud.GetComponent<WaveHud>() ?? hud.gameObject.AddComponent<WaveHud>();
            waveHud.Configure(waves);
            foreach (var component in new Object[] { hud, spawner, waves, waveHud }) EditorUtility.SetDirty(component);
        }

        private static DefenseDefinition GetDefense(string assetName, string id, string displayName, int cost, int health,
            float range, int damage, float fireRate, TargetingMode mode)
        {
            string prefabPath = $"Assets/Game/Prefabs/Defenses/{assetName}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<DefenseBase>(prefabPath);
            if (prefab == null)
            {
                var square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Placeholders/Square.png");
                var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/TopDownPlaceholder.mat");
                GameObject root;
                if (assetName == "SniperTower")
                {
                    root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Phase2BSceneSetup.PrefabPath));
                    Object.DestroyImmediate(root.GetComponent<MachineGunPresentation>());
                    Object.DestroyImmediate(root.GetComponent<MachineGunCombat>());
                    root.AddComponent<TurretCombat>();
                    var weapon = root.transform.Find("Rotating Weapon");
                    var muzzle = weapon.Find("Muzzle");
                    var flash = muzzle.Find("Muzzle Flash").GetComponent<SpriteRenderer>();
                    var tracer = root.transform.Find("Reusable Tracer").GetComponent<SpriteRenderer>();
                    var presentation = root.AddComponent<TurretPresentation>();
                    presentation.Configure(weapon, muzzle, flash, tracer);
                    var tuning = new SerializedObject(presentation);
                    tuning.FindProperty("flashDuration").floatValue = 0.18f;
                    tuning.FindProperty("tracerWidth").floatValue = 0.09f;
                    tuning.ApplyModifiedPropertiesWithoutUndo();
                    tracer.color = flash.color = new Color(0.4f, 0.9f, 1);
                    root.transform.Find("Sandbag Base").GetComponent<SpriteRenderer>().color = new Color(0.36f, 0.56f, 0.65f);
                    weapon.Find("Barrel").localScale = new Vector3(1, 0.11f, 1);
                    muzzle.localPosition = new Vector3(0.94f, 0, 0);
                }
                else
                {
                    root = new GameObject(displayName);
                    root.AddComponent<DefenseBase>();
                    if (assetName == "BarbedWire")
                    {
                        root.AddComponent<BarbedWire>();
                        for (int i = 0; i < 3; i++)
                        {
                            var rail = Shape("Wire strand", root.transform, square, material, new Color(0.65f, 0.7f, 0.71f), new Vector2(0, (i - 1) * 0.3f), new Vector2(1.1f, 0.07f), 1);
                            rail.transform.localRotation = Quaternion.Euler(0, 0, i == 1 ? -15 : 15);
                        }
                        Shape("Left post", root.transform, square, material, new Color(0.3f, 0.25f, 0.2f), new Vector2(-0.45f, 0), new Vector2(0.13f, 0.9f), 2);
                        Shape("Right post", root.transform, square, material, new Color(0.3f, 0.25f, 0.2f), new Vector2(0.45f, 0), new Vector2(0.13f, 0.9f), 2);
                    }
                    else
                    {
                        root.AddComponent<LandMine>();
                        var body = Shape("Mine body", root.transform, square, material, new Color(0.2f, 0.26f, 0.2f), Vector2.zero, new Vector2(0.6f, 0.6f), 1);
                        body.transform.localRotation = Quaternion.Euler(0, 0, 45);
                        Shape("Trigger", root.transform, square, material, new Color(0.95f, 0.45f, 0.16f), Vector2.zero, new Vector2(0.2f, 0.2f), 2);
                        var flash = Shape("Mine Explosion Flash", root.transform, square, material, new Color(1, 0.65f, 0.2f, 0.5f), Vector2.zero, Vector2.one, 15);
                        flash.sortingLayerName = "Effects";
                        flash.enabled = false;
                        root.AddComponent<MineExplosionPresentation>().Configure(flash);
                    }
                }
                root.name = displayName;
                prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath).GetComponent<DefenseBase>();
                Object.DestroyImmediate(root);
            }
            string definitionPath = $"Assets/Game/Data/Defenses/{assetName}.asset";
            var definition = AssetDatabase.LoadAssetAtPath<DefenseDefinition>(definitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<DefenseDefinition>();
                var fields = new SerializedObject(definition);
                fields.FindProperty("id").stringValue = id;
                fields.FindProperty("displayName").stringValue = displayName;
                fields.FindProperty("prefab").objectReferenceValue = prefab;
                fields.FindProperty("cost").intValue = cost;
                fields.FindProperty("maxHealth").intValue = health;
                fields.FindProperty("damage").intValue = damage;
                fields.FindProperty("rangeInCells").floatValue = range;
                fields.FindProperty("fireRate").floatValue = fireRate;
                fields.FindProperty("targetingMode").enumValueIndex = (int)mode;
                fields.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(definition, definitionPath);
            }
            return definition;
        }

        private static SpriteRenderer Shape(string name, Transform parent, Sprite sprite, Material material, Color color, Vector2 position, Vector2 size, int order)
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
    }
}
