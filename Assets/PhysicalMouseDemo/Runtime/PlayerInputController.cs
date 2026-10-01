using UnityEngine;
using UnityEngine.InputSystem;

namespace PhysicalMouseDemo
{
    // Actions are local to this demo: no changes to the project's shared action asset.
    public sealed class PlayerInputController : MonoBehaviour
    {
        [Header("Rebind these actions in the Inspector")]
        [SerializeField] private InputAction pointer = new InputAction("Pointer", InputActionType.Value, "<Mouse>/delta");
        [SerializeField] private InputAction cameraLook = new InputAction("Camera Look", InputActionType.Button, "<Mouse>/leftButton");
        [SerializeField] private InputAction grab = new InputAction("Grab / Interact", InputActionType.Button, "<Keyboard>/e");
        [SerializeField] private InputAction point = new InputAction("Point / Teleport (reserved)", InputActionType.Button, "<Mouse>/middleButton");
        [SerializeField] private InputAction selection = new InputAction("Options / Selection (reserved)", InputActionType.Button, "<Keyboard>/tab");
        [SerializeField] private InputAction depth = new InputAction("Reach depth", InputActionType.Value, "<Mouse>/scroll/y");
        [SerializeField] private InputAction releaseCursor = new InputAction("Release cursor", InputActionType.Button, "<Keyboard>/escape");
        [SerializeField] private bool captureOnStart = true;

        [SerializeField] private InputAction depthDrag = new InputAction("Depth drag", InputActionType.Button, "<Mouse>/rightButton");
        public bool DepthHeld => enabled && Captured && depthDrag.IsPressed();
        public string DepthBinding => depthDrag.GetBindingDisplayString();
        public void SetGrabBinding(string path) { grab.ChangeBinding(0).WithPath(path); }
        public bool Captured { get; private set; }
        public Vector2 PointerDelta => enabled && Captured ? pointer.ReadValue<Vector2>() : Vector2.zero;
        public bool LookHeld => enabled && Captured && cameraLook.IsPressed();
        public bool GrabHeld => enabled && Captured && grab.IsPressed();
        public bool GrabPressed => enabled && Captured && grab.WasPressedThisFrame();
        public bool PointHeld => enabled && Captured && point.IsPressed();
        public bool SelectionHeld => enabled && Captured && selection.IsPressed();
        public float DepthDelta => enabled && Captured ? depth.ReadValue<float>() : 0f;
        public string LookBinding => cameraLook.GetBindingDisplayString();
        public string GrabBinding => grab.GetBindingDisplayString();
        public string PointBinding => point.GetBindingDisplayString();
        public string SelectionBinding => selection.GetBindingDisplayString();

        private InputAction[] Actions => new[] { pointer, cameraLook, grab, point, selection, depth, releaseCursor, depthDrag };
        private void OnEnable() { foreach (var action in Actions) action.Enable(); }
        private void Start() { SetCapture(captureOnStart); }
        private void Update()
        {
            if (releaseCursor.WasPressedThisFrame()) SetCapture(false);
            else if (!Captured && (cameraLook.WasPressedThisFrame() || depthDrag.WasPressedThisFrame())) SetCapture(true);
        }
        public void SetCapture(bool capture)
        {
            Captured = capture;
            Cursor.lockState = capture ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !capture;
        }
        private void OnApplicationFocus(bool focused) { if (!focused) SetCapture(false); }
        private void OnDisable()
        {
            foreach (var action in Actions) action.Disable();
            SetCapture(false);
        }
    }
}
