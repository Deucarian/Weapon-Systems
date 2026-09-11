using System;
using Deucarian.Attacks.Authoring;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using Deucarian.WeaponSystems.Authoring;

namespace Deucarian.WeaponSystems.Editor
{
    internal static class WeaponToolkitAuthoring
    {
        internal static VisualElement Create(GameContentAuthoringSurfaceContext context)
        {
            var asset = context.SelectedItem?.Asset as WeaponDefinitionAsset;
            return GameContentToolkitDraftEditor.Create(context,
                () => asset == null ? new WeaponAuthoringState() : WeaponAuthoringDraft.FromWeaponAsset(asset),
                WeaponAuthoringDraft.BuildStateFingerprint,
                state => asset == null ? WeaponAuthoringSession.ValidateDraft(state) : WeaponDefinitionAssetCreator.ValidateForUpdate(state, asset),
                state => asset == null ? WeaponDefinitionAssetCreator.CreateAssets(state) : WeaponDefinitionAssetCreator.UpdateExistingAsset(asset, state),
                (root, state) =>
                {
                    Fields(root, state, context);
                    GameContentToolkitPreview.Add(root, () => state.Prefab,
                        playback => WeaponAuthoringPreview.BuildWeaponActionPreview(state),
                        () => WeaponGameContentPreviewSummaries.BuildWeaponRows(state));
                });
        }
        private static void Fields(VisualElement root, WeaponAuthoringState state, GameContentAuthoringSurfaceContext context)
        {
            var form = new DeucarianEditorWorkspaceForm(root);
            form.Text("DisplayName", "Name", () => state.DisplayName, value => state.DisplayName = value);
            form.Asset("Attack", "Attack", typeof(AttackDefinitionAsset), () => state.Attack, value => state.Attack = (AttackDefinitionAsset)value);
            form.IntegerWithSlider("CooldownTicks", "Cooldown (ticks)", 0, 120, () => state.CooldownTicks, value => state.CooldownTicks = value);
            form.NumberWithSlider("Range", "Range", 0, 20, () => state.Range, value => state.Range = value);
            form.Enum("FireMode", "Fire Mode", () => state.FireMode, value => state.FireMode = value);
            var advanced = form.Section("Advanced data", true);
            advanced.Text("WeaponId", "Weapon Id", () => state.WeaponId, value => state.WeaponId = value);
            advanced.Asset("Icon", "Icon", typeof(Sprite), () => state.Icon, value => state.Icon = (Sprite)value);
            advanced.Text("TagsCsv", "Tags Csv", () => state.TagsCsv, value => state.TagsCsv = value);
            advanced.Text("OutputRoot", "Output Root", () => state.OutputRoot, value => state.OutputRoot = value);
            advanced.Text("ProjectileDefinitionId", "Projectile Definition Id", () => state.ProjectileDefinitionId, value => state.ProjectileDefinitionId = value);
            advanced.Integer("BuildCost", "Build Cost", () => state.BuildCost, value => state.BuildCost = value);
            advanced.Integer("BurstCount", "Burst Count", () => state.BurstCount, value => state.BurstCount = value);
            advanced.Integer("VolleyCount", "Volley Count", () => state.VolleyCount, value => state.VolleyCount = value);
            advanced.Number("SpreadDegrees", "Spread Degrees", () => state.SpreadDegrees, value => state.SpreadDegrees = value);
            advanced.Text("TargetingRoleId", "Targeting Role Id", () => state.TargetingRoleId, value => state.TargetingRoleId = value);
            advanced.Text("MuzzleRoleId", "Muzzle Role Id", () => state.MuzzleRoleId, value => state.MuzzleRoleId = value);
            advanced.Text("UpgradeGroupId", "Upgrade Group Id", () => state.UpgradeGroupId, value => state.UpgradeGroupId = value);
            advanced.Asset("Prefab", "Prefab", typeof(GameObject), () => state.Prefab, value => state.Prefab = (GameObject)value);
            advanced.Asset("PlacementAudio", "Placement Audio", typeof(AudioClip), () => state.PlacementAudio, value => state.PlacementAudio = (AudioClip)value);
            advanced.Asset("PlacementVfxPrefab", "Placement Vfx Prefab", typeof(GameObject), () => state.PlacementVfxPrefab, value => state.PlacementVfxPrefab = (GameObject)value);

        }
    }
}
