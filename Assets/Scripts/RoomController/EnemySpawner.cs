using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaveEntry
{
    public GameObject enemyPrefab;
    public int count;
}

[System.Serializable]
public class Wave
{
    public List<WaveEntry> enemies;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Waves Configuration")]
    public List<Wave> waves = new List<Wave>();
    public Transform[] spawnPoints;

    private int currentWaveIndex = 0;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private RoomManager roomManager;

    public void SetRoomManager(RoomManager manager)
    {
        roomManager = manager;
    }

    public void StartSpawning()
    {
        currentWaveIndex = 0;
        SpawnCurrentWave();
    }

    private void SpawnCurrentWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("All waves cleared!");
            if (roomManager != null)
            {
                roomManager.OnRoomCleared();
            }
            return;
        }

        Debug.Log($"Spawning Wave {currentWaveIndex + 1}");
        Wave currentWave = waves[currentWaveIndex];

        foreach (WaveEntry entry in currentWave.enemies)
        {
            for (int i = 0; i < entry.count; i++)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject enemy = Instantiate(entry.enemyPrefab, spawnPoint.position, Quaternion.identity);
                spawnedEnemies.Add(enemy);

                EnemyAIController ai = enemy.GetComponent<EnemyAIController>();
                if (ai != null)
                {
                    ai.spawner = this;
                }
            }
        }
    }

    public void OnEnemyDeath(GameObject enemy)
    {
        if (spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Remove(enemy);
        }

        if (spawnedEnemies.Count == 0)
        {
            Debug.Log($"Wave {currentWaveIndex + 1} cleared!");
            currentWaveIndex++;
            SpawnCurrentWave();
        }
    }
}
