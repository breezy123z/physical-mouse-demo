using UnityEngine;

namespace PhysicalMouseDemo
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Grabbable : MonoBehaviour
    {
        [SerializeField] private string displayName = "Object";
        [Tooltip("Multiplier on the hand's grab drive. Mass stays on the Rigidbody.")]
        [SerializeField, Range(0.1f, 2f)] private float gripStrength = 1f;
        [Header("Object-relative mouse movement")]
        [Tooltip("Use a stable base transform, not the moving handle. Leave empty for camera-relative movement.")]
        [SerializeField] private Transform movementFrame;
        [SerializeField] private Vector3 mouseXAxis = Vector3.right;
        [SerializeField] private Vector3 mouseYAxis = Vector3.forward;
        [SerializeField, Min(.05f)] private float movementRadius = .55f;
        public Transform MovementFrame => movementFrame;
        public Vector3 MouseXAxis => mouseXAxis;
        public Vector3 MouseYAxis => mouseYAxis;
        public float MovementRadius => movementRadius;
        public void ConfigureMovement(Transform frame, Vector3 x, Vector3 y) { movementFrame=frame;mouseXAxis=x;mouseYAxis=y; }
        public bool IsHeld { get; set; }
        public Rigidbody Body { get; private set; }
        public string DisplayName => displayName;
        public float GripStrength => gripStrength;
        public void SetName(string label) { displayName = label; }
        private void Awake() { Body = GetComponent<Rigidbody>(); }
    }
}
