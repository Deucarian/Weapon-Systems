// <deucarian-definition schema="attacks" />
// Editable declaration. Use the package definition editor or edit the values below.
namespace Deucarian.ProjectDefinitions.Definition_attacks
{
    public static class Definition_WeaponSampleStrike
    {
        public static global::Deucarian.Attacks.Editor.Definitions.AttackDefinitionSpec Value =>
        // definition-value
        new global::Deucarian.Attacks.Editor.Definitions.AttackDefinitionSpec
        {
            BalancingNotes = "",
            Delivery = new global::Deucarian.Attacks.Editor.Definitions.AttackDeliveryDefinitionSpec
            {
                BeamVfxPrefab = null,
                Homing = false,
                HomingTurnRate = 180f,
                ImpactVfxPrefab = null,
                MaxHits = 1,
                Mode = global::Deucarian.Attacks.Authoring.AttackRecipeDeliveryMode.Projectile,
                PierceCount = 0,
                ProjectileDefinitionId = "projectile.example.basic",
                ProjectileLifetimeTicks = 120,
                ProjectilePrefab = null,
                ProjectileSpawnableId = "projectile.example.basic",
                ProjectileSpeed = 8f,
                Radius = 1.5f,
                TickIntervalSeconds = 0.5f,
            },
            Icon = null,
            Id = "d1acaf3a3db6443fb7eb45b9f304ebe4",
            Mechanics = new global::Deucarian.Attacks.Editor.Definitions.AttackMechanicsDefinitionSpec
            {
                CooldownTicks = 20,
                DamageAmount = 10f,
                DamageTypeId = "398c67c8ad154863bb832525f5f71e64",
                Range = 6f,
            },
            Name = "WeaponSampleStrike",
            Presentation = new global::Deucarian.Attacks.Editor.Definitions.AttackPresentationDefinitionSpec
            {
                Events = new global::Deucarian.Attacks.Editor.Definitions.AttackPresentationEventSpec[]
                {
                },
            },
            StatusEffects = new global::Deucarian.Attacks.Editor.Definitions.AttackStatusEffectsDefinitionSpec
            {
                StatusEffects = new global::Deucarian.Attacks.Editor.Definitions.AttackStatusEffectSpec[]
                {
                },
            },
            Tags = new global::System.String[]
            {
            },
            Targeting = new global::Deucarian.Attacks.Editor.Definitions.AttackTargetingDefinitionSpec
            {
                MaxTargets = 1,
                Mode = global::Deucarian.Attacks.Authoring.AttackRecipeTargetingMode.Nearest,
                RequiresLineOfSight = false,
            },
            UpgradeHookId = "",
        };
        // end-definition-value
    }
}
