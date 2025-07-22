using UnityEngine;

public class WeaponDisplayInteractable : MonoBehaviour
{
    public WeaponType weaponType;
    public GameObject lockVisual;

    [Header("Unlock Settings")]
    public bool isUnlocked = false;
    public int unlockCost = 50;

    private bool playerInRange = false;

    private void Start()
    {
        isUnlocked = PlayerData.IsWeaponUnlocked(weaponType);
        UpdateLockVisual();
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
            PlayerInventory.Instance.natureForce -= unlockCost;
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
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
    public void RefreshLockStatus()
    {
        isUnlocked = PlayerData.IsWeaponUnlocked(weaponType);
        UpdateLockVisual();
    }

}
