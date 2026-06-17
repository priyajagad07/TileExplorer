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
    
    void Awake()
    {
        instance = this;
    }

    public void ShowDestination(DestinationData destination, bool isFromUnlock = false)
    {
        titleText.text = destination.destinationName;
        descriptionText.text = destination.description;

        BackgroundManager.Instance.SetCountryInfoScreen(destination.background);

        PlayIntroAnimation(isFromUnlock);
    }

    void PlayIntroAnimation(bool isFromUnlock)
    {
        // ---> FIX: Kill all existing tweens on these objects so they don't overlap!
        DOTween.Kill("CountryInfoSeq");
        DOTween.Kill(descriptionText);
        backButton.DOKill();
        titleTextRect.DOKill();
        continueButton.DOKill();
        backButtonGroup.DOKill();
        titleGroup.DOKill();
        descriptionGroup.DOKill();
        continueButtonGroup.DOKill();

        backButton.gameObject.SetActive(!isFromUnlock);
        
        backButton.localScale = Vector3.zero;
        titleTextRect.localScale = Vector3.zero;
        continueButton.localScale = Vector3.zero;

        backButtonGroup.alpha = 0;
        titleGroup.alpha = 0;
        descriptionGroup.alpha = 0;
        continueButtonGroup.alpha = 0;

        descriptionText.maxVisibleCharacters = 0;

        // ---> FIX: Give this sequence an ID so we can kill it easily next time
        Sequence seq = DOTween.Sequence().SetId("CountryInfoSeq");

        float startDelay = isFromUnlock ? 0.3f : 0f;

        seq.AppendInterval(startDelay);

        if (isFromUnlock)
        {
            DOVirtual.DelayedCall(0.25f, () =>
            {
                MapScreenUI.instance.HideUnlockTransition();
            });
        }

        // Back Button
        seq.Append(backButton.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        seq.Join(backButtonGroup.DOFade(1f, 0.25f));

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

    public void ContinueExploring()
    {
        openedFromUnlock = false;
        UIManager.Instance.Show(ScreenType.GamePlay);
    }
}