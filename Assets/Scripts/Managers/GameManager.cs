using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>   
{
    [SerializeField] private bool _skipPlayerTutorial = false;

    [Space]

    [SerializeField] private GameObject _canvasGameOver;
    [SerializeField] private GameObject _canvasVictory;

    [Space] [Header("Player progress")] 
    [SerializeField] private List<GameObject> weaponsPrefabs;
    
    private WaveManager _waveManager;
    private Transform _weaponsSpawnPoint;
    private PulsatingLightBehaviour _lightEffect;
    private List<GameObject> weaponsToSpawn = new List<GameObject>();

    public static bool SkipPlayerTutorial => Instance._skipPlayerTutorial;
    public PlayerCharacterController Player { get; private set; }
    public static bool IsPaused { get; private set; } = false;
    public static bool AlreadyPlayedIntroCutscene = false; // Flag to check if the intro cutscene has already been played
    
    public static event Action OnPauseGame;
    public static event Action OnResumeGame;
    public static event Action OnGameOver;

    public static float HordeTimer => Instance._waveManager.HordeTimer;
    public static WaveStatus WaveStatus => Instance._waveManager.WaveStatus;
    public static int EnemiesInSceneCount => Instance._waveManager.EnemiesInSceneCount;
    public static int CurrentWave => Instance._waveManager.CurrentWave;
    
    private void Awake()
    {
        OnAwake();
        
        _waveManager = GetComponent<WaveManager>();
    }    
    
    private void Start()
    {
        HandleSceneStart();
    }

    private void HandleSceneStart()
    {
        // Find the player character controller in the scene
        Player = UnityEngine.Object.FindFirstObjectByType<PlayerCharacterController>();

        _weaponsSpawnPoint = GameObject.FindWithTag("WeaponsSpawnPoint")?.transform;
        if (_weaponsSpawnPoint == null)
            Debug.LogError("Weapons spawn point not found int the scene.");
        
        
        SoundManager.PlayMusic(MusicType.AMBIENCE, true);
        
        weaponsToSpawn = new List<GameObject>(weaponsPrefabs);

        if (!_skipPlayerTutorial)                           
            StartCoroutine(IntroductionRoutine());
        else
            StartCoroutine(StartGame());
        
        _lightEffect = FindFirstObjectByType<PulsatingLightBehaviour>();
        if(!_lightEffect) Debug.LogWarning("No PulsatingLightBehaviour found in the scene.");
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

        HUDManager.Disable(); 

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

        Instance._waveManager.StartHorde(WaveFinished);

        if(_lightEffect) _lightEffect.enabled = false;
        
        yield return null;
    }    
    
    private IEnumerator WaveFinished()
    {
        if (_waveManager.WavesFinished)
        {
            Instantiate(_canvasVictory, Vector3.zero, Quaternion.identity);
            PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.UI);
            Cursor.lockState = CursorLockMode.None;
            FadeManager.FadeOut(() => { });
        }
        else
        {
            if(weaponsPrefabs.Count == 0)
            {
                yield return new WaitForSeconds(5f);
                StartCoroutine(StartGame());
                yield break;
            }
            
            var weaponPrefab = weaponsToSpawn.FirstOrDefault();
            weaponsToSpawn.Remove(weaponPrefab);
            weaponPrefab = Instantiate(weaponPrefab, _weaponsSpawnPoint.position, Quaternion.identity);
        
            var weaponPickup = weaponPrefab.GetComponent<PickupBehaviour>();
            if (weaponPickup is null) yield break;
        
            weaponPickup.OnPickup += PlayerPickedUpWeapon; // Subscribe to the weapon pickup event
        }
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
        Instance._waveManager.StopHorde();
        
        SoundManager.PlayMusic(MusicType.DEFEAT);
        
        FadeManager.FadeOut(() => { });
        
        // Spawn endgame canvas
        if (Instance._canvasGameOver != null)
        {
            Instantiate(Instance._canvasGameOver, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Endgame canvas is not assigned in the GameManager.");
        }

        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.CUTSCENE);
        Cursor.lockState = CursorLockMode.None;
        
        // Invoke game over event
        OnGameOver?.Invoke();
    }

    public static void ReloadScene()
    {        
        Instance._waveManager.StopHorde();
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneStart();
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        WaveManager.onWaveStatusChanged = null; // Clear all subscribers to avoid duplicates
    }
}
