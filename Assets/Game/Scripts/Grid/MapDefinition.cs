using System;
using UnityEngine;

namespace ShorelineDefense.Grid
{
    [CreateAssetMenu(menuName = "Shoreline Defense/Map Definition")]
    public sealed class MapDefinition : ScriptableObject
    {
        public string id, displayName, sceneName;
        [TextArea] public string description;
        public string gameplayStyle;
        public Sprite preview;
        public BattlefieldDefinition battlefield;
    }

}
