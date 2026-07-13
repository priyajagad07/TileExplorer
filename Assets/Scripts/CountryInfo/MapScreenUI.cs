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
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        unlockZoomImage.gameObject.SetActive(false);
    }

    public static class DestinationUnlocker
    {
        public static void SetPending(int index)
        {
            SaveManager.instance.data.pendingDestination = index;
            SaveManager.instance.SaveData();
        }

        public static int GetPending()
        {
            return SaveManager.instance.data.pendingDestination;
        }

        public static void Clear()
        {
            SaveManager.instance.data.pendingDestination = -1;
            SaveManager.instance.SaveData();
        }
    }

    public void PlayPendingUnlock()
    {
        int pending = DestinationUnlocker.GetPending();
        if (pending < 0) return;

        CountryData country = CountryManager.Instance.GetNextCountry();

        CountryUIPanel panel = MapManager.instance.countryPanels.Find(p => p.countryData == country);
        if (panel == null) return;

        if (pending >= panel.destinationCards.Length) return;
        DestinationCard card = panel.destinationCards[pending];

        if (card == null || card.cityImage == null) return;
        Image image = card.cityImage;

        Sequence seq = DOTween.Sequence();
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);

        seq.Append(image.transform.DOShakeRotation(0.45f, 8f, 10));

        seq.AppendCallback(() =>
        {
            if (SoundManager.instance != null) SoundManager.instance.PlaySound(SoundName.MapUnlock);
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
        seq.AppendInterval(0.5f);

        seq.AppendCallback(() =>
        {
            DestinationUnlocker.Clear();
            MapManager.instance.RefreshMap();
            CountryInfoScreen.instance.openedFromUnlock = true;
            CountryInfoScreen.instance.ShowDestination(country.destinations[pending], true);
            UIManager.Instance.Show(ScreenType.CountryInfoScreen);
        });
    }

    public void HideUnlockTransition()
    {
        unlockZoomImage.gameObject.SetActive(false);
        unlockZoomGroup.alpha = 1f;
        unlockZoomImage.rectTransform.localScale = Vector3.one;
    }

    public void ShowLockedMessage()
    {
        lockedMessagePopup.SetActive(true);
        lockedMessagePopup.transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(lockedMessagePopup.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
        seq.AppendInterval(1.2f);
        seq.Append(lockedMessagePopup.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack));

        seq.OnComplete(() =>
        {
            lockedMessagePopup.SetActive(false);
        });
    }
}