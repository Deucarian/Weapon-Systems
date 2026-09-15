using System;
using Deucarian.Attacks;
using Deucarian.Combat;
using Deucarian.Combat.Unity;
using Deucarian.WeaponSystems.Authoring;
using UnityEngine;
namespace Deucarian.WeaponSystems.Samples.DefinitionWorkflow
{
    [DefaultExecutionOrder(-2000)]
    public sealed class SampleWeaponSetup : MonoBehaviour
    {
        [SerializeField] private WeaponHost host;
        [SerializeField] private WeaponDefinitionAsset weapon;
        [SerializeField] private Combatant target;
        private AttackRuntime attacks;
        private WeaponRuntime weapons;
        public double TargetHealth => target.CurrentHealth;
        private void Awake()
        {
            var source = new AttackSourceSnapshot(new AttackSourceId(Guid.NewGuid().ToString("N")), new CombatantId(Guid.NewGuid().ToString("N")));
            attacks = new AttackRuntime(CombatDefinitionCatalog.LoadProject().CreateCatalog(), new[] { weapon.Stats.Attack.ToRuntimeDefinition() });
            attacks.RegisterSource(source);
            weapons = new WeaponRuntime(new[] { weapon.ToRuntimeDefinition() }, new AttackRuntimeWeaponAttackAdapter(attacks), new ProjectileLaunchWeaponAdapter());
            host.Configure(weapons, () => source, new[] { SampleWeaponSlots.Primary });
        }
        public WeaponFireResult Fire()
        {
            target.Handle.TryGetState(out var health, out _, out var defense);
            var result = host.Fire(SampleWeaponSlots.Primary, new WeaponFireRequest(new[] { new AttackTargetCandidate(health.Id, health, 1, defense: defense) }, transform.position, target.transform.position));
            if (result.Succeeded) foreach (var intent in result.Intents) CombatDamageResolver.Resolve(intent.AttackIntent.ResolutionRequest);
            return result;
        }
        private void FixedUpdate() { attacks?.Tick(1); weapons?.Tick(1); }
    }
}
