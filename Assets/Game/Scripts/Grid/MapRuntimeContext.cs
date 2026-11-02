using System;
using System.Text;
using ShorelineDefense.Core;
using ShorelineDefense.Waves;
using UnityEngine;

namespace ShorelineDefense.Grid
{
    /// <summary>Scene identity and development validation; grid/nav remain GridManager-owned.</summary>
    [DefaultExecutionOrder(-100)]
    public sealed class MapRuntimeContext : MonoBehaviour
    {
        public MapDefinition definition;
        public GridManager grid;
        public BoatLandingSystem boats;
        public BorderBreachService border;
        public string MapId => definition != null ? definition.id : MapCatalog.OriginalId;
        public string DisplayName => definition != null ? definition.displayName : "DANUBE BORDER";
        private void Start()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            string staticReport = ValidateStaticObstacles(out int staticInvalid);
            if (staticInvalid > 0) Debug.LogWarning(staticReport, this);
#endif
            string report = Validate(out int invalid);
            if (invalid > 0) { Debug.LogError(report, this); FindFirstObjectByType<GameStateController>()?.EndRun(); }
        }
        public string ValidateStaticObstacles(out int invalid)
        {
            invalid = 0;
            if (definition?.battlefield == null || grid == null || boats == null || border == null)
            { invalid = 1; return "Missing static map validation configuration."; }
            // A fresh grid excludes player construction, including restored preparation saves.
            var terrain = definition.battlefield.CreateGrid(grid.transform.position);
            var paths = new PathfindingService(terrain);
            var route = new System.Collections.Generic.List<Vector2Int>();
            int blocked = 0, containers = 0, cranes = 0, buildings = 0;
            for (int x = 0; x < terrain.Width; x++) for (int y = 0; y < terrain.Height; y++)
                if (terrain.GetCell(new Vector2Int(x,y)).Terrain == TerrainType.Blocked) blocked++;
            var ids = new System.Collections.Generic.HashSet<string>();
            var text = new StringBuilder();
            foreach (var obstacle in definition.battlefield.navigationObstacles)
            {
                if (obstacle == null || string.IsNullOrEmpty(obstacle.id) || !ids.Add(obstacle.id) || !terrain.ContainsFootprint(obstacle.anchor, obstacle.size))
                { invalid++; text.Append("Zero/invalid/duplicate blocker footprint; "); continue; }
                if (obstacle.category == StaticObstacleCategory.Container) containers++;
                if (obstacle.category == StaticObstacleCategory.Crane) cranes++;
                if (obstacle.category == StaticObstacleCategory.Building) buildings++;
            }
            var markers = FindObjectsByType<StaticNavigationObstacle>(FindObjectsSortMode.None);
            foreach (string id in ids)
                if (!System.Linq.Enumerable.Any(markers, m => m.obstacleId == id && m.GetComponent<BoxCollider2D>() != null))
                { invalid++; text.Append("Missing collider binding: " + id + "; "); }
            foreach (var marker in markers)
                if (!ids.Contains(marker.obstacleId)) { invalid++; text.Append("Known blocker has zero footprint: " + marker.name + "; "); }
            foreach (var p in boats.LandingPoints)
            {
                if (p?.unloadingPositions == null) { invalid++; continue; }
                foreach (var position in p.unloadingPositions)
                {
                    bool found = false;
                    if (terrain.TryWorldToCell(position, out var start))
                        foreach (var goal in border.BreachCells) found |= paths.TryFindPath(start, goal, route);
                    if (!found) { invalid++; text.Append("Static-disconnected landing " + position + "; "); }
                }
            }
            return $"{MapId}: static blocked cells={blocked}, container footprints={containers}, crane footprints={cranes}, building footprints={buildings}, invalid={invalid}. " + text;
        }
        public string Validate(out int invalid)
        {
            invalid = 0;
            if (definition == null || grid?.Grid == null || boats == null || border == null || border.BreachCells.Count == 0)
            { invalid = 1; return "Map validation: missing definition/grid/boats/border."; }
            var text = new StringBuilder(); int buildable = 0, blocked = 0;
            for (int x = 0; x < grid.Grid.Width; x++) for (int y = 0; y < grid.Grid.Height; y++)
            { var c = grid.Grid.GetCell(new Vector2Int(x,y)); if (c.Buildable) buildable++; if (!c.Walkable) blocked++; }
            foreach (var p in boats.LandingPoints)
            {
                if (!boats.IsValidApproach(p)) { invalid++; text.Append("Invalid boat approach; "); }
                if (p == null || p.unloadingPositions == null || p.unloadingPositions.Length == 0) { invalid++; continue; }
                foreach (var position in p.unloadingPositions)
                    if (!grid.Grid.TryWorldToCell(position, out var cell) || !border.TryGetDestination(cell, out _))
                    { invalid++; text.Append("Unreachable landing " + position + "; "); }
            }
            foreach (var carrier in boats.Carriers)
                if (carrier == null || carrier.visualPrefab == null || !System.Linq.Enumerable.Any(boats.LandingPoints, p => boats.IsValidApproach(p, carrier)))
                { invalid++; text.Append("Missing carrier or no water-safe approach; "); }
            return $"Map {MapId}: {boats.LandingPoints.Count} landings, {border.BreachCells.Count} exits, {buildable} buildable, {blocked} blocked, {invalid} invalid. " + text;
        }
    }
}
