using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using UnityEngine;
namespace ShorelineDefense.Presentation
{
    /// <summary>Clocked by the existing arrival owner; animation never releases cargo.</summary>
    public sealed class AquaticCarrierPresentation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private Sprite[] floating, moving, arrival, release, retreat;
        private Sprite[] emergence;
        private ArrivalMethod method;
        private float clock;
        private float arrivalSeconds;
        private int lastStage = -1;
        public void Begin(CarrierDefinition definition, EnemyController passenger)
        {
            method = definition != null ? definition.method : ArrivalMethod.DriftPod;
            arrivalSeconds = definition != null ? definition.ruptureSeconds : .4f;
            emergence = passenger.GetComponent<EnemySpritePresentation>()?.EmergenceFrames;
            clock = 0; lastStage = -1;
            body.color = Color.white; body.enabled = true;
            body.transform.localScale = Vector3.one;
            if (method == ArrivalMethod.DirectEmergence)
            {
                var source = passenger.transform.Find("Visual/Body");
                if (source != null) { body.sprite = source.GetComponent<SpriteRenderer>().sprite; body.transform.localScale = source.lossyScale; }
                body.color = new Color(.15f, .4f, .39f, .65f);
            }
        }
        public void Tick(int stage, float dt)
        {
            if (stage != lastStage) { lastStage = stage; clock = 0; } else clock += dt;
            Sprite[] frames;
            if (method == ArrivalMethod.DirectEmergence)
            {
                frames = emergence;
                body.color = stage == 0 ? new Color(.12f, .35f, .36f, .65f) : Color.white;
                if (frames != null && frames.Length > 0) body.sprite = frames[stage == 0 ? 0 : Mathf.Min(frames.Length - 1, (int)(clock * 8))];
                body.enabled = stage < 2; return;
            }
            frames = stage == 0 ? moving : stage == 1 ? clock < arrivalSeconds ? arrival : release : retreat;
            if (frames == null || frames.Length == 0) frames = floating;
            float sequenceClock = stage == 1 && clock >= arrivalSeconds ? clock - arrivalSeconds : clock;
            if (frames != null && frames.Length > 0) body.sprite = frames[stage == 0 ? (int)(clock * 6) % frames.Length : Mathf.Min(frames.Length - 1, (int)(sequenceClock * 8))];
            body.color = stage == 3 ? new Color(.4f, .65f, .61f, Mathf.Clamp01(1 - clock * .7f)) : Color.white;
        }
    }
}
