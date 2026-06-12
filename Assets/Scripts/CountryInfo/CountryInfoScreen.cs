using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class CountryInfoScreen : MonoBehaviour
{
    public static CountryInfoScreen instance;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private RectTransform backButton;
    [SerializeField] private RectTransform titleTextRect;
    [SerializeField] private RectTransform continueButton;
    [SerializeField] private CanvasGroup backButtonGroup;
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private CanvasGroup descriptionGroup;
    [SerializeField] private CanvasGroup continueButtonGroup;

    public bool openedFromUnlock;

    Coroutine descriptionRoutine;

    void Awake()
    {
        instance = this;
    }

    // ← NEW! Parameter for detecting unlock animation
    public void ShowDestination(
        DestinationData destination,
        bool isFromUnlock = false)  // ← NEW!
    {
        if (descriptionRoutine != null)
        {
            StopCoroutine(descriptionRoutine);
        }

        titleText.text = destination.destinationName;
        descriptionText.text = destination.description;

        BackgroundManager.Instance.SetCountryInfoScreen(destination.background);

        PlayIntroAnimation(isFromUnlock);  // ← Pass the flag!
    }

    void PlayIntroAnimation(
    bool isFromUnlock
)
    {
        backButton.localScale = Vector3.zero;
        titleTextRect.localScale = Vector3.zero;
        continueButton.localScale = Vector3.zero;

        backButtonGroup.alpha = 0;
        titleGroup.alpha = 0;
        descriptionGroup.alpha = 0;
        continueButtonGroup.alpha = 0;

        descriptionText.maxVisibleCharacters = 0;

        Sequence seq =
            DOTween.Sequence();

        float startDelay =
            isFromUnlock ? 0.8f : 0f;

        seq.AppendInterval(
            startDelay
        );

        // Back Button
        seq.Append(
            backButton.DOScale(
                1f,
                0.3f
            ).SetEase(Ease.OutBack)
        );

        seq.Join(
            backButtonGroup.DOFade(
                1f,
                0.25f
            )
        );

        // Title
        seq.Append(
            titleTextRect.DOScale(
                1f,
                0.35f
            ).SetEase(Ease.OutBack)
        );

        seq.Join(
            titleGroup.DOFade(
                1f,
                0.25f
            )
        );

        // Description
        seq.Append(
            descriptionGroup.DOFade(
                1f,
                0.3f
            )
        );

        seq.AppendCallback(() =>
        {
            descriptionText
                .ForceMeshUpdate();

            int totalChars =
                descriptionText
                    .textInfo
                    .characterCount;

            DOTween.To(
                () => descriptionText.maxVisibleCharacters,
                x => descriptionText.maxVisibleCharacters = x,
                totalChars,
                2.5f
            )
            .SetEase(Ease.Linear);
        });

        seq.AppendInterval(
            2.6f
        );

        // Continue Button
        seq.Append(
            continueButton.DOScale(
                1f,
                0.35f
            ).SetEase(Ease.OutBack)
        );

        seq.Join(
            continueButtonGroup.DOFade(
                1f,
                0.25f
            )
        );
    }

    public void ContinueExploring()
    {
        if (openedFromUnlock)
        {
            LevelManager.instance
                .loadLevelSilently = true;

            LevelManager.instance
                .NextLevel(false);

            openedFromUnlock = false;
            return;
        }

        UIManager.Instance.Show(
            ScreenType.HomeScreen
        );
    }
}