namespace Deucarian.WeaponSystems.Samples.DefinitionWorkflow
{
    [WeaponSlotKeySet] public static class SampleWeaponSlots
    {
        public static WeaponSlotKey Primary => new Key();
        private sealed class Key : WeaponSlotKey { public Key() : base("sample.workflow.primary") { } }
    }
}
