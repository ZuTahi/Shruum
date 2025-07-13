using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class StatBar
{
    public RectTransform background;  // Resizes with max stat (black base)
    public RectTransform fill;        // Shrinks/fills based on current stat
    public float baseStat = 100f;     // Base max stat
    public float baseHeight = 120f;   // Base height for baseStat (in pixels)
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
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    public void UpdateHP(float current, float max) => UpdateStatBar(hpBar, current, max);
    public void UpdateSP(float current, float max) => UpdateStatBar(spBar, current, max);
    public void UpdateMP(float current, float max) => UpdateStatBar(mpBar, current, max);

    private void UpdateStatBar(StatBar bar, float current, float max)
    {
        // ⬆️ How much the bar should grow for this max stat
        float scale = max / bar.baseStat;
        float fillHeight = bar.baseHeight * scale;
        float backgroundHeight = fillHeight + 20f;     // Matches fill

        // 🔲 Resize black background (scales with max stat)
        if (bar.background != null)
            bar.background.sizeDelta = new Vector2(bar.background.sizeDelta.x, backgroundHeight);

        // 🟩 Resize fill to match usable area
        if (bar.fill != null)
            bar.fill.sizeDelta = new Vector2(bar.fill.sizeDelta.x, fillHeight);

        // 🌊 Update fill amount based on current stat
        Image fillImage = bar.fill.GetComponent<Image>();
        if (fillImage != null)
            fillImage.fillAmount = current / max;
    }
}
