using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>   
{
    [SerializeField] private bool _skipPlayerTutorial = false;

    [Space]

    [SerializeField] private GameObject _canvasGameOver;

    [Space] [Header("Player progress")] 
    [SerializeField] private List<GameObject> weaponsPrefabs;
    [SerializeField] private Transform weaponsSpawnPoint;
    
    private WaveManager _waveManager;

    public static bool SkipPlayerTutorial => Instance._skipPlayerTutorial;
    public static PlayerCharacterController Player { get; private set; }
    public static bool IsPaused { get; private set; } = false;
    public static bool AlreadyPlayedIntroCutscene = false; // Flag to check if the intro cutscene has already been played
    
    public static event Action OnPauseGame;
    public static event Action OnResumeGame;

    public static float HordeTimer => Instance._waveManager.HordeTimer;
    public static HordeStatus HordeStatus => Instance._waveManager.HordeStatus;
    public static int EnemiesInSceneCount => Instance._waveManager.EnemiesInSceneCount;
    public static int CurrentWave => Instance._waveManager.CurrentWave;
    
    private void Awake()
    {
        if(weaponsSpawnPoint == null) Debug.LogError("Weapons spawn point is not assigned in the GameManager.");
        
        // Find the player character controller in the scene
        Player = UnityEngine.Object.FindFirstObjectByType<PlayerCharacterController>();      
        _waveManager = GetComponent<WaveManager>();
    }    

    private void Start()
    {
        if (SoundManager.CurrentMusicType != MusicType.AMBIENCE) SoundManager.PlayMusic(MusicType.AMBIENCE, true);

        if (!_skipPlayerTutorial)                           
            StartCoroutine(IntroductionRoutine());
        else
            StartCoroutine(StartGame());
    }

    private void Update()
    {
        // Check if the 'P' key is pressed
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ReloadScene();
        }
    }

    private IEnumerator IntroductionRoutine()
    {                
        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.CUTSCENE);

        Cursor.lockState = CursorLockMode.Locked;                

        if (!AlreadyPlayedIntroCutscene)
        {
            AlreadyPlayedIntroCutscene = true; // Set the flag to true after playing the cutscene
            yield return StartCoroutine(CutsceneManager.StartCutscene(CutsceneType.INTRO)); // Play and wait until cutscene is finished
        }

        StartCoroutine(StartGame());
    }    

    private IEnumerator StartGame()
    {
        Debug.Log("Starting Game");
        
        Time.timeScale = 1f; // Resume time scale     
        if(!SkipPlayerTutorial) FadeManager.FadeIn(() => {});

        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.GAMEPLAY);
        Player.GetComponent<PlayerCharacterCombatController>().enabled = true;

        yield return Instance._waveManager.StartHorde();
        
        if(weaponsPrefabs.Count == 0)
        {
            Debug.LogWarning("No weapons available to spawn. Please assign weapons in the GameManager.");
            yield break;
        }
        
        var weaponPrefab = weaponsPrefabs.First();
        weaponsPrefabs.Remove(weaponPrefab);
        weaponPrefab = Instantiate(weaponPrefab, weaponsSpawnPoint.position, Quaternion.identity);
        
        var weaponPickup = weaponPrefab.GetComponent<PickupBehaviour>();
        if (weaponPickup is null)
        {
            yield break;
        }
            
        
        weaponPickup.OnPickup += PlayerPickedUpWeapon; // Subscribe to the weapon pickup event
    }    
    
    private void PlayerPickedUpWeapon()
    {
        StartCoroutine(StartGame());
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
        Instance._waveManager.StopHorde();
        
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

    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeInitialize()
    {
        Instance._waveManager.StopHorde();
        AlreadyPlayedIntroCutscene = false; // Reset the flag when the game starts
    }
}
