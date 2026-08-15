# Unity Multiplayer Gameplay Systems

Selected gameplay and networking code from an in-development Unity multiplayer sea-adventure project.

This repository is a **code portfolio**, not a redistributable copy of the full game. Scenes, art, animations, models, audio, paid assets, and other third-party content are intentionally excluded.

## Repository scope

This is a curated source-code showcase extracted from a larger game project. It is intentionally **not a standalone Unity project**: project scenes, prefabs, art, paid packages, and generated code are omitted.

The repository is meant to make architecture and implementation decisions easy to review during a technical evaluation.

## What this repository demonstrates

### Interaction architecture
- Interface-driven interaction targets (`IInteractable`)
- Context passed as immutable data (`InteractionContext`)
- Instant and hold interactions through `InteractionOffer`
- Focus enter/exit notifications
- Collider-to-gameplay target separation through `InteractionTargetProxy`
- A ship boarding interaction as a concrete implementation

Start here:
- `Source/Interaction/Player/PlayerInteractor.cs`
- `Source/Interaction/Core/InteractionOffer.cs`
- `Source/Ship/Boarding/ShipBoardingPoint.cs`

### Character movement and swimming
- CharacterController-based movement motor
- Separation of input/decision logic from motor simulation
- Ground probing and moving-platform tracking
- Grounded, airborne, surface-swimming, and underwater locomotion
- ScriptableObject-driven tuning values

Start here:
- `Source/Player/PlayerBrain.cs`
- `Source/Player/Movement/CharacterMotor.cs`
- `Source/Player/Movement/CharacterPlatformTracker.cs`

### Multiplayer presentation and validation
Built with **Netcode for GameObjects**.

- Owner-authored network animation state
- Quantized animation values to reduce replicated state size
- Server-side validation of client transform requests
- Moving-platform-relative state for remote player presentation
- Owner-only behaviour activation
- Networked player visual selection

Start here:
- `Source/Networking/Player/Animation/NetworkPlayerAnimationState.cs`
- `Source/Networking/Player/Animation/NetworkPlayerAnimationPresenter.cs`
- `Source/Networking/Player/NetworkPlayerMotionValidator.cs`
- `Source/Networking/Player/NetworkPlayerPlatformPresentation.cs`

### Ship physics and buoyancy
- Rigidbody wrapper with explicit simulation control
- Multi-point buoyancy
- Per-point vertical damping and horizontal water drag
- Batched water-surface sampling
- Network authority bridge for ship physics

Start here:
- `Source/Ship/Buoyancy/ShipBuoyancy.cs`
- `Source/Ship/Physics/ShipPhysicsBody.cs`
- `Source/Water/WaterSurfaceProvider.cs`

## Architecture overview

```text
Input
  |
  v
PlayerBrain ------------------------------+
  |                                      |
  v                                      v
CharacterMotor                     Interaction System
  |                                      |
  +--> Ground / Platform Tracking        +--> Boarding / Future Interactions
  |
  +--> Swimming / Water Sampling

Owner Gameplay State
  |
  +--> Network Animation State
  +--> Network Platform State
  +--> Transform Validation (Server)

Water Surface --> Multi-point Buoyancy --> Ship Physics
```

## Dependencies

The original project currently uses:
- Unity 6.x
- C#
- Unity Input System
- Netcode for GameObjects
- Cinemachine
- TextMesh Pro
- Stylized Water 3

`WaterSurfaceProvider.cs` is original integration code that calls the Stylized Water 3 API. **Stylized Water 3 itself is a paid third-party asset and is not included in this repository.**

The Unity Input System generated `PlayerInputActions.cs` wrapper is also intentionally omitted from the portfolio source. `Config/IA_Player.inputactions` is included only to show the authored input configuration.

## Project status

Work in progress. The code reflects systems currently being developed and iterated on, rather than a finished commercial release.

## Notes

The purpose of this repository is to demonstrate gameplay-programming, multiplayer, and systems-design work. It is not intended to be imported as a standalone Unity package without the dependencies and scene setup from the original project.
