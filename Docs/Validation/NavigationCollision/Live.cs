using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Persistence;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using Object=UnityEngine.Object;

public static class NavigationLive
{
    const string Folder="Docs/Validation/NavigationCollision/";
    static T Find<T>()where T:Object=>Object.FindFirstObjectByType<T>();
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static async Task Load(string map)
    {
        RunSession.Pending=null;var operation=SceneManager.LoadSceneAsync(map);while(!operation.isDone)await Task.Delay(20);await Task.Delay(250);Time.timeScale=0;
        Find<BorderBreachService>().ConfigureThreshold(0);Find<BuildManager>().Economy.AddMoney(50000);
    }
    static void Capture(string name)
    {
        var camera=Camera.main;var target=new RenderTexture(1920,1080,24);var original=camera.targetTexture;var active=RenderTexture.active;
        var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);
        try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(Folder+name+".png",image.EncodeToPNG());}
        finally{camera.targetTexture=original;RenderTexture.active=active;Object.DestroyImmediate(image);Object.DestroyImmediate(target);}
    }
    static void DisableWeapons(DefenseBase d)
    {foreach(var c in d.GetComponents<MonoBehaviour>())if(c is TurretCombat||c is FlameWeapon||c is AreaProjectileWeapon)c.enabled=false;}
    public static async Task<string> Run()
    {
        Check(Application.isPlaying,"Play mode required");var report=new List<string>();
        foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            await Load(map);var builder=Find<BuildManager>();var grid=builder.Battlefield.Grid;var registry=Find<EnemyRegistry>();
            string validation=Find<MapRuntimeContext>().ValidateStaticObstacles(out int invalid);Check(invalid==0,validation);report.Add(validation);
            var spawner=Find<EnemySpawner>();spawner.ConfigureDeployment(.1f);spawner.ConfigureWave(400,1,1);
            var waves=Find<WaveManager>();var crawler=waves.EndlessSettings.enemies.Single(e=>e.Definition.Kind==EnemyKind.Crawler);
            var bulwark=waves.EndlessSettings.enemies.Single(e=>e.Definition.Kind==EnemyKind.Bulwark);
            var defenses=Find<BuildHud>().Defenses;var mg=defenses.Single(d=>d.Id=="machine-gun-nest");
            foreach(var cell in new[]{new Vector2Int(13,5),new Vector2Int(15,11),new Vector2Int(21,12)})
            {Check(builder.TryPlaceDefense(mg,cell,out var defense)==PlacementResult.Valid,"Open-route defense");DisableWeapons(defense);}
            for(int i=0;i<18;i++)
            {
                int y=i;var start=new Vector2Int(6,y);if(!grid.IsTraversable(start))continue;
                spawner.TrySpawnAt(crawler,grid.CellToWorld(start),out _);
            }
            Time.timeScale=3;
            for(int t=0;t<100;t++)
            {
                foreach(var e in registry.Active.ToArray())
                {if(e.gameObject.activeInHierarchy){Check(grid.TryWorldToCell(e.transform.position,out var c)&&grid.IsTraversable(c),"Open-route ghost");Check(e.StructureAttack.AttacksPerformed==0,"Open-route tower hunt");}}
                if(t==25)Capture(map+"-open-routes");await Task.Delay(34);
            }
            Time.timeScale=0;
            foreach(var e in registry.Active.ToArray())e.Health.TakeDefenseDamage(int.MaxValue);
            foreach(var d in Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None))builder.TryDismantle(d,out _);
            var wall=new List<DefenseBase>();
            for(int y=0;y<grid.Height;y++)
            {
                var c=new Vector2Int(14,y);if(!grid.IsTraversable(c))continue;
                var data=defenses.Single(d=>d.Id==(y%3==0?"machine-gun-nest":y%3==1?"sniper-tower":"flamethrower"));
                Check(builder.TryPlaceDefense(data,c,out var d)==PlacementResult.Valid,"Wall");DisableWeapons(d);wall.Add(d);
            }
            for(int y=2;y<17;y+=2)
            {var start=new Vector2Int(8,y);if(grid.IsTraversable(start))spawner.TrySpawnAt(bulwark,grid.CellToWorld(start),out _);}
            bool sawAttack=false,sawDamage=false,capturedAttack=false,capturedOpen=false;int before=wall.Count;
            Time.timeScale=3;
            for(int t=0;t<320&&registry.Count>0;t++)
            {
                foreach(var e in registry.Active.ToArray())
                {
                    if(!e.gameObject.activeInHierarchy)continue;
                    Check(grid.TryWorldToCell(e.transform.position,out var c)&&grid.IsTraversable(c),"Wall ghost");
                    if(e.StructureAttack.State==StructureAttackState.Attacking)
                    {sawAttack=true;if(e.StructureAttack.Target!=null){Find<StructureSelection>().Select(e.StructureAttack.Target);sawDamage|=e.StructureAttack.Target.CurrentHealth<e.StructureAttack.Target.MaxHealth;}}
                }
                if(sawDamage&&!capturedAttack){Capture(map+"-wall-attacks");capturedAttack=true;}
                if(wall.Any(d=>d==null||!d.IsAlive)&&!capturedOpen){Capture(map+"-wall-opened");capturedOpen=true;}
                await Task.Delay(34);
            }
            Check(sawAttack&&sawDamage,"Attack animation/HP state");Check(registry.Count==0,"Wall crowd resumes");
            Check(wall.Count(d=>d!=null&&d.IsAlive)<before,"Actual destruction");
            report.Add(map+": inspected open routing, MG/Sniper/Flame wall contact, attack pose, live selected HP/damage, destruction and resumed border progress. All sampled cells traversable.");
            Time.timeScale=0;
        }
        Time.timeScale=0;File.WriteAllLines(Folder+"live-results.txt",report);return string.Join("\n",report);
    }
}
