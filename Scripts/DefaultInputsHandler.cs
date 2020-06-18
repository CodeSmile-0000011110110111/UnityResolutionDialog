using System.Collections;
using NewResolutionDialog.Scripts.Controller;
using UnityEngine;

#pragma warning disable 0649

namespace NewResolutionDialog.Scripts
{
    /// <summary>
    ///     <para>
    ///         Default input handler that provides basic support for the legacy <see cref="Input" /> system.
    ///     </para>
    ///     <para>
    ///         This is used only if the <see cref="ResolutionDialogStyle" /> is set to
    ///         <see cref="ResolutionDialogStyle.PopupDialog" />.
    ///     </para>
    ///     <para>
    ///         When the <see cref="popupKeyCode" /> is pressed, the popup will be shown/hidden.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     You can create your own Inputs Handler, notably to add support for the new InputSystem.
    ///     In that case, remove this script from the Resolution Dialog prefab, and add your new script.
    /// </remarks>
    /// <seealso cref="PopupHandler" />
    public class DefaultInputsHandler : MonoBehaviour
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        [SerializeField]
        private Settings settings;

        [SerializeField]
        private Canvas dialogCanvas;

        [SerializeField]
        private KeyCode popupKeyCode = KeyCode.Escape;

        private void Awake()
        {
            if (settings == null) Debug.LogError($"Serialized Field {nameof(settings)} is missing!");
        }

        private void Start()
        {
            if (settings.dialogStyle == ResolutionDialogStyle.PopupDialog)
                StartCoroutine(WaitForActivation());
        }

        private IEnumerator WaitForActivation()
        {
            while (true)
            {
                yield return new WaitUntil(() => Input.GetKeyUp(popupKeyCode));

                ToggleCanvas();

                // wait twice (into next frame) to prevent the hotkey from being recognized again in the same frame
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
            }
        }

        private void ToggleCanvas()
        {
            dialogCanvas.enabled = !dialogCanvas.enabled;
        }

#elif ENABLE_INPUT_SYSTEM
    private void Awake()
    {
        Debug.LogError(
            "The new InputSystem is not supported out of the box. " +
            "If you want to use the popup mode, you must create your own InputsHandler and remove this one from the prefab. " +
            $"Otherwise, just remove this {nameof(DefaultInputsHandler)} component for the prefab.");
    }
#endif
    }
}