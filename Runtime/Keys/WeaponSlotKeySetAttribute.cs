using System;

namespace Deucarian.WeaponSystems
{
    /// <summary>Marks an authoritative set of named WeaponSlotKey fields or properties for the Inspector.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class WeaponSlotKeySetAttribute : Attribute { }
}
