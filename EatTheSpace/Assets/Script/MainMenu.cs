using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Loads the Gym scene.
    /// </summary>
    public void GoToGymScene()
    {
        SceneManager.LoadScene("Gym");
    }

    /// <summary>
    /// Loads the Main scene.
    /// </summary>
    public void GoToMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    /// <summary>
    /// Quits the application or stops play mode in the editor.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
