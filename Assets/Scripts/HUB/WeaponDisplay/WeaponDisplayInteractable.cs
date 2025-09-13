using UnityEngine;

public class WeaponDisplayInteractable : MonoBehaviour
{
    public WeaponType weaponType;
    public GameObject lockVisual;

    [Header("Unlock Settings")]
    public bool isUnlocked = false;

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            promptUI?.HidePrompt();

            if (!isUnlocked)
            {
                TryUnlockWeapon();
            }
            else
            {
                WeaponEquipUI.Instance.OpenEquipPrompt(weaponType);
                PlayerMovement.Instance.canMove = false; // freeze movement if you want
            }
        }

        // 🚫 Disable dash whenever in range
        PlayerMovement.Instance.canDash = false;
    }

    private void TryUnlockWeapon()
    {
        if (PlayerInventory.Instance.HasPermanentItem(PermanentItemType.WeaponKey, 1))
        {
            PlayerInventory.Instance.ConsumePermanentItem(PermanentItemType.WeaponKey, 1);
            PlayerData.UnlockWeapon(weaponType);
            isUnlocked = true;
            UpdateLockVisual();

            GameManager.Instance.SaveGame();

            Debug.Log($"[WeaponDisplay] {weaponType} unlocked using a WeaponKey!");
        }
        else
        {
            Debug.Log("[WeaponDisplay] Not enough WeaponKeys to unlock this weapon.");
        }
    }

    private void UpdateLockVisual()
    {
        if (lockVisual != null)
            lockVisual.SetActive(!isUnlocked);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptUI != null)
            {
                string promptText = isUnlocked ? "[SPACE]" : "[SPACE]";
                promptUI.ShowPrompt(promptText);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            promptUI?.HidePrompt();
            PlayerMovement.Instance.canDash = true; // ✅ restore dash
        }
    }

    public void RefreshLockStatus()
    {
        isUnlocked = PlayerData.IsWeaponUnlocked(weaponType);
        UpdateLockVisual();
    }
}
