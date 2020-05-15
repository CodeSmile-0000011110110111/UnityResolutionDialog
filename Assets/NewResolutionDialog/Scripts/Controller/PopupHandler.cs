using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class PopupHandler : MonoBehaviour
{
    [SerializeField] Settings settings;
    [SerializeField] Canvas dialogCanvas;

    void Start()
    {
        if (settings.dialogStyle == ResolutionDialogStyle.PopupDialog)
        {
            dialogCanvas.enabled = false;
            StartCoroutine(WaitForActivation());
        }
    }

    IEnumerator WaitForActivation()
    {
        while (true)
        {
            yield return new WaitUntil(() => Input.GetKeyUp(settings.popupKeyCode));

            // toggle canvas
            dialogCanvas.enabled = !dialogCanvas.enabled;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
    }
}
