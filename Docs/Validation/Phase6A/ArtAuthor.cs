using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Grid;

public static class Phase6AArt
{
    static Sprite[] props,terrain;
    static Material material;
    static Transform root;
    static BattlefieldGrid grid;
    public static string Run()
    {
        if (Application.isPlaying) throw new Exception("Edit mode only");
        props = Export("Assets/Game/Art/SourceSheets/p6a_port_001/port_props_raw_v002.png",4,128,"Assets/Game/Art/Props/bd_prop_port_");
        terrain = Export("Assets/Game/Art/SourceSheets/p6a_port_001/port_terrain_raw_v001.png",2,64,"Assets/Game/Art/Environment/bd_env_port_");
        var scene = EditorSceneManager.OpenScene("Assets/Game/Scenes/DanubeIndustrialPort.unity");
        var context = UnityEngine.Object.FindFirstObjectByType<MapRuntimeContext>(); grid = context.definition.battlefield.CreateGrid(context.grid.transform.position);
        material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/TopDownPlaceholder.mat");
        var old = GameObject.Find("Port Environment"); if (old != null) UnityEngine.Object.DestroyImmediate(old);
        root = new GameObject("Port Environment").transform;
        var water = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Environment/bd_env_deep_danube_water_tile_v001.png");
        if (water == null) water = AssetDatabase.FindAssets("t:Sprite",new[]{"Assets/Game/Art/Environment"}).Select(g => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g))).First(s => s != null && s.name.Contains("deep_water"));
        // Off-grid river backing deliberately remains presentation-only.
        Make("Danube approach water",water,new Rect(-7,-4,14,26),"Environment",-5);
        for (int y = 0; y < grid.Height; y++) for (int x = 0; x < grid.Width; x++)
        {
            var type = grid.GetCell(new Vector2Int(x,y)).Terrain;
            int index = x >= 28 || y == 5 || y == 6 || y == 11 || y == 12 ? 1 : x < 9 ? 2 : (x+y)%11 == 0 ? 3 : 0;
            Make(type == TerrainType.Water ? "River" : x < 6 ? "Wet pier deck" : "Cargo paving", type == TerrainType.Water ? water : terrain[index],new Rect(x,y,1,1),"Environment",0);
        }
        var blocks = context.definition.battlefield.blockedCells;
        // Container rows follow the same deliberate authored rectangles used by navigation.
        foreach (var rect in new[] {new Rect(10,2,5,3),new Rect(18,2,5,3),new Rect(9,7,4,4),new Rect(16,7,5,3),new Rect(11,13,5,3),new Rect(19,13,4,3)})
        {
            Make("Blocked cargo footprint",terrain[1],rect,"GroundObjects",0);
            for (int y = 0; y < 2; y++) for (int x = 0; x < 2; x++)
                Make("Container block",props[(x+y+(int)rect.x)%4],new Rect(rect.x+x*rect.width/2,rect.y+y*rect.height/2,rect.width/2,rect.height/2),"GroundObjects",3);
        }
        Make("Warehouse roof",props[4],new Rect(24,1,3,4),"GroundObjects",4);
        Make("Maintenance roof",props[5],new Rect(24,7,3,4),"GroundObjects",4);
        Make("Loading shed",props[4],new Rect(24,14,3,3),"GroundObjects",4);
        Make("Port crane landmark",props[8],new Rect(7,16,3,2),"GroundObjects",4);
        foreach (var rect in new[] {new Rect(28,0,4,3),new Rect(28,7,4,3),new Rect(28,15,4,3)})
        {
            Make("Quarantine service compound",terrain[0],rect,"GroundObjects",1);
            Make("Checkpoint booth",props[5],new Rect(rect.x+1,rect.y+.6f,2,1.8f),"GroundObjects",4);
            for (int i = 0; i < 4; i++) Make("Reinforced checkpoint barrier",props[13],new Rect(rect.x+i,rect.y+.15f,1,.6f),"GroundObjects",4);
        }
        var fence=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Props/bd_prop_quarantine_fence_segment_v001.png");
        var sandbag=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Props/bd_prop_sandbag_segment_v001.png");
        foreach(int y in new[]{0,1,2,7,8,9,15,16,17}) { Make("Checkpoint perimeter fence",fence,new Rect(27.8f,y,.3f,1),"GroundObjects",5); Make("Checkpoint sandbags",sandbag,new Rect(28,y,.7f,1),"GroundObjects",4); }
        foreach(int y in new[]{3,10,16}) for(int x=4;x<9;x++) Make("Pier mooring edge",props[13],new Rect(x,y-.2f,1,.22f),"GroundObjects",3);
        int[] piers = {2,7,13}; foreach (int y in piers)
        {
            Make("Mooring ladder",props[12],new Rect(4,y,1,1),"GroundObjects",4);
            Make("Dockside crane",props[8],new Rect(6,y+1,2.4f,2),"GroundObjects",4);
            for (int x = 4; x < 8; x++) Make("Dock hazard markings",terrain[3],new Rect(x,y+.1f,1,.35f),"GroundObjects",1);
        }
        for (int i = 0; i < 16; i++)
        {
            int x = 9+(i*7)%18, y = 1+(i*5)%16;
            if (!grid.GetCell(new Vector2Int(x,y)).Walkable) continue;
            Make("Decorative cargo / oil",props[i%3==0 ? 14 : i%3==1 ? 9 : 10],new Rect(x+.1f,y+.1f,.7f,.7f),"GroundObjects",2);
        }
        Make("Abandoned logistics truck",props[7],new Rect(29,8,2.2f,1.5f),"GroundObjects",5);
        Make("Forklift parking",props[6],new Rect(26,14,1.2f,1.2f),"GroundObjects",5);
        foreach (int y in new[]{5,12})
        {
            Make("Inland road",terrain[1],new Rect(27,y-1,7,3),"Environment",1);
            for (int x = 27; x < 32; x++) Make("Exit road marking",terrain[3],new Rect(x,y+.35f,.75f,.12f),"GroundObjects",2);
            Make("Inspection floodlight",props[15],new Rect(28,y-1,1,1),"GroundObjects",5);
            Make("Inspection floodlight",props[15],new Rect(28,y+1,1,1),"GroundObjects",5);
        }
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        return "Generated runtime exports: 16 props and 4 terrain sprites; saved static port environment.";
    }
    static SpriteRenderer Make(string name, Sprite sprite, Rect cells, string layer, int order)
    {
        var go = new GameObject(name); go.isStatic = true; go.transform.SetParent(root);
        go.transform.position = grid.Origin + new Vector3(cells.center.x*grid.CellSize,cells.center.y*grid.CellSize,0);
        go.transform.localScale = new Vector3(cells.width*grid.CellSize/sprite.bounds.size.x,cells.height*grid.CellSize/sprite.bounds.size.y,1);
        if(layer=="GroundObjects" && order>=3){var sh=new GameObject("Contact Shadow");sh.isStatic=true;sh.transform.SetParent(root);sh.transform.position=go.transform.position+new Vector3(.04f,-.04f,0);sh.transform.localScale=go.transform.localScale;var sr=sh.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sharedMaterial=material;sr.sortingLayerName=layer;sr.sortingOrder=order-1;sr.color=new Color(.12f,.16f,.17f,.27f);}
        var r = go.AddComponent<SpriteRenderer>(); r.sprite = sprite; r.sharedMaterial = material; r.sortingLayerName = layer; r.sortingOrder = order; return r;
    }
    static Sprite[] Export(string source,int count,int size,string prefix)
    {
        var input = new Texture2D(2,2); input.LoadImage(File.ReadAllBytes(source)); var result = new Sprite[count*count];
        for (int i = 0; i < result.Length; i++)
        {
            int left=(i%count)*input.width/count, bottom=(count-1-i/count)*input.height/count;
            int minX=input.width/count, minY=input.height/count,maxX=0,maxY=0;
            for(int sy=0;sy<input.height/count;sy++)for(int sx=0;sx<input.width/count;sx++) if(input.GetPixel(left+sx,bottom+sy).a>=.5f){minX=Math.Min(minX,sx);minY=Math.Min(minY,sy);maxX=Math.Max(maxX,sx);maxY=Math.Max(maxY,sy);}
            var output = new Texture2D(size,size,TextureFormat.RGBA32,false);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                int sx = left + minX + Mathf.FloorToInt((x+.5f)*(maxX-minX+1)/size);
                int sy = bottom + minY + Mathf.FloorToInt((y+.5f)*(maxY-minY+1)/size);
                var c = input.GetPixel(sx,sy); c.a = c.a >= .5f ? 1 : 0; if (c.a == 0) c = Color.clear; output.SetPixel(x,y,c);
            }
            output.Apply(); string path = prefix+i.ToString("00")+"_v001.png"; File.WriteAllBytes(path,output.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(output);
            AssetDatabase.ImportAsset(path); var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 51.2f; importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.npotScale = TextureImporterNPOTScale.None; importer.spriteImportMode = SpriteImportMode.Single; importer.SaveAndReimport();
            result[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        UnityEngine.Object.DestroyImmediate(input); return result;
    }
}

