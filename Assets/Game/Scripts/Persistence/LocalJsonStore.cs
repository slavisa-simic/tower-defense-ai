using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace ShorelineDefense.Persistence
{
    /// <summary>Bounded local JSON with flushed temporary writes and atomic replacement.</summary>
    public sealed class LocalJsonStore
    {
        public string Root { get; }
        public LocalJsonStore(string directory) { Root = directory; }
        public string PathFor(string name) => Path.Combine(Root, name + ".json");
        public bool Write<T>(string name, T data, out string error)
        {
            error = null;
            try
            {
                Directory.CreateDirectory(Root);
                string target = PathFor(name), temporary = target + ".tmp";
                byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
                using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
                if (File.Exists(target)) File.Replace(temporary, target, target + ".bak");
                else File.Move(temporary, target);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
            { error = exception.Message; return false; }
        }
        public bool Read<T>(string name, Func<T, bool> validate, out T data, out string error) where T : class
        {
            data = null; error = null;
            string target = PathFor(name);
            foreach (string path in new[] { target, target + ".bak" })
            {
                if (!File.Exists(path)) continue;
                try
                {
                    if (new FileInfo(path).Length > 4 * 1024 * 1024) throw new IOException("Local data file is too large.");
                    var candidate = JsonUtility.FromJson<T>(File.ReadAllText(path));
                    if (candidate == null || !validate(candidate)) throw new IOException("Unsupported or invalid local data.");
                    data = candidate;
                    if (path != target) error = "Recovered the previous save from backup.";
                    return true;
                }
                catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
                { error = exception.Message; }
            }
            return false;
        }
        public bool Delete(string name, out string error)
        {
            error = null;
            try { foreach (string suffix in new[] { "", ".bak", ".tmp" }) { string path = PathFor(name) + suffix; if (File.Exists(path)) File.Delete(path); } return true; }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            { error = exception.Message; return false; }
        }
    }
}
