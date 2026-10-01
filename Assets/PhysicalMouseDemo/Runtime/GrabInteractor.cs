using UnityEngine;

namespace PhysicalMouseDemo
{
    [RequireComponent(typeof(PhysicalHandController))]
    public sealed class GrabInteractor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputController input;
        [SerializeField] private PhysicalHandController hand;
        [Header("Detection")]
        [SerializeField, Min(0.01f)] private float grabRadius = 0.2f;
        [SerializeField] private LayerMask grabbableLayers = ~0;
        [Header("Compliant joint")]
        [SerializeField, Min(0f)] private float grabSpring = 1400f;
        [SerializeField, Min(0f)] private float grabDamping = 75f;
        [SerializeField, Min(1f)] private float maximumGrabForce = 180f;
        [SerializeField, Min(0.1f)] private float maximumStretch = 0.9f;
        private readonly Collider[] nearby = new Collider[64];
        private ConfigurableJoint grabJoint;
        private Vector3 candidatePoint;
        public Grabbable Candidate { get; private set; }
        public Grabbable Held { get; private set; }
        public bool IsHolding => Held && grabJoint;
        public void Configure(PlayerInputController source, PhysicalHandController controller) { input = source; hand = controller; }
        private void Awake() { if (!hand) hand = GetComponent<PhysicalHandController>(); }
        private void Update()
        {
            if (Held && !grabJoint) Release();
            if (!input || !input.enabled) return;
            if (IsHolding && !input.GrabHeld) Release();
            if (!IsHolding)
            {
                FindCandidate();
                if (input.GrabPressed) TryGrab();
            }
        }
        private void FixedUpdate()
        {
            if (IsHolding && Vector3.Distance(grabJoint.transform.TransformPoint(grabJoint.anchor), hand.Body.transform.TransformPoint(grabJoint.connectedAnchor)) > maximumStretch) Release();
        }
        public void FindCandidate()
        {
            Candidate = null; float best = grabRadius * grabRadius;
            int count = Physics.OverlapSphereNonAlloc(transform.position, grabRadius, nearby, grabbableLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var rb = nearby[i].attachedRigidbody;
                if (!rb || rb == hand.Body || rb.isKinematic) continue;
                var item = rb.GetComponent<Grabbable>();
                if (!item) continue;
                Vector3 point = nearby[i].ClosestPoint(transform.position);
                float distance = (point - transform.position).sqrMagnitude;
                if (distance > best) continue;
                best = distance; Candidate = item; candidatePoint = point;
            }
        }
        public bool TryGrab()
        {
            if (IsHolding) return false;
            FindCandidate();
            if (!Candidate) return false;
            Held = Candidate;Held.IsHeld=true;
            // Drives pull two anchors together with finite force. The object remains dynamic.
            grabJoint = Held.Body.gameObject.AddComponent<ConfigurableJoint>();
            grabJoint.connectedBody = hand.Body;
            grabJoint.autoConfigureConnectedAnchor = false;
            grabJoint.anchor = Held.transform.InverseTransformPoint(candidatePoint);
            grabJoint.connectedAnchor = hand.transform.InverseTransformPoint(candidatePoint);
            grabJoint.xMotion = grabJoint.yMotion = grabJoint.zMotion = ConfigurableJointMotion.Free;
            grabJoint.angularXMotion = grabJoint.angularYMotion = grabJoint.angularZMotion = ConfigurableJointMotion.Free;
            var drive = new JointDrive { positionSpring = grabSpring, positionDamper = grabDamping, maximumForce = maximumGrabForce * Held.GripStrength };
            grabJoint.xDrive = grabJoint.yDrive = grabJoint.zDrive = drive;
            grabJoint.enableCollision = false;
            Held.Body.WakeUp();hand.BeginObjectMovement(Held);Candidate = null;
            return true;
        }
        public void Release()
        {
            if (grabJoint)
            {
                // Clear drives immediately; Destroy is deferred until the end of this frame.
                grabJoint.xDrive = grabJoint.yDrive = grabJoint.zDrive = new JointDrive();
                Destroy(grabJoint);
            }
            if (Held) { Held.IsHeld=false;if(hand) hand.EndObjectMovement(); }
            grabJoint = null;Held = null;
            // Deliberately preserve Rigidbody velocities: moving releases can become throws.
        }
        private void OnDisable() { Release(); }
        private void OnDrawGizmosSelected() { Gizmos.color = Color.cyan;Gizmos.DrawWireSphere(transform.position, grabRadius); }
    }
}
