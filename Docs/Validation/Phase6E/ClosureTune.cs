using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ShorelineDefense.Waves;
using ShorelineDefense.Enemies;
using ShorelineDefense.Progression;

public static class Phase6E1Tune
{
    static void Set(string path,string name,object value)
    {
        var target=AssetDatabase.LoadMainAssetAtPath(path);var s=new SerializedObject(target);var p=s.FindProperty(name);
        if(p==null)throw new Exception(path+":"+name);
        if(value is int i)p.intValue=i;else if(value is float f)p.floatValue=f;else if(value is string text)p.stringValue=text;
        s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(target);
    }
    public static string BeginValidation() => ShorelineDefense.Editor.TutorialValidationTools.BeginFreshProfile();
    public static string EndValidation() { ShorelineDefense.Editor.TutorialValidationTools.EndWalkthrough(); return "Validation redirect cleared"; }
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop play first");
        string wave="Assets/Game/Data/Waves/EndlessSurvival.asset";
        Set(wave,"incomeTaperStartWave",10);Set(wave,"incomeTaperEndWave",30);Set(wave,"lateKillIncome",.12f);Set(wave,"lateWaveIncome",.5f);
        string dredger="Assets/Game/Data/Enemies/Dredger.asset",brood="Assets/Game/Data/Enemies/Broodmother.asset";
        Set(dredger,"maxHealth",18000);Set(dredger,"armor",.6f);Set(dredger,"movementSpeed",.9f);Set(dredger,"structureDamage",150);
        Set(brood,"maxHealth",13500);Set(brood,"armor",.55f);Set(brood,"movementSpeed",.9f);
        string wire="Assets/Game/Data/Research/hardened-wire.asset";
        Set(wire,"description","Level III Barbed Wire slows creatures by 70% instead of 65%. Walkable; overlapping Wire does not stack.");
        Set(wire,"effects.Array.data[0].kind",(int)ResearchEffectKind.WireSlowBonus);Set(wire,"effects.Array.data[0].amount",.05f);
        AssetDatabase.SaveAssets();
        var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/DanubeIndustrialPort.unity");
        var boats=UnityEngine.Object.FindFirstObjectByType<BoatLandingSystem>();var so=new SerializedObject(boats);
        so.FindProperty("distributedPressureFromWave").intValue=11;so.FindProperty("lateDirectRatio").floatValue=.55f;so.FindProperty("lateReleaseRate").floatValue=3f;so.FindProperty("latePressureMultiplier").floatValue=2.25f;so.FindProperty("lateArrivalMultiplier").floatValue=.5f;so.FindProperty("lateExtraCarriers").intValue=6;so.FindProperty("lateSpitterWeight").floatValue=3;so.FindProperty("lateBurrowerWeight").floatValue=4;so.FindProperty("lateFormationMultiplier").floatValue=2.4f;
        var points=so.FindProperty("landingPoints");if(points.arraySize==8)points.InsertArrayElementAtIndex(8);
        if(points.arraySize!=9)throw new Exception("Unexpected landing count");
        var inlet=points.GetArrayElementAtIndex(8);inlet.FindPropertyRelative("riverEntry").vector3Value=new Vector3(-25,-10,0);
        inlet.FindPropertyRelative("berth").vector3Value=new Vector3(-7.5f,-10,0);inlet.FindPropertyRelative("authoredSector").boolValue=true;
        inlet.FindPropertyRelative("sector").enumValueIndex=0;inlet.FindPropertyRelative("minimumWave").intValue=11;
        var unload=inlet.FindPropertyRelative("unloadingPositions");unload.arraySize=2;unload.GetArrayElementAtIndex(0).vector3Value=new Vector3(-4.375f,-10.625f,0);unload.GetArrayElementAtIndex(1).vector3Value=new Vector3(-4.375f,-9.375f,0);
        var grid=UnityEngine.Object.FindFirstObjectByType<ShorelineDefense.Grid.GridManager>().Definition.CreateGrid(UnityEngine.Object.FindFirstObjectByType<ShorelineDefense.Grid.GridManager>().transform.position);
        for(int i=0;i<points.arraySize;i++)
        {
            var point=points.GetArrayElementAtIndex(i);var original=point.FindPropertyRelative("unloadingPositions");var cells=new System.Collections.Generic.HashSet<Vector2Int>();
            for(int j=0;j<original.arraySize;j++)if(grid.TryWorldToCell(original.GetArrayElementAtIndex(j).vector3Value,out var center))
                for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++){var cell=center+new Vector2Int(x,y);if(grid.Contains(cell)&&grid.IsTraversable(cell))cells.Add(cell);}
            var positions=point.FindPropertyRelative("lateUnloadingPositions");positions.arraySize=cells.Count;int n=0;
            foreach(var cell in System.Linq.Enumerable.ThenBy(System.Linq.Enumerable.OrderBy(cells,c=>c.y),c=>c.x))positions.GetArrayElementAtIndex(n++).vector3Value=grid.CellToWorld(cell);
        }
        var battlefield=AssetDatabase.LoadAssetAtPath<ShorelineDefense.Grid.BattlefieldDefinition>("Assets/Game/Data/Maps/BattlefieldIndustrialPort.asset");
        var bs=new SerializedObject(battlefield);var noBuild=bs.FindProperty("noBuildCells");noBuild.arraySize=18;int nbb=0;
        foreach(int x in new[]{4,5})foreach(int y in new[]{2,3,4,7,8,9,13,14,15})noBuild.GetArrayElementAtIndex(nbb++).vector2IntValue=new Vector2Int(x,y);
        bs.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(battlefield);AssetDatabase.SaveAssetIfDirty(battlefield);
        so.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return "Late income, boss durability, Wire III and late Port inlet/distribution tuned";
    }
}












