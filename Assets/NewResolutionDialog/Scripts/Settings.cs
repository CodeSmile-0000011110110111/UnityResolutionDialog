using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResolutionDialogStyle
{
    LaunchDialog,
    PopupDialog,
}

public class Settings : MonoBehaviour
{
    public ResolutionDialogStyle dialogStyle = ResolutionDialogStyle.LaunchDialog;
    public KeyCode popupKeyCode = KeyCode.Escape;
}
