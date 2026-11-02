using System;
using UnityEngine;

namespace ShorelineDefense.Grid
{
    public enum StaticObstacleCategory { Container, Crane, Building, Fortification, Equipment, Vehicle }

    [Serializable]
    public sealed class StaticObstacleFootprint
    {
        public string id;
        public StaticObstacleCategory category;
        public Vector2Int anchor;
        public Vector2Int size = Vector2Int.one;
        public bool Contains(Vector2Int cell) => cell.x >= anchor.x && cell.y >= anchor.y &&
            cell.x < anchor.x + size.x && cell.y < anchor.y + size.y;
    }
}
