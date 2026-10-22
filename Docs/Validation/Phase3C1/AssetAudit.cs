using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public static class Phase3C1Assets
{
    static Color32[] Pixels(string path)
    {
        var t=new Texture2D(2,2);t.LoadImage(File.ReadAllBytes(path));var p=t.GetPixels32();UnityEngine.Object.DestroyImmediate(t);return p;
    }
    public static string Run()
    {
        const string prefix="Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_mask_";
        var evidence=new List<object>();
        var tiles=Enumerable.Range(0,16).Select(m=>Pixels(prefix+m.ToString("D2")+"_v002.png")).ToArray();
        for(int a=0;a<16;a++)for(int b=0;b<16;b++)
        {
            if((a&2)!=0&&(b&8)!=0)for(int y=0;y<64;y++)if(!tiles[a][y*64+63].Equals(tiles[b][y*64]))throw new Exception("Horizontal seam "+a+" "+b);
            if((a&1)!=0&&(b&4)!=0)for(int x=0;x<64;x++)if(!tiles[a][63*64+x].Equals(tiles[b][x]))throw new Exception("Vertical seam "+a+" "+b);
        }
        var paths=Enumerable.Range(0,16).Select(m=>prefix+m.ToString("D2")+"_v002.png").Concat(Directory.GetFiles("Assets/Game/Art/Environment/Vegetation","*.png")).Concat(new[]{"Assets/Game/Art/Effects/ContactShadow/bd_fx_contact_shadow_v001.png"});
        foreach(var path in paths)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);var pixels=Pixels(path);
            bool pass=importer.filterMode==FilterMode.Point&&!importer.mipmapEnabled&&!importer.crunchedCompression&&importer.textureCompression==TextureImporterCompression.Uncompressed&&Mathf.Approximately(sprite.pixelsPerUnit,51.2f)&&pixels.Any(p=>p.a==0)&&pixels.Any(p=>p.a>0);
            evidence.Add(new{path,pass,dimensions=sprite.rect.ToString(),ppu=sprite.pixelsPerUnit,opaque=pixels.Count(p=>p.a==255),transparent=pixels.Count(p=>p.a==0)});
            if(!pass)throw new Exception("Import/alpha failure "+path);
        }
        foreach(var path in Directory.GetFiles("Assets/Game/Art/SourceSheets/p3c1_polish_001","*.png"))
        {
            var t=new Texture2D(2,2);t.LoadImage(File.ReadAllBytes(path));evidence.Add(new{source=path,width=t.width,height=t.height,hash=BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(File.ReadAllBytes(path))).Replace("-","")});UnityEngine.Object.DestroyImmediate(t);
        }
        File.WriteAllText("Docs/Validation/Phase3C1/asset-audit.json",JsonConvert.SerializeObject(new{horizontalSeamPairs=64,verticalSeamPairs=64,allEdgePixelsMatch=true,assets=evidence},Formatting.Indented));
        return "20 runtime import/alpha checks; all 128 connected edge-pairs match pixel-for-pixel";
    }
}
