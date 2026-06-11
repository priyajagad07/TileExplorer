using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CountryInfoScreen : MonoBehaviour
{
    public static CountryInfoScreen instance;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    public bool openedFromUnlock;
    [SerializeField]
    private RectTransform contentPanel;

    void Awake()
    {
        instance = this;
    }

    public void ShowDestination(
        DestinationData destination)
    {
        titleText.text =
            destination.destinationName;

        descriptionText.text =
            destination.description;

        contentPanel.localScale =
    Vector3.zero;

        contentPanel
            .DOScale(
                1f,
                0.35f
            )
            .SetEase(
                Ease.OutBack
            );

        BackgroundManager.Instance
            .SetCountryInfoScreen(
                destination.background
            );
    }

    public void ContinueExploring()
    {
        UIManager.Instance.Show(
            ScreenType.HomeScreen
        );

        if (openedFromUnlock)
        {
            LevelManager.instance.NextLevel(false);
            openedFromUnlock = false;
        }
    }
}