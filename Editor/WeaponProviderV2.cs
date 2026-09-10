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
    internal sealed class WeaponProviderV2View
    {
        private static readonly string[] DetailPages =
        {
            "Overview",
            "Stats",
            "Attack",
            "Presentation",
            "Balance",
            "References",
            "Advanced"
        };

        public void Draw(
            GameContentAuthoringSurfaceContext context,
            WeaponAuthoringState draft,
            WeaponGameContentPreviewController previewController,
            WeaponProviderV2State state)
        {
            if (context == null || draft == null || state == null)
                return;

            IReadOnlyList<WeaponProviderV2ListItem> items = WeaponProviderV2ListItem.Build(context.AuthoredItems);
            WeaponAuthoringSession.EnsureDefaultMode(context, state, items);
            WeaponAuthoringSession.EnsureEditingState(context, state);
            WeaponAuthoringPreview.TrackPreviewSource(context, state, previewController);

            GameContentAuthoringWorkbench.Draw(
                context,
                () => WeaponAuthoringLibrary.DrawWeaponList(context, state, items),
                () => DrawDetailOrWizard(context, draft, state),
                () => WeaponAuthoringPreview.DrawPreviewLab(context, draft, state));
        }

        private static void DrawDetailOrWizard(GameContentAuthoringSurfaceContext context, WeaponAuthoringState draft, WeaponProviderV2State state)
        {
            state.DetailScroll = EditorGUILayout.BeginScrollView(state.DetailScroll);
            if (state.Creating)
                WeaponAuthoringWizard.DrawCreateWizard(context, draft, state);
            else
                DrawSelectedWeapon(context, state);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawSelectedWeapon(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state)
        {
            WeaponDefinitionAsset asset = context.SelectedItem == null ? null : context.SelectedItem.Asset as WeaponDefinitionAsset;
            if (asset == null || state.EditingState == null || state.EditingContext == null)
            {
                DeucarianEditorTextGUI.LabelField("Select a weapon to edit.", DeucarianEditorStyles.MutedLabel);
                return;
            }

            WeaponAuthoringState edit = state.EditingState;
            string fingerprint = WeaponAuthoringDraft.BuildStateFingerprint(edit);
            GameContentAuthoringValidationResult validation = WeaponDefinitionAssetCreator.ValidateForUpdate(edit, asset);
            state.EditingContext.Capture(fingerprint, validation);

            WeaponAuthoringFields.DrawHeader(edit.DisplayName, edit.WeaponId, WeaponAuthoringSummary.BuildWeaponChips(edit, validation));
            GameContentAuthoringCommand command = GameContentAuthoringCommandBar.Draw(
                GameContentAuthoringWorkbenchMode.Edit,
                validation.IsValid,
                state.EditingContext.IsDirty,
                "Save",
                state.LastEditResult == null ? state.EditingContext.StatusMessage : state.LastEditResult.Message);
            WeaponAuthoringSession.HandleEditCommand(context, state, asset, command);

            state.DetailPage = DeucarianEditorSegmentedControl.DrawPageChips(state.DetailPage, DetailPages);
            GUILayout.Space(DeucarianEditorSpacing.Small);
            switch (Mathf.Clamp(state.DetailPage, 0, DetailPages.Length - 1))
            {
                case 0:
                    WeaponAuthoringFields.DrawOverview(context, edit, context.SelectedItem, false);
                    break;
                case 1:
                    WeaponAuthoringFields.DrawStats(context, edit);
                    break;
                case 2:
                    WeaponAuthoringFields.DrawAttack(context, edit);
                    break;
                case 3:
                    WeaponAuthoringFields.DrawPresentation(context, edit);
                    break;
                case 4:
                    WeaponAuthoringFields.DrawBalance(context, edit);
                    break;
                case 5:
                    WeaponAuthoringFields.DrawReferences(context, context.SelectedItem);
                    break;
                default:
                    WeaponAuthoringFields.DrawAdvanced(context, edit, context.SelectedItem, asset);
                    break;
            }

            GameContentAuthoringProviderGUI.DrawValidationIssues(
                validation,
                GameContentAuthoringValidationSummaryStyle.Counts);
        }

        public static GameContentAuthoringActionPreview BuildWeaponActionPreview(WeaponAuthoringState state, WeaponProviderV2State previewState = null)
        {
            return WeaponAuthoringPreview.BuildWeaponActionPreview(state, previewState);
        }

        public static WeaponAuthoringState FromWeaponAsset(WeaponDefinitionAsset asset)
        {
            return WeaponAuthoringDraft.FromWeaponAsset(asset);
        }

        public static string BuildStateFingerprint(WeaponAuthoringState state)
        {
            return WeaponAuthoringDraft.BuildStateFingerprint(state);
        }

        public static string GetWeaponTypeLabel(WeaponAuthoringState state)
        {
            return WeaponAuthoringSummary.GetWeaponTypeLabel(state);
        }
    }
}
