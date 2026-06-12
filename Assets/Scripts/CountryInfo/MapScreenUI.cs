using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Solo.MOST_IN_ONE;

public class MapScreenUI : MonoBehaviour
{
    public static MapScreenUI instance;

    [SerializeField]
    private RectTransform cardZoomTarget;
    [SerializeField]
    private GameObject lockedMessagePopup;

    void Awake()
    {
        instance = this;
    }

    public static class DestinationUnlocker
    {
        private const string PendingKey =
            "PendingDestination";

        public static void SetPending(
            int index
        )
        {
            PlayerPrefs.SetInt(
                PendingKey,
                index
            );
        }

        public static int GetPending()
        {
            return PlayerPrefs.GetInt(
                PendingKey,
                -1
            );
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(
                PendingKey
            );
        }
    }

    public void PlayPendingUnlock()
    {
        int pending =
            DestinationUnlocker.GetPending();

        if (pending < 0)
            return;

        CountryData country =
            BackgroundManager.Instance
                .GetCurrentCountry();

        if (country == null)
            return;

        Transform countryTransform =
            GameObject.Find(
                country.countryName
            )?.transform;

        if (countryTransform == null)
            return;

        Transform card =
            countryTransform.Find(
                "Card" + (pending + 1) + " - Button"
            );

        if (card == null)
            return;

        Transform cityImage =
            card.Find("City - Image");

        if (cityImage == null)
            return;

        Image image =
            cityImage.GetComponent<Image>();

        if (image == null)
            return;

        Vector3 originalPos =
            cityImage.position;

        Vector3 originalScale =
            cityImage.localScale;

        Sequence seq =
            DOTween.Sequence();


        SoundManager.instance.PlayHaptic(
    MOST_HapticFeedback.HapticTypes.Success
);

        // Step 1: Small shake
        seq.Append(
            cityImage.DOShakeRotation(
                0.5f,
                8f,
                10
            )
        );

        // Step 2: Reveal destination image
        seq.AppendCallback(() =>
        {
            image.sprite =
                country.previewCards[pending];
        });

        // Step 3: Small pause so player notices reveal
        seq.AppendInterval(0.3f);

        // Step 4: Move image to center
        seq.Append(
            cityImage.DOMove(
                cardZoomTarget.position,
                0.75f
            )
            .SetEase(
                Ease.OutCubic
            )
        );

        // Step 5: Zoom image
        seq.Append(
            cityImage.DOScale(
                3f,
                0.75f
            )
            .SetEase(
                Ease.OutBack
            )
        );

        // Step 6: Open info screen
        seq.AppendCallback(() =>
        {
            DestinationUnlocker.Clear();

            CountryInfoScreen.instance
                .openedFromUnlock = true;

            UIManager.Instance.Show(
                ScreenType.CountryInfoScreen
            );

            DOVirtual.DelayedCall(
                0.05f,
                () =>
                {
                    CountryInfoScreen.instance
                        .ShowDestination(
                            country.destinations[pending]
                        );
                }
            );

            MapManager.instance
                ?.RefreshMap();

            cityImage.position =
                originalPos;

            cityImage.localScale =
                originalScale;
        });
    }

    public void ShowLockedMessage()
    {
        lockedMessagePopup.SetActive(true);

        lockedMessagePopup.transform.localScale =
            Vector3.zero;

        Sequence seq =
            DOTween.Sequence();

        seq.Append(
            lockedMessagePopup.transform
                .DOScale(1f, 0.25f)
                .SetEase(Ease.OutBack)
        );

        seq.AppendInterval(1.2f);

        seq.Append(
            lockedMessagePopup.transform
                .DOScale(0f, 0.2f)
                .SetEase(Ease.InBack)
        );

        seq.OnComplete(() =>
        {
            lockedMessagePopup.SetActive(false);
        });
    }
}