using System;
using System.Collections.Generic;
using Deucarian.Attacks;
using Deucarian.GameplayFoundation;
using Deucarian.Projectiles;
using UnityEngine;

namespace Deucarian.WeaponSystems
{
    /// <summary>Stable authored identifier for a weapon definition.</summary>
    public readonly struct WeaponDefinitionId : IEquatable<WeaponDefinitionId>, IComparable<WeaponDefinitionId>
    {
        private readonly ContentId _value;
        public WeaponDefinitionId(string value) { _value = new ContentId(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(WeaponDefinitionId other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is WeaponDefinitionId other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(WeaponDefinitionId other) => _value.CompareTo(other._value);
        public override string ToString() => Value;
    }

    /// <summary>Stable identifier for a mounted runtime weapon slot.</summary>
    public readonly struct WeaponSlotId : IEquatable<WeaponSlotId>, IComparable<WeaponSlotId>
    {
        private readonly ContentId _value;
        public WeaponSlotId(string value) { _value = new ContentId(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(WeaponSlotId other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is WeaponSlotId other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(WeaponSlotId other) => _value.CompareTo(other._value);
        public override string ToString() => Value;
    }

    /// <summary>Weapon output mode. Actual damage and projectile lifecycles are delegated.</summary>
    public enum WeaponFireMode { DirectAttack = 0, Projectile = 1 }

    /// <summary>Reason a weapon fire attempt did not produce all requested intents.</summary>
    public enum WeaponFailureReason
    {
        None = 0,
        UnknownSlot = 1,
        UnknownDefinition = 2,
        Disabled = 3,
        NotReady = 4,
        NoCandidates = 5,
        AttackFailed = 6,
        ProjectileFailed = 7,
        InvalidInput = 8
    }

    /// <summary>Kind of intent emitted by a weapon.</summary>
    public enum WeaponIntentKind { DirectAttack = 0, ProjectileLaunch = 1 }

    /// <summary>Lightweight authored pattern descriptor. Aiming and steering remain caller-owned.</summary>
    public readonly struct WeaponFirePattern
    {
        public WeaponFirePattern(int volleyCount = 1, float spreadDegrees = 0f)
        {
            if (volleyCount <= 0) throw new ArgumentOutOfRangeException(nameof(volleyCount));
            if (float.IsNaN(spreadDegrees) || float.IsInfinity(spreadDegrees) || spreadDegrees < 0f) throw new ArgumentOutOfRangeException(nameof(spreadDegrees));
            VolleyCount = volleyCount;
            SpreadDegrees = spreadDegrees;
        }

        public int VolleyCount { get; }
        public float SpreadDegrees { get; }
        public static WeaponFirePattern Single => new WeaponFirePattern(1, 0f);
    }

    /// <summary>Authored weapon data. It references attack and projectile definitions but does not own them.</summary>
    public sealed class WeaponDefinition
    {
        public WeaponDefinition(WeaponDefinitionId id, WeaponFireMode fireMode, AttackDefinitionId attackDefinitionId, int cooldownTicks, ProjectileDefinitionId projectileDefinitionId = default, int burstCount = 1, WeaponFirePattern pattern = default)
        {
            if (id.IsEmpty) throw new ArgumentException("Weapon definition id cannot be empty.", nameof(id));
            if (attackDefinitionId.IsEmpty) throw new ArgumentException("Attack definition id cannot be empty.", nameof(attackDefinitionId));
            if (cooldownTicks < 0) throw new ArgumentOutOfRangeException(nameof(cooldownTicks));
            if (burstCount <= 0) throw new ArgumentOutOfRangeException(nameof(burstCount));
            if (fireMode == WeaponFireMode.Projectile && projectileDefinitionId.IsEmpty) throw new ArgumentException("Projectile weapons require a projectile definition id.", nameof(projectileDefinitionId));
            Id = id;
            FireMode = fireMode;
            AttackDefinitionId = attackDefinitionId;
            ProjectileDefinitionId = projectileDefinitionId;
            CooldownTicks = cooldownTicks;
            BurstCount = burstCount;
            Pattern = pattern.VolleyCount == 0 ? WeaponFirePattern.Single : pattern;
        }

        public WeaponDefinitionId Id { get; }
        public WeaponFireMode FireMode { get; }
        public AttackDefinitionId AttackDefinitionId { get; }
        public ProjectileDefinitionId ProjectileDefinitionId { get; }
        public int CooldownTicks { get; }
        public int BurstCount { get; }
        public WeaponFirePattern Pattern { get; }
        public int IntentsPerFire => BurstCount * Pattern.VolleyCount;
    }

    /// <summary>Caller-owned source data for a mounted weapon.</summary>
    public readonly struct WeaponSourceSnapshot
    {
        public WeaponSourceSnapshot(AttackSourceSnapshot attackSource)
        {
            AttackSource = attackSource;
        }

        public AttackSourceSnapshot AttackSource { get; }
    }

    /// <summary>Serializable runtime state for one mounted weapon slot.</summary>
    public readonly struct WeaponMountSnapshot
    {
        public WeaponMountSnapshot(WeaponSlotId slotId, WeaponDefinitionId definitionId, AttackSourceSnapshot source, bool enabled = true, int remainingCooldownTicks = 0)
        {
            if (slotId.IsEmpty) throw new ArgumentException("Slot id cannot be empty.", nameof(slotId));
            if (definitionId.IsEmpty) throw new ArgumentException("Weapon definition id cannot be empty.", nameof(definitionId));
            if (remainingCooldownTicks < 0) throw new ArgumentOutOfRangeException(nameof(remainingCooldownTicks));
            SlotId = slotId;
            DefinitionId = definitionId;
            Source = source;
            Enabled = enabled;
            RemainingCooldownTicks = remainingCooldownTicks;
        }

        public WeaponSlotId SlotId { get; }
        public WeaponDefinitionId DefinitionId { get; }
        public AttackSourceSnapshot Source { get; }
        public bool Enabled { get; }
        public int RemainingCooldownTicks { get; }
        public bool IsReady => RemainingCooldownTicks <= 0;
    }

    /// <summary>Caller-supplied fire context. Target discovery, range checks, and aim are external.</summary>
    public sealed class WeaponFireRequest
    {
        public WeaponFireRequest(IReadOnlyList<AttackTargetCandidate> candidates, Vector3 origin, Vector3 destination, IReadOnlyList<Vector3> path = null)
        {
            Candidates = candidates ?? Array.Empty<AttackTargetCandidate>();
            Origin = origin;
            Destination = destination;
            Path = Copy(path);
        }

        public IReadOnlyList<AttackTargetCandidate> Candidates { get; }
        public Vector3 Origin { get; }
        public Vector3 Destination { get; }
        public IReadOnlyList<Vector3> Path { get; }
        public bool HasCandidates => Candidates.Count > 0;

        private static Vector3[] Copy(IReadOnlyList<Vector3> source)
        {
            if (source == null) return Array.Empty<Vector3>();
            var copy = new Vector3[source.Count];
            for (int i = 0; i < source.Count; i++) copy[i] = source[i];
            return copy;
        }
    }

    /// <summary>One emitted direct or projectile weapon action.</summary>
    public sealed class WeaponIntent
    {
        public WeaponIntent(WeaponIntentKind kind, WeaponSlotId slotId, WeaponDefinitionId definitionId, int burstIndex, int volleyIndex, WeaponFirePattern pattern, AttackIntent attackIntent = null, ProjectileLaunchRequest projectileLaunchRequest = default)
        {
            Kind = kind;
            SlotId = slotId;
            DefinitionId = definitionId;
            BurstIndex = burstIndex;
            VolleyIndex = volleyIndex;
            Pattern = pattern;
            AttackIntent = attackIntent;
            ProjectileLaunchRequest = projectileLaunchRequest;
        }

        public WeaponIntentKind Kind { get; }
        public WeaponSlotId SlotId { get; }
        public WeaponDefinitionId DefinitionId { get; }
        public int BurstIndex { get; }
        public int VolleyIndex { get; }
        public WeaponFirePattern Pattern { get; }
        public AttackIntent AttackIntent { get; }
        public ProjectileLaunchRequest ProjectileLaunchRequest { get; }
    }

    /// <summary>Result for one or more slot fire attempts.</summary>
    public sealed class WeaponFireResult
    {
        public WeaponFireResult(bool succeeded, WeaponFailureReason failureReason, IReadOnlyList<WeaponIntent> intents, int firedCount, int failureCount)
        {
            Succeeded = succeeded;
            FailureReason = failureReason;
            Intents = Copy(intents);
            FiredCount = firedCount;
            FailureCount = failureCount;
        }

        public bool Succeeded { get; }
        public WeaponFailureReason FailureReason { get; }
        public IReadOnlyList<WeaponIntent> Intents { get; }
        public int FiredCount { get; }
        public int FailureCount { get; }

        private static WeaponIntent[] Copy(IReadOnlyList<WeaponIntent> source)
        {
            if (source == null) return Array.Empty<WeaponIntent>();
            var copy = new WeaponIntent[source.Count];
            for (int i = 0; i < source.Count; i++) copy[i] = source[i];
            return copy;
        }
    }

    /// <summary>Deterministic runtime reconstruction data.</summary>
    public sealed class WeaponSnapshot
    {
        public WeaponSnapshot(IReadOnlyList<WeaponMountSnapshot> mounts)
        {
            Mounts = Copy(mounts);
        }

        public IReadOnlyList<WeaponMountSnapshot> Mounts { get; }

        private static WeaponMountSnapshot[] Copy(IReadOnlyList<WeaponMountSnapshot> source)
        {
            if (source == null) return Array.Empty<WeaponMountSnapshot>();
            var copy = new WeaponMountSnapshot[source.Count];
            for (int i = 0; i < source.Count; i++) copy[i] = source[i];
            Array.Sort(copy, (a, b) => a.SlotId.CompareTo(b.SlotId));
            return copy;
        }
    }

    public readonly struct WeaponAttackAdapterResult
    {
        public WeaponAttackAdapterResult(bool succeeded, WeaponFailureReason failureReason, AttackIntent intent)
        {
            Succeeded = succeeded;
            FailureReason = failureReason;
            Intent = intent;
        }

        public bool Succeeded { get; }
        public WeaponFailureReason FailureReason { get; }
        public AttackIntent Intent { get; }
    }

    public interface IWeaponAttackAdapter
    {
        WeaponAttackAdapterResult TryCreateIntent(WeaponDefinition definition, WeaponMountSnapshot mount, IReadOnlyList<AttackTargetCandidate> candidates);
    }

    /// <summary>Adapter that delegates direct attacks to an Attacks runtime.</summary>
    public sealed class AttackRuntimeWeaponAttackAdapter : IWeaponAttackAdapter
    {
        private readonly AttackRuntime _runtime;

        public AttackRuntimeWeaponAttackAdapter(AttackRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public WeaponAttackAdapterResult TryCreateIntent(WeaponDefinition definition, WeaponMountSnapshot mount, IReadOnlyList<AttackTargetCandidate> candidates)
        {
            if (definition == null) return new WeaponAttackAdapterResult(false, WeaponFailureReason.InvalidInput, null);
            AttackResult result = _runtime.TryAttack(mount.Source.Id, definition.AttackDefinitionId, candidates);
            return result.Succeeded
                ? new WeaponAttackAdapterResult(true, WeaponFailureReason.None, result.Intent)
                : new WeaponAttackAdapterResult(false, Map(result.FailureReason), null);
        }

        private static WeaponFailureReason Map(AttackFailureReason reason)
        {
            if (reason == AttackFailureReason.NoCandidates || reason == AttackFailureReason.InvalidCandidate) return WeaponFailureReason.NoCandidates;
            if (reason == AttackFailureReason.NotReady) return WeaponFailureReason.NotReady;
            if (reason == AttackFailureReason.SourceDisabled) return WeaponFailureReason.Disabled;
            if (reason == AttackFailureReason.InvalidInput) return WeaponFailureReason.InvalidInput;
            return WeaponFailureReason.AttackFailed;
        }
    }

    public readonly struct WeaponProjectileAdapterResult
    {
        public WeaponProjectileAdapterResult(bool succeeded, WeaponFailureReason failureReason, ProjectileLaunchRequest request)
        {
            Succeeded = succeeded;
            FailureReason = failureReason;
            Request = request;
        }

        public bool Succeeded { get; }
        public WeaponFailureReason FailureReason { get; }
        public ProjectileLaunchRequest Request { get; }
    }

    public interface IWeaponProjectileAdapter
    {
        WeaponProjectileAdapterResult TryCreateLaunchRequest(WeaponDefinition definition, WeaponMountSnapshot mount, WeaponFireRequest request);
    }

    /// <summary>Default adapter that creates Projectiles launch requests without launching them.</summary>
    public sealed class ProjectileLaunchWeaponAdapter : IWeaponProjectileAdapter
    {
        public WeaponProjectileAdapterResult TryCreateLaunchRequest(WeaponDefinition definition, WeaponMountSnapshot mount, WeaponFireRequest request)
        {
            if (definition == null || request == null) return new WeaponProjectileAdapterResult(false, WeaponFailureReason.InvalidInput, default);
            if (definition.ProjectileDefinitionId.IsEmpty) return new WeaponProjectileAdapterResult(false, WeaponFailureReason.ProjectileFailed, default);
            var launch = new ProjectileLaunchRequest(definition.ProjectileDefinitionId, mount.Source.Id, definition.AttackDefinitionId, mount.Source, request.Origin, request.Destination, request.Path);
            return new WeaponProjectileAdapterResult(true, WeaponFailureReason.None, launch);
        }
    }

    /// <summary>Deterministic fixed-tick weapon orchestration runtime.</summary>
    public sealed class WeaponRuntime
    {
        private readonly Dictionary<WeaponDefinitionId, WeaponDefinition> _definitions = new Dictionary<WeaponDefinitionId, WeaponDefinition>();
        private readonly Dictionary<WeaponSlotId, State> _mounts = new Dictionary<WeaponSlotId, State>();
        private readonly List<WeaponSlotId> _orderedSlots = new List<WeaponSlotId>();
        private readonly IWeaponAttackAdapter _attackAdapter;
        private readonly IWeaponProjectileAdapter _projectileAdapter;

        public WeaponRuntime(IReadOnlyList<WeaponDefinition> definitions, IWeaponAttackAdapter attackAdapter, IWeaponProjectileAdapter projectileAdapter)
        {
            if (definitions == null || definitions.Count == 0) throw new ArgumentException("At least one weapon definition is required.", nameof(definitions));
            for (int i = 0; i < definitions.Count; i++)
            {
                WeaponDefinition definition = definitions[i] ?? throw new ArgumentException("Weapon definition cannot be null.");
                if (_definitions.ContainsKey(definition.Id)) throw new ArgumentException("Duplicate weapon definition: " + definition.Id);
                _definitions.Add(definition.Id, definition);
            }

            _attackAdapter = attackAdapter;
            _projectileAdapter = projectileAdapter;
        }

        public int RegisteredCount => _orderedSlots.Count;

        public bool RegisterWeapon(WeaponMountSnapshot mount)
        {
            if (!_definitions.ContainsKey(mount.DefinitionId)) return false;
            bool exists = _mounts.ContainsKey(mount.SlotId);
            _mounts[mount.SlotId] = new State(mount);
            if (!exists)
            {
                _orderedSlots.Add(mount.SlotId);
                _orderedSlots.Sort();
            }

            return true;
        }

        public bool RemoveWeapon(WeaponSlotId slotId)
        {
            if (!_mounts.Remove(slotId)) return false;
            _orderedSlots.Remove(slotId);
            return true;
        }

        public bool SetEnabled(WeaponSlotId slotId, bool enabled)
        {
            if (!_mounts.TryGetValue(slotId, out State state)) return false;
            state.Mount = new WeaponMountSnapshot(state.Mount.SlotId, state.Mount.DefinitionId, state.Mount.Source, enabled, state.Mount.RemainingCooldownTicks);
            return true;
        }

        public bool TryGetMount(WeaponSlotId slotId, out WeaponMountSnapshot mount)
        {
            if (_mounts.TryGetValue(slotId, out State state))
            {
                mount = state.Mount;
                return true;
            }

            mount = default;
            return false;
        }

        public void Tick(int ticks)
        {
            if (ticks < 0) throw new ArgumentOutOfRangeException(nameof(ticks));
            for (int i = 0; i < _orderedSlots.Count; i++)
            {
                State state = _mounts[_orderedSlots[i]];
                int remaining = Math.Max(0, state.Mount.RemainingCooldownTicks - ticks);
                state.Mount = new WeaponMountSnapshot(state.Mount.SlotId, state.Mount.DefinitionId, state.Mount.Source, state.Mount.Enabled, remaining);
            }
        }

        public WeaponFireResult TryFire(WeaponSlotId slotId, WeaponFireRequest request)
        {
            var intents = new List<WeaponIntent>();
            WeaponFailureReason failure = TryFireInternal(slotId, request, intents);
            bool succeeded = failure == WeaponFailureReason.None;
            return new WeaponFireResult(succeeded, failure, intents, succeeded ? 1 : 0, succeeded ? 0 : 1);
        }

        public WeaponFireResult FireReady(WeaponFireRequest request)
        {
            var intents = new List<WeaponIntent>();
            int fired = 0;
            int failures = 0;
            WeaponFailureReason firstFailure = WeaponFailureReason.None;

            for (int i = 0; i < _orderedSlots.Count; i++)
            {
                WeaponFailureReason failure = TryFireInternal(_orderedSlots[i], request, intents);
                if (failure == WeaponFailureReason.None)
                {
                    fired++;
                }
                else if (failure != WeaponFailureReason.NotReady)
                {
                    failures++;
                    if (firstFailure == WeaponFailureReason.None) firstFailure = failure;
                }
            }

            return new WeaponFireResult(fired > 0 && failures == 0, firstFailure, intents, fired, failures);
        }

        public WeaponSnapshot CreateSnapshot()
        {
            var mounts = new WeaponMountSnapshot[_orderedSlots.Count];
            for (int i = 0; i < _orderedSlots.Count; i++) mounts[i] = _mounts[_orderedSlots[i]].Mount;
            return new WeaponSnapshot(mounts);
        }

        public static WeaponRuntime FromSnapshot(IReadOnlyList<WeaponDefinition> definitions, IWeaponAttackAdapter attackAdapter, IWeaponProjectileAdapter projectileAdapter, WeaponSnapshot snapshot)
        {
            var runtime = new WeaponRuntime(definitions, attackAdapter, projectileAdapter);
            if (snapshot == null) return runtime;
            for (int i = 0; i < snapshot.Mounts.Count; i++) runtime.RegisterWeapon(snapshot.Mounts[i]);
            return runtime;
        }

        private WeaponFailureReason TryFireInternal(WeaponSlotId slotId, WeaponFireRequest request, List<WeaponIntent> intents)
        {
            if (request == null) return WeaponFailureReason.InvalidInput;
            if (!_mounts.TryGetValue(slotId, out State state)) return WeaponFailureReason.UnknownSlot;
            if (!_definitions.TryGetValue(state.Mount.DefinitionId, out WeaponDefinition definition)) return WeaponFailureReason.UnknownDefinition;
            if (!state.Mount.Enabled) return WeaponFailureReason.Disabled;
            if (!state.Mount.IsReady) return WeaponFailureReason.NotReady;
            if (definition.FireMode == WeaponFireMode.DirectAttack && !request.HasCandidates) return WeaponFailureReason.NoCandidates;

            int startCount = intents.Count;
            for (int burst = 0; burst < definition.BurstCount; burst++)
            {
                for (int volley = 0; volley < definition.Pattern.VolleyCount; volley++)
                {
                    WeaponFailureReason failure = CreateIntent(definition, state.Mount, request, burst, volley, intents);
                    if (failure != WeaponFailureReason.None)
                    {
                        if (intents.Count > startCount) intents.RemoveRange(startCount, intents.Count - startCount);
                        return failure;
                    }
                }
            }

            state.Mount = new WeaponMountSnapshot(state.Mount.SlotId, state.Mount.DefinitionId, state.Mount.Source, state.Mount.Enabled, definition.CooldownTicks);
            return WeaponFailureReason.None;
        }

        private WeaponFailureReason CreateIntent(WeaponDefinition definition, WeaponMountSnapshot mount, WeaponFireRequest request, int burstIndex, int volleyIndex, List<WeaponIntent> intents)
        {
            if (definition.FireMode == WeaponFireMode.DirectAttack)
            {
                if (_attackAdapter == null) return WeaponFailureReason.AttackFailed;
                WeaponAttackAdapterResult result = _attackAdapter.TryCreateIntent(definition, mount, request.Candidates);
                if (!result.Succeeded) return result.FailureReason;
                intents.Add(new WeaponIntent(WeaponIntentKind.DirectAttack, mount.SlotId, definition.Id, burstIndex, volleyIndex, definition.Pattern, result.Intent));
                return WeaponFailureReason.None;
            }

            if (_projectileAdapter == null) return WeaponFailureReason.ProjectileFailed;
            WeaponProjectileAdapterResult projectile = _projectileAdapter.TryCreateLaunchRequest(definition, mount, request);
            if (!projectile.Succeeded) return projectile.FailureReason;
            intents.Add(new WeaponIntent(WeaponIntentKind.ProjectileLaunch, mount.SlotId, definition.Id, burstIndex, volleyIndex, definition.Pattern, projectileLaunchRequest: projectile.Request));
            return WeaponFailureReason.None;
        }

        private sealed class State
        {
            public State(WeaponMountSnapshot mount) { Mount = mount; }
            public WeaponMountSnapshot Mount;
        }
    }
}
