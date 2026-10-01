# Physical Mouse Demo

## Open and play

The demo is installed in `H:\Unity\My project (2)` at `Assets/PhysicalMouseDemo/PhysicalMouseDemo.unity`. Open that scene, press Play, and click the Game view. The main scene was not overwritten. A copy of its previously loaded working state is in `Assets/PhysicalMouseDemo/Backups`.

For teammates: import `PhysicalMouseDemo.unitypackage` into a Unity 6 URP project with the Input System installed and enabled. It was validated with Unity 6000.6.0f1, URP 17.6 and Input System 1.20. Open the included scene. MCP is not required to play the demo. The package excludes the main scene, its backup, hand FBXs and MCP setup.

| Control | Result |
|---|---|
| Move mouse | Move the animated hand within reach |
| Mouse wheel | Move the reach target nearer/farther |
| Hold left mouse | Rotate the camera; release to let momentum decay |
| Hold right mouse and move forward/back | Move the hand away from / closer to the camera |
| Hold E near an object | Grab it; release to drop/throw |
| Escape | Release the cursor |
| Left mouse over Game view | Capture the cursor again |
| Middle mouse / Tab | Reserved Point / Selection bindings, with debug feedback only |

Try the green 1 kg cube, then the orange 8 kg cube. Move the sphere near the top of the blue flight stick or yellow lever, hold E, and move it sideways. While grabbing the flight stick, mouse X/Y moves along its base's local right/forward axes, regardless of camera direction. The lever accepts local sideways movement. Ordinary objects retain camera-relative movement; RMB switches vertical mouse motion to depth. LMB camera look takes priority. Cyan means neutral, amber means a nearby object is available, and green means holding. The display shows the candidate, held object, camera speed, and normalized control values.

## Components

All gameplay scripts are in `Assets/PhysicalMouseDemo/Runtime`, in the `PhysicalMouseDemo` namespace.

| Script | Responsibility |
|---|---|
| PlayerInputController | Owns editable InputActions and cursor capture |
| PhysicalHandController | Converts mouse movement into a bounded target and uses forces to follow it |
| CameraLookController | Integrates camera angular velocity and applies acceleration, damping and pitch limits |
| GrabInteractor | Finds nearby grabbables and creates/releases a temporary physical joint |
| Grabbable | Marks a dynamic Rigidbody as usable; exposes display name and grip strength |
| ConstrainedInteractable | Reads lever/stick rotation and converts it to normalized input |
| HandVisual | Blends the imported hand between idle and grip poses without root motion |
| DemoFeedback | Colors the sphere and draws the simple debug display |
| DemoPlayChecks | Editor-only automated Play Mode checks; not part of player builds |

The editor utilities build/reset this demo, run its checks, and export a teammate package. **Build or Reset Demo Scene replaces the demo's objects**, so save a separate copy before experimenting with its layout.

## Hierarchy

```text
PhysicalMouseDemo
  Player [input, camera look, feedback]
    CameraPivot
      Main Camera
      HandAnchor
        PhysicalHand [Rigidbody, hand controller, grab interactor]
  Environment
    Floor
    Workbench
    GrabCube
    HeavyCube
    LeverDemo
      Base
      Handle [Rigidbody, Grabbable, HingeJoint, control reader]
    AircraftControlDemo
      Base
      Handle [Rigidbody, Grabbable, ConfigurableJoint, control reader]
    Labels
  Lighting
    Sun
```

At runtime the hand detaches from its parent so camera movement cannot teleport its Rigidbody. It keeps a reference to CameraPivot for calculating its target.

## Values to experiment with

- **Player / PlayerInputController:** expand the inline InputActions to edit Look, Grab, Point and Selection bindings. Defaults are centralized here.
- **Player / CameraLookController:** sensitivity, acceleration, damping and pitch range. Lower damping produces a longer coast after releasing Look.
- **PhysicalHand / PhysicalHandController:** sensitivity, smoothing, horizontal/vertical reach, depth range, depth-drag sensitivity, spring, damping and maximum force. Bounds limit the target; collisions and inertia can displace the physical sphere from it.
- **PhysicalHand / GrabInteractor:** detection radius, spring, damping, maximum force and maximum stretch. A stretched grab releases automatically.
- **Object / Rigidbody:** mass. The cubes use 1 kg and 8 kg. Avoid changing masses by huge factors without retuning the forces.
- **Object / Grabbable:** grip-strength multiplier and display name. Set Movement Frame to a stable base transform to opt into object-relative input; Mouse X Axis and Mouse Y Axis map mouse movement to that frame. Zero an axis to disable it. Movement Radius limits travel around the grabbed point. Leave the frame empty for camera-relative control.
- **Lever Handle / HingeJoint:** limits of ±45 degrees and return spring.
- **Flight Stick Handle / ConfigurableJoint:** ±25-degree pitch/roll limits. Keep the reader's range setting consistent when changing limits.

Both control handles are connected to kinematic base Rigidbodies, with collisions between each handle and its own base disabled. ConstrainedInteractable exposes Holding Spring, Holding Damping and Holding Force. It holds the released angle rather than returning to center. Finite drives allow external forces to move it; these are not immovable locks.

## How it works

Input feeds either hand movement or camera look. The hand's target is smoothed, then a capped spring-and-damping force pulls its Rigidbody toward that target during FixedUpdate. The sphere collides with the environment.

Grabbing adds a temporary ConfigurableJoint to the selected object's Rigidbody. Finite-force drives pull an object anchor toward an anchor on the hand. The object stays dynamic and receives physical forces. The same operation works on the cubes, lever and flight stick; their existing joints provide their own restrictions. Releasing clears the temporary drives and removes only that grab joint, leaving linear/angular velocity and the original control constraint intact.

Camera look converts mouse delta into a desired angular velocity. Acceleration eases toward it while Look is held. On release, exponential damping reduces the stored velocity while the camera continues rotating. Pitch is clamped; yaw can turn around freely.

## Validation

The updated automated Play Mode suite passed 38 checks with zero runtime errors. It checks RMB depth, visible hand/grip blending, object-relative movement, released control position and anchored bases. See the included JSON report for the latest result. The original run passed 27 checks with zero runtime console errors. It covered hand delta movement and target bounds, dynamic grabbing/releasing of both cubes, retained release velocity, mass response, shared lever/stick grabbing and rotation, retained constraints, camera coasting, decay and both pitch limits. In the same lift test the light cube moved about 0.311 m and the heavy cube about 0.130 m. The JSON report is included alongside this guide.

These checks drive controller methods directly and verify bindings are present; they are not a substitute for testing real mouse feel on each teammate's computer. Use `Tools > Physical Mouse Demo > Run Play Mode Checks` with the demo active to repeat them. The report is written to the project's `Logs/PhysicalMouseDemo-tests.json`.

## Future additions

The animated hand is now installed as a visual child of the hidden sphere collider. HandVisual blends the existing idle and grab-hold clips using Playables. Root motion is disabled. It is a visual grip, not per-finger contact IK; fingers may intersect differently sized objects. Point/pinch and object-specific finger fitting remain future work.

Pointing and teleportation can use an index-tip transform for a ray and a separate destination validator. UI interaction can use a fingertip probe and explicit press state. Aircraft logic can consume the normalized control values without changing grabbing. Doors can use the same Grabbable plus a hinge. Throws already retain release velocity; later tune gesture response and speed limits. Punching can read collision impulse and relative velocity. These larger systems are intentionally not implemented here.

## MCP connection

The current project contains the MCP for Unity package, and Codex is configured for `http://127.0.0.1:8080/mcp`. A local compatibility fix handles Unity 6.6 object lookup. Hierarchy/component reads, object creation, script creation/editing, component attachment, Inspector changes and test-scene creation were exercised during setup.

The server is running locally for this session. After restarting Windows, run the supplied `Start_Unity_MCP.ps1` with PowerShell, open Unity, and use its MCP window to start/reconnect the session if it does not connect automatically. The launcher uses this computer's installed workspace runtime; teammates only need the demo package, not this launcher.
