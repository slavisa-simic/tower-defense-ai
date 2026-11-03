using System;
using System.Collections.Generic;

namespace ShorelineDefense.Tutorial
{
    public sealed class TutorialPrompt
    {
        public TutorialDefinition Definition { get; }
        public object Subject { get; }
        public TutorialPrompt(TutorialDefinition definition, object subject) { Definition = definition; Subject = subject; }
    }

    /// <summary>Single presenter queue. Events enqueue; the UI pumps once after a batch of events.</summary>
    public sealed class TutorialManager : IDisposable
    {
        private readonly TutorialProfileService profile;
        private readonly List<TutorialPrompt> pending = new List<TutorialPrompt>();
        public TutorialPrompt Active { get; private set; }
        public IReadOnlyList<TutorialPrompt> Queue => pending;
        public TutorialManager(TutorialProfileService progress) { profile = progress; profile.Changed += ProfileChanged; }
        public bool Enqueue(string id, object subject = null)
        {
            var d = TutorialCatalog.Find(id);
            if (!profile.Enabled || d == null || profile.HasSeen(id) || Active?.Definition.Id == id) return false;
            // Refresh a contextual target without duplicating its lesson.
            int existing = pending.FindIndex(p => p.Definition.Id == id);
            if (existing >= 0) { pending[existing] = new TutorialPrompt(d, subject); return false; }
            pending.Add(new TutorialPrompt(d, subject)); return true;
        }
        public TutorialPrompt Pump(Func<TutorialPrompt, bool> eligible)
        {
            if (!profile.Enabled) { Clear(); return null; }
            if (Active != null && !eligible(Active)) { pending.Add(Active); Active = null; }
            TutorialPrompt best = null;
            foreach (var p in pending) if (eligible(p) && (best == null || p.Definition.Priority > best.Definition.Priority)) best = p;
            if (best == null) return Active;
            if (Active != null && (best.Definition.Priority < 125 || best.Definition.Priority <= Active.Definition.Priority)) return Active;
            if (Active != null) pending.Add(Active);
            pending.Remove(best); Active = best;
            profile.MarkShown(best.Definition.Id);
            return Active;
        }
        public void Dismiss() => Active = null;
        public void Clear() { Active = null; pending.Clear(); }
        private void ProfileChanged() { if (!profile.Enabled) Clear(); }
        public void Dispose() => profile.Changed -= ProfileChanged;
    }
}
