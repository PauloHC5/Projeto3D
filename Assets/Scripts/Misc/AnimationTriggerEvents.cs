using System;
using UnityEngine;

public class AnimationTriggerEvents : MonoBehaviour
{
    public static event Action onDropShotgun;   
    public static event Action onReTrieveNewShotguns;
    public static event Action onFinishReload;

    public void DropShotgun()
    {
        onDropShotgun?.Invoke();
    }

    public void RetrieveNewShotguns()
    {
        onReTrieveNewShotguns?.Invoke();
    }

    public void FinishReload()
    {
        onFinishReload?.Invoke();
    }
}
