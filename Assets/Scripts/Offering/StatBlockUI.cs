using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatBlockUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI upgradeCountText;
    [SerializeField] private Image icon;

    [Header("Colors")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.5f, 0.5f, 0.5f); // Gray

    /// <summary>
    /// Updates how the stat block looks.
    /// </summary>
    public void SetData(string name, int count, int maxCount, Sprite iconSprite, bool isSelected)
    {
        // Base slot visuals
        statNameText.text = name;
        icon.sprite = iconSprite;

        // Determine display text for upgrade count
        string displayText;
        if (count <= 0)
            displayText = "upgrade";
        else if (count >= maxCount)
            displayText = "Max";
        else
            displayText = $"LvLl. {count}";

        // Apply text + visibility
        upgradeCountText.text = displayText;
        upgradeCountText.gameObject.SetActive(isSelected);

        // Tint colors depending on selection
        statNameText.color = isSelected ? selectedColor : unselectedColor;
        icon.color = isSelected ? selectedColor : unselectedColor;

        if (isSelected)
            upgradeCountText.color = selectedColor;
    }
}
