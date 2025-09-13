using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private GameObject backgroundImage; // 👈 add this

    private void Start()
    {
        HidePrompt();
    }

    public void ShowPrompt(string promptMessage)
    {
        promptUI.SetActive(true);
        backgroundImage.SetActive(true);  // show background
        promptText.text = promptMessage;
    }

    public void HidePrompt()
    {
        promptUI.SetActive(false);
        backgroundImage.SetActive(false); // hide background
        promptText.text = "";
    }
}
