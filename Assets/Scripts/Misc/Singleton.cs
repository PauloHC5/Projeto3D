using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;

    [SerializeField] protected bool _shouldPersistBetweenScenes = true;

    protected static T Instance
    {
        get
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning($"{typeof(T).Name} singleton accessed in edit mode. Returning null.");
                return null;
            }

            if (_instance == null)
            {
                var foundInstances = FindObjectsByType<T>(FindObjectsSortMode.None);
                _instance = foundInstances.Length > 0 ? foundInstances[0] : null;

                if (_instance == null)
                {
                    Debug.LogError($"Singleton instance of {typeof(T).Name} not found in the scene. Please ensure there is a {typeof(T).Name} object.");
                }

                if (foundInstances.Length > 1)
                {
                    for (int i = 1; i < foundInstances.Length; i++)
                    {
                        Destroy(foundInstances[i].gameObject);
                    }
                }

                // Only persist if ShouldPersistBetweenScenes is true
                if (Application.isPlaying && _instance != null && _instance._shouldPersistBetweenScenes)
                {
                    DontDestroyOnLoad(_instance.gameObject);
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (!Application.isPlaying)
            return;

        if (_instance == null)
        {
            _instance = (T)this;            
            if (_shouldPersistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        OnAwake();
    }

    protected virtual void OnAwake()
    {
        // To be override in the child classes
    }

    private void OnApplicationQuit()
    {
        if (_instance != null)
            _instance = null;
    }

    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeInitialize()
    {
        _instance = null;
        Debug.Log($"{typeof(T).Name} has been reset. A new instance will be created on the next access.");
    }
}
