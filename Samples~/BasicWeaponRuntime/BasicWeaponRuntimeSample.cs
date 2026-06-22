using System;
using System.Collections.Generic;
using Deucarian.Attacks;
using Deucarian.Projectiles;
using UnityEngine;

namespace Deucarian.WeaponSystems.Samples
{
    public static class BasicWeaponRuntimeSample
    {
        public static IReadOnlyList<WeaponIntent> CreateProjectileIntents()
        {
            var definition = new WeaponDefinition(
                new WeaponDefinitionId("sample.projectile.weapon"),
                WeaponFireMode.Projectile,
                new AttackDefinitionId("sample.attack"),
                3,
                new ProjectileDefinitionId("sample.projectile"));
            var runtime = new WeaponRuntime(new[] { definition }, null, new ProjectileLaunchWeaponAdapter());
            var source = default(AttackSourceSnapshot);
            runtime.RegisterWeapon(new WeaponMountSnapshot(new WeaponSlotId("slot.primary"), definition.Id, source));
            WeaponFireResult result = runtime.TryFire(new WeaponSlotId("slot.primary"), new WeaponFireRequest(Array.Empty<AttackTargetCandidate>(), Vector3.zero, Vector3.forward));
            return result.Intents;
        }
    }
}
