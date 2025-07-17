using System.Collections.Generic;
using UnityEngine;

public class ForestManager : MonoBehaviour
{
    public static ForestManager Instance { get; private set; }

    [Header("Room Configuration")]
    public List<string> normalRoomScenes;  // Populate via inspector (e.g., Room1, Room2,...)
    public string midBossScene;
    public string finalBossScene;

    private List<string> roomSequence = new List<string>();
    private int currentRoomIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Generates the shuffled room sequence for the current run.
    /// </summary>
    public void GenerateRoomSequence()
    {
        roomSequence.Clear();
        currentRoomIndex = 0;

        List<string> shuffledRooms = new List<string>(normalRoomScenes);
        Shuffle(shuffledRooms);

        // Example: 8 normal rooms, midboss after 4th, final boss last
        int normalRoomsBeforeMidBoss = 4;
        int normalRoomsAfterMidBoss = 4;

        for (int i = 0; i < normalRoomsBeforeMidBoss; i++)
        {
            roomSequence.Add(shuffledRooms[i % shuffledRooms.Count]);
        }

        roomSequence.Add(midBossScene);

        for (int i = normalRoomsBeforeMidBoss; i < normalRoomsBeforeMidBoss + normalRoomsAfterMidBoss; i++)
        {
            roomSequence.Add(shuffledRooms[i % shuffledRooms.Count]);
        }

        roomSequence.Add(finalBossScene);

        Debug.Log("ForestManager: Generated Room Sequence:");
        foreach (string sceneName in roomSequence)
        {
            Debug.Log(sceneName);
        }
    }

    /// <summary>
    /// Returns the current scene name in the sequence without advancing.
    /// </summary>
    public string GetNextRoomScene()
    {
        if (currentRoomIndex < roomSequence.Count)
        {
            return roomSequence[currentRoomIndex];
        }
        else
        {
            Debug.LogWarning("ForestManager: No more rooms in sequence.");
            return null;
        }
    }

    /// <summary>
    /// Advances the room index after a room is loaded.
    /// </summary>
    public void AdvanceRoomIndex()
    {
        currentRoomIndex++;
    }

    /// <summary>
    /// Shuffles a list in-place.
    /// </summary>
    private void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
