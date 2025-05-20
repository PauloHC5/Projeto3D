using System;
using UnityEngine;

public class CorngunAnimationTriggerEvents : MonoBehaviour
{
    public static event Action onReloadCorngun;

    public void ReloadCorngun()
    {
        onReloadCorngun?.Invoke();
    }
}
