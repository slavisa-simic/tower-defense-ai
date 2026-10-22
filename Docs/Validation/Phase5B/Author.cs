using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ShorelineDefense.Waves;
using EventModifiers = ShorelineDefense.Waves.EventModifiers;
using ShorelineDefense.Enemies;
using ShorelineDefense.UI;
using ShorelineDefense.Presentation;
using ShorelineDefense.Grid;

public static class Phase5BAuthor
{
    static T Get<T>(GameObject go) where T:Component => go.GetComponent<T>() ?? go.AddComponent<T>();
    static Sprite Sprite(string path) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First();
    static CompositionBias Bias(EnemyKind kind, float multiplier) => new CompositionBias { kind=kind, multiplier=multiplier };
    public static string Integrate()
    {
        if(EditorApplication.isPlaying) throw new Exception("Edit Mode required");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!="Assets/Game/Scenes/CoastalDefense_TopDown.unity" || scene.isDirty)throw new Exception("Clean primary scene required");
        const string folder="Assets/Game/Data/Events";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Game/Data","Events");
        var catalog=AssetDatabase.LoadAssetAtPath<EndlessEventSettings>(folder+"/EndlessEvents.asset");
        if(catalog==null){catalog=ScriptableObject.CreateInstance<EndlessEventSettings>();AssetDatabase.CreateAsset(catalog,folder+"/EndlessEvents.asset");}
        string[] ids={"mass-landing","pirate-raid","militant-assault","heavy-assault","runner-swarm","fog","flooded-shore","ammunition-shortage","emergency-supply","field-engineers","calm-before-storm","multi-sector","concentrated-landing"};
        string[] names={"MASS LANDING","PIRATE RAID","MILITANT ASSAULT","HEAVY ASSAULT","RUNNER SWARM","FOG","FLOODED SHORE","AMMUNITION SHORTAGE","EMERGENCY SUPPLY","FIELD ENGINEERS","CALM BEFORE THE STORM","MULTI-SECTOR ATTACK","CONCENTRATED LANDING"};
        string[] descriptions={"50% more infected. More boats; arrivals 30% faster.","Fast pirate units. Faster craft across multiple sectors.","Militant breachers and heavy targets incoming.","More heavy targets. Reinforce anti-heavy defenses.","Fast infected presence greatly increased. Cover multiple sectors.","DEFENSE RANGE REDUCED BY 20%.","First inland column: NO NEW BUILD. Existing structures stay.","UPGRADES +20% COST / COMBAT REPAIRS +20% COST.","Emergency RSD paper-money supply delivered.","PREPARATION REPAIRS 30% CHEAPER.","Major assault expected: +30% infected, +25% boat slots.","Concurrent landings across North, Center and South.","Most boats will focus on the announced sector."};
        int[] minimum={7,20,18,18,8,5,8,8,5,5,10,8,8};
        var entries=new EndlessEventDefinition[ids.Length];
        for(int i=0;i<ids.Length;i++)
        {
            string path=folder+"/"+ids[i]+".asset";var e=AssetDatabase.LoadAssetAtPath<EndlessEventDefinition>(path);
            if(e==null){e=ScriptableObject.CreateInstance<EndlessEventDefinition>();AssetDatabase.CreateAsset(e,path);}
            e.id=ids[i];e.displayName=names[i];e.description=descriptions[i];e.minimumWave=minimum[i];e.weight=1;e.cooldown=8;
            e.category=i==8||i==9?EventCategory.Relief:i==11||i==12?EventCategory.Pattern:EventCategory.Challenge;
            e.durationType=i==8||i==9?EventDuration.Preparation:EventDuration.EntireWave;e.preparationEffects=i==6||i==7;e.modifiers=new EventModifiers();
            e.uiIcon=Sprite("Assets/Game/Art/UI/"+(i==8?"ui_icon_money_bundle_rsd.png":i==9?"ui_icon_dismantle.png":i==3||i==2?"ui_icon_autocannon.png":"bd_ui_wave_icon_v001.png"));
            entries[i]=e;
        }
        entries[0].modifiers.quantity=1.5f;entries[0].modifiers.pressure=1.5f;entries[0].modifiers.boatConcurrency=1.5f;entries[0].modifiers.boatInterval=.7f;entries[0].modifiers.landingPattern=LandingPattern.Distributed;
        var pirate=entries[1].modifiers;pirate.composition=new[]{Bias(EnemyKind.PirateRunner,5),Bias(EnemyKind.PirateRaider,4),Bias(EnemyKind.Runner,3),Bias(EnemyKind.Basic,.4f)};pirate.boatSpeed=1.2f;pirate.boatInterval=.75f;pirate.landingPattern=LandingPattern.Distributed;pirate.boatVisual=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Presentation/PirateBossTransport.prefab");
        entries[2].modifiers.composition=new[]{Bias(EnemyKind.IsisBreacher,5),Bias(EnemyKind.IsisHeavy,5),Bias(EnemyKind.Brute,2),Bias(EnemyKind.Runner,.5f)};
        entries[3].modifiers.composition=new[]{Bias(EnemyKind.Brute,7),Bias(EnemyKind.IsisHeavy,10),Bias(EnemyKind.Basic,.5f),Bias(EnemyKind.Runner,.4f),Bias(EnemyKind.PirateRunner,.4f)};
        entries[4].modifiers.composition=new[]{Bias(EnemyKind.Runner,6),Bias(EnemyKind.PirateRunner,6),Bias(EnemyKind.Basic,.3f),Bias(EnemyKind.Brute,.3f)};entries[4].modifiers.landingPattern=LandingPattern.Distributed;
        entries[5].modifiers.fog=true;entries[5].modifiers.defenseRange=.8f;
        entries[6].modifiers.floodedColumns=1;
        entries[7].modifiers.upgradeCost=1.2f;entries[7].modifiers.combatRepair=1.2f;
        entries[8].supplyBase=100;entries[8].supplyPerWave=8;entries[8].supplyMaximum=500;
        entries[9].modifiers.preparationRepair=.7f;
        entries[10].preparationWarningSeconds=8;entries[10].modifiers.quantity=1.3f;entries[10].modifiers.pressure=1.3f;entries[10].modifiers.boatConcurrency=1.25f;
        entries[11].modifiers.landingPattern=LandingPattern.Distributed;entries[11].modifiers.boatConcurrency=1.25f;entries[11].modifiers.boatInterval=.7f;
        entries[12].modifiers.landingPattern=LandingPattern.Concentrated;
        catalog.events=entries;foreach(var e in entries)EditorUtility.SetDirty(e);EditorUtility.SetDirty(catalog);
        var waves=UnityEngine.Object.FindFirstObjectByType<WaveManager>();waves.EndlessSettings.eventSettings=catalog;EditorUtility.SetDirty(waves.EndlessSettings);
        var hud=UnityEngine.Object.FindFirstObjectByType<WaveHud>();Get<EndlessEventHud>(hud.gameObject).Configure(waves);EditorUtility.SetDirty(hud.gameObject);
        Get<EndlessEventPresentation>(waves.gameObject).Configure(waves,UnityEngine.Object.FindFirstObjectByType<GridManager>(),Sprite("Assets/Game/Art/Placeholders/Square.png"),Sprite("Assets/Game/Art/Environment/bd_env_shallow_water_tile_v001.png"));EditorUtility.SetDirty(waves.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "13 event definitions, catalog, HUD and two overlays saved";
    }
}

