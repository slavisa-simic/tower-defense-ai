using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using ShorelineDefense.Core;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.UI;
public static class Phase4DBossLoad {
 static T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindFirstObjectByType<T>();
 public static async Task<string> Run(){
  Time.timeScale=0;var op=SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");while(!op.isDone)await Task.Delay(20);await Task.Delay(100);
  Find<BorderBreachService>().ConfigureThreshold(0);var spawn=Find<EnemySpawner>();spawn.ConfigureWave(400,3,1,3);spawn.ConfigureDeployment(.05f);
  var registry=Find<EnemyRegistry>();var waves=Find<WaveManager>();var boats=Find<BoatLandingSystem>();boats.ConfigureMotion(12,9,.07f,.15f);
  for(int i=0;i<150;i++){var position=spawn.Battlefield.Grid.CellToWorld(new Vector2Int(7+i%5,1+(i/5)%16))+new Vector3(i/80*.2f,0,0);if(!spawn.TrySpawnAt(spawn.DefaultPrefab,position,out _))throw new Exception("Load spawn failed at " + i);}
  var build=Find<BuildManager>();build.Economy.AddMoney(1000);var cannon=Find<BuildHud>().Defenses.First(d=>d.Id=="autocannon");build.TryPlaceDefense(cannon,new Vector2Int(14,4),out var defense);var combat=defense.GetComponent<TurretCombat>();var rows=new List<object>();
  foreach(var prefab in waves.EndlessSettings.bossEncounters.bosses){
   if(!boats.TryDispatch(prefab,1))throw new Exception("Boss transport failed");
   for(int i=0;i<100 && Find<BossEncounterManager>().ActiveBoss==null;i++){boats.Tick(.1f);await Task.Delay(1);}
   var boss=Find<BossEncounterManager>().ActiveBoss;if(boss==null||boss.Health.MaxHealth!=prefab.Definition.MaxHealth*3)throw new Exception("HP/arrival failed");
   var text=Find<BossHUDController>().GetComponent<UIDocument>().rootVisualElement.Q("boss-panel").Query<Label>().ToList().Select(l=>l.text).ToArray();
   if(!text.Any(t=>t!=null && t.Contains(boss.Health.MaxHealth.ToString("N0"))))throw new Exception("Large boss HP display failed");
   boss.transform.position=spawn.Battlefield.Grid.CellToWorld(new Vector2Int(15,4));int hp=boss.Health.CurrentHealth;combat.Tick(2);
   if(combat.Target!=boss || boss.Health.CurrentHealth>=hp)throw new Exception("Highest-HP targeting failed");
   int money=build.Economy.Money;boss.Health.TakeDefenseDamage(int.MaxValue);boss.Health.TakeDefenseDamage(int.MaxValue);
   if(build.Economy.Money-money!=prefab.Definition.KillReward || Find<BossEncounterManager>().ActiveBoss!=null)throw new Exception("Kill/reward cleanup failed");boats.Cancel();await Task.Delay(20);
   if(!boats.TryDispatch(prefab,1))throw new Exception("Boss reuse transport failed");for(int i=0;i<100 && Find<BossEncounterManager>().ActiveBoss==null;i++){boats.Tick(.1f);await Task.Delay(1);}
   boss=Find<BossEncounterManager>().ActiveBoss;if(boss==null)throw new Exception("Reused boss arrival failed");money=build.Economy.Money;long breaches=Find<RunStatistics>().BossesBreached;boss.Tick(100);
   if(Find<RunStatistics>().BossesBreached-breaches!=1 || build.Economy.Money!=money || Find<BorderBreachService>().TryBreach(boss))throw new Exception("Boss breach accounting failed");boats.Cancel();await Task.Delay(20);
   rows.Add(new{kind=prefab.Definition.Kind.ToString(),hp=prefab.Definition.MaxHealth*3,activeStandardEnemies=registry.Count,killReward=prefab.Definition.KillReward,targeting=true,hud=text,breach=true});
  }
  File.WriteAllText("Docs/Validation/Phase4D/boss-under-load.json",JsonConvert.SerializeObject(new{initialStandardEnemies=150,rows,spawn.ReusedEnemies},Formatting.Indented));return "Both bosses: 3x HP, bar, cannon targeting, reward, pooled transport reuse and breach verified with 150 standard enemies";
 }
}

