using System.Linq;
using System.Reflection;
using UnityEngine;
using ShorelineDefense.Presentation;
using Newtonsoft.Json;
public static class InspectRecoil
{
    public static string Run()
    {
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;var type=typeof(TurretPresentation);
        return JsonConvert.SerializeObject(UnityEngine.Object.FindObjectsByType<TurretPresentation>(FindObjectsSortMode.None).Select(t=>new{
            t.name,t.enabled,time=Time.time,started=type.GetField("shotStarted",flags).GetValue(t),
            renderer=((SpriteRenderer)type.GetField("weaponRenderer",flags).GetValue(t))?.name,
            sprite=((SpriteRenderer)type.GetField("weaponRenderer",flags).GetValue(t))?.sprite?.name,
            frames=((Sprite[])type.GetField("firingFrames",flags).GetValue(t))?.Select(s=>s?.name).ToArray(),
            fps=type.GetField("firingFramesPerSecond",flags).GetValue(t)}));
    }
}
