using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.WeaponSystems.Authoring
{
    /// <summary>Generated project definitions; scene composition supplies the existing runtime's other dependencies.</summary>
    public sealed class WeaponDefinitionCatalog : ScriptableObject
    {
        public const string ResourcePath = "Deucarian/Definitions/WeaponDefinitionCatalog";
        [SerializeField] private WeaponDefinitionAsset[] definitions = Array.Empty<WeaponDefinitionAsset>();
        public IReadOnlyList<WeaponDefinitionAsset> Definitions => Array.AsReadOnly(definitions);
        public static WeaponDefinitionCatalog LoadProject() => Resources.Load<WeaponDefinitionCatalog>(ResourcePath) ??
            throw new InvalidOperationException("Create a Weapon definition in the Definitions editor before loading the project catalog.");
        public WeaponDefinitionAsset Get(WeaponKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key), "Select a definition in the Inspector or pass a generated key.");
            foreach (var definition in definitions) if (definition != null && definition.Id == key.Id) return definition;
            throw new InvalidOperationException("The Weapon catalog does not contain '" + key.Id + "'. Synchronize this definition in the Definitions editor.");
        }
        public WeaponDefinition[] CreateRuntimeDefinitions()
        {
            var result = new WeaponDefinition[definitions.Length];
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < result.Length; i++)
            {
                var definition = definitions[i];
                if (definition == null || !ids.Add(definition.Id)) throw new InvalidOperationException("The Weapon catalog contains a missing or duplicate definition. Synchronize it in the Definitions editor.");
                try { result[i] = definition.ToRuntimeDefinition(); }
                catch (Exception error) { throw new InvalidOperationException("Complete Weapon definition '" + definition.DisplayName + "' in the Definitions editor: " + error.Message, error); }
            }
            return result;
        }
    }
}
