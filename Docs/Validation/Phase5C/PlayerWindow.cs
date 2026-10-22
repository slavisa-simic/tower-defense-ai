var assembly = System.Array.Find(System.AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "ShorelineDefense.Runtime");
var local = assembly.GetType("ShorelineDefense.Persistence.LocalGameData");
local.GetMethod("UseDirectory").Invoke(null, new object[] { System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BorderDefendersStandalone5C") });
UnityEngine.Application.runInBackground = true; UnityEngine.Application.targetFrameRate = 60;
var type = assembly.GetType("ShorelineDefense.Persistence.GameSettings"); var data = System.Activator.CreateInstance(type);
type.GetField("width").SetValue(data, 1366); type.GetField("height").SetValue(data, 768); type.GetField("fullscreen").SetValue(data, false); type.GetField("vsync").SetValue(data, false); type.GetField("vfxDensity").SetValue(data, 0);
var settingsType = assembly.GetType("ShorelineDefense.Persistence.SettingsService"); var service = settingsType.GetProperty("Instance").GetValue(null);
return "saved=" + settingsType.GetMethod("Save").Invoke(service, new object[] { data }) + "; " + UnityEngine.JsonUtility.ToJson(data);
