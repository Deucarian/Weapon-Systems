using System.Globalization;
using Deucarian.Attacks.Authoring;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.WeaponSystems.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.WeaponSystems.Tests
{
    public sealed class WeaponAuthoringCompositionTests
    {
        [Test]
        public void AssetMappingCreatesIndependentDraftsAndRetainsCadenceAndPresentation()
        {
            var model = new GameObject("authoring-model");
            var source = new WeaponAuthoringState
            {
                WeaponId = "weapon.composition",
                DisplayName = "Composition",
                FireMode = WeaponFireMode.Projectile,
                ProjectileDefinitionId = "projectile.composition",
                CooldownTicks = 7,
                Range = 14.5f,
                BurstCount = 2,
                VolleyCount = 3,
                SpreadDegrees = 12.5f,
                BuildCost = 40,
                TargetingRoleId = "strongest",
                MuzzleRoleId = "secondary",
                UpgradeGroupId = "upgrade.composition",
                Prefab = model
            };
            var asset = WeaponDefinitionAssetCreator.BuildTransient(source);
            try
            {
                var first = WeaponAuthoringDraft.FromWeaponAsset(asset);
                var second = WeaponAuthoringDraft.FromWeaponAsset(asset);
                Assert.That(first.ProjectileDefinitionId, Is.EqualTo("projectile.composition"));
                Assert.That(first.BurstCount * first.VolleyCount, Is.EqualTo(6));
                Assert.That(first.TargetingRoleId, Is.EqualTo("strongest"));
                Assert.That(first.MuzzleRoleId, Is.EqualTo("secondary"));
                Assert.That(first.Prefab, Is.SameAs(model));
                first.CooldownTicks = 90;
                first.Range = 100;
                first.Prefab = null;
                Assert.That(second.CooldownTicks, Is.EqualTo(7));
                Assert.That(second.Range, Is.EqualTo(14.5f));
                Assert.That(asset.Stats.CooldownTicks, Is.EqualTo(7));
                Assert.That(asset.Presentation.Prefab, Is.SameAs(model));
            }
            finally
            {
                WeaponDefinitionAssetCreator.DestroyTransient(asset);
                Object.DestroyImmediate(model);
            }
        }

        [Test]
        public void FingerprintIsCultureIndependentAndDetectsCadenceEdits()
        {
            var draft = new WeaponAuthoringState { Range = 12.5f, SpreadDegrees = 8.25f };
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                string saved = WeaponAuthoringDraft.BuildStateFingerprint(draft);
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("nl-NL");
                Assert.That(WeaponAuthoringDraft.BuildStateFingerprint(draft), Is.EqualTo(saved));
                draft.VolleyCount++;
                Assert.That(WeaponAuthoringDraft.BuildStateFingerprint(draft), Is.Not.EqualTo(saved));
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Test]
        public void PreviewControlsDoNotAlterDraftOrAnotherProviderSession()
        {
            var draft = new WeaponAuthoringState { WeaponId = "weapon.preview-isolation" };
            var first = new WeaponProviderV2State { PreviewPlaying = true, PreviewMuted = false };
            var second = new WeaponProviderV2State { PreviewPlaying = true };
            string fingerprint = WeaponAuthoringDraft.BuildStateFingerprint(draft);
            var preview = WeaponAuthoringPreview.BuildWeaponActionPreview(draft, first);
            first.StopPreview();
            Assert.That(second.PreviewPlaying, Is.True);
            Assert.That(preview.Mode, Is.EqualTo(GameContentAuthoringActionPreviewMode.Static));
            Assert.That(WeaponAuthoringDraft.BuildStateFingerprint(draft), Is.EqualTo(fingerprint));
        }
    }
}
