using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class WeaponEquipUI : MonoBehaviour
{
    public static WeaponEquipUI Instance;

    [Header("Slot Circles")]
    public Image slotJ;
    public Image slotK;
    public Image slotL;

    [Header("Equip Prompt")]
    public TextMeshProUGUI promptText;

    private WeaponType currentWeapon;
    private bool isChoosingSlot = false;

    private void Awake()
    {
        Instance = this;
        promptText.gameObject.SetActive(false);
    }

    public void OpenEquipPrompt(WeaponType weapon)
    {
        currentWeapon = weapon;
        isChoosingSlot = true;
        promptText.gameObject.SetActive(true);
        promptText.text = $"Press J / K / L to equip {weapon}";

        // Optional: make slot circles glow or pulse
        HighlightSlots(true);
    }

    public void CloseEquipPrompt()
    {
        isChoosingSlot = false;
        promptText.gameObject.SetActive(false);
        HighlightSlots(false);
    }

    void Update()
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

    private void EquipToSlot(ModularWeaponSlotKey slotKey)
    {
        ModularWeaponSlotManager.Instance.AssignWeaponToSlot(currentWeapon, slotKey);
        CloseEquipPrompt();
    }

    private void HighlightSlots(bool enable)
    {
        Color color = enable ? Color.yellow : Color.white;

        slotJ.color = color;
        slotK.color = color;
        slotL.color = color;

        // You can also add animations or scale up/down here
    }
}
