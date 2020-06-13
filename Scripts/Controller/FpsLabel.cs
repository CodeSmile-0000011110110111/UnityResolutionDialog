using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace NewResolutionDialog.Scripts.Controller
{
    public class FpsLabel : MonoBehaviour
    {
        [SerializeField] Text fpsLabel;
        [SerializeField] float updateRateSeconds = 0.5f;

        int frameCount = 0;
        float deltaTime = 0f;
        float fps = 0f;

        void Update()
        {
            frameCount++;
            deltaTime += Time.unscaledDeltaTime;
            if (deltaTime > updateRateSeconds)
            {
                fps = frameCount / deltaTime;
                frameCount = 0;
                deltaTime -= updateRateSeconds;
            }

            fpsLabel.text = string.Format("{0} fps", (int)(fps + 0.5f));
        }
    }
}
