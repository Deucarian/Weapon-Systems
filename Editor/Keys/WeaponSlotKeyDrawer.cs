using System;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.WeaponSystems.Editor
{
    [CustomPropertyDrawer(typeof(WeaponSlotKey), true)]
    public sealed class WeaponSlotKeyDrawer : DeucarianKeyDrawer
    {
        public override Type KeyType => typeof(WeaponSlotKey);
        public override Type DefinitionSetAttribute => typeof(WeaponSlotKeySetAttribute);
        public override string SetupHint => "Select an existing WeaponSlotKey; declare reusable keys once in a [WeaponSlotKeySet] class.";
    }
}
