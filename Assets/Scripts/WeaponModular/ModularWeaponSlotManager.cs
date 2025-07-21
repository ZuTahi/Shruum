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

    private void ApplyEquippedWeaponsFromPlayerData()
    {
        if (PlayerData.equippedWeapons == null || PlayerData.equippedWeapons.Length == 0)
        {
            Debug.Log("[WeaponSlotManager] No weapons to apply from PlayerData.");
            return;
        }

        for (int i = 0; i < PlayerData.equippedWeapons.Length && i < weaponSlots.Length; i++)
        {
            WeaponType type = PlayerData.equippedWeapons[i];

            if (type == WeaponType.None)
            {
                Debug.Log($"[WeaponSlotManager] Slot {i} is empty (None). Skipping.");
                continue;
            }

            ModularWeaponCombo weapon = System.Array.Find(allWeapons, w => w.weaponType == type);

            if (weapon != null)
            {
                weapon.gameObject.SetActive(true);
                EquipWeaponToSlot(weapon, (ModularWeaponSlotKey)i);
            }
        }

        Debug.Log("[WeaponSlotManager] Equipped weapons from PlayerData.");
    }

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
        EquipWeaponToSlot(weapon, slot);

        PlayerData.equippedWeapons[(int)slot] = type;
        Debug.Log($"[WeaponSlotManager] Assigned {type} to slot {slot} and saved to PlayerData.");
    }

    public void EquipWeaponToSlot(ModularWeaponCombo weapon, ModularWeaponSlotKey slot)
    {
        int index = (int)slot;
        weaponSlots[index] = weapon;
        Debug.Log($"Equipped {weapon.weaponType} to slot {slot}");
    }

    public void EquipWeaponToSlot(ModularWeaponSlotKey slot, WeaponType weaponType)
    {
        if (weaponType == WeaponType.None)
        {
            Debug.LogWarning("Cannot equip WeaponType.None.");
            return;
        }

        ModularWeaponCombo weapon = System.Array.Find(allWeapons, w => w.weaponType == weaponType);
        if (weapon == null)
        {
            Debug.LogWarning($"Weapon of type {weaponType} not found in allWeapons.");
            return;
        }

        weapon.gameObject.SetActive(true);
        EquipWeaponToSlot(weapon, slot);
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
