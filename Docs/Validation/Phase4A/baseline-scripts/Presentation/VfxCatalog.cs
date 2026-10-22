using System;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    public enum VfxId
    {
        MgMuzzle, SniperMuzzle, MgTracer, SniperTracer, ImpactDirt, ImpactMud,
        ImpactConcrete, ImpactMetal, ImpactFlesh, BloodHit, BloodDecal, BloodDecalMedium,
        MineExplosion, ExplosionSmall, SmokeSmall, SmokeMedium, SplashSmall,
        SplashMedium, SplashLarge, WaterWake, WaterRipple, ShoreFoam
    }
    public enum VfxChannel { Burst, Tracer, Smoke, Decal, Corpse }

    [Serializable]
    public sealed class VfxSequence
    {
        public VfxId id;
        public Sprite[] frames;
        public float fps = 16;
        public VfxChannel channel;
        public string sortingLayer = "Effects";
        public int sortingOrder = 5;
        public float lifetime;
        [Range(0, 1)] public float opacity = 1;
        [Range(0, 1)] public float fadePortion = .3f;
        public Vector2 drift;
    }

    [CreateAssetMenu(menuName = "Shoreline Defense/VFX Catalog")]
    public sealed class VfxCatalog : ScriptableObject
    {
        public VfxSequence[] sequences = Array.Empty<VfxSequence>();
        public VfxSequence Get(VfxId id)
        {
            for (int i = 0; i < sequences.Length; i++)
                if (sequences[i].id == id) return sequences[i];
            return null;
        }
    }
}
