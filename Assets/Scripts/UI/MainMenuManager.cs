using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject[] buttons = new GameObject[0];


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.PlayMusic(MusicType.MENU, true);
    }
    
    public void StartGame()
    {
        SceneManager.LoadScene("NewMap");
    }

}
