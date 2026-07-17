# Basic Weapon Runtime

This sample creates a projectile-mode weapon, registers it in a primary slot, and emits deterministic fire intents through the package's projectile adapter.

Open `BasicWeaponRuntime.unity` for the importable scene and call `BasicWeaponRuntimeSample.CreateProjectileIntents()` from a bootstrap or smoke test. Targeting and projectile execution remain in their owning packages.
