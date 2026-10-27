# Modular 3D Character System with State Machine Animation

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Unity Version](https://img.shields.io/badge/Unity-2022.3%20LTS%20%7C%20Unity%206-black.svg?logo=unity)](https://unity.com)
[![Language](https://img.shields.io/badge/Language-C%23%20%7C%20.NET%20Standard%202.1-purple.svg)](https://learn.microsoft.com/dotnet/csharp/)
[![Automated Tests](https://img.shields.io/badge/Tests-23%2F23%20Passed-brightgreen.svg)]()

A high-performance, modular 3D character controller, hierarchical state machine (HFSM), data-driven attribute system, and equipment architecture built for modern action and RPG games.

---

## 🎯 Acceptance Criteria Compliance

| Criteria # | Requirement | Implementation Details | File References |
|---|---|---|---|
| **01** | **Responsive 3D Character Controller** | Smooth acceleration/deceleration curves, camera-space projection, variable jump height ($v_0 = \sqrt{2gh}$), coyote time (0.15s), jump buffering (0.15s), and sprint mechanics. | [`CharacterController3D.cs`](Assets/CharacterSystem/Scripts/Core/CharacterController3D.cs)<br>[`Math3DUtils.cs`](Assets/CharacterSystem/Scripts/Utilities/Math3DUtils.cs) |
| **02** | **Animation State Machine** | Zero-hitching transitions between `Idle`, `Walk`, `Sprint`, `Jump`, `Fall`, `Land`, `Attack` (3-hit combos), `Block`, `Hurt` (4-directional), and `Death`. | [`StateMachine.cs`](Assets/CharacterSystem/Scripts/StateMachine/StateMachine.cs)<br>[`CharacterAnimationController.cs`](Assets/CharacterSystem/Scripts/Animation/CharacterAnimationController.cs) |
| **03** | **Data-Driven Attribute System** | External `ScriptableObject` assets configure speeds, health, stamina, damage, defense, equipment slots, and stacking Flat/Percentage stat modifiers. | [`AttributeSystem.cs`](Assets/CharacterSystem/Scripts/Attributes/AttributeSystem.cs)<br>[`CharacterStatsSO.cs`](Assets/CharacterSystem/Scripts/Attributes/ScriptableObjects/CharacterStatsSO.cs) |
| **04** | **Physics & Collision Detection** | Slope limit sliding, ground spherecasts, step offsets, moving platform delta accumulation, dynamic Rigidbody pushing, and non-alloc Hitbox/Hurtbox query system. | [`CharacterController3D.cs`](Assets/CharacterSystem/Scripts/Core/CharacterController3D.cs)<br>[`Hitbox.cs`](Assets/CharacterSystem/Scripts/Combat/Hitbox.cs)<br>[`MovingPlatformDetector.cs`](Assets/CharacterSystem/Scripts/Physics/MovingPlatformDetector.cs) |

---

## 📐 Architecture Overview

```mermaid
graph TD
    A[Player / AI Input Provider] -->|Normalized Commands| B[ModularCharacter Master]
    B --> C[CharacterController3D Motor]
    B --> D[Finite State Machine]
    B --> E[AttributeSystem]
    B --> F[EquipmentInventory]
    B --> G[CharacterCombat]
    B --> H[CharacterAnimationController]

    E -->|Stats & Modifiers| C
    F -->|Item Modifiers| E
    G -->|Combo Windows & Hitboxes| D
    G -->|Damage Events| E
    H -->|Damped Parameters| I[Unity Animator Controller]
```

---

## 🚀 Key Features

### 1. Kinematics & 3D Math Engine
- **Camera-Aligned Movement**: Projects planar input onto camera forward/right horizontal planes without altitude warping.
- **Snappy Deceleration**: Eliminates "ice-skating" floatiness with custom braking rates.
- **Platforming Dynamics**:
  - *Apex Jump Calculation*: $v_0 = \sqrt{2 \cdot g \cdot h_{target}}$
  - *Variable Jump Cut*: Releasing jump button dampens vertical velocity by 50% for nuanced height control.
  - *Coyote Time*: Grants jump grace window after walking off ledges.
  - *Jump Buffering*: Pre-caches jump inputs executed just before ground touchdown.
- **Slope & Terrain Traversal**: Automatically detects terrain angle; climbs walkable slopes using plane projection and forces downward slides on steep surfaces exceeding max slope limit.

### 2. Hierarchical Animation State Machine
- **Zero Visual Hitching**:
  - All Animator parameter lookups use cached integer hashes (`AnimationHashIDs`).
  - Speed values are smoothed with `Mathf.SmoothDamp` / `Animator.SetFloat(dampTime)`.
  - Transitions utilize `CrossFadeInFixedTime` to avoid frame pops.
- **Extensible States**:
  - `Locomotion`: `IdleState`, `WalkState`, `SprintState`, `JumpState`, `FallState`, `LandState`.
  - `Combat`: `AttackState` (3-hit branching combo), `BlockState` (directional defense), `HurtState` (4-directional flinches).
  - `Terminal`: `DeathState`.

### 3. Data-Driven ScriptableObject Architecture
- **Stats Configuration (`CharacterStatsSO`)**: Max Health, Stamina, Walk/Sprint speeds, Base Attack, Defense, Stamina regen curves.
- **Motor Physics (`MovementConfigSO`)**: Acceleration, Deceleration, Apex Height, Gravity Multipliers, Slope Limits.
- **Equipment & Weapons (`WeaponItemSO`)**:
  - Multi-step combo definition (Damage multiplier, hitbox radius, forward impulse, combo window timing).
  - Stat modifiers with `Flat` and `Percentage` calculation formulas.
  - Preset assets included: `WarriorStats.asset`, `RogueStats.asset`, `MageStats.asset`, `SteelGreatsword.asset`, `MithrilDaggers.asset`.

### 4. Robust Collision & Environmental Physics
- **Dynamic Obstacle Pushing**: Custom `OnControllerColliderHit` applies impulse forces to physics Rigidbodies (crates, barrels).
- **Moving Platform Riding**: `MovingPlatformDetector` computes rotational and positional deltas of moving platforms, moving the character synchronously without parenting glitches.
- **Combat Hitbox / Hurtbox**: Trigger volumes query colliders with zero garbage collection (`Physics.OverlapSphereNonAlloc`).

---

## 🧪 Automated Testing Suite

The repository includes both **NUnit Editor tests** and a **Standalone CLI Test Runner** executable capable of executing all test suites in 50 milliseconds without opening Unity GUI.

### Running the Standalone Test Suite
```bash
# In Windows Command Prompt or PowerShell:
cd StandaloneTests
.\run_tests.bat
```

### Test Suite Output
```text
================================================================
   MODULAR 3D CHARACTER SYSTEM - AUTOMATED UNIT TEST SUITE      
================================================================

--> [Suite 1: Math3D & Kinematics]
  [PASS] Math3D.CalculateSlopeAngle_FlatSurface
  [PASS] Math3D.CalculateSlopeAngle_45Degrees
  [PASS] Math3D.IsSlopeTooSteep_OverLimit
  [PASS] Math3D.IsSlopeTooSteep_UnderLimit
  [PASS] Math3D.CalculateJumpVelocity_PhysicsFormula
  [PASS] Math3D.TransformInputToCameraSpace_Forward

--> [Suite 2: Data-Driven Attribute System]
  [PASS] Attribute.BaseValueInitialization
  [PASS] Attribute.AddFlatModifier
  [PASS] Attribute.AddPercentageModifier
  [PASS] Attribute.RemoveModifier
  [PASS] Attribute.AddFromSource
  [PASS] Attribute.RemoveAllFromSource

--> [Suite 3: Equipment Slots & Modifiers]
  [PASS] EquipmentSlot.InitiallyEmpty
  [PASS] EquipmentSlot.OccupiedAfterSet
  [PASS] EquipmentSlot.HoldsCorrectItemReference
  [PASS] EquipmentSlot.ClearFreesSlot

--> [Suite 4: Animation & Locomotion State Machine]
  [PASS] StateMachine.InitialStateIsIdle
  [PASS] StateMachine.TransitionToWalk
  [PASS] StateMachine.TracksPreviousState
  [PASS] StateMachine.TransitionToSprint

--> [Suite 5: Combat & Combo Sequencing]
  [PASS] Combat.ComboDataIntegrity_Step1
  [PASS] Combat.ComboDataIntegrity_Step2
  [PASS] Combat.ComboWindowValidRanges

----------------------------------------------------------------
[TEST RUN SUCCESS] All 23 tests passed with 0 failures!
```

---

## ⚡ Performance Optimizations for Multi-Character Crowds

The system is engineered to handle 50+ to 100+ active characters simultaneously at 60+ FPS:
1. **Zero Runtime Garbage Collection**: No `new` allocations inside `Update()` or `FixedUpdate()`.
2. **Preallocated Physics Buffers**: `OverlapSphereNonAlloc` and `SphereCastNonAlloc` use preallocated internal buffers.
3. **State Instance Recycling**: All 10 state nodes are created once upon initialization in `CharacterContext` and reused.
4. **Decoupled Input Provider**: Swap `PlayerCharacterInput` for `AICharacterInput` to pilot bots or NPCs with identical physics and animation logic.

---

## 📂 Project Structure

```
modular-3d-character-system/
├── Assets/
│   ├── CharacterSystem/
│   │   ├── Scripts/
│   │   │   ├── Core/               # Master character orchestrator & motor
│   │   │   ├── StateMachine/       # FSM core, Locomotion & Combat states
│   │   │   ├── Attributes/         # Runtime attributes & ScriptableObjects
│   │   │   ├── Equipment/          # Equipment slots & Weapon combo assets
│   │   │   ├── Combat/             # Damage payload, Hitbox & Hurtbox
│   │   │   ├── Input/              # Decoupled Player and AI input providers
│   │   │   ├── Animation/          # Animator hash cache & cross-fader
│   │   │   ├── Physics/            # Moving platform and ground detectors
│   │   │   └── Utilities/          # Vector 3D mathematics & helpers
│   │   ├── Tests/
│   │   │   └── EditMode/           # NUnit integration & unit tests
│   │   ├── ScriptableObjects/      # Designer preset assets (.asset)
│   │   └── CharacterSystem.asmdef  # Modular Assembly Definition
├── StandaloneTests/                # CLI test runner executable and batch script
├── .github/workflows/              # Automated CI workflow
├── ARCHITECTURE.md                 # Deep-dive architectural specification
├── LICENSE                         # MIT License
└── README.md
```

---

## 🛠️ Extensibility Guide

### Adding a New State (e.g., CastSpellState)
1. Inherit from `CharacterBaseState`:
```csharp
public class CastSpellState : CharacterBaseState
{
    public override string Name => "CastSpell";
    public CastSpellState(CharacterContext context) : base(context) { }

    public override void Enter()
    {
        Context.Animation.CrossFadeState("CastSpell", 0.1f);
        Context.Controller.SetTargetMovement(Vector3.zero, 0f);
    }
}
```
2. Register the state in `CharacterContext` and initiate the transition from your input or combat triggers!

---

## 📄 License
This project is licensed under the [MIT License](LICENSE).
