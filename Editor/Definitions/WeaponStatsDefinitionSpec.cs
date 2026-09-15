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
    public sealed class WeaponStatsDefinitionSpec
    {
        [DefinitionField("_fireMode")] public WeaponFireMode FireMode = WeaponFireMode.DirectAttack;
        [DefinitionField("_attack")] public AttackDefinitionAsset Attack;
        [DefinitionField("_projectileDefinitionId")] public string ProjectileDefinitionId = string.Empty;
        [DefinitionField("_cooldownTicks")] public int CooldownTicks = 12;
        [DefinitionField("_range")] public float Range = 7f;
        [DefinitionField("_burstCount")] public int BurstCount = 1;
        [DefinitionField("_volleyCount")] public int VolleyCount = 1;
        [DefinitionField("_spreadDegrees")] public float SpreadDegrees;
        [DefinitionField("_buildCost")] public int BuildCost = 25;
        [DefinitionField("_targetingRoleId")] public string TargetingRoleId = "nearest";
        [DefinitionField("_muzzleRoleId")] public string MuzzleRoleId = "primary";
    }
}
