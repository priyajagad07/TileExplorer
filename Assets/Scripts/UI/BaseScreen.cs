using UnityEngine;

public class BaseScreen : MonoBehaviour
{
    private Canvas canvas;
    public AnimationType animationType;
    public float duration = 0.4f;
    public Transform target;

    private UIScreenAnimation screenAnimation;

    void Awake()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();

        screenAnimation = GetComponent<UIScreenAnimation>();

        if (screenAnimation == null)
        {
            screenAnimation = gameObject.AddComponent<UIScreenAnimation>();
        }

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        screenAnimation.canvasGroup = canvasGroup;

        if (target == null)
        {
            target = transform;
        }

        screenAnimation.animationType = animationType;
        screenAnimation.duration = duration;
        screenAnimation.target = target;
    }

    public void Show()
    {
        canvas.enabled = true;

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        screenAnimation.Show();
    }

    public void Hide()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        screenAnimation.Hide();
        canvas.enabled = false;
    }
}