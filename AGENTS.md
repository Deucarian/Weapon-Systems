# Deucarian Weapon Systems Agent Notes

Package ID: `com.deucarian.weapon-systems`
Repository: `Deucarian/Weapon-Systems`

Follow the canonical Deucarian governance docs in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/develop/ARCHITECTURE.md), especially capability ownership and dependency rules.

## Ownership

This package owns:

- Weapon definitions, active weapon slots, fire cadence, direct and projectile fire modes, deterministic weapon intents, Unity weapon authoring assets, and Game Content Authoring providers for weapon definitions.

Registered capabilities:
- None.

This package must not own:

- Attack target selection, Combat damage resolution, projectile movement/impact lifecycle, world spawning internals, navigation/pathing internals, run upgrade drafting, progression/rewards, persistence, UI, VFX/audio playback, or product-specific weapon rules.

## Dependencies

Allowed dependency shape:

- Runtime weapon orchestration may depend on Gameplay Foundation, Attacks, and Projectiles.
- Authoring/editor surfaces may depend on Editor and Game Content Authoring.

Required dependencies and why:

- `com.deucarian.gameplay-foundation`: shared gameplay IDs and deterministic primitives.
- `com.deucarian.attacks`: attack definitions and direct attack intent context.
- `com.deucarian.editor`: shared editor shell/resources for authoring surfaces.
- `com.deucarian.projectiles`: projectile weapon mode integration points.
- `com.deucarian.game-content-authoring`: provider registration and validation UI for weapon content.

Optional/version-defined dependencies:

- None.

Architecture exceptions:

- None.

## Policies

- Keep this package focused on weapon orchestration and authoring.
- Do not add hard dependencies on Defense Games, Auto Defense, Run Upgrades, Progression, Persistence, UI, or template packages.
- Targeting, damage resolution, projectile lifecycle, and genre-specific weapon behavior belong in their owning packages or caller adapters.
- Logging: Do not introduce direct Unity Debug calls.
- Unity object lifetime: Use Common only if production code directly owns transient Unity object cleanup.
- Testing: Test fixture teardown may use Unity `DestroyImmediate` directly.

## Validation

Run the shared validator before committing:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Also run existing repository tests when changing code or asmdefs. Documentation-only updates should still run `git diff --check`.

## Codex Guidance

- Inspect current files before changing anything.
- Work on `develop`; do not edit or merge `main` unless the task is promotion-only.
- Do not edit `Library/PackageCache`.
- Do not guess package versions or dependency versions.
- Do not add package dependencies casually; update asmdefs, `package.json`, `deucarian-package.json`, Package Registry, Package Installer fallback, and Bootstrap fallback together when a dependency is truly required.
- Do not create local copies of shared helpers.
- Keep commits focused and report exactly what changed and what was validated.
