using System;
using System.Collections.Generic;
using System.Globalization;
using Deucarian.Attacks.Authoring;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.WeaponSystems.Authoring;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WeaponSystems.Editor
{
    [InitializeOnLoad]
    internal static class WeaponGameContentAuthoringProviderRegistration
    {
        static WeaponGameContentAuthoringProviderRegistration()
        {
            GameContentAuthoringProviderRegistry.Register(new WeaponAuthoringProvider());
        }
    }

    internal sealed class WeaponAuthoringProvider : IGameContentAuthoringProvider
    {
        private readonly WeaponAuthoringState _state = new WeaponAuthoringState();
        private readonly WeaponGameContentPreviewController _preview = new WeaponGameContentPreviewController();

        public string ProviderId => "com.deucarian.weapon-systems.weapon";
        public string DisplayName => "Tower / Weapon";
        public string Description => "Create a root WeaponDefinition with stats and presentation sections.";
        public int SortOrder => 130;
        public bool Enabled => true;
        public void OnSelected() { }
        public void DrawPreview(GameContentAuthoringPreviewContext context) { _preview.Draw(context, _state); }
        public void StopPreview() { _preview.Stop(); }

        public void Draw(GameContentAuthoringContext context)
        {
            WeaponDefinitionAsset preview = WeaponDefinitionAssetCreator.BuildTransient(_state);
            GameContentAuthoringValidationResult report;
            try
            {
                report = WeaponDefinitionAssetCreator.ValidateForCreation(_state, preview);
            }
            finally
            {
                WeaponDefinitionAssetCreator.DestroyTransient(preview);
            }

            context.DrawSection("Weapon Identity", () =>
            {
                _state.WeaponId = EditorGUILayout.TextField("Stable ID", _state.WeaponId);
                _state.DisplayName = EditorGUILayout.TextField("Display Name", _state.DisplayName);
                _state.Icon = (Sprite)EditorGUILayout.ObjectField("Icon", _state.Icon, typeof(Sprite), false);
                _state.TagsCsv = EditorGUILayout.TextField("Tags", _state.TagsCsv);
                _state.OutputRoot = context.DrawOutputRootField(_state.OutputRoot);
            });

            context.DrawSection("Runtime Stats", () =>
            {
                _state.FireMode = (WeaponFireMode)EditorGUILayout.EnumPopup("Fire Mode", _state.FireMode);
                _state.Attack = (AttackDefinitionAsset)EditorGUILayout.ObjectField("Attack", _state.Attack, typeof(AttackDefinitionAsset), false);
                if (_state.FireMode == WeaponFireMode.Projectile)
                    _state.ProjectileDefinitionId = EditorGUILayout.TextField("Projectile ID Override", _state.ProjectileDefinitionId);
                _state.CooldownTicks = EditorGUILayout.IntField("Cooldown Ticks", _state.CooldownTicks);
                _state.Range = EditorGUILayout.FloatField("Range", _state.Range);
                _state.BuildCost = EditorGUILayout.IntField("Build Cost", _state.BuildCost);
                _state.BurstCount = EditorGUILayout.IntField("Burst Count", _state.BurstCount);
                _state.VolleyCount = EditorGUILayout.IntField("Volley Count", _state.VolleyCount);
                _state.SpreadDegrees = EditorGUILayout.FloatField("Spread Degrees", _state.SpreadDegrees);
                _state.TargetingRoleId = EditorGUILayout.TextField("Targeting Role", _state.TargetingRoleId);
                _state.MuzzleRoleId = EditorGUILayout.TextField("Muzzle Role", _state.MuzzleRoleId);
                _state.UpgradeGroupId = EditorGUILayout.TextField("Upgrade Group", _state.UpgradeGroupId);
            });

            context.DrawSection("Presentation", () =>
            {
                _state.Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab / Model", _state.Prefab, typeof(GameObject), false);
                _state.PlacementAudio = (AudioClip)EditorGUILayout.ObjectField("Placement Audio", _state.PlacementAudio, typeof(AudioClip), false);
                _state.PlacementVfxPrefab = (GameObject)EditorGUILayout.ObjectField("Placement VFX", _state.PlacementVfxPrefab, typeof(GameObject), false);
            });

            context.DrawSection("Preview", () =>
            {
                foreach (string line in WeaponDefinitionAssetCreator.GetPreviewLines(_state))
                    EditorGUILayout.LabelField(line, context.MutedStyle);
                GUILayout.Space(6f);
                context.DrawValidation(report, "Ready to create one root WeaponDefinition asset with stats and presentation sub-assets.");
                GUILayout.Space(8f);
                if (context.DrawCreateButton("Create Weapon Asset", report.IsValid))
                    context.SetCreationResult(WeaponDefinitionAssetCreator.CreateAssets(_state));
                context.DrawCreationResult();
            });
        }
    }

    internal sealed class WeaponAuthoringState
    {
        public string WeaponId = "weapon.example.fire-tower";
        public string DisplayName = "Fire Tower";
        public Sprite Icon;
        public string TagsCsv = "tower, weapon";
        public string OutputRoot = "Assets/GameContent/Weapons";
        public WeaponFireMode FireMode = WeaponFireMode.Projectile;
        public AttackDefinitionAsset Attack;
        public string ProjectileDefinitionId = string.Empty;
        public int CooldownTicks = 18;
        public float Range = 8f;
        public int BuildCost = 25;
        public int BurstCount = 1;
        public int VolleyCount = 1;
        public float SpreadDegrees;
        public string TargetingRoleId = "nearest";
        public string MuzzleRoleId = "primary";
        public string UpgradeGroupId = "upgrade.group.example.fire";
        public GameObject Prefab;
        public AudioClip PlacementAudio;
        public GameObject PlacementVfxPrefab;
    }

    internal sealed class WeaponGameContentPreviewController
    {
        private string _status = "Preview idle";

        public void Draw(GameContentAuthoringPreviewContext context, WeaponAuthoringState state)
        {
            if (context == null) return;
            context.SetStatus(_status);

            context.DrawCard("Weapon Playback", () =>
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (context.DrawPrimaryButton("Preview Weapon Fire", true, GUILayout.Height(26f)))
                        SetStatus(context, WeaponGameContentPreviewSummaries.PreviewWeaponFire(state));
                    if (context.DrawSecondaryButton("Stop Preview", true, GUILayout.Width(104f), GUILayout.Height(26f)))
                        Stop(context);
                }

                context.DrawStatus(_status);
            });

            context.DrawCard("Weapon Model", () =>
            {
                context.DrawObjectPreview(state == null ? null : state.Prefab, "Prefab / Model", "Assign a weapon or tower prefab to see its editor thumbnail.");
                context.DrawAssetRow("Placement VFX", state == null ? null : state.PlacementVfxPrefab, "Not assigned");
                context.DrawAssetRow("Placement Audio", state == null ? null : state.PlacementAudio, "Not assigned");
            });

            context.DrawCard("Runtime Summary", () =>
            {
                context.DrawSummaryRows(WeaponGameContentPreviewSummaries.BuildWeaponRows(state));
            });

            context.DrawCard("Assigned Attack", () =>
            {
                context.DrawSummaryRows(WeaponGameContentPreviewSummaries.BuildAttackRows(state));
            });

            context.DrawWarnings(WeaponGameContentPreviewSummaries.BuildWarnings(state));
        }

        public void Stop()
        {
            _status = "Preview stopped";
        }

        private void Stop(GameContentAuthoringPreviewContext context)
        {
            Stop();
            context.SetStatus(_status);
        }

        private void SetStatus(GameContentAuthoringPreviewContext context, string status)
        {
            _status = string.IsNullOrWhiteSpace(status) ? "Preview idle" : status;
            context.SetStatus(_status);
        }
    }

    internal static class WeaponGameContentPreviewSummaries
    {
        public static string PreviewWeaponFire(WeaponAuthoringState state)
        {
            if (state == null) return "Weapon preview unavailable: authoring state is missing.";
            string attack = state.Attack == null ? "no attack assigned" : state.Attack.DisplayName + " (" + state.Attack.Id + ")";
            return "Weapon fire preview: " + state.FireMode + " using " + attack + ", "
                + state.BurstCount.ToString(CultureInfo.InvariantCulture) + " burst(s) x "
                + state.VolleyCount.ToString(CultureInfo.InvariantCulture) + " volley.";
        }

        public static IReadOnlyList<GameContentAuthoringPreviewRow> BuildWeaponRows(WeaponAuthoringState state)
        {
            if (state == null) return Array.Empty<GameContentAuthoringPreviewRow>();
            return new[]
            {
                Row("Mode", state.FireMode.ToString()),
                Row("Cooldown", state.CooldownTicks.ToString(CultureInfo.InvariantCulture) + " tick(s)"),
                Row("Range", FormatFloat(state.Range)),
                Row("Cost", state.BuildCost.ToString(CultureInfo.InvariantCulture)),
                Row("Pattern", state.BurstCount.ToString(CultureInfo.InvariantCulture) + " burst x " + state.VolleyCount.ToString(CultureInfo.InvariantCulture) + " volley, " + FormatFloat(state.SpreadDegrees) + " deg"),
                Row("Targeting", string.IsNullOrWhiteSpace(state.TargetingRoleId) ? "Not assigned" : state.TargetingRoleId),
                Row("Muzzle", string.IsNullOrWhiteSpace(state.MuzzleRoleId) ? "Not assigned" : state.MuzzleRoleId),
                Row("Upgrade Group", string.IsNullOrWhiteSpace(state.UpgradeGroupId) ? "Not assigned" : state.UpgradeGroupId)
            };
        }

        public static IReadOnlyList<GameContentAuthoringPreviewRow> BuildAttackRows(WeaponAuthoringState state)
        {
            if (state == null || state.Attack == null)
                return new[] { Row("Attack", "Not assigned") };
            string delivery = state.Attack.Delivery == null ? "Missing delivery" : state.Attack.Delivery.Mode.ToString();
            string damage = state.Attack.Mechanics == null
                ? "Missing mechanics"
                : FormatFloat(state.Attack.Mechanics.DamageAmount) + " " + state.Attack.Mechanics.DamageTypeId;
            string projectile = state.FireMode == WeaponFireMode.Projectile
                ? ResolveProjectileId(state)
                : "Not used";
            return new[]
            {
                Row("Attack", state.Attack.DisplayName + " (" + state.Attack.Id + ")"),
                Row("Delivery", delivery),
                Row("Damage", damage),
                Row("Projectile", string.IsNullOrWhiteSpace(projectile) ? "Not assigned" : projectile),
                Row("Presentation", "Attack audio/VFX are summarized by the Attack provider; missing optional assets are skipped.")
            };
        }

        public static IReadOnlyList<string> BuildWarnings(WeaponAuthoringState state)
        {
            if (state == null) return new[] { "Weapon preview state is missing." };
            var warnings = new List<string>();
            if (state.Prefab == null) warnings.Add("Weapon prefab/model is not assigned; asset creation validation will block until a model reference is chosen.");
            if (state.Attack == null) warnings.Add("Weapon has no assigned attack; runtime conversion requires an AttackDefinition asset.");
            if (state.FireMode == WeaponFireMode.Projectile && string.IsNullOrWhiteSpace(ResolveProjectileId(state)))
                warnings.Add("Projectile mode has no projectile definition ID. Assign a projectile attack or enter an override.");
            if (state.PlacementAudio == null && state.PlacementVfxPrefab == null)
                warnings.Add("No placement presentation assets assigned. Runtime and preview skip these optional hooks safely.");
            return warnings;
        }

        private static string ResolveProjectileId(WeaponAuthoringState state)
        {
            if (!string.IsNullOrWhiteSpace(state.ProjectileDefinitionId)) return state.ProjectileDefinitionId.Trim();
            if (state.Attack != null && state.Attack.Delivery != null && state.Attack.Delivery.Mode == AttackRecipeDeliveryMode.Projectile)
                return state.Attack.Delivery.ProjectileDefinitionId;
            return string.Empty;
        }

        private static GameContentAuthoringPreviewRow Row(string label, string value)
        {
            return new GameContentAuthoringPreviewRow(label, value);
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }

    internal static class WeaponDefinitionAssetCreator
    {
        private const string DefaultRoot = "Assets/GameContent/Weapons";

        public static WeaponDefinitionAsset BuildTransient(WeaponAuthoringState state)
        {
            return BuildRecipe(state, true);
        }

        public static GameContentAuthoringValidationResult ValidateForCreation(WeaponAuthoringState state, WeaponDefinitionAsset recipe)
        {
            var issues = ToSharedIssues(WeaponDefinitionValidator.Validate(recipe, WeaponDefinitionValidationOptions.AssetCreation));
            string folder = GetWeaponFolder(state);
            string rootPath = GetRootPath(state);
            GameContentAuthoringEditorAssets.AddPathIssues(issues, state.OutputRoot, DefaultRoot, folder, rootPath, "Weapon", "OutputRoot");
            if (GameContentAuthoringEditorAssets.HasDuplicateId<WeaponDefinitionAsset>(state.WeaponId, asset => asset.Id))
                issues.Add(GameContentAuthoringValidationIssue.Error("Weapon.Id", "Weapon IDs must be unique. Rename this weapon or edit the existing asset instead of creating another."));
            return new GameContentAuthoringValidationResult(issues);
        }

        public static IReadOnlyList<string> GetPreviewLines(WeaponAuthoringState state)
        {
            return new[]
            {
                "Folder: " + GetWeaponFolder(state),
                "Root asset: " + GetFileStem(state) + "_WeaponDefinition.asset",
                "Sections: Stats, Presentation",
                "Runtime: converts to a pure WeaponDefinition and references the assigned AttackDefinition by stable ID.",
                "Presentation: prefab/model plus optional placement audio and VFX are editor/runtime-safe optional hooks."
            };
        }

        public static GameContentCreationResult CreateAssets(WeaponAuthoringState state)
        {
            WeaponDefinitionAsset preview = BuildRecipe(state, true);
            GameContentAuthoringValidationResult report;
            try
            {
                report = ValidateForCreation(state, preview);
                if (!report.IsValid)
                    return new GameContentCreationResult(false, "Fix validation errors before creating assets.", null);
            }
            finally
            {
                DestroyTransient(preview);
            }

            string folder = GetWeaponFolder(state);
            string rootPath = GetRootPath(state);
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(rootPath) != null)
                return new GameContentCreationResult(false, "Asset already exists: " + rootPath, null);
            if (AssetDatabase.IsValidFolder(folder) && GameContentAuthoringEditorPaths.FolderContainsAssets(folder))
            {
                bool confirmed = GameContentAuthoringEditorAssets.ConfirmExistingFolder(folder, "Weapon");
                if (!confirmed)
                    return new GameContentCreationResult(false, "Creation canceled before writing into existing folder.", null);
            }

            folder = GameContentAuthoringEditorPaths.EnsureFolder(folder, DefaultRoot);
            WeaponDefinitionAsset root = BuildRecipe(state, false);
            AssetDatabase.CreateAsset(root, rootPath);
            GameContentAuthoringEditorAssets.AddSubAsset(root.Stats, root, GetFileStem(state) + "_Stats");
            GameContentAuthoringEditorAssets.AddSubAsset(root.Presentation, root, GetFileStem(state) + "_Presentation");
            root.Configure(state.WeaponId, state.DisplayName, state.Icon, GameContentAuthoringEditorAssets.SplitCsv(state.TagsCsv), state.UpgradeGroupId, root.Stats, root.Presentation);
            EditorUtility.SetDirty(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return new GameContentCreationResult(true, "Created weapon definition at " + rootPath, AssetDatabase.LoadAssetAtPath<WeaponDefinitionAsset>(rootPath));
        }

        public static void DestroyTransient(WeaponDefinitionAsset recipe)
        {
            if (recipe == null || recipe.hideFlags != HideFlags.HideAndDontSave) return;
            WeaponStatsDefinitionAsset stats = recipe.Stats;
            WeaponPresentationDefinitionAsset presentation = recipe.Presentation;
            GameContentAuthoringEditorAssets.DestroyTransientObject(stats);
            GameContentAuthoringEditorAssets.DestroyTransientObject(presentation);
            GameContentAuthoringEditorAssets.DestroyTransientObject(recipe);
        }

        private static WeaponDefinitionAsset BuildRecipe(WeaponAuthoringState state, bool transient)
        {
            var stats = ScriptableObject.CreateInstance<WeaponStatsDefinitionAsset>();
            var presentation = ScriptableObject.CreateInstance<WeaponPresentationDefinitionAsset>();
            var root = ScriptableObject.CreateInstance<WeaponDefinitionAsset>();
            if (transient)
            {
                stats.hideFlags = HideFlags.HideAndDontSave;
                presentation.hideFlags = HideFlags.HideAndDontSave;
                root.hideFlags = HideFlags.HideAndDontSave;
            }

            stats.Configure(state.FireMode, state.Attack, state.ProjectileDefinitionId, state.CooldownTicks, state.Range, state.BurstCount, state.VolleyCount, state.SpreadDegrees, state.BuildCost, state.TargetingRoleId, state.MuzzleRoleId);
            presentation.Configure(state.Prefab, state.PlacementAudio, state.PlacementVfxPrefab);
            root.Configure(state.WeaponId, state.DisplayName, state.Icon, GameContentAuthoringEditorAssets.SplitCsv(state.TagsCsv), state.UpgradeGroupId, stats, presentation);
            return root;
        }

        private static string GetWeaponFolder(WeaponAuthoringState state)
        {
            string root = GameContentAuthoringEditorPaths.NormalizeAssetFolderPath(state.OutputRoot, DefaultRoot);
            return root.TrimEnd('/') + "/" + GetFileStem(state);
        }

        private static string GetRootPath(WeaponAuthoringState state)
        {
            return GetWeaponFolder(state) + "/" + GetFileStem(state) + "_WeaponDefinition.asset";
        }

        private static string GetFileStem(WeaponAuthoringState state)
        {
            return GameContentAuthoringEditorPaths.SanitizePathSegment(state.WeaponId, "NewWeapon");
        }

        private static List<GameContentAuthoringValidationIssue> ToSharedIssues(WeaponDefinitionValidationReport report)
        {
            var issues = new List<GameContentAuthoringValidationIssue>();
            if (report == null) return issues;
            for (int i = 0; i < report.Issues.Count; i++)
            {
                WeaponDefinitionValidationIssue issue = report.Issues[i];
                GameContentAuthoringValidationSeverity severity = issue.Severity == WeaponDefinitionValidationSeverity.Error
                    ? GameContentAuthoringValidationSeverity.Error
                    : issue.Severity == WeaponDefinitionValidationSeverity.Warning
                        ? GameContentAuthoringValidationSeverity.Warning
                        : GameContentAuthoringValidationSeverity.Info;
                issues.Add(new GameContentAuthoringValidationIssue(severity, issue.Path, issue.Message));
            }

            return issues;
        }
    }
}
