using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _Instance;

    // Static instance of GameManager which allows it to be accessed by any other script
    public static GameManager Instance {
        get
        {
            if(_Instance == null)
            {
                _Instance = Object.FindFirstObjectByType<GameManager>();                
            }

            return _Instance;
        }        
    }

    public PlayerCharacterController Player { get; private set; }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {        
        // Make this instance persistent across scenes
        //DontDestroyOnLoad(gameObject);

        // Find the player character controller in the scene
        Player = Object.FindFirstObjectByType<PlayerCharacterController>();                
    }    

    private void Update()
    {
        // Check if the 'P' key is pressed
        if (Input.GetKeyDown(KeyCode.P))
        {
            ReloadScene();
        }

        // Check if the 'Escape' key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitApplication();
        }
    }

    private void ReloadScene()
    {
        _Instance = null;
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void QuitApplication()
    {
        // Quit the application
        Application.Quit();
    }

    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeInitialize()
    {
        _Instance = null;
        Debug.Log("GameManager has been reset.");
    }
}
