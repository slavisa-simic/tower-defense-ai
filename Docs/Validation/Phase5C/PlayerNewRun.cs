var assembly = System.Array.Find(System.AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "ShorelineDefense.Runtime");
var menuType = assembly.GetType("ShorelineDefense.UI.ProductMenuController"); var menu = UnityEngine.Object.FindFirstObjectByType(menuType);
menuType.GetMethod("NewGame").Invoke(menu, null);
return "Standalone New Game dispatched";
