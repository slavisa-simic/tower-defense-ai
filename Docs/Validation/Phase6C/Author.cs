using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using ShorelineDefense.Progression;
using ShorelineDefense.UI;
using Object = UnityEngine.Object;

public static class Phase6CAuthor
{
    public static string SaveRewardTuning()
    {
        var rewards=ResearchCatalog.Load().rewards;rewards.qualityHomicidesPerWaveMaximum=25;
        var clean=rewards.milestones.First(m=>m.id=="clean-wave-10");clean.maxHomicidesPerWave=25;
        EditorUtility.SetDirty(rewards);AssetDatabase.SaveAssets();return "Quality allowance max(250,25 x wave reached); clean milestone shares that allowance.";
    }
    public static string ImportRP()
    {
        const string path="Assets/Game/Art/UI/Research/bd_ui_research_points_v001.png";
        AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=2048;importer.SaveAndReimport();
        var catalog=ResearchCatalog.Load();catalog.researchPointIcon=AssetDatabase.LoadAssetAtPath<Sprite>(path);EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();return "Distinct RP research-file icon imported.";
    }
    static T Asset<T>(string path) where T : ScriptableObject
    { var a = AssetDatabase.LoadAssetAtPath<T>(path); if (a != null) return a; a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); return a; }
    static Sprite[] Import(string branch)
    {
        string path = "Assets/Game/Art/UI/Research/bd_ui_research_" + branch + "_sheet_v001.png";
        AssetDatabase.ImportAsset(path); var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.isReadable = true; importer.maxTextureSize = 4096;
        importer.spritePixelsPerUnit = 51.2f; importer.SaveAndReimport();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); var pixels = texture.GetPixels32();
        var rects = new List<SpriteRect>();
        for (int i = 0; i < 5; i++)
        {
            int left = i * texture.width / 5, right = (i + 1) * texture.width / 5;
            int minX = right, maxX = left, minY = texture.height, maxY = 0;
            for (int y = 0; y < texture.height; y++) for (int x = left; x < right; x++) if (pixels[y * texture.width + x].a > 128)
            { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); }
            rects.Add(new SpriteRect { name = branch + "_" + i, rect = new Rect(minX, minY, maxX-minX+1, maxY-minY+1), pivot = new Vector2(.5f,.5f), alignment = SpriteAlignment.Center, spriteID = GUID.Generate() });
        }
        var factories = new SpriteDataProviderFactories(); factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
        var old = provider.GetSpriteRects(); foreach (var rect in rects) { var prior = old.FirstOrDefault(r => r.name == rect.name); if (prior != null) rect.spriteID = prior.spriteID; }
        provider.SetSpriteRects(rects.ToArray());
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        provider.Apply(); importer.isReadable = false; importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }
    public static string Build()
    {
        AssetDatabase.Refresh();
        Directory.CreateDirectory("Assets/Game/Data/Research"); Directory.CreateDirectory("Assets/Game/Resources"); AssetDatabase.Refresh();
        var catalog = Asset<ResearchCatalog>("Assets/Game/Resources/ResearchCatalog.asset");
        var nodes = new List<ResearchNodeDefinition>();
        ResearchNodeDefinition Node(string id,string name,string description,ResearchBranch branch,int cost,int tier,ResearchEffectKind effect,float amount,Vector2 pos,params ResearchNodeDefinition[] prereq)
        {
            var n=Asset<ResearchNodeDefinition>("Assets/Game/Data/Research/"+id+".asset"); n.id=id;n.displayName=name;n.description=description;n.branch=branch;n.costRP=cost;n.tier=tier;n.maxRank=1;n.boardPosition=pos;
            n.prerequisites=prereq;n.effects=new[]{new ResearchEffectDefinition{kind=effect,amount=amount}};nodes.Add(n);EditorUtility.SetDirty(n);return n;
        }
        var root=new Vector2(.5f,0);var left=new Vector2(.25f,106);var right=new Vector2(.75f,106);var mid=new Vector2(.5f,212);var end=new Vector2(.5f,318);
        var a=Node("emergency-fund","Emergency Fund","+100 starting RSD.",ResearchBranch.Logistics,10,1,ResearchEffectKind.StartingRsd,100,root);
        var b=Node("supply-contracts","Supply Contracts","Preparation repairs cost 10% less, after the normal Preparation discount.",ResearchBranch.Logistics,20,2,ResearchEffectKind.PreparationRepairDiscount,.1f,left,a);
        var c=Node("field-salvage","Field Salvage","Dismantling returns 55% of paid construction and upgrade investment.",ResearchBranch.Logistics,25,2,ResearchEffectKind.RefundBonus,.05f,right,a);
        var d=Node("logistics-network","Logistics Network","Wave-completion RSD rewards increase by 10%. Kill income is unchanged.",ResearchBranch.Logistics,35,3,ResearchEffectKind.WaveRewardBonus,.1f,mid,b);
        Node("reserve-stockpile","Reserve Stockpile","+150 additional starting RSD. Total starting research bonus: +250 RSD.",ResearchBranch.Logistics,50,4,ResearchEffectKind.StartingRsd,150,end,c,d);
        a=Node("reinforced-emplacements","Reinforced Emplacements","Blocking player defenses gain +10% Max HP. Excludes Wire and Mines.",ResearchBranch.Engineering,15,1,ResearchEffectKind.BlockingHealthBonus,.1f,root);
        b=Node("hardened-wire","Hardened Wire","Barbed Wire gains +20% Max HP. Slow strength is unchanged.",ResearchBranch.Engineering,20,2,ResearchEffectKind.WireHealthBonus,.2f,left,a);
        c=Node("field-maintenance","Field Maintenance","Combat repairs cost 10% less. Preparation remains cheaper.",ResearchBranch.Engineering,30,2,ResearchEffectKind.CombatRepairDiscount,.1f,right,a);
        d=Node("modular-construction","Modular Construction","Level II and III upgrades cost 10% less. Base construction costs stay unchanged.",ResearchBranch.Engineering,35,3,ResearchEffectKind.UpgradeDiscount,.1f,mid,a);
        Node("fortification-doctrine","Fortification Doctrine","Blocking player defenses gain +10% additional Max HP. Total: +20%. Excludes Wire and Mines.",ResearchBranch.Engineering,55,4,ResearchEffectKind.BlockingHealthBonus,.1f,end,b,c,d);
        a=Node("improved-ammunition","Improved Ammunition","MG Nests and Auto Cannons gain +5% damage.",ResearchBranch.Weapons,20,1,ResearchEffectKind.BallisticDamageBonus,.05f,root);
        b=Node("optical-calibration","Optical Calibration","Sniper Towers gain +8% range.",ResearchBranch.Weapons,25,2,ResearchEffectKind.SniperRangeBonus,.08f,left,a);
        c=Node("area-munitions","Area Munitions","Mortars and Rocket Launchers gain +10% explosion radius.",ResearchBranch.Weapons,30,2,ResearchEffectKind.AreaRadiusBonus,.1f,right,a);
        d=Node("thermal-mixture","Thermal Mixture","Flamethrower burns last 10% longer.",ResearchBranch.Weapons,30,3,ResearchEffectKind.BurnDurationBonus,.1f,mid,a);
        Node("advanced-fire-control","Advanced Fire Control","Ranged weapon defenses fire 5% faster, including Flame damage ticks. Excludes Wire and Mines.",ResearchBranch.Weapons,60,4,ResearchEffectKind.FireRateBonus,.05f,end,a,b,c,d);
        a=Node("river-monitoring","River Monitoring","Preparation reveals the approximate next-wave organism count.",ResearchBranch.Intelligence,10,1,ResearchEffectKind.EnemyCountIntel,1,root);
        b=Node("advance-warning","Advance Warning","Preparation reveals the dominant threat class: swarm, armored, or high-speed contacts.",ResearchBranch.Intelligence,20,2,ResearchEffectKind.ThreatIntel,1,new Vector2(.5f,106),a);
        c=Node("biological-analysis","Biological Analysis","Preparation reveals approximate creature composition: High, Medium, or Low.",ResearchBranch.Intelligence,30,3,ResearchEffectKind.CompositionIntel,1,new Vector2(.25f,212),b);
        d=Node("event-forecasting","Event Forecasting","Reveal the following wave's major event one wave early, before its Preparation.",ResearchBranch.Intelligence,35,3,ResearchEffectKind.EventForecast,1,new Vector2(.75f,212),a);
        Node("deep-water-sensor-network","Deep-Water Sensor Network","Preparation reveals expected carrier pressure, emergence sectors, and massive boss signatures.",ResearchBranch.Intelligence,55,4,ResearchEffectKind.DeepWaterIntel,1,end,c,d);
        foreach(var branch in new[]{"logistics","engineering","weapons","intelligence"})
        {
            var sprites=Import(branch);var group=nodes.Where(n=>n.branch.ToString().ToLowerInvariant()==branch).ToArray();
            for(int i=0;i<5;i++){group[i].icon=sprites[i];EditorUtility.SetDirty(group[i]);}
        }
        var rewards=Asset<ResearchRewardSettings>("Assets/Game/Data/Research/ResearchRewards.asset");
        ResearchMilestone Milestone(string id,MilestoneCondition condition,int wave,int reward,string map=null)=>new ResearchMilestone{id=id,condition=condition,wave=wave,reward=reward,mapId=map};
        rewards.milestones=new[]{Milestone("first-wave-5",MilestoneCondition.WaveReached,5,8),Milestone("first-wave-10",MilestoneCondition.WaveReached,10,5),Milestone("first-wave-25",MilestoneCondition.WaveReached,25,10),Milestone("first-wave-50",MilestoneCondition.WaveReached,50,20),Milestone("first-boss",MilestoneCondition.FirstBoss,0,10),Milestone("first-dredger",MilestoneCondition.Dredger,0,10),Milestone("first-broodmother",MilestoneCondition.Broodmother,0,10),Milestone("clean-wave-10",MilestoneCondition.LowHomicides,10,5),Milestone("border-wave-20",MilestoneCondition.MapWave,20,5,"river_border"),Milestone("port-wave-20",MilestoneCondition.MapWave,20,5,"industrial_port")};
        rewards.milestones.First(m=>m.id=="clean-wave-10").maxHomicidesPerWave=25;
        catalog.nodes=nodes.ToArray();catalog.rewards=rewards;catalog.researchPointIcon=nodes.First(n=>n.id=="river-monitoring").icon;
        EditorUtility.SetDirty(rewards);EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
        foreach(string scene in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            var s=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+scene+".unity");
            var menu=Object.FindFirstObjectByType<ProductMenuController>();if(menu.GetComponent<PreparationIntelHud>()==null)menu.gameObject.AddComponent<PreparationIntelHud>();
            EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);
        }
        EditorSceneManager.OpenScene("Assets/Game/Scenes/MainMenu.unity");
        string report="20 research nodes; "+nodes.Sum(n=>n.costRP)+" RP total; 10 one-time milestones; both maps integrated.";
        File.WriteAllText("Docs/Validation/Phase6C/author.txt",report);return report;
    }
}
