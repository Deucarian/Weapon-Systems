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
    internal static class WeaponAuthoringSummary
    {
        internal static IReadOnlyList<DeucarianEditorStatusChip> BuildWeaponChips(WeaponAuthoringState state, GameContentAuthoringValidationResult validation)
        {
            return new[]
            {
                new DeucarianEditorStatusChip(GetWeaponTypeLabel(state), DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(validation != null && validation.ErrorCount > 0 ? "Blocked" : validation != null && validation.WarningCount > 0 ? "Warnings" : "Ready", validation != null && validation.ErrorCount > 0 ? DeucarianEditorStatus.Error : validation != null && validation.WarningCount > 0 ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Success),
                new DeucarianEditorStatusChip(state.Attack == null ? "NoAttack" : "Attack", state.Attack == null ? DeucarianEditorStatus.Error : DeucarianEditorStatus.Success),
                new DeucarianEditorStatusChip(state.Prefab == null ? "NoModel" : "Model", state.Prefab == null ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Success)
            };
        }

        private static IReadOnlyList<GameContentAuthoringPreviewRow> WeaponRows(params GameContentAuthoringPreviewRow[] rows)
        {
            return rows;
        }

        internal static GameContentAuthoringPreviewRow Row(string label, string value)
        {
            return new GameContentAuthoringPreviewRow(label, value);
        }

        internal static string BuildHumanSummary(WeaponAuthoringState state)
        {
            return GetWeaponTypeLabel(state) + ", " + FormatFloat(state.Range) + " range, " + BuildCadenceLabel(state);
        }

        internal static string BuildReverseReferenceSummary(GameContentLibraryItem item)
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

        internal static string BuildCadenceLabel(WeaponAuthoringState state)
        {
            if (state == null)
                return string.Empty;
            return state.CooldownTicks.ToString(CultureInfo.InvariantCulture) + " ticks, "
                + state.BurstCount.ToString(CultureInfo.InvariantCulture) + "x"
                + state.VolleyCount.ToString(CultureInfo.InvariantCulture);
        }

        internal static string BuildDpsEstimate(WeaponAuthoringState state)
        {
            if (state == null || state.Attack == null || state.Attack.Mechanics == null)
                return "Assign an attack for estimate";
            float shots = Mathf.Max(1, state.BurstCount) * Mathf.Max(1, state.VolleyCount);
            float cooldown = Mathf.Max(1, state.CooldownTicks);
            float damagePerTick = state.Attack.Mechanics.DamageAmount * shots / cooldown;
            return FormatFloat(damagePerTick * 60f) + " damage / 60 ticks";
        }

        internal static string Sanitize(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? "NewWeapon" : id.Trim().Replace('\\', '-').Replace('/', '-').Replace(':', '-');
        }

        internal static string FormatFloat(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
