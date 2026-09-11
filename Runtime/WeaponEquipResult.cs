namespace Deucarian.WeaponSystems
{
    public enum WeaponEquipStatus { Equipped, AlreadyEquipped, SourceUnavailable }

    public readonly struct WeaponEquipResult
    {
        public WeaponEquipResult(WeaponEquipStatus status) { Status = status; }
        public WeaponEquipStatus Status { get; }
        public bool Succeeded => Status == WeaponEquipStatus.Equipped || Status == WeaponEquipStatus.AlreadyEquipped;
    }
}
