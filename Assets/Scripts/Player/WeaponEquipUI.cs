using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponEquipUI : MonoBehaviour
{
    public static WeaponEquipUI Instance;

    [Header("Slot Images (UI placeholders for J/K/L)")]
    public Image slotJImage;
    public Image slotKImage;
    public Image slotLImage;

    [Header("Weapon Sprites")]
    public Sprite daggerSprite;
    public Sprite gauntletSprite;
    public Sprite slingshotSprite;

    [Header("Default Sprites (empty slots)")]
    public Sprite defaultSpriteJ;
    public Sprite defaultSpriteK;
    public Sprite defaultSpriteL;

    [Header("Assigning Sprites (shown when choosing)")]
    public Sprite assigningSpriteJ;
    public Sprite assigningSpriteK;
    public Sprite assigningSpriteL;

    [Header("Equip Prompt")]
    public TextMeshProUGUI promptText;

    private WeaponType currentWeapon;
    private bool isChoosingSlot = false;
    public bool IsChoosingSlot => isChoosingSlot;

    private void Awake()
    {
        Instance = this;
        promptText.gameObject.SetActive(false);

        // Initialize with default sprites
        slotJImage.sprite = defaultSpriteJ;
        slotKImage.sprite = defaultSpriteK;
        slotLImage.sprite = defaultSpriteL;
    }

    private void Update()
    {
        if (!isChoosingSlot) return;

        if (Input.GetKeyDown(KeyCode.J))
            EquipToSlot(ModularWeaponSlotKey.Slot1);
        else if (Input.GetKeyDown(KeyCode.K))
            EquipToSlot(ModularWeaponSlotKey.Slot2);
        else if (Input.GetKeyDown(KeyCode.L))
            EquipToSlot(ModularWeaponSlotKey.Slot3);
    }

    public void OpenEquipPrompt(WeaponType weapon)
    {
        currentWeapon = weapon;
        isChoosingSlot = true;
        promptText.gameObject.SetActive(true);
        promptText.text = $"Press J / K / L to equip {weapon}";

        // Show assigning state
        slotJImage.sprite = assigningSpriteJ;
        slotKImage.sprite = assigningSpriteK;
        slotLImage.sprite = assigningSpriteL;

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.canMove = false;
            PlayerMovement.Instance.isInputGloballyLocked = true;
        }
    }

    public void CloseEquipPrompt()
    {
        isChoosingSlot = false;
        promptText.gameObject.SetActive(false);

        // Revert back to correct visuals (default if empty, weapon if assigned)
        RefreshAllSlots();

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.canMove = true;
            PlayerMovement.Instance.isInputGloballyLocked = false;
        }
    }

    private void EquipToSlot(ModularWeaponSlotKey slotKey)
    {
        // Remove weapon from other slots if already equipped
        for (int i = 0; i < PlayerData.equippedWeapons.Length; i++)
        {
            if (PlayerData.equippedWeapons[i] == currentWeapon)
            {
                PlayerData.equippedWeapons[i] = WeaponType.None;
                UpdateSlotVisual((ModularWeaponSlotKey)i, WeaponType.None);
            }
        }

        // Assign new weapon
        PlayerData.equippedWeapons[(int)slotKey] = currentWeapon;
        ModularWeaponSlotManager.Instance.AssignWeaponToSlot(currentWeapon, slotKey);

        UpdateSlotVisual(slotKey, currentWeapon);
        CloseEquipPrompt();
    }

    public void UpdateSlotVisual(ModularWeaponSlotKey slotKey, WeaponType weapon)
    {
        Image slotImage = slotKey switch
        {
            ModularWeaponSlotKey.Slot1 => slotJImage,
            ModularWeaponSlotKey.Slot2 => slotKImage,
            ModularWeaponSlotKey.Slot3 => slotLImage,
            _ => null,
        };

        if (slotImage == null) return;

        if (weapon == WeaponType.None)
        {
            // Empty slot → show default sprite
            slotImage.sprite = slotKey switch
            {
                ModularWeaponSlotKey.Slot1 => defaultSpriteJ,
                ModularWeaponSlotKey.Slot2 => defaultSpriteK,
                ModularWeaponSlotKey.Slot3 => defaultSpriteL,
                _ => null,
            };
        }
        else
        {
            // Equipped → show weapon sprite
            slotImage.sprite = weapon switch
            {
                WeaponType.Dagger => daggerSprite,
                WeaponType.Gauntlet => gauntletSprite,
                WeaponType.Slingshot => slingshotSprite,
                _ => null,
            };
        }
    }

    public void RefreshAllSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            UpdateSlotVisual((ModularWeaponSlotKey)i, PlayerData.equippedWeapons[i]);
        }
    }
}
