# Physical Mouse Demo

A Unity class prototype in which the mouse moves a physical, animated hand in the world. It demonstrates grabbing light/heavy objects, a one-axis lever, and a two-axis flight stick.

## Start here

1. Clone this repository.
2. In Unity Hub, add the cloned folder as an existing project.
3. Open with **Unity 6000.6.0f1** and let package import finish. The project uses URP and the new Input System.
4. Open `Assets/PhysicalMouseDemo/PhysicalMouseDemo.unity`.
5. Press Play and click the Game view.

| Input | Action |
|---|---|
| Mouse movement | Move the hand |
| Hold RMB | Camera look with rotational momentum |
| Hold Left Shift + move mouse forward/back | Move the hand away/closer |
| Hold LMB near an object | Grab; release LMB to let go |
| T | Toggle edge turning |
| Wheel | Adjust reach depth |
| Escape | Release the cursor |
| Middle mouse / Tab | Reserved Point / Selection inputs |

The hand follows and rotates with the camera while carrying. Edge turning pans the view as the hand approaches the screen edges; adjust the activation margin and speed on Player > EdgeTurnController.

Bindings are editable on `Player > PlayerInputController`. While grabbing a lever or flight stick, input follows its base's axes rather than the camera. Controls hold their released position. The visible hand blends between idle and grip; per-finger contact IK is not implemented.

## Team workflow

**Start here: [Team setup and branch workflow](TEAM_WORKFLOW.md).** Everyone clones this repository; use one branch per task and a pull request into `main`.

- Make a branch for each change and use pull requests for review.
- Pull before starting work. Coordinate scene edits: Unity scene conflicts are harder to merge than scripts.
- Commit assets together with their `.meta` files; move/rename assets through Unity.
- Do not commit Library, Temp, Logs, local recovery copies, or generated IDE files.
- Use a separate scene for experiments. The builder's **Build or Reset Demo Scene** command replaces the demo layout.

## Code and testing

Gameplay components are in `Assets/PhysicalMouseDemo/Runtime`. Input, camera movement, physical hand following, grabbing, control readouts and visual animation are separate components.

Read [the demo guide](Assets/PhysicalMouseDemo/README.md) for architecture and Inspector tuning. Run **Tools > Physical Mouse Demo > Run Play Mode Checks** with the demo scene active. The Play Mode suite covers physical interaction, hand/camera following and edge-turn behavior; actual mouse feel should also be tested manually.

The original game scene and hand assets are retained. Generated scene backups and machine-specific MCP connection scripts are not shared.

## Optional AI connection

The embedded MCP for Unity package includes a Unity 6.6 object-lookup compatibility fix. MCP is optional for playing or editing the demo. Each developer configures their own local MCP server if desired; no credentials or local server process are included. Third-party package licensing is in its `LICENSE` file.

