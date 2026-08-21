# HimoHitoPrototype project guide

## Project purpose

This is a Unity 2D graybox prototype of 「ヒモヒト」. The player spends their own rope length to attach to platforms, swings like a pendulum, and recovers the spent length when the rope is released.

The current goal is to validate the feel of the core mechanic. Do not add enemies, final art, BGM, multiple stages, or rope cutting unless the user explicitly expands the scope.

## Environment

- Unity Editor: `6000.3.21f1`
- Main scene: `Assets/Scenes/Prototype.unity`
- Input: Built-in Input Manager
- Main branch: `main`

Never commit `Library/`, `Temp/`, `Logs/`, or `UserSettings/`. Always preserve and commit Unity `.meta` files together with their assets.

## Controls

- `A` / `D`: move and pump the pendulum
- `Space`: jump while not attached
- `Left` / `Right Arrow`: rotate the rope aim guide
- `Up` / `Down Arrow`: snap the aim upward or downward
- Press `E` while detached: shoot and attach the rope
- Press `E` while attached: detach, recover the rope, and preserve momentum
- Press `Q` while attached: turn the rope from the hook to the player into a permanent platform

Mouse aiming remains an optional secondary input.

## Code map

- `Assets/Scripts/RopeResource.cs`: single source of truth for remaining rope length
- `Assets/Scripts/RopeController.cs`: aiming, raycast, joint attachment, line rendering, detach/refund
- `Assets/Scripts/PlayerMover.cs`: ground, air, and pendulum motion modes
- `Assets/Scripts/PrototypeHud.cs`: remaining rope, speed, and control help
- `Assets/Editor/PrototypeSceneBuilder.cs`: reproducible graybox scene generation

Read `README.md` before changing gameplay and update it when controls or physical behavior change.

## Physics contract

- Ground movement may approach a target horizontal speed for responsiveness.
- Airborne and attached movement must use forces; do not overwrite velocity each physics frame.
- While attached, input force must remain tangent to the circle around `RopeController.AnchorPoint`.
- Releasing the rope must preserve linear and angular velocity.
- Keep swing damping small so momentum decays gradually rather than stopping abruptly.
- `DistanceJoint2D` provides rope tension; do not replace it without explaining and testing the new constraint model.

## Change workflow

1. Make one gameplay idea per commit.
2. Explain the changed input, state, and physics effect in plain language.
3. Compile against Unity 6.3 assemblies or verify inside Unity.
4. Run `git diff --check` and confirm `git status` before committing.
5. Update `Docs/LEARNING_LOG.md` for a new learning milestone.

Prefer small, reviewable changes because the owner is learning the generated code and must be able to explain it.
