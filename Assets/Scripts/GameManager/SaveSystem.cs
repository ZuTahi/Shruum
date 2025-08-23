using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class SaveSystem
{
    private const string FileName = "save.json";
    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    // --------- API ---------

    public static void SavePlayer()
    {
        var data = BuildSaveDataFromPlayerData();
        // Make sure the serialized list reflects the current dictionary before writing
        data.BuildSerializableListsFromDictionaries();

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveSystem] Saved to: {SavePath}");
    }

    public static void LoadPlayer()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveSystem] No save found. Initializing defaults.");
            PlayerData.ResetToDefault();

            // Ensure inventory starts at zeros so your UI/debugs don't NRE
            var empty = new PlayerSaveData();
            empty.RebuildDictionariesFromSerializableLists();
            PlayerInventory.Instance?.LoadData(empty);
            return;
        }

        string json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<PlayerSaveData>(json) ?? new PlayerSaveData();

        // Rebuild runtime dictionaries (JsonUtility can’t handle Dictionary)
        data.RebuildDictionariesFromSerializableLists();

        // Push into PlayerData
        PlayerData.LoadFromSaveData(data);

        // Keep PlayerInventory’s internal dictionary in sync (for keys/flowers/etc.)
        PlayerInventory.Instance?.LoadData(data);

        // If PlayerStats is alive, refresh runtime stats/UI
        PlayerStats.Instance?.LoadFromData();

        Debug.Log($"[SaveSystem] Loaded from: {SavePath}");
    }

    public static void ClearSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[SaveSystem] Save deleted.");
        }
    }

    public static bool SaveExists() => File.Exists(SavePath);

    // --------- Helpers ---------

    private static PlayerSaveData BuildSaveDataFromPlayerData()
    {
        var data = new PlayerSaveData
        {
            // Core stats
            natureForce = PlayerData.natureForce,
            maxHP = PlayerData.maxHP,
            maxSP = PlayerData.maxSP,
            maxMP = PlayerData.maxMP,
            currentHP = PlayerData.currentHP,
            currentSP = PlayerData.currentSP,
            currentMP = PlayerData.currentMP,

            attackMultiplier = PlayerData.attackMultiplier,
            baseDefensePercent = PlayerData.baseDefensePercent,
            defenseMultiplier = PlayerData.defenseMultiplier,
            staminaRegenRate = PlayerData.staminaRegenRate,

            // Progress
            loreNotes = new List<string>(PlayerData.loreNotes),

            // Weapons / unlocks
            unlockedWeapons = new List<WeaponType>(PlayerData.unlockedWeapons),

            // Upgrade counts
            hpUpgradeCount = PlayerData.hpUpgradeCount,
            spUpgradeCount = PlayerData.spUpgradeCount,
            mpUpgradeCount = PlayerData.mpUpgradeCount,
            atkUpgradeCount = PlayerData.atkUpgradeCount,
            defUpgradeCount = PlayerData.defUpgradeCount,

            // Permanent items (runtime dictionary; we’ll serialize it as a list)
            permanentItems = new Dictionary<PermanentItemType, int>(PlayerData.permanentItems)
        };

        return data;
    }
}

// =====================
// Serializable Save DTO
// =====================
[Serializable]
public class PlayerSaveData
{
    // Core Stats
    public int maxHP = 50;
    public int currentHP = 50;

    public int maxSP = 50;
    public int currentSP = 50;

    public int maxMP = 50;
    public int currentMP = 50;

    public float attackMultiplier = 1f;
    public float baseDefensePercent = 0f;
    public float defenseMultiplier = 1f;
    public float staminaRegenRate = 15f;

    // Currency & Progression
    public int natureForce = 0;
    public List<string> loreNotes = new List<string>();

    // Equipment / Unlocks
    public List<WeaponType> unlockedWeapons = new List<WeaponType>();

    // Upgrade tracking
    public int hpUpgradeCount = 0;
    public int spUpgradeCount = 0;
    public int mpUpgradeCount = 0;
    public int atkUpgradeCount = 0;
    public int defUpgradeCount = 0;

    // -------- Permanent Items --------
    // Runtime (not serialized directly by JsonUtility)
    [NonSerialized]
    public Dictionary<PermanentItemType, int> permanentItems = new Dictionary<PermanentItemType, int>();

    // Serialized form (because JsonUtility can’t handle dictionaries)
    [Serializable]
    public struct PermanentItemEntry
    {
        public PermanentItemType type;
        public int count;
    }

    public List<PermanentItemEntry> permanentItemsSerialized = new List<PermanentItemEntry>();

    // Convert serialized lists -> dictionaries (call after JsonUtility.FromJson)
    public void RebuildDictionariesFromSerializableLists()
    {
        permanentItems ??= new Dictionary<PermanentItemType, int>();
        permanentItems.Clear();

        if (permanentItemsSerialized != null)
        {
            foreach (var entry in permanentItemsSerialized)
            {
                permanentItems[entry.type] = entry.count;
            }
        }

        // Ensure all enum keys exist (so GetItemCount never NREs)
        foreach (PermanentItemType t in Enum.GetValues(typeof(PermanentItemType)))
        {
            if (!permanentItems.ContainsKey(t))
                permanentItems[t] = 0;
        }
    }

    // Convert runtime dictionaries -> serialized lists (call before JsonUtility.ToJson)
    public void BuildSerializableListsFromDictionaries()
    {
        permanentItemsSerialized ??= new List<PermanentItemEntry>();
        permanentItemsSerialized.Clear();

        if (permanentItems != null)
        {
            foreach (var kvp in permanentItems)
            {
                permanentItemsSerialized.Add(new PermanentItemEntry
                {
                    type = kvp.Key,
                    count = kvp.Value
                });
            }
        }
    }

    // Helper for your current PlayerInventory.LoadData()
    public int GetItemCount(PermanentItemType type)
    {
        if (permanentItems == null || permanentItems.Count == 0)
        {
            // If someone calls before dictionaries are rebuilt, try to rebuild
            RebuildDictionariesFromSerializableLists();
        }

        return permanentItems != null && permanentItems.TryGetValue(type, out var c) ? c : 0;
    }
}
