using UnityEngine;

public class EndGameManager : MonoBehaviour
{
    public void RestartGame()
    {
        GameManager.ReloadScene();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        // If running in the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
