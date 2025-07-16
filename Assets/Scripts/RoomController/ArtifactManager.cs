using UnityEngine;

public class ArtifactManager : MonoBehaviour
{
    [Header("Artifact UI")]
    public GameObject artifactUIPanel;

    private RoomManager currentRoom;

    public void ShowArtifactChoices(RoomManager roomManager)
    {
        currentRoom = roomManager;

        if (artifactUIPanel != null)
        {
            artifactUIPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Artifact UI Panel is not assigned.");
        }
    }

    public void OnArtifactSelected()
    {
        Debug.Log("ArtifactManager: Player selected an artifact.");

        if (artifactUIPanel != null)
        {
            artifactUIPanel.SetActive(false);
        }

        if (currentRoom != null)
        {
            currentRoom.OnArtifactChosen();
        }
    }
}
