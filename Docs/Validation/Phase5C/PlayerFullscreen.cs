var assembly = System.Array.Find(System.AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "ShorelineDefense.Runtime");
var type = assembly.GetType("ShorelineDefense.Persistence.GameSettings"); var data = System.Activator.CreateInstance(type);
type.GetField("width").SetValue(data, 1920); type.GetField("height").SetValue(data, 1080); type.GetField("fullscreen").SetValue(data, true); type.GetField("vsync").SetValue(data, true); type.GetField("vfxDensity").SetValue(data, 1);
var settingsType = assembly.GetType("ShorelineDefense.Persistence.SettingsService"); var service = settingsType.GetProperty("Instance").GetValue(null);
return "saved=" + settingsType.GetMethod("Save").Invoke(service, new object[] { data });
