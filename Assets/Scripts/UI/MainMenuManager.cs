using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : Singleton<MainMenuManager>
{
    [SerializeField] private FadeBehaviour _fade;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SoundManager.PlayMusic(MusicType.AMBIENCE, true);
            _fade.FadeOut();
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
        // e.g., load the next scene
        StartGame();
    }

}
