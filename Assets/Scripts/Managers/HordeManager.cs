using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct HordeManagerData
{
    public int timeToStart;
    public int timeToEnd;
    public List<Enemy> enemies;
    public int maxEnemies;
    public float timeToSpawn;
    public float spawnInterval;
}

public class HordeManager : MonoBehaviour
{
    [Header("Enemy Spawning Properties")]
    [SerializeField] private HordeManagerData[] hordeManagerDatas;
    [SerializeField] private int hordeIndex = 0;
    [SerializeField] private float hordeTimer;

    private List<GameObject> enemiesInScene = new List<GameObject>();
    private GameObject[] spawnPoints;
    
    private Coroutine spawnCoroutine;
    
    public float HordeTimer => hordeTimer;
    
    private void FindSpawnPoints()
    {
        // Find all spawn points in the scene by tag
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Length == 0)
            Debug.LogWarning("No enemy spawn points found in the scene.");
    }

    public void StartHorde()
    {
        // Check if the player is not null
        if (spawnPoints != null && spawnPoints.Length == 0)
        {
            FindSpawnPoints();
            
            if (spawnPoints.Length == 0) return;
        }
        
        if(hordeManagerDatas.Length == 0 || hordeIndex >= hordeManagerDatas.Length)
        {
            Debug.LogWarning("HordeManagerData is not set up correctly.");
            return;
        }
        
        if (hordeManagerDatas[hordeIndex].enemies.Count == 0)
        {
            Debug.LogWarning("No enemies available to spawn.");
            return;
        }
        
        if (enemiesInScene.Count >= hordeManagerDatas[hordeIndex].maxEnemies)
        {
            CancelInvoke(nameof(SpawnEnemy));
            return;
        }
        
        // Start spawning enemies at the specified interval
        if (spawnCoroutine == null)
            spawnCoroutine = StartCoroutine(SpawnEnemiesCoroutine(hordeManagerDatas[hordeIndex]));
        else
            Debug.LogWarning("Horde is already spawning enemies.");
    }
    
    private IEnumerator SpawnEnemiesCoroutine(HordeManagerData hordeManagerData)
    {
        hordeTimer = hordeManagerData.timeToStart;
        while (hordeTimer > 0f)
        {
            // You can display 'timer' as the countdown value
            hordeTimer -= Time.deltaTime;
            yield return null;
        }
        
        while (enemiesInScene.Count < hordeManagerData.maxEnemies)
        {
            SpawnEnemy(hordeManagerData);
            yield return new WaitForSeconds(hordeManagerData.spawnInterval);
        }
    }

    public void StopHorde()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    private void SpawnEnemy(HordeManagerData hordeManagerData)
    {
        // Get a random spawn point from the list
        var spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform;

        // Get a random enemy from the list
        var enemy = hordeManagerData.enemies[UnityEngine.Random.Range(0, hordeManagerData.enemies.Count)];

        // Instantiate the enemy at the spawn point
        var enemySpawned = Instantiate(enemy, spawnPoint.position, Quaternion.identity);

        // Add the spawned enemy to the list of enemies in the scene
        enemiesInScene.Add(enemySpawned.gameObject);
    }
}
