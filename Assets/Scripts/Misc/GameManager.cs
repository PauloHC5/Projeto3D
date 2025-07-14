using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>   
{
    [SerializeField] private bool _skipPlayerTutorial = false;
    public static bool SkipPlayerTutorial => Instance._skipPlayerTutorial;

    [Space]

    [SerializeField] private GameObject _canvasGameOver;
    

    [Header("Enemy Spawning Properties")]
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float timeToSpawn = 1f;
    [SerializeField] private float spawnInterval = 2f;

    private List<GameObject> enemiesInScene = new List<GameObject>();
    private GameObject[] spawnPoints;

    public static PlayerCharacterController Player { get; private set; }

    public static bool IsPaused { get; private set; } = false;

    public static bool alreadyPlayedIntroCutscene = false; // Flag to check if the intro cutscene has already been played

    // Action event to be invoked when PauseGame is called
    public static event Action OnPauseGame;

    // Action event to be invoked when ResumeGame is called
    public static event Action OnResumeGame;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Find the player character controller in the scene
        Player = UnityEngine.Object.FindFirstObjectByType<PlayerCharacterController>();        

        // Find all spawn points in the scene by tag
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Length == 0)
            Debug.LogWarning("No enemy spawn points found in the scene.");
    }    

    private void Start()
    {
        if (SoundManager.CurrentMusicType != MusicType.AMBIENCE) SoundManager.PlayMusic(MusicType.AMBIENCE, true);

        if (!_skipPlayerTutorial)                           
            StartCoroutine(IntroductionRoutine());
        else
            StartGame();
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

    private IEnumerator IntroductionRoutine()
    {                
        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.CUTSCENE);

        Cursor.lockState = CursorLockMode.Locked;                

        if (!alreadyPlayedIntroCutscene)
        {
            alreadyPlayedIntroCutscene = true; // Set the flag to true after playing the cutscene
            yield return StartCoroutine(CutsceneManager.StartCutscene(CutsceneType.INTRO)); // Play and wait until cutscene is finished
        }                

        StartGame();

    }    

    public static void StartGame()
    {
        Time.timeScale = 1f; // Resume time scale     
        if(!SkipPlayerTutorial) FadeManager.FadeIn(() => {});

        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.GAMEPLAY);
        Player.GetComponent<PlayerCharacterCombatController>().enabled = true;

    }    

    public static void PauseGame()
    {
        if(TutorialManager.IsPlayingTutorial) return;
        
        Time.timeScale = 0f;
        IsPaused = true;
        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.UI);

        // Invoke pause event
        OnPauseGame?.Invoke();
    }

    public static void ResumeGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        
        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.GAMEPLAY);

        // Invoke resume event
        OnResumeGame?.Invoke();
    }

    public static void GameOver()
    {                        
        HUDManager.Disable();
        // Spawn endgame canvas
        if (Instance._canvasGameOver != null)
        {
            Instantiate(Instance._canvasGameOver, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Endgame canvas is not assigned in the GameManager.");
        }

        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.UI);
        Cursor.lockState = CursorLockMode.None;
    }

    public static void ReloadScene()
    {        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        // If running in the editor, stop playing
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
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

    public static void EnemyDied(Enemy enemy)
    {
        // Remove the enemy from the list of enemies in the scene
        Instance.enemiesInScene.Remove(enemy.gameObject);
    }

    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeInitialize()
    {
        alreadyPlayedIntroCutscene = false; // Reset the flag when the game starts
    }
}
