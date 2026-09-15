using System;
using Deucarian.Editor;
using Deucarian.WeaponSystems.Authoring;

namespace Deucarian.WeaponSystems.Editor
{
    public sealed class WeaponKeySource : DeucarianAssetKeySource<WeaponDefinitionAsset>
    {
        public override Type KeyType => typeof(WeaponKey);
        public override Type DefinitionSetAttribute => typeof(WeaponKeySetAttribute);
        public override string GeneratedClassName => "ProjectWeapons";
        protected override DeucarianKeyChoice ReadDefinition(WeaponDefinitionAsset asset) =>
            new DeucarianKeyChoice(asset.Id, asset.DisplayName);
    }
}
