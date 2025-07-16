using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public PlayerStats playerStats;
    public PlayerInventory playerInventory;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNewRun()
    {
        Debug.Log("GameManager: Starting a new run!");
        ResetCurrentRunData();
    }

    private void ResetCurrentRunData()
    {
        Debug.Log("Resetting current run data...");
        // Clear temporary run-based data here if needed
    }

    public void SaveGame()
    {
        SaveSystem.SavePlayer(playerStats, playerInventory);
    }

    public void LoadGame()
    {
        SaveSystem.LoadPlayer(playerStats, playerInventory);
    }

    public void RespawnAtHub()
    {
        Debug.Log("Player died, respawning at Hub...");
        ResetCurrentRunData();
        SceneManager.LoadScene("HubScene");
        SaveGame();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        SaveGame();
        Application.Quit();
    }

    private void OnApplicationQuit()
    {
        QuitGame();
    }
}
