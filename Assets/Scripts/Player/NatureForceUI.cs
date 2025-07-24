using TMPro;
using UnityEngine;

public class NatureForceUI : MonoBehaviour
{
    public static NatureForceUI Instance;

    [SerializeField] private TextMeshProUGUI natureText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateNatureUI(PlayerInventory.Instance.natureForce);
    }

    public void UpdateNatureUI(int newAmount)
    {
        natureText.text = $" {newAmount}";
        Debug.Log($"Nature UI updated: {newAmount}");
    }

}
