using System;
using UnityEngine;

namespace Deucarian.WeaponSystems
{
    /// <summary>A declared Weapon identity. Reuse a named definition or select it in the Inspector.</summary>
    [Serializable]
    public class WeaponKey : IEquatable<WeaponKey>
    {
        [SerializeField] private string definitionId;

        /// <summary>For central definition sets and generated declarations; ordinary callers reuse those keys.</summary>
        protected WeaponKey(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id != id.Trim())
                throw new ArgumentException("A WeaponKey definition needs a non-empty stable ID without surrounding whitespace.", nameof(id));
            definitionId = id;
        }

        public string Id => !string.IsNullOrWhiteSpace(definitionId) ? definitionId :
            throw new InvalidOperationException("No WeaponKey is selected. Select an existing definition in the Inspector or assign a named key from a WeaponKeySet declaration.");
        public bool Equals(WeaponKey other) => other != null && string.Equals(definitionId, other.definitionId, StringComparison.Ordinal);
        public override bool Equals(object other) => other is WeaponKey key && Equals(key);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(definitionId ?? string.Empty);
        public override string ToString() => definitionId ?? string.Empty;
    }
}
