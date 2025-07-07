using UnityEngine;

public class OfferingShrine : MonoBehaviour
{
    [SerializeField] private int baseCost = 100;
    [SerializeField] private float costMultiplier = 1.5f;

    // Tracks how many times each stat has been upgraded
    [SerializeField] private int[] offeringCounts = new int[5]; // Index by StatType

    public void MakeOffering(StatType type)
    {
        int index = (int)type;
        int cost = Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, offeringCounts[index]));

        // Check requirements
        if (PlayerInventory.Instance.natureForce < cost)
        {
            Debug.LogWarning($"Not enough Nature Force to offer {type}. Needed: {cost}");
            return;
        }

        // Deduct resources
        PlayerInventory.Instance.natureForce -= cost;
        offeringCounts[index]++;

        // === OPTION A: Apply flat upgrade ===
        int upgradeAmount = 10;

        switch (type)
        {
            case StatType.HP:
                PlayerStats.Instance.maxHP += upgradeAmount;
                PlayerStats.Instance.currentHP = PlayerStats.Instance.maxHP;
                break;

            case StatType.SP:
                PlayerStats.Instance.maxSP += upgradeAmount;
                PlayerStats.Instance.currentSP = PlayerStats.Instance.maxSP;
                break;

            case StatType.MP:
                PlayerStats.Instance.maxMP += upgradeAmount;
                PlayerStats.Instance.currentMP = PlayerStats.Instance.maxMP;
                break;

            case StatType.ATK:
                PlayerStats.Instance.attackDamage += upgradeAmount;
                break;

            case StatType.DEF:
                PlayerStats.Instance.baseDefensePercent += 0.05f; // Add 5% per upgrade
                break;
        }

        // Refresh UI
        PlayerStats.Instance.RecalculateStats();

        Debug.Log($"Offered {type}. Cost: {cost}. Total offerings: {offeringCounts[index]}");
    }

    public int GetOfferingCount(StatType type) => offeringCounts[(int)type];

    public int GetUpgradeCost(StatType type)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, offeringCounts[(int)type]));
    }
}
