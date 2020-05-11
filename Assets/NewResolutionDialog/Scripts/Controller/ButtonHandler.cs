using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable 0649

public class ButtonHandler : MonoBehaviour
{
    [SerializeField] Settings settings;

    public void OnPlay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
