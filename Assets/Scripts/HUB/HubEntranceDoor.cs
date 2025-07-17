using UnityEngine;
using UnityEngine.SceneManagement;

public class HubEntranceDoor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("HubEntranceDoor: Moving to Room0");
            GameManager.Instance.StartNewRun();
        }
    }
}
