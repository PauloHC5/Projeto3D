using UnityEngine;

public class SFXAnimatrionTriggerEvent : MonoBehaviour
{
    [SerializeField] private WorldSfxType sfxType;
    [SerializeField] private bool looped = false;
    [SerializeField] private AudioSource sfxSource;    

    public void PlaySFX()
    {
        if(sfxSource == null)
        {
            Debug.LogError("SFX Source is not assigned in SFXAnimatrionTriggerEvent of the object: " + gameObject.name);
            return;
        }

        SoundManager.PlayRandomSFX(sfxType, sfxSource, looped);
    }    
}