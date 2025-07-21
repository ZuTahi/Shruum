// --- CheatManager.cs ---
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    [Header("Cheat Settings")]
    public int addNatureForceAmount = 1000;

    void Update()
    {
        // Add Nature Force
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.AddNatureForce(addNatureForceAmount);
                Debug.Log($"[Cheat] Added {addNatureForceAmount} Nature Force.");
            }
        }

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
