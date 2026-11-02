using UnityEngine;

namespace ShorelineDefense.Grid
{
    /// <summary>Scene/collider binding to an authored permanent footprint. Never an attack target.</summary>
    public sealed class StaticNavigationObstacle : MonoBehaviour
    {
        public string obstacleId;
    }
}
