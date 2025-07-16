using UnityEngine;
using System.Collections.Generic;

public class ForestManager : MonoBehaviour
{
    public static ForestManager Instance;

    public List<GameObject> forestRoomPrefabs; // pool of unique rooms
    public GameObject bossRoomPrefab;
    public Transform roomSpawnPoint;
    public GameObject player;

    private Queue<GameObject> shuffledRooms = new Queue<GameObject>();
    private GameObject currentRoom;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartRun()
    {
        Debug.Log("Starting Run: Shuffling rooms.");
        ShuffleRooms();
        SpawnNextRoom();
    }

    private void ShuffleRooms()
    {
        List<GameObject> shuffledList = new List<GameObject>(forestRoomPrefabs);

        for (int i = 0; i < shuffledList.Count; i++)
        {
            int rnd = Random.Range(i, shuffledList.Count);
            var temp = shuffledList[i];
            shuffledList[i] = shuffledList[rnd];
            shuffledList[rnd] = temp;
        }

        shuffledRooms = new Queue<GameObject>(shuffledList);
    }

    public void SpawnNextRoom()
    {
        if (currentRoom != null)
            Destroy(currentRoom);

        GameObject nextRoomPrefab;

        if (shuffledRooms.Count > 0)
        {
            nextRoomPrefab = shuffledRooms.Dequeue();
            Debug.Log("Spawning Forest Room: " + nextRoomPrefab.name);
        }
        else
        {
            nextRoomPrefab = bossRoomPrefab;
            Debug.Log("Spawning Boss Room!");
        }

        currentRoom = Instantiate(nextRoomPrefab, roomSpawnPoint.position, Quaternion.identity);

        RoomManager roomManager = currentRoom.GetComponent<RoomManager>();
        if (roomManager != null)
        {
            player.transform.position = roomManager.playerSpawnPoint.position;
            roomManager.InitializeRoom();
        }
    }
}
