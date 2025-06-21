using UnityEngine;
using System;
using UnityEngine.Audio;

[Serializable]
public enum WorldSfxType
{
    SHOOT,
    RELOAD,    
    WOODSMAN_ATTACK,
    FOOTSTEP,
    CHEMAGENT_GAS,        
    SUPERSHOOT,
    CLOSEDOOR,    
}

[Serializable]
public enum GlobalSfxTypes
{             
    MENUCLICK,
    MENUHOVER,
    HIT,
    DEATH,
    PICKUP,
}

[Serializable]
public enum MusicType
{
    MENU,
    BATTLE,
    AMBIENCE,    
    VICTORY,
    DEFEAT,

    DEFAULT, // Default music type for fallback
}

[Serializable]
public enum AmbienceSoundType
{
    WIND,
    RAIN,
    FOREST,
    CITY,
    OCEAN,
}

[RequireComponent(typeof(AudioSource)), RequireComponent(typeof(AudioSource)), RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{            
    [SerializeField] private MusicClip[] _musics;
    [SerializeField] private SoundClip[] _ambienceSounds, _globalSoundEffects, _worldSoundEffects;    
    
    private AudioSource _ambienceSource;
    private AudioSource _musicSource;    
    private AudioSource _globalSfxSource;
    
    private float _sfxVolume = 1f;
    
    private static MusicType _currentMusicType = MusicType.DEFAULT; // Default music type to avoid null reference issues
    public static MusicType CurrentMusicType => _currentMusicType;

    private static SoundManager _instance; // Do not use directly in the functions, use Instance property instead
    private static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var soundManagers = FindObjectsByType<SoundManager>(FindObjectsSortMode.None);                
                _instance = soundManagers.Length != 0 ? soundManagers[0] : null;

                if (_instance == null)
                {
                    Debug.LogError("SoundManager instance not found in the scene. Please ensure it is added to a GameObject.");
                }

                if (soundManagers.Length > 1)
                {
                    foreach (var mgr in soundManagers)
                    {
                        if (mgr != _instance)
                        {
                            Destroy(mgr.gameObject);
                        }
                    }
                }

                // Make this SoundManager persist across scene loads
                DontDestroyOnLoad(_instance.gameObject);

                if (_instance._musicSource == null || _instance._ambienceSource == null || _instance._globalSfxSource)
                {
                    var audioSources = _instance.GetComponents<AudioSource>();
                    if (audioSources.Length < 3)
                    {
                        Debug.LogError("SoundManager requires at least three AudioSources.");                        
                    }

                    _instance._ambienceSource = audioSources[0];
                    _instance._musicSource = audioSources[1];
                    _instance._globalSfxSource = audioSources[2];
                }
                
            }
            return _instance;
        }
    }

    public static float MusicVolume
    {
        get { return Instance._musicSource.volume; }
        set { Instance._musicSource.volume = Mathf.Clamp(value, 0f, 1f); }
    }

    public static float SfxVolume
    {
        get { return Instance._sfxVolume; }
        set 
        {
            Instance._ambienceSource.volume = Mathf.Clamp(value, 0f, 1f);
            Instance._globalSfxSource.volume = Mathf.Clamp(value, 0f, 1f);
            Instance._sfxVolume = Mathf.Clamp(value, 0f, 1f); 
        } 
    }         

    void Start()
    {        
        if (_instance == null)
        {
            _instance = UnityEngine.Object.FindFirstObjectByType<SoundManager>();
        }

        if (_instance._musicSource == null || _instance._ambienceSource == null || _instance._globalSfxSource == null)
        {
            var audioSources = _instance.GetComponents<AudioSource>();
            if (audioSources.Length < 3)
            {
                Debug.LogError("SoundManager requires at least three AudioSources.");
            }

            _instance._ambienceSource = audioSources[0];
            _instance._musicSource = audioSources[1];
            _instance._globalSfxSource = audioSources[2];
        }
    }

    private void Update()
    {
        if(!Instance._musicSource.isPlaying)
        {
            _currentMusicType = MusicType.DEFAULT; // Reset to default if no music is playing
        }
    }

    public static void PlayRandomSFX(WorldSfxType sfxType, AudioSource sfxSource, bool loop = false)
    {        
        AudioClip[] clips = Instance._worldSoundEffects[(int)sfxType].Sounds;
        // Check if index is within bounds
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"SoundManager: No sound clips found for {sfxType}.");
            return;
        }

        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];        
        if (randomClip == null)
        {
            Debug.LogWarning($"SoundManager: AudioSource or clip for {randomClip.name} is not set.");
            return;
        }

        if (!loop)
        {
            sfxSource.PlayOneShot(randomClip, Instance._sfxVolume);
        }
        else
        {
            if (sfxSource.isPlaying) return;                

            sfxSource.volume = Instance._sfxVolume;
            sfxSource.clip = randomClip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
    }

    public static void PlayRandomSFX(GlobalSfxTypes sfxType)
    {        
        AudioClip[] clips = Instance._globalSoundEffects[(int)sfxType].Sounds;

        // Check if index is within bounds
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"SoundManager: No sound clips found for {sfxType}.");
            return;
        }

        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        if (randomClip != null && Instance._globalSfxSource != null)
        {
            Instance._globalSfxSource.PlayOneShot(randomClip, Instance._sfxVolume);
        }
        else
        {
            Debug.LogWarning($"SoundManager: AudioSource or clip for {randomClip.name} is not set.");
        }
    }    

    public static void PlayShootSound(WeaponTypes weaponType, AudioSource audioSource)
    {
        var shootSound = Instance._worldSoundEffects[(int)WorldSfxType.SHOOT].Sounds[(int)weaponType];

        if(shootSound != null) {
            audioSource.volume = Instance._sfxVolume;
            audioSource.PlayOneShot(shootSound, Instance._sfxVolume);
        }
        else
        {
            Debug.LogWarning($"SoundManager: No shoot sound found for {weaponType}.");
        }        
    }    

    public static void PlayMusic(MusicType musicType, bool loopMusic = true)
    {                
        if (Instance._musicSource.isPlaying)
            Instance._musicSource.Stop();

        AudioClip musicClip = Instance._musics[(int)musicType].Sounds;
        if (musicClip != null)
        {
            Instance._musicSource.enabled = true;
            Instance._musicSource.clip = musicClip;
            Instance._musicSource.loop = loopMusic;
            Instance._musicSource.Play();

            _currentMusicType = musicType;
        }
        else
        {
            Debug.LogWarning($"SoundManager: Music clip for {musicType} is not set.");
        }
    }

    public static void PlayGlobalSfx(GlobalSfxTypes sfxType)
    {        
        AudioClip[] sfxClips = Instance._globalSoundEffects[(int)sfxType].Sounds;
        AudioClip randomClip = sfxClips[UnityEngine.Random.Range(0, sfxClips.Length)];
        
        if (Instance._globalSfxSource != null && randomClip != null)
        {
            Instance._globalSfxSource.PlayOneShot(randomClip, Instance._sfxVolume);
        }
        else
        {
            Debug.LogWarning($"SoundManager: AudioSource or clip for {sfxType} is not set.");
        }
    }

    public static void PlayAmbienceSound(AmbienceSoundType ambienceSoundType)
    {
        AudioClip[] ambienceClips = Instance._ambienceSounds[(int)ambienceSoundType].Sounds;
        AudioClip ramdomClip = ambienceClips[UnityEngine.Random.Range(0, ambienceClips.Length)];        

        if (Instance._ambienceSource != null && ramdomClip != null)
        {
            if(Instance._ambienceSource.isPlaying)
                Instance._ambienceSource.Stop();

            Instance._ambienceSource.clip = ramdomClip;
            Instance._ambienceSource.Play();
            Instance._ambienceSource.loop = true;
        }
        else
        {
            Debug.LogWarning($"SoundManager: AudioSource or clip for {ramdomClip.name} is not set.");
        }
    }

#if UNITY_EDITOR
    private void OnEnable()
    {        
        string[] names = Enum.GetNames(typeof(MusicType));
        Array.Resize(ref _musics, names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            _musics[i]._name = names[i];
        }

        names = Enum.GetNames(typeof(AmbienceSoundType));
        Array.Resize(ref _ambienceSounds, names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            _ambienceSounds[i]._name = names[i];
        }

        names = Enum.GetNames(typeof(GlobalSfxTypes));
        Array.Resize(ref _globalSoundEffects, names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            _globalSoundEffects[i]._name = names[i];
        }

        names = Enum.GetNames(typeof(WorldSfxType));
        Array.Resize(ref _worldSoundEffects, names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            _worldSoundEffects[i]._name = names[i];
        }

        PauseManager.OnPauseGame += OnPauseGame;
        PauseManager.OnResumeGame += OnResumeGame;
    }
#endif                

    private void OnDisable()
    {
        PauseManager.OnPauseGame -= OnPauseGame;
        PauseManager.OnResumeGame -= OnResumeGame;
    }

    private void OnPauseGame()
    {        
        Instance._ambienceSource.Pause();
        Instance._musicSource.Pause();
    }

    private void OnResumeGame()
    {        
        Instance._ambienceSource.UnPause();
        Instance._musicSource.UnPause();
    }

    private void OnApplicationQuit()
    {
        // Disable the ambience source to stop playing sounds when the application quits
        if (_ambienceSource != null)
        {
            _ambienceSource.Stop();
            _ambienceSource.clip = null; // Clear the clip to avoid memory leaks
        }

        // Clean up the instance when the application quits
        if (_instance != null)
            _instance = null;        
    }
}


[Serializable]
public struct SoundClip
{
    [HideInInspector] public string _name;
    [SerializeField] private AudioClip[] _sounds;    

    public AudioClip[] Sounds => _sounds;
}

[Serializable]
public struct MusicClip
{
    [HideInInspector] public string _name;
    [SerializeField] private AudioClip _music;    
    public AudioClip Sounds => _music;
}
