using UnityEngine;

public class HubManager : MonoBehaviour
{
    private void Start()
    {
        RefreshWeaponDisplays();
    }

    private void RefreshWeaponDisplays()
    {
        var displays = FindObjectsByType<WeaponDisplayInteractable>(FindObjectsSortMode.None);

        foreach (var display in displays)
        {
            display.RefreshLockStatus();
        }

        Debug.Log($"[HubManager] Refreshed {displays.Length} weapon displays.");
    }
}
