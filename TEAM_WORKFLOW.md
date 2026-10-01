# Team starting guide

Use this repository as the shared starting foundation. Each teammate clones it onto their own computer. Keep `main` as the version that opens and plays; create a separate short-lived branch for each task and merge through a pull request.

There is no shared `develop` branch to maintain. A branch belongs to a task, not permanently to a person. You do not need to fork the repository when you have collaborator access.

## First-time setup

1. Accept your GitHub collaborator invitation. This repository is private: the URL alone does not grant access.
2. In GitHub Desktop, choose **File > Clone repository**, select `breezy123z/physical-mouse-demo`, and choose a local folder. Alternatively clone `https://github.com/breezy123z/physical-mouse-demo.git` using Git.
3. In Unity Hub, add that folder and open it with **Unity 6000.6.0f1**. Let packages import.
4. Open `Assets/PhysicalMouseDemo/PhysicalMouseDemo.unity`, press Play, and try the controls in the [README](README.md).
5. Read the [architecture and tuning guide](Assets/PhysicalMouseDemo/README.md). No MCP or AI account is required to work on the game.

## Every new task

1. Agree on a small task in a GitHub issue. Claim it so two people do not unknowingly change the same system or scene.
2. Save your Unity work and leave Play Mode. In GitHub Desktop, switch to `main`, fetch and pull the latest changes. Commit or stash unfinished work before switching branches; do not discard it.
3. Choose **Current branch > New branch**. Use a task name such as `feature/button-interaction`, `feature/cockpit-scene`, or `fix/grab-jitter`.
4. Work locally, test, and commit related changes together. Publish/push your branch.
5. Open a pull request into `main`. Describe the behavior and how a teammate can test it. Include a short recording for interaction changes.
6. Have another teammate review and try it. Resolve problems, then squash-merge the pull request. Review is a team convention; this setup does not enforce branch protection.
7. Return to `main`, pull the merged result, and start a fresh branch for the next task. GitHub removes merged remote task branches automatically; local copies can be removed after your work is safely merged.

## Unity collaboration rules

- Commit each asset together with its `.meta` file. Move and rename assets in Unity to preserve references.
- Coordinate ownership of a scene or prefab before editing it. For experiments, create a separate scene rather than having everyone edit PhysicalMouseDemo simultaneously.
- Prefer reusable components and prefabs so teammates can build features independently.
- Never blindly choose “ours” or “theirs” for a scene conflict. Coordinate with the other author, preserve both versions, and verify references in Unity.
- Do not upgrade Unity or shared package versions without agreeing with the team.
- Do not commit Library, Temp, Logs, generated IDE files, credentials or local backups. The ignore file handles common generated files.
- **Build or Reset Demo Scene** replaces the demo layout. Make a separate copy if you want to keep custom scene edits.

## Before merging

- Unity opens and compiles without errors.
- The demo still plays; test grab/release, camera look, depth movement, edge turning and both controls when relevant.
- For interaction/controller changes, run **Tools > Physical Mouse Demo > Run Play Mode Checks** with PhysicalMouseDemo active and report the outcome.
- Check the changed-file list for unrelated scene edits or missing `.meta` files.
- Document changed bindings and tuning defaults.

## Foundation snapshot

The `foundation-v0.1` release preserves the starting prototype so you can compare future work with the original demo. It contains physical grabbing, the animated hand, camera momentum, edge turning, lever and flight-stick examples. It is a class prototype, not a finished game: finger-contact IK, full aircraft physics, teleportation and UI interaction remain future work.

Clone the repository's `main` branch for active development. The release ZIP is useful for inspecting the snapshot, but does not include Git history and is not the normal team workflow.

## Adding teammates

The owner adds each teammate under **Settings > Collaborators**, and each teammate accepts the invitation. Everyone then works in this same repository using task branches and pull requests. Do not share account credentials.
