using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<ScreenData> screens;
    public ScreenType startScreenType;
    private BaseScreen currentScreen;
    private BaseScreen activePopup;
    private ScreenType currentScreenType;
    private Stack<ScreenType> screenHistory = new Stack<ScreenType>();

    [Header("Shop Scroll")]
    [SerializeField] private ScrollRect shopScrollRect;
    [SerializeField] private RectTransform coinPacksSection;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        foreach (var s in screens)
        {
            s.screen.Hide();
        }

        Show(startScreenType);
    }

    public void Show(ScreenType type, bool isBack = false)
    {
        foreach (var s in screens)
        {
            if (s.screenType == type)
            {
                if (!isBack && currentScreen != null && currentScreen != s.screen)
                {
                    screenHistory.Push(currentScreenType);
                }

                if (type == ScreenType.SettingsScreen && currentScreenType == ScreenType.GamePlay)
                {
                    Time.timeScale = 0;
                }

                if (!s.isPopup && currentScreen != null)
                {
                    currentScreen.Hide();
                }

                currentScreen = s.screen;
                currentScreenType = type;

                currentScreen.Show();
                RefreshBannerVisibility();

                if (type == ScreenType.HomeScreen)
                {
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        if (DailyStreakManager.instance.CanShowReward())
                        {
                            UIManager.Instance.Show(
                                ScreenType.DailyStreakScreen
                            );

                            DailyStreakUI.instance.OpenDailyReward();
                        }
                    });
                }

                ScrollReset reset = currentScreen.GetComponent<ScrollReset>();
                if (reset != null)
                {
                    reset.ResetToTop();
                }
                return;
            }
        }
    }

    public void ShowPopup(ScreenType type)
    {
        foreach (var s in screens)
        {
            if (s.screenType == type && s.isPopup)
            {
                if (activePopup == s.screen)
                {
                    return;
                }

                if (activePopup != null)
                {
                    activePopup.Hide();
                }

                activePopup = s.screen;
                activePopup.Show();
                RefreshBannerVisibility();

                return;
            }
        }
    }

    public void HidePopup(ScreenType type)
    {
        foreach (var s in screens)
        {
            if (s.screenType == type && s.isPopup)
            {
                s.screen.Hide();

                if (activePopup == s.screen)
                {
                    activePopup = null;
                }

                RefreshBannerVisibility();
                return;
            }
        }
    }

    public void GoBack()
    {
        if (screenHistory.Count > 0)
        {
            ScreenType previous =
                screenHistory.Pop();

            if (previous == ScreenType.GamePlay)
            {
                Time.timeScale = 1f;
            }

            Show(previous, true);
        }
    }

    public void OpenShopAtCoinPacks()
    {
        Show(ScreenType.ShopScreen);

        DOVirtual.DelayedCall(0.2f, () =>
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(shopScrollRect.content);

            float contentHeight = shopScrollRect.content.rect.height;
            float viewportHeight = shopScrollRect.viewport.rect.height;

            float hiddenHeight = contentHeight - viewportHeight;

            float targetY = Mathf.Abs(coinPacksSection.anchoredPosition.y);

            float normalized = Mathf.Clamp01(targetY / hiddenHeight);

            shopScrollRect.DOVerticalNormalizedPos(
                1f - normalized,
                0.5f
            ).SetEase(Ease.OutCubic);
        });
    }

    private bool ShouldShowBanner(ScreenType screenType)
    {
        return screenType == ScreenType.GamePlay;
    }

    private void RefreshBannerVisibility()
    {
        if (AdManager.instance == null)
            return;

        // Hide banners whenever a popup is open.
        if (activePopup != null)
        {
            AdManager.instance.HideBannerAd();
            return;
        }

        if (ShouldShowBanner(currentScreenType))
        {
            AdManager.instance.ShowBannerAd();
        }
        else
        {
            AdManager.instance.HideBannerAd();
        }
    }

    public void ReturnToGameplayFromPopup()
    {
        // Close whichever popup is currently registered.
        if (activePopup != null)
        {
            activePopup.Hide();
            activePopup = null;
        }

        Show(ScreenType.GamePlay);

        // Refresh again next frame after all popup/screen
        // activation changes have completed.
        StartCoroutine(RefreshBannerNextFrame());
    }

    private IEnumerator RefreshBannerNextFrame()
    {
        yield return null;
        RefreshBannerVisibility();
    }
}