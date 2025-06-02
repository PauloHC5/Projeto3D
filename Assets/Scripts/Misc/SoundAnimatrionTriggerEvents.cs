using UnityEngine;

public class SoundAnimatrionTriggerEvents : MonoBehaviour
{
    [SerializeField] private SoundType soundType;
    [SerializeField] private float volume = 1f;

    public void PlaySound()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.PlayRandomSound(soundType, volume);
        }
        else
        {
            Debug.LogWarning("SoundManager instance is null. Cannot play sound.");
        }
    }

    public void PlaySoundInLoop()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.PlaySoundInLoop(soundType, volume);
        }
    }
}