using UnityEngine;
using UnityEngine.EventSystems;

namespace NewResolutionDialog.Scripts.Controller
{
    [ExecuteInEditMode]
    public class CreateEventSystemIfNoneExists : MonoBehaviour
    {
        private void Awake()
        {
            // in case prefab gets dropped onto a scene, check if an event system exists and if not, create one
            // this is to avoid any issues due to a scene not having an event system because adding prefabs to a scene
            // apparently doesn't add the event system even if the prefab contains a canvas with UI controls
            if (FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Debug.Log("NewResolutionDialog added 'EventSystem' to Scene.");
            }
        }
    }
}
