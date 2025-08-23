// PlayerInventory.cs
using UnityEngine;
using System;
using System.Collections.Generic;

public enum PermanentItemType
{
    Flower,   // HP
    Leaf,     // SP
    Water,    // MP
    Fruit,    // Attack
    Root,     // Defense
    WeaponKey // Unlock weapons
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    // How many of each item player owns (runtime mirror of PlayerData)
    private Dictionary<PermanentItemType, int> permanentItems = new Dictionary<PermanentItemType, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Init dictionary with 0 counts
        foreach (PermanentItemType type in Enum.GetValues(typeof(PermanentItemType)))
            permanentItems[type] = 0;
    }

    private void Start()
    {
        // Optional: sync from PlayerData on scene start
        foreach (PermanentItemType t in Enum.GetValues(typeof(PermanentItemType)))
            permanentItems[t] = PlayerData.GetPermanentItemCount(t);
    }

    // --- Add / Remove / Query permanent items ---
    public void AddPermanentItem(PermanentItemType type, int amount = 1)
    {
        if (!permanentItems.ContainsKey(type))
            permanentItems[type] = 0;

        permanentItems[type] += amount;

        // also reflect into PlayerData (which saves)
        PlayerData.AddPermanentItem(type, amount);

        Debug.Log($"[Inventory] Added {amount}x {type}. Total = {permanentItems[type]}");
    }

    public bool ConsumePermanentItem(PermanentItemType type, int amount = 1)
    {
        if (GetPermanentItemCount(type) >= amount)
        {
            permanentItems[type] -= amount;

            // also reflect into PlayerData (which saves)
            var ok = PlayerData.ConsumePermanentItem(type, amount);
            Debug.Log($"[Inventory] Consumed {amount}x {type}. Remaining = {permanentItems[type]}");
            return ok;
        }
        Debug.LogWarning($"[Inventory] Tried to consume {amount}x {type}, but not enough in inventory!");
        return false;
    }

    public int GetPermanentItemCount(PermanentItemType type)
    {
        return permanentItems.ContainsKey(type) ? permanentItems[type] : 0;
    }

    public bool HasPermanentItem(PermanentItemType type, int amount = 1)
    {
        return GetPermanentItemCount(type) >= amount;
    }

    public void LoadData(PlayerSaveData data)
    {
        foreach (PermanentItemType type in Enum.GetValues(typeof(PermanentItemType)))
            permanentItems[type] = data.GetItemCount(type);

        Debug.Log("[Inventory] Data loaded from PlayerSaveData.");
    }

    // Handy debug
    public void DebugInventory()
    {
        Debug.Log("=== Permanent Inventory ===");
        foreach (var kvp in permanentItems)
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        Debug.Log("===========================");
    }
}
