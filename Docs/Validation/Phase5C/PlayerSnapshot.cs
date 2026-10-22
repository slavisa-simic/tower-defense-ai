var assembly = System.Array.Find(System.AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "ShorelineDefense.Runtime");
var type = assembly.GetType("ShorelineDefense.Persistence.RunSession"); var session = UnityEngine.Object.FindFirstObjectByType(type); return UnityEngine.JsonUtility.ToJson(type.GetMethod("Capture").Invoke(session, null));
