using UnityEngine;
using UnityEngine.SceneManagement;

public class EntranceDoor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (ForestManager.Instance == null)
        {
            Debug.LogError("EntranceDoor: ForestManager does not exist! Cannot proceed.");
            return;
        }

        string nextRoom = ForestManager.Instance.GetNextRoomScene();
        if (string.IsNullOrEmpty(nextRoom))
        {
            Debug.LogError("EntranceDoor: Next room is invalid or sequence is finished.");
            return;
        }

        ForestManager.Instance.AdvanceRoomIndex();
        Debug.Log("EntranceDoor: Loading next room: " + nextRoom);
        SceneManager.LoadSceneAsync(nextRoom);
    }
}
