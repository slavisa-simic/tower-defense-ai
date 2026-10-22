using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

// Explicit non-runtime reference import only. Never touches placeholders or consumers.
public static class Phase3A1MasterImport
{
    public static string Run()
    {
        var rows = new List<object>();
        foreach (var revision in new[] { "v001", "v002" })
        foreach (var path in new[] {
            "Assets/Game/Art/MasterReferences/bd_ref_danube_master_" + revision + ".png",
            "Assets/Game/Art/SourceSheets/p3a1_master_001/bd_src_danube_master_" + revision + ".png" })
        {
            if (!File.Exists(path)) continue;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("No texture importer: " + path);
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.sRGBTexture = true;
            importer.isReadable = false;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            rows.Add(new { path, guid = AssetDatabase.AssetPathToGUID(path), texture.width, texture.height,
                type = importer.textureType.ToString(), filter = importer.filterMode.ToString(),
                compression = importer.textureCompression.ToString(), importer.mipmapEnabled,
                importer.maxTextureSize, npot = importer.npotScale.ToString(), importer.isReadable,
                runtimeConsumer = (string)null });
        }
        var json = JsonConvert.SerializeObject(rows, Formatting.Indented);
        File.WriteAllText("Docs/Validation/Phase3A1/master-imports.json", json);
        return "Configured " + rows.Count + " non-runtime reference/source textures; no scene or prefab changes.";
    }
}
