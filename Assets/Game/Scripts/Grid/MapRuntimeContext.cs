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
            string report = Validate(out int invalid);
            if (invalid > 0) { Debug.LogError(report, this); FindFirstObjectByType<GameStateController>()?.EndRun(); }
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
            return $"Map {MapId}: {boats.LandingPoints.Count} landings, {border.BreachCells.Count} exits, {buildable} buildable, {blocked} blocked, {invalid} invalid. " + text;
        }
    }
}
