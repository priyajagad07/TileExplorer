using DG.Tweening;
using UnityEngine;

public class UIScreenAnimation : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public AnimationType animationType;

    public Transform target;

    public float duration = 0.35f;

    private Vector3 originalPosition;

    void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponent<CanvasGroup>();
        }

        if (target == null)
        {
            target = transform;
        }

        originalPosition = target.localPosition;
    }

    public void Show()
    {
        DOTween.Kill(target);
        DOTween.Kill(canvasGroup);

        canvasGroup.alpha = 0;

        switch (animationType)
        {
            // GameStartScreen
            case AnimationType.ScaleFade:

                target.localScale =
                    Vector3.one * 0.85f;

                canvasGroup
                    .DOFade(1, duration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);

                target
                    .DOScale(1, duration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);

                break;

            // HomeScreen
            case AnimationType.SlideUpFade:

                target.localPosition =
                    originalPosition +
                    new Vector3(0, -300, 0);

                canvasGroup
                    .DOFade(1, duration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);

                target
                    .DOLocalMove(originalPosition, duration)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true);

                break;

            // MapScreen
            case AnimationType.SlideLeft:

                target.localPosition =
                    originalPosition +
                    new Vector3(600, 0, 0);

                canvasGroup
                    .DOFade(1, duration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);

                target
                    .DOLocalMove(originalPosition, duration)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true);

                break;

            // Gameplay
            case AnimationType.Fade:

                canvasGroup
                    .DOFade(1, duration)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true);

                break;

            // Popups
            case AnimationType.Popup:

                target.localScale =
                    Vector3.one * 0.5f;

                canvasGroup
                    .DOFade(1, duration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);

                target
                    .DOScale(1, duration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);

                break;

            default:

                canvasGroup.alpha = 1;

                break;
        }
    }

    public void Hide(System.Action onComplete = null)
    {
        DOTween.Kill(target);
        DOTween.Kill(canvasGroup);

        switch (animationType)
        {
            case AnimationType.Popup:

                canvasGroup
                    .DOFade(0, duration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);

                target
                    .DOScale(0.5f, duration)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        onComplete?.Invoke();
                    });

                break;

            default:

                canvasGroup.alpha = 0;
                onComplete?.Invoke();

                break;
        }
    }
}