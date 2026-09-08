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
    internal static class WeaponProviderV2PreviewModel
    {
        public const bool ExposesRedundantSelectButton = false;

        public static string GetScopeLabel(bool creating, bool unsaved)
        {
            if (creating)
                return "Draft";
            return unsaved ? "Unsaved" : "Selected";
        }

        public static IReadOnlyList<DeucarianEditorStatusChip> BuildChips(WeaponAuthoringState state, WeaponProviderV2State previewState)
        {
            if (state == null)
                return Array.Empty<DeucarianEditorStatusChip>();
            bool debug = previewState != null && previewState.PreviewRenderMode == GameContentAuthoringActionPreviewRenderMode.Debug;
            return new[]
            {
                new DeucarianEditorStatusChip(debug ? "Debug" : "Game", debug ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(state.Attack == null ? "NoAttack" : "Attack", state.Attack == null ? DeucarianEditorStatus.Error : DeucarianEditorStatus.Success),
                new DeucarianEditorStatusChip(state.Prefab == null ? "Placeholder" : "Model", state.Prefab == null ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Success),
                new DeucarianEditorStatusChip(previewState == null || previewState.PreviewMuted ? "Muted" : "Audio", previewState == null || previewState.PreviewMuted ? DeucarianEditorStatus.Disabled : DeucarianEditorStatus.Success)
            };
        }
    }
}
