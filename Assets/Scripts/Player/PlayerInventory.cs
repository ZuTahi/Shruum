using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public int natureForce = 0;
    public List<string> loreNotes = new List<string>();

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
        natureForce = PlayerData.natureForce;
    }

    public void AddNatureForce(int amount)
    {
        natureForce += amount;
        PlayerData.natureForce = natureForce;
        // Optional: Clamp to non-negative values
        if (natureForce < 0)
            natureForce = 0;

        NatureForceUI.Instance?.UpdateNatureUI(natureForce);
    }

    public void AddRandomLoreNote()
    {
        if (PlayerData.loreNotes.Count == 0)
        {
            Debug.Log("No lore notes available to collect.");
            return;
        }

        // Pick a random lore note not already owned
        var availableNotes = new List<string>(PlayerData.loreNotes);
        availableNotes.RemoveAll(note => loreNotes.Contains(note));

        if (availableNotes.Count == 0)
        {
            Debug.Log("All lore notes already collected.");
            return;
        }

        int randomIndex = Random.Range(0, availableNotes.Count);
        string randomNote = availableNotes[randomIndex];
        loreNotes.Add(randomNote);
        Debug.Log("Collected Random Lore Note: " + randomNote);

        // Optionally update UI here
    }

    public void LoadData(PlayerSaveData data)
    {
        natureForce = data.natureForce;
        PlayerData.natureForce = natureForce;

        loreNotes = new List<string>(data.loreNotes);
        NatureForceUI.Instance?.UpdateNatureUI(natureForce);
    }
}
