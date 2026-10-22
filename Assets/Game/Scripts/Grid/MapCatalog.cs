using System;
using UnityEngine;
namespace ShorelineDefense.Grid
{
    [CreateAssetMenu(menuName = "Shoreline Defense/Map Catalog")]
    public sealed class MapCatalog : ScriptableObject
    {
        public const string OriginalId = "river_border";
        public MapDefinition[] maps = Array.Empty<MapDefinition>();
        public MapDefinition Find(string id) => Array.Find(maps, m => m != null && m.id == Normalize(id));
        public static string Normalize(string id) => string.IsNullOrEmpty(id) ? OriginalId : id;
    }
}
