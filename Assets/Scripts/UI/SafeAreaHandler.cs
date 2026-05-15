using System.Collections;
using UnityEngine;

namespace StarterKit.UI
{
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect safeRect;
        private Vector2 minAnchor;
        private Vector2 maxAnchor;

        private void Awake()
        {
            StartCoroutine(AdjustCanvasDelayed());
        }

        private IEnumerator AdjustCanvasDelayed()
        {
            yield return new WaitForSeconds(0.1f);
            AdjustCanvas();
        }

        [ContextMenu("Adjust Canvas")]
        public void AdjustCanvas()
        {
            ScreenOrientation screenOrientation = Screen.orientation;

            rectTransform = GetComponent<RectTransform>();
            safeRect = Screen.safeArea;

            minAnchor = safeRect.position;
            maxAnchor = minAnchor + safeRect.size;

            if (screenOrientation == ScreenOrientation.LandscapeLeft ||
                screenOrientation == ScreenOrientation.LandscapeRight)
            {
                minAnchor.x /= Screen.width;
                minAnchor.y = 0;

                maxAnchor.x /= Screen.width;
                maxAnchor.y = 1;
            }
            else
            {
                minAnchor.x = 0;
                minAnchor.y /= Screen.height;

                maxAnchor.x = 1;
                maxAnchor.y /= Screen.height;
            }

            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
        }
    }
}