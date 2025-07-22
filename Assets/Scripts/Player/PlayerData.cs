using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PlayerData
{
    public static int maxHP = 100;
    public static int currentHP = 100;

    public static int maxSP = 100;
    public static int currentSP = 100;

    public static int maxMP = 100;
    public static int currentMP = 100;

    public static float attackDamage = 10f;
    public static float baseDefensePercent = 0f;
    public static float defenseMultiplier = 1f;

    public static float hpMultiplier = 1f;
    public static float spMultiplier = 1f;
    public static float mpMultiplier = 1f;
    public static float attackMultiplier = 1f;

    public static float staminaRegenRate = 15f;

    public static int natureForce = 0;
    public static List<string> loreNotes = new List<string>();

    public static WeaponType[] equippedWeapons = new WeaponType[3];
    public static HashSet<WeaponType> unlockedWeapons = new HashSet<WeaponType>();

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
        maxHP = 100;
        maxSP = 100;
        maxMP = 100;
        attackMultiplier = 1f;
        baseDefensePercent = 0f;
        defenseMultiplier = 1f;
        natureForce = 0;
        loreNotes.Clear();

        ResetWeapons();

        Debug.Log("[PlayerData] Reset to default values.");
    }

    private static void ResetWeapons()
    {
        for (int i = 0; i < equippedWeapons.Length; i++)
            equippedWeapons[i] = WeaponType.None;

        unlockedWeapons = new HashSet<WeaponType>();
    }
}
