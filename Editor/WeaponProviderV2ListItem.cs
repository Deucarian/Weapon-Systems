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
            return WeaponAuthoringSummary.GetWeaponTypeLabel(state);
        }

        private static string GetTypeLabel(WeaponDefinitionAsset asset)
        {
            return WeaponAuthoringSummary.GetWeaponTypeLabel(asset == null ? null : WeaponAuthoringDraft.FromWeaponAsset(asset));
        }

        private static bool Contains(string text, string value)
        {
            return (text ?? string.Empty).IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
