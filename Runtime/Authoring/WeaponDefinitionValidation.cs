using System;
using System.Collections.Generic;
using Deucarian.Attacks.Authoring;

namespace Deucarian.WeaponSystems.Authoring
{
    public enum WeaponDefinitionValidationSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    public readonly struct WeaponDefinitionValidationOptions
    {
        public WeaponDefinitionValidationOptions(bool requirePrefab)
        {
            RequirePrefab = requirePrefab;
        }

        public bool RequirePrefab { get; }
        public static WeaponDefinitionValidationOptions RuntimeFriendly => new WeaponDefinitionValidationOptions(false);
        public static WeaponDefinitionValidationOptions AssetCreation => new WeaponDefinitionValidationOptions(true);
    }

    public readonly struct WeaponDefinitionValidationIssue
    {
        public WeaponDefinitionValidationIssue(WeaponDefinitionValidationSeverity severity, string path, string message)
        {
            Severity = severity;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public WeaponDefinitionValidationSeverity Severity { get; }
        public string Path { get; }
        public string Message { get; }
        public bool IsError => Severity == WeaponDefinitionValidationSeverity.Error;

        public static WeaponDefinitionValidationIssue Error(string path, string message)
        {
            return new WeaponDefinitionValidationIssue(WeaponDefinitionValidationSeverity.Error, path, message);
        }

        public static WeaponDefinitionValidationIssue Warning(string path, string message)
        {
            return new WeaponDefinitionValidationIssue(WeaponDefinitionValidationSeverity.Warning, path, message);
        }
    }

    public sealed class WeaponDefinitionValidationReport
    {
        private readonly WeaponDefinitionValidationIssue[] _issues;

        public WeaponDefinitionValidationReport(IReadOnlyList<WeaponDefinitionValidationIssue> issues)
        {
            _issues = Copy(issues);
        }

        public IReadOnlyList<WeaponDefinitionValidationIssue> Issues => _issues;

        public bool IsValid
        {
            get
            {
                for (int i = 0; i < _issues.Length; i++)
                    if (_issues[i].IsError)
                        return false;
                return true;
            }
        }

        private static WeaponDefinitionValidationIssue[] Copy(IReadOnlyList<WeaponDefinitionValidationIssue> issues)
        {
            if (issues == null || issues.Count == 0) return Array.Empty<WeaponDefinitionValidationIssue>();
            var copy = new WeaponDefinitionValidationIssue[issues.Count];
            for (int i = 0; i < issues.Count; i++) copy[i] = issues[i];
            return copy;
        }
    }

    public static class WeaponDefinitionValidator
    {
        public static WeaponDefinitionValidationReport Validate(WeaponDefinitionAsset definition)
        {
            return Validate(definition, WeaponDefinitionValidationOptions.RuntimeFriendly);
        }

        public static WeaponDefinitionValidationReport Validate(WeaponDefinitionAsset definition, WeaponDefinitionValidationOptions options)
        {
            var issues = new List<WeaponDefinitionValidationIssue>();
            if (definition == null)
            {
                issues.Add(WeaponDefinitionValidationIssue.Error("Weapon", "Weapon definition is missing."));
                return new WeaponDefinitionValidationReport(issues);
            }

            if (string.IsNullOrWhiteSpace(definition.Id)) issues.Add(WeaponDefinitionValidationIssue.Error("Weapon.Id", "Weapon ID is required."));
            if (string.IsNullOrWhiteSpace(definition.DisplayName)) issues.Add(WeaponDefinitionValidationIssue.Warning("Weapon.DisplayName", "Display name is empty."));
            ValidateStats(definition.Stats, issues);
            ValidatePresentation(definition.Presentation, options, issues);
            return new WeaponDefinitionValidationReport(issues);
        }

        private static void ValidateStats(WeaponStatsDefinitionAsset stats, List<WeaponDefinitionValidationIssue> issues)
        {
            if (stats == null)
            {
                issues.Add(WeaponDefinitionValidationIssue.Error("Stats", "Stats section is required."));
                return;
            }

            if (stats.Attack == null)
            {
                issues.Add(WeaponDefinitionValidationIssue.Error("Stats.Attack", "Choose an AttackDefinition asset."));
            }
            else
            {
                AttackRecipeValidationReport attackReport = AttackRecipeValidator.Validate(stats.Attack, AttackRecipeValidationOptions.RuntimeFriendly);
                if (!attackReport.IsValid)
                    issues.Add(WeaponDefinitionValidationIssue.Error("Stats.Attack", "Assigned attack definition is invalid."));
            }

            if (stats.CooldownTicks < 0) issues.Add(WeaponDefinitionValidationIssue.Error("Stats.CooldownTicks", "Cooldown cannot be negative."));
            if (stats.Range < 0f || float.IsNaN(stats.Range) || float.IsInfinity(stats.Range)) issues.Add(WeaponDefinitionValidationIssue.Error("Stats.Range", "Range must be a finite non-negative value."));
            if (stats.BurstCount <= 0) issues.Add(WeaponDefinitionValidationIssue.Error("Stats.BurstCount", "Burst count must be greater than zero."));
            if (stats.VolleyCount <= 0) issues.Add(WeaponDefinitionValidationIssue.Error("Stats.VolleyCount", "Volley count must be greater than zero."));
            if (stats.SpreadDegrees < 0f || float.IsNaN(stats.SpreadDegrees) || float.IsInfinity(stats.SpreadDegrees)) issues.Add(WeaponDefinitionValidationIssue.Error("Stats.SpreadDegrees", "Spread must be a finite non-negative value."));
            if (stats.BuildCost < 0) issues.Add(WeaponDefinitionValidationIssue.Error("Stats.BuildCost", "Build cost cannot be negative."));
            if (stats.FireMode == WeaponFireMode.Projectile && string.IsNullOrWhiteSpace(stats.ResolveProjectileDefinitionId()))
                issues.Add(WeaponDefinitionValidationIssue.Error("Stats.ProjectileDefinitionId", "Projectile weapons require a projectile definition ID. Assign a projectile attack or enter an override."));
        }

        private static void ValidatePresentation(WeaponPresentationDefinitionAsset presentation, WeaponDefinitionValidationOptions options, List<WeaponDefinitionValidationIssue> issues)
        {
            if (presentation == null)
            {
                issues.Add(WeaponDefinitionValidationIssue.Warning("Presentation", "Presentation section is missing; runtime will use data only."));
                return;
            }

            if (options.RequirePrefab && presentation.Prefab == null)
                issues.Add(WeaponDefinitionValidationIssue.Error("Presentation.Prefab", "Choose a weapon/tower prefab or model before creating this asset."));
        }
    }
}
