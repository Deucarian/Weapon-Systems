using System;
using Deucarian.Attacks.Authoring;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.WeaponSystems.Editor;
using Deucarian.WeaponSystems.Authoring;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.WeaponSystems.Tests
{
    public sealed class WeaponSystemsAuthoringTests
    {
        [Test]
        public void WeaponDefinitionAssetConvertsToRuntimeDefinition()
        {
            AttackDefinitionAsset attack = AttackDefinitionAsset.CreateTransient(
                "attack.weapon.authoring.projectile",
                "Projectile Attack",
                AttackRecipeDeliveryMode.Projectile,
                "damage.test",
                6,
                4,
                9,
                AttackRecipeTargetingMode.Nearest,
                projectileDefinitionId: "projectile.weapon.authoring",
                projectileSpawnableId: "projectile.weapon.authoring");
            GameObject prefab = new GameObject("weapon-authoring-prefab");
            WeaponDefinitionAsset weapon = WeaponDefinitionAsset.CreateTransient(
                "weapon.authoring.projectile",
                "Projectile Weapon",
                WeaponFireMode.Projectile,
                attack,
                12,
                9f,
                prefab: prefab);

            try
            {
                WeaponDefinitionValidationReport report = WeaponDefinitionValidator.Validate(weapon, WeaponDefinitionValidationOptions.AssetCreation);
                WeaponDefinition runtime = weapon.ToRuntimeDefinition();

                Assert.IsTrue(report.IsValid);
                Assert.AreEqual("weapon.authoring.projectile", runtime.Id.Value);
                Assert.AreEqual(WeaponFireMode.Projectile, runtime.FireMode);
                Assert.AreEqual("attack.weapon.authoring.projectile", runtime.AttackDefinitionId.Value);
                Assert.AreEqual("projectile.weapon.authoring", runtime.ProjectileDefinitionId.Value);
                Assert.AreEqual(12, runtime.CooldownTicks);
            }
            finally
            {
                Object.DestroyImmediate(prefab);
                Object.DestroyImmediate(weapon.Presentation);
                Object.DestroyImmediate(weapon.Stats);
                Object.DestroyImmediate(weapon);
                Object.DestroyImmediate(attack.Presentation);
                Object.DestroyImmediate(attack.StatusEffects);
                Object.DestroyImmediate(attack.Delivery);
                Object.DestroyImmediate(attack.Targeting);
                Object.DestroyImmediate(attack.Mechanics);
                Object.DestroyImmediate(attack);
            }
        }

        [Test]
        public void WeaponValidationRejectsMissingAttackAndWarnsForMissingPrefabForCreation()
        {
            WeaponDefinitionAsset weapon = WeaponDefinitionAsset.CreateTransient(
                "weapon.authoring.invalid",
                "Invalid Weapon",
                WeaponFireMode.DirectAttack,
                null,
                6,
                8f);

            try
            {
                WeaponDefinitionValidationReport report = WeaponDefinitionValidator.Validate(weapon, WeaponDefinitionValidationOptions.AssetCreation);

                Assert.IsFalse(report.IsValid);
                Assert.That(FindIssue(report, "Stats.Attack", WeaponDefinitionValidationSeverity.Error), Is.True);
                Assert.That(FindIssue(report, "Presentation.Prefab", WeaponDefinitionValidationSeverity.Warning), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(weapon.Presentation);
                Object.DestroyImmediate(weapon.Stats);
                Object.DestroyImmediate(weapon);
            }
        }

        [Test]
        public void WeaponValidationAllowsDataOnlyPrefabWhenAttackIsValid()
        {
            AttackDefinitionAsset attack = AttackDefinitionAsset.CreateTransient(
                "attack.weapon.authoring.data-only",
                "Data Only Attack",
                AttackRecipeDeliveryMode.Hitscan,
                "damage.test",
                5,
                0,
                8,
                AttackRecipeTargetingMode.Nearest);
            WeaponDefinitionAsset weapon = WeaponDefinitionAsset.CreateTransient(
                "weapon.authoring.data-only",
                "Data Only Weapon",
                WeaponFireMode.DirectAttack,
                attack,
                10,
                8f);

            try
            {
                WeaponDefinitionValidationReport report = WeaponDefinitionValidator.Validate(weapon, WeaponDefinitionValidationOptions.AssetCreation);

                Assert.IsTrue(report.IsValid);
                Assert.That(FindIssue(report, "Presentation.Prefab", WeaponDefinitionValidationSeverity.Warning), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(weapon.Presentation);
                Object.DestroyImmediate(weapon.Stats);
                Object.DestroyImmediate(weapon);
                Object.DestroyImmediate(attack.Presentation);
                Object.DestroyImmediate(attack.StatusEffects);
                Object.DestroyImmediate(attack.Delivery);
                Object.DestroyImmediate(attack.Targeting);
                Object.DestroyImmediate(attack.Mechanics);
                Object.DestroyImmediate(attack);
            }
        }

        [Test]
        public void WeaponPreviewSummaryHandlesMissingOptionalAssets()
        {
            var state = new WeaponAuthoringState
            {
                Attack = null,
                Prefab = null,
                PlacementAudio = null,
                PlacementVfxPrefab = null
            };

            Assert.DoesNotThrow(() =>
            {
                Assert.That(WeaponGameContentPreviewSummaries.BuildWeaponRows(state).Count, Is.GreaterThan(0));
                Assert.That(WeaponGameContentPreviewSummaries.BuildAttackRows(state).Count, Is.GreaterThan(0));
                Assert.That(WeaponGameContentPreviewSummaries.BuildWarnings(state).Count, Is.GreaterThan(0));
                StringAssert.Contains("no attack assigned", WeaponGameContentPreviewSummaries.PreviewWeaponFire(state));
            });
        }

        [Test]
        public void WeaponAuthoringProviderRegistersWithSharedWindow()
        {
            Assert.IsTrue(GameContentAuthoringProviderRegistry.IsProviderRegistered("com.deucarian.weapon-systems.weapon"));
        }

        [Test]
        public void WeaponAuthoringProviderUsesCustomV2Surface()
        {
            var provider = new WeaponAuthoringProvider();

            Assert.That(provider, Is.InstanceOf<IGameContentAuthoringSurfaceProvider>());
            Assert.That(provider.ProviderId, Is.EqualTo("com.deucarian.weapon-systems.weapon"));
            Assert.That(WeaponProviderV2PreviewModel.ExposesRedundantSelectButton, Is.False);
        }

        [Test]
        public void WeaponProviderV2ListModel_ClassifiesWeaponTypes()
        {
            AttackDefinitionAsset projectile = CreateAttack("attack.weapon.v2.projectile", AttackRecipeDeliveryMode.Projectile);
            AttackDefinitionAsset homing = CreateAttack("attack.weapon.v2.homing", AttackRecipeDeliveryMode.Projectile, homingProjectile: true);
            AttackDefinitionAsset beam = CreateAttack("attack.weapon.v2.beam", AttackRecipeDeliveryMode.Hitscan);
            AttackDefinitionAsset area = CreateAttack("attack.weapon.v2.area", AttackRecipeDeliveryMode.Area);
            AttackDefinitionAsset aura = CreateAttack("attack.weapon.v2.aura", AttackRecipeDeliveryMode.Aura);

            try
            {
                Assert.That(WeaponProviderV2ListItem.GetTypeLabelForTests(new WeaponAuthoringState { FireMode = WeaponFireMode.Projectile, Attack = projectile }), Is.EqualTo("Projectile"));
                Assert.That(WeaponProviderV2ListItem.GetTypeLabelForTests(new WeaponAuthoringState { FireMode = WeaponFireMode.Projectile, Attack = homing }), Is.EqualTo("Homing"));
                Assert.That(WeaponProviderV2ListItem.GetTypeLabelForTests(new WeaponAuthoringState { FireMode = WeaponFireMode.DirectAttack, Attack = beam }), Is.EqualTo("Beam"));
                Assert.That(WeaponProviderV2ListItem.GetTypeLabelForTests(new WeaponAuthoringState { FireMode = WeaponFireMode.DirectAttack, Attack = area }), Is.EqualTo("AOE"));
                Assert.That(WeaponProviderV2ListItem.GetTypeLabelForTests(new WeaponAuthoringState { FireMode = WeaponFireMode.DirectAttack, Attack = aura }), Is.EqualTo("Aura"));
                Assert.That(WeaponProviderV2ListItem.GetTypeLabelForTests(new WeaponAuthoringState { FireMode = WeaponFireMode.DirectAttack, Attack = null }), Is.EqualTo("Direct"));
            }
            finally
            {
                DestroyAttack(projectile);
                DestroyAttack(homing);
                DestroyAttack(beam);
                DestroyAttack(area);
                DestroyAttack(aura);
            }
        }

        [Test]
        public void WeaponProviderV2Preview_UsesAssignedAttackAndModel()
        {
            AttackDefinitionAsset attack = CreateAttack("attack.weapon.v2.cursor-ray", AttackRecipeDeliveryMode.Hitscan);
            var prefab = new GameObject("weapon-v2-preview-prefab");
            var state = new WeaponAuthoringState
            {
                WeaponId = "weapon.v2.cursor-beam",
                DisplayName = "Cursor Beam",
                FireMode = WeaponFireMode.DirectAttack,
                Attack = attack,
                Prefab = prefab,
                Range = 11f
            };

            try
            {
                GameContentAuthoringActionPreview preview = WeaponProviderV2View.BuildWeaponActionPreview(state);

                Assert.That(preview, Is.Not.Null);
                Assert.That(preview.PrimaryAsset, Is.SameAs(prefab));
                Assert.That(preview.SourcePrefab, Is.SameAs(prefab));
                Assert.That(preview.Mode, Is.EqualTo(GameContentAuthoringActionPreviewMode.Hitscan));
                Assert.That(preview.TargetContextLabel, Does.Contain("Cursor Ray"));
            }
            finally
            {
                Object.DestroyImmediate(prefab);
                DestroyAttack(attack);
            }
        }

        [Test]
        public void WeaponProviderV2Preview_DraftAndUnsavedExposeCompactChips()
        {
            var state = new WeaponAuthoringState { Attack = null, Prefab = null };
            var previewState = new WeaponProviderV2State
            {
                Creating = true,
                PreviewMuted = true,
                PreviewRenderMode = GameContentAuthoringActionPreviewRenderMode.Game
            };

            Assert.That(WeaponProviderV2PreviewModel.GetScopeLabel(true, false), Is.EqualTo("Draft"));
            Assert.That(WeaponProviderV2PreviewModel.GetScopeLabel(false, true), Is.EqualTo("Unsaved"));
            AssertChip(WeaponProviderV2PreviewModel.BuildChips(state, previewState), "NoAttack", DeucarianEditorStatus.Error);
            AssertChip(WeaponProviderV2PreviewModel.BuildChips(state, previewState), "Placeholder", DeucarianEditorStatus.Warning);
        }

        [Test]
        public void WeaponDefinitionAssetCreator_UpdateExistingAssetSavesSelectedWeaponSections()
        {
            const string folder = "Assets/WeaponAuthoringUpdateTests";
            AttackDefinitionAsset firstAttack = CreateAttack("attack.weapon.v2.save.first", AttackRecipeDeliveryMode.Projectile);
            AttackDefinitionAsset secondAttack = CreateAttack("attack.weapon.v2.save.second", AttackRecipeDeliveryMode.Hitscan);
            GameObject firstPrefab = new GameObject("weapon-v2-first-model");
            GameObject secondPrefab = new GameObject("weapon-v2-second-model");
            AssetDatabase.DeleteAsset(folder);
            AssetDatabase.CreateFolder("Assets", "WeaponAuthoringUpdateTests");

            WeaponDefinitionAsset root = ScriptableObject.CreateInstance<WeaponDefinitionAsset>();
            WeaponStatsDefinitionAsset stats = ScriptableObject.CreateInstance<WeaponStatsDefinitionAsset>();
            WeaponPresentationDefinitionAsset presentation = ScriptableObject.CreateInstance<WeaponPresentationDefinitionAsset>();
            stats.Configure(WeaponFireMode.Projectile, firstAttack, "", 12, 8f, 1, 1, 0f, 25, "nearest", "primary");
            presentation.Configure(firstPrefab, null, null);
            root.Configure("weapon.v2.save", "Old Weapon", null, new[] { "old" }, "upgrade.old", stats, presentation, "keep notes");
            AssetDatabase.CreateAsset(root, folder + "/weapon.v2.save_WeaponDefinition.asset");
            GameContentAuthoringEditorAssets.AddSubAsset(stats, root, "weapon.v2.save_Stats");
            GameContentAuthoringEditorAssets.AddSubAsset(presentation, root, "weapon.v2.save_Presentation");
            AssetDatabase.SaveAssets();

            try
            {
                var edit = new WeaponAuthoringState
                {
                    WeaponId = "weapon.v2.save",
                    DisplayName = "Saved Weapon",
                    TagsCsv = "saved, beam",
                    FireMode = WeaponFireMode.DirectAttack,
                    Attack = secondAttack,
                    CooldownTicks = 6,
                    Range = 13f,
                    BuildCost = 42,
                    BurstCount = 2,
                    VolleyCount = 3,
                    SpreadDegrees = 12f,
                    TargetingRoleId = "strongest",
                    MuzzleRoleId = "muzzle.alt",
                    UpgradeGroupId = "upgrade.saved",
                    Prefab = secondPrefab
                };

                GameContentCreationResult result = WeaponDefinitionAssetCreator.UpdateExistingAsset(root, edit);

                Assert.That(result.Succeeded, Is.True);
                Assert.That(root.DisplayName, Is.EqualTo("Saved Weapon"));
                Assert.That(root.BalancingNotes, Is.EqualTo("keep notes"));
                Assert.That(root.Stats.Attack, Is.SameAs(secondAttack));
                Assert.That(root.Stats.CooldownTicks, Is.EqualTo(6));
                Assert.That(root.Stats.Range, Is.EqualTo(13f));
                Assert.That(root.Stats.VolleyCount, Is.EqualTo(3));
                Assert.That(root.Presentation.Prefab, Is.SameAs(secondPrefab));
            }
            finally
            {
                AssetDatabase.DeleteAsset(folder);
                Object.DestroyImmediate(firstPrefab);
                Object.DestroyImmediate(secondPrefab);
                DestroyAttack(firstAttack);
                DestroyAttack(secondAttack);
            }
        }

        [Test]
        public void WeaponDefinitionAssetCreator_UpdateValidationBlocksMissingAttackButAllowsQuietPresentation()
        {
            WeaponDefinitionAsset root = ScriptableObject.CreateInstance<WeaponDefinitionAsset>();
            var edit = new WeaponAuthoringState
            {
                WeaponId = "weapon.v2.invalid",
                DisplayName = "Invalid Weapon",
                Attack = null,
                Prefab = null,
                PlacementAudio = null,
                PlacementVfxPrefab = null
            };

            try
            {
                GameContentAuthoringValidationResult validation = WeaponDefinitionAssetCreator.ValidateForUpdate(edit, root);

                Assert.That(validation.IsValid, Is.False);
                Assert.That(FindIssue(validation, "Stats.Attack", GameContentAuthoringValidationSeverity.Error), Is.True);
                Assert.That(FindIssue(validation, "Presentation.Prefab", GameContentAuthoringValidationSeverity.Warning), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void WeaponDuplicateIdsAreDetectedInAssets()
        {
            const string folder = "Assets/WeaponAuthoringDuplicateTests";
            const string id = "weapon.authoring.duplicate";
            AssetDatabase.DeleteAsset(folder);
            AssetDatabase.CreateFolder("Assets", "WeaponAuthoringDuplicateTests");
            WeaponDefinitionAsset asset = ScriptableObject.CreateInstance<WeaponDefinitionAsset>();
            asset.Configure(id, "Duplicate Weapon", null, Array.Empty<string>(), string.Empty, null, null);
            AssetDatabase.CreateAsset(asset, folder + "/DuplicateWeapon.asset");
            AssetDatabase.SaveAssets();

            try
            {
                Assert.IsTrue(GameContentAuthoringEditorAssets.HasDuplicateId<WeaponDefinitionAsset>(id, candidate => candidate.Id));
            }
            finally
            {
                AssetDatabase.DeleteAsset(folder);
            }
        }

        private static bool FindIssue(WeaponDefinitionValidationReport report, string path, WeaponDefinitionValidationSeverity? severity = null)
        {
            for (int i = 0; i < report.Issues.Count; i++)
                if (report.Issues[i].Path == path && (!severity.HasValue || report.Issues[i].Severity == severity.Value))
                    return true;
            return false;
        }

        private static bool FindIssue(GameContentAuthoringValidationResult report, string path, GameContentAuthoringValidationSeverity severity)
        {
            for (int i = 0; i < report.Issues.Count; i++)
                if (report.Issues[i].Path == path && report.Issues[i].Severity == severity)
                    return true;
            return false;
        }

        private static void AssertChip(System.Collections.Generic.IReadOnlyList<DeucarianEditorStatusChip> chips, string label, DeucarianEditorStatus status)
        {
            for (int i = 0; i < chips.Count; i++)
                if (chips[i].Label == label && chips[i].Status == status)
                    return;
            Assert.Fail("Expected chip " + label + " with status " + status + ".");
        }

        private static AttackDefinitionAsset CreateAttack(string id, AttackRecipeDeliveryMode mode, bool homingProjectile = false)
        {
            AttackDefinitionAsset attack = AttackDefinitionAsset.CreateTransient(
                id,
                BuildAttackName(id),
                mode,
                "damage.test",
                6,
                5,
                10,
                AttackRecipeTargetingMode.Nearest,
                projectileDefinitionId: id + ".projectile",
                projectileSpawnableId: id + ".projectile");
            if (mode == AttackRecipeDeliveryMode.Projectile && homingProjectile)
                attack.Delivery.ConfigureProjectile(id + ".projectile", id + ".projectile", null, 8f, 60, true);
            return attack;
        }

        private static string BuildAttackName(string id)
        {
            if (id.EndsWith("cursor-ray", StringComparison.Ordinal))
                return "Cursor Ray";
            return id;
        }

        private static void DestroyAttack(AttackDefinitionAsset attack)
        {
            if (attack == null)
                return;
            Object.DestroyImmediate(attack.Presentation);
            Object.DestroyImmediate(attack.StatusEffects);
            Object.DestroyImmediate(attack.Delivery);
            Object.DestroyImmediate(attack.Targeting);
            Object.DestroyImmediate(attack.Mechanics);
            Object.DestroyImmediate(attack);
        }
    }
}
