# Simple usage

Copy the reference example into your project, add SimpleUsageExample and assign its scoped host references. Its serialized definition fields use the same typed keys as code.

Configure WeaponHost once with the existing WeaponRuntime, a current source snapshot callback, and `new[] { Slots.Primary }`. Register the weapon definition in the runtime catalog. The runtime owner still ticks cadence and dispatches attack/projectile intents. Re-equipping the same weapon preserves cadence. For authored WeaponDefinitionAssets, use the generated `Deucarian.Generated.ProjectWeapons` keys instead of maintaining a second set.

Definitions are authored once in SampleDefinitions.cs where applicable; the caller never invents an ID. Replace the sample set with your project's central definitions. A selected key proves its identity and payload type; startup still needs to bind that definition in the correct scope. Missing configuration reports how to fix it. Dynamic targets and choices are issued by their owner instead of selected from a definition dropdown.
```csharp
using UnityEngine;

namespace Deucarian.WeaponSystems.Samples.SimpleUsage
{
    public sealed class SimpleUsageExample : MonoBehaviour
    {
        [SerializeField] private WeaponHost weapons;
        [SerializeField] private WeaponSlotKey slot = Slots.Primary;
        [SerializeField] private WeaponKey weapon = Weapons.Bow;
        public WeaponEquipResult Equip() => weapons.Equip(slot, weapon);
        public bool Unequip() => weapons.Unequip(slot);
    }
}
```
