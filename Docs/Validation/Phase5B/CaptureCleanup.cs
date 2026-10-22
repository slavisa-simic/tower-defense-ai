using System.IO;
using UnityEditor;
public static class Phase5BCaptureCleanup
{
    public static string Run()
    {
        foreach(string path in new[]{"Assets/Docs/Validation/Phase5B","Assets/Docs/Validation","Assets/Docs"})
            if(Directory.Exists(path) && Directory.GetFileSystemEntries(path).Length==0) AssetDatabase.DeleteAsset(path);
        return "Removed only empty capture-created folders through AssetDatabase";
    }
}
