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
    internal static class WeaponAuthoringPreview
    {
        internal static void TrackPreviewSource(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state, WeaponGameContentPreviewController previewController)
        {
            string key = state.Creating
                ? "__draft_weapon__"
                : context.SelectedItem == null
                    ? string.Empty
                    : context.SelectedItem.Key;
            state.SetPreviewSource(key, () => previewController?.Stop());
        }

        internal static void DrawPreviewLab(GameContentAuthoringSurfaceContext context, WeaponAuthoringState draft, WeaponProviderV2State state)
        {
            WeaponAuthoringState previewState = state.Creating ? draft : state.EditingState ?? draft;
            if (previewState == null)
                return;

            state.PreviewScroll = EditorGUILayout.BeginScrollView(state.PreviewScroll);
            GameContentPreviewLabRenderer.Draw(context.Preview, new GameContentPreviewLabModel
            {
                Title = "Preview Lab - " + (string.IsNullOrWhiteSpace(previewState.DisplayName) ? "Weapon" : previewState.DisplayName),
                ScopeLabel = WeaponProviderV2PreviewModel.GetScopeLabel(state.Creating, state.EditingContext != null && state.EditingContext.IsDirty),
                PreviewTitle = string.IsNullOrWhiteSpace(previewState.DisplayName) ? "Weapon" : previewState.DisplayName,
                PrimaryAsset = previewState.Prefab,
                EmptyText = "No weapon model assigned. Preview uses a neutral source marker.",
                PreviewOptions = new GameContentAuthoringObjectPreviewOptions
                {
                    MinimumHeight = 220f,
                    ActionPreview = BuildWeaponActionPreview(previewState, state)
                },
                Chips = WeaponProviderV2PreviewModel.BuildChips(previewState, state),
                DrawControls = () => DrawPreviewControls(context, state),
                DrawContext = () => DrawPreviewContext(context, previewState),
                DrawBody = () =>
                {
                    context.Preview.DrawSummaryRows(WeaponGameContentPreviewSummaries.BuildWeaponRows(previewState));
                    context.Preview.DrawSummaryRows(WeaponGameContentPreviewSummaries.BuildAttackRows(previewState));
                }
            });
            EditorGUILayout.EndScrollView();
        }

        private static void DrawPreviewControls(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (DeucarianEditorMiniToolbar.Button(state.PreviewPlaying ? "Pause" : "Play", true, GUILayout.Width(56f), GUILayout.Height(22f)))
                {
                    if (state.PreviewPlaying)
                    {
                        state.PausedNormalizedTime = 0.5f;
                        state.PreviewPlaying = false;
                    }
                    else
                    {
                        state.PreviewStartTime = EditorApplication.timeSinceStartup;
                        state.PreviewPlaying = true;
                    }
                }

                if (DeucarianEditorMiniToolbar.Button("Stop", true, GUILayout.Width(48f), GUILayout.Height(22f)))
                    state.StopPreview();
                if (DeucarianEditorMiniToolbar.Button("Restart", true, GUILayout.Width(62f), GUILayout.Height(22f)))
                {
                    state.PreviewStartTime = EditorApplication.timeSinceStartup;
                    state.PreviewPlaying = true;
                }

                state.PreviewLoop = GUILayout.Toggle(state.PreviewLoop, "Loop", DeucarianEditorStyles.ToolbarButton, GUILayout.Width(52f), GUILayout.Height(22f));
                state.PreviewMuted = GUILayout.Toggle(state.PreviewMuted, "Muted", DeucarianEditorStyles.ToolbarButton, GUILayout.Width(62f), GUILayout.Height(22f));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (DeucarianEditorMiniToolbar.Button("0.5x", Math.Abs(state.PreviewSpeed - 0.5f) > 0.01f, GUILayout.Height(22f)))
                    state.PreviewSpeed = 0.5f;
                if (DeucarianEditorMiniToolbar.Button("1x", Math.Abs(state.PreviewSpeed - 1f) > 0.01f, GUILayout.Height(22f)))
                    state.PreviewSpeed = 1f;
                if (DeucarianEditorMiniToolbar.Button("2x", Math.Abs(state.PreviewSpeed - 2f) > 0.01f, GUILayout.Height(22f)))
                    state.PreviewSpeed = 2f;
                GUILayout.Space(6f);
                bool game = state.PreviewRenderMode == GameContentAuthoringActionPreviewRenderMode.Game;
                if (DeucarianEditorMiniToolbar.Button("Game", !game, GUILayout.Height(22f)))
                    state.PreviewRenderMode = GameContentAuthoringActionPreviewRenderMode.Game;
                if (DeucarianEditorMiniToolbar.Button("Debug", game, GUILayout.Height(22f)))
                    state.PreviewRenderMode = GameContentAuthoringActionPreviewRenderMode.Debug;
            }

            context.Preview.SetStatus(state.PreviewStatus);
        }

        private static void DrawPreviewContext(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state)
        {
            context.Preview.DrawSummaryRow("Source", state.Prefab == null ? "Neutral weapon source" : state.Prefab.name);
            context.Preview.DrawSummaryRow("Attack", state.Attack == null ? "No assigned attack" : state.Attack.DisplayName + " - " + state.Attack.Id);
            context.Preview.DrawSummaryRow("Target", "Preview target at " + WeaponAuthoringSummary.FormatFloat(state.Range) + " range");
        }

        public static GameContentAuthoringActionPreview BuildWeaponActionPreview(WeaponAuthoringState state, WeaponProviderV2State previewState = null)
        {
            if (state == null)
                return null;

            AttackDeliveryDefinitionAsset delivery = state.Attack == null ? null : state.Attack.Delivery;
            var preview = new GameContentAuthoringActionPreview
            {
                PrimaryAsset = state.Prefab,
                SourcePrefab = state.Prefab,
                Mode = GetPreviewMode(delivery),
                RenderMode = previewState == null ? GameContentAuthoringActionPreviewRenderMode.Game : previewState.PreviewRenderMode,
                Playing = previewState == null || previewState.PreviewPlaying,
                Loop = previewState == null || previewState.PreviewLoop,
                Speed = previewState == null ? 1f : previewState.PreviewSpeed,
                StartTime = previewState == null ? EditorApplication.timeSinceStartup : previewState.PreviewStartTime,
                StaticNormalizedTime = previewState == null ? 0.5f : previewState.PausedNormalizedTime,
                Muted = previewState == null || previewState.PreviewMuted,
                Label = string.IsNullOrWhiteSpace(state.DisplayName) ? "Weapon Preview" : state.DisplayName,
                DeliveryTypeLabel = delivery == null ? state.FireMode.ToString() : delivery.Mode.ToString(),
                SourceContextLabel = state.MuzzleRoleId,
                TargetContextLabel = state.Attack == null ? "Missing attack" : state.Attack.DisplayName,
                ProjectilePrefab = delivery == null ? null : delivery.ProjectilePrefab,
                BeamVfxPrefab = delivery == null ? null : delivery.BeamVfxPrefab,
                ImpactVfxPrefab = delivery == null ? null : delivery.ImpactVfxPrefab,
                FireVfxPrefab = state.PlacementVfxPrefab,
                DurationSeconds = 2.4f
            };
            preview.Roles.Add(new GameContentAuthoringActionPreviewRole("Source", string.IsNullOrWhiteSpace(state.DisplayName) ? "Weapon" : state.DisplayName, state.Prefab));
            preview.Roles.Add(new GameContentAuthoringActionPreviewRole("Attack", state.Attack == null ? "Missing attack" : state.Attack.DisplayName, state.Attack));
            preview.Roles.Add(new GameContentAuthoringActionPreviewRole("Target", "Preview target"));
            return preview;
        }

        private static GameContentAuthoringActionPreviewMode GetPreviewMode(AttackDeliveryDefinitionAsset delivery)
        {
            if (delivery == null)
                return GameContentAuthoringActionPreviewMode.Static;
            switch (delivery.Mode)
            {
                case AttackRecipeDeliveryMode.Projectile:
                    return GameContentAuthoringActionPreviewMode.Projectile;
                case AttackRecipeDeliveryMode.Hitscan:
                    return GameContentAuthoringActionPreviewMode.Hitscan;
                case AttackRecipeDeliveryMode.Area:
                    return GameContentAuthoringActionPreviewMode.Area;
                case AttackRecipeDeliveryMode.Aura:
                    return GameContentAuthoringActionPreviewMode.Aura;
                default:
                    return GameContentAuthoringActionPreviewMode.Static;
            }
        }
    }
}
