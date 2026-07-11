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

## Game Content Authoring

Weapon Systems contributes the `Weapon / Tower` lens to `Tools/Deucarian/Game Content Authoring`. The lens matches either the Weapon or Tower capability; games are not required to expose both. It displays immutable projected fire mode, damage, cooldown, range, targeting, payload, area, rank path, mutation/evolution links, and presentation data from the globally selected content pack.

External JSON-backed records are read-only and keep their canonical pack-scoped identity when opened in another compatible lens such as Attacks. Missing prefab or VFX data uses an authored-value preview fallback. Template packages provide `IGameContentRecordProjectionAdapter<WeaponContentRecordProjection>` adapters, so Weapon Systems does not parse game-specific formats or depend on a template.

Selecting `Project Content` preserves the existing standalone `WeaponDefinitionAsset` creation and editing workflow under `Assets/GameContent`. Creation is unavailable for read-only packs, All Packs, and contexts without an explicit writable backend.

## Install

Stable:

```json
"com.deucarian.weapon-systems": "https://github.com/Deucarian/Weapon-Systems.git#main"
```

Development:

```json
"com.deucarian.weapon-systems": "https://github.com/Deucarian/Weapon-Systems.git#develop"
```

Use `#main` for stable package consumption and `#develop` when testing active package work.

## When To Use This

Use this package when you need Reusable weapon orchestration plus Unity authoring assets/providers for active weapon slots, fire cadence, direct/projectile modes, and deterministic weapon intents.

Do not use this package to take ownership of capabilities outside its `AGENTS.md` boundary. Reusable behavior should stay with the package that owns that capability in the Package Registry governance docs.

## Quick Start

1. Install the package through Deucarian Package Installer or Unity Package Manager using the URL above.
2. Let Unity finish resolving packages and compiling assemblies.
3. Import the `Basic Weapon Runtime` sample if you want a working reference scene or setup.
4. Start from the package README sections above and the public runtime/editor APIs in this repository.

## Validation

Run the shared package validator from this repository root:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Documentation-only updates should still pass:

```powershell
git diff --check
```

## Troubleshooting

- Package does not resolve: confirm the stable or development Git URL matches the Package Registry entry and that required Deucarian dependencies are installed.
- Unity compile errors after install: let Package Manager finish resolving dependencies, then check asmdef references against `package.json` dependencies.
- Behavior appears to belong in another package: consult `AGENTS.md` and the Package Registry governance docs before moving or duplicating code.

## License

MIT. See `LICENSE.md`.
