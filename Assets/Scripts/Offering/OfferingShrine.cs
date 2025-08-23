using UnityEngine;

public class OfferingShrine : MonoBehaviour
{
    [Header("Upgrade amounts per level")]
    public int hpUpgradeAmount = 20;
    public int spUpgradeAmount = 10;
    public int mpUpgradeAmount = 10;
    public float attackUpgradeMultiplier = 0.1f;   // +10% per level
    public float defenseUpgradePercent  = 0.05f;   // +5% (as a fraction) per level

    public void UpgradePlayer(StatType type)
    {
        if (PlayerData.HasReachedMaxUpgrades(type))
        {
            Debug.Log($"{type} has reached the upgrade cap.");
            return;
        }

        PermanentItemType item = MapToPermanentItem(type);
        if (!PlayerInventory.Instance.HasPermanentItem(item, 1))
        {
            Debug.Log($"Not enough {item} to upgrade {type}.");
            return;
        }

        // consume item
        PlayerInventory.Instance.ConsumePermanentItem(item, 1);

        // increment level tracking
        PlayerData.IncrementUpgradeCount(type);

        // apply the permanent effect to PlayerData + live stats
        var ps = PlayerStats.Instance;
        switch (type)
        {
            case StatType.HP:
                PlayerData.maxHP += hpUpgradeAmount;
                if (ps != null)
                {
                    float ratio = ps.maxHP > 0 ? (float)ps.currentHP / ps.maxHP : 1f;
                    ps.maxHP = PlayerData.maxHP;
                    ps.currentHP = Mathf.CeilToInt(ps.maxHP * Mathf.Clamp01(ratio));
                }
                break;

            case StatType.SP:
                PlayerData.maxSP += spUpgradeAmount;
                if (ps != null)
                {
                    float ratio = ps.maxSP > 0 ? (float)ps.currentSP / ps.maxSP : 1f;
                    ps.maxSP = PlayerData.maxSP;
                    ps.currentSP = Mathf.CeilToInt(ps.maxSP * Mathf.Clamp01(ratio));
                }
                break;

            case StatType.MP:
                PlayerData.maxMP += mpUpgradeAmount;
                if (ps != null)
                {
                    float ratio = ps.maxMP > 0 ? (float)ps.currentMP / ps.maxMP : 1f;
                    ps.maxMP = PlayerData.maxMP;
                    ps.currentMP = Mathf.CeilToInt(ps.maxMP * Mathf.Clamp01(ratio));
                }
                break;

            case StatType.ATK:
                PlayerData.attackMultiplier += attackUpgradeMultiplier;
                if (ps != null) ps.attackMultiplier = PlayerData.attackMultiplier;
                break;

            case StatType.DEF:
                PlayerData.baseDefensePercent += defenseUpgradePercent;
                if (ps != null) ps.baseDefensePercent = PlayerData.baseDefensePercent;
                break;
        }

        // refresh live UI
        if (ps != null) ps.RefreshUI();
        PlayerUIManager.Instance?.RefreshAllStats();
        Debug.Log($"[Shrine] Upgraded {type}. New level: {PlayerData.GetUpgradeCount(type)}");
    }

    private PermanentItemType MapToPermanentItem(StatType type)
    {
        switch (type)
        {
            case StatType.HP:  return PermanentItemType.Flower;
            case StatType.SP:  return PermanentItemType.Leaf;
            case StatType.MP:  return PermanentItemType.Water;
            case StatType.ATK: return PermanentItemType.Fruit;
            case StatType.DEF: return PermanentItemType.Root;
            default:           return PermanentItemType.Flower;
        }
    }
}
