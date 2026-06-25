using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.WeaponSystems.Authoring
{
    [CreateAssetMenu(menuName = "Deucarian/Weapons/Weapon Definition", fileName = "WeaponDefinition")]
    public sealed class WeaponDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string _id = "weapon.example.basic";
        [SerializeField] private string _displayName = "Example Weapon";
        [SerializeField] private Sprite _icon;
        [SerializeField] private string[] _tags = Array.Empty<string>();
        [SerializeField] private string _upgradeGroupId = string.Empty;
        [SerializeField] private WeaponStatsDefinitionAsset _stats;
        [SerializeField] private WeaponPresentationDefinitionAsset _presentation;
        [SerializeField] private string _balancingNotes = string.Empty;

        public string Id => _id ?? string.Empty;
        public string DisplayName => _displayName ?? string.Empty;
        public Sprite Icon => _icon;
        public IReadOnlyList<string> Tags => _tags ?? Array.Empty<string>();
        public string UpgradeGroupId => _upgradeGroupId ?? string.Empty;
        public WeaponStatsDefinitionAsset Stats => _stats;
        public WeaponPresentationDefinitionAsset Presentation => _presentation;
        public string BalancingNotes => _balancingNotes ?? string.Empty;

        public void Configure(
            string id,
            string displayName,
            Sprite icon,
            IReadOnlyList<string> tags,
            string upgradeGroupId,
            WeaponStatsDefinitionAsset stats,
            WeaponPresentationDefinitionAsset presentation,
            string balancingNotes = "")
        {
            _id = id ?? string.Empty;
            _displayName = displayName ?? string.Empty;
            _icon = icon;
            _tags = CopyTags(tags);
            _upgradeGroupId = upgradeGroupId ?? string.Empty;
            _stats = stats;
            _presentation = presentation;
            _balancingNotes = balancingNotes ?? string.Empty;
        }

        public WeaponDefinition ToRuntimeDefinition()
        {
            if (_stats == null) throw new InvalidOperationException("Weapon definition has no stats section.");
            return _stats.ToRuntimeDefinition(Id);
        }

        public static WeaponDefinitionAsset CreateTransient(
            string id,
            string displayName,
            WeaponFireMode fireMode,
            Deucarian.Attacks.Authoring.AttackDefinitionAsset attack,
            int cooldownTicks,
            float range,
            string projectileDefinitionId = "",
            int burstCount = 1,
            int volleyCount = 1,
            float spreadDegrees = 0f,
            int buildCost = 25,
            string upgradeGroupId = "",
            GameObject prefab = null,
            IReadOnlyList<string> tags = null)
        {
            var stats = CreateInstance<WeaponStatsDefinitionAsset>();
            stats.hideFlags = HideFlags.HideAndDontSave;
            stats.Configure(fireMode, attack, projectileDefinitionId, cooldownTicks, range, burstCount, volleyCount, spreadDegrees, buildCost, "nearest", "primary");

            var presentation = CreateInstance<WeaponPresentationDefinitionAsset>();
            presentation.hideFlags = HideFlags.HideAndDontSave;
            presentation.Configure(prefab, null, null);

            var root = CreateInstance<WeaponDefinitionAsset>();
            root.hideFlags = HideFlags.HideAndDontSave;
            root.Configure(id, displayName, null, tags ?? Array.Empty<string>(), upgradeGroupId, stats, presentation);
            return root;
        }

        private static string[] CopyTags(IReadOnlyList<string> tags)
        {
            if (tags == null || tags.Count == 0) return Array.Empty<string>();
            var copy = new List<string>();
            for (int i = 0; i < tags.Count; i++)
            {
                string tag = tags[i];
                if (!string.IsNullOrWhiteSpace(tag)) copy.Add(tag.Trim());
            }

            return copy.ToArray();
        }
    }
}
