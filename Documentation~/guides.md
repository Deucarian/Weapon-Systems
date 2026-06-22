# Guides

## Runtime

Create a `WeaponRuntime`, register `WeaponMountSnapshot` values, call `Tick`, then call `FireReady` or `TryFire`.

## Fire Modes

`DirectAttack` delegates to Attacks. `Projectile` creates projectile launch requests and leaves actual launching to Projectiles.

## Burst, Volley, And Pattern

`BurstCount` and `WeaponFirePattern.VolleyCount` multiply emitted intents. `SpreadDegrees` is metadata for adapters and callers; this package does not aim or steer.

## Attacks Integration

Use `AttackRuntimeWeaponAttackAdapter` and register the same attack source with Attacks. Use zero-cooldown attack definitions when Weapon Systems owns cadence.

## Projectiles Integration

Use `ProjectileLaunchWeaponAdapter`, collect projectile intents, and pass their requests to `ProjectileRuntime.Launch`.

## Defense Games Composition

Defense Games can mount a weapon on a tower or defender and supply target candidates from its own lane/threat logic. This package does not reference Defense Games at runtime.

## Donor Mapping

The donor `WeaponRuntimeBase` and concrete projectile/melee/hitscan runtimes map to weapon definitions, slots, fire modes, and adapters. Donor enemy registry lookups, pooling, VFX/audio, owner callbacks, progression, and prefab instantiation should be discarded or moved to later adapters.

## Idle Auto Defense

Idle games can batch ticks and evaluate ready weapons against precomputed candidates. Snapshot data is sufficient for reconstruction; offline rewards remain outside this package.

## Classic Tower Defense

Towers can register one slot each, supply lane candidates, and emit direct/projectile intents. Placement, paths, range queries, and upgrades stay in other packages.

## Future Upgrade And Evolution

Upgrades should transform definitions or mount state outside this package and then re-register or reconstruct weapons.

## Future Inventory And Equipment

Inventory/equipment systems should decide which slots exist. Weapon Systems only runs mounted slots.

## Performance

Use warmed runtimes and preallocated candidate lists for benchmarks. Record elapsed time and allocated bytes honestly; result arrays are intentionally allocated by the current API.

## Validation

Run import, EditMode tests twice, and the benchmark method in the clean validation project.
