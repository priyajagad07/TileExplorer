using UnityEngine;
using DG.Tweening;

public class BaseScreen : MonoBehaviour
{
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public AnimationType animationType;
    public float duration = 0.4f;

    [Tooltip(
        "For bottom-tab screens, assign the TransitionRoot child."
    )]
    public Transform target;

    private UIScreenAnimation screenAnimation;
    private Vector2 restingAnchoredPosition;

    private bool isModalBlocked;
    private bool savedInteractable;
    private bool savedBlocksRaycasts;

    public bool IsModalBlocked => isModalBlocked;

    public Canvas ScreenCanvas => canvas;

    public RectTransform TransitionRect =>
        target as RectTransform;

    public Vector2 RestingAnchoredPosition =>
        restingAnchoredPosition;

    void Awake()
    {
        canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError(
                $"Canvas is missing on {gameObject.name}."
            );
        }

        screenAnimation =
            GetComponent<UIScreenAnimation>();

        if (screenAnimation == null)
        {
            screenAnimation =
                gameObject.AddComponent<UIScreenAnimation>();
        }

        canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

        if (target == null)
        {
            target = transform;
        }

        RectTransform targetRect =
            target as RectTransform;

        if (targetRect != null)
        {
            restingAnchoredPosition =
                targetRect.anchoredPosition;
        }

        screenAnimation.canvasGroup =
            canvasGroup;

        screenAnimation.animationType =
            animationType;

        screenAnimation.duration =
            duration;

        screenAnimation.target =
            target;
    }

    /// <summary>
    /// While a popup is open, block input on the screen underneath.
    /// </summary>
    public void SetModalBlocked(bool blocked)
    {
        if (canvasGroup == null)
            return;

        if (blocked)
        {
            if (isModalBlocked)
                return;

            savedInteractable = canvasGroup.interactable;
            savedBlocksRaycasts = canvasGroup.blocksRaycasts;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            isModalBlocked = true;
            return;
        }

        if (!isModalBlocked)
            return;

        canvasGroup.interactable = savedInteractable;
        canvasGroup.blocksRaycasts = savedBlocksRaycasts;
        isModalBlocked = false;
    }

    public void Show()
    {
        Debug.Log(
            "SHOW SCREEN: " +
            gameObject.name
        );

        canvas.enabled = true;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        PlayerNameUI.RefreshAll();

        screenAnimation.Show();

        if (gameObject.name == "GamePlay")
        {
            Debug.Log(
                "GAMEPLAY SHOW CALLED"
            );

            DOVirtual.DelayedCall(
                0.35f,
                () =>
                {
                    Debug.Log(
                        "PLAYING SPAWN"
                    );

                    if (BoardSpawner.instance != null)
                    {
                        BoardSpawner.instance
                            .PlaySpawnAnimation();
                    }
                }
            );
        }
    }

    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        screenAnimation.Hide(() =>
        {
            canvas.enabled = false;

            if (UIManager.Instance != null)
            {
                UIManager.Instance
                    .NotifyScreenFinishedHiding(this);
            }
        });
    }

    public void PrepareForTabTransition()
    {
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }

        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponent<CanvasGroup>();
        }

        canvas.enabled = true;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void CompleteTabShow()
    {
        canvas.enabled = true;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        RectTransform rect =
            TransitionRect;

        if (rect != null)
        {
            rect.anchoredPosition =
                restingAnchoredPosition;
        }

        PlayerNameUI.RefreshAll();
        AvatarUI.RefreshAll();
    }

    public void CompleteTabHide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        RectTransform rect =
            TransitionRect;

        if (rect != null)
        {
            rect.anchoredPosition =
                restingAnchoredPosition;
        }

        canvas.enabled = false;
    }
}