# Unity Multiplayer Gameplay Systems

Selected gameplay, networking, physics, interaction, and presentation code from an in-development Unity multiplayer sea-adventure project.

This repository is a **code portfolio**, not a redistributable copy of the full game. Scenes, art, animations, models, audio, paid assets, and other third-party content are intentionally excluded.

## Repository scope

This is a curated source-code showcase extracted from a larger game project. It is intentionally **not a standalone Unity project**: project scenes, prefabs, art, paid packages, and generated code are omitted.

The goal is to make architecture and implementation decisions easy to review during a technical evaluation without publishing the complete game project.

## Suggested review order

If you only have a few minutes, these files show the main engineering decisions in the project:

1. `Source/Interaction/Player/PlayerInteractor.cs`
2. `Source/Player/Movement/CharacterMotor.cs`
3. `Source/Player/PlayerBrain.cs`
4. `Source/Networking/Player/NetworkPlayerPlatformPresentation.cs`
5. `Source/Networking/Player/Animation/NetworkPlayerAnimationState.cs`
6. `Source/Networking/Player/NetworkPlayerMotionValidator.cs`
7. `Source/Ship/Buoyancy/ShipBuoyancy.cs`

## What this repository demonstrates

### Interaction architecture
- Interface-driven interaction targets (`IInteractable`)
- Context passed as immutable data (`InteractionContext`)
- Instant and hold interactions through `InteractionOffer`
- Focus enter/exit notifications
- Collider-to-gameplay target separation through `InteractionTargetProxy`
- A ship boarding interaction as a concrete implementation
- UI presentation kept separate from interaction logic

Start here:
- `Source/Interaction/Player/PlayerInteractor.cs`
- `Source/Interaction/Core/InteractionOffer.cs`
- `Source/Ship/Boarding/ShipBoardingPoint.cs`
- `Source/Presentation/Interaction/InteractionPromptPresenter.cs`

### Character movement and swimming
- CharacterController-based movement motor
- Separation of input/decision logic from motor simulation
- Ground probing and moving-platform tracking
- Grounded, airborne, surface-swimming, and underwater locomotion
- External velocity support
- ScriptableObject-driven tuning values

Start here:
- `Source/Player/PlayerBrain.cs`
- `Source/Player/Movement/CharacterMotor.cs`
- `Source/Player/Movement/CharacterPlatformTracker.cs`
- `Source/Player/Swimming/CharacterWaterDetector.cs`

### Multiplayer presentation and validation
Built with **Netcode for GameObjects**.

- Owner-authored network animation state
- Quantized animation values to reduce replicated state size
- Server-side validation of client transform requests
- Moving-platform-relative state for remote player presentation
- Presentation smoothing separated from gameplay movement
- Owner-only behaviour activation
- Networked player visual selection

Start here:
- `Source/Networking/Player/Animation/NetworkPlayerAnimationState.cs`
- `Source/Networking/Player/Animation/NetworkPlayerAnimationPresenter.cs`
- `Source/Networking/Player/NetworkPlayerMotionValidator.cs`
- `Source/Networking/Player/NetworkPlayerPlatformState.cs`
- `Source/Networking/Player/NetworkPlayerPlatformPresentation.cs`

### Moving-platform stability
A moving ship creates a special multiplayer problem: the player has local movement of their own while the platform underneath them also moves and rotates.

The current solution separates **gameplay motion** from **remote visual presentation**:

- `CharacterPlatformTracker` tracks local gameplay-relative platform motion.
- The owning client publishes a compact platform-relative position and yaw.
- Remote clients reconstruct the visual position from the platform transform and the replicated relative state.
- The remote character intentionally does not inherit platform pitch/roll visually.
- `LocalCameraPlatformPresentation` handles local camera attachment separately so tiny CharacterController corrections are not amplified into visible camera jitter.

Start here:
- `Source/Player/Movement/CharacterPlatformTracker.cs`
- `Source/Networking/Player/NetworkPlayerPlatformPresentation.cs`
- `Source/Presentation/Player/LocalCameraPlatformPresentation.cs`

### Ship physics and buoyancy
- Rigidbody wrapper with explicit simulation control
- Multi-point buoyancy
- Per-point vertical damping and horizontal water drag
- Batched water-surface sampling
- Server-authoritative ship physics bridge
- Shared synchronized water simulation time for gameplay and rendering

Start here:
- `Source/Ship/Buoyancy/ShipBuoyancy.cs`
- `Source/Ship/Physics/ShipPhysicsBody.cs`
- `Source/Networking/Ship/NetworkShipBridge.cs`
- `Source/Water/WaterSurfaceProvider.cs`
- `Source/Networking/Water/NetworkWaterTimeDriver.cs`

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
                         |
                         v
                 Server Ship Authority
```

## A few implementation choices

### Gameplay and presentation are separate concerns
The player gameplay object remains responsible for movement and collision. Camera and remote-character compensation for the moving ship are handled by dedicated presentation components instead of being mixed into the motor.

### Network state is deliberately compact
`NetworkPlayerAnimationState` stores normalized movement values using quantized integer representations rather than continuously replicating raw floating-point animation parameters.

### Client movement is not accepted without bounds
`NetworkPlayerMotionValidator` constrains requested position and rotation changes on the server using elapsed server time, configured movement limits, and tolerance values.

### Water integration is wrapped behind project-owned code
The rest of the gameplay systems do not query Stylized Water 3 directly. `WaterSurfaceProvider` acts as the project-owned adapter and exposes single-point and batched gameplay sampling.

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

The Unity Input System generated `PlayerInputActions.cs` wrapper is intentionally omitted from the portfolio source. `Config/IA_Player.inputactions` is included to show the authored input configuration.

## Project status

Work in progress. The code reflects systems currently being developed and iterated on rather than a finished commercial release.

## Portfolio note

This repository is shared to demonstrate gameplay programming, multiplayer, architecture, and systems-design work. It is not intended to be imported as a standalone Unity package without the dependencies and scene setup from the original project.
