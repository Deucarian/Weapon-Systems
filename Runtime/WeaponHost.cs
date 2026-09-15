using Deucarian.Diagnostics;
using System;
using System.Collections.Generic;
using Deucarian.Attacks;
using UnityEngine;

namespace Deucarian.WeaponSystems
{
    /// <summary>One owner's typed weapon access. The configured runtime remains the authoritative cadence and mount owner.</summary>
    [DisallowMultipleComponent]
    public sealed class WeaponHost : MonoBehaviour, IDiagnosticProvider
    {
        private WeaponRuntime runtime;
        private Func<AttackSourceSnapshot> captureSource;
        private HashSet<WeaponSlotId> slots;
        private bool destroyed;

        public void Configure(WeaponRuntime value, Func<AttackSourceSnapshot> source, IEnumerable<WeaponSlotKey> allowedSlots)
        {
            if (destroyed) throw new ObjectDisposedException(nameof(WeaponHost));
            if (runtime != null) throw new InvalidOperationException("WeaponHost '" + name + "' is already configured.");
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (allowedSlots == null) throw new ArgumentNullException(nameof(allowedSlots));
            var selected = new HashSet<WeaponSlotId>();
            foreach (var slot in allowedSlots)
            {
                if (slot == null) throw new ArgumentException("Select a key for every allowed weapon slot.", nameof(allowedSlots));
                if (!selected.Add(new WeaponSlotId(slot.Id))) throw new ArgumentException("Weapon slot '" + slot.Id + "' is configured twice. Register it once.", nameof(allowedSlots));
            }
            if (selected.Count == 0) throw new ArgumentException("Configure at least one allowed weapon slot.", nameof(allowedSlots));
            slots = selected;
            captureSource = source;
            runtime = value;
        }

        public WeaponEquipResult Equip(WeaponSlotKey slot, WeaponKey weapon)
        {
            WeaponSlotId slotId = RequireSlot(slot);
            if (weapon == null) throw new ArgumentNullException(nameof(weapon), "Select a WeaponKey or pass a named weapon definition.");
            var definitionId = new WeaponDefinitionId(weapon.Id);
            if (runtime.TryGetMount(slotId, out var current) && current.DefinitionId.Equals(definitionId))
                return new WeaponEquipResult(WeaponEquipStatus.AlreadyEquipped);
            var source = captureSource();
            if (source.Id.IsEmpty || !source.Enabled) return new WeaponEquipResult(WeaponEquipStatus.SourceUnavailable);
            if (!runtime.RegisterWeapon(new WeaponMountSnapshot(slotId, definitionId, source)))
                throw new InvalidOperationException("WeaponHost '" + name + "' cannot equip '" + weapon.Id + "'. Add this weapon definition to its configured WeaponRuntime catalog.");
            return new WeaponEquipResult(WeaponEquipStatus.Equipped);
        }

        public bool Unequip(WeaponSlotKey slot) => runtime.RemoveWeapon(RequireSlot(slot));
        public WeaponFireResult Fire(WeaponSlotKey slot, WeaponFireRequest request)
        {
            var id = RequireSlot(slot);
            if (request == null) throw new ArgumentNullException(nameof(request));
            return runtime.TryFire(id, request);
        }

        private WeaponSlotId RequireSlot(WeaponSlotKey slot)
        {
            if (destroyed) throw new ObjectDisposedException(nameof(WeaponHost));
            if (runtime == null) throw new InvalidOperationException("WeaponHost '" + name + "' is not configured. Supply its runtime, source and allowed slot keys once during startup.");
            if (slot == null) throw new ArgumentNullException(nameof(slot), "Select an allowed WeaponSlotKey.");
            var id = new WeaponSlotId(slot.Id);
            if (!slots.Contains(id)) throw new InvalidOperationException("Weapon slot '" + slot.Id + "' is not configured for '" + name + "'. Add it to this host's allowed slots.");
            return id;
        }

        private void OnDestroy() { diagnosticRegistration?.Dispose(); diagnosticRegistration = null;  destroyed = true; runtime = null; captureSource = null; slots?.Clear(); }
        private DiagnosticProviderRegistration diagnosticRegistration;
        private void Awake() => diagnosticRegistration = DiagnosticProviderRegistry.Register(this);
        string IDiagnosticProvider.ProviderId => "weapon-systems.host." + GetInstanceID();
        string IDiagnosticProvider.DisplayName => "WeaponHost";
        void IDiagnosticProvider.Collect(DiagnosticReportBuilder builder)
        {
            bool configured = runtime != null;
            builder.AddSection(((IDiagnosticProvider)this).ProviderId, "WeaponHost")
                .AddItem("configured", "Configured", configured ? "Ready" : "Call Configure during startup",
                    configured ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning)
                .AddItem("enabled", "Enabled", isActiveAndEnabled.ToString());
        }
    }
}
