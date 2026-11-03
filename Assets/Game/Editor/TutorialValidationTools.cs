#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using ShorelineDefense.Persistence;
using ShorelineDefense.Tutorial;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShorelineDefense.Editor
{
    /// <summary>Opt-in Editor-only isolated walkthrough. Never redirects production saves by default.</summary>
    public static class TutorialValidationTools
    {
        private const string Key = "BorderDefenders.Phase6D.ValidationDirectory";
        public static string BeginFreshProfile()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before beginning a walkthrough.");
            string directory = Path.GetFullPath(Path.Combine("Temp", "Phase6DProfiles", Guid.NewGuid().ToString("N")));
            SessionState.SetString(Key, directory);
            EditorSceneManager.OpenScene("Assets/Game/Scenes/MainMenu.unity");
            return directory;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RedirectWalkthrough()
        { string directory = SessionState.GetString(Key, ""); if (!string.IsNullOrEmpty(directory)) LocalGameData.UseDirectory(directory); }
        public static void EndWalkthrough() => SessionState.EraseString(Key);
        public static string Snapshot()
        {
            var tutorial = UnityEngine.Object.FindFirstObjectByType<TutorialRuntime>();
            var doc = tutorial.GetComponent<UIDocument>(); var root = doc.rootVisualElement;
            string[] ids = { "tutorial-panel", "tutorial-title", "tutorial-copy", "tutorial-detail", "health-panel", "money-panel", "event-panel", "boss-panel", "build-panel", "structure-panel", "preparation-intelligence" };
            return JsonUtility.ToJson(new UiSnapshot {
                profile = LocalGameData.Files.Root, active = tutorial.Manager.Active?.Definition.Id,
                queue = tutorial.Manager.Queue.Select(p => p.Definition.Id).ToArray(), width = root.resolvedStyle.width, height = root.resolvedStyle.height,
                elements = ids.Select(id => { var e = root.Q(id); return new Element { id = id, visible = e != null && e.resolvedStyle.display != DisplayStyle.None && e.visible, bounds = e?.worldBound ?? Rect.zero, text = (e as Label)?.text }; }).ToArray()
            }, true);
        }
        [Serializable] private class UiSnapshot { public string profile, active; public string[] queue; public float width, height; public Element[] elements; }
        [Serializable] private class Element { public string id, text; public bool visible; public Rect bounds; }
    }
}
#endif
