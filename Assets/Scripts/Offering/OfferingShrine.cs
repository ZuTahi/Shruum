using UnityEngine;

public class OfferingShrine : MonoBehaviour
{
    public int hpUpgradeAmount = 10;
    public int spUpgradeAmount = 5;
    public int mpUpgradeAmount = 5;
    public float attackUpgradeMultiplier = 0.1f;
    public float defenseUpgradePercent = 0.05f;
   
    public void UpgradePlayer(StatType type)
    {
        if (PlayerData.HasReachedMaxUpgrades(type))
        {
            Debug.Log($"{type} has reached the upgrade cap.");
            return;
        }

        int cost = PlayerData.GetUpgradeCost(type);
        if (PlayerInventory.Instance.natureForce < cost)
        {
            Debug.Log("Not enough Nature Force to upgrade.");
            return;
        }

        PlayerInventory.Instance.AddNatureForce(-cost);
        PlayerData.IncrementUpgradeCount(type);

        switch (type)
        {
            case StatType.HP:
                PlayerData.maxHP += hpUpgradeAmount;
                PlayerStats.Instance.LoadFromData();
                break;
            case StatType.SP:
                PlayerData.maxSP += spUpgradeAmount;
                PlayerStats.Instance.LoadFromData();
                break;
            case StatType.MP:
                PlayerData.maxMP += mpUpgradeAmount;
                PlayerStats.Instance.LoadFromData();
                break;
            case StatType.ATK:
                PlayerData.attackMultiplier += attackUpgradeMultiplier;
                PlayerStats.Instance.LoadFromData();
                break;
            case StatType.DEF:
                PlayerData.baseDefensePercent += defenseUpgradePercent;
                PlayerStats.Instance.LoadFromData();
                break;
        }

        Debug.Log($"[Shrine] Upgraded {type}, new count: {PlayerData.GetUpgradeCount(type)}");

        PlayerStats.Instance?.LoadFromData();
        PlayerUIManager.Instance?.RefreshAllStats();
    }
}
