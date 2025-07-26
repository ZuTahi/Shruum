using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferingUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject offeringPanel;
    [SerializeField] private StatBlockUI[] statBlocks;
    [SerializeField] private Transform slidingBorder;               // moves between selections
    [SerializeField] private Transform[] statBlockTargets;         // assign from Editor
    [SerializeField] private Sprite[] statIcons;                   // assign sprites in order: HP → SP → MP → ATK → DEF
    [SerializeField] private TextMeshProUGUI upgradePromptText;
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
            selectedIndex = (selectedIndex - 1 + statBlocks.Length) % statBlocks.Length;
            UpdateUI();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex = (selectedIndex + 1) % statBlocks.Length;
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

        // Smooth sliding effect for border
        if (slidingBorder && selectedIndex < statBlockTargets.Length)
        {
            Vector3 targetPos = statBlockTargets[selectedIndex].position;
            float distance = Vector3.Distance(slidingBorder.position, targetPos);

            if (distance > 0.1f)
            {
                slidingBorder.position = Vector3.Lerp
                (
                    slidingBorder.position,
                    targetPos,
                    Time.deltaTime * 50f
                );
            }
            else
            {
                slidingBorder.position = targetPos; // snap when close enough
            }
        }
    }

    private void TryUpgradeSelectedStat()
    {
        if (currentShrine == null) return;

        StatType type = (StatType)selectedIndex;
        int cost = PlayerData.GetUpgradeCost(type);

        if (PlayerInventory.Instance.natureForce >= cost)
        {
            currentShrine.UpgradePlayer(type);
            UpdateUI(); // refresh cost + visuals
        }
        else
        {
            Debug.Log($"Not enough Nature Force. Needed: {cost}");
        }
    }

    public void ShowOfferingPanel(OfferingShrine shrine)
    {
        currentShrine = shrine;
        selectedIndex = statBlocks.Length / 2; // start in the middle
        offeringPanel.SetActive(true);
        playerMovement.canMove = false;
        inputBufferTimer = inputBufferTime;

        UpdateUI();

        // Snap border immediately to middle selection
        if (slidingBorder && selectedIndex < statBlockTargets.Length)
        {
            slidingBorder.position = statBlockTargets[selectedIndex].position;
        }
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
        for (int i = 0; i < statBlocks.Length; i++)
        {
            StatType type = (StatType)i;
            string label = type.ToString();
            int upgradeCount = PlayerData.GetUpgradeCount(type);
            int maxCount = PlayerData.maxUpgradeCount;
            int upgradeCost = PlayerData.GetUpgradeCost(type);
            Sprite icon = (i < statIcons.Length) ? statIcons[i] : null;
            bool selected = i == selectedIndex;

            // ✅ Show cost instead of stat value
            statBlocks[i].SetData(label, upgradeCost, upgradeCount, maxCount, icon, selected);
        }

        // Show upgrade prompt only if panel active
        if (upgradePromptText != null)
            upgradePromptText.gameObject.SetActive(true);
    }
}
