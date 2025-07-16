using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : Singleton<MainMenuManager>
{        
    private GameObject _starGameTxt;
    private bool _gameStarted = false;

    private void Awake()
    {        
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
            Cursor.lockState = CursorLockMode.Locked;            
            _gameStarted = true;
            _starGameTxt.SetActive(false); // Hide the "Start Game" text

            FadeManager.FadeOut(HandleFadeOutComplete); // Start fade out effect
        }
    } 

    private void HandleFadeOutComplete()
    {
        SceneManager.LoadScene("NewMap");
    }

}
