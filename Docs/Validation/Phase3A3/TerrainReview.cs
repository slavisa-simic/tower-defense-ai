using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using ShorelineDefense.Grid;

public static class DefenseTerrainReview
{
    public static string Run()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode only; temporary renderers are discarded on stop");
        var grid=UnityEngine.Object.FindFirstObjectByType<GridManager>().Grid;
        var root=new GameObject("P3A3 Temporary Terrain Review");
        string[] biomes={"grass","muddy_shore","wet_bank_transition","bunker_floor"};
        string[] types={"MachineGunNest","SniperTower","BarbedWire","LandMine"};
        for(int x=0;x<4;x++)for(int y=0;y<4;y++)
        {
            var group=new GameObject(biomes[x]+" "+types[y]);group.transform.SetParent(root.transform);
            group.transform.position=grid.CellToWorld(new Vector2Int(8+x*6,15-y*3));
            var ground=new GameObject("Review ground").AddComponent<SpriteRenderer>();ground.transform.SetParent(group.transform,false);
            ground.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Environment/bd_env_"+biomes[x]+"_tile_v001.png");
            ground.drawMode=SpriteDrawMode.Tiled;ground.tileMode=SpriteTileMode.Continuous;ground.size=new Vector2(3.75f,2.5f);ground.transform.localScale=Vector3.one;
            ground.sortingLayerName="Environment";ground.sortingOrder=30;
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Defenses/"+types[y]+".prefab");
            foreach(var src in prefab.GetComponentsInChildren<SpriteRenderer>(true).Where(r=>r.enabled&&r.sprite!=null&&r.sortingLayerName!="Effects"))
            {
                var r=new GameObject(src.name).AddComponent<SpriteRenderer>();r.transform.SetParent(group.transform,false);
                r.sprite=src.sprite;r.sharedMaterial=src.sharedMaterial;r.color=Color.white;r.sortingLayerName=src.sortingLayerName;r.sortingOrder=src.sortingOrder;
            }
            // Previously delivered enemy art shown only as a reference sprite, with no enemy gameplay binding.
            var enemy=new GameObject("Basic infected scale reference").AddComponent<SpriteRenderer>();enemy.transform.SetParent(group.transform,false);
            enemy.transform.localPosition=new Vector3(-1.05f,0,0);enemy.sprite=AssetDatabase.LoadAllAssetsAtPath("Assets/Game/Art/Enemies/BasicInfected/bd_enemy_basic_infected_idle_e_sheet_v001.png").OfType<Sprite>().OrderBy(s=>s.name).First();
            enemy.sortingLayerName="Enemies";
        }
        return "16 temporary contexts, original 51.2 PPU, primary camera/grid unchanged; no gameplay clones.";
    }
    public static string RelocateCapture()
    {
        const string source="Assets/Docs/Validation/Phase3A3/game-over-hud.png";
        File.Copy(source,"Docs/Validation/Phase3A3/game-over-hud.png",true);
        AssetDatabase.DeleteAsset(source);
        foreach(var dir in new[]{"Assets/Docs/Validation/Phase3A3","Assets/Docs/Validation","Assets/Docs"})
            if(Directory.Exists(dir)&&!Directory.EnumerateFileSystemEntries(dir).Any())AssetDatabase.DeleteAsset(dir);
        return "Capture retained under Docs; temporary Assets capture removed.";
    }
}
