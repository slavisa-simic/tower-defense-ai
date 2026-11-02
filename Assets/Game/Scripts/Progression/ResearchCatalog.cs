using System;
using UnityEngine;
namespace ShorelineDefense.Progression
{
    [CreateAssetMenu(menuName = "Shoreline Defense/Research Catalog")]
    public sealed class ResearchCatalog : ScriptableObject
    {
        public ResearchNodeDefinition[] nodes = Array.Empty<ResearchNodeDefinition>();
        public ResearchRewardSettings rewards;
        public Sprite researchPointIcon;
        public ResearchNodeDefinition Find(string id) => Array.Find(nodes, n => n != null && n.id == id);
        public static ResearchCatalog Load() => Resources.Load<ResearchCatalog>("ResearchCatalog");
    }
}
