using UnityEngine;
using ShorelineDefense.Enemies;
namespace ShorelineDefense.Waves {
 [CreateAssetMenu(menuName="Shoreline Defense/Boss Encounters")]
 public sealed class BossEncounterDefinition : ScriptableObject {
  [Min(1)] public int firstWave = 25;
  [Min(1)] public int interval = 25;
  public EnemyController[] bosses = System.Array.Empty<EnemyController>();
  public EnemyController ForWave(int wave) {
   if(wave < Mathf.Max(1,firstWave) || bosses == null || bosses.Length == 0 || ((long)wave - Mathf.Max(1,firstWave)) % Mathf.Max(1,interval) != 0) return null;
   return bosses[(int)(((long)wave - Mathf.Max(1,firstWave))/Mathf.Max(1,interval) % bosses.Length)];
  }
 }
}
