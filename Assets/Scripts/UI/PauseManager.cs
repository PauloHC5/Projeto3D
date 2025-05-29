using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager _Instance { get; set; }

    public bool IsPaused { get; private set; } = false;

    [Header("Pause Menu Properties")]
    [SerializeField] private GameObject pauseMenuUI;

    public static PauseManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = Object.FindFirstObjectByType<PauseManager>();
            }

            return _Instance;
        }
    }

    private void Start()
    {
        if (pauseMenuUI == null)
        {
            Debug.LogError("Pause Menu UI is not assigned in the inspector.");
            return;
        }
        pauseMenuUI.SetActive(false);
        _Instance = this; // Ensure the instance is set
    }

    private void Update()
    {
        if(Instance == null)
            Debug.Log("PauseManager instance is null. Please ensure it is assigned in the scene.");
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);

        IsPaused = true;
        Time.timeScale = 0f;

        PlayerCharacterController.PlayerControls.UI.Enable();
        PlayerCharacterController.PlayerControls.Player.Disable();
        Cursor.lockState = CursorLockMode.None;

        // unfocus the game window to prevent input
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);

        IsPaused = false;
        Time.timeScale = 1f;

        PlayerCharacterController.PlayerControls.UI.Disable();
        PlayerCharacterController.PlayerControls.Player.Enable();
        Cursor.lockState = CursorLockMode.Locked;

        // refocus the game window to allow input
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeInitialize()
    {
        _Instance = null;
        Debug.Log("Pause Manager has been reset.");
    }
}
