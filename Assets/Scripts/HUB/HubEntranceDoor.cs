using UnityEngine;
using UnityEngine.SceneManagement;

public class HubEntranceDoor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("HubEntranceDoor: Moving to Room0");

            // ✅ Save the player's current HP/SP/MP before leaving hub
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.SaveToPlayerData();
            }

            // ✅ Prepare new run data
            GameManager.Instance.StartNewRun();

            // ✅ Handle transition here (same approach as MainMenu)
            string nextRoom = "Room0"; // first room always Room0
            SceneTransition sceneTransition = Object.FindFirstObjectByType<SceneTransition>();

            if (sceneTransition != null)
            {
                sceneTransition.FadeToScene(nextRoom);
            }
            else
            {
                SceneManager.LoadScene(nextRoom); // fallback
            }
        }
    }
}
