using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewResolutionDialog.Scripts
{
    public enum ResolutionDialogStyle
    {
        /// <summary>
        ///     Use this as a first scene. Cannot be reopened once closed.
        /// </summary>
        LaunchDialog,

        /// <summary>
        ///     Use this as a popup. Open it by pressing the corresponding key.
        /// </summary>
        PopupDialog
    }

    public class Settings : MonoBehaviour
    {
        public ResolutionDialogStyle dialogStyle = ResolutionDialogStyle.LaunchDialog;
    }
}