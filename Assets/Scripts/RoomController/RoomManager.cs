using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class RoomManager : MonoBehaviour
{
    [Header("Room Setup")]
    public Transform playerSpawnPoint;
    public DoorController[] doors;
    public EnemySpawner enemySpawner;
    public ArtifactManager artifactManager;

    private bool isRoomCleared = false;

    private void Start()
    {
        StartCoroutine(DelayedInitialization());
    }

    private IEnumerator DelayedInitialization()
    {
        yield return null;  // Wait for 1 frame

        RepositionPlayer();
        InitializeRoom();
    }

    private void RepositionPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;

            Debug.Log("[RoomManager] Player repositioned to spawn point.");
            PlayerUIManager.Instance?.RefreshAllStats();
        }
        else
        {
            Debug.LogWarning("[RoomManager] Player or playerSpawnPoint is missing.");
        }
    }

    public void InitializeRoom()
    {
        Debug.Log("RoomManager: Initializing Room...");

        if (enemySpawner != null)
        {
            // Ask ForestManager for the next difficulty wave set
            WaveSet waveSet = ForestManager.Instance?.GetNextWaveSet();

            if (waveSet != null && waveSet.waves != null && waveSet.waves.Count > 0)
            {
                enemySpawner.AssignWaves(new List<Wave>(waveSet.waves));
            }
            else
            {
                Debug.LogWarning("RoomManager: No WaveSet provided; using waves assigned in the scene's EnemySpawner (if any).");
            }

            enemySpawner.SetRoomManager(this);
            enemySpawner.StartSpawning();
        }
        else
        {
            Debug.LogWarning("No EnemySpawner assigned. Room is instantly cleared.");
            OnRoomCleared();
        }
    }

    private void UnlockDoors()
    {
        foreach (DoorController door in doors)
        {
            door.Unlock();
        }
    }

    public void OnRoomCleared()
    {
        if (isRoomCleared) return;
        isRoomCleared = true;

        Debug.Log("RoomManager: Room Cleared!");
        UnlockDoors();

        StartCoroutine(ShowArtifactChoicesAfterDelay());
    }

    private IEnumerator ShowArtifactChoicesAfterDelay()
    {
        yield return new WaitForSeconds(1.2f); // match door open animation duration
        if (artifactManager != null)
        {
            artifactManager.ShowArtifactChoices(this);
        }
    }

    public void OnArtifactChosen()
    {
        PlayerUIManager.Instance?.RefreshAllStats();
        Debug.Log("RoomManager: Artifact chosen, player may proceed.");
    }
}
