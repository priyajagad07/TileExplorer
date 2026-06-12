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

        public static void SetPending(int index)
        {
            PlayerPrefs.SetInt(PendingKey, index);
        }

        public static int GetPending()
        {
            return PlayerPrefs.GetInt(PendingKey, -1);
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(PendingKey);
        }
    }

    public void PlayPendingUnlock()
    {
        int pending = DestinationUnlocker.GetPending();

        if (pending < 0)
            return;

        CountryData country =
     CountryManager.Instance.GetNextCountry();

        Debug.Log(
       "Unlocking Country: " +
       country.countryName +
       " Pending Destination: " +
       pending
   );

        Transform countryTransform =
            GameObject.Find(country.countryName)?.transform;

        if (countryTransform == null)
            return;

        Transform card =
            countryTransform.Find("Card" + (pending + 1) + " - Button");

        if (card == null)
            return;

        Transform cityImage = card.Find("City - Image");

        if (cityImage == null)
            return;

        Image image = cityImage.GetComponent<Image>();

        if (image == null)
            return;

        // Store original state
        Vector3 originalPos = cityImage.position;
        Vector3 originalScale = cityImage.localScale;
        int originalSiblingIndex = cityImage.GetSiblingIndex();
        Color originalColor = image.color;

        Sequence seq = DOTween.Sequence();

        SoundManager.instance.PlayHaptic(
            MOST_HapticFeedback.HapticTypes.Success
        );

        // Step 1: Shake the card
        seq.Append(
            cityImage.DOShakeRotation(0.45f, 8f, 10)
        );

        // Step 2: Reveal the destination image
        seq.AppendCallback(() =>
        {
            image.sprite = country.previewCards[pending];
        });

        // Step 3: Pause
        seq.AppendInterval(0.3f);

        // Step 4: Move to center - ✅ NOW IN SEQUENCE!
        seq.Append(
            cityImage.DOMove(cardZoomTarget.position, 0.8f)
                .SetEase(Ease.OutCubic)
        );

        // Step 5: Bring card to front BEFORE zoom
        seq.AppendCallback(() =>
        {
            cityImage.SetAsLastSibling();
        });

        // Step 6: Zoom - ✅ NOW IN SEQUENCE!
        seq.Append(
            cityImage.DOScale(3f, 0.7f)
                .SetEase(Ease.OutBack)
        );

        // Step 7: Pause before fade
        seq.AppendInterval(0.4f);

        // Step 8: Fade out card and show info screen
        seq.AppendCallback(() =>
        {
            DestinationUnlocker.Clear();

            // Fade out the zoomed card
            image.DOFade(0, 0.3f);

            // Show the destination info screen
            CountryInfoScreen.instance.openedFromUnlock = true;
            CountryInfoScreen.instance.ShowDestination(
                country.destinations[pending],
                true  // Flag for unlock animation
            );

            UIManager.Instance.Show(ScreenType.CountryInfoScreen);
            MapManager.instance?.RefreshMap();
        });

        // Step 9: Reset card state after animation completes
        seq.OnComplete(() =>
        {
            Debug.Log("Animation Complete - Resetting card to original state");

            // Reset everything to original state
            cityImage.position = originalPos;
            cityImage.localScale = originalScale;
            cityImage.SetSiblingIndex(originalSiblingIndex);
            image.color = originalColor;  // Restore full opacity
            image.sprite = null;  // Clear the preview card image

            Debug.Log("Card reset complete");
        });
    }

    public void ShowLockedMessage()
    {
        lockedMessagePopup.SetActive(true);

        lockedMessagePopup.transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();

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
