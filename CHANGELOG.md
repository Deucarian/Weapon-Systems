# Changelog

## 0.1.1 - 2026-07-17

- Aligned package metadata and the playable sample with the portfolio contract; direct Deucarian dependencies now use the coordinated patch versions.
- Replaced duplicated Weapon provider state, validation, reference, and summary code with shared Game Content Authoring primitives.
- Converted the stable Weapon provider into a pack-aware Weapon / Tower lens supporting independent Weapon-only and Tower-only records, typed read-only projections, cross-lens identity, and the existing Project Content ScriptableObject workflow.

## 0.1.0

- Added initial Weapon Systems runtime for deterministic slot registration, fixed-tick fire cadence, direct attack intents, projectile launch intents, burst/volley descriptors, snapshots, and adapter-driven composition with Attacks and Projectiles.
- Added EditMode tests, sample, API documentation, ADR, validation notes, and performance recording guidance.
