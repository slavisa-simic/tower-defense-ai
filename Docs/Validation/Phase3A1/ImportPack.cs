using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class Phase3A1ImportPack
{
    public static string Archives()
    {
        var selected = new HashSet<string>();
        foreach (JObject e in JArray.Parse(File.ReadAllText("Docs/Art/Batches/p3a1_final_manifest.json"))) selected.Add((string)e["source"]);
        var rows = new List<object>();
        foreach (string nativePath in Directory.GetFiles("Assets/Game/Art/SourceSheets", "*.png", SearchOption.AllDirectories))
        {
            string path = nativePath.Replace('\\','/');
            if (selected.Contains(path)) continue;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var t = (TextureImporter)AssetImporter.GetAtPath(path);
            t.textureType = TextureImporterType.Default;
            t.filterMode = FilterMode.Point;
            t.textureCompression = TextureImporterCompression.Uncompressed;
            t.crunchedCompression = false;
            t.mipmapEnabled = false;
            t.wrapMode = TextureWrapMode.Clamp;
            t.npotScale = TextureImporterNPOTScale.None;
            t.maxTextureSize = 4096;
            t.isReadable = false;
            t.sRGBTexture = true;
            t.alphaSource = TextureImporterAlphaSource.FromInput;
            t.alphaIsTransparency = false;
            t.SaveAndReimport();
            rows.Add(new { path, guid=AssetDatabase.AssetPathToGUID(path), type=t.textureType.ToString(), filter=t.filterMode.ToString(), t.mipmapEnabled });
        }
        File.WriteAllText("Docs/Validation/Phase3A1/archived-source-imports.json", JsonConvert.SerializeObject(rows, Formatting.Indented));
        return "Configured " + rows.Count + " non-runtime provenance textures";
    }
    public static string Run(string manifest)
    {
        var entries = JArray.Parse(File.ReadAllText(manifest));
        var rows = new List<object>();
        foreach (JObject entry in entries)
        {
            string output = (string)entry["output"], source = (string)entry["source"];
            if (!output.StartsWith("Assets/Game/Art/") || output.Contains("..") ||
                !source.StartsWith("Assets/Game/Art/SourceSheets/") || source.Contains(".."))
                throw new ArgumentException("Unexpected asset path");
            foreach (string path in new[] { source, output })
            {
                bool runtime = path == output;
                if (!File.Exists(path)) throw new FileNotFoundException(path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = runtime ? TextureImporterType.Sprite : TextureImporterType.Default;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.crunchedCompression = false;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.sRGBTexture = true;
                importer.isReadable = false;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = runtime && (bool)entry["alpha"];
                importer.maxTextureSize = 4096;
                string repeat = runtime ? ((string)entry["repeat"] ?? "") : "";
                importer.wrapModeU = repeat.Contains("x") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                importer.wrapModeV = repeat.Contains("y") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                importer.wrapModeW = TextureWrapMode.Clamp;
                foreach (var platform in new[] { "Standalone", "Android", "iPhone", "WebGL" })
                    importer.ClearPlatformTextureSettings(platform);
                if (runtime)
                {
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteMode = (int)SpriteImportMode.Single;
                    settings.spritePixelsPerUnit = output.Contains("/UI/") ? 100f : 51.2f;
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    settings.spriteAlignment = (int)SpriteAlignment.Center;
                    settings.spritePivot = new Vector2(0.5f, 0.5f);
                    settings.spriteGenerateFallbackPhysicsShape = false;
                    if (entry["border"] is JArray b)
                        settings.spriteBorder = new Vector4((float)b[0],(float)b[1],(float)b[2],(float)b[3]);
                    importer.SetTextureSettings(settings);
                }
                importer.SaveAndReimport();
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (runtime && (texture.width != (int)entry["size"][0] || texture.height != (int)entry["size"][1]))
                    throw new InvalidOperationException("Unexpected imported size: " + path);
                rows.Add(new { path, runtime, guid = AssetDatabase.AssetPathToGUID(path), texture.width, texture.height,
                    type = importer.textureType.ToString(), filter = importer.filterMode.ToString(),
                    compression = importer.textureCompression.ToString(), importer.mipmapEnabled,
                    importer.spritePixelsPerUnit, importer.alphaIsTransparency,
                    wrapU = importer.wrapModeU.ToString(), wrapV = importer.wrapModeV.ToString(),
                    spriteBorder = importer.spriteBorder.ToString(), pivot = importer.spritePivot.ToString(),
                    spriteImportMode = importer.spriteImportMode.ToString(), importer.maxTextureSize,
                    importer.isReadable, importer.sRGBTexture, npotScale = importer.npotScale.ToString() });
            }
        }
        string report = "Docs/Validation/Phase3A1/" + Path.GetFileNameWithoutExtension(manifest) + "-imports.json";
        File.WriteAllText(report, JsonConvert.SerializeObject(rows, Formatting.Indented));
        return "Imported " + rows.Count + " source/runtime textures; " + report;
    }
}
