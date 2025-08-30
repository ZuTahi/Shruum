// PlayerData.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PlayerData
{
    // Core Stats
    public static int maxHP = 50;
    public static int currentHP = 50;

    public static int maxSP = 50;
    public static int currentSP = 50;

    public static int maxMP = 50;
    public static int currentMP = 50;

    public static float attackDamage = 10f;
    public static float baseDefensePercent = 0f;
    public static float defenseMultiplier = 1f;

    public static float hpMultiplier = 1f;
    public static float spMultiplier = 1f;
    public static float mpMultiplier = 1f;
    public static float attackMultiplier = 1f;

    public static float staminaRegenRate = 15f;

    // Currency & Progression
    public static int natureForce = 0;
    public static List<string> loreNotes = new List<string>();

    // Equipment
    public static WeaponType[] equippedWeapons = new WeaponType[3];
    public static HashSet<WeaponType> unlockedWeapons = new HashSet<WeaponType>();

    // Upgrade Tracking
    public static int hpUpgradeCount = 0;
    public static int spUpgradeCount = 0;
    public static int mpUpgradeCount = 0;
    public static int atkUpgradeCount = 0;
    public static int defUpgradeCount = 0;

    public static int maxUpgradeCount = 10;
    public static int baseUpgradeCost = 100;
    public static int costIncreasePerUpgrade = 50;

    // Permanent items (kept across runs)
    public static Dictionary<PermanentItemType, int> permanentItems = new Dictionary<PermanentItemType, int>();

    static PlayerData()
    {
        RespawnResetWeapons();
        InitPermanentItems();
    }

    public static void InitPermanentItems()
    {
        foreach (PermanentItemType type in Enum.GetValues(typeof(PermanentItemType)))
            if (!permanentItems.ContainsKey(type)) permanentItems[type] = 0;
    }

    // -------- Permanent item helpers (used by PlayerInventory) --------
    public static void AddPermanentItem(PermanentItemType type, int amount = 1)
    {
        if (!permanentItems.ContainsKey(type)) permanentItems[type] = 0;
        permanentItems[type] += amount;
        Debug.Log($"[PlayerData] Added {amount}x {type}. Total = {permanentItems[type]}");
        SaveSystem.SavePlayer();
    }

    public static bool ConsumePermanentItem(PermanentItemType type, int amount = 1)
    {
        if (permanentItems.ContainsKey(type) && permanentItems[type] >= amount)
        {
            permanentItems[type] -= amount;
            Debug.Log($"[PlayerData] Consumed {amount}x {type}. Remaining = {permanentItems[type]}");
            SaveSystem.SavePlayer();
            return true;
        }
        Debug.LogWarning($"[PlayerData] Tried to consume {amount}x {type}, but not enough!");
        return false;
    }

    public static int GetPermanentItemCount(PermanentItemType type)
    {
        return permanentItems.ContainsKey(type) ? permanentItems[type] : 0;
    }

    // -------- Save/Load --------
    public static void LoadFromSaveData(PlayerSaveData data)
    {
        natureForce = data.natureForce;

        maxHP = data.maxHP; maxSP = data.maxSP; maxMP = data.maxMP;
        currentHP = data.currentHP; currentSP = data.currentSP; currentMP = data.currentMP;

        attackMultiplier = data.attackMultiplier;
        baseDefensePercent = data.baseDefensePercent;
        defenseMultiplier = data.defenseMultiplier;
        staminaRegenRate = data.staminaRegenRate;

        loreNotes = data.loreNotes.ToList();
        unlockedWeapons = new HashSet<WeaponType>(data.unlockedWeapons);

        hpUpgradeCount = data.hpUpgradeCount;
        spUpgradeCount = data.spUpgradeCount;
        mpUpgradeCount = data.mpUpgradeCount;
        atkUpgradeCount = data.atkUpgradeCount;
        defUpgradeCount = data.defUpgradeCount;

        // rebuild permanent item dict from array
        permanentItems.Clear();
        InitPermanentItems();
        foreach (PermanentItemType t in Enum.GetValues(typeof(PermanentItemType)))
            permanentItems[t] = data.GetItemCount(t);

        Debug.Log("[PlayerData] Loaded from save.");
    }

    // -------- Weapons --------
    public static void UnlockWeapon(WeaponType weapon)
    {
        if (unlockedWeapons.Add(weapon))
        {
            Debug.Log($"[PlayerData] Unlocked weapon: {weapon}");
            SaveSystem.SavePlayer();
        }
    }

    public static bool IsWeaponUnlocked(WeaponType weapon) => unlockedWeapons.Contains(weapon);

    // -------- Reset --------
    public static void ResetToDefault()
    {
        maxHP = 50; maxSP = 50; maxMP = 50;
        currentHP = maxHP; currentSP = maxSP; currentMP = maxMP;

        attackMultiplier = 1f;
        baseDefensePercent = 0f;
        defenseMultiplier = 1f;

        natureForce = 0;
        loreNotes.Clear();

        hpUpgradeCount = 0; spUpgradeCount = 0; mpUpgradeCount = 0; atkUpgradeCount = 0; defUpgradeCount = 0;
        ResetWeaponsToDefault();
        RespawnResetWeapons();
        permanentItems.Clear();
        InitPermanentItems();

        Debug.Log("[PlayerData] Reset to default values.");
        SaveSystem.SavePlayer();
    }

    // Used on brand new save file
    public static void ResetWeaponsToDefault()
    {
        equippedWeapons[0] = WeaponType.Dagger;
        equippedWeapons[1] = WeaponType.None;
        equippedWeapons[2] = WeaponType.None;

        // 🟢 only unlock dagger by default
        unlockedWeapons.Clear();
        unlockedWeapons.Add(WeaponType.Dagger);
    }

    // Used when returning to hub after death
    public static void RespawnResetWeapons()
    {
        if (equippedWeapons[0] == WeaponType.None)
            equippedWeapons[0] = WeaponType.Dagger;

        // 🟢 ensure dagger remains unlocked, do not touch other weapons
        if (!unlockedWeapons.Contains(WeaponType.Dagger))
            unlockedWeapons.Add(WeaponType.Dagger);
    }

    // -------- Upgrade helpers (unchanged) --------
    public static int GetUpgradeCount(StatType type) => type switch
    {
        StatType.HP => hpUpgradeCount,
        StatType.SP => spUpgradeCount,
        StatType.MP => mpUpgradeCount,
        StatType.ATK => atkUpgradeCount,
        StatType.DEF => defUpgradeCount,
        _ => 0
    };

    public static void IncrementUpgradeCount(StatType type)
    {
        switch (type)
        {
            case StatType.HP: hpUpgradeCount++; break;
            case StatType.SP: spUpgradeCount++; break;
            case StatType.MP: mpUpgradeCount++; break;
            case StatType.ATK: atkUpgradeCount++; break;
            case StatType.DEF: defUpgradeCount++; break;
        }
        SaveSystem.SavePlayer();
    }

    public static int GetUpgradeCost(StatType type)
    {
        int count = GetUpgradeCount(type);
        return baseUpgradeCost + (count * costIncreasePerUpgrade);
    }

    public static bool HasReachedMaxUpgrades(StatType type) => GetUpgradeCount(type) >= maxUpgradeCount;
}
