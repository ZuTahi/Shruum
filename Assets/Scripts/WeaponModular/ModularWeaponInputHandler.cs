using UnityEngine;
using System.Collections;

public class ModularWeaponInputHandler : MonoBehaviour
{
    [SerializeField] private ModularWeaponSlotManager slotManager;
    [SerializeField] private ModularComboBuffer comboBuffer;
    private ModularWeaponSlotKey? lastSlotUsed = null;
    private bool inputSuppressedTemporarily = false;

    private void Update()
    {
        if (PlayerMovement.Instance.isInputGloballyLocked)
            return;

        if (inputSuppressedTemporarily) return;

        if (WeaponEquipUI.Instance != null && WeaponEquipUI.Instance.IsChoosingSlot)
            return;

        if (Input.GetKeyDown(KeyCode.J))
            HandleInput(KeyCode.J, ModularWeaponInput.J);
        else if (Input.GetKeyDown(KeyCode.K))
            HandleInput(KeyCode.K, ModularWeaponInput.K);
        else if (Input.GetKeyDown(KeyCode.L))
            HandleInput(KeyCode.L, ModularWeaponInput.L);
    }

    private void HandleInput(KeyCode key, ModularWeaponInput input)
    {
        Debug.Log($"[InputHandler] Key Pressed: {key}, Input Enum: {input}");

        comboBuffer.RegisterInput(input);

        ModularWeaponCombo weapon = slotManager.GetWeaponByKeyCode(key);
        if (weapon == null || !weapon.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[InputHandler] Weapon not found or inactive.");
            return;
        }

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
            {
                lastWeapon.ResetCombo();
                Debug.Log($"[InputHandler] Switched slot from {lastSlotUsed} to {currentSlot}, resetting previous weapon combo.");
            }
        }

        lastSlotUsed = currentSlot;

        // Check mix finisher
        var combo = comboBuffer.GetCombo();
        if (combo.Length == 3 && combo[2] == input)
        {
            if (!comboBuffer.HasValidCombo())
            {
                Debug.Log("[InputHandler] Combo expired due to timeout. Resetting weapons.");
                comboBuffer.ClearBuffer();

                foreach (var w in slotManager.GetAllWeapons())
                    w?.ResetCombo();

                // Proceed with normal input
                weapon.HandleInput();
                return;
            }

            Debug.Log("[InputHandler] 3-input combo detected: checking for mix finisher...");
            weapon.suppressNormalFinisher = false;
            weapon.HandleMixFinisher(combo);

            if (weapon.suppressNormalFinisher)
            {
                Debug.Log("[InputHandler] Mix finisher accepted. Suppressing normal input.");
                comboBuffer.ClearBuffer();
                foreach (var w in slotManager.GetAllWeapons()) w?.ResetCombo();
                return;
            }
            else
            {
                Debug.Log("[InputHandler] Not a valid mix finisher. Proceeding with normal input.");
                weapon.HandleInput();
            }
        }
        else
        {
            Debug.Log("[InputHandler] Triggering weapon.HandleInput()");
            weapon.HandleInput();
        }
    }

    public void SuppressInputTemporarily(float duration)
    {
        StartCoroutine(SuppressInputRoutine(duration));
    }

    private IEnumerator SuppressInputRoutine(float duration)
    {
        inputSuppressedTemporarily = true;
        yield return new WaitForSeconds(duration);
        inputSuppressedTemporarily = false;
    }
}
