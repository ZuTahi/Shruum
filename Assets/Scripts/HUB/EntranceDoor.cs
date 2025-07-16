using UnityEngine;
using UnityEngine.SceneManagement;

public class EntranceDoor : MonoBehaviour
{
    public string forestSceneName = "ForestScene";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Entering Forest, starting new run.");
            GameManager.Instance.StartNewRun();
            SceneManager.LoadScene(forestSceneName);
        }
    }
}
