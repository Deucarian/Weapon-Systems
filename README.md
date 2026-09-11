# Deucarian Weapon Systems

`com.deucarian.weapon-systems` orchestrates active weapons. It owns weapon slots, enable/disable state, fixed-tick cadence, direct/projectile fire modes, burst and volley descriptors, caller-supplied target candidates, and deterministic weapon intents.

It does not own unlocks, upgrades, rewards, persistence, enemy discovery, projectile physics, UI, VFX, audio, tower placement, encounters, or ECS.

## Generated keys in code and the Inspector

Project weapon definitions generate named, typed C# keys automatically. A `.g.cs` file is generated C# that Unity compiles normally. The generator runs in the editor; the player uses the compiled key code.

1. Create or edit a `WeaponDefinitionAsset` under your project's `Assets` folder using the existing authoring workflow. Keep its stable ID unique and give it a display name, for example `Bow`.
2. Let Unity finish importing and compiling. The editor produces `Assets/DeucarianGeneratedKeys/WeaponKey/ProjectWeapons.g.cs` and its generated assembly definition.
3. Configure the runtime owner once, then use the generated key in code or select the same definition from a serialized field dropdown.

Configure WeaponHost with the existing WeaponRuntime, its source callback and declared slots. Its runtime catalog must include the weapon. The runtime owner still ticks cadence and dispatches attack/projectile intents.

After creating the `Bow` definition, a caller can use:

```csharp
using Deucarian.WeaponSystems;
using Deucarian.Generated;
using UnityEngine;

public sealed class GeneratedKeyExample : MonoBehaviour
{
    [SerializeField] private WeaponHost weapons;
    [SerializeField] private WeaponKey definition = ProjectWeapons.Bow;

    public WeaponEquipResult Equip(WeaponSlotKey slot) => weapons.Equip(slot, definition);
}
```

The `definition` field exposes existing `WeaponKey` choices in the Inspector. A direct code call uses the same typed value:

```csharp
weapons.Equip(slot, ProjectWeapons.Bow);
```

The caller retains a typed identity, without a reference to the definition asset. Misspelled generated members and keys from another domain fail compilation. A valid key does not configure a scene or add the definition to its runtime catalog; follow [Simple usage](Documentation~/SimpleUsage.md) for scope setup.

**Updating definitions:** edit the source asset. Changing its display name changes the generated member after regeneration, so update old code references. Existing serialized selections retain their stable ID. Deleting a definition removes its member and marks serialized selections as missing. Duplicate IDs or generated names must be corrected at the source. Renaming only the asset file leaves its display name and ID unchanged.

**Assemblies and source control:** callers with their own asmdef reference `Deucarian.GeneratedKeys.WeaponKey` in addition to the package assemblies they use; `Assembly-CSharp` sees it automatically. Commit source assets, generated `.g.cs`, generated `.asmdef` files and their `.meta` files together. Edit source definitions instead of generated files.

**If a key is missing or stale:** reimport a source definition and let Unity finish compilation. Check that the asset is under `Assets`, its name/ID are valid and automatic generation has not been disabled by a test harness. Inspector and build validation report missing selections and stale generated output. Custom bundle/content pipelines should invoke the shared validator for their additional content.

[Shared generation, serialization and build-validation guide](https://github.com/Deucarian/Editor/blob/develop/Documentation~/TypedKeys.md).

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

## Simple typed usage

See [Simple usage](Documentation~/SimpleUsage.md) for the short caller, Inspector selections and one-time scoped setup.
