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

    public bool openedFromUnlock;

    Coroutine descriptionRoutine;

    void Awake()
    {
        instance = this;
    }

    public void ShowDestination(
        DestinationData destination)
    {
        if (descriptionRoutine != null)
        {
            StopCoroutine(descriptionRoutine);
        }

        titleText.text =
    destination.destinationName;

        descriptionText.text =
            destination.description;

        BackgroundManager.Instance
            .SetCountryInfoScreen(
                destination.background
            );

        PlayIntroAnimation();
    }

    void PlayIntroAnimation()
    {
        backButton.DOKill();
        titleTextRect.DOKill();
        continueButton.DOKill();

        backButton.localScale = Vector3.zero;
        titleTextRect.localScale = Vector3.zero;
        continueButton.localScale = Vector3.zero;

        descriptionText.maxVisibleCharacters = 0;

        Sequence seq = DOTween.Sequence();

        // Back button
        seq.Append(
            backButton.DOScale(
                1f,
                0.3f
            ).SetEase(Ease.OutBack)
        );

        // Title
        seq.Append(
            titleTextRect.DOScale(
                1f,
                0.35f
            ).SetEase(Ease.OutBack)
        );

        // Description typing
        seq.AppendCallback(() =>
        {
            descriptionText.ForceMeshUpdate();

            int totalChars =
                descriptionText.textInfo.characterCount;

            DOTween.To(
                () => descriptionText.maxVisibleCharacters,
                x => descriptionText.maxVisibleCharacters = x,
                totalChars,
                2.2f   // slower typing
            )
            .SetEase(Ease.Linear);
        });

        // Wait for typing animation
        seq.AppendInterval(2.3f);

        // Continue button
        seq.Append(
            continueButton.DOScale(
                1f,
                0.35f
            ).SetEase(Ease.OutBack)
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
        }

        UIManager.Instance.Show(
            ScreenType.HomeScreen
        );
    }
}