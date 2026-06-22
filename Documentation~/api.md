# API Documentation

## Identifiers

- `WeaponDefinitionId`: stable authored weapon definition id.
- `WeaponSlotId`: stable runtime slot or mount id.

## Definitions

- `WeaponDefinition`: authored weapon data for cadence, fire mode, attack definition, optional projectile definition, burst count, and pattern.
- `WeaponFireMode`: `DirectAttack` or `Projectile`.
- `WeaponFirePattern`: volley count plus spread angle descriptor.

## Runtime

- `WeaponRuntime`: registers/removes/disables slots, advances cooldowns, fires ready weapons, creates snapshots, and reconstructs from snapshots.
- `WeaponMountSnapshot`: slot state containing slot id, weapon definition id, source snapshot, enabled state, and remaining cooldown ticks.
- `WeaponSnapshot`: deterministic copy of all mounts.

## Requests and Results

- `WeaponFireRequest`: caller-supplied target candidates, origin, destination, and optional path.
- `WeaponFireResult`: success flag, failure reason, intents, fired count, and failure count.
- `WeaponIntent`: direct attack or projectile launch intent emitted by a weapon.
- `WeaponFailureReason`: explicit failure codes for unknown slots, disabled slots, cooldowns, missing candidates, adapter failures, and invalid input.

## Adapters

- `IWeaponAttackAdapter`: creates direct attack intents from Attacks.
- `AttackRuntimeWeaponAttackAdapter`: adapter for `AttackRuntime`.
- `IWeaponProjectileAdapter`: creates projectile launch requests.
- `ProjectileLaunchWeaponAdapter`: default adapter that creates `ProjectileLaunchRequest` values without launching them.
