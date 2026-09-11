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
