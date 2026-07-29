using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScrollReset : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    public void ResetToTop()
    {
        DOVirtual.DelayedCall(0.05f, () =>
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
        });
    }
}