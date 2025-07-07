using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class StatBar
{
    public RectTransform background;  // The full stat background
    public RectTransform fill;        // The fill that grows/shrinks
    public float baseStat;            // Starting stat (e.g., 100 HP)
    public float baseHeight;          // Height in pixels that represents baseStat
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

    // ✅ Called when current HP changes (e.g. damage or heal)
    public void UpdateHP(float current, float max)
    {
        UpdateStatBar(hpBar, current, max);
    }

    public void UpdateSP(float current, float max)
    {
        UpdateStatBar(spBar, current, max);
    }

    public void UpdateMP(float current, float max)
    {
        UpdateStatBar(mpBar, current, max);
    }

    private void UpdateStatBar(StatBar bar, float current, float max)
    {
        // Background height (includes buffer/padding to frame the fill)
        float maxScale = max / bar.baseStat;
        float fillAreaHeight = bar.baseHeight * maxScale;
        float bgHeight = fillAreaHeight + 20f; // Add top/bottom padding for visuals

        bar.background.sizeDelta = new Vector2(bar.background.sizeDelta.x, bgHeight);

        // Match fill height to actual usable area (no padding)
        bar.fill.sizeDelta = new Vector2(bar.fill.sizeDelta.x, fillAreaHeight);

        // Set fillAmount to reflect current stat
        Image fillImage = bar.fill.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.fillAmount = current / max;
        }
    }
}
