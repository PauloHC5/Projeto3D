using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PauseManager : Singleton<PauseManager>
{       
    [Header("Pause Menu Properties")]
    [SerializeField] private GameObject canvasPauseMenu;
    [SerializeField] private RectTransform pauseOverlay;
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private GameObject[] buttons = new GameObject[9];    

    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    
    private Vector3 _pauseOverlayOriginalScale;

    public static Slider MouseSensitivitySlider
    {
        get { return Instance.mouseSensitivitySlider; }        
    }

    private void Awake()
    {
        OnAwake();
        
        _pauseOverlayOriginalScale = pauseOverlay.localScale;
    }

    private void Start()
    {
        if (canvasPauseMenu == null)
        {
            Debug.LogError("Pause Menu UI is not assigned in the inspector.");
            return;
        }
        canvasPauseMenu.SetActive(false);        
    }

    private void Update()
    {
        if(Instance == null)
            Debug.Log("PauseManager instance is null. Please ensure it is assigned in the scene.");

        SoundManager.MusicVolume = musicSlider.value;
        SoundManager.SfxVolume = sfxSlider.value;        
    }

    private void OnPauseGame()
    {        
        canvasPauseMenu.SetActive(true);                
        
        Cursor.lockState = CursorLockMode.None;                

        // unfocus the game window to prevent input
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
        
        // Start the scaling coroutine
        StartCoroutine(ScalePauseOverlay());
        
        SoundManager.PlayRandomSFX(GlobalSfxTypes.MENUHOVER);
    }

    private void OnResumeGame()
    {        
        canvasPauseMenu.SetActive(false);                

        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.GAMEPLAY);
        Cursor.lockState = CursorLockMode.Locked;                  

        // refocus the game window to allow input
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void ResumeGame()
    {  
        GameManager.ResumeGame();
    }

    public void QuitGame()
    {
        GameManager.QuitGame();
    }
    
    private IEnumerator ScalePauseOverlay()
    {
        pauseOverlay.localScale = pauseOverlay.localScale / 2f; // Start at half the original size
        
        Vector3 targetScale = _pauseOverlayOriginalScale;
        Vector3 initialScale = pauseOverlay.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < scaleDuration)
        {
            pauseOverlay.localScale = Vector3.Lerp(initialScale, targetScale, (elapsedTime / scaleDuration));
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time to ignore time scale changes
            yield return null;
        }
    }

    private void OnEnable()
    {
        // Subscribe to the pause and resume events
        GameManager.OnPauseGame += OnPauseGame;
        GameManager.OnResumeGame += OnResumeGame;
    }

    private void OnDisable()
    {
        // Unsubscribe from the pause and resume events
        GameManager.OnPauseGame -= OnPauseGame;
        GameManager.OnResumeGame -= OnResumeGame;
    }
}
