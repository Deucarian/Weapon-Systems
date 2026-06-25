using Deucarian.Attacks;
using Deucarian.Attacks.Authoring;
using Deucarian.Projectiles;
using UnityEngine;

namespace Deucarian.WeaponSystems.Authoring
{
    public sealed class WeaponStatsDefinitionAsset : ScriptableObject
    {
        [SerializeField] private WeaponFireMode _fireMode = WeaponFireMode.DirectAttack;
        [SerializeField] private AttackDefinitionAsset _attack;
        [SerializeField] private string _projectileDefinitionId = string.Empty;
        [SerializeField] private int _cooldownTicks = 12;
        [SerializeField] private float _range = 7f;
        [SerializeField] private int _burstCount = 1;
        [SerializeField] private int _volleyCount = 1;
        [SerializeField] private float _spreadDegrees;
        [SerializeField] private int _buildCost = 25;
        [SerializeField] private string _targetingRoleId = "nearest";
        [SerializeField] private string _muzzleRoleId = "primary";

        public WeaponFireMode FireMode => _fireMode;
        public AttackDefinitionAsset Attack => _attack;
        public string ProjectileDefinitionId => _projectileDefinitionId ?? string.Empty;
        public int CooldownTicks => _cooldownTicks;
        public float Range => _range;
        public int BurstCount => _burstCount;
        public int VolleyCount => _volleyCount;
        public float SpreadDegrees => _spreadDegrees;
        public int BuildCost => _buildCost;
        public string TargetingRoleId => _targetingRoleId ?? string.Empty;
        public string MuzzleRoleId => _muzzleRoleId ?? string.Empty;

        public void Configure(
            WeaponFireMode fireMode,
            AttackDefinitionAsset attack,
            string projectileDefinitionId,
            int cooldownTicks,
            float range,
            int burstCount,
            int volleyCount,
            float spreadDegrees,
            int buildCost,
            string targetingRoleId,
            string muzzleRoleId)
        {
            _fireMode = fireMode;
            _attack = attack;
            _projectileDefinitionId = projectileDefinitionId ?? string.Empty;
            _cooldownTicks = cooldownTicks;
            _range = range;
            _burstCount = burstCount;
            _volleyCount = volleyCount;
            _spreadDegrees = spreadDegrees;
            _buildCost = buildCost;
            _targetingRoleId = targetingRoleId ?? string.Empty;
            _muzzleRoleId = muzzleRoleId ?? string.Empty;
        }

        public WeaponDefinition ToRuntimeDefinition(string weaponId)
        {
            string attackId = _attack == null ? string.Empty : _attack.Id;
            string projectileId = ResolveProjectileDefinitionId();
            return new WeaponDefinition(
                new WeaponDefinitionId(weaponId),
                _fireMode,
                new AttackDefinitionId(attackId),
                _cooldownTicks,
                string.IsNullOrWhiteSpace(projectileId) ? default : new ProjectileDefinitionId(projectileId),
                _burstCount,
                new WeaponFirePattern(_volleyCount, _spreadDegrees));
        }

        public string ResolveProjectileDefinitionId()
        {
            if (!string.IsNullOrWhiteSpace(_projectileDefinitionId)) return _projectileDefinitionId.Trim();
            if (_attack != null && _attack.Delivery != null && _attack.Delivery.Mode == AttackRecipeDeliveryMode.Projectile)
                return _attack.Delivery.ProjectileDefinitionId;
            return string.Empty;
        }
    }
}
