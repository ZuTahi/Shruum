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
        if (PlayerMovement.Instance.isInputGloballyLocked || inputSuppressedTemporarily)
            return;

        if (WeaponEquipUI.Instance != null && WeaponEquipUI.Instance.IsChoosingSlot)
            return;

        if (Input.GetKeyDown(KeyCode.J)) HandleInput(KeyCode.J, ModularWeaponInput.J);
        else if (Input.GetKeyDown(KeyCode.K)) HandleInput(KeyCode.K, ModularWeaponInput.K);
        else if (Input.GetKeyDown(KeyCode.L)) HandleInput(KeyCode.L, ModularWeaponInput.L);
    }

    private void HandleInput(KeyCode key, ModularWeaponInput input)
    {
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

        if (lastSlotUsed != null && lastSlotUsed != currentSlot)
        {
            var lastWeapon = slotManager.GetWeaponInSlot(lastSlotUsed.Value);
            lastWeapon?.ResetCombo();
            Debug.Log($"[InputHandler] Switched slot from {lastSlotUsed} to {currentSlot}, resetting previous weapon combo.");
        }

        lastSlotUsed = currentSlot;

        comboBuffer.RegisterInput(input);
        var combo = comboBuffer.GetCombo();

        bool isFullCombo = combo.Length == 3 && combo[2] == input;
        bool isMixCombo = isFullCombo && comboBuffer.HasValidCombo();

        if (isMixCombo)
        {
            if (weapon.suppressMixFinisher)
            {
                Debug.Log("[InputHandler] Mix finisher suppressed.");
                comboBuffer.ClearBuffer();
                ResetAllWeapons();
                return;
            }

            weapon.HandleMixFinisher(combo);
            comboBuffer.ClearBuffer(); // ⬅️ make sure this happens regardless
            ResetAllWeapons();

            if (weapon.suppressNormalFinisher)
            {
                Debug.Log("[InputHandler] Mix finisher triggered. Skipping normal input.");
                return; // 🛑 Stop right here!
            }

            Debug.Log("[InputHandler] Mix combo fallback → running normal input.");
            weapon.HandleInput(); // only runs if mix finisher didn't go through
            return;
        }

        if (weapon.suppressNormalFinisher)
        {
            Debug.Log("[InputHandler] Normal finisher suppressed. Ignoring input.");
            return;
        }

        weapon.HandleInput();
    }

    private void ResetAllWeapons()
    {
        foreach (var w in slotManager.GetAllWeapons())
            w?.ResetCombo();
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
