using System;
using System.Collections.Generic;
using UnityEngine;

public class HordeManager : MonoBehaviour
{
    [Header("Enemy Spawning Properties")]
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float timeToSpawn = 1f;
    [SerializeField] private float spawnInterval = 2f;  

    private List<GameObject> enemiesInScene = new List<GameObject>();
    private GameObject[] spawnPoints;

    private void Awake()
    {
        // Find all spawn points in the scene by tag
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Length == 0)
            Debug.LogWarning("No enemy spawn points found in the scene.");
    }

    private void Update()
    {
        if (enemiesInScene.Count >= maxEnemies)
        {
            CancelInvoke(nameof(SpawnEnemy));
        }
    }
    
    private void SpawnEnemy()
    {
        if (enemies.Count == 0)
        {
            Debug.LogWarning("No enemies available to spawn.");
            return;
        }

        // Check if the player is not null
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Get a random spawn point from the list
            Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform;

            // Get a random enemy from the list
            Enemy enemy = enemies[UnityEngine.Random.Range(0, enemies.Count)];

            // Instantiate the enemy at the spawn point
            Enemy enemySpawned = Instantiate(enemy, spawnPoint.position, Quaternion.identity);

            // Add the spawned enemy to the list of enemies in the scene
            enemiesInScene.Add(enemySpawned.gameObject);
        }
    }
}
