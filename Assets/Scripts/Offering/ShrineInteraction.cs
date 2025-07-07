using UnityEngine;

public class ShrineInteraction : MonoBehaviour
{
    [SerializeField] private OfferingUIManager uiManager;
    private bool playerInRange = false;

    private void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.F) && !uiManager.IsPanelActive())
        {
            uiManager.ShowOfferingPanel(GetComponentInParent<OfferingShrine>());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            uiManager.HideOfferingPanel();
        }
    }
}
