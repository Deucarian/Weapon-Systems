using System;
using System.Collections.Generic;
using System.Diagnostics;
using Deucarian.Attacks;
using Deucarian.Combat;
using Deucarian.DefenseGames;
using Deucarian.Projectiles;
using Deucarian.WorldNavigation;
using Deucarian.WorldSpawning;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Deucarian.WeaponSystems.Tests
{
    public sealed class WeaponSystemsTests
    {
        [Test]
        public void DefinitionRejectsInvalidInput()
        {
            var valid = new WeaponDefinition(new WeaponDefinitionId("weapon.valid"), WeaponFireMode.DirectAttack, AttackId, 1);
            Assert.AreEqual("weapon.valid", valid.Id.Value);
            Assert.Throws<ArgumentException>(() => new WeaponDefinition(default, WeaponFireMode.DirectAttack, AttackId, 1));
            Assert.Throws<ArgumentException>(() => new WeaponDefinition(new WeaponDefinitionId("weapon"), WeaponFireMode.DirectAttack, default, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponDefinition(new WeaponDefinitionId("weapon"), WeaponFireMode.DirectAttack, AttackId, -1));
            Assert.Throws<ArgumentException>(() => new WeaponDefinition(new WeaponDefinitionId("weapon"), WeaponFireMode.Projectile, AttackId, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponFirePattern(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponFirePattern(1, float.NaN));
            Assert.Throws<ArgumentException>(() => new WeaponMountSnapshot(default, ProjectileWeaponId, Source()));
        }

        [Test]
        public void RegisterRemoveAndDisableWeapons()
        {
            WeaponRuntime runtime = Runtime(Projector());
            Assert.True(runtime.RegisterWeapon(Mount("slot.a", ProjectileWeaponId)));
            Assert.AreEqual(1, runtime.RegisteredCount);
            Assert.True(runtime.RegisterWeapon(Mount("slot.a", SpreadWeaponId)));
            Assert.AreEqual(1, runtime.RegisteredCount);
            Assert.True(runtime.TryGetMount(new WeaponSlotId("slot.a"), out WeaponMountSnapshot replaced));
            Assert.AreEqual(SpreadWeaponId, replaced.DefinitionId);
            Assert.True(runtime.SetEnabled(new WeaponSlotId("slot.a"), false));
            WeaponFireResult disabled = runtime.TryFire(new WeaponSlotId("slot.a"), Request());
            Assert.False(disabled.Succeeded);
            Assert.AreEqual(WeaponFailureReason.Disabled, disabled.FailureReason);
            Assert.True(runtime.RemoveWeapon(new WeaponSlotId("slot.a")));
            Assert.False(runtime.RemoveWeapon(new WeaponSlotId("slot.a")));
        }

        [Test]
        public void TickAdvancesCadenceAndResetThroughSnapshot()
        {
            WeaponRuntime runtime = Runtime(Projector());
            runtime.RegisterWeapon(Mount("slot.a", ProjectileWeaponId));
            Assert.True(runtime.TryFire(new WeaponSlotId("slot.a"), Request()).Succeeded);
            Assert.True(runtime.TryGetMount(new WeaponSlotId("slot.a"), out WeaponMountSnapshot mount));
            Assert.AreEqual(3, mount.RemainingCooldownTicks);
            runtime.Tick(2);
            runtime.TryGetMount(new WeaponSlotId("slot.a"), out mount);
            Assert.AreEqual(1, mount.RemainingCooldownTicks);
            WeaponFireResult notReady = runtime.TryFire(new WeaponSlotId("slot.a"), Request());
            Assert.False(notReady.Succeeded);
            Assert.AreEqual(WeaponFailureReason.NotReady, notReady.FailureReason);
            runtime.Tick(1);
            Assert.True(runtime.TryFire(new WeaponSlotId("slot.a"), Request()).Succeeded);
        }

        [Test]
        public void DirectModeCreatesAttackIntentAndCombatCanResolve()
        {
            HealthState target = Target();
            AttackRuntime attacks = AttackRuntime(target);
            var runtime = new WeaponRuntime(Definitions(), new AttackRuntimeWeaponAttackAdapter(attacks), Projector());
            runtime.RegisterWeapon(Mount("slot.direct", DirectWeaponId));

            WeaponFireResult result = runtime.TryFire(new WeaponSlotId("slot.direct"), Request(target));

            Assert.True(result.Succeeded);
            Assert.AreEqual(WeaponIntentKind.DirectAttack, result.Intents[0].Kind);
            DamageResolutionResult resolved = CombatDamageResolver.Resolve(result.Intents[0].AttackIntent.ResolutionRequest);
            Assert.AreEqual(CombatStatus.Success, resolved.Status);
            Assert.AreEqual(90d, target.CurrentHealth);
        }

        [Test]
        public void ProjectileModeCreatesLaunchRequestAndProjectilesCanLaunch()
        {
            WeaponRuntime weapons = Runtime(Projector());
            weapons.RegisterWeapon(Mount("slot.projectile", ProjectileWeaponId));
            WeaponFireResult result = weapons.TryFire(new WeaponSlotId("slot.projectile"), Request());
            Assert.True(result.Succeeded);

            var spawner = new FakeProjectileSpawner();
            ProjectileRuntime projectiles = ProjectileRuntime(spawner);
            ProjectileLaunchResult launch = projectiles.Launch(result.Intents[0].ProjectileLaunchRequest);

            Assert.True(launch.Succeeded);
            Assert.AreEqual(1, projectiles.ActiveCount);
            projectiles.Cleanup(launch.ProjectileId);
            spawner.DestroyCreated();
        }

        [Test]
        public void BurstVolleyAndSpreadDescriptorsMultiplyIntents()
        {
            WeaponRuntime runtime = Runtime(Projector());
            runtime.RegisterWeapon(Mount("slot.spread", SpreadWeaponId));
            WeaponFireResult result = runtime.TryFire(new WeaponSlotId("slot.spread"), Request());
            Assert.True(result.Succeeded);
            Assert.AreEqual(6, result.Intents.Count);
            Assert.AreEqual(2, result.Intents[5].BurstIndex);
            Assert.AreEqual(1, result.Intents[5].VolleyIndex);
            Assert.AreEqual(15f, result.Intents[5].Pattern.SpreadDegrees);
        }

        [Test]
        public void DeterministicOrderIgnoresRegistrationOrder()
        {
            WeaponRuntime runtime = Runtime(Projector());
            runtime.RegisterWeapon(Mount("slot.z", ProjectileWeaponId));
            runtime.RegisterWeapon(Mount("slot.a", ProjectileWeaponId));
            runtime.RegisterWeapon(Mount("slot.m", ProjectileWeaponId));
            WeaponFireResult result = runtime.FireReady(Request());
            Assert.True(result.Succeeded);
            Assert.AreEqual("slot.a", result.Intents[0].SlotId.Value);
            Assert.AreEqual("slot.m", result.Intents[1].SlotId.Value);
            Assert.AreEqual("slot.z", result.Intents[2].SlotId.Value);
        }

        [Test]
        public void CallerSuppliedTargetCandidatesControlDirectSelection()
        {
            HealthState low = new HealthState(new CombatantId("enemy.low"), 100, 100);
            HealthState high = new HealthState(new CombatantId("enemy.high"), 100, 100);
            AttackRuntime attacks = AttackRuntime(high);
            attacks.RegisterSource(Source());
            var runtime = new WeaponRuntime(Definitions(), new AttackRuntimeWeaponAttackAdapter(attacks), Projector());
            runtime.RegisterWeapon(Mount("slot.direct", DirectWeaponId));
            var candidates = new[]
            {
                new AttackTargetCandidate(low.Id, low, 1d),
                new AttackTargetCandidate(high.Id, high, 99d)
            };

            WeaponFireResult result = runtime.TryFire(new WeaponSlotId("slot.direct"), new WeaponFireRequest(candidates, Vector3.zero, Vector3.one));

            Assert.True(result.Succeeded);
            Assert.AreEqual(high.Id, result.Intents[0].AttackIntent.Selection.Target.CombatantId);
        }

        [Test]
        public void AttackTieBreakComesFromAttacks()
        {
            HealthState b = new HealthState(new CombatantId("enemy.b"), 100, 100);
            HealthState a = new HealthState(new CombatantId("enemy.a"), 100, 100);
            AttackRuntime attacks = AttackRuntime(a);
            var runtime = new WeaponRuntime(Definitions(), new AttackRuntimeWeaponAttackAdapter(attacks), Projector());
            runtime.RegisterWeapon(Mount("slot.direct", DirectWeaponId));
            var candidates = new[]
            {
                new AttackTargetCandidate(b.Id, b, 10d),
                new AttackTargetCandidate(a.Id, a, 10d)
            };

            WeaponFireResult result = runtime.TryFire(new WeaponSlotId("slot.direct"), new WeaponFireRequest(candidates, Vector3.zero, Vector3.one));

            Assert.True(result.Succeeded);
            Assert.AreEqual(a.Id, result.Intents[0].AttackIntent.Selection.Target.CombatantId);
        }

        [Test]
        public void DirectModeWithoutCandidatesReturnsNoCandidateFailure()
        {
            AttackRuntime attacks = AttackRuntime(Target());
            var runtime = new WeaponRuntime(Definitions(), new AttackRuntimeWeaponAttackAdapter(attacks), Projector());
            runtime.RegisterWeapon(Mount("slot.direct", DirectWeaponId));
            WeaponFireResult result = runtime.TryFire(new WeaponSlotId("slot.direct"), Request());
            Assert.False(result.Succeeded);
            Assert.AreEqual(WeaponFailureReason.NoCandidates, result.FailureReason);
        }

        [Test]
        public void AttackFailureIsReported()
        {
            var runtime = new WeaponRuntime(Definitions(), new FailingAttackAdapter(), Projector());
            runtime.RegisterWeapon(Mount("slot.direct", DirectWeaponId));
            WeaponFireResult result = runtime.TryFire(new WeaponSlotId("slot.direct"), Request(Target()));
            Assert.False(result.Succeeded);
            Assert.AreEqual(WeaponFailureReason.AttackFailed, result.FailureReason);
        }

        [Test]
        public void MissingAttackDefinitionIsReportedAsAttackFailure()
        {
            HealthState target = Target();
            AttackRuntime attacks = AttackRuntime(target);
            var missing = new WeaponDefinition(new WeaponDefinitionId("weapon.missing.attack"), WeaponFireMode.DirectAttack, new AttackDefinitionId("attack.missing"), 1);
            var runtime = new WeaponRuntime(new[] { missing }, new AttackRuntimeWeaponAttackAdapter(attacks), Projector());
            runtime.RegisterWeapon(new WeaponMountSnapshot(new WeaponSlotId("slot.direct"), missing.Id, Source()));
            WeaponFireResult result = runtime.TryFire(new WeaponSlotId("slot.direct"), Request(target));
            Assert.False(result.Succeeded);
            Assert.AreEqual(WeaponFailureReason.AttackFailed, result.FailureReason);
        }

        [Test]
        public void FailureInOneWeaponDoesNotCorruptOtherWeaponState()
        {
            var runtime = new WeaponRuntime(Definitions(), new FailingAttackAdapter(), Projector());
            runtime.RegisterWeapon(Mount("slot.a.direct", DirectWeaponId));
            runtime.RegisterWeapon(Mount("slot.b.projectile", ProjectileWeaponId));

            WeaponFireResult result = runtime.FireReady(Request(Target()));

            Assert.False(result.Succeeded);
            Assert.AreEqual(WeaponFailureReason.AttackFailed, result.FailureReason);
            Assert.AreEqual(1, result.FiredCount);
            Assert.AreEqual(1, result.FailureCount);
            Assert.AreEqual("slot.b.projectile", result.Intents[0].SlotId.Value);
        }

        [Test]
        public void SnapshotReconstructionPreservesCooldownAndOrder()
        {
            WeaponRuntime runtime = Runtime(Projector());
            runtime.RegisterWeapon(Mount("slot.b", ProjectileWeaponId));
            runtime.RegisterWeapon(Mount("slot.a", ProjectileWeaponId, remaining: 2));
            runtime.TryFire(new WeaponSlotId("slot.b"), Request());
            WeaponSnapshot snapshot = runtime.CreateSnapshot();

            WeaponRuntime reconstructed = WeaponRuntime.FromSnapshot(Definitions(), null, Projector(), snapshot);

            Assert.True(reconstructed.TryGetMount(new WeaponSlotId("slot.a"), out WeaponMountSnapshot a));
            Assert.True(reconstructed.TryGetMount(new WeaponSlotId("slot.b"), out WeaponMountSnapshot b));
            Assert.AreEqual(2, a.RemainingCooldownTicks);
            Assert.AreEqual(3, b.RemainingCooldownTicks);
            Assert.AreEqual("slot.a", reconstructed.CreateSnapshot().Mounts[0].SlotId.Value);
        }

        [Test]
        public void DeterministicReplayProducesSameIntentOrder()
        {
            WeaponRuntime first = Runtime(Projector());
            first.RegisterWeapon(Mount("slot.c", ProjectileWeaponId));
            first.RegisterWeapon(Mount("slot.a", ProjectileWeaponId));
            first.RegisterWeapon(Mount("slot.b", ProjectileWeaponId));
            WeaponSnapshot snapshot = first.CreateSnapshot();

            WeaponRuntime replay = WeaponRuntime.FromSnapshot(Definitions(), null, Projector(), snapshot);
            WeaponFireResult a = first.FireReady(Request());
            WeaponFireResult b = replay.FireReady(Request());

            Assert.AreEqual(a.Intents.Count, b.Intents.Count);
            for (int i = 0; i < a.Intents.Count; i++)
                Assert.AreEqual(a.Intents[i].SlotId, b.Intents[i].SlotId);
        }

        [Test]
        public void ZeroCooldownCanFireRepeatedly()
        {
            var zero = new WeaponDefinition(new WeaponDefinitionId("weapon.zero"), WeaponFireMode.Projectile, AttackId, 0, ProjectileId);
            var runtime = new WeaponRuntime(new[] { zero }, null, Projector());
            runtime.RegisterWeapon(Mount("slot.zero", zero.Id));
            Assert.True(runtime.TryFire(new WeaponSlotId("slot.zero"), Request()).Succeeded);
            Assert.True(runtime.TryFire(new WeaponSlotId("slot.zero"), Request()).Succeeded);
        }

        [Test]
        public void UnknownAndDuplicateDefinitionCasesAreHandled()
        {
            Assert.Throws<ArgumentException>(() => new WeaponRuntime(new[] { ProjectileDefinition(), ProjectileDefinition() }, null, Projector()));
            WeaponRuntime runtime = Runtime(Projector());
            Assert.False(runtime.RegisterWeapon(Mount("slot.missing", new WeaponDefinitionId("weapon.missing"))));
            WeaponFireResult result = runtime.TryFire(new WeaponSlotId("slot.none"), Request());
            Assert.AreEqual(WeaponFailureReason.UnknownSlot, result.FailureReason);
        }

        [Test]
        public void ProjectileAdapterFailureIsReported()
        {
            WeaponRuntime runtime = Runtime(new FailingProjectileAdapter());
            runtime.RegisterWeapon(Mount("slot.projectile", ProjectileWeaponId));
            WeaponFireResult result = runtime.TryFire(new WeaponSlotId("slot.projectile"), Request());
            Assert.False(result.Succeeded);
            Assert.AreEqual(WeaponFailureReason.ProjectileFailed, result.FailureReason);
        }

        [Test]
        public void DefenseGamesCompositionProofUsesNoRuntimeDependency()
        {
            var objective = new DefenseObjectiveDefinition(new DefenseObjectiveId("base"), 100);
            var definition = new DefenseRuntimeDefinition(new[] { objective });
            Assert.AreEqual("base", definition.Objectives[0].Id.Value);

            WeaponRuntime runtime = Runtime(Projector());
            runtime.RegisterWeapon(Mount("tower.01.primary", ProjectileWeaponId));
            WeaponFireResult result = runtime.TryFire(new WeaponSlotId("tower.01.primary"), Request());

            Assert.True(result.Succeeded);
            Assert.AreEqual(WeaponIntentKind.ProjectileLaunch, result.Intents[0].Kind);
        }

        [Test]
        public void DonorIdleAndClassicTowerDefenseProofsShareSameApi()
        {
            WeaponRuntime survivorLike = Runtime(Projector());
            survivorLike.RegisterWeapon(Mount("donor.player.weapon.01", ProjectileWeaponId));
            Assert.True(survivorLike.TryFire(new WeaponSlotId("donor.player.weapon.01"), Request()).Succeeded);

            WeaponRuntime idleBatch = Runtime(Projector());
            idleBatch.RegisterWeapon(Mount("idle.defender.weapon.01", ProjectileWeaponId));
            idleBatch.Tick(60);
            Assert.True(idleBatch.FireReady(Request()).Succeeded);

            WeaponRuntime tower = Runtime(Projector());
            tower.RegisterWeapon(Mount("tower.01.weapon", ProjectileWeaponId));
            Assert.True(tower.TryFire(new WeaponSlotId("tower.01.weapon"), Request()).Succeeded);
        }

        [Test]
        public void BenchmarkRecordsWeaponFireEvaluationCosts()
        {
            var counts = new[] { 1000, 5000, 10000 };
            for (int c = 0; c < counts.Length; c++)
            {
                int count = counts[c];
                var definitions = new[] { ProjectileDefinition() };
                var runtime = new WeaponRuntime(definitions, null, Projector());
                for (int i = 0; i < count; i++)
                    runtime.RegisterWeapon(Mount("slot." + i.ToString("D5"), ProjectileWeaponId));
                WeaponFireRequest request = Request();
                runtime.FireReady(request);
                runtime.Tick(3);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                long before = GC.GetAllocatedBytesForCurrentThread();
                var stopwatch = Stopwatch.StartNew();
                WeaponFireResult result = runtime.FireReady(request);
                stopwatch.Stop();
                long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

                Assert.AreEqual(count, result.FiredCount);
                Debug.Log($"WeaponSystems benchmark Unity=6000.3.5f1 operations={count} weapons={count} candidates=0 mix=100% projectile elapsedMs={stopwatch.Elapsed.TotalMilliseconds:F3} allocationsBytes={allocated} path=Tests/EditMode/WeaponSystemsTests.BenchmarkRecordsWeaponFireEvaluationCosts");
                runtime.Tick(3);
            }
        }

        private static readonly AttackDefinitionId AttackId = new AttackDefinitionId("attack.basic");
        private static readonly ProjectileDefinitionId ProjectileId = new ProjectileDefinitionId("projectile.basic");
        private static readonly WeaponDefinitionId DirectWeaponId = new WeaponDefinitionId("weapon.direct");
        private static readonly WeaponDefinitionId ProjectileWeaponId = new WeaponDefinitionId("weapon.projectile");
        private static readonly WeaponDefinitionId SpreadWeaponId = new WeaponDefinitionId("weapon.spread");
        private static readonly DamageTypeId DamageType = new DamageTypeId("damage.kinetic");

        private static IReadOnlyList<WeaponDefinition> Definitions()
        {
            return new[]
            {
                new WeaponDefinition(DirectWeaponId, WeaponFireMode.DirectAttack, AttackId, 3),
                ProjectileDefinition(),
                new WeaponDefinition(SpreadWeaponId, WeaponFireMode.Projectile, AttackId, 3, ProjectileId, 3, new WeaponFirePattern(2, 15f))
            };
        }

        private static WeaponDefinition ProjectileDefinition() => new WeaponDefinition(ProjectileWeaponId, WeaponFireMode.Projectile, AttackId, 3, ProjectileId);
        private static WeaponRuntime Runtime(IWeaponProjectileAdapter projectileAdapter) => new WeaponRuntime(Definitions(), null, projectileAdapter);
        private static IWeaponProjectileAdapter Projector() => new ProjectileLaunchWeaponAdapter();
        private static AttackSourceSnapshot Source() => new AttackSourceSnapshot(new AttackSourceId("source.player"), new CombatantId("player"));
        private static WeaponMountSnapshot Mount(string slot, WeaponDefinitionId definitionId, int remaining = 0) => new WeaponMountSnapshot(new WeaponSlotId(slot), definitionId, Source(), true, remaining);
        private static WeaponFireRequest Request() => new WeaponFireRequest(Array.Empty<AttackTargetCandidate>(), Vector3.zero, Vector3.forward);
        private static WeaponFireRequest Request(HealthState target) => new WeaponFireRequest(new[] { new AttackTargetCandidate(target.Id, target, 10d) }, Vector3.zero, Vector3.forward);
        private static HealthState Target() => new HealthState(new CombatantId("enemy"), 100, 100);
        private static CombatCatalog Catalog() => new CombatCatalog(new[] { new DamageTypeDefinition(DamageType) });

        private static AttackRuntime AttackRuntime(HealthState target)
        {
            var attacks = new AttackRuntime(Catalog(), new[] { new AttackDefinition(AttackId, 0, DamageType, 10d) });
            attacks.RegisterSource(Source());
            return attacks;
        }

        private static ProjectileRuntime ProjectileRuntime(FakeProjectileSpawner spawner)
        {
            var definition = new ProjectileDefinition(ProjectileId, new WorldSpawnableId("spawnable.projectile"), DamageType, 10d, 30, 5f);
            return new ProjectileRuntime(Catalog(), new[] { definition }, spawner, new FakeProjectileNavigator());
        }

        private sealed class FailingAttackAdapter : IWeaponAttackAdapter
        {
            public WeaponAttackAdapterResult TryCreateIntent(WeaponDefinition definition, WeaponMountSnapshot mount, IReadOnlyList<AttackTargetCandidate> candidates)
            {
                return new WeaponAttackAdapterResult(false, WeaponFailureReason.AttackFailed, null);
            }
        }

        private sealed class FailingProjectileAdapter : IWeaponProjectileAdapter
        {
            public WeaponProjectileAdapterResult TryCreateLaunchRequest(WeaponDefinition definition, WeaponMountSnapshot mount, WeaponFireRequest request)
            {
                return new WeaponProjectileAdapterResult(false, WeaponFailureReason.ProjectileFailed, default);
            }
        }

        private sealed class FakeProjectileSpawner : IProjectileSpawner
        {
            private readonly List<GameObject> _created = new List<GameObject>();
            public ProjectileSpawnResult Spawn(ProjectileDefinition definition, ProjectileLaunchRequest request)
            {
                var instance = new GameObject("projectile-test");
                _created.Add(instance);
                return new ProjectileSpawnResult(true, new ProjectileSpawnHandle(_created.Count), instance);
            }

            public void Despawn(ProjectileSpawnHandle handle, ProjectileExpiryReason reason) { }

            public void DestroyCreated()
            {
                for (int i = 0; i < _created.Count; i++)
                    if (_created[i] != null) Object.DestroyImmediate(_created[i]);
                _created.Clear();
            }
        }

        private sealed class FakeProjectileNavigator : IProjectileNavigator
        {
            public ProjectileNavigationResult Start(GameObject instance, ProjectileDefinition definition, ProjectileLaunchRequest request) => new ProjectileNavigationResult(true, new MovementAgentId(1));
            public void Stop(MovementAgentId agentId) { }
            public bool TryGetProgress(MovementAgentId agentId, out MovementProgress progress) { progress = default; return false; }
        }
    }
}
