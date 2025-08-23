using UnityEngine;

public class CheatManager : MonoBehaviour
{
    void Update()
    {
        // Debug Inventory
        if (Input.GetKeyDown(KeyCode.M))
        {
            PlayerInventory.Instance?.DebugInventory();
        }

        // Add Permanent Items
        if (Input.GetKeyDown(KeyCode.Alpha1))
            PlayerInventory.Instance?.AddPermanentItem(PermanentItemType.Flower, 1);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            PlayerInventory.Instance?.AddPermanentItem(PermanentItemType.Leaf, 1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            PlayerInventory.Instance?.AddPermanentItem(PermanentItemType.Water, 1);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            PlayerInventory.Instance?.AddPermanentItem(PermanentItemType.Fruit, 1);

        if (Input.GetKeyDown(KeyCode.Alpha5))
            PlayerInventory.Instance?.AddPermanentItem(PermanentItemType.Root, 1);

        if (Input.GetKeyDown(KeyCode.K))
            PlayerInventory.Instance?.AddPermanentItem(PermanentItemType.WeaponKey, 1);

        // Clear Save Data
        if (Input.GetKeyDown(KeyCode.C))
        {
            SaveSystem.ClearSave();
            PlayerData.ResetToDefault();

            var playerStats = FindFirstObjectByType<PlayerStats>();
            playerStats?.LoadFromData();

            Debug.Log("[Cheat] Save cleared and PlayerData reset to default.");
        }

        // Save Game
        if (Input.GetKeyDown(KeyCode.V))
        {
            SaveSystem.SavePlayer();
            Debug.Log("[Cheat] Game saved.");
        }

        // Load Game
        if (Input.GetKeyDown(KeyCode.B))
        {
            SaveSystem.LoadPlayer();
            Debug.Log("[Cheat] Game loaded.");
        }
    }
}
