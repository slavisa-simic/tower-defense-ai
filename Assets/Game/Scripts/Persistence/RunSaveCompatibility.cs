using System;
using System.Collections.Generic;
using ShorelineDefense.Defenses;
using ShorelineDefense.Grid;
using ShorelineDefense.Waves;
using UnityEngine;

namespace ShorelineDefense.Persistence
{
    /// <summary>Catalog/map preflight also runs in Main Menu, allowing backup recovery before scene load.</summary>
    public static class RunSaveCompatibility
    {
        public static bool Valid(RunSaveData data, IReadOnlyList<DefenseDefinition> defenses, BattlefieldDefinition map, EndlessEventSettings events)
        {
            if (!SaveGameService.Valid(data) || defenses == null || map == null) return false;
            var cells = new HashSet<Vector2Int>();
            foreach (var item in data.structures)
            {
                DefenseDefinition definition = null;
                foreach (var candidate in defenses) if (candidate != null && candidate.Id == item.defenseId) { definition = candidate; break; }
                if (definition == null || !definition.IsValid || Mathf.RoundToInt(definition.MaxHealth * (definition.LevelData(item.level)?.health ?? 1)) != item.maxHealth) return false;
                if (item.x < 0 || item.y < 0 || (long)item.x + definition.Footprint.x > map.Width || (long)item.y + definition.Footprint.y > map.Height) return false;
                for (int x = 0; x < definition.Footprint.x; x++) for (int y = 0; y < definition.Footprint.y; y++)
                { var cell = new Vector2Int(item.x + x, item.y + y); if (map.TerrainAt(cell) != TerrainType.Ground || Array.IndexOf(map.noBuildCells, cell) >= 0 || !cells.Add(cell)) return false; }
            }
            bool Known(string id) => events != null && Array.Exists(events.events, e => e != null && e.id == id);
            var state = data.events;
            if (!string.IsNullOrEmpty(state.currentId) && (!Known(state.currentId) || state.wave != data.wave || state.wavesRemaining <= 0)) return false;
            foreach (var list in new[] { state.cooldowns, state.counts, state.history }) foreach (var item in list) if (!Known(item.id)) return false;
            foreach (string id in state.recent) if (!Known(id)) return false;
            return true;
        }
    }
}
