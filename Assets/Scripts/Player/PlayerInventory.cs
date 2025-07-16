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
    }

    public void AddNatureForce(int amount)
    {
        natureForce += amount;
        Debug.Log("Nature Force: " + natureForce);
        // Update UI if needed
    }

    public void AddLoreNote(string note)
    {
        if (!loreNotes.Contains(note))
        {
            loreNotes.Add(note);
            Debug.Log("Collected Lore Note: " + note);
            // Update UI if needed
        }
    }

    public void LoadData(PlayerSaveData data)
    {
        natureForce = data.natureForce;
        loreNotes = new List<string>(data.loreNotes);
    }
}
