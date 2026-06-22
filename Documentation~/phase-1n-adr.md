# ADR: Phase 1N Weapon Systems Boundary

## Decision

Weapon Systems is a reusable orchestration package for active weapons. It sits above Attacks and Projectiles and below game-specific loadouts, unlocks, progression, encounters, UI, and save data.

## Required boundary points

1. Weapon Systems owns active weapon slots, weapon enable/disable state, fixed-tick cadence, fire modes, pattern descriptors, and deterministic intent ordering.
2. Attacks owns target selection policy, direct attack intent creation, and Combat request creation. Weapon Systems calls it through `IWeaponAttackAdapter`.
3. Projectiles owns projectile lifecycle, launch, movement adapter calls, impact reporting, and cleanup. Weapon Systems emits `ProjectileLaunchRequest` values through `IWeaponProjectileAdapter`.
4. Weapon Systems does not discover enemies. Callers supply target candidates.
5. Weapon Systems does not place towers, build paths, spawn waves, grant rewards, unlock content, save state, render UI, play VFX/audio, or drive physics hit detection.
6. Runtime dependencies are limited to Gameplay Foundation, Attacks, and Projectiles.
7. Combat is not directly referenced by runtime code. Tests may reference Combat to prove direct-damage composition.
8. Defense Games, Encounters, World Spawning, World Navigation, Progression, Persistence, UI, Core State, and Entities are not runtime dependencies.
9. Slot ordering is deterministic by `WeaponSlotId`, independent of registration order.
10. Weapon cadence is fixed-tick based and independent from frame time.
11. Burst and volley are descriptors that multiply emitted intents; timing between burst shots is future work.
12. Fire patterns carry lightweight spread metadata but do not compute aim, pathfinding, physics, or projectile steering.
13. `WeaponSnapshot` is reconstruction data, not persistence policy.
14. Adapters are explicit so survivor games, Idle Auto Defense, and classic Tower Defense can connect the same API to different target and launch systems.
15. Future ECS, inventory/equipment, upgrade/evolution, and specialized navigation integrations should layer on top without changing this core runtime boundary.

## Consequences

The package is intentionally small. It can represent a rotating set of survivor-style weapons, idle defense weapons that tick offline in batches, or tower-defense weapons mounted on towers, but it does not encode those game modes.
