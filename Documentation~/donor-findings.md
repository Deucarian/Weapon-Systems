# Donor Findings

Audited donor project:

`C:\Repositories\JorisHoef\Codex-Attempted-Vampire-Project\Codex-Attempted-Vampire-Project`

## Clean Mappings

- `WeaponDefinition.Id` maps to `WeaponDefinitionId`.
- Donor runtime slot ownership maps to `WeaponSlotId` plus `WeaponMountSnapshot`.
- `cooldownSeconds` maps to fixed-tick `CooldownTicks` after project-specific seconds-to-ticks conversion.
- `projectileCount`, `GetProjectileCountForRank`, and simple count bonuses map to `BurstCount` or `WeaponFirePattern.VolleyCount` where they are pure emitted-intent counts.
- `projectileSpreadAngle` maps to `WeaponFirePattern.SpreadDegrees` as metadata.
- Direct melee/hitscan behaviors map to `WeaponFireMode.DirectAttack` and `IWeaponAttackAdapter`.
- Projectile behaviors map to `WeaponFireMode.Projectile` and `ProjectileLaunchRequest`.

## Adapter Needs

- `EnemyRegistry.FindNearest` and active-enemy scans should become caller-supplied `AttackTargetCandidate` lists.
- Donor damage stat calculations should feed Attacks/Combat source snapshots, not Weapon Systems.
- Donor projectile object creation should move through Projectiles and World Spawning adapters.
- Donor projectile movement, impact, chain, pierce, fork, return, hazards, summons, and payload behavior should remain in later packages or project adapters.
- Donor VFX/audio/feedback calls should remain application-level subscribers or adapters.

## Discarded Assumptions

- Weapons should not scan scenes or registries by themselves.
- Weapons should not instantiate prefabs or create beam/slash GameObjects directly.
- Weapon firing should not directly grant rewards, select upgrades, write saves, or update UI.
- Survivor auto-targeting should not be encoded as the only supported weapon model.

## Proof

The package tests include donor-style projectile, direct, idle defense, and classic tower defense flows through the same `WeaponRuntime` API. No donor files were modified.
