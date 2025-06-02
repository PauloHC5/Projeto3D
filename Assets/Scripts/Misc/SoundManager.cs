using UnityEngine;
using System;

public enum SoundType
{
    SHOOT,
    RELOAD,
    HIT,
    WOODSMAN,
    FOOTSTEP,
    CHEMAGENT,
    DEATH,
    PICKUP,
    SUPERSHOOT,
    CLOSEDOOR,    
    MENUCLICK,
    MENUHOVER,
    ITEMPICKUP,    
}

[RequireComponent(typeof(AudioSource)), RequireComponent(typeof(AudioSource)), RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;

    public static SoundManager instance;

    private AudioSource sfxSource;
    private AudioSource ambienceSource;
    private AudioSource musicSource;


    public AudioSource SfxSource => sfxSource;
    public AudioSource AmbienceSource => ambienceSource;
    public AudioSource MusicSource => musicSource;  

    void Start()
    {
        if (instance == null)
        {
            instance = UnityEngine.Object.FindFirstObjectByType<SoundManager>();
        }

        var audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < 3)
        {
            Debug.LogError("SoundManager requires at least three AudioSources.");
            return;
        }
        sfxSource = audioSources[0];
        ambienceSource = audioSources[1];        
        musicSource = audioSources[2];        
    }

    public static void PlayRandomSound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip ramdomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        if (instance.sfxSource != null && ramdomClip != null)
        {
            instance.sfxSource.PlayOneShot(ramdomClip, volume);
        }
        else
        {
            Debug.LogWarning($"SoundManager: AudioSource or clip for {sound} is not set.");
        }
    }

    public static void PlaySoundInLoop(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        if (instance.ambienceSource != null && randomClip != null)
        {
            instance.ambienceSource.clip = randomClip;
            instance.ambienceSource.volume = volume;
            instance.ambienceSource.loop = true;
            instance.ambienceSource.Play();
        }
        else
        {
            Debug.LogWarning($"SoundManager: Ambience AudioSource or clip for {sound} is not set.");
        }
    }

    public static void StopSoundInLoop(SoundType sound)
    {
        if (instance.ambienceSource != null && instance.ambienceSource.isPlaying)
        {
            instance.ambienceSource.Stop();
            instance.ambienceSource.clip = null; // Clear the clip to avoid memory leaks
        }
    }

    public static void PlayShootSound(WeaponTypes weaponType, float volume = 1)
    {
        instance.sfxSource.PlayOneShot(instance.soundList[(int)SoundType.SHOOT].Sounds[(int)weaponType], volume);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            soundList[i].name = names[i];
        }                
    }
#endif            

    private void OnApplicationQuit()
    {
        // Disable the ambience source to stop playing sounds when the application quits
        if (ambienceSource != null)
        {
            ambienceSource.Stop();
            ambienceSource.clip = null; // Clear the clip to avoid memory leaks
        }

        // Clean up the instance when the application quits
        if (instance != null)
            instance = null;        
    }
}


[Serializable]
public struct SoundList
{
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;    
    public AudioClip[] Sounds => sounds;
}
