using System;
using UnityEditor;
using UnityEngine;
public static class Phase6ETune
{
    static void Set(string path,string name,object value)
    {
        var target=AssetDatabase.LoadMainAssetAtPath(path);var s=new SerializedObject(target);var p=s.FindProperty(name);
        if(p==null)throw new Exception(path+" : "+name);
        if(value is int i)p.intValue=i;else if(value is float f)p.floatValue=f;else throw new Exception("type");
        s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(target);
    }
    public static string Early()
    {Set("Assets/Game/Data/Waves/EndlessSurvival.asset","earlyPressureTarget",8);Set("Assets/Game/Data/Waves/EndlessSurvival.asset","pressureGrowthPerWave",4f);AssetDatabase.SaveAssets();return "Early pressure 8 + 4 per wave; quantity/HP unchanged";}
    static void Defense(string asset,int ii,int iii,float d2=1.25f,float d3=1.75f,float r2=1.1f,float r3=1.25f)
    {
        string p="Assets/Game/Data/Defenses/"+asset+".asset";
        Set(p,"levelII.cost",ii);Set(p,"levelIII.cost",iii);Set(p,"levelII.damage",d2);Set(p,"levelIII.damage",d3);Set(p,"levelII.fireRate",r2);Set(p,"levelIII.fireRate",r3);
    }
    public static string Roles()
    {
        string root="Assets/Game/Data/Defenses/";
        Defense("MachineGunNest",75,150);
        Defense("SniperTower",150,225,1.4f,2f,1f,1.2f);Set(root+"SniperTower.asset","damage",180);Set(root+"SniperTower.asset","targetingMode",4);Set(root+"SniperTower.asset","armorPenetration",1f);
        Defense("AutoCannon",300,450,1.25f,1.75f,1.1f,1.25f);Set(root+"AutoCannon.asset","damage",100);Set(root+"AutoCannon.asset","armorPenetration",.75f);
        Defense("MortarPosition",200,300,1.25f,1.75f,1.25f,1.5f);
        Defense("RocketLauncherPosition",450,675,1.25f,1.75f,1.25f,1.5f);Set(root+"RocketLauncherPosition.asset","damage",350);
        Defense("FlamethrowerPosition",150,225,1.25f,1.6f,1f,1f);
        Defense("BarbedWire",30,50,1f,1f,1f,1f);Set(root+"BarbedWire.asset","levelIII.slow",.7f);
        Defense("LandMine",20,35,1.4f,2f,1f,1f);
        Set("Assets/Game/Data/Enemies/Bulwark.asset","armor",.5f);
        Set("Assets/Game/Data/Enemies/Spitter.asset","structureRangeCells",6f);
        AssetDatabase.SaveAssets();return "Individual heavy roles, targeting, armor, upgrades, wire and mine tuned";
    }
    public static string Rewards()
    {
        foreach(var pair in new[]{("Crawler",1),("Skitter",3),("Bulwark",8),("Spitter",4),("Burrower",4),("Broodling",1)})Set("Assets/Game/Data/Enemies/"+pair.Item1+".asset","killReward",pair.Item2);
        Set("Assets/Game/Data/Waves/EndlessSurvival.asset","baseReward",300);Set("Assets/Game/Data/Waves/EndlessSurvival.asset","rewardGrowth",2);
        AssetDatabase.SaveAssets();return "Role-priced rewards: mass 1, fast 3, armored 8, ranged 4, bypass 4, larva 1. Predictable wave grant 300 + 2 per wave. Boss rewards unchanged.";
    }
    public static string Late()
    {Set("Assets/Game/Data/Waves/EndlessSurvival.asset","maximumPressureTarget",240);AssetDatabase.SaveAssets();return "Late pressure ceiling 240 of hard cap 400";}
}
