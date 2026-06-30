using DG.Tweening;
using UnityEngine;

public class UIAnimations : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private RectTransform target;

    [Header("Intro Animation")]
    [SerializeField] private bool playIntro = true;

    [Header("Floating")]
    [SerializeField] private bool floating;
    [SerializeField] private float floatAmount = 15f;
    [SerializeField] private float floatDuration = 2f;

    [Header("Breathing")]
    [SerializeField] private bool breathing;
    [SerializeField] private float breatheScale = 1.03f;
    [SerializeField] private float breatheDuration = 2f;

    [Header("Rotation")]
    [SerializeField] private bool rotating;
    [SerializeField] private float rotateAmount = 2f;
    [SerializeField] private float rotateDuration = 1.5f;

    [SerializeField]
    private float introDelay = 0f;

    private Vector2 startPos;

    private void Start()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        startPos = target.anchoredPosition;

        if (playIntro)
        {
            target.localScale = Vector3.zero;

            target
                .DOScale(1f, 0.6f)
                .SetDelay(introDelay)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(StartAnimations);

        }
        else
        {
            StartAnimations();
        }
    }

    void StartAnimations()
    {
        if (floating)
        {
            target
                .DOAnchorPosY(startPos.y + floatAmount, floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        if (breathing)
        {
            target.localScale = Vector3.one;
            target
                .DOScale(breatheScale, breatheDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        if (rotating)
        {
            target
                .DORotate(
                    new Vector3(0, 0, rotateAmount),
                    rotateDuration
                )
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }
    }
}