using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    [Header("Door Components")]
    public Animator doorAnimator;

    public string nextSceneName; // assign in inspector

    public void Unlock()
    {
        if (doorAnimator != null)
            doorAnimator.SetBool("isLocked", false);

        Debug.Log($"{gameObject.name} is now Unlocked.");

        // Save player stats before leaving this room
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.SaveToPlayerData();
        }

        // Then load next room
        SceneManager.LoadScene(nextSceneName);
    }
}

