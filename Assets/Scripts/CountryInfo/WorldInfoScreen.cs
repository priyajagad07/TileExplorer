using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class WorldInfoScreen : MonoBehaviour
{
    public static WorldInfoScreen instance;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private RectTransform titleTextRect;
    [SerializeField] private RectTransform continueButton;
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private CanvasGroup descriptionGroup;
    [SerializeField] private CanvasGroup continueButtonGroup;

    public bool openedFromUnlock;
    public bool openedFromMap;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDestination(DestinationData destination, bool isFromUnlock = false)
    {
        titleText.text = destination.destinationName;
        descriptionText.text = destination.description;

        BackgroundManager.Instance.SetWorldInfoScreen(destination.background);

        PlayIntroAnimation(isFromUnlock);
    }

    void PlayIntroAnimation(bool isFromUnlock)
    {
        DOTween.Kill("WorldInfoSeq");
        DOTween.Kill(descriptionText);
        titleTextRect.DOKill();
        continueButton.DOKill();
        titleGroup.DOKill();
        descriptionGroup.DOKill();
        continueButtonGroup.DOKill();
        titleTextRect.localScale = Vector3.zero;
        continueButton.localScale = Vector3.zero;
        titleGroup.alpha = 0;
        descriptionGroup.alpha = 0;
        continueButtonGroup.alpha = 0;

        descriptionText.maxVisibleCharacters = 0;

        Sequence seq = DOTween.Sequence().SetId("WorldInfoSeq");

        float startDelay = isFromUnlock ? 0.3f : 0f;

        seq.AppendInterval(startDelay);

        if (isFromUnlock)
        {
            DOVirtual.DelayedCall(0.25f, () =>
            {
                MapScreenUI.instance.HideUnlockTransition();
            });
        }

        // Title
        seq.Append(titleTextRect.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        seq.Join(titleGroup.DOFade(1f, 0.25f));

        // Description
        seq.Append(descriptionGroup.DOFade(1f, 0.3f));

        seq.AppendCallback(() =>
        {
            descriptionText.ForceMeshUpdate();

            int totalChars = descriptionText.textInfo.characterCount;

            DOTween.To(
                () => descriptionText.maxVisibleCharacters,
                x => descriptionText.maxVisibleCharacters = x,
                totalChars,
                2.5f
            )
            .SetTarget(descriptionText)
            .SetEase(Ease.Linear);
        });

        seq.AppendInterval(2.6f);

        // Continue Button
        seq.Append(continueButton.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        seq.Join(continueButtonGroup.DOFade(1f, 0.25f));
    }

    // public void ContinueExploring()
    // {
    //     openedFromUnlock = false;

    //     if (GameManager.instance != null && GameManager.instance.returnToHomeAfterMap)
    //     {
    //         GameManager.instance.returnToHomeAfterMap = false;
    //         UIManager.Instance.Show(ScreenType.HomeScreen);
    //     }
    //     else
    //     {
    //         UIManager.Instance.Show(ScreenType.GamePlay);

    //         if (BoardSpawner.instance != null)
    //         {
    //             BoardSpawner.instance.PlaySpawnAnimation();
    //         }
    //     }
    // }

    public void ContinueExploring()
    {
        // Opened manually from Map
        if (openedFromMap)
        {
            openedFromMap = false;
            UIManager.Instance.Show(ScreenType.MapScreen);
            return;
        }

        // Opened after unlocking
        if (openedFromUnlock)
        {
            openedFromUnlock = false;

            UIManager.Instance.Show(ScreenType.GamePlay);

            if (BoardSpawner.instance != null)
                BoardSpawner.instance.PlaySpawnAnimation();

            return;
        }

        // Returned to Home after level complete
        if (GameManager.instance != null && GameManager.instance.returnToHomeAfterMap)
        {
            GameManager.instance.returnToHomeAfterMap = false;
            UIManager.Instance.Show(ScreenType.HomeScreen);
            return;
        }

        UIManager.Instance.Show(ScreenType.GamePlay);

        if (BoardSpawner.instance != null)
            BoardSpawner.instance.PlaySpawnAnimation();
    }
}