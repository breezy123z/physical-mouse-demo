using UnityEngine;

namespace PhysicalMouseDemo
{
    public sealed class CameraLookController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputController input;
        [SerializeField] private Transform cameraPivot;
        [Header("Inertial look")]
        [Tooltip("Degrees turned per mouse pixel before inertial filtering.")]
        [SerializeField, Min(0.001f)] private float sensitivity = 0.12f;
        [Tooltip("How quickly angular velocity approaches the requested mouse velocity.")]
        [SerializeField, Min(0.1f)] private float acceleration = 18f;
        [Tooltip("Angular velocity decay per second after releasing Look. Lower = longer coasting.")]
        [SerializeField, Min(0.1f)] private float damping = 5f;
        [SerializeField] private float minimumPitch = -55f;
        [SerializeField] private float maximumPitch = 60f;
        [SerializeField, Min(1f)] private float maximumAngularSpeed = 220f;
        [SerializeField] private Vector2 angularVelocity;
        [SerializeField] private float pitch;
        private float yaw;
        private Quaternion initialRotation;
        public Vector2 AngularVelocity => angularVelocity;
        public float Pitch => pitch;
        public float Yaw => yaw;
        public void Configure(PlayerInputController source, Transform pivot) { input = source; cameraPivot = pivot; }
        private void Awake() { initialRotation = cameraPivot ? cameraPivot.localRotation : Quaternion.identity; }
        private void Update() { if (input && cameraPivot) Step(input.PointerDelta, input.LookHeld, Time.deltaTime); }

        // Mouse delta is already integrated over one frame. Convert it to deg/sec before filtering.
        public void Step(Vector2 mousePixels, bool looking, float dt)
        {
            if (!cameraPivot || dt <= 0f) return;
            if (looking)
            {
                var requested = Vector2.ClampMagnitude(mousePixels * sensitivity / dt, maximumAngularSpeed);
                angularVelocity = Vector2.Lerp(angularVelocity, requested, 1f - Mathf.Exp(-acceleration * dt));
            }
            else angularVelocity *= Mathf.Exp(-damping * dt);
            yaw = Mathf.Repeat(yaw + angularVelocity.x * dt + 180f, 360f) - 180f;
            float nextPitch = pitch - angularVelocity.y * dt;
            pitch = Mathf.Clamp(nextPitch, minimumPitch, maximumPitch);
            if (!Mathf.Approximately(pitch, nextPitch)) angularVelocity.y = 0f; // Do not store momentum into a pitch stop.
            cameraPivot.localRotation = initialRotation * Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}
