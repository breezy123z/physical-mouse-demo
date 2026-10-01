using UnityEngine;

namespace PhysicalMouseDemo
{
    public sealed class DemoFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerInputController input;
        [SerializeField] private GrabInteractor grab;
        [SerializeField] private CameraLookController look;
        [SerializeField] private ConstrainedInteractable lever, stick;
        [SerializeField] private Renderer handRenderer;
        private MaterialPropertyBlock block;
        private Renderer[] handParts;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        public void Configure(PlayerInputController i, GrabInteractor g, CameraLookController l, ConstrainedInteractable le, ConstrainedInteractable st, Renderer renderer)
        { input = i;grab = g;look = l;lever = le;stick = st;handRenderer = renderer; }
        private void Awake() { block = new MaterialPropertyBlock();handParts=grab ? grab.GetComponentsInChildren<Renderer>():new Renderer[0]; }
        private void Update()
        {
            if (!handRenderer || !grab) return;
            Color color = grab.IsHolding ? new Color(0.2f, 1f, 0.5f) : grab.Candidate ? new Color(1f, 0.7f, 0.15f) : new Color(0.25f, 0.8f, 1f);
            foreach(var part in handParts) { part.GetPropertyBlock(block);block.SetColor(BaseColor,color);part.SetPropertyBlock(block); }
        }
        private void OnGUI()
        {
            if (!input || !grab) return;
            GUILayout.BeginArea(new Rect(16, 16, 440, 240), GUI.skin.box);
            GUILayout.Label("PHYSICAL MOUSE  |  interaction lab");
            GUILayout.Label("Mouse: hand   |   Hold " + input.DepthBinding + ": depth");
            GUILayout.Label(input.LookBinding + ": hold to look   |   " + input.GrabBinding + ": hold to grab");
            GUILayout.Label("Esc: free cursor   |   Look button: recapture");
            GUILayout.Label(grab.IsHolding ? "HOLDING: " + grab.Held.DisplayName : grab.Candidate ? "READY: " + grab.Candidate.DisplayName : "Move the hand near an object.");
            if (look) GUILayout.Label("Camera momentum: " + look.AngularVelocity.magnitude.ToString("F1") + " deg/s");
            if (lever) GUILayout.Label("Lever: " + lever.CurrentInput.x.ToString("F2"));
            if (stick) GUILayout.Label("Flight stick: " + stick.CurrentInput.ToString("F2"));
            if (input.PointHeld || input.SelectionHeld) GUILayout.Label("Reserved mode binding detected; no teleport/UI system yet.");
            if (!input.Captured) GUILayout.Label("INPUT RELEASED — press " + input.LookBinding + " over Game view.");
            GUILayout.EndArea();
        }
    }
}
