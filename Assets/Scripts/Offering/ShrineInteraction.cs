using UnityEngine;

public class ShrineInteraction : MonoBehaviour
{
    [SerializeField] private OfferingUIManager uiManager;
    private bool playerInRange = false;

    private InteractionPromptUI promptUI;
    private void Start()
    {
        promptUI = FindFirstObjectByType<InteractionPromptUI>();
        if (promptUI == null)
            Debug.LogError("❌ InteractionPromptUI not found in the scene!");
    }

    private void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.Space) && !uiManager.IsPanelActive())
        {
            uiManager.ShowOfferingPanel(GetComponentInParent<OfferingShrine>());
            if (promptUI != null)
                promptUI.HidePrompt(); // Hide when UI is open
        }
        PlayerMovement.Instance.canDash = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (promptUI != null && !uiManager.IsPanelActive())
                promptUI.ShowPrompt("[SPACE]");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (promptUI != null)
                promptUI.HidePrompt();

            uiManager.HideOfferingPanel();
            PlayerMovement.Instance.canDash = true; // Auto-close UI when walking away
        }
    }
}
