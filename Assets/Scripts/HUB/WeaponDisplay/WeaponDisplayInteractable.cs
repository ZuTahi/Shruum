using UnityEngine;

public class WeaponDisplayInteractable : MonoBehaviour
{
    public WeaponType weaponType;
    public GameObject lockVisual; // Assign the padlock child object here

    [Header("Unlock Settings")]
    public bool isUnlocked = false;
    public int unlockCost = 50;

    private bool playerInRange = false;

    private void Start()
    {
        // Sync with PlayerData to persist unlock status
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
        }
    }

    private void UpdateLockVisual()
    {
        if (lockVisual == null)
        {
            return;
        }

        lockVisual.SetActive(!isUnlocked);
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
}
