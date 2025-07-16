using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Room Setup")]
    public Transform playerSpawnPoint;
    public DoorController[] doors;
    public EnemySpawner enemySpawner;
    public ArtifactManager artifactManager;

    private bool isRoomCleared = false;

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
