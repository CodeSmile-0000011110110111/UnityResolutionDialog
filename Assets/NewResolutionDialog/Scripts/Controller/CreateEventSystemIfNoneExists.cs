using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class CreateEventSystemIfNoneExists : MonoBehaviour
{
    private void Awake()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.Log("NewResolutionDialog added 'EventSystem' to Scene.");
        }
    }
}
