using System;
using System.Collections;
using System.Collections.Generic;
using Misc;
using UnityEngine;

[Serializable]
public struct WaveData
{
    public int preparationTime;
    public List<Enemy> enemies;
    public int enemiesAmountToSpawn;
    public int maxEnemiesInTheScene;
    public float timeToSpawn;
    public float spawnInterval;
}

public enum WaveStatus
{
    NotStarted,
    Preparing,
    Running,
    Finishing,
    Finished
}

public class WaveManager : MonoBehaviour
{
    [SerializeField] private WaveData[] wavesDatas;
    [SerializeField] private int hordeIndex = 0;
    
    [Space]
    
    public static Action<WaveStatus> onWaveStatusChanged;

    private float hordeTimer;
    private List<GameObject> _enemiesInScene = new List<GameObject>();
    private int enemiesInSceneCount;
    private GameObject[] _spawnPoints;
    private WaveStatus _waveStatus = WaveStatus.NotStarted;
    private int _enemiesSpawned = 0;
    private PulsatingLightBehaviour _lightEffect;
    
    private Coroutine _waveCoroutine;
    
    public float HordeTimer => hordeTimer;
    public WaveStatus WaveStatus => _waveStatus;
    public int EnemiesInSceneCount => _enemiesInScene.Count;
    public int CurrentWave => hordeIndex + 1;
    public bool WavesFinished => hordeIndex >= wavesDatas.Length;

    private void Start()
    {
        _lightEffect = FindFirstObjectByType<PulsatingLightBehaviour>();
        if(!_lightEffect) Debug.LogWarning("No PulsatingLightBehaviour found in the scene.");
    }

    private void Update()
    {
        enemiesInSceneCount = _enemiesInScene.Count;
    }

    private void FindSpawnPoints()
    {
        // Find all spawn points in the scene by tag
        _spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (_spawnPoints.Length == 0)
            Debug.LogWarning("No enemy spawn points found in the scene.");
    }

    public void StartHorde(Func<IEnumerator> onFinishedCallback)
    {
        // Check if the player is not null
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            FindSpawnPoints();
            
            if (_spawnPoints.Length == 0) return;
        }
        
        if(wavesDatas.Length == 0 || hordeIndex >= wavesDatas.Length)
        {
            Debug.LogWarning("HordeManagerData is not set up correctly.");
            return;
        }
        
        if (wavesDatas[hordeIndex].enemies.Count == 0)
        {
            Debug.LogWarning("No enemies available to spawn.");
            return;
        }
        
        // Start spawning enemies at the specified interval
        if (_waveCoroutine == null)
        {
            _waveCoroutine = StartCoroutine(WaveCoroutine(onFinishedCallback));
        }
        else
        {
            Debug.LogWarning("Horde is already spawning enemies.");
        }
    }
    
    private IEnumerator WaveCoroutine(Func<IEnumerator> onFinishedCallback)
    {
        // Preparation Phase
        yield return StartCoroutine(HordePreparationCoroutine());
        
        // Running Phase
        SoundManager.PlayMusic(MusicType.PREPAREFORBATTLE, false);
        SoundManager.OnMusicFinished += OnMusicFinished;
        yield return StartCoroutine(HordeRunningCoroutine());
        
        _waveStatus = WaveStatus.Finished;
        onWaveStatusChanged.Invoke(_waveStatus);
        _enemiesSpawned = 0;
        hordeIndex++;
        if(_lightEffect) _lightEffect = FindFirstObjectByType<PulsatingLightBehaviour>();
        _waveCoroutine = null;
        SoundManager.PlayMusic(MusicType.VICTORY, false);
        SoundManager.OnMusicFinished += OnMusicFinished;
        
        if (onFinishedCallback != null)
            yield return StartCoroutine(onFinishedCallback());

    }

    private void OnMusicFinished(MusicType musicType)
    {
        if (_waveStatus == WaveStatus.Running) SoundManager.PlayMusic(MusicType.BATTLE);
        else if (musicType == MusicType.VICTORY) SoundManager.PlayMusic(MusicType.AMBIENCE);
        SoundManager.OnMusicFinished -= OnMusicFinished;
    }
    
    private IEnumerator HordePreparationCoroutine()
    {
        _waveStatus = WaveStatus.Preparing;
        onWaveStatusChanged?.Invoke(_waveStatus);
        hordeTimer = wavesDatas[hordeIndex].preparationTime;
        
        while (hordeTimer > 0f)
        {
            // You can display 'timer' as the countdown value
            hordeTimer -= Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator HordeRunningCoroutine()
    {
        _waveStatus = WaveStatus.Running;
        onWaveStatusChanged?.Invoke(_waveStatus);
        InvokeRepeating(nameof(SpawnEnemies), wavesDatas[hordeIndex].timeToSpawn, wavesDatas[hordeIndex].spawnInterval);
        
        while (_enemiesSpawned != wavesDatas[hordeIndex].enemiesAmountToSpawn)
        {
            yield return null;
        }
        
        CancelInvoke(nameof(SpawnEnemies));
        
        _waveStatus = WaveStatus.Finishing;
        onWaveStatusChanged?.Invoke(_waveStatus);
        while (_enemiesInScene.Count > 0)
        {
            yield return null;
        }
    }

    public void StopHorde()
    {
        if (_waveCoroutine != null)
        {
            StopCoroutine(_waveCoroutine);
            _waveCoroutine = null;
            _waveStatus = WaveStatus.NotStarted;
            onWaveStatusChanged?.Invoke(_waveStatus);
            _enemiesSpawned = 0;
            _enemiesInScene.Clear();
            CancelInvoke(nameof(SpawnEnemies));
            hordeIndex = 0; // Reset to the first horde
            hordeTimer = 0f;
            _spawnPoints = null;
        }
    }
    
    private void SpawnEnemies()
    {
        if(EnemiesInSceneCount >= wavesDatas[hordeIndex].maxEnemiesInTheScene) return;
        
        // Get a random spawn point from the list
        var spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)].transform;

        // Get a random enemy from the list
        var enemy = wavesDatas[hordeIndex].enemies[UnityEngine.Random.Range(0, wavesDatas[hordeIndex].enemies.Count)];

        // Instantiate the enemy at the spawn point
        var enemySpawned = Instantiate(enemy, spawnPoint.position, Quaternion.identity);
        
        _enemiesSpawned++; // Increment the count of spawned enemies

        // Add the spawned enemy to the list of enemies in the scene
        _enemiesInScene.Add(enemySpawned.gameObject);
    }
    
    private void HandleEnemyDeath(Enemy enemy)
    {
        if (_enemiesInScene.Contains(enemy.gameObject))
        {
            _enemiesInScene.Remove(enemy.gameObject);
        }
    }

    private void OnEnable()
    {
        Enemy.OnDeath += HandleEnemyDeath;
    }
    
    private void OnDisable()
    {
        Enemy.OnDeath -= HandleEnemyDeath;
    }
}
