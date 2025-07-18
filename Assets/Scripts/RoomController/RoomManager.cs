using UnityEngine;
using System.Collections;

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
    public void InitializeRoom()
    {
        Debug.Log("RoomManager: Initializing Room...");
        LockDoors();

        if (enemySpawner != null)
        {
            enemySpawner.SetRoomManager(this);
            enemySpawner.StartSpawning();
        }
        else
        {
            Debug.LogWarning("No EnemySpawner assigned. Room is instantly cleared.");
            OnRoomCleared();
        }
    }
    private IEnumerator DelayedInitialization()
    {
        // Wait 1 frame to ensure Player is loaded
        yield return null;

        RepositionPlayer();
        InitializeRoom();
    }

    private void RepositionPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && playerSpawnPoint != null)
        {
            Vector3 newPosition = new Vector3(
                playerSpawnPoint.position.x,
                player.transform.position.y,   // Keep current Y
                playerSpawnPoint.position.z
            );

            player.transform.position = newPosition;
            player.transform.rotation = playerSpawnPoint.rotation;

            Debug.Log($"Player repositioned to XZ: {newPosition}");
        }
        else
        {
            Debug.LogWarning("Player or PlayerSpawnPoint is missing!");
        }
    }

    private void LockDoors()
    {
        foreach (DoorController door in doors)
        {
            door.Lock();
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

        if (artifactManager != null)
        {
            artifactManager.ShowArtifactChoices(this);
        }
    }

    public void OnArtifactChosen()
    {
        Debug.Log("RoomManager: Artifact chosen, player may proceed.");
        // Optional: unlock any special exits or perform other logic
    }
}
