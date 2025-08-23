using UnityEngine;

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

            // ✅ Start new run (this should trigger SceneManager.LoadScene inside GameManager)
            GameManager.Instance.StartNewRun();
        }
    }
}
