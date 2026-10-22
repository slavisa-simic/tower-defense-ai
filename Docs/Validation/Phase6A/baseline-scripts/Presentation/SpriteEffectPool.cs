using UnityEngine;

namespace ShorelineDefense.Presentation
{
    /// <summary>Fixed storage; overload recycles the oldest visual, never gameplay objects.</summary>
    public sealed class SpriteEffectPool
    {
        private sealed class Slot
        {
            public SpriteRenderer renderer;
            public Sprite[] frames;
            public float fps, age, lifetime, opacity, fade;
            public Vector3 origin, drift;
            public Transform follow;
            public bool active;
            public long serial;
        }
        private readonly Slot[] slots;
        private long serial;
        public int Capacity => slots.Length;
        public int PlaybackLimit { get; set; } = int.MaxValue;
        public int ActiveCount { get; private set; }
        public int RecycledCount { get; private set; }
        public int PeakCount { get; private set; }

        public SpriteEffectPool(Transform parent, string name, int capacity, Material material)
        {
            slots = new Slot[Mathf.Max(1, capacity)];
            for (int i = 0; i < slots.Length; i++)
            {
                var obj = new GameObject(name + " " + i);
                obj.transform.SetParent(parent, false);
                var renderer = obj.AddComponent<SpriteRenderer>();
                if (material != null) renderer.sharedMaterial = material;
                renderer.enabled = false;
                slots[i] = new Slot { renderer = renderer };
            }
        }

        public SpriteRenderer Play(Sprite[] frames, float fps, Vector3 position, Quaternion rotation,
            Vector3 scale, string layer, int order, float lifetime, float opacity, float fade,
            Vector2 drift, Transform follow = null)
        {
            if (frames == null || frames.Length == 0 || frames[0] == null) return null;
            if (ActiveCount >= PlaybackLimit) return null;
            Slot slot = slots[0];
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].active) { slot = slots[i]; break; }
                if (slots[i].serial < slot.serial) slot = slots[i];
            }
            if (slot.active) RecycledCount++; else ActiveCount++;
            PeakCount = Mathf.Max(PeakCount, ActiveCount);
            slot.active = true; slot.serial = ++serial; slot.age = 0;
            slot.frames = frames; slot.fps = Mathf.Max(1, fps);
            slot.lifetime = lifetime > 0 ? lifetime : frames.Length / slot.fps;
            slot.opacity = Mathf.Clamp01(opacity); slot.fade = Mathf.Clamp01(fade);
            slot.origin = position; slot.drift = drift; slot.follow = follow;
            var renderer = slot.renderer;
            renderer.transform.SetPositionAndRotation(position, rotation);
            renderer.transform.localScale = scale;
            renderer.sortingLayerName = layer; renderer.sortingOrder = order;
            renderer.sprite = frames[0]; renderer.color = new Color(1, 1, 1, slot.opacity);
            renderer.enabled = true;
            return renderer;
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (!slot.active) continue;
                slot.age += Mathf.Max(0, deltaTime);
                if (slot.age >= slot.lifetime) { Release(slot); continue; }
                slot.renderer.sprite = slot.frames[Mathf.Min(slot.frames.Length - 1, (int)(slot.age * slot.fps))];
                float alpha = slot.fade <= 0 ? 1 : Mathf.Clamp01((slot.lifetime - slot.age) / (slot.lifetime * slot.fade));
                slot.renderer.color = new Color(1, 1, 1, slot.opacity * alpha);
                slot.renderer.transform.position = slot.follow != null ? slot.follow.position : slot.origin + slot.drift * slot.age;
            }
        }

        private void Release(Slot slot)
        {
            slot.active = false; slot.renderer.enabled = false; slot.renderer.sprite = null;
            slot.renderer.color = Color.white; slot.renderer.transform.localScale = Vector3.one;
            slot.renderer.transform.rotation = Quaternion.identity;
            slot.frames = null; slot.follow = null; slot.drift = Vector3.zero;
            ActiveCount--;
        }
        public void Clear()
        {
            for (int i = 0; i < slots.Length; i++) if (slots[i].active) Release(slots[i]);
        }
    }
}
