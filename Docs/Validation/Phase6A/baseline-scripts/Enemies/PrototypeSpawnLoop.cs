using UnityEngine;

namespace ShorelineDefense.Enemies
{
    /// <summary>Phase 1 regression scene only. Never installed in the primary wave scene.</summary>
    [RequireComponent(typeof(EnemySpawner))]
    public sealed class PrototypeSpawnLoop : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float interval = 3;
        [SerializeField, Min(0)] private float initialDelay = 1;
        private EnemySpawner spawner;
        private float timer;
        private void Awake() { spawner = GetComponent<EnemySpawner>(); timer = initialDelay; }
        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0) return;
            timer = interval;
            spawner.TrySpawn(spawner.DefaultPrefab, out _);
        }
    }
}
