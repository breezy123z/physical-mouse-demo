using UnityEngine;

namespace PhysicalMouseDemo
{
    public sealed class ConstrainedInteractable : MonoBehaviour
    {
        public enum ControlType { Lever, FlightStick }
        [SerializeField] private ControlType controlType;
        [SerializeField, Min(1f)] private float rangeDegrees = 25f;
        [Header("Live readout")]
        [SerializeField] private Vector2 currentInput;
        [SerializeField] private Vector2 currentAngles;
        private Quaternion initialRotation;
        private HingeJoint hinge;
        private Grabbable grabbable;
        private ConfigurableJoint stickJoint;
        private Quaternion heldRotation;
        private float heldAngle;
        [Header("Hold the last position when released")]
        [SerializeField, Min(0)] private float holdingSpring=160f;
        [SerializeField, Min(0)] private float holdingDamping=24f;
        [SerializeField, Min(0)] private float holdingForce=250f;
        private void FixedUpdate()
        {
            bool moving=grabbable && grabbable.IsHeld;
            if(moving) { heldAngle=hinge ? hinge.angle:0;heldRotation=transform.localRotation; }
            if(hinge)
            {
                hinge.useSpring=!moving;
                hinge.spring=new JointSpring { spring=holdingSpring,damper=holdingDamping,targetPosition=heldAngle };
            }
            if(stickJoint)
            {
                stickJoint.targetRotation=Quaternion.Inverse(heldRotation)*initialRotation;
                var drive=new JointDrive { positionSpring=moving ? 0:holdingSpring,positionDamper=moving ? 2f:holdingDamping,maximumForce=holdingForce };
                stickJoint.angularXDrive=stickJoint.angularYZDrive=drive;
            }
        }
        public Vector2 CurrentInput => currentInput;
        public Vector2 CurrentAngles => currentAngles;
        public void Configure(ControlType type, float range) { controlType = type;rangeDegrees = range; }
        private void Awake() { initialRotation = transform.localRotation;hinge = GetComponent<HingeJoint>();grabbable=GetComponent<Grabbable>();stickJoint=GetComponent<ConfigurableJoint>();heldRotation=initialRotation; }
        private void Update()
        {
            if (controlType == ControlType.Lever && hinge) currentAngles = new Vector2(hinge.angle, 0);
            else
            {
                Vector3 e = (Quaternion.Inverse(initialRotation) * transform.localRotation).eulerAngles;
                currentAngles = new Vector2(Mathf.DeltaAngle(0, e.z), -Mathf.DeltaAngle(0, e.x));
            }
            currentInput = new Vector2(Mathf.Clamp(currentAngles.x / rangeDegrees, -1, 1), Mathf.Clamp(currentAngles.y / rangeDegrees, -1, 1));
        }
    }
}

