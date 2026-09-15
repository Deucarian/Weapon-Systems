// <deucarian-definition schema="weapons" />
// Editable declaration. Use the package definition editor or edit the values below.
namespace Deucarian.ProjectDefinitions.Definition_weapons
{
    public static class Definition_WeaponSampleBlaster
    {
        public static global::Deucarian.WeaponSystems.Editor.Definitions.WeaponDefinitionSpec Value =>
        // definition-value
        new global::Deucarian.WeaponSystems.Editor.Definitions.WeaponDefinitionSpec
        {
            BalancingNotes = "",
            Icon = null,
            Id = "cf141da177084f64b6bbf439bf07389a",
            Name = "WeaponSampleBlaster",
            Presentation = new global::Deucarian.WeaponSystems.Editor.Definitions.WeaponPresentationDefinitionSpec
            {
                PlacementAudio = null,
                PlacementVfxPrefab = null,
                Prefab = null,
            },
            Stats = new global::Deucarian.WeaponSystems.Editor.Definitions.WeaponStatsDefinitionSpec
            {
                Attack = global::Deucarian.Editor.Definitions.DeucarianDefinitionAssets.Load<global::Deucarian.Attacks.Authoring.AttackDefinitionAsset>("4806cd4321ec8e14abed7dfe43013d7b", 11400000L),
                BuildCost = 25,
                BurstCount = 1,
                CooldownTicks = 12,
                FireMode = global::Deucarian.WeaponSystems.WeaponFireMode.DirectAttack,
                MuzzleRoleId = "primary",
                ProjectileDefinitionId = "",
                Range = 7f,
                SpreadDegrees = 0f,
                TargetingRoleId = "nearest",
                VolleyCount = 1,
            },
            Tags = new global::System.String[]
            {
            },
            UpgradeGroupId = "",
        };
        // end-definition-value
    }
}
