using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;

public static class Phase6ESnapshot
{
    public static string Baseline() => Write("BASELINE");
    public static string Final() => Write("FINAL_BALANCE");
    public static string Write(string label)
    {
        var originalCulture=System.Threading.Thread.CurrentThread.CurrentCulture;
        try
        {
        System.Threading.Thread.CurrentThread.CurrentCulture=System.Globalization.CultureInfo.InvariantCulture;
        Directory.CreateDirectory("Docs/Balance");
        string folder = "Docs/Validation/Phase6E/";
        var assets = AssetDatabase.FindAssets("t:ScriptableObject", new[]{"Assets/Game/Data"})
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).ToArray();
        var raw = new JObject();
        foreach(var p in assets) raw[p] = JObject.Parse(JsonUtility.ToJson(AssetDatabase.LoadMainAssetAtPath(p)));
        File.WriteAllText(folder+label.ToLowerInvariant()+"-assets.json",raw.ToString());
        if(label=="BASELINE")
        {
            var sources = new JObject();
            foreach(string p in Directory.GetFiles("Assets/Game/Scripts","*.cs",SearchOption.AllDirectories)) sources[p.Replace('\\','/')] = File.ReadAllText(p);
            File.WriteAllText(folder+"baseline-source.json",sources.ToString());
        }
        var b = new StringBuilder("# Phase 6E "+label.Replace('_',' ')+"\n\nCaptured from loaded Unity assets, 2026-11-02. All authored ScriptableObjects (including legacy wave assets and all research effects) are preserved in `Docs/Validation/Phase6E/"+label.ToLowerInvariant()+"-assets.json`. Values below are zero research; range is in cells. DPS is sustained nominal single-target output before armor, overkill, travel, coverage and downtime. Flame includes its continuous burn; mines are one charge.\n\n");
        b.AppendLine("## Defenses and all levels\n\n|Defense / level|Paid total RSD|HP|Damage|Attacks/s|Range|Splash / minimum|Slow|Nominal DPS|RSD/DPS|RSD/HP|Coverage cells²|RSD/coverage|RSD/(DPS×blast area)|\n|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---:|---:|---:|---:|");
        foreach(string p in assets)
        {
            var d=AssetDatabase.LoadAssetAtPath<DefenseDefinition>(p);if(d==null)continue;int paid=d.Cost;
            for(int level=1;level<=3;level++)
            {
                var l=d.LevelData(level);if(l!=null)paid+=l.cost;
                int hp=Mathf.RoundToInt(d.MaxHealth*(l?.health??1)),damage=Mathf.RoundToInt(d.Damage*(l?.damage??1));
                float rate=d.FireRate*(l?.fireRate??1),range=d.RangeInCells*(l?.range??1),splash=(d.HeavyWeapon?.splashRadiusInCells??0)*(l?.splash??1),min=d.HeavyWeapon?.minimumRangeInCells??0;
                double coverage=Math.PI*(range*range-min*min),dps=damage*rate;
                if(d.Id=="barbed-wire"){coverage=d.Footprint.x*d.Footprint.y;splash=0;}
                if(d.Id=="flamethrower"){coverage*=d.HeavyWeapon.coneAngle/360.0;splash=0;}
                if(d.Id=="land-mine")splash=range;
                if(d.Id=="land-mine"||d.Id=="barbed-wire")dps=0;
                if(d.Id=="flamethrower")dps=damage/d.HeavyWeapon.damageTick+d.HeavyWeapon.burnDps*(l?.burnDps??1);
                string slow=d.Id=="barbed-wire"?((1-d.MovementMultiplier*(l?.slow??1))*100).ToString("0.#")+"%":"—";
                b.AppendLine($"|{d.DisplayName} {level}|{paid}|{hp}|{damage}|{rate:0.###}|{range:0.###}|{splash:0.###} / {min:0.###}|{slow}|{dps:0.##}|{(dps>0?paid/dps:0):0.##}|{paid/(double)hp:0.###}|{coverage:0.##}|{paid/coverage:0.##}|{(dps>0&&splash>0?paid/(dps*Math.PI*splash*splash):0):0.###}|");
            }
        }
        b.AppendLine("\nCoverage uses an unobstructed circle/annulus, the actual 55-degree Flame cone, or Wire footprint cells. Mine splash equals its detonation range and is a single charge. Wire's displayed slow applies only while crossing its cell. Zero ratios mean not applicable. Armor penetration, if present, applies to direct hits only and is described in the change record.\n\n## Creatures\n\n|Creature|HP|Speed|Armor|Kill RSD|Homicides|Structure damage / interval|Wave cost|Unlock|\n|---|---:|---:|---:|---:|---:|---|---:|---:|");
        foreach(string p in assets){var e=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(p);if(e==null)continue;b.AppendLine($"|{e.DisplayName}|{e.MaxHealth}|{e.MovementSpeed}|{e.Armor:P0}|{e.KillReward}|{e.BreachPenalty}|{e.StructureDamage} / {e.StructureAttackInterval}s|{e.WaveCostHalfUnits/2f}|{e.UnlockWave}|");}
        var s=AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
        b.AppendLine("\n## Seeded wave projections\n\nSeed 1977, no event, additive boss escorts included. Rewards assume every organism is killed; these are maximum income, not a playthrough.\n\n|Wave|Count|HP multiplier|Speed multiplier|Pressure target|Carrier limit|Kill RSD|Wave RSD|\n|---|---:|---:|---:|---:|---:|---:|---:|");
        var gen=new EndlessWaveGenerator(s,1977);
        foreach(int n in new[]{1,3,5,10,15,20,25,30,40,50,100,250,1000,int.MaxValue})
        {var w=gen.Generate(n);b.AppendLine($"|{n}|{w.ScheduledCount}|{w.HealthMultiplier:0.###}|{w.SpeedMultiplier:0.###}|{s.PressureTarget(n)}|{s.BoatConcurrency(n)}|{w.Groups.Sum(g=>(long)g.Count*g.Prefab.Definition.KillReward)}|{w.CompletionReward}|");UnityEngine.Object.DestroyImmediate(w);}
        b.AppendLine("\n## Economy, meta, events and abilities\n\nStarting balance 500 RSD (full tree 750). Repair = ceil(paid investment × 0.5 × missing/max HP × phase × research × event); phase Preparation 0.75 / combat 1.25. Dismantle = floor(paid construction + paid upgrades, multiplied by 0.50; full tree 0.55). Repair is excluded from refundable investment. Upgrade prices are the authored II/III costs, research ×0.90, Ammunition Shortage ×1.20. Completion reward = 100 + 5×(wave−1), full tree ×1.10 floored; see projections.\n\nFull tree: starting +250; blocking HP +20%; Wire HP +20% with no additional slow; selected MG/Cannon damage +5%; weapon fire rate +5%; Sniper range +8%; Mortar/Rocket splash +10%; Flame burn duration +10%; Preparation/combat repair ×0.90; upgrades ×0.90; refunds +0.05; wave reward +10%; information flags. Individual node costs/effects and RP milestones are in the complete asset dump.\n\nBroodmother prefab: 6 larvae per 8s, lifetime 60, concurrent 18. Burrower: one 5-cell bypass after 1s surface travel, leaves 8 final cells, follows valid navigation. Spitter flight 0.65s, initial reload 1.5s. Dredger direct emergence and Broodmother Leviathan arrival; escorts are authored in boss assets and expanded fivefold. Carrier capacities, release/rupture/remains timings and all thirteen event modifiers are recorded verbatim below.\n");
        foreach(string p in assets.Where(p=>p.Contains("/Events/")||p.Contains("/Carriers/")||p.Contains("/Research/")||p.Contains("/Waves/Endless")||p.Contains("/Enemies/Dredger")||p.Contains("/Enemies/Broodmother")))
        {b.AppendLine("### "+p+"\n\n```json\n"+raw[p]+"\n```\n");}
        string document=b.ToString();
        if(label=="FINAL_BALANCE")document=document.Replace("Completion reward = 100 + 5×(wave−1)","Completion reward = "+s.baseReward+" + "+s.rewardGrowth+"×(wave−1)")
            .Replace("Broodmother prefab: 6 larvae per 8s","Broodmother prefab: first release at 3s, then 6 larvae per 8s")
            .Replace("Spitter flight 0.65s, initial reload 1.5s.","Spitter flight 0.65s, initial reload 1.5s; range 6 cells and holds a valid firing position through reload. Sniper direct hits ignore 100% of armor; Auto Cannon ignores 75%.");
        File.WriteAllText("Docs/Balance/PHASE_6E_"+label+".md",document);
        return "Captured "+assets.Length+" assets; tables written";
        }
        finally { System.Threading.Thread.CurrentThread.CurrentCulture=originalCulture; }
    }
}
