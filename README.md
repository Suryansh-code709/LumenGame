# Lumen: The Little Light Between Worlds

A 2D platform game about guiding a lantern-moth through a twilight world. Collect all six fireflies, avoid drifting wisps, and reach the moon gate.

## Run the game

1. Open this folder in Unity `6000.3.26f1`.
2. Open `Assets/Scenes/Lumen.unity`.
3. Press **Play**, then select **Begin the Journey**.

## Controls

- **A / D** or **Left / Right Arrow** — move
- **Space**, **W**, or **Up Arrow** — jump
- **Left / Right Shift** — dash
- **Q** — switch between Moon and Dawn phases
- **Esc** — pause or resume
- **R** — restart after winning or losing

Platforms in the current phase are solid. Dim platforms show the other phase. Collect all six fireflies to open the moon gate. Wisps and falls cost a heart and return you to the start.

## How the code is organized

- `LumenGame.cs` builds the level, tracks fireflies and hearts, switches phases, and manages the title, pause, win, and loss screens. Its variables hold the current run state; methods such as `BeginRun`, `ShiftPhase`, and `CollectFirefly` carry out game actions.
- `PlayerController2D.cs` reads keyboard input through Unity's New Input System and controls movement, jumping, dashing, and damage protection.
- `PhasePlatform2D.cs` makes a platform solid when its Moon or Dawn phase matches the active phase. The game loops through all platforms after each shift and updates their colliders and appearance.
- `FireflyPickup.cs`, `WispHazard.cs`, and `GoalGate2D.cs` handle collecting fireflies, taking damage, and unlocking the exit.
- `FollowCamera2D.cs` smoothly follows the player. `RuntimeSprites.cs` creates the simple shapes and glows used by the level.
- `Assets/Prefabs/PhasePlatform.prefab` and `Assets/Prefabs/Firefly.prefab` are reusable object templates. The saved `Lumen` scene contains the camera and game manager; the manager creates the level when Play begins.
- `Assets/Editor/LumenGameSetup.cs` contains the **Lumen → Build 2D Game Scene** editor command, which recreates the scene and Prefabs.

Most gameplay scripts inherit from `MonoBehaviour`, so Unity calls methods such as `Start`, `Update`, and collision callbacks at the right time. For example, `LumenGame.Update` checks for phase-change input, then `ShiftPhase` updates every phase platform. This connects input, conditions, loops, components, and game rules in one mechanic.
