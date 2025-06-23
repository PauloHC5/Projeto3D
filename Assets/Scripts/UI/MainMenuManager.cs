using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : Singleton<MainMenuManager>
{    
    private FadeBehaviour _fade;
    private GameObject _starGameTxt;
    private bool _gameStarted = false;

    private void Awake()
    {
        _fade = GetComponentInChildren<FadeBehaviour>(true);        
        _starGameTxt = GetComponentInChildren<TextMeshProUGUI>(true).gameObject;
    }

    void Start()
    {
        SoundManager.PlayMusic(MusicType.MENU, true);
    }

    private void Update()
    {
        // Check if the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If the Escape key is pressed, quit the application
            Application.Quit();
        }

        // Check if the "Start Game" button is pressed
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) && !_gameStarted)
        {
            SoundManager.PlayMusic(MusicType.AMBIENCE, true);
            _fade.FadeOut();
            Cursor.lockState = CursorLockMode.Locked;            
            _gameStarted = true;
            _starGameTxt.SetActive(false); // Hide the "Start Game" text
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("NewMap");        
    }

    private void OnEnable()
    {
        _fade.OnFadeOutComplete += HandleFadeOutComplete;
    }

    private void OnDisable()
    {
        _fade.OnFadeOutComplete -= HandleFadeOutComplete;
    }

    private void HandleFadeOutComplete()
    {        
        StartGame();
    }

}
