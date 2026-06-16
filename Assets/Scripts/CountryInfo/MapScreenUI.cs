using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Solo.MOST_IN_ONE;

public class MapScreenUI : MonoBehaviour
{
    public static MapScreenUI instance;

    [SerializeField]
    private GameObject lockedMessagePopup;
    [SerializeField]
    private Image unlockZoomImage;

    [SerializeField]
    private CanvasGroup unlockZoomGroup;

    void Awake()
    {
        instance = this;
        unlockZoomImage.gameObject.SetActive(false);
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

        CountryData country = CountryManager.Instance.GetNextCountry();
        Debug.Log("Unlocking Country: " + country.countryName + " Pending Destination: " + pending);

        Transform countryTransform = GameObject.Find(country.countryName)?.transform;

        if (countryTransform == null)
            return;

        Transform card = countryTransform.Find("Card" + (pending + 1) + " - Button");

        if (card == null)
            return;

        Transform cityImage = card.Find("City - Image");

        if (cityImage == null)
            return;

        Image image = cityImage.GetComponent<Image>();

        if (image == null)
            return;

        Sequence seq = DOTween.Sequence();

        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);

        seq.Append(cityImage.DOShakeRotation(0.45f, 8f, 10)
        );

        seq.AppendCallback(() =>
        {
            image.sprite = country.previewCards[pending];
        });

        seq.AppendInterval(0.3f);

        seq.AppendCallback(() =>
        {
            unlockZoomImage.gameObject.SetActive(true);
            unlockZoomImage.sprite = country.previewCards[pending];
            unlockZoomImage.rectTransform.localScale = Vector3.zero;
            unlockZoomGroup.alpha = 1f;
        });

        seq.Append(unlockZoomImage.rectTransform.DOScale(1f, 0.8f).SetEase(Ease.OutBack));
        seq.AppendInterval(0.3f);

        seq.AppendInterval(0.2f);
        seq.AppendCallback(() =>
        {
            DestinationUnlocker.Clear();

            MapManager.instance.RefreshMap();

            CountryInfoScreen.instance.openedFromUnlock = true;

            CountryInfoScreen.instance.ShowDestination(
                country.destinations[pending],
                true
            );

            UIManager.Instance.Show(
                ScreenType.CountryInfoScreen
            );
        });
    }
    public void HideUnlockTransition()
    {
        unlockZoomImage.gameObject.SetActive(false);

        unlockZoomGroup.alpha = 1f;

        unlockZoomImage.rectTransform.localScale =
            Vector3.one;
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
