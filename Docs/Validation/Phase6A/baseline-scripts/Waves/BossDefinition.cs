using UnityEngine;
using ShorelineDefense.Enemies;
namespace ShorelineDefense.Waves {
 [CreateAssetMenu(menuName="Shoreline Defense/Boss Definition")]
 public sealed class BossDefinition : EnemyDefinition {
  [SerializeField] private Sprite bossArt;
  [SerializeField] private GameObject bossBoat;
  [SerializeField] private WaveEnemyGroup[] escorts = System.Array.Empty<WaveEnemyGroup>();
  [SerializeField, Range(.4f,1)] private float arrivalMultiplier = 1;
  [SerializeField, Range(1,2)] private float boatSpeedMultiplier = 1;
  public Sprite BossArt => bossArt;
  public GameObject BossBoat => bossBoat;
  public System.Collections.Generic.IReadOnlyList<WaveEnemyGroup> Escorts => escorts;
  public float ArrivalMultiplier => arrivalMultiplier;
  public float BoatSpeedMultiplier => boatSpeedMultiplier;
 }
}
