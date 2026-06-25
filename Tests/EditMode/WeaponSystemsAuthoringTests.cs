using System;
using Deucarian.Attacks.Authoring;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.WeaponSystems.Authoring;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.WeaponSystems.Tests
{
    public sealed class WeaponSystemsAuthoringTests
    {
        [Test]
        public void WeaponDefinitionAssetConvertsToRuntimeDefinition()
        {
            AttackDefinitionAsset attack = AttackDefinitionAsset.CreateTransient(
                "attack.weapon.authoring.projectile",
                "Projectile Attack",
                AttackRecipeDeliveryMode.Projectile,
                "damage.test",
                6,
                4,
                9,
                AttackRecipeTargetingMode.Nearest,
                projectileDefinitionId: "projectile.weapon.authoring",
                projectileSpawnableId: "projectile.weapon.authoring");
            GameObject prefab = new GameObject("weapon-authoring-prefab");
            WeaponDefinitionAsset weapon = WeaponDefinitionAsset.CreateTransient(
                "weapon.authoring.projectile",
                "Projectile Weapon",
                WeaponFireMode.Projectile,
                attack,
                12,
                9f,
                prefab: prefab);

            try
            {
                WeaponDefinitionValidationReport report = WeaponDefinitionValidator.Validate(weapon, WeaponDefinitionValidationOptions.AssetCreation);
                WeaponDefinition runtime = weapon.ToRuntimeDefinition();

                Assert.IsTrue(report.IsValid);
                Assert.AreEqual("weapon.authoring.projectile", runtime.Id.Value);
                Assert.AreEqual(WeaponFireMode.Projectile, runtime.FireMode);
                Assert.AreEqual("attack.weapon.authoring.projectile", runtime.AttackDefinitionId.Value);
                Assert.AreEqual("projectile.weapon.authoring", runtime.ProjectileDefinitionId.Value);
                Assert.AreEqual(12, runtime.CooldownTicks);
            }
            finally
            {
                Object.DestroyImmediate(prefab);
                Object.DestroyImmediate(weapon.Presentation);
                Object.DestroyImmediate(weapon.Stats);
                Object.DestroyImmediate(weapon);
                Object.DestroyImmediate(attack.Presentation);
                Object.DestroyImmediate(attack.StatusEffects);
                Object.DestroyImmediate(attack.Delivery);
                Object.DestroyImmediate(attack.Targeting);
                Object.DestroyImmediate(attack.Mechanics);
                Object.DestroyImmediate(attack);
            }
        }

        [Test]
        public void WeaponValidationRejectsMissingAttackAndPrefabForCreation()
        {
            WeaponDefinitionAsset weapon = WeaponDefinitionAsset.CreateTransient(
                "weapon.authoring.invalid",
                "Invalid Weapon",
                WeaponFireMode.DirectAttack,
                null,
                6,
                8f);

            try
            {
                WeaponDefinitionValidationReport report = WeaponDefinitionValidator.Validate(weapon, WeaponDefinitionValidationOptions.AssetCreation);

                Assert.IsFalse(report.IsValid);
                Assert.That(FindIssue(report, "Stats.Attack"), Is.True);
                Assert.That(FindIssue(report, "Presentation.Prefab"), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(weapon.Presentation);
                Object.DestroyImmediate(weapon.Stats);
                Object.DestroyImmediate(weapon);
            }
        }

        [Test]
        public void WeaponAuthoringProviderRegistersWithSharedWindow()
        {
            Assert.IsTrue(GameContentAuthoringProviderRegistry.IsProviderRegistered("com.deucarian.weapon-systems.weapon"));
        }

        [Test]
        public void WeaponDuplicateIdsAreDetectedInAssets()
        {
            const string folder = "Assets/WeaponAuthoringDuplicateTests";
            const string id = "weapon.authoring.duplicate";
            AssetDatabase.DeleteAsset(folder);
            AssetDatabase.CreateFolder("Assets", "WeaponAuthoringDuplicateTests");
            WeaponDefinitionAsset asset = ScriptableObject.CreateInstance<WeaponDefinitionAsset>();
            asset.Configure(id, "Duplicate Weapon", null, Array.Empty<string>(), string.Empty, null, null);
            AssetDatabase.CreateAsset(asset, folder + "/DuplicateWeapon.asset");
            AssetDatabase.SaveAssets();

            try
            {
                Assert.IsTrue(GameContentAuthoringEditorAssets.HasDuplicateId<WeaponDefinitionAsset>(id, candidate => candidate.Id));
            }
            finally
            {
                AssetDatabase.DeleteAsset(folder);
            }
        }

        private static bool FindIssue(WeaponDefinitionValidationReport report, string path)
        {
            for (int i = 0; i < report.Issues.Count; i++)
                if (report.Issues[i].Path == path)
                    return true;
            return false;
        }
    }
}
