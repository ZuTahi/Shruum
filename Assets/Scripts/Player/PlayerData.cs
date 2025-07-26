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

    public static int maxUpgradeCount = 10;              // Cap per stat
    public static int baseUpgradeCost = 100;             // Cost of first upgrade
    public static int costIncreasePerUpgrade = 50;       // Linear increase per upgrade

    static PlayerData()
    {
        ResetWeapons();
    }

    public static void LoadFromSaveData(PlayerSaveData data)
    {
        natureForce = data.natureForce;
        maxHP = data.maxHP;
        maxSP = data.maxSP;
        maxMP = data.maxMP;
        attackMultiplier = data.attackMultiplier;
        baseDefensePercent = data.baseDefensePercent;
        defenseMultiplier = data.defenseMultiplier;
        loreNotes = data.loreNotes.ToList();

        unlockedWeapons = new HashSet<WeaponType>(data.unlockedWeapons);

        hpUpgradeCount = data.hpUpgradeCount;
        spUpgradeCount = data.spUpgradeCount;
        mpUpgradeCount = data.mpUpgradeCount;
        atkUpgradeCount = data.atkUpgradeCount;
        defUpgradeCount = data.defUpgradeCount;
    }

    public static void UnlockWeapon(WeaponType weapon)
    {
        if (!unlockedWeapons.Contains(weapon))
        {
            unlockedWeapons.Add(weapon);
            Debug.Log($"[PlayerData] Unlocked weapon: {weapon}");
        }
    }

    public static bool IsWeaponUnlocked(WeaponType weapon)
    {
        return unlockedWeapons.Contains(weapon);
    }

    public static void ResetToDefault()
    {
        maxHP = 50;
        maxSP = 50;
        maxMP = 50;
        currentHP = maxHP;
        currentSP = maxSP;
        currentMP = maxMP;

        attackMultiplier = 1f;
        baseDefensePercent = 0f;
        defenseMultiplier = 1f;

        natureForce = 0;
        loreNotes.Clear();

        hpUpgradeCount = 0;
        spUpgradeCount = 0;
        mpUpgradeCount = 0;
        atkUpgradeCount = 0;
        defUpgradeCount = 0;

        ResetWeapons();

        Debug.Log("[PlayerData] Reset to default values.");
    }

    private static void ResetWeapons()
    {
        for (int i = 0; i < equippedWeapons.Length; i++)
            equippedWeapons[i] = WeaponType.None;

        unlockedWeapons = new HashSet<WeaponType>();
    }

    // Upgrade Tracking Utilities
    public static int GetUpgradeCount(StatType type)
    {
        return type switch
        {
            StatType.HP => hpUpgradeCount,
            StatType.SP => spUpgradeCount,
            StatType.MP => mpUpgradeCount,
            StatType.ATK => atkUpgradeCount,
            StatType.DEF => defUpgradeCount,
            _ => 0
        };
    }

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
    }

    public static int GetUpgradeCost(StatType type)
    {
        int count = GetUpgradeCount(type);
        return baseUpgradeCost + (count * costIncreasePerUpgrade);
    }

    public static bool HasReachedMaxUpgrades(StatType type)
    {
        return GetUpgradeCount(type) >= maxUpgradeCount;
    }
}
