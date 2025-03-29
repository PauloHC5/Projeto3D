using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Static instance of GameManager which allows it to be accessed by any other script
    public static GameManager Instance { get; private set; }

    public static PlayerCharacterController Player { get; private set; }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Check if instance already exists and if so, destroy this instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // Set the instance to this instance
            Instance = this;
            // Make this instance persistent across scenes
            DontDestroyOnLoad(gameObject);
        }

        // Find the player character controller in the scene
        Player = Object.FindFirstObjectByType<PlayerCharacterController>();
    }

    private void Start()
    {
        Debug.Log("GameManager is ready!");        
    }

}
