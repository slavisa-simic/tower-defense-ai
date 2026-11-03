using System;
using UnityEditor;
using ShorelineDefense.Waves;
using ShorelineDefense.Editor;

public static class Phase6ESetup
{
    static void Logging(bool enabled)
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before validation setup.");
        var settings=AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
        settings.writeBalanceLog=enabled;EditorUtility.SetDirty(settings);AssetDatabase.SaveAssetIfDirty(settings);
    }
    public static string Fresh(){Logging(true);return TutorialValidationTools.BeginFreshProfile();}
    public static string Resume()
    {
        Logging(false);
        var receipt=Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("Docs/Validation/Phase6E/fresh-product-loop.json"));
        string directory=(string)receipt["directory"];
        SessionState.SetString("BorderDefenders.Phase6D.ValidationDirectory",directory);
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Game/Scenes/MainMenu.unity");
        return directory;
    }
    public static string Finish(){Logging(false);TutorialValidationTools.EndWalkthrough();return "Production logging disabled; isolated walkthrough redirect cleared.";}
}
