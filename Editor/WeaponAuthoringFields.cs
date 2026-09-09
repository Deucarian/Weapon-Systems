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
    internal static class WeaponAuthoringFields
    {
        internal static void DrawHeader(string title, string subtitle, IReadOnlyList<DeucarianEditorStatusChip> chips)
        {
            EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(title) ? "Weapon" : title, DeucarianEditorStyles.SectionTitle);
            if (!string.IsNullOrWhiteSpace(subtitle))
                EditorGUILayout.LabelField(subtitle, DeucarianEditorStyles.MutedLabel);
            DeucarianEditorStatusChipRow.Draw(chips);
        }

        internal static void DrawOverview(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state, GameContentLibraryItem selectedItem, bool creating)
        {
            state.WeaponId = context.Authoring.DrawTextField("Stable ID", state.WeaponId);
            state.DisplayName = context.Authoring.DrawTextField("Display Name", state.DisplayName);
            state.Icon = DrawObjectField("Icon", state.Icon);
            state.TagsCsv = context.Authoring.DrawTextField("Tags", state.TagsCsv);
            if (creating)
                state.OutputRoot = context.Authoring.DrawOutputRootField(state.OutputRoot);

            DrawSummaryRows(
                WeaponAuthoringSummary.Row("Type", WeaponAuthoringSummary.GetWeaponTypeLabel(state)),
                WeaponAuthoringSummary.Row("Assigned Attack", state.Attack == null ? "Not assigned" : state.Attack.DisplayName + " (" + state.Attack.Id + ")"),
                WeaponAuthoringSummary.Row("Summary", WeaponAuthoringSummary.BuildHumanSummary(state)),
                WeaponAuthoringSummary.Row("Used By", selectedItem == null ? "New draft" : WeaponAuthoringSummary.BuildReverseReferenceSummary(selectedItem)));
        }

        internal static void DrawStats(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state)
        {
            state.FireMode = context.Authoring.DrawEnumPopup("Fire Mode", state.FireMode);
            state.CooldownTicks = context.Authoring.DrawIntField("Cooldown Ticks", state.CooldownTicks);
            state.Range = context.Authoring.DrawFloatField("Range", state.Range);
            state.BuildCost = context.Authoring.DrawIntField("Build Cost", state.BuildCost);
            state.BurstCount = context.Authoring.DrawIntField("Burst Count", state.BurstCount);
            state.VolleyCount = context.Authoring.DrawIntField("Volley Count", state.VolleyCount);
            state.SpreadDegrees = context.Authoring.DrawFloatField("Spread Degrees", state.SpreadDegrees);
            state.TargetingRoleId = context.Authoring.DrawTextField("Targeting Role", state.TargetingRoleId);
            state.MuzzleRoleId = context.Authoring.DrawTextField("Muzzle Role", state.MuzzleRoleId);
        }

        internal static void DrawAttack(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state)
        {
            state.Attack = DrawObjectField("Assigned Attack", state.Attack);
            if (state.FireMode == WeaponFireMode.Projectile)
                state.ProjectileDefinitionId = context.Authoring.DrawTextField("Projectile ID Override", state.ProjectileDefinitionId);

            DrawSummaryRows(WeaponGameContentPreviewSummaries.BuildAttackRows(state));
        }

        internal static void DrawPresentation(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state)
        {
            state.Prefab = DrawObjectField("Prefab / Model", state.Prefab);
            state.PlacementVfxPrefab = DrawObjectField("Placement VFX", state.PlacementVfxPrefab);
            state.PlacementAudio = DrawObjectField("Placement Audio", state.PlacementAudio);
            DrawSummaryRows(
                WeaponAuthoringSummary.Row("Model", state.Prefab == null ? "Missing" : state.Prefab.name),
                WeaponAuthoringSummary.Row("Placement VFX", state.PlacementVfxPrefab == null ? "Not assigned" : state.PlacementVfxPrefab.name),
                WeaponAuthoringSummary.Row("Placement Audio", state.PlacementAudio == null ? "Not assigned" : state.PlacementAudio.name));
        }

        internal static void DrawBalance(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state)
        {
            state.UpgradeGroupId = context.Authoring.DrawTextField("Upgrade Group", state.UpgradeGroupId);
            DrawSummaryRows(
                WeaponAuthoringSummary.Row("Cost", state.BuildCost.ToString(CultureInfo.InvariantCulture)),
                WeaponAuthoringSummary.Row("Cadence", WeaponAuthoringSummary.BuildCadenceLabel(state)),
                WeaponAuthoringSummary.Row("Range", WeaponAuthoringSummary.FormatFloat(state.Range)),
                WeaponAuthoringSummary.Row("Attack DPS", WeaponAuthoringSummary.BuildDpsEstimate(state)),
                WeaponAuthoringSummary.Row("Upgrade Hook", string.IsNullOrWhiteSpace(state.UpgradeGroupId) ? "Not assigned" : state.UpgradeGroupId));
        }

        internal static void DrawReferences(GameContentAuthoringSurfaceContext context, GameContentLibraryItem selectedItem)
        {
            if (selectedItem == null || selectedItem.ReverseReferences.Count == 0)
            {
                EditorGUILayout.LabelField("No authored references found.", DeucarianEditorStyles.MutedLabel);
                return;
            }

            for (int i = 0; i < selectedItem.ReverseReferences.Count; i++)
            {
                GameContentLibraryReference reference = selectedItem.ReverseReferences[i];
                context.Authoring.DrawInlineCard(() =>
                {
                    EditorGUILayout.LabelField(reference.Target.DisplayName, DeucarianEditorStyles.SectionTitle);
                    EditorGUILayout.LabelField(reference.Target.Category + " - " + reference.Target.Id, DeucarianEditorStyles.MutedLabel);
                });
            }
        }

        internal static void DrawAdvanced(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state, GameContentLibraryItem selectedItem, WeaponDefinitionAsset asset)
        {
            context.Authoring.DrawFoldoutCard("weapon-v2-advanced-paths", "Raw Asset Data", null, () =>
            {
                DrawSummaryRows(
                    WeaponAuthoringSummary.Row("Path", selectedItem == null ? "Draft" : selectedItem.Path),
                    WeaponAuthoringSummary.Row("Stats Section", asset == null || asset.Stats == null ? "Missing" : asset.Stats.name),
                    WeaponAuthoringSummary.Row("Presentation Section", asset == null || asset.Presentation == null ? "Missing" : asset.Presentation.name),
                    WeaponAuthoringSummary.Row("Output Root", state.OutputRoot));
            }, false);
        }

        internal static void DrawSummaryRows(params GameContentAuthoringPreviewRow[] rows)
        {
            GameContentAuthoringProviderGUI.DrawSummaryRows((IReadOnlyList<GameContentAuthoringPreviewRow>)rows, true);
        }

        internal static void DrawSummaryRows(IReadOnlyList<GameContentAuthoringPreviewRow> rows)
        {
            GameContentAuthoringProviderGUI.DrawSummaryRows(rows, true);
        }

        private static T DrawObjectField<T>(string label, T value) where T : UnityEngine.Object
        {
            T next = value;
            DeucarianEditorFieldRow.Draw(label, () =>
            {
                next = (T)EditorGUILayout.ObjectField(value, typeof(T), false);
                if (DeucarianEditorMiniToolbar.PingButton(next))
                    GUI.FocusControl(null);
            });
            return next;
        }
    }
}
