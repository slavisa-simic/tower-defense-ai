var assembly = System.Array.Find(System.AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "ShorelineDefense.Runtime");
var type = assembly.GetType("ShorelineDefense.UI.ProductMenuController"); var menu = UnityEngine.Object.FindFirstObjectByType(type); type.GetMethod("Continue").Invoke(menu, null); return "Continue requested";
