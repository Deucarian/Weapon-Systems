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
    internal sealed class WeaponProviderV2State
    {
        public string SearchText = string.Empty;
        public bool Creating;
        public int DetailPage;
        public int WizardStep;
        public Vector2 ListScroll;
        public Vector2 DetailScroll;
        public Vector2 PreviewScroll;
        public bool PreviewMuted = true;
        public bool PreviewLoop = true;
        public float PreviewSpeed = 1f;
        public bool PreviewPlaying = true;
        public GameContentAuthoringActionPreviewRenderMode PreviewRenderMode = GameContentAuthoringActionPreviewRenderMode.Game;
        public double PreviewStartTime;
        public float PausedNormalizedTime = 0.5f;
        public string ActivePreviewKey = string.Empty;
        public string PreviewStatus = "Preview idle";
        public WeaponAuthoringState EditingState;
        public GameContentAuthoringObjectEditorContext EditingContext;
        public GameContentCreationResult LastEditResult;

        public void StopPreview()
        {
            PreviewPlaying = false;
            PreviewStartTime = 0d;
            PausedNormalizedTime = 0.5f;
            PreviewStatus = "Preview stopped";
        }

        public void BeginCreate()
        {
            Creating = true;
            DetailScroll = Vector2.zero;
            WizardStep = 0;
            ClearEditingState();
            PreviewStatus = "Previewing draft weapon";
        }

        public void ResetProviderSession()
        {
            Creating = false;
            DetailPage = 0;
            WizardStep = 0;
            ListScroll = Vector2.zero;
            DetailScroll = Vector2.zero;
            PreviewScroll = Vector2.zero;
            ActivePreviewKey = string.Empty;
            PreviewStatus = "Preview idle";
            ClearEditingState();
        }

        public void SetPreviewSource(string key, WeaponGameContentPreviewController controller)
        {
            key = key ?? string.Empty;
            if (string.Equals(ActivePreviewKey, key, StringComparison.Ordinal))
                return;

            controller?.Stop();
            ActivePreviewKey = key;
            PreviewPlaying = true;
            PreviewStartTime = EditorApplication.timeSinceStartup;
            PausedNormalizedTime = 0f;
            PreviewStatus = "Previewing";
        }

        public void ClearEditingState()
        {
            EditingState = null;
            EditingContext = null;
            LastEditResult = null;
        }
    }

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

        private static readonly string[] WizardSteps =
        {
            "Identity",
            "Stats",
            "Attack",
            "Presentation",
            "Balance",
            "Review"
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
            EnsureDefaultMode(context, state, items);
            EnsureEditingState(context, state);
            TrackPreviewSource(context, state, previewController);

            GameContentAuthoringWorkbench.Draw(
                context,
                () => DrawWeaponList(context, state, items),
                () => DrawDetailOrWizard(context, draft, state),
                () => DrawPreviewLab(context, draft, state));
        }

        private static void EnsureDefaultMode(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state, IReadOnlyList<WeaponProviderV2ListItem> items)
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

        private static void EnsureEditingState(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state)
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

            state.EditingState = FromWeaponAsset(selected);
            string fingerprint = BuildStateFingerprint(state.EditingState);
            state.EditingContext = new GameContentAuthoringObjectEditorContext(context.SelectedItem, fingerprint);
            state.LastEditResult = null;
        }

        private static void TrackPreviewSource(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state, WeaponGameContentPreviewController previewController)
        {
            string key = state.Creating
                ? "__draft_weapon__"
                : context.SelectedItem == null
                    ? string.Empty
                    : context.SelectedItem.Key;
            state.SetPreviewSource(key, previewController);
        }

        private static void DrawWeaponList(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state, IReadOnlyList<WeaponProviderV2ListItem> items)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Weapons", DeucarianEditorStyles.SectionTitle);
                GUILayout.FlexibleSpace();
                if (DeucarianEditorMiniToolbar.Button("Refresh", true, GUILayout.Width(62f), GUILayout.Height(22f)))
                    context.RefreshLibrary();
            }

            state.SearchText = DeucarianEditorSearchField.Draw(state.SearchText, "Search weapons", GUILayout.ExpandWidth(true));
            if (DeucarianEditorButtons.Secondary("Create New", true, GUILayout.Height(24f)))
            {
                state.BeginCreate();
                context.ClearSelection();
                context.RequestRepaint();
            }

            GUILayout.Space(DeucarianEditorSpacing.Small);
            state.ListScroll = EditorGUILayout.BeginScrollView(state.ListScroll);
            int shown = 0;
            for (int i = 0; i < items.Count; i++)
            {
                WeaponProviderV2ListItem item = items[i];
                if (!item.Matches(state.SearchText))
                    continue;

                shown++;
                DrawWeaponCard(context, state, item);
            }

            if (shown == 0)
                EditorGUILayout.LabelField(items.Count == 0 ? "No authored weapons found." : "No weapons match the current search.", DeucarianEditorStyles.MutedLabel);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawWeaponCard(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state, WeaponProviderV2ListItem item)
        {
            bool selected = !state.Creating && context.IsSelected(item.Source);
            var chips = new[]
            {
                new DeucarianEditorStatusChip(item.TypeLabel, DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(item.ReadinessLabel, item.ReadinessStatus),
                new DeucarianEditorStatusChip(item.AttackLabel, item.HasAttack ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error, item.AttackTooltip),
                new DeucarianEditorStatusChip(item.HasPrefab ? "Model" : "NoModel", item.HasPrefab ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Warning),
                new DeucarianEditorStatusChip(item.HasPresentation ? "VFX/Aud" : "Quiet", item.HasPresentation ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Disabled)
            };

            bool clicked = DeucarianEditorCompactObjectCard.Draw(
                item.DisplayName,
                item.StableId,
                selected,
                chips,
                () =>
                {
                    if (DeucarianEditorMiniToolbar.PingButton(item.Source.Asset))
                        GUI.FocusControl(null);
                },
                null,
                GUILayout.ExpandWidth(true));

            if (clicked && item.Source != null)
            {
                state.Creating = false;
                state.DetailScroll = Vector2.zero;
                context.SelectItem(item.Source);
                Event.current.Use();
            }
        }

        private static void DrawDetailOrWizard(GameContentAuthoringSurfaceContext context, WeaponAuthoringState draft, WeaponProviderV2State state)
        {
            state.DetailScroll = EditorGUILayout.BeginScrollView(state.DetailScroll);
            if (state.Creating)
                DrawCreateWizard(context, draft, state);
            else
                DrawSelectedWeapon(context, state);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawSelectedWeapon(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state)
        {
            WeaponDefinitionAsset asset = context.SelectedItem == null ? null : context.SelectedItem.Asset as WeaponDefinitionAsset;
            if (asset == null || state.EditingState == null || state.EditingContext == null)
            {
                EditorGUILayout.LabelField("Select a weapon to edit.", DeucarianEditorStyles.MutedLabel);
                return;
            }

            WeaponAuthoringState edit = state.EditingState;
            string fingerprint = BuildStateFingerprint(edit);
            GameContentAuthoringValidationResult validation = WeaponDefinitionAssetCreator.ValidateForUpdate(edit, asset);
            state.EditingContext.Capture(fingerprint, validation);

            DrawHeader(edit.DisplayName, edit.WeaponId, BuildWeaponChips(edit, validation));
            GameContentAuthoringCommand command = GameContentAuthoringCommandBar.Draw(
                GameContentAuthoringWorkbenchMode.Edit,
                validation.IsValid,
                state.EditingContext.IsDirty,
                "Save",
                state.LastEditResult == null ? state.EditingContext.StatusMessage : state.LastEditResult.Message);
            HandleEditCommand(context, state, asset, command);

            state.DetailPage = DeucarianEditorSegmentedControl.DrawPageChips(state.DetailPage, DetailPages);
            GUILayout.Space(DeucarianEditorSpacing.Small);
            switch (Mathf.Clamp(state.DetailPage, 0, DetailPages.Length - 1))
            {
                case 0:
                    DrawOverview(context, edit, context.SelectedItem, false);
                    break;
                case 1:
                    DrawStats(context, edit);
                    break;
                case 2:
                    DrawAttack(context, edit);
                    break;
                case 3:
                    DrawPresentation(context, edit);
                    break;
                case 4:
                    DrawBalance(context, edit);
                    break;
                case 5:
                    DrawReferences(context, context.SelectedItem);
                    break;
                default:
                    DrawAdvanced(context, edit, context.SelectedItem, asset);
                    break;
            }

            DrawValidationIssues(validation);
        }

        private static void HandleEditCommand(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state, WeaponDefinitionAsset asset, GameContentAuthoringCommand command)
        {
            if (command == GameContentAuthoringCommand.Revert)
            {
                state.EditingState = FromWeaponAsset(asset);
                string fingerprint = BuildStateFingerprint(state.EditingState);
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
                state.EditingState = FromWeaponAsset(asset);
                string fingerprint = BuildStateFingerprint(state.EditingState);
                state.EditingContext.Accept(fingerprint, "Saved");
                context.RefreshLibrary();
            }
        }

        private static void DrawCreateWizard(GameContentAuthoringSurfaceContext context, WeaponAuthoringState draft, WeaponProviderV2State state)
        {
            WeaponDefinitionAsset preview = WeaponDefinitionAssetCreator.BuildTransient(draft);
            GameContentAuthoringValidationResult validation;
            try
            {
                validation = WeaponDefinitionAssetCreator.ValidateForCreation(draft, preview);
            }
            finally
            {
                WeaponDefinitionAssetCreator.DestroyTransient(preview);
            }

            DrawHeader("New Weapon", draft.WeaponId, BuildWeaponChips(draft, validation));
            GameContentAuthoringCommand command = GameContentAuthoringCommandBar.Draw(GameContentAuthoringWorkbenchMode.Create, validation.IsValid, true, "Create");
            if (command == GameContentAuthoringCommand.Create)
            {
                GameContentCreationResult result = WeaponDefinitionAssetCreator.CreateAssets(draft);
                context.Authoring.SetCreationResult(result);
                if (result != null && result.Succeeded)
                {
                    state.Creating = false;
                    context.RefreshLibrary();
                }
            }

            state.WizardStep = DeucarianEditorWizardHeader.Draw(state.WizardStep, WizardSteps);
            GUILayout.Space(DeucarianEditorSpacing.Small);
            switch (Mathf.Clamp(state.WizardStep, 0, WizardSteps.Length - 1))
            {
                case 0:
                    DrawOverview(context, draft, null, true);
                    break;
                case 1:
                    DrawStats(context, draft);
                    break;
                case 2:
                    DrawAttack(context, draft);
                    break;
                case 3:
                    DrawPresentation(context, draft);
                    break;
                case 4:
                    DrawBalance(context, draft);
                    break;
                default:
                    DrawReview(context, draft, validation);
                    break;
            }

            DrawValidationIssues(validation);
            context.Authoring.DrawCreationResult();
        }

        private static void DrawHeader(string title, string subtitle, IReadOnlyList<DeucarianEditorStatusChip> chips)
        {
            EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(title) ? "Weapon" : title, DeucarianEditorStyles.SectionTitle);
            if (!string.IsNullOrWhiteSpace(subtitle))
                EditorGUILayout.LabelField(subtitle, DeucarianEditorStyles.MutedLabel);
            DeucarianEditorStatusChipRow.Draw(chips);
        }

        private static void DrawOverview(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state, GameContentLibraryItem selectedItem, bool creating)
        {
            state.WeaponId = context.Authoring.DrawTextField("Stable ID", state.WeaponId);
            state.DisplayName = context.Authoring.DrawTextField("Display Name", state.DisplayName);
            state.Icon = DrawObjectField("Icon", state.Icon);
            state.TagsCsv = context.Authoring.DrawTextField("Tags", state.TagsCsv);
            if (creating)
                state.OutputRoot = context.Authoring.DrawOutputRootField(state.OutputRoot);

            DrawSummaryRows(
                Row("Type", GetWeaponTypeLabel(state)),
                Row("Assigned Attack", state.Attack == null ? "Not assigned" : state.Attack.DisplayName + " (" + state.Attack.Id + ")"),
                Row("Summary", BuildHumanSummary(state)),
                Row("Used By", selectedItem == null ? "New draft" : BuildReverseReferenceSummary(selectedItem)));
        }

        private static void DrawStats(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state)
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

        private static void DrawAttack(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state)
        {
            state.Attack = DrawObjectField("Assigned Attack", state.Attack);
            if (state.FireMode == WeaponFireMode.Projectile)
                state.ProjectileDefinitionId = context.Authoring.DrawTextField("Projectile ID Override", state.ProjectileDefinitionId);

            DrawSummaryRows(WeaponGameContentPreviewSummaries.BuildAttackRows(state));
        }

        private static void DrawPresentation(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state)
        {
            state.Prefab = DrawObjectField("Prefab / Model", state.Prefab);
            state.PlacementVfxPrefab = DrawObjectField("Placement VFX", state.PlacementVfxPrefab);
            state.PlacementAudio = DrawObjectField("Placement Audio", state.PlacementAudio);
            DrawSummaryRows(
                Row("Model", state.Prefab == null ? "Missing" : state.Prefab.name),
                Row("Placement VFX", state.PlacementVfxPrefab == null ? "Not assigned" : state.PlacementVfxPrefab.name),
                Row("Placement Audio", state.PlacementAudio == null ? "Not assigned" : state.PlacementAudio.name));
        }

        private static void DrawBalance(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state)
        {
            state.UpgradeGroupId = context.Authoring.DrawTextField("Upgrade Group", state.UpgradeGroupId);
            DrawSummaryRows(
                Row("Cost", state.BuildCost.ToString(CultureInfo.InvariantCulture)),
                Row("Cadence", BuildCadenceLabel(state)),
                Row("Range", FormatFloat(state.Range)),
                Row("Attack DPS", BuildDpsEstimate(state)),
                Row("Upgrade Hook", string.IsNullOrWhiteSpace(state.UpgradeGroupId) ? "Not assigned" : state.UpgradeGroupId));
        }

        private static void DrawReferences(GameContentAuthoringSurfaceContext context, GameContentLibraryItem selectedItem)
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

        private static void DrawAdvanced(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state, GameContentLibraryItem selectedItem, WeaponDefinitionAsset asset)
        {
            context.Authoring.DrawFoldoutCard("weapon-v2-advanced-paths", "Raw Asset Data", null, () =>
            {
                DrawSummaryRows(
                    Row("Path", selectedItem == null ? "Draft" : selectedItem.Path),
                    Row("Stats Section", asset == null || asset.Stats == null ? "Missing" : asset.Stats.name),
                    Row("Presentation Section", asset == null || asset.Presentation == null ? "Missing" : asset.Presentation.name),
                    Row("Output Root", state.OutputRoot));
            }, false);
        }

        private static void DrawReview(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state, GameContentAuthoringValidationResult validation)
        {
            DrawSummaryRows(
                Row("Folder", state.OutputRoot.TrimEnd('/') + "/" + Sanitize(state.WeaponId)),
                Row("Root Asset", Sanitize(state.WeaponId) + "_WeaponDefinition.asset"),
                Row("Sections", "Stats, Presentation"),
                Row("Validation", validation.ErrorCount.ToString(CultureInfo.InvariantCulture) + " blocker(s), " + validation.WarningCount.ToString(CultureInfo.InvariantCulture) + " warning(s)"));
        }

        private static void DrawPreviewLab(GameContentAuthoringSurfaceContext context, WeaponAuthoringState draft, WeaponProviderV2State state)
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
            context.Preview.DrawSummaryRow("Target", "Preview target at " + FormatFloat(state.Range) + " range");
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

        private static IReadOnlyList<DeucarianEditorStatusChip> BuildWeaponChips(WeaponAuthoringState state, GameContentAuthoringValidationResult validation)
        {
            return new[]
            {
                new DeucarianEditorStatusChip(GetWeaponTypeLabel(state), DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(validation != null && validation.ErrorCount > 0 ? "Blocked" : validation != null && validation.WarningCount > 0 ? "Warnings" : "Ready", validation != null && validation.ErrorCount > 0 ? DeucarianEditorStatus.Error : validation != null && validation.WarningCount > 0 ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Success),
                new DeucarianEditorStatusChip(state.Attack == null ? "NoAttack" : "Attack", state.Attack == null ? DeucarianEditorStatus.Error : DeucarianEditorStatus.Success),
                new DeucarianEditorStatusChip(state.Prefab == null ? "NoModel" : "Model", state.Prefab == null ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Success)
            };
        }

        private static void DrawValidationIssues(GameContentAuthoringValidationResult validation)
        {
            if (validation == null || validation.Issues.Count == 0)
                return;

            var messages = new List<string>();
            for (int i = 0; i < validation.Issues.Count; i++)
            {
                GameContentAuthoringValidationIssue issue = validation.Issues[i];
                messages.Add((string.IsNullOrWhiteSpace(issue.Path) ? string.Empty : issue.Path + ": ") + issue.Message);
            }

            DeucarianEditorStatus status = validation.ErrorCount > 0
                ? DeucarianEditorStatus.Error
                : validation.WarningCount > 0
                    ? DeucarianEditorStatus.Warning
                    : DeucarianEditorStatus.Info;
            DeucarianEditorStatusPanel.DrawValidationCard(BuildValidationSummary(validation), messages, status);
        }

        private static string BuildValidationSummary(GameContentAuthoringValidationResult validation)
        {
            if (validation == null)
                return string.Empty;
            return validation.ErrorCount.ToString(CultureInfo.InvariantCulture) + " blocker(s), "
                + validation.WarningCount.ToString(CultureInfo.InvariantCulture) + " warning(s).";
        }

        private static IReadOnlyList<GameContentAuthoringPreviewRow> WeaponRows(params GameContentAuthoringPreviewRow[] rows)
        {
            return rows;
        }

        private static void DrawSummaryRows(params GameContentAuthoringPreviewRow[] rows)
        {
            DrawSummaryRows((IReadOnlyList<GameContentAuthoringPreviewRow>)rows);
        }

        private static void DrawSummaryRows(IReadOnlyList<GameContentAuthoringPreviewRow> rows)
        {
            if (rows == null || rows.Count == 0)
                return;
            for (int i = 0; i < rows.Count; i++)
                DeucarianEditorFieldRow.Draw(rows[i].Label, () => EditorGUILayout.LabelField(rows[i].Value ?? string.Empty, DeucarianEditorStyles.MutedLabel));
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

        private static GameContentAuthoringPreviewRow Row(string label, string value)
        {
            return new GameContentAuthoringPreviewRow(label, value);
        }

        private static string BuildHumanSummary(WeaponAuthoringState state)
        {
            return GetWeaponTypeLabel(state) + ", " + FormatFloat(state.Range) + " range, " + BuildCadenceLabel(state);
        }

        private static string BuildReverseReferenceSummary(GameContentLibraryItem item)
        {
            if (item == null || item.ReverseReferences.Count == 0)
                return "0 set(s), 0 pack(s), 0 upgrade(s)";
            int sets = 0;
            int packs = 0;
            int upgrades = 0;
            for (int i = 0; i < item.ReverseReferences.Count; i++)
            {
                GameContentLibraryKind kind = item.ReverseReferences[i].Target.Kind;
                if (kind == GameContentLibraryKind.ContentSet) sets++;
                else if (kind == GameContentLibraryKind.ContentPack) packs++;
                else if (kind == GameContentLibraryKind.Upgrade) upgrades++;
            }

            return sets.ToString(CultureInfo.InvariantCulture) + " set(s), "
                + packs.ToString(CultureInfo.InvariantCulture) + " pack(s), "
                + upgrades.ToString(CultureInfo.InvariantCulture) + " upgrade(s)";
        }

        public static string GetWeaponTypeLabel(WeaponAuthoringState state)
        {
            if (state == null)
                return "Custom";
            if (state.Attack != null && state.Attack.Delivery != null)
            {
                switch (state.Attack.Delivery.Mode)
                {
                    case AttackRecipeDeliveryMode.Projectile:
                        return state.Attack.Delivery.Homing ? "Homing" : "Projectile";
                    case AttackRecipeDeliveryMode.Hitscan:
                        return "Beam";
                    case AttackRecipeDeliveryMode.Area:
                        return "AOE";
                    case AttackRecipeDeliveryMode.Aura:
                        return "Aura";
                }
            }

            return state.FireMode == WeaponFireMode.Projectile ? "Projectile" : "Direct";
        }

        private static string BuildCadenceLabel(WeaponAuthoringState state)
        {
            if (state == null)
                return string.Empty;
            return state.CooldownTicks.ToString(CultureInfo.InvariantCulture) + " ticks, "
                + state.BurstCount.ToString(CultureInfo.InvariantCulture) + "x"
                + state.VolleyCount.ToString(CultureInfo.InvariantCulture);
        }

        private static string BuildDpsEstimate(WeaponAuthoringState state)
        {
            if (state == null || state.Attack == null || state.Attack.Mechanics == null)
                return "Assign an attack for estimate";
            float shots = Mathf.Max(1, state.BurstCount) * Mathf.Max(1, state.VolleyCount);
            float cooldown = Mathf.Max(1, state.CooldownTicks);
            float damagePerTick = state.Attack.Mechanics.DamageAmount * shots / cooldown;
            return FormatFloat(damagePerTick * 60f) + " damage / 60 ticks";
        }

        private static string Sanitize(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? "NewWeapon" : id.Trim().Replace('\\', '-').Replace('/', '-').Replace(':', '-');
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }

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

    internal sealed class WeaponProviderV2ListItem
    {
        private WeaponProviderV2ListItem(GameContentLibraryItem source, WeaponDefinitionAsset asset)
        {
            Source = source;
            Asset = asset;
            StableId = source == null ? string.Empty : source.Id;
            DisplayName = source == null ? "Weapon" : source.DisplayName;
            TypeLabel = GetTypeLabel(asset);
            HasAttack = asset != null && asset.Stats != null && asset.Stats.Attack != null;
            HasPrefab = asset != null && asset.Presentation != null && asset.Presentation.Prefab != null;
            HasPresentation = asset != null && asset.Presentation != null && (asset.Presentation.PlacementAudio != null || asset.Presentation.PlacementVfxPrefab != null);
            ReadinessStatus = source != null && source.ErrorCount > 0 ? DeucarianEditorStatus.Error : source != null && source.WarningCount > 0 ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Success;
            ReadinessLabel = source == null ? "Draft" : source.ErrorCount > 0 ? "Blocked" : source.WarningCount > 0 ? "Warnings" : "Ready";
            AttackLabel = HasAttack ? "Attack" : "NoAttack";
            AttackTooltip = HasAttack ? asset.Stats.Attack.DisplayName : "Assign an AttackDefinition asset.";
        }

        public GameContentLibraryItem Source { get; }
        public WeaponDefinitionAsset Asset { get; }
        public string StableId { get; }
        public string DisplayName { get; }
        public string TypeLabel { get; }
        public bool HasAttack { get; }
        public bool HasPrefab { get; }
        public bool HasPresentation { get; }
        public string ReadinessLabel { get; }
        public DeucarianEditorStatus ReadinessStatus { get; }
        public string AttackLabel { get; }
        public string AttackTooltip { get; }

        public static IReadOnlyList<WeaponProviderV2ListItem> Build(IReadOnlyList<GameContentLibraryItem> items)
        {
            if (items == null || items.Count == 0)
                return Array.Empty<WeaponProviderV2ListItem>();
            var result = new List<WeaponProviderV2ListItem>();
            for (int i = 0; i < items.Count; i++)
            {
                WeaponDefinitionAsset asset = items[i].Asset as WeaponDefinitionAsset;
                if (asset != null)
                    result.Add(new WeaponProviderV2ListItem(items[i], asset));
            }

            return result;
        }

        public bool Matches(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return true;
            string value = searchText.Trim();
            return Contains(DisplayName, value)
                || Contains(StableId, value)
                || Contains(TypeLabel, value)
                || Contains(AttackTooltip, value)
                || Contains(string.Join(", ", Asset == null ? Array.Empty<string>() : Asset.Tags), value);
        }

        public static string GetTypeLabelForTests(WeaponAuthoringState state)
        {
            return WeaponProviderV2View.GetWeaponTypeLabel(state);
        }

        private static string GetTypeLabel(WeaponDefinitionAsset asset)
        {
            return WeaponProviderV2View.GetWeaponTypeLabel(asset == null ? null : WeaponProviderV2View.FromWeaponAsset(asset));
        }

        private static bool Contains(string text, string value)
        {
            return (text ?? string.Empty).IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
