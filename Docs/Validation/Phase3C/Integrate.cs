using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using ShorelineDefense.Base;
using ShorelineDefense.Grid;
using ShorelineDefense.Economy;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Defenses;

public static class Phase3CIntegrate
{
    const string ScenePath="Assets/Game/Scenes/CoastalDefense_TopDown.unity";
    static Material material;
    static Vector3 origin;
    static Sprite Sprite(string category,string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/"+category+"/bd_"+name+"_v001.png");
    static SpriteRenderer Visual(Transform parent,string name,Sprite sprite,Vector3 position,string layer="GroundObjects",int order=-5,float angle=0)
    {
        if(sprite==null)throw new Exception("Missing sprite "+name);
        var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;
        go.transform.localRotation=Quaternion.Euler(0,0,angle);
        var r=go.AddComponent<SpriteRenderer>();r.sprite=sprite;r.color=Color.white;r.sharedMaterial=material;r.sortingLayerName=layer;r.sortingOrder=order;return r;
    }
    static Vector3 Cell(float x,float y)=>origin+new Vector3((x+.5f)*1.25f,(y+.5f)*1.25f,0);
    static void Prop(Transform parent,string category,string name,float x,float y,float angle=0)=>Visual(parent,name,Sprite(category,name),Cell(x,y),"GroundObjects",-5,angle);
    static SpriteRenderer Tile(Transform p,string name,string asset,Vector3 pos,Vector2 size,int order=2,float opacity=1)
    {
        var r=Visual(p,name,Sprite("Environment",asset),pos,"Environment",order);
        r.drawMode=SpriteDrawMode.Tiled;r.tileMode=SpriteTileMode.Continuous;r.size=size;r.transform.localScale=Vector3.one;r.color=new Color(1,1,1,opacity);return r;
    }
    static void Set(UnityEngine.Object owner,string field,UnityEngine.Object value)
    {var s=new SerializedObject(owner);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    public static string Run()
    {
        var scene=SceneManager.GetActiveScene();if(Application.isPlaying||scene.path!=ScenePath||scene.isDirty)throw new Exception("Requires clean saved primary scene in Edit Mode");
        AssetDatabase.Refresh();
        if(File.Exists("Assets/Docs/Validation/Phase3C/baseline-world.png"))
        {File.Copy("Assets/Docs/Validation/Phase3C/baseline-world.png","Docs/Validation/Phase3C/baseline-world.png",true);AssetDatabase.DeleteAsset("Assets/Docs");}
        material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/TopDownPlaceholder.mat");
        var grid=UnityEngine.Object.FindFirstObjectByType<GridManager>();origin=grid.transform.position;
        var old=GameObject.Find("Danube Quarantine Dressing");if(old!=null)UnityEngine.Object.DestroyImmediate(old);
        var dressing=new GameObject("Danube Quarantine Dressing").transform;
        var terrain=new GameObject("Terrain detail").transform;terrain.SetParent(dressing,false);
        Tile(terrain,"Quarantine concrete apron","env_concrete_tile",Cell(29,8.5f),new Vector2(6.25f,22.5f),2);
        Tile(terrain,"Checkpoint access road","env_road_tile",Cell(29,8.7f),new Vector2(6.25f,2.5f),3);
        Tile(terrain,"Command bunker floor","env_bunker_floor_tile",Cell(29,9),new Vector2(3.75f,3.75f),4);
        for(int y=0;y<18;y++)
        {
            float alpha=.28f+(y%4)*.06f;
            Tile(terrain,"Wet silt fringe "+y,"env_muddy_shore_tile",Cell(6,y),Vector2.one*1.25f,2,alpha);
            if(y%3!=0)Tile(terrain,"Silt fade "+y,"env_muddy_shore_tile",Cell(7,y),Vector2.one*1.25f,2,.11f);
        }
        // Quiet tracks and weathered patches, never occupancy or route changes.
        for(int x=8;x<27;x++)foreach(int lane in new[]{4,9,14})
        {
            int y=lane+((x/5+lane)%3==0?1:0);
            Tile(terrain,"Worn track "+x+" "+lane,"env_dirt_tile",Cell(x,y),Vector2.one*1.25f,2,.18f);
            if(x%3==0)Tile(terrain,"Track fringe "+x+" "+lane,"env_dirt_tile",Cell(x,y+1),Vector2.one*1.25f,2,.07f);
        }
        foreach(var c in new[]{new Vector2(10,2),new Vector2(18,16),new Vector2(23,3),new Vector2(8,12)})
            Tile(terrain,"Weathered soil "+c,"env_cracked_ground_tile",Cell(c.x,c.y),Vector2.one*1.25f,2,.3f);
        var river=new GameObject("River arrival props").transform;river.SetParent(dressing,false);
        Prop(river,"Props","prop_rubber_boat",1.6f,4.2f,9);
        Prop(river,"Props","prop_rubber_boat",1.4f,13.6f,-8);
        Prop(river,"Props","prop_destroyed_rubber_boat",3.8f,10.4f,20);
        Prop(river,"Props","prop_wrecked_supply_box",5.1f,11.7f,15);
        Prop(river,"Props","prop_debris_scrap",5.3f,3.3f,25);
        Prop(river,"Props","prop_oil_barrel",5.1f,15.2f);
        Prop(river,"Props","prop_debris_scrap",6.1f,15.5f,-20);
        var line=new GameObject("Quarantine position").transform;line.SetParent(dressing,false);
        for(int y=1;y<=16;y++)if(y<6||y>12)
            Prop(line,"Base","base_bunker_wall_segment",26.3f,y,90);
        Prop(line,"Base","base_bunker_gate_piece",26.3f,7.1f,90);
        Prop(line,"Base","base_river_checkpoint_marker",27.8f,8.4f);
        Prop(line,"Base","base_military_tent_topdown",29.4f,14.3f);
        Prop(line,"Base","base_watch_post",27.7f,3.1f);
        Prop(line,"Base","base_generator_unit",30.1f,5.9f);
        Prop(line,"Base","base_ammo_supply_crate_cluster",29.7f,3.6f);
        Prop(line,"Props","prop_antenna",30.1f,12.1f);
        Prop(line,"Props","prop_radar_device",27.7f,16);
        Prop(line,"Props","prop_floodlight",26.8f,5.5f,180);
        Prop(line,"Props","prop_floodlight",26.8f,12.7f,180);
        Prop(line,"Props","prop_warning_sign",25.4f,6.6f);
        Prop(line,"Props","prop_warning_sign",25.4f,11.3f);
        Prop(line,"Props","prop_military_barrel",30.3f,13.1f);
        Prop(line,"Props","prop_metal_crate",28.1f,12.1f);
        Prop(line,"Props","prop_wooden_crate",30.4f,16.2f);
        foreach(float x in new[]{27.7f,28.8f,29.9f,31f})
        {Prop(line,"Props","prop_quarantine_fence_segment",x,1.4f);Prop(line,"Props","prop_quarantine_fence_segment",x,16.9f);}
        Prop(line,"Props","prop_sandbag_segment",24.8f,3.2f);
        Prop(line,"Props","prop_sandbag_segment",24.8f,14.7f);
        Prop(line,"Props","prop_czech_hedgehog_obstacle",24.1f,1.6f);
        Prop(line,"Props","prop_czech_hedgehog_obstacle",24.1f,16.2f);
        var guides=GameObject.Find("Ground Cell Guides");if(guides!=null)guides.SetActive(false);
        string basePath="Assets/Game/Prefabs/Gameplay/PlayerBase_TopDown.prefab";
        var baseRoot=PrefabUtility.LoadPrefabContents(basePath);
        try
        {
            var visual=baseRoot.transform.Find("Visual");
            var roof=visual.Find("Roof")??visual.Find("Command Post");roof.name="Command Post";roof.localScale=Vector3.one;roof.localPosition=Vector3.zero;
            var r=roof.GetComponent<SpriteRenderer>();r.sprite=Sprite("Base","base_command_post_object");r.color=Color.white;r.sortingLayerName="GroundObjects";r.sortingOrder=8;
            foreach(string name in new[]{"Footprint","Base","Roof Stripe"}){var t=visual.Find(name);if(t!=null)UnityEngine.Object.DestroyImmediate(t.gameObject);}
            PrefabUtility.SaveAsPrefabAsset(baseRoot,basePath);
        }finally{PrefabUtility.UnloadPrefabContents(baseRoot);}
        // Clean dormant primitive children while retaining muzzle/flash/tracer references.
        foreach(string name in new[]{"MachineGunNest","SniperTower","BarbedWire","LandMine","Crawler_TopDown"})
        {
            string path="Assets/Game/Prefabs/"+(name=="Crawler_TopDown"?"Enemies/":"Defenses/")+name+".prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach(var r in root.GetComponentsInChildren<SpriteRenderer>(true))
                    if(!r.enabled&&r.sprite==null&&r.transform.childCount==0&&r.name!="Muzzle Flash"&&r.name!="Tracer"&&r.name!="Water Wake"&&r.name!="Explosion")
                        UnityEngine.Object.DestroyImmediate(r.gameObject);
                if(name=="Crawler_TopDown")
                {
                    foreach(string obsolete in new[]{"Body Outline","Facing Tip"})
                    {var t=root.transform.Find("Visual/"+obsolete);if(t!=null)UnityEngine.Object.DestroyImmediate(t.gameObject);}
                    var s=new SerializedObject(root.GetComponent<EnemySpritePresentation>());var p=s.FindProperty("hit");
                    var frames=AssetDatabase.LoadAllAssetsAtPath("Assets/Game/Art/Enemies/BasicInfected/bd_enemy_basic_infected_hit_e_sheet_v001.png").OfType<Sprite>().OrderBy(f=>f.name).ToArray();
                    p.arraySize=frames.Length;for(int i=0;i<frames.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=frames[i];s.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        var hud=UnityEngine.Object.FindFirstObjectByType<BuildHud>();
        hud.ConfigureCatalog(new[]{"MachineGunNest","BarbedWire","LandMine","SniperTower"}.Select(n=>AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/"+n+".asset")).ToArray());
        hud.ConfigureDisplayOrder(new[]{"BarbedWire","LandMine","MachineGunNest","SniperTower"}.Select(n=>AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/"+n+".asset")).ToArray());
        var art=hud.GetComponent<HudArtPresentation>();if(art==null)art=hud.gameObject.AddComponent<HudArtPresentation>();
        Set(art,"economy",UnityEngine.Object.FindFirstObjectByType<EconomyManager>());Set(art,"playerBase",UnityEngine.Object.FindFirstObjectByType<BaseHealth>());
        var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Art/UI/bd_ui_hud_pixel_v001.ttf");if(font==null)throw new Exception("HUD font import missing");
        Set(art,"pixelFont",font);Set(hud.GetComponent<CombatFeedbackHud>(),"pixelFont",font);
        // Reserve HUD strips so edge cells remain visible and clickable at both target resolutions.
        Camera.main.rect=new Rect(0,.17f,1,.69f);
        var camera=Camera.main.GetComponent<TopDownCamera>();var cs=new SerializedObject(camera);cs.FindProperty("padding").floatValue=.6f;cs.ApplyModifiedPropertiesWithoutUndo();camera.FrameBattlefield();
        EditorUtility.SetDirty(hud);EditorUtility.SetDirty(art);EditorUtility.SetDirty(hud.GetComponent<CombatFeedbackHud>());
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        File.WriteAllText("Docs/Validation/Phase3C/dressing.json",JsonConvert.SerializeObject(dressing.GetComponentsInChildren<SpriteRenderer>().Select(r=>new{r.name,path=AssetDatabase.GetAssetPath(r.sprite),position=r.transform.position.ToString(),r.sortingLayerName,r.sortingOrder,alpha=r.color.a}),Formatting.Indented));
        return "Integrated saved environment/base/HUD, hit frames and cleaned dormant primitives; "+dressing.GetComponentsInChildren<SpriteRenderer>().Length+" decorative renderers";
    }
}
