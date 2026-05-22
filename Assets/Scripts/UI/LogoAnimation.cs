using DG.Tweening;
using UnityEngine;

public class LogoAnimation : MonoBehaviour
{
    [SerializeField]
    private RectTransform logo;

    void Start()
    {
        PlayIntro();
    }

    void PlayIntro()
    {
        logo.localScale = Vector3.zero;

        logo
            .DOScale(1f, 0.6f)
            .SetEase(Ease.OutBack)
            .OnComplete(StartIdleAnimation);
    }

    void StartIdleAnimation()
    {
        // Floating
        logo
            .DOAnchorPosY(
                logo.anchoredPosition.y + 15f,
                2f
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Breathing scale
        logo
            .DOScale(1.03f, 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}