using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Presentation
{
    /// <summary>Fits the XY battlefield at any aspect ratio, keeping the camera perpendicular to it.</summary>
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    public sealed class TopDownCamera : MonoBehaviour
    {
        [SerializeField] private GridManager battlefield;
        [SerializeField, Min(0)] private float padding = 0.6f;
        private Camera view;
        private float lastAspect = -1;

        public void Configure(GridManager grid) { battlefield = grid; FrameBattlefield(); }
        private void OnEnable() => FrameBattlefield();
        private void OnValidate() => lastAspect = -1;
        private void LateUpdate()
        {
            if (view == null) view = GetComponent<Camera>();
            if (!Mathf.Approximately(view.aspect, lastAspect) || !Application.isPlaying) FrameBattlefield();
        }

        public void FrameBattlefield()
        {
            if (battlefield == null || battlefield.Definition == null) return;
            if (view == null) view = GetComponent<Camera>();
            var data = battlefield.Definition;
            if (data.Plane != GameplayPlane.XY) return;
            float width = data.Width * data.CellSize;
            float height = data.Height * data.CellSize;
            view.orthographic = true;
            view.orthographicSize = Mathf.Max(height * 0.5f + padding,
                (width * 0.5f + padding) / Mathf.Max(0.01f, view.aspect));
            transform.SetPositionAndRotation(battlefield.transform.position + new Vector3(width * 0.5f, height * 0.5f, -10), Quaternion.identity);
            lastAspect = view.aspect;
        }
    }
}
