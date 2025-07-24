using UnityEngine;
using System.Collections.Generic;

public class OfferingShrine : MonoBehaviour
{
    public int baseNatureForceCost = 100;
    public float costMultiplier = 1.5f;

    public int hpUpgradeAmount = 10;
    public int spUpgradeAmount = 5;
    public int mpUpgradeAmount = 5;
    public float attackUpgradeMultiplier = 0.1f;
    public float defenseUpgradePercent = 0.05f;

    private Dictionary<StatType, int> upgradeCounts = new();


    public void UpgradePlayer(StatType type)
    {
        int cost = GetUpgradeCost(type);

        if (PlayerInventory.Instance.natureForce < cost)
        {
            Debug.Log("Not enough Nature Force to upgrade.");
            return;
        }

        PlayerInventory.Instance.AddNatureForce(-cost);

        switch (type)
        {
            case StatType.HP:
                PlayerData.maxHP += hpUpgradeAmount;
                PlayerStats.Instance.IncreaseMaxHP(hpUpgradeAmount);
                break;
            case StatType.SP:
                PlayerData.maxSP += spUpgradeAmount;
                PlayerStats.Instance.IncreaseMaxSP(spUpgradeAmount);
                break;
            case StatType.MP:
                PlayerData.maxMP += mpUpgradeAmount;
                PlayerStats.Instance.IncreaseMaxMP(mpUpgradeAmount);
                break;
            case StatType.ATK:
                PlayerData.attackMultiplier += attackUpgradeMultiplier;
                PlayerStats.Instance.attackMultiplier += attackUpgradeMultiplier;
                break;
            case StatType.DEF:
                PlayerData.baseDefensePercent += defenseUpgradePercent;
                PlayerStats.Instance.baseDefensePercent += defenseUpgradePercent;
                break;
            default:
                Debug.LogWarning("Unknown StatType for upgrade.");
                break;
        }

        if (!upgradeCounts.ContainsKey(type))
            upgradeCounts[type] = 0;
        upgradeCounts[type]++;

        Debug.Log($"Upgraded {type} at cost {cost}. Total upgrades for {type}: {upgradeCounts[type]}");

        var playerStats = FindFirstObjectByType<PlayerStats>();
        playerStats?.LoadFromData();

        PlayerUIManager.Instance?.RefreshAllStats();
    }

    public int GetUpgradeCost(StatType type)
    {
        if (!upgradeCounts.ContainsKey(type))
            upgradeCounts[type] = 0;

        return Mathf.RoundToInt(baseNatureForceCost * Mathf.Pow(costMultiplier, upgradeCounts[type]));
    }
}
