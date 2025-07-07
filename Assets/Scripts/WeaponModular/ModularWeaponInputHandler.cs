using UnityEngine;

public class ModularWeaponInputHandler : MonoBehaviour
{
    [SerializeField] private ModularWeaponSlotManager slotManager;
    [SerializeField] private ModularComboBuffer comboBuffer;
    private ModularWeaponSlotKey? lastSlotUsed = null;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            HandleInput(KeyCode.J, ModularWeaponInput.J);

        else if (Input.GetKeyDown(KeyCode.K))
            HandleInput(KeyCode.K, ModularWeaponInput.K);

        else if (Input.GetKeyDown(KeyCode.L))
            HandleInput(KeyCode.L, ModularWeaponInput.L);
    }

    private void HandleInput(KeyCode key, ModularWeaponInput input)
    {
        Debug.Log($"Key Pressed: {key}, Input: {input}");

        comboBuffer.RegisterInput(input);

        ModularWeaponCombo weapon = slotManager.GetWeaponByKeyCode(key);
        if (weapon == null || !weapon.gameObject.activeInHierarchy)
            return;

        ModularWeaponSlotKey currentSlot = key switch
        {
            KeyCode.J => ModularWeaponSlotKey.Slot1,
            KeyCode.K => ModularWeaponSlotKey.Slot2,
            KeyCode.L => ModularWeaponSlotKey.Slot3,
            _ => throw new System.Exception("Unknown key")
        };

        // Reset combo if switching weapons
        if (lastSlotUsed != null && lastSlotUsed != currentSlot)
        {
            ModularWeaponCombo lastWeapon = slotManager.GetWeaponInSlot(lastSlotUsed.Value);
            if (lastWeapon != null)
                lastWeapon.ResetCombo();
        }

        lastSlotUsed = currentSlot;

        // Try mix finisher ONLY if this is the 3rd input in combo
        var combo = comboBuffer.GetCombo();
        if (combo.Length == 3 && combo[2] == input)
        {
            // Let the weapon check the pattern and decide whether to execute a mix finisher
            weapon.suppressNormalFinisher = false; // default to false before checking
            weapon.HandleMixFinisher(combo);

            // If the weapon accepted the combo, suppress normal and clear input
            if (weapon.suppressNormalFinisher)
            {
                comboBuffer.ClearBuffer(); // ✅ Only clear if mix executed
            }
            else
            {
                // Not a valid mix finisher → treat as normal input
                weapon.HandleInput();
            }
        }
        else
        {
            weapon.HandleInput();
        }
    }
}
