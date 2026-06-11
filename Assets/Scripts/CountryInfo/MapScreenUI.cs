using UnityEngine;
using DG.Tweening;

public class MapScreenUI : MonoBehaviour
{
    public static MapScreenUI instance;
    [SerializeField]
    private Transform cardZoomTarget;

    [SerializeField]
    private CanvasGroup mapCanvasGroup;

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

        Vector3 originalPos =
            card.position;

        Vector3 originalScale =
            card.localScale;

        Sequence seq =
            DOTween.Sequence();

        // 1. Tiny unlock shake
        seq.Append(
            card.DOShakeRotation(
                0.2f,
                8f,
                8
            )
        );

        // 2. Reveal destination image
        seq.AppendCallback(() =>
        {
            MapManager.instance
                ?.RefreshMap();
        });

        // 3. Move card to center
        seq.Append(
            card.DOMove(
                cardZoomTarget.position,
                0.45f
            )
            .SetEase(Ease.OutCubic)
        );

        // 4. Fade map
        seq.Join(
            mapCanvasGroup.DOFade(
                0f,
                0.4f
            )
        );

        // 5. Big zoom
        seq.Append(
            card.DOScale(
                2.5f,
                0.35f
            )
            .SetEase(Ease.OutBack)
        );

        // 6. Open destination screen
        seq.AppendCallback(() =>
        {
            DestinationUnlocker.Clear();

            CountryInfoScreen.instance
                .ShowDestination(
                    country.destinations[pending]
                );

            CountryInfoScreen.instance
                .openedFromUnlock = true;

            UIManager.Instance.Show(
                ScreenType.CountryInfoScreen
            );

            // Reset card
            card.position =
                originalPos;

            card.localScale =
                originalScale;

            mapCanvasGroup.alpha = 1f;
        });
    }
}