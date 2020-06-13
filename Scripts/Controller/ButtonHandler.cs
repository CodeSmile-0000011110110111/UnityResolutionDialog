using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#pragma warning disable 0649

namespace NewResolutionDialog.Scripts.Controller
{
    public class ButtonHandler : MonoBehaviour
    {
        [SerializeField] Settings settings;
        [SerializeField] Button playButton;
        [SerializeField] Button quitButton;
        [SerializeField] Button closeButton;

        private void OnEnable()
        {
            var isLaunchScene = settings.dialogStyle == ResolutionDialogStyle.LaunchDialog;
            playButton.gameObject.SetActive(isLaunchScene);
            quitButton.gameObject.SetActive(isLaunchScene);
            closeButton.gameObject.SetActive(!isLaunchScene);
        }

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
}
