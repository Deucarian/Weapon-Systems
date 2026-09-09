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
    internal static class WeaponAuthoringSession
    {
        internal static void EnsureDefaultMode(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state, IReadOnlyList<WeaponProviderV2ListItem> items)
        {
            if (items.Count == 0)
            {
                state.Creating = true;
                state.ClearEditingState();
                return;
            }

            if (!state.Creating && context.SelectedItem == null)
            {
                context.SelectItem(items[0].Source);
                context.RequestRepaint();
            }
        }

        internal static void EnsureEditingState(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state)
        {
            if (state.Creating || context.SelectedItem == null)
            {
                state.ClearEditingState();
                return;
            }

            WeaponDefinitionAsset selected = context.SelectedItem.Asset as WeaponDefinitionAsset;
            if (selected == null)
            {
                state.ClearEditingState();
                return;
            }

            if (state.EditingContext != null && string.Equals(state.EditingContext.Key, context.SelectedItem.Key, StringComparison.Ordinal) && state.EditingState != null)
                return;

            state.EditingState = WeaponAuthoringDraft.FromWeaponAsset(selected);
            string fingerprint = WeaponAuthoringDraft.BuildStateFingerprint(state.EditingState);
            state.EditingContext = new GameContentAuthoringObjectEditorContext(context.SelectedItem, fingerprint);
            state.LastEditResult = null;
        }

        internal static void HandleEditCommand(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state, WeaponDefinitionAsset asset, GameContentAuthoringCommand command)
        {
            if (command == GameContentAuthoringCommand.Revert)
            {
                state.EditingState = WeaponAuthoringDraft.FromWeaponAsset(asset);
                string fingerprint = WeaponAuthoringDraft.BuildStateFingerprint(state.EditingState);
                state.EditingContext.Accept(fingerprint, "Reverted");
                state.LastEditResult = null;
                context.RequestRepaint();
                return;
            }

            if (command != GameContentAuthoringCommand.Save)
                return;

            state.LastEditResult = WeaponDefinitionAssetCreator.UpdateExistingAsset(asset, state.EditingState);
            if (state.LastEditResult != null && state.LastEditResult.Succeeded)
            {
                state.EditingState = WeaponAuthoringDraft.FromWeaponAsset(asset);
                string fingerprint = WeaponAuthoringDraft.BuildStateFingerprint(state.EditingState);
                state.EditingContext.Accept(fingerprint, "Saved");
                context.RefreshLibrary();
            }
        }

        internal static void Create(GameContentAuthoringSurfaceContext context, WeaponAuthoringState draft, WeaponProviderV2State state)
        {
            GameContentCreationResult result = WeaponDefinitionAssetCreator.CreateAssets(draft);
            context.Authoring.SetCreationResult(result);
            if (result != null && result.Succeeded)
            {
                state.Creating = false;
                context.RefreshLibrary();
            }
        }

        internal static GameContentAuthoringValidationResult ValidateDraft(WeaponAuthoringState draft)
        {
            WeaponDefinitionAsset preview = WeaponDefinitionAssetCreator.BuildTransient(draft);
            try
            {
                return WeaponDefinitionAssetCreator.ValidateForCreation(draft, preview);
            }
            finally
            {
                WeaponDefinitionAssetCreator.DestroyTransient(preview);
            }
        }
    }
}
