using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    private void Start()
    {
        HidePrompt();
    }

    public void ShowPrompt(string promptMessage)
    {
        promptUI.SetActive(true);
        promptText.text = promptMessage;
    }

    public void HidePrompt()
    {
        promptUI.SetActive(false);
        promptText.text = "";
    }
}
