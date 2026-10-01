using UnityEngine;

namespace PhysicalMouseDemo
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PhysicalHandController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputController input;
        [SerializeField] private Transform cameraPivot;
        [Header("Reach envelope")]
        [SerializeField, Min(0.0001f)] private float movementSensitivity = 0.0025f;
        [SerializeField, Min(0.01f)] private float smoothing = 0.055f;
        [SerializeField, Min(0.05f)] private float maximumHorizontalReach = 1.05f;
        [SerializeField, Min(0.05f)] private float maximumVerticalReach = 0.7f;
        [SerializeField, Min(0.1f)] private float distanceFromCamera = 2f;
        [SerializeField] private Vector2 depthLimits = new Vector2(1.0f, 2.8f);
        [SerializeField, Min(0.0001f)] private float scrollSensitivity = 0.001f;
        [SerializeField] private Vector2 reachOffset = new Vector2(0f, -0.38f);
        [Header("Physical following (forces, not transform teleporting)")]
        [SerializeField, Min(1f)] private float positionSpring = 650f;
        [SerializeField, Min(0f)] private float positionDamping = 42f;
        [SerializeField, Min(1f)] private float maximumForce = 180f;
        private Rigidbody body;
        private Vector3 target, smoothingVelocity;
        private bool initialized;
        private Grabbable movementObject;
        private Vector3 localGrabOrigin, localGrabTarget;
        [Tooltip("Metres of depth per vertical mouse pixel while Depth Drag is held.")]
        [SerializeField] private float depthDragSensitivity = .0025f;
        public float Depth => distanceFromCamera;
        public void BeginObjectMovement(Grabbable item)
        {
            movementObject=item;
            if(item && item.MovementFrame) localGrabOrigin=localGrabTarget=item.MovementFrame.InverseTransformPoint(target);
        }
        public void EndObjectMovement() { movementObject=null;AimAtWorldPoint(target); }
        public void MovePointer(Vector2 pixels, bool depthHeld)
        {
            if(movementObject && movementObject.MovementFrame)
            {
                localGrabTarget += (movementObject.MouseXAxis * pixels.x + movementObject.MouseYAxis * pixels.y) * movementSensitivity;
                localGrabTarget=localGrabOrigin+Vector3.ClampMagnitude(localGrabTarget-localGrabOrigin,movementObject.MovementRadius);
            }
            else if(depthHeld)
            {
                AddPointerDelta(new Vector2(pixels.x,0));
                distanceFromCamera=Mathf.Clamp(distanceFromCamera+pixels.y*depthDragSensitivity,depthLimits.x,depthLimits.y);
            }
            else AddPointerDelta(pixels);
        }
        public Rigidbody Body => body;
        public Vector3 Target => target;
        public Vector2 ReachOffset => reachOffset;
        public void Configure(PlayerInputController source, Transform pivot) { input = source; cameraPivot = pivot; }
        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            // A Rigidbody must not inherit the rotating camera's transform. Keep only the logical reference.
            transform.SetParent(null, true);
            target = body.position; initialized = true;
        }
        private void Update()
        {
            if (!input || !cameraPivot) return;
            if (!input.LookHeld) MovePointer(input.PointerDelta,input.DepthHeld);
            distanceFromCamera = Mathf.Clamp(distanceFromCamera + input.DepthDelta * scrollSensitivity, depthLimits.x, depthLimits.y);
        }
        public void AddPointerDelta(Vector2 pixels)
        {
            reachOffset += pixels * movementSensitivity;
            reachOffset.x = Mathf.Clamp(reachOffset.x, -maximumHorizontalReach, maximumHorizontalReach);
            reachOffset.y = Mathf.Clamp(reachOffset.y, -maximumVerticalReach, maximumVerticalReach);
        }
        // Also useful later when changing interaction modes or recentering the hand.
        public void AimAtWorldPoint(Vector3 position)
        {
            if (!cameraPivot) return;
            if(movementObject && movementObject.MovementFrame) localGrabTarget=movementObject.MovementFrame.InverseTransformPoint(position);
            Vector3 local = cameraPivot.InverseTransformPoint(position);
            reachOffset = new Vector2(Mathf.Clamp(local.x, -maximumHorizontalReach, maximumHorizontalReach), Mathf.Clamp(local.y, -maximumVerticalReach, maximumVerticalReach));
            distanceFromCamera = Mathf.Clamp(local.z, depthLimits.x, depthLimits.y);
        }
        private void FixedUpdate()
        {
            if (!initialized || !cameraPivot) return;
            Vector3 desired = cameraPivot.TransformPoint(new Vector3(reachOffset.x, reachOffset.y, distanceFromCamera));
            if(movementObject && movementObject.MovementFrame) desired=movementObject.MovementFrame.TransformPoint(localGrabTarget);
            target = Vector3.SmoothDamp(target, desired, ref smoothingVelocity, smoothing, Mathf.Infinity, Time.fixedDeltaTime);
            Vector3 force = (target - body.position) * positionSpring - body.linearVelocity * positionDamping;
            body.AddForce(Vector3.ClampMagnitude(force, maximumForce), ForceMode.Force);
        }
    }
}
