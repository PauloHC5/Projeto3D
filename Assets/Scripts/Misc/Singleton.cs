using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;    

    protected static T Instance
    {
        get
        {
            if(_instance == null)
            {
                // Find any existing instances of the singleton in the scene
                var foundInstances = FindObjectsByType<T>(FindObjectsSortMode.None);
                _instance = foundInstances.Length > 0 ? foundInstances[0] : null;
                
                if (_instance == null)
                {
                    Debug.LogError($"Singleton instance of {typeof(T).Name} not found in the scene. Please ensure there is a {typeof(T).Name} object.");
                }

                // If there are multiple instances, destroy them
                if (foundInstances.Length > 1)
                {
                    for (int i = 1; i < foundInstances.Length; i++)
                    {
                        Destroy(foundInstances[i].gameObject);
                    }
                }

                // Make this instance persist across scene loads
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    private void OnApplicationQuit()
    {
        // Clean up the instance when the application quits
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
