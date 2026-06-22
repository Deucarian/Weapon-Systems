# Package Validation Notes

## Unity Version

Validation target: Unity `6000.3.5f1`.

## Package Import

The package is consumed through a local file reference in `C:\Repositories\Deucarian\WeaponSystems-TestProject`.

## Dependency Review

Runtime package dependencies:

- `com.deucarian.gameplay-foundation`
- `com.deucarian.attacks`
- `com.deucarian.projectiles`

No runtime dependency is declared on Defense Games, Encounters, World Spawning, World Navigation, Progression, Persistence, UI, Core State, or Entities.

## Completion Gate Records

Import:

- `WeaponSystems-TestProject-import-final.log`: clean import, no compiler/package errors found.

EditMode tests:

- Pass 1: 21 passed, 0 failed, 0 skipped, 0 inconclusive, duration 28.3390779 seconds.
- Pass 2: 21 passed, 0 failed, 0 skipped, 0 inconclusive, duration 31.9485958 seconds.

Benchmarks:

- Pass 1: 1,000 operations, 1,000 weapons, 0 candidates, 100% projectile mix, 1.916 ms, 0 allocated bytes.
- Pass 1: 5,000 operations, 5,000 weapons, 0 candidates, 100% projectile mix, 8.571 ms, 0 allocated bytes.
- Pass 1: 10,000 operations, 10,000 weapons, 0 candidates, 100% projectile mix, 17.527 ms, 0 allocated bytes.
- Pass 2: 1,000 operations, 1,000 weapons, 0 candidates, 100% projectile mix, 1.792 ms, 0 allocated bytes.
- Pass 2: 5,000 operations, 5,000 weapons, 0 candidates, 100% projectile mix, 9.313 ms, 0 allocated bytes.
- Pass 2: 10,000 operations, 10,000 weapons, 0 candidates, 100% projectile mix, 17.962 ms, 0 allocated bytes.

Benchmark path: `Tests/EditMode/WeaponSystemsTests.BenchmarkRecordsWeaponFireEvaluationCosts`.

These are Unity Editor benchmarks only and do not claim mobile runtime performance.
