using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferingUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject offeringPanel;
    [SerializeField] private Image[] statIcons; // HP → DEF
    [SerializeField] private TextMeshProUGUI[] costTexts;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [SerializeField] private int[] baseCosts = new int[5]; // HP → DEF
    [SerializeField] private float costMultiplier = 1.5f;
    private int[] offeringCounts = new int[5]; // One per StatType (HP → DEF)

    [Header("Player Reference")]
    [SerializeField] private PlayerMovement playerMovement;

    private int selectedIndex = 0;
    private OfferingShrine currentShrine;

    private void Update()
    {
        if (!offeringPanel.activeSelf) return;

        // Navigate stat selection
        if (Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex = (selectedIndex - 1 + statIcons.Length) % statIcons.Length;
            UpdateUI();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex = (selectedIndex + 1) % statIcons.Length;
            UpdateUI();
        }
        // Upgrade stat
        else if (Input.GetKeyDown(KeyCode.F))
        {
            TryUpgradeSelectedStat();
        }
        // Exit panel
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideOfferingPanel();
        }
    }
    private void TryUpgradeSelectedStat()
    {
        if (currentShrine == null)
        {
            Debug.LogWarning("No active shrine selected.");
            return;
        }

        StatType type = (StatType)selectedIndex;
        int cost = currentShrine.GetUpgradeCost(type);

        if (PlayerInventory.Instance.natureForce >= cost)
        {
            currentShrine.MakeOffering(type);
            UpdateUI();
        }
        else
        {
            Debug.Log($"Not enough Nature Force for {type}. Needed: {cost}");
        }
    }

    public int GetUpgradeCost(StatType type)
    {
        int index = (int)type;

        if (index < 0 || index >= baseCosts.Length)
        {
            Debug.LogError("Invalid stat index for base cost.");
            return 999;
        }

        return Mathf.RoundToInt(baseCosts[index] * Mathf.Pow(costMultiplier, offeringCounts[index]));
    }

    public void ShowOfferingPanel(OfferingShrine shrine)
    {
        currentShrine = shrine;
        selectedIndex = statIcons.Length / 2; // Center stat (e.g., MP if 5 stats)
        offeringPanel.SetActive(true);
        playerMovement.canMove = false;
        UpdateUI();
    }

    public void HideOfferingPanel()
    {
        offeringPanel.SetActive(false);
        currentShrine = null;
        playerMovement.canMove = true;
    }

    public bool IsPanelActive() => offeringPanel.activeSelf;

    private void UpdateUI()
    {
        if (statIcons == null || costTexts == null)
        {
            Debug.LogError("Stat icons or cost texts not assigned in OfferingUIManager.");
            return;
        }

        if (statIcons.Length == 0 || costTexts.Length == 0)
        {
            Debug.LogError("Stat icon or cost text arrays are empty.");
            return;
        }

        for (int i = 0; i < statIcons.Length; i++)
        {
            if (statIcons[i] != null)
                statIcons[i].color = (i == selectedIndex) ? selectedColor : defaultColor;

            if (i < costTexts.Length && costTexts[i] != null)
            {
                if (currentShrine != null)
                {
                    int cost = currentShrine.GetUpgradeCost((StatType)i);
                    costTexts[i].text = $"Cost: {cost}";
                }
                else
                {
                    costTexts[i].text = "Cost: ?";
                }
            }
        }
    }

}
