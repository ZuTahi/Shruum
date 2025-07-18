using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ArtifactManager : MonoBehaviour
{
    [Header("Artifact Config")]
    public List<ArtifactSO> artifactPool;
    public Image[] artifactSlotImages;
    public GameObject artifactUIPanel;

    private ArtifactSO[] currentArtifacts = new ArtifactSO[2];
    private int currentSelectionIndex = -1;
    private bool selectionActive = false;
    private RoomManager roomManagerRef;

    public float selectScale = 1.2f;
    public float normalScale = 1f;

    public void ShowArtifactChoices(RoomManager roomManager)
    {
        roomManagerRef = roomManager;
        artifactUIPanel.SetActive(true);
        PlayerMovement.Instance.canMove = false;

        // Select 2 random artifacts
        for (int i = 0; i < 2; i++)
        {
            ArtifactSO randomArtifact = artifactPool[Random.Range(0, artifactPool.Count)];
            currentArtifacts[i] = randomArtifact;
            artifactSlotImages[i].sprite = randomArtifact.artifactIcon;
            artifactSlotImages[i].transform.localScale = Vector3.one * normalScale;
        }

        StartCoroutine(EnableSelectionWithDelay(0.5f)); // give player time before selection is active
    }

    private IEnumerator EnableSelectionWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        selectionActive = true;
        currentSelectionIndex = 0;
        UpdateUISelection();
    }

    private void Update()
    {
        if (!selectionActive) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            currentSelectionIndex = Mathf.Max(0, currentSelectionIndex - 1);
            UpdateUISelection();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            currentSelectionIndex = Mathf.Min(artifactSlotImages.Length - 1, currentSelectionIndex + 1);
            UpdateUISelection();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            ConfirmSelection();
        }
    }

    private void UpdateUISelection()
    {
        for (int i = 0; i < artifactSlotImages.Length; i++)
        {
            artifactSlotImages[i].transform.localScale = (i == currentSelectionIndex)
                ? Vector3.one * selectScale
                : Vector3.one * normalScale;
        }
    }

    private void ConfirmSelection()
    {
        ArtifactSO selectedArtifact = currentArtifacts[currentSelectionIndex];
        Debug.Log("Artifact Selected: " + selectedArtifact.artifactName);
        selectedArtifact.ApplyEffect();

        selectionActive = false;
        PlayerMovement.Instance.canMove = true;
        artifactUIPanel.SetActive(false);

        roomManagerRef.OnArtifactChosen();
    }
}