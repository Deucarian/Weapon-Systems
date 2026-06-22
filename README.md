# Deucarian Weapon Systems

`com.deucarian.weapon-systems` orchestrates active weapons. It owns weapon slots, enable/disable state, fixed-tick cadence, direct/projectile fire modes, burst and volley descriptors, caller-supplied target candidates, and deterministic weapon intents.

It does not own unlocks, upgrades, rewards, persistence, enemy discovery, projectile physics, UI, VFX, audio, tower placement, encounters, or ECS.

## Runtime Dependencies

- `com.deucarian.gameplay-foundation`
- `com.deucarian.attacks`
- `com.deucarian.projectiles`

The package has no runtime dependency on Defense Games, Encounters, World Spawning, World Navigation, Progression, Persistence, UI, Core State, or Entities. Projectiles carries its own transitive integration dependencies; Weapon Systems references only the Projectiles API.

## Minimal Flow

1. Create `WeaponDefinition` entries.
2. Register weapon slots with `WeaponMountSnapshot`.
3. Provide `IWeaponAttackAdapter` and `IWeaponProjectileAdapter`.
4. Call `FireReady` or `TryFire`.
5. Resolve direct `AttackIntent` values through Combat or pass `ProjectileLaunchRequest` values to Projectiles.
6. Persist or inspect `WeaponSnapshot` where needed.

See `Samples~/BasicWeaponRuntime`.
