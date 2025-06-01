using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager _Instance { get; set; }

    public bool IsPaused { get; private set; } = false;

    [Header("Pause Menu Properties")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject[] buttons = new GameObject[9];

    [SerializeField] private Slider mouseSensitivitySlider;

    public Slider MouseSensitivitySlider
    {
        get { return mouseSensitivitySlider; }        
    }

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

        MouseOverResumeButton();
        
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);

        IsPaused = true;
        Time.timeScale = 0f;

        PlayerCharacterController.PlayerControls.UI.Enable();
        PlayerCharacterController.PlayerControls.Player.Disable();
        Cursor.lockState = CursorLockMode.None;
        GameManager.Instance.Hud.gameObject.SetActive(false);
        SoundManager.instance.SfxSource.Pause();
        SoundManager.instance.AmbienceSource.Pause();
        SoundManager.instance.MusicSource.Pause();

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
        GameManager.Instance.Hud.gameObject.SetActive(true);
        SoundManager.instance.SfxSource.UnPause();
        SoundManager.instance.AmbienceSource.UnPause();
        SoundManager.instance.MusicSource.UnPause();

        // refocus the game window to allow input
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void QuitGame()
    {
        // Save any necessary game state here before quitting
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    private void MouseOverResumeButton()
    {
        // Check if mouse is over an button
        for (int i = 0; i < buttons.Length; i++)
        {
            if (IsMouseOverResumeButton(buttons[i]))
            {
                // If the mouse is over the resume button, change its color
                // Get the Image component of the button and change its color
                var buttonImages = buttons[i].GetComponentsInChildren<UnityEngine.UI.Image>();
                foreach (var image in buttonImages)
                {                    
                    image.color = Color.yellow; // Change to your desired color
                }

                // Set its scale to indicate it's hovered
                buttons[i].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); // Slightly increase size
            }
            else
            {
                // If the mouse is not over the resume button, reset its color
                // Get the Image component of the button and reset its color
                var buttonImages = buttons[i].GetComponentsInChildren<UnityEngine.UI.Image>();
                foreach (var image in buttonImages)
                {
                    if(image) image.color = Color.white; // Change to your desired color
                }

                // Reset its scale to normal
                buttons[i].transform.localScale = new Vector3(1f, 1f, 1f); // Reset to normal size
            }
        }
    }

    private bool IsMouseOverResumeButton(GameObject buttom)
    {
        if (buttom == null)
            return false;

        // Obtém o RectTransform do botão
        RectTransform rectTransform = buttom.GetComponent<RectTransform>();
        if (rectTransform == null)
            return false;

        // Pega a posição do mouse na tela
        Vector2 mousePosition = Input.mousePosition;        

        // Verifica se o mouse está sobre o botão
        return RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            mousePosition,            
            GetComponent<UnityEngine.Canvas>().worldCamera
        );
    }

    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeInitialize()
    {
        _Instance = null;
        Debug.Log("Pause Manager has been reset.");
    }
}
