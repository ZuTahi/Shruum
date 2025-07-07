using UnityEngine;

public class WeaponDisplayInteractable : MonoBehaviour
{
    public WeaponType weaponType;

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            WeaponEquipUI.Instance.OpenEquipPrompt(weaponType);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
