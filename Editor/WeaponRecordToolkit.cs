using System;
using System.Globalization;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.WeaponSystems.Editor
{
    internal static class WeaponRecordToolkit
    {
        internal static VisualElement Weapon(GameContentRecordDescriptor record)
        {
            if (!GameContentRecordProjectionRegistry<WeaponContentRecordProjection>.TryProject(record, out var p)) return null;
            var root = new VisualElement { name = "record-weapon-details" };
            var form = new DeucarianEditorWorkspaceForm(root);
            form.ReadOnly(null, "Damage", () => Convert.ToString(p.Damage, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Cooldown (s)", () => Convert.ToString(p.CooldownSeconds, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Range", () => Convert.ToString(p.Range, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Targeting", () => Convert.ToString(p.TargetingMode, CultureInfo.InvariantCulture));
            form = form.Section("Weapon details", true);
            form.ReadOnly(null, "Kind", () => Convert.ToString(p.IsTower ? "Tower" : "Weapon", CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Fire mode", () => Convert.ToString(p.FireMode, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Payload", () => Convert.ToString(p.PayloadRecordId, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Area", () => Convert.ToString(p.AreaRadius, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Rank path", () => Convert.ToString(p.RankPathSummary, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Mutation", () => Convert.ToString(p.MutationSummary, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Evolution", () => Convert.ToString(p.EvolutionSummary, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Presentation", () => Convert.ToString(p.PresentationSummary, CultureInfo.InvariantCulture));
            if (record.Preview != null) GameContentToolkitPreview.Add(root, () => record.Preview, null);
            return root;
        }

    }
}
