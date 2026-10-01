using UnityEngine;

namespace PhysicalMouseDemo
{
    // Requests camera velocity; CameraLookController retains responsibility for rotation and pitch limits.
    public sealed class EdgeTurnController : MonoBehaviour
    {
        [SerializeField] private PlayerInputController input;
        [SerializeField] private PhysicalHandController hand;
        [SerializeField] private Camera view;
        [SerializeField] private bool modeEnabled = true;
        [Tooltip("Distance inward from each edge, as a percentage of screen width/height. 20 means the outer 20 percent activates turning.")]
        [SerializeField, Range(1f,45f)] private float edgeMarginPercent=20f;
        [SerializeField, Min(0)] private float maximumTurnSpeed=75f;
        [Tooltip("Enable vertical edge turning as well as left/right turning.")]
        [SerializeField] private bool verticalTurning=true;
        public bool ModeEnabled { get => modeEnabled; set => modeEnabled=value; }
        public void Configure(PlayerInputController source, PhysicalHandController controller, Camera camera)
        { input=source;hand=controller;view=camera; }
        private void Update() { if(input && input.EdgeTogglePressed)modeEnabled=!modeEnabled; }
        public Vector2 GetRequestedVelocity(bool manuallyLooking)
        {
            // Anchored switches keep their own frame. Do not auto-pan while operating them.
            if(!modeEnabled || manuallyLooking || !input || !input.Captured || !hand || !view || hand.UsesObjectFrame)return Vector2.zero;
            return EvaluateViewport(view.WorldToViewportPoint(hand.Body.position));
        }
        public Vector2 EvaluateViewport(Vector3 point)
        {
            if(!modeEnabled || point.z<=0)return Vector2.zero;
            float margin=Mathf.Clamp(edgeMarginPercent*.01f,.01f,.45f);
            return new Vector2(EdgeAmount(point.x,margin),verticalTurning ? EdgeAmount(point.y,margin):0)*maximumTurnSpeed;
        }
        private static float EdgeAmount(float value,float margin)
        {
            if(value<margin)return -Mathf.SmoothStep(0,1,Mathf.Clamp01((margin-value)/margin));
            if(value>1-margin)return Mathf.SmoothStep(0,1,Mathf.Clamp01((value-(1-margin))/margin));
            return 0;
        }
    }
}
