using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModularWeaponSlotManager : MonoBehaviour
{
    public static ModularWeaponSlotManager Instance { get; private set; }

    [Header("Weapon Slots (3 total)")]
    [SerializeField] private ModularWeaponCombo[] weaponSlots = new ModularWeaponCombo[3];

    [Header("All Available Weapons")]
    [SerializeField] private ModularWeaponCombo[] allWeapons; // Drag Dagger, Gauntlet, Slingshot here

    private void Awake()
    {
        Instance = this;

        foreach (var w in allWeapons)
        {
            if (w != null)
                w.gameObject.SetActive(false);
        }

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i] = null;
        }
    }

    private void Start()
    {
        ApplyEquippedWeaponsFromPlayerData();
    }

    public void ApplyEquippedWeaponsFromPlayerData()
    {
        if (PlayerData.equippedWeapons == null || PlayerData.equippedWeapons.Length == 0)
        {
            Debug.Log("[WeaponSlotManager] No weapons to apply from PlayerData.");
            return;
        }

        for (int i = 0; i < PlayerData.equippedWeapons.Length && i < weaponSlots.Length; i++)
        {
            WeaponType type = PlayerData.equippedWeapons[i];

            if (type == WeaponType.None) continue;

            ModularWeaponCombo weapon = System.Array.Find(allWeapons, w => w.weaponType == type);

            if (weapon != null)
            {
                weapon.gameObject.SetActive(true);
                weapon.suppressInput = false;
                EquipWeaponToSlot(weapon, (ModularWeaponSlotKey)i);
                WeaponEquipUI.Instance?.UpdateSlotVisual((ModularWeaponSlotKey)i, type);
            }
        }
    }

    /// <summary>
    /// Assigns weapon to slot, removing it from any previous slot first.
    /// </summary>
    public void AssignWeaponToSlot(WeaponType type, ModularWeaponSlotKey slot)
    {
        if (type == WeaponType.None)
        {
            Debug.LogWarning("Cannot assign WeaponType.None to a slot.");
            return;
        }

        ModularWeaponCombo weapon = System.Array.Find(allWeapons, w => w.weaponType == type);
        if (weapon == null)
        {
            Debug.LogWarning($"Weapon of type {type} not found in allWeapons.");
            return;
        }

        weapon.gameObject.SetActive(true);
        StartCoroutine(EnableInputWithDelay(weapon, 0.5f));

        EquipWeaponToSlot(weapon, slot);

        PlayerData.equippedWeapons[(int)slot] = type;
        Debug.Log($"[WeaponSlotManager] Assigned {type} to slot {slot} and saved to PlayerData.");
    }
    private IEnumerator EnableInputWithDelay(ModularWeaponCombo weapon, float delay)
    {
        yield return new WaitForSeconds(delay);
        weapon.suppressInput = false;
        Debug.Log($"[WeaponSlotManager] Input enabled for {weapon.weaponType} after delay.");
    }

    public void EquipWeaponToSlot(ModularWeaponCombo weapon, ModularWeaponSlotKey slot)
    {
        weaponSlots[(int)slot] = weapon;
    }

    public ModularWeaponCombo GetWeaponInSlot(ModularWeaponSlotKey slot)
    {
        return weaponSlots[(int)slot];
    }

    public ModularWeaponCombo GetWeaponByKeyCode(KeyCode key)
    {
        return key switch
        {
            KeyCode.J => weaponSlots[0],
            KeyCode.K => weaponSlots[1],
            KeyCode.L => weaponSlots[2],
            _ => null,
        };
    }

    public ModularWeaponCombo[] GetAllWeapons() => weaponSlots;

    /// <summary>
    /// Suppress input across all weapons
    /// </summary>
    public void SuppressInputForAllWeapons(bool suppress)
    {
        foreach (var weapon in weaponSlots)
        {
            if (weapon != null)
                weapon.suppressInput = suppress;
        }
    }

    public ModularWeaponCombo GetWeaponByType(WeaponType type)
    {
        foreach (var weapon in allWeapons)
        {
            if (weapon != null && weapon.weaponType == type)
                return weapon;
        }
        return null;
    }
}
