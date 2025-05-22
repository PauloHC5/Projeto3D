using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _Instance;

    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float timeToSpawn = 1f;
    [SerializeField] private float spawnInterval = 2f;

    private int enemyCount = 0;
    private GameObject[] spawnPoints;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), timeToSpawn, spawnInterval);

        // Find all spawn points in the scene by tag
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if(spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points found in the scene.");
        }
    }

    // Static instance of GameManager which allows it to be accessed by any other script
    public static GameManager Instance {
        get
        {
            if(_Instance == null)
            {
                _Instance = Object.FindFirstObjectByType<GameManager>();                
            }

            return _Instance;
        }        
    }

    public PlayerCharacterController Player { get; private set; }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {        
        // Make this instance persistent across scenes
        //DontDestroyOnLoad(gameObject);

        // Find the player character controller in the scene
        Player = Object.FindFirstObjectByType<PlayerCharacterController>();                
    }    

    private void Update()
    {
        // Check if the 'P' key is pressed
        if (Input.GetKeyDown(KeyCode.P))
        {
            ReloadScene();
        }

        // Check if the 'Escape' key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitApplication();
        }

        enemyCount = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;

        if (enemyCount >= maxEnemies)
        {
            // Stop spawning enemies if the limit is reached
            CancelInvoke(nameof(SpawnEnemy));
        }
    }

    private void ReloadScene()
    {
        _Instance = null;
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void QuitApplication()
    {
        // Quit the application
        Application.Quit();
    }

    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeInitialize()
    {
        _Instance = null;
        Debug.Log("GameManager has been reset.");
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
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform;

            // Get a random enemy from the list
            Enemy enemy = enemies[Random.Range(0, enemies.Count)];

            // Instantiate the enemy at the spawn point
            Instantiate(enemy, spawnPoint.position, Quaternion.identity);
        }
    }
}
