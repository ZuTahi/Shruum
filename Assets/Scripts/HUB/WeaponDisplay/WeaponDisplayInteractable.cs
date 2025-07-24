using UnityEngine;

public class WeaponDisplayInteractable : MonoBehaviour
{
    public WeaponType weaponType;
    public GameObject lockVisual;

    [Header("Unlock Settings")]
    public bool isUnlocked = false;
    public int unlockCost = 50;

    private bool playerInRange = false;
    private InteractionPromptUI promptUI;

    private void Start()
    {
        isUnlocked = PlayerData.IsWeaponUnlocked(weaponType);
        UpdateLockVisual();
        promptUI = FindFirstObjectByType<InteractionPromptUI>();
        if (promptUI == null)
            Debug.LogError("❌ InteractionPromptUI not found in the scene!");
    }

    private void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isUnlocked)
            {
                TryUnlockWeapon();
            }
            else
            {
                WeaponEquipUI.Instance.OpenEquipPrompt(weaponType);
                LockPlayerInput();
            }
        }
    }

    private void TryUnlockWeapon()
    {
        if (PlayerInventory.Instance.natureForce >= unlockCost)
        {
            PlayerInventory.Instance.AddNatureForce(-unlockCost);
            PlayerData.UnlockWeapon(weaponType);
            isUnlocked = true;
            UpdateLockVisual();

            GameManager.Instance.SaveGame();
        }
    }

    private void UpdateLockVisual()
    {
        if (lockVisual != null)
            lockVisual.SetActive(!isUnlocked);
    }

    private void LockPlayerInput()
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.canMove = false;
            PlayerMovement.Instance.isInputGloballyLocked = true; // 🔒 Lock global input while assigning
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            playerInRange = true;
            if (promptUI != null)
            {
                string promptText = isUnlocked ? "Press [F] to Interact" : $"Press [F] to Unlock ({unlockCost})";
                promptUI.ShowPrompt(promptText);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null)
                promptUI.HidePrompt();
        }   
    }
    public void RefreshLockStatus()
    {
        isUnlocked = PlayerData.IsWeaponUnlocked(weaponType);
        UpdateLockVisual();
    }

}
