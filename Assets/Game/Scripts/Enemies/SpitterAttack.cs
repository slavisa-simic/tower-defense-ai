using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Enemies
{
    /// <summary>Local grid acquisition, never a scene scan. EnemyController owns the clock.</summary>
    public sealed class SpitterAttack : MonoBehaviour
    {
        private float rangeCells => enemy != null ? enemy.Definition.StructureRangeCells : 5;
        [SerializeField, Min(.2f)] private float acquisitionInterval = .5f;
        private float cooldown => enemy != null ? enemy.Definition.StructureAttackInterval : 5;
        private int damage => enemy != null ? enemy.Definition.StructureDamage : 24;
        [SerializeField, Min(.1f)] private float flightSeconds = .65f;
        [SerializeField] private Sprite glob;
        private EnemyController enemy;
        private GridManager grid;
        private GameStateController game;
        private DefenseBase target;
        private float acquire, reload, pose;
        private ShorelineDefense.Presentation.VfxService effects;
        public bool IsAttacking => pose > 0;
        public int ShotsFired { get; private set; }
        public void Initialize(EnemyController owner, GridManager battlefield, GameStateController state)
        { enemy = owner; grid = battlefield; game = state; target = null; acquire = 0; reload = 1.5f; pose = 0; ShotsFired = 0; effects = FindFirstObjectByType<ShorelineDefense.Presentation.VfxService>(); }
        public bool Tick(float dt, DefenseBase obstruction = null)
        {
            if (dt <= 0 || enemy == null || !enemy.IsCombatActive) return false;
            pose = Mathf.Max(0, pose - dt); reload -= dt; acquire -= dt;
            float range = rangeCells * grid.Grid.CellSize;
            if (obstruction != null) target = obstruction;
            else if (acquire <= 0) { acquire = acquisitionInterval; target = Acquire(range); }
            bool inRange = target != null && target.IsAlive && target.gameObject.activeInHierarchy &&
                ((Vector2)(target.ClosestFootprintPoint(transform.position) - transform.position)).sqrMagnitude <= range * range;
            if (inRange) enemy.Movement.FaceTowards(target.transform.position);
            if (reload <= 0 && inRange)
            {
                reload = cooldown; pose = .45f; ShotsFired++;
                enemy.Movement.FaceTowards(target.transform.position); enemy.NotifyAttack();
                CorrosiveSpit.Launch(transform.position, target.transform.position, grid.Grid, game, glob, damage, flightSeconds, effects);
            }
            // A ranged organism holds its firing position through reload rather than
            // walking into short-range fire between shots. Losing the target resumes the route.
            return pose > 0 || inRange;
        }
        private DefenseBase Acquire(float range)
        {
            if (!grid.Grid.TryWorldToCell(transform.position, out var center)) return null;
            DefenseBase best = null; float distance = range * range;
            int radius = Mathf.CeilToInt(rangeCells);
            for (int x = Mathf.Max(0, center.x - radius); x <= Mathf.Min(grid.Grid.Width - 1, center.x + radius); x++)
            for (int y = Mathf.Max(0, center.y - radius); y <= Mathf.Min(grid.Grid.Height - 1, center.y + radius); y++)
            {
                var defense = grid.Grid.OccupantAt(new Vector2Int(x, y)) as DefenseBase;
                if (defense == null || !defense.IsAlive || !defense.gameObject.activeInHierarchy ||
                    defense.Definition.Id == "land-mine" || defense.Definition.Id == "barbed-wire") continue;
                float candidate = ((Vector2)(defense.transform.position - transform.position)).sqrMagnitude;
                if (candidate >= distance) continue;
                distance = candidate; best = defense;
            }
            return best;
        }
    }
}
