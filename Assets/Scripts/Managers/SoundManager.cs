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
    COUGHING
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
    PREPAREFORBATTLE,

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

[RequireComponent(typeof(AudioSource)), RequireComponent(typeof(AudioSource)), RequireComponent(typeof(AudioSource)), RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioMixer sfxAudioMixer;
    [SerializeField] private MusicClip[] _musics;
    [SerializeField] private SoundClip[] _ambienceSounds, _globalSoundEffects, _worldSoundEffects;    
    
    private AudioSource _ambienceSource, _musicSource, _globalSfxSource, _UISfxSource;    
    
    private float _sfxVolume = 1f;
    private bool _gamePaused = false;
    
    private static MusicType _currentMusicType = MusicType.DEFAULT; // Default music type to avoid null reference issues
    public static MusicType CurrentMusicType => _currentMusicType;
    
    public static event Action<MusicType> OnMusicFinished;

    public static float MusicVolume
    {
        get
        {
            Instance.EnsureAudioSourcesInitialized();
            return Instance._musicSource.volume;
        }
        set
        {
            Instance.EnsureAudioSourcesInitialized();
            Instance._musicSource.volume = Mathf.Clamp(value, 0f, 1f);
        }
    }

    public static float SfxVolume
    {
        get 
        {
            Instance.EnsureAudioSourcesInitialized();
            return Instance._sfxVolume;
        }
        set 
        {
            Instance.EnsureAudioSourcesInitialized();

            Instance._ambienceSource.volume = Mathf.Clamp(value, 0f, 1f);
            Instance._globalSfxSource.volume = Mathf.Clamp(value, 0f, 1f);
            Instance._UISfxSource.volume = Mathf.Clamp(value, 0f, 1f);
            Instance._sfxVolume = Mathf.Clamp(value, 0f, 1f); 
        } 
    }

    private void Awake()
    {
        OnAwake();
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (Instance._musicSource && !Instance._musicSource.isPlaying)
            {
                _currentMusicType = MusicType.DEFAULT; // Reset to default if no music is playing
            }
        }        
    }

    private void EnsureAudioSourcesInitialized()
    {
        if (_musicSource == null || _ambienceSource == null || _globalSfxSource == null)
        {
            var audioSources = GetComponents<AudioSource>();
            if (audioSources.Length < 3)
            {
                Debug.LogError("SoundManager requires at least three AudioSources.");
                return;
            }
            _ambienceSource = audioSources[0];
            _musicSource = audioSources[1];
            _globalSfxSource = audioSources[2];
            _UISfxSource = audioSources[3];
        }        
    }

    public static void PlayRandomSFX(WorldSfxType sfxType, AudioSource sfxSource, bool loop = false)
    {
        Instance.EnsureAudioSourcesInitialized();

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
        Instance.EnsureAudioSourcesInitialized();

        AudioClip[] clips = Instance._globalSoundEffects[(int)sfxType].Sounds;

        // Check if index is within bounds
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"SoundManager: No sound clips found for {sfxType}.");
            return;
        }

        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        
        var sfxSource = sfxType is GlobalSfxTypes.MENUCLICK or GlobalSfxTypes.MENUHOVER ? Instance._UISfxSource : Instance._globalSfxSource;
        
        if (randomClip != null && Instance._globalSfxSource != null)
        {
            sfxSource.PlayOneShot(randomClip, Instance._sfxVolume);
        }
        else
        {
            Debug.LogWarning($"SoundManager: AudioSource or clip for {randomClip.name} is not set.");
        }
    }    
    
    public static void PlaySfx(GlobalSfxTypes sfxType, int index)
    {
        Instance.EnsureAudioSourcesInitialized();

        AudioClip[] clips = Instance._globalSoundEffects[(int)sfxType].Sounds;

        // Check if index is within bounds
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"SoundManager: No sound clips found for {sfxType}.");
            return;
        }

        // check if index is within bounds
        if (index < 0 || index >= clips.Length)
        {
            Debug.LogWarning($"SoundManager: Index {index} is out of range.");
            return;
        }
        
        AudioClip clip = clips[index];
        
        var sfxSource = sfxType is GlobalSfxTypes.MENUCLICK or GlobalSfxTypes.MENUHOVER ? Instance._UISfxSource : Instance._globalSfxSource;
        
        if (clip != null && Instance._globalSfxSource != null)
        {
            sfxSource.PlayOneShot(clip, Instance._sfxVolume);
        }
        else
        {
            Debug.LogWarning($"SoundManager: AudioSource or clip for {clip.name} is not set.");
        }
    }

    public static void PlayShootSound(PlayerWeaponTypes weaponType, AudioSource audioSource)
    {
        Instance.EnsureAudioSourcesInitialized();

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
        Instance.EnsureAudioSourcesInitialized();

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
            
            if (!loopMusic)
                Instance.StartCoroutine(Instance.MonitorMusicEnd(musicType));
        }
        else
        {
            Debug.LogWarning($"SoundManager: Music clip for {musicType} is not set.");
        }
    }
    
    private System.Collections.IEnumerator MonitorMusicEnd(MusicType musicType)
    {
        yield return new WaitWhile(() =>Mathf.Round(_musicSource.time) < Math.Round(_musicSource.clip.length));
        OnMusicFinished?.Invoke(musicType);
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

    private void OnEnable()
    {
#if UNITY_EDITOR
        // Editor-only array resizing and naming
        string[] names = Enum.GetNames(typeof(MusicType));
        Array.Resize(ref _musics, names.Length);
        for (int i = 0; i < names.Length; i++)
            _musics[i]._name = names[i];

        names = Enum.GetNames(typeof(AmbienceSoundType));
        Array.Resize(ref _ambienceSounds, names.Length);
        for (int i = 0; i < names.Length; i++)
            _ambienceSounds[i]._name = names[i];

        names = Enum.GetNames(typeof(GlobalSfxTypes));
        Array.Resize(ref _globalSoundEffects, names.Length);
        for (int i = 0; i < names.Length; i++)
            _globalSoundEffects[i]._name = names[i];

        names = Enum.GetNames(typeof(WorldSfxType));
        Array.Resize(ref _worldSoundEffects, names.Length);
        for (int i = 0; i < names.Length; i++)
            _worldSoundEffects[i]._name = names[i];
#endif        

        if (Application.isPlaying)
        {
            GameManager.OnPauseGame += OnPauseGame;
            GameManager.OnResumeGame += OnResumeGame;
        }        
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            GameManager.OnPauseGame -= OnPauseGame;
            GameManager.OnResumeGame -= OnResumeGame;
        }        
    }

    private void OnPauseGame()
    {    
        _gamePaused = true;
        
        Instance._ambienceSource.Pause();
        Instance._musicSource.Pause();
        
        sfxAudioMixer.SetFloat("WorldSFXVolume" , -80f); // Mute SFX
    }

    private void OnResumeGame()
    {        
        _gamePaused = false;
        
        Instance._ambienceSource.UnPause();
        Instance._musicSource.UnPause();
        
        sfxAudioMixer.SetFloat("WorldSFXVolume" , 0f); // Unmute SFX
    }

    private void OnApplicationQuit()
    {
        // Disable the ambience source to stop playing sounds when the application quits
        if (_ambienceSource != null)
        {
            _ambienceSource.Stop();
            _ambienceSource.clip = null; // Clear the clip to avoid memory leaks
        }               
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
