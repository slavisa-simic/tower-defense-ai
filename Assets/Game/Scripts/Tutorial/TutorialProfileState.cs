using System;
using System.Collections.Generic;
using ShorelineDefense.Persistence;

namespace ShorelineDefense.Tutorial
{
    [Serializable]
    public sealed class TutorialProfileState
    {
        public int version = 1;
        public bool enabled = true;
        public bool runCompleted;
        public List<string> shown = new List<string>();
    }

    /// <summary>Independent UX profile. Never enters a run snapshot or a research transaction.</summary>
    public sealed class TutorialProfileService
    {
        private const string FileName = "tutorial_profile";
        private readonly LocalJsonStore files;
        private TutorialProfileState state;
        public event Action Changed;
        public string Error { get; private set; }
        public bool Enabled => state.enabled;
        public bool RunCompleted => state.runCompleted;
        public bool HasSeen(string id) => state.shown.Contains(id);
        public bool BasicsCompleted => HasSeen("objective") && HasSeen("build") && HasSeen("start-wave") && HasSeen("repair") && HasSeen("upgrade");
        public TutorialProfileService(LocalJsonStore store)
        {
            files = store;
            files.Read<TutorialProfileState>(FileName, Valid, out state, out string error);
            state = state ?? new TutorialProfileState(); Error = error;
        }
        public static bool Valid(TutorialProfileState data) => data != null && data.version == 1 && data.shown != null &&
            data.shown.Count <= 256 && data.shown.TrueForAll(id => !string.IsNullOrWhiteSpace(id) && id.Length <= 80);
        public bool MarkShown(string id)
        {
            if (HasSeen(id)) return true;
            return Save(s => s.shown.Add(id));
        }
        public bool SetEnabled(bool enabled) => Save(s => s.enabled = enabled);
        public bool RecordRunCompleted() => state.runCompleted || Save(s => s.runCompleted = true);
        public bool ResetProgress() => Save(s => { s.shown.Clear(); s.runCompleted = false; });
        private bool Save(Action<TutorialProfileState> change)
        {
            var next = new TutorialProfileState { enabled = state.enabled, runCompleted = state.runCompleted, shown = new List<string>(state.shown) };
            change(next);
            if (!Valid(next)) { Error = "Invalid tutorial profile."; return false; }
            if (!files.Write(FileName, next, out string error)) { Error = error; return false; }
            state = next; Error = null; Changed?.Invoke(); return true;
        }
    }
}
