using UnityEngine;

#pragma warning disable 0649

namespace NewResolutionDialog.Scripts.Controller
{
    /// <summary>
    ///     Shows the popup at Start if the <see cref="ResolutionDialogStyle" /> is set to
    ///     <see cref="ResolutionDialogStyle.LaunchDialog" />
    /// </summary>
    /// <seealso cref="ResolutionDialogStyle.LaunchDialog" />
    /// <seealso cref="DefaultInputsHandler" />
    public class PopupHandler : MonoBehaviour
    {
        [SerializeField]
        private Settings settings;

        [SerializeField]
        private Canvas dialogCanvas;

        private void Start()
        {
            dialogCanvas.enabled = settings.dialogStyle == ResolutionDialogStyle.LaunchDialog;
        }
    }
}