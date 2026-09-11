namespace Deucarian.WeaponSystems.Samples.SimpleUsage
{
    [WeaponSlotKeySet]
    public static class Slots
    {
        public static WeaponSlotKey Primary => new Definition();
        private sealed class Definition : WeaponSlotKey
        {
            public Definition() : base("sample.primary") { }
        }
    }
    [WeaponKeySet]
    public static class Weapons
    {
        public static WeaponKey Bow => new Definition();
        private sealed class Definition : WeaponKey
        {
            public Definition() : base("sample.bow") { }
        }
    }
}
