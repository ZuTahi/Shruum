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

    [Header("Player Reference")]
    [SerializeField] private PlayerMovement playerMovement;

    private int selectedIndex = 0;
    private OfferingShrine currentShrine;
    private float inputBufferTime = 0.2f;
    private float inputBufferTimer = 0f;

    private void Update()
    {
        if (!offeringPanel.activeSelf) return;
        if (inputBufferTimer > 0f)
        {
            inputBufferTimer -= Time.deltaTime;
            return;
        }

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
        else if (Input.GetKeyDown(KeyCode.F))
        {
            TryUpgradeSelectedStat();
        }
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

        int cost = currentShrine.GetUpgradeCost((StatType)selectedIndex);

        if (PlayerInventory.Instance.natureForce >= cost)
        {
            currentShrine.UpgradePlayer((StatType)selectedIndex);
            UpdateUI();
        }
        else
        {
            Debug.Log($"Not enough Nature Force. Needed: {cost}");
        }
    }

    public void ShowOfferingPanel(OfferingShrine shrine)
    {
        currentShrine = shrine;
        selectedIndex = statIcons.Length / 2;
        offeringPanel.SetActive(true);
        playerMovement.canMove = false;
        inputBufferTimer = inputBufferTime;
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
                    costTexts[i].text = $"{cost}";
                }
                else
                {
                    costTexts[i].text = "?";
                }
            }
        }
    }
}
