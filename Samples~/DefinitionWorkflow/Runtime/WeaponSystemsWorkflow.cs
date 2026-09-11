using System;
using UnityEngine;

namespace Deucarian.WeaponSystems.Samples.DefinitionWorkflow
{
    /// <summary>Small caller example. The configured scene hosts own services and resource lifetimes.</summary>
    public sealed class WeaponSystemsWorkflow : MonoBehaviour
    {
        [SerializeField] private WeaponHost host;
        [SerializeField] private WeaponKey weapon;
        [SerializeField] private WeaponSlotKey slot = SampleWeaponSlots.Primary;
        [SerializeField] private WeaponEquipTrigger trigger;
        [SerializeField] private SampleWeaponSetup setup;
        private string status = "Ready. Choose an action below.";
        public string Status => status;
        public void Equip() { status = "Equip: " + host.Equip(slot, weapon).Status; }
        public void EquipComponent() { trigger.Equip(); status = "Equip: " + trigger.LastResult.Status; }
        public void Fire() { var result = setup.Fire(); status = "Fire: " + result.FailureReason + ". Target health: " + setup.TargetHealth; }
        public void Unequip() { host.Unequip(slot); status = "Unequipped."; }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(24, 24, Math.Min(540, Screen.width - 48), Screen.height - 48), GUI.skin.box);
            GUILayout.Label("Weapon-Systems — definition workflow");
            GUILayout.Label("The weapon definition reuses its attack. Setup registers the allowed slot once; equipping again preserves cooldowns. The sample routes direct attack intents to Combat.");
            GUILayout.Space(12);
            if (GUILayout.Button("Equip with C#", GUILayout.Height(32))) { try { Equip(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Equip from component", GUILayout.Height(32))) { try { EquipComponent(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Fire direct weapon", GUILayout.Height(32))) { try { Fire(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Unequip", GUILayout.Height(32))) { try { Unequip(); } catch (Exception error) { status = error.Message; } }
            GUILayout.Space(12);
            GUILayout.Label(status);
            GUILayout.EndArea();
        }
    }
}
