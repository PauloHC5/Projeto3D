using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _Instance;

    [Header("UI Properties")]
    [SerializeField] private HUD hud;
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private EndGameManager endGameManager;
    public HUD Hud
    {
        get { return hud; }        
    }

    [Header("Enemy Spawning Properties")]
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float timeToSpawn = 1f;
    [SerializeField] private float spawnInterval = 2f;

    private List<GameObject> enemiesInScene = new List<GameObject>();
    private GameObject[] spawnPoints;

    public PlayerCharacterController Player { get; private set; }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Make this instance persistent across scenes
        //DontDestroyOnLoad(gameObject);

        if (_Instance == null)
        {
            _Instance = Object.FindFirstObjectByType<GameManager>();
        }
        else if (_Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }

        // Find the player character controller in the scene
        Player = Object.FindFirstObjectByType<PlayerCharacterController>();

        if (hud == null)
            hud = GameObject.FindAnyObjectByType<HUD>(FindObjectsInactive.Include);

        if (pauseManager == null)
            pauseManager = GameObject.FindAnyObjectByType<PauseManager>(FindObjectsInactive.Include);

        if (endGameManager == null)
            endGameManager = GameObject.FindAnyObjectByType<EndGameManager>(FindObjectsInactive.Include);

        // Find all spawn points in the scene by tag
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Length == 0)
            Debug.LogWarning("No enemy spawn points found in the scene.");
    }    

    private void Start()
    {       
        Time.timeScale = 0f;

        TutorialManager.Instance.gameObject.SetActive(true); // Show the tutorial manager UI
        PlayerCharacterController.PlayerControls.UI.Enable();
        PlayerCharacterController.PlayerControls.Player.Disable();
        Cursor.lockState = CursorLockMode.None;

    }

    private IEnumerator StartGameRoutine()
    {        
        yield return new WaitForSeconds(1f); // Wait for 1 second before enabling controls        
        PlayerCharacterController.PlayerControls.UI.Disable();
        PlayerCharacterController.PlayerControls.Player.Enable();
        hud.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        InvokeRepeating(nameof(SpawnEnemy), timeToSpawn, spawnInterval);
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
      

    private void Update()
    {
        // Check if the 'P' key is pressed
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ReloadScene();
        }           
        
        if (enemiesInScene.Count >= maxEnemies)
        {
            CancelInvoke(nameof(SpawnEnemy));
        }
    }

    public static void StartGame()
    {
        if (_Instance == null)
        {
            _Instance = Object.FindFirstObjectByType<GameManager>();
        }

        Time.timeScale = 1f; // Resume time scale
        _Instance.StartCoroutine(_Instance.StartGameRoutine());
        SoundManager.PlayMusic(); // Play the gameplay music
    }

    public static void GameOver()
    {
        if (_Instance == null)
        {
            _Instance = Object.FindFirstObjectByType<GameManager>();
        }
        
        Destroy(_Instance.hud.gameObject);
        _Instance.hud = null; // Clear the HUD reference
        _Instance.pauseManager.gameObject.SetActive(false); // Hide the pause manager UI
        _Instance.endGameManager.gameObject.SetActive(true); // Show the end game manager UI
        PlayerCharacterController.PlayerControls.UI.Disable();
        PlayerCharacterController.PlayerControls.Player.Enable();
        Cursor.lockState = CursorLockMode.None;
    }

    public static void ReloadScene()
    {
        _Instance = null;
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
            Enemy enemySpawned = Instantiate(enemy, spawnPoint.position, Quaternion.identity);

            // Add the spawned enemy to the list of enemies in the scene
            enemiesInScene.Add(enemySpawned.gameObject);
        }
    }

    internal void EnemyDied(Enemy enemy)
    {
        // Remove the enemy from the list of enemies in the scene
        enemiesInScene.Remove(enemy.gameObject);
    }    
}
