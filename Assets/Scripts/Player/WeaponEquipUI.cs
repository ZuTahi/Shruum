using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponEquipUI : MonoBehaviour
{
    public static WeaponEquipUI Instance;

    [Header("Slot Images")]
    public Image slotJImage;
    public Image slotKImage;
    public Image slotLImage;

    [Header("Weapon Sprites")]
    public Sprite daggerSprite;
    public Sprite gauntletSprite;
    public Sprite slingshotSprite;

    [Header("Default Key Sprites")]
    public Sprite KeySpriteJ;
    public Sprite KeySpriteK;
    public Sprite KeySpriteL;

    [Header("Equip Prompt")]
    public TextMeshProUGUI promptText;

    private WeaponType currentWeapon;
    private bool isChoosingSlot = false;

    public bool IsChoosingSlot => isChoosingSlot;

    private Dictionary<ModularWeaponSlotKey, WeaponType> equippedWeapons = new Dictionary<ModularWeaponSlotKey, WeaponType>
    {
        { ModularWeaponSlotKey.Slot1, WeaponType.None },
        { ModularWeaponSlotKey.Slot2, WeaponType.None },
        { ModularWeaponSlotKey.Slot3, WeaponType.None },
    };

    private void Awake()
    {
        Instance = this;
        promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isChoosingSlot) return;

        if (Input.GetKeyDown(KeyCode.J))
        {
            EquipToSlot(ModularWeaponSlotKey.Slot1);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            EquipToSlot(ModularWeaponSlotKey.Slot2);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            EquipToSlot(ModularWeaponSlotKey.Slot3);
        }
    }

    public void OpenEquipPrompt(WeaponType weapon)
    {
        currentWeapon = weapon;
        isChoosingSlot = true;
        promptText.gameObject.SetActive(true);
        promptText.text = $"Press J / K / L to equip {weapon}";

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.canMove = false;
            PlayerMovement.Instance.isInputGloballyLocked = true; // 🔒 lock global input
        }
    }

    public void CloseEquipPrompt()
    {
        isChoosingSlot = false;
        promptText.gameObject.SetActive(false);

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.canMove = true;
            PlayerMovement.Instance.isInputGloballyLocked = false; // ✅ unlock input globally
        }

        ModularWeaponSlotManager.Instance?.SuppressInputForAllWeapons(false);

        StartCoroutine(TemporarilyBlockInputAfterEquip());
    }

    private IEnumerator TemporarilyBlockInputAfterEquip()
    {
        ModularWeaponInputHandler inputHandler = FindFirstObjectByType<ModularWeaponInputHandler>();
        if (inputHandler != null)
            inputHandler.SuppressInputTemporarily(0.15f);  // ~150ms delay

        yield return null;
    }

    private void EquipToSlot(ModularWeaponSlotKey slotKey)
    {
        foreach (var kvp in equippedWeapons)
        {
            if (kvp.Value == currentWeapon)
            {
                equippedWeapons[kvp.Key] = WeaponType.None;
                UpdateSlotVisual(kvp.Key, WeaponType.None);
                break;
            }
        }

        equippedWeapons[slotKey] = currentWeapon;
        ModularWeaponSlotManager.Instance.AssignWeaponToSlot(currentWeapon, slotKey);
        UpdateSlotVisual(slotKey, currentWeapon);

        CloseEquipPrompt();
    }

    private void UpdateSlotVisual(ModularWeaponSlotKey slotKey, WeaponType weapon)
    {
        Image slotImage = slotKey switch
        {
            ModularWeaponSlotKey.Slot1 => slotJImage,
            ModularWeaponSlotKey.Slot2 => slotKImage,
            ModularWeaponSlotKey.Slot3 => slotLImage,
            _ => null,
        };

        if (slotImage == null) return;

        slotImage.sprite = weapon switch
        {
            WeaponType.Dagger => daggerSprite,
            WeaponType.Gauntlet => gauntletSprite,
            WeaponType.Slingshot => slingshotSprite,
            _ => GetDefaultKeySprite(slotKey),
        };
    }

    private Sprite GetDefaultKeySprite(ModularWeaponSlotKey slotKey)
    {
        return slotKey switch
        {
            ModularWeaponSlotKey.Slot1 => KeySpriteJ,
            ModularWeaponSlotKey.Slot2 => KeySpriteK,
            ModularWeaponSlotKey.Slot3 => KeySpriteL,
            _ => null,
        };
    }
}
