using System;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.WeaponSystems.Editor
{
    [CustomPropertyDrawer(typeof(WeaponKey), true)]
    public sealed class WeaponKeyDrawer : DeucarianKeyDrawer
    {
        public override Type KeyType => typeof(WeaponKey);
        public override Type DefinitionSetAttribute => typeof(WeaponKeySetAttribute);
        public override string SetupHint => "Select an existing WeaponKey; declare reusable keys once in a [WeaponKeySet] class.";
    }
}
