using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatBlockUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statNameText;
    public TextMeshProUGUI statValueText;
    public TextMeshProUGUI upgradeCountText;
    public Image icon;

    [Header("Colors")]
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(0.5f, 0.5f, 0.5f); // Gray

    public void SetData(string name, int value, int count, int maxCount, Sprite iconSprite, bool isSelected)
    {
        statNameText.text = name;
        statValueText.text = value.ToString();
        upgradeCountText.text = $"{count}/{maxCount}";
        icon.sprite = iconSprite;

        // Set visibility and color
        statNameText.color = isSelected ? selectedColor : unselectedColor;
        statValueText.color = isSelected ? selectedColor : unselectedColor;
        upgradeCountText.color = isSelected ? selectedColor : unselectedColor;
        icon.color = isSelected ? selectedColor : unselectedColor;

        statValueText.gameObject.SetActive(isSelected);
    }
}
