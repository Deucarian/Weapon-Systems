using System;
using System.Linq;
using Deucarian.Editor.Definitions;
using Deucarian.WeaponSystems.Authoring;
using Deucarian.Attacks.Authoring;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WeaponSystems.Editor.Definitions
{
    public sealed class WeaponDefinitionSchema : DeucarianSerializedDefinitionSchema<WeaponDefinitionAsset, WeaponDefinitionSpec>
    {
        public override string Id => "weapons";
        public override string DisplayName => "Weapons";
        protected override string IdPath => "_id";
        protected override string NamePath => "_displayName";
        public override void ValidateAssetReady(ScriptableObject asset)
        {
            base.ValidateAssetReady(asset);
            var issues = WeaponDefinitionValidator.Validate((WeaponDefinitionAsset)asset).Issues.Where(x => x.IsError).Select(x => x.Path + ": " + x.Message).ToArray();
            if (issues.Length > 0) throw new InvalidOperationException("Complete Weapons definition '" + asset.name + "' in Definitions: " + string.Join("; ", issues));
        }
        public override void RefreshCatalog(bool validateOnly = false)
        {
            var definitions = AssetDatabase.FindAssets("t:WeaponDefinitionAsset", new[] { "Assets" })
                .Select(x => AssetDatabase.LoadAssetAtPath<WeaponDefinitionAsset>(AssetDatabase.GUIDToAssetPath(x)))
                .Where(x => x != null).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            if (definitions.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("Weapon definition IDs must be unique.");
            DeucarianDefinitionCatalog.Update<WeaponDefinitionCatalog>("Assets/DeucarianDefinitions/Resources/Deucarian/Definitions/WeaponDefinitionCatalog.asset", "definitions", definitions, validateOnly);
        }
        [MenuItem("Assets/Create/Deucarian/Weapons/Weapon Definition")]
        private static void CreateDefinition() { Selection.activeObject = DeucarianDefinitionSync.Create(new WeaponDefinitionSchema(), "NewWeapon"); }
    }
}
