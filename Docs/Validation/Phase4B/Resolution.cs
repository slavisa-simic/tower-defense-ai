using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public static class Phase4BResolution
{
    const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
    static object Sizes()
    {
        var type=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        return typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance",BindingFlags.Static|BindingFlags.Public).GetValue(null);
    }
    static object Group()=>Sizes().GetType().GetProperty("currentGroup",Flags).GetValue(Sizes());
    static EditorWindow View()=>EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
    public static string Inspect()
    {
        var group=Group();var view=View();
        return JsonConvert.SerializeObject(new{labels=group.GetType().GetMethod("GetDisplayTexts",Flags).Invoke(group,null),selected=view.GetType().GetProperty("selectedSizeIndex",Flags).GetValue(view)});
    }
    public static async Task<string> Set(int width,int height)
    {
        var group=Group();var view=View();var selected=view.GetType().GetProperty("selectedSizeIndex",Flags);
        string record="Docs/Validation/Phase4B/view-original.json";
        if(!File.Exists(record))File.WriteAllText(record,JsonConvert.SerializeObject(new{selected=selected.GetValue(view)}));
        var sizeType=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
        var labels=(string[])group.GetType().GetMethod("GetDisplayTexts",Flags).Invoke(group,null);
        string label="Phase4B "+width+"x"+height;int index=Array.FindIndex(labels,s=>s.Contains("("+width+"x"+height+")")||s.StartsWith(label));
        if(index<0)
        {
            var constructor=sizeType.GetConstructors(Flags).First(c=>c.GetParameters().Length==4);
            var enumType=constructor.GetParameters()[0].ParameterType;
            var size=constructor.Invoke(new[]{Enum.Parse(enumType,"FixedResolution"),(object)width,height,label});
            group.GetType().GetMethod("AddCustomSize",Flags).Invoke(group,new[]{size});
            index=(int)group.GetType().GetMethod("GetTotalCount",Flags).Invoke(group,null)-1;
        }
        selected.SetValue(view,index);view.Repaint();await Task.Delay(300);
        if(Screen.width!=width||Screen.height!=height)throw new Exception("Game View resolution did not apply: "+Screen.width+"x"+Screen.height);
        return "Actual Game View set to "+Screen.width+"x"+Screen.height;
    }
    public static string Restore()
    {
        var data=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("Docs/Validation/Phase4B/view-original.json"));
        var view=View();view.GetType().GetProperty("selectedSizeIndex",Flags).SetValue(view,(int)data["selected"]);view.Repaint();
        return "Restored original Game View selection";
    }
}

