using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting; // <-- Add this for Action

public class PauseManager : MonoBehaviour
{
    private static PauseManager _instance;
    public static PauseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find all PauseManager objects in the scene
                var foundInstances = FindObjectsByType<PauseManager>(FindObjectsSortMode.None);
                _instance = foundInstances.Length > 0 ? foundInstances[0] : null;

                if (_instance == null)
                {
                    Debug.LogError("    ");
                }

                // If there are multiple PauseManager instances, destroy them
                if (foundInstances.Length > 1)
                {
                    for (int i = 1; i < foundInstances.Length; i++)
                    {
                        Destroy(foundInstances[i].gameObject);
                    }
                }
            }

            return _instance;
        }
    }    

    [Header("Pause Menu Properties")]
    [SerializeField] private GameObject _canvasPauseMenu;
    [SerializeField] private GameObject[] buttons = new GameObject[9];    

    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    public Slider MouseSensitivitySlider
    {
        get { return mouseSensitivitySlider; }        
    }            

    private void Start()
    {
        if (_canvasPauseMenu == null)
        {
            Debug.LogError("Pause Menu UI is not assigned in the inspector.");
            return;
        }
        _canvasPauseMenu.SetActive(false);        
    }

    private void Update()
    {
        if(Instance == null)
            Debug.Log("PauseManager instance is null. Please ensure it is assigned in the scene.");

        MouseOverResumeButton();

        SoundManager.MusicVolume = musicSlider.value;
        SoundManager.SfxVolume = sfxSlider.value;        
    }

    private void PauseGame()
    {        
        _canvasPauseMenu.SetActive(true);                
        
        Cursor.lockState = CursorLockMode.None;                

        // unfocus the game window to prevent input
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void ResumeGame()
    {        
        _canvasPauseMenu.SetActive(false);                

        PlayerCharacterController.PlayerControls.UI.Disable();
        PlayerCharacterController.PlayerControls.Player.Enable();
        Cursor.lockState = CursorLockMode.Locked;                  

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
            _canvasPauseMenu.GetComponent<UnityEngine.Canvas>().worldCamera
        );
    }

    private void OnEnable()
    {
        // Subscribe to the pause and resume events
        GameManager.OnPauseGame += PauseGame;
        GameManager.OnResumeGame += ResumeGame;
    }

    private void OnDisable()
    {
        // Unsubscribe from the pause and resume events
        GameManager.OnPauseGame -= PauseGame;
        GameManager.OnResumeGame -= ResumeGame;
    }

    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeInitialize()
    {
        _instance = null;
        Debug.Log("Pause Manager has been reset.");
    }
}
