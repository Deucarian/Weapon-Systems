using System;
using System.Linq;
using Deucarian.Editor.Definitions;
using Deucarian.WeaponSystems.Authoring;
using Deucarian.Attacks.Authoring;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WeaponSystems.Editor.Definitions
{
    [Serializable]
    public sealed class WeaponDefinitionSpec : DeucarianDefinitionSpec
    {
        [DefinitionField("_icon")] public Sprite Icon;
        [DefinitionField("_tags")] public string[] Tags = Array.Empty<string>();
        [DefinitionField("_upgradeGroupId")] public string UpgradeGroupId = string.Empty;
        [DefinitionSection("_stats", typeof(WeaponStatsDefinitionAsset))] public WeaponStatsDefinitionSpec Stats = new WeaponStatsDefinitionSpec();
        [DefinitionSection("_presentation", typeof(WeaponPresentationDefinitionAsset))] public WeaponPresentationDefinitionSpec Presentation = new WeaponPresentationDefinitionSpec();
        [DefinitionField("_balancingNotes")] public string BalancingNotes = string.Empty;
    }
}
