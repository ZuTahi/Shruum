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

    // ForestManager will call this to inject the correct difficulty waves
    public void AssignWaves(List<Wave> newWaves)
    {
        waves = newWaves ?? new List<Wave>();
    }

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
        if (waves == null || waves.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: No waves assigned.");
            roomManager?.OnRoomCleared();
            return;
        }

        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("All waves cleared!");
            roomManager?.OnRoomCleared();
            return;
        }

        Debug.Log($"Spawning Wave {currentWaveIndex + 1}");
        Wave currentWave = waves[currentWaveIndex];

        foreach (WaveEntry entry in currentWave.enemies)
        {
            for (int i = 0; i < entry.count; i++)
            {
                if (spawnPoints == null || spawnPoints.Length == 0)
                {
                    Debug.LogError("EnemySpawner: spawnPoints not set.");
                    return;
                }

                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (entry.enemyPrefab == null)
                {
                    Debug.LogWarning("EnemySpawner: an entry has a null enemyPrefab.");
                    continue;
                }

                GameObject enemy = Instantiate(entry.enemyPrefab, spawnPoint.position, Quaternion.identity);
                spawnedEnemies.Add(enemy);

                EnemyAIController ai = enemy.GetComponent<EnemyAIController>();
                if (ai != null)
                {
                    ai.spawner = this;

                    // Copy data from prefab (if present)
                    EnemyAIController prefabAI = entry.enemyPrefab.GetComponent<EnemyAIController>();
                    if (prefabAI != null)
                    {
                        ai.data = prefabAI.data;
                    }
                    else
                    {
                        Debug.LogWarning($"{entry.enemyPrefab.name} has no EnemyAIController on prefab to copy data from.");
                    }
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

    // Optional: cleanup to avoid stale references if scene unloads early
    private void OnDisable()
    {
        spawnedEnemies.Clear();
    }
}
