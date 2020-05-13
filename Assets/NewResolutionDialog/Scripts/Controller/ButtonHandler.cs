using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable 0649

public class ButtonHandler : MonoBehaviour
{
    public void OnPlay()
    {
        // just load the next scene in the "included in build" scenes list
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OnQuit()
    {
        // either quit or leave play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
