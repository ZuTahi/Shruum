using UnityEngine;

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

        // Disable all weapons
        foreach (var w in allWeapons)
        {
            if (w != null)
                w.gameObject.SetActive(false);
        }

        // Clear out any pre-assigned slots
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i] = null;
        }
    }

    /// <summary>
    /// Assign weapon to a slot using WeaponType (called by WeaponEquipUI)
    /// </summary>
    public void AssignWeaponToSlot(WeaponType type, ModularWeaponSlotKey slot)
    {
        ModularWeaponCombo weapon = System.Array.Find(allWeapons, w => w.weaponType == type);
        if (weapon == null)
        {
            Debug.LogWarning($"Weapon of type {type} not found in allWeapons.");
            return;
        }

        weapon.gameObject.SetActive(true);
        EquipWeaponToSlot(weapon, slot);
    }

    /// <summary>
    /// Internal logic to put weapon in a slot
    /// </summary>
    public void EquipWeaponToSlot(ModularWeaponCombo weapon, ModularWeaponSlotKey slot)
    {
        int index = (int)slot;
        weaponSlots[index] = weapon;
        Debug.Log($"Equipped {weapon.weaponType} to slot {slot}");
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
}
