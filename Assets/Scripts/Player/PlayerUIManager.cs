using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class StatBar
{
    public RectTransform background;   // Resizes with max stat (black base)
    public RectTransform fill;         // Shrinks/fills based on current stat
    public float baseStat = 100f;      // Base max stat
    public float baseHeight = 120f;    // Base height for baseStat (in pixels)

    [Header("Optional Number Display")]
    public TextMeshProUGUI valueText;  // Shows "current / max"
}

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance { get; private set; }

    [Header("Stat Bars")]
    public StatBar hpBar;
    public StatBar spBar;
    public StatBar mpBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void UpdateHP(float current, float max) => UpdateStatBar(hpBar, current, max);
    public void UpdateSP(float current, float max) => UpdateStatBar(spBar, current, max);
    public void UpdateMP(float current, float max) => UpdateStatBar(mpBar, current, max);

    /// <summary>
    /// Refresh all stat bars to reflect current PlayerStats values.
    /// </summary>
    public void RefreshAllStats()
    {
        var stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            UpdateHP(stats.currentHP, stats.maxHP);
            UpdateSP(stats.currentSP, stats.maxSP);
            UpdateMP(stats.currentMP, stats.maxMP);
            Debug.Log("[PlayerUIManager] Stats UI refreshed.");
        }
        else
        {
            Debug.LogWarning("[PlayerUIManager] No PlayerStats found to refresh UI.");
        }
    }

    private void UpdateStatBar(StatBar bar, float current, float max)
    {
        if (bar == null) return;

        float scale = max / bar.baseStat;
        float fillHeight = bar.baseHeight * scale;
        float backgroundHeight = fillHeight + 20f;

        if (bar.background != null)
            bar.background.sizeDelta = new Vector2(bar.background.sizeDelta.x, backgroundHeight);

        if (bar.fill != null)
            bar.fill.sizeDelta = new Vector2(bar.fill.sizeDelta.x, fillHeight);

        Image fillImage = bar.fill?.GetComponent<Image>();
        if (fillImage != null)
            fillImage.fillAmount = max > 0 ? current / max : 0;

        // 🔹 Only show current value
        if (bar.valueText != null)
            bar.valueText.text = Mathf.RoundToInt(current).ToString();
    }

}
