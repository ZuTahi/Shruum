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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        if (ForestManager.Instance == null)
        {
            GameObject forestManagerObj = new GameObject("ForestManager");
            forestManagerObj.AddComponent<ForestManager>();
        }

        ForestManager.Instance.GenerateRoomSequence();

        // ✅ Load the waiting Room0 scene
        SceneManager.LoadSceneAsync("Room0");
    }

    private void ResetCurrentRunData()
    {
        Debug.Log("Resetting current run data...");
        // Clear temporary run-based data here if needed
    }

    public void SaveGame()
    {
        SaveSystem.SavePlayer();
    }

    public void LoadGame()
    {
        SaveSystem.LoadPlayer();
    }

    public void RespawnAtHub()
    {
        Debug.Log("Player died, respawning at Hub...");
        RunData.ClearRunData();   // Clear temporary buffs
        ResetCurrentRunData();
        SceneManager.sceneLoaded += OnHubLoaded;
        SceneManager.LoadScene("SampleScene");
        SaveGame();
    }
    private void OnHubLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene")
        {
            PlayerStats.Instance?.RevivePlayer();
            ModularWeaponSlotManager.Instance?.ApplyEquippedWeaponsFromPlayerData();
        }
        SceneManager.sceneLoaded -= OnHubLoaded; // Unsubscribe
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        RunData.ClearRunData();   // <-- Clear buffs
        SaveGame();
        Application.Quit();
    }

    private void OnApplicationQuit()
    {
        QuitGame();
    }
}
