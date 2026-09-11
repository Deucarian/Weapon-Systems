using System;
using UnityEngine;
namespace Deucarian.WeaponSystems
{
    public sealed class WeaponEquipTrigger : MonoBehaviour
    {
        [SerializeField] private WeaponHost host;
        [SerializeField] private WeaponSlotKey slot;
        [SerializeField] private WeaponKey weapon;
        public WeaponEquipResult LastResult { get; private set; }
        private WeaponHost Host => host != null ? host : throw new InvalidOperationException("Assign a configured WeaponHost to WeaponEquipTrigger.");
        public void Equip() => LastResult = Host.Equip(slot, weapon);
        public void Unequip() => Host.Unequip(slot);
    }
}
