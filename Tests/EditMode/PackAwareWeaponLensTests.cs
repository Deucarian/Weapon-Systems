using System;
using System.Linq;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.WeaponSystems.Editor;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Deucarian.WeaponSystems.Tests
{
    public sealed class PackAwareWeaponLensTests
    {
        [Test]
        public void Lens_SupportsWeaponOnlyAndTowerOnlyWithoutAssumingBoth()
        {
            var provider = new WeaponAuthoringProvider();
            GameContentRecordDescriptor weapon = Record("weapon", GameContentRecordCapabilities.Weapon);
            GameContentRecordDescriptor tower = Record("tower", GameContentRecordCapabilities.Tower);
            GameContentRecordDescriptor attackOnly = Record("attack", GameContentRecordCapabilities.Attack);

            Assert.That(provider.ProviderId, Is.EqualTo("com.deucarian.weapon-systems.weapon"));
            Assert.That(provider.Lens.Matches(weapon), Is.True);
            Assert.That(provider.Lens.Matches(tower), Is.True);
            Assert.That(provider.Lens.Matches(attackOnly), Is.False);
            Assert.That(weapon.HasCapability(GameContentRecordCapabilities.Tower), Is.False);
            Assert.That(tower.HasCapability(GameContentRecordCapabilities.Weapon), Is.False);
        }

        [Test]
        public void Projection_PreservesAuthoredWeaponValuesAndKind()
        {
            GameContentRecordDescriptor record = Record("weapon.arc", GameContentRecordCapabilities.Weapon, GameContentRecordCapabilities.Attack);
            var projection = new WeaponContentRecordProjection(
                record,
                false,
                "Projectile",
                14f,
                0.75f,
                8f,
                "Nearest",
                "projectile.arc-bolt",
                1.5f,
                "Ranks 1-8",
                "Chain mutation",
                "Arc evolution",
                "Arc presentation");

            Assert.That(projection.IsTower, Is.False);
            Assert.That(projection.Damage, Is.EqualTo(14f));
            Assert.That(projection.CooldownSeconds, Is.EqualTo(0.75f));
            Assert.That(projection.PayloadRecordId, Is.EqualTo("projectile.arc-bolt"));
           Assert.That(record.CanonicalKey.StableKey, Does.EndWith("::weapon.arc"));
            var adapter = new NativeProjectionAdapter(projection);
            try
            {
                GameContentRecordProjectionRegistry<WeaponContentRecordProjection>.Register(adapter);
                var view = new WeaponAuthoringProvider().CreateRecordDetails(record);
                var labels = view.Query<Label>().ToList().Select(label => label.text).ToArray();
                Assert.That(labels, Does.Contain("Ranks 1-8"));
                Assert.That(labels, Does.Contain("Chain mutation"));
                Assert.That(labels, Does.Contain("Arc evolution"));
                Assert.That(labels, Does.Contain("Arc presentation"));
                Assert.That(view.Q<FloatField>(), Is.Null, "Imported projections must remain read-only.");
            }
            finally { GameContentRecordProjectionRegistry<WeaponContentRecordProjection>.Unregister(adapter.AdapterId); }
        }

        private sealed class NativeProjectionAdapter : IGameContentRecordProjectionAdapter<WeaponContentRecordProjection>
        {
            private readonly WeaponContentRecordProjection projection;
            public NativeProjectionAdapter(WeaponContentRecordProjection projection) { this.projection = projection; }
            public string AdapterId { get; } = "native-record-test-" + Guid.NewGuid().ToString("N");
            public int SortOrder => int.MinValue;
            public bool TryProject(GameContentRecordDescriptor record, out WeaponContentRecordProjection value)
            { value = projection; return record.CanonicalKey.Equals(projection.Record.CanonicalKey); }
        }

        private static GameContentRecordDescriptor Record(
            string id,
            params GameContentRecordCapability[] capabilities)
        {
            return new GameContentRecordDescriptor(
                "test-pack::weapons::" + id,
                id,
                "weapons",
                null,
                id,
                string.Empty,
                string.Empty,
                Array.Empty<GameContentMetadataDescriptor>(),
                null,
                "Assets/weapons.json",
                "weapons[0]",
                Array.Empty<GameContentRecordReferenceDescriptor>(),
                Array.Empty<GameContentRecordReferenceDescriptor>(),
                GameContentAuthoringValidationResult.Valid,
                0,
                null,
                string.Empty,
                new GameContentRecordKey("com.deucarian.tests", "test-pack", id, "weapons", "weapons[0]"),
                capabilities);
        }
    }
}
