using System;
using UnityEngine;
using ShorelineDefense.Enemies;
using ShorelineDefense.Core;
namespace ShorelineDefense.Waves {
 /// <summary>Observes encounters; WaveManager retains all scheduling and completion ownership.</summary>
 public sealed class BossEncounterManager : MonoBehaviour {
  [SerializeField] private WaveManager waves;
  [SerializeField] private EnemyRegistry registry;
  [SerializeField] private BoatLandingSystem boats;
  [SerializeField] private BorderBreachService border;

  private WaveDefinition announced;
  public EnemyController ActiveBoss { get; private set; }
  public BossDefinition IncomingBoss { get; private set; }
  public string Message { get; private set; }
  public float MessageUntil { get; private set; }
  public event Action Changed;
  private void OnEnable() { waves.Changed += OnWave; boats.Unloaded += OnUnloaded; registry.EnemyRemoved += OnRemoved; border.BossBreached += OnBreach; }
  private void OnDisable() { waves.Changed -= OnWave; boats.Unloaded -= OnUnloaded; registry.EnemyRemoved -= OnRemoved; border.BossBreached -= OnBreach; ClearHealth(); IncomingBoss=null; }
  private void OnWave() {
   if(waves.State == WaveState.GameOver){ClearHealth();IncomingBoss=null;Changed?.Invoke();return;}
   if(waves.State != WaveState.Spawning || waves.CurrentWave == announced) return;
   announced=waves.CurrentWave;IncomingBoss=announced.Encounter;
   if(IncomingBoss!=null){Message="SPECIAL LANDING DETECTED — " + IncomingBoss.DisplayName;MessageUntil=Time.time+4;Changed?.Invoke();}
  }
  private void OnUnloaded(EnemyController enemy) { if(!(enemy.Definition is BossDefinition))return;ClearHealth();ActiveBoss=enemy;IncomingBoss=null;enemy.Health.Damaged += OnDamage;Changed?.Invoke(); }
  private void OnDamage(int amount)=>Changed?.Invoke();
  private void ClearHealth(){if(ActiveBoss!=null)ActiveBoss.Health.Damaged-=OnDamage;ActiveBoss=null;}
  private void OnRemoved(EnemyController enemy){if(enemy!=ActiveBoss)return;ClearHealth();Changed?.Invoke();}
  private void OnBreach(EnemyController enemy,int penalty){Message=$"BOSS BREACH +{penalty} HOMICIDES";MessageUntil=Time.time+4;Changed?.Invoke();}
 }
}
