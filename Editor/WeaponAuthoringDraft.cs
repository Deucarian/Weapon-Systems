using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Deucarian.Attacks.Authoring;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.WeaponSystems.Authoring;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WeaponSystems.Editor
{
    internal static class WeaponAuthoringDraft
    {
        public static WeaponAuthoringState FromWeaponAsset(WeaponDefinitionAsset asset)
        {
            var state = new WeaponAuthoringState();
            if (asset == null)
                return state;

            WeaponStatsDefinitionAsset stats = asset.Stats;
            WeaponPresentationDefinitionAsset presentation = asset.Presentation;
            state.WeaponId = asset.Id;
            state.DisplayName = asset.DisplayName;
            state.Icon = asset.Icon;
            state.TagsCsv = string.Join(", ", asset.Tags);
            state.UpgradeGroupId = asset.UpgradeGroupId;
            state.OutputRoot = "Assets/GameContent/Weapons";
            if (stats != null)
            {
                state.FireMode = stats.FireMode;
                state.Attack = stats.Attack;
                state.ProjectileDefinitionId = stats.ProjectileDefinitionId;
                state.CooldownTicks = stats.CooldownTicks;
                state.Range = stats.Range;
                state.BuildCost = stats.BuildCost;
                state.BurstCount = stats.BurstCount;
                state.VolleyCount = stats.VolleyCount;
                state.SpreadDegrees = stats.SpreadDegrees;
                state.TargetingRoleId = stats.TargetingRoleId;
                state.MuzzleRoleId = stats.MuzzleRoleId;
            }

            if (presentation != null)
            {
                state.Prefab = presentation.Prefab;
                state.PlacementAudio = presentation.PlacementAudio;
                state.PlacementVfxPrefab = presentation.PlacementVfxPrefab;
            }

            return state;
        }

        public static string BuildStateFingerprint(WeaponAuthoringState state)
        {
            if (state == null)
                return string.Empty;

            var builder = new StringBuilder();
            builder.Append(state.WeaponId).Append('|')
                .Append(state.DisplayName).Append('|')
                .Append(GetAssetKey(state.Icon)).Append('|')
                .Append(state.TagsCsv).Append('|')
                .Append(state.FireMode).Append('|')
                .Append(GetAssetKey(state.Attack)).Append('|')
                .Append(state.ProjectileDefinitionId).Append('|')
                .Append(state.CooldownTicks).Append('|')
                .Append(state.Range.ToString("R", CultureInfo.InvariantCulture)).Append('|')
                .Append(state.BuildCost).Append('|')
                .Append(state.BurstCount).Append('|')
                .Append(state.VolleyCount).Append('|')
                .Append(state.SpreadDegrees.ToString("R", CultureInfo.InvariantCulture)).Append('|')
                .Append(state.TargetingRoleId).Append('|')
                .Append(state.MuzzleRoleId).Append('|')
                .Append(state.UpgradeGroupId).Append('|')
                .Append(GetAssetKey(state.Prefab)).Append('|')
                .Append(GetAssetKey(state.PlacementAudio)).Append('|')
                .Append(GetAssetKey(state.PlacementVfxPrefab));
            return builder.ToString();
        }

        private static string GetAssetKey(UnityEngine.Object asset)
        {
            if (asset == null)
                return string.Empty;
            string path = AssetDatabase.GetAssetPath(asset);
            return string.IsNullOrWhiteSpace(path) ? asset.GetInstanceID().ToString(CultureInfo.InvariantCulture) : path;
        }
    }
}
