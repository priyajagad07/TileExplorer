using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Text;

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

    private bool bannerSuppressedByTutorial;
    private Coroutine tutorialBannerRefreshCoroutine;

    [Header("Screen Analytics")]
    [SerializeField]
    private bool showScreenAnalyticsLogs = true;

    private string lastLoggedAnalyticsView =
        string.Empty;

    private Coroutine pendingScreenAnalyticsCoroutine;

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
        CancelPendingScreenAnalytics();

        foreach (var s in screens)
        {
            if (s.screenType != type)
                continue;

            // Never treat a popup as the main screen.
            if (s.isPopup)
            {
                ShowPopup(type);
                return;
            }

            if (!isBack &&
                currentScreen != null &&
                currentScreen != s.screen)
            {
                screenHistory.Push(currentScreenType);
            }

            if (type == ScreenType.SettingsScreen &&
                currentScreenType == ScreenType.GamePlay)
            {
                Time.timeScale = 0;
            }

            if (currentScreen != null)
            {
                currentScreen.Hide();
            }

            currentScreen = s.screen;
            currentScreenType = type;

            currentScreen.Show();
            RefreshBannerVisibility();

            // Do not log a background screen while a popup
            // is still covering it.
            if (activePopup == null)
            {
                LogScreenView(
                    type,
                    isPopup: false
                );
            }

            if (type == ScreenType.HomeScreen)
            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    if (DailyStreakManager.instance != null &&
                        DailyStreakManager.instance.CanShowReward())
                    {
                        UIManager.Instance.Show(
                            ScreenType.DailyStreakScreen
                        );

                        if (DailyStreakUI.instance != null)
                        {
                            DailyStreakUI.instance.OpenDailyReward();
                        }
                    }
                });
            }

            ScrollReset reset =
                currentScreen.GetComponent<ScrollReset>();

            if (reset != null)
            {
                reset.ResetToTop();
            }

            return;
        }
    }

    public void ShowPopup(ScreenType type)
    {
        CancelPendingScreenAnalytics();

        foreach (var s in screens)
        {
            if (s.screenType == type &&
                s.isPopup)
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

                LogScreenView(
                    type,
                    isPopup: true
                );

                return;
            }
        }
    }

    public void HidePopup(ScreenType type)
    {
        foreach (var s in screens)
        {
            if (s.screenType == type &&
                s.isPopup)
            {
                bool wasActivePopup =
                    activePopup == s.screen;

                s.screen.Hide();

                if (wasActivePopup)
                {
                    activePopup = null;

                    ScheduleCurrentScreenAnalytics();
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

        Debug.Log(
            $"[BANNER STATE] " +
            $"Screen: {currentScreenType}, " +
            $"Popup: {(activePopup != null ? activePopup.name : "None")}, " +
            $"TutorialSuppressed: {bannerSuppressedByTutorial}"
        );

        if (activePopup != null ||
            bannerSuppressedByTutorial)
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
        CancelPendingScreenAnalytics();

        // Close whichever popup is currently registered.
        if (activePopup != null)
        {
            activePopup.Hide();
            activePopup = null;
        }

        Show(ScreenType.GamePlay);

        // Refresh again next frame after all popup/screen
        // activation changes have completed.
        StartCoroutine(
            RefreshBannerNextFrame()
        );
    }
    private IEnumerator RefreshBannerNextFrame()
    {
        yield return null;
        RefreshBannerVisibility();
    }

    public void NotifyScreenFinishedHiding(
    BaseScreen hiddenScreen)
    {
        if (hiddenScreen == null)
            return;

        if (activePopup != hiddenScreen)
            return;

        activePopup = null;

        Debug.Log(
            $"POPUP FULLY CLOSED: " +
            $"{hiddenScreen.gameObject.name}"
        );

        RefreshBannerVisibility();

        ScheduleCurrentScreenAnalytics();
    }

    public void SetTutorialBannerSuppressed(bool suppressed)
    {
        bannerSuppressedByTutorial = suppressed;

        if (tutorialBannerRefreshCoroutine != null)
        {
            StopCoroutine(tutorialBannerRefreshCoroutine);
            tutorialBannerRefreshCoroutine = null;
        }

        if (suppressed)
        {
            if (AdManager.instance != null)
            {
                AdManager.instance.HideBannerAd();
            }

            return;
        }

        tutorialBannerRefreshCoroutine =
            StartCoroutine(RefreshBannerAfterTutorial());
    }

    private IEnumerator RefreshBannerAfterTutorial()
    {
        // Temporary tutorial Canvas/GraphicRaycaster components
        // are removed at the end of the frame.
        yield return null;
        yield return null;

        tutorialBannerRefreshCoroutine = null;

        RefreshBannerVisibility();
    }

    private void LogScreenView(
    ScreenType type,
    bool isPopup)
    {
        string analyticsName =
            GetAnalyticsScreenName(
                type,
                isPopup
            );

        if (string.IsNullOrEmpty(analyticsName))
            return;

        // Prevent repeated calls for the same visible screen.
        if (lastLoggedAnalyticsView ==
            analyticsName)
        {
            return;
        }

        if (AnalyticsManager.Instance == null)
        {
            Debug.LogWarning(
                "[Screen Analytics] " +
                "AnalyticsManager is missing."
            );

            return;
        }

        lastLoggedAnalyticsView =
            analyticsName;

        AnalyticsManager.Instance.LogScreenView(
            analyticsName
        );

        if (showScreenAnalyticsLogs)
        {
            Debug.Log(
                "<color=#5DADE2>" +
                "[Screen Analytics] Viewed: " +
                analyticsName +
                "</color>"
            );
        }
    }

    private string GetAnalyticsScreenName(
        ScreenType type,
        bool isPopup)
    {
        string screenName =
            ConvertToSnakeCase(
                type.ToString()
            );

        // GamePlay becomes game_play through automatic
        // conversion. Use gameplay for a cleaner name.
        if (screenName == "game_play")
        {
            screenName = "gameplay";
        }

        if (isPopup)
        {
            // BuyUndoScreen becomes buy_undo_popup
            // instead of buy_undo_screen_popup.
            if (screenName.EndsWith("_screen"))
            {
                screenName =
                    screenName.Substring(
                        0,
                        screenName.Length -
                        "_screen".Length
                    );
            }

            if (!screenName.EndsWith("_popup"))
            {
                screenName += "_popup";
            }
        }
        else
        {
            if (screenName.EndsWith("_popup"))
            {
                screenName =
                    screenName.Substring(
                        0,
                        screenName.Length -
                        "_popup".Length
                    );
            }

            if (!screenName.EndsWith("_screen"))
            {
                screenName += "_screen";
            }
        }

        return screenName;
    }

    private string ConvertToSnakeCase(
        string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        StringBuilder result =
            new StringBuilder();

        for (int i = 0;
             i < value.Length;
             i++)
        {
            char currentCharacter =
                value[i];

            if (char.IsUpper(currentCharacter) &&
                i > 0)
            {
                char previousCharacter =
                    value[i - 1];

                bool previousIsLowerOrNumber =
                    char.IsLower(previousCharacter) ||
                    char.IsDigit(previousCharacter);

                bool nextIsLower =
                    i + 1 < value.Length &&
                    char.IsLower(value[i + 1]);

                if (previousIsLowerOrNumber ||
                    nextIsLower)
                {
                    result.Append('_');
                }
            }

            result.Append(
                char.ToLowerInvariant(
                    currentCharacter
                )
            );
        }

        return result.ToString();
    }

    private void ScheduleCurrentScreenAnalytics()
    {
        CancelPendingScreenAnalytics();

        pendingScreenAnalyticsCoroutine =
            StartCoroutine(
                LogCurrentScreenNextFrame()
            );
    }

    private IEnumerator LogCurrentScreenNextFrame()
    {
        yield return null;

        pendingScreenAnalyticsCoroutine = null;

        // Another popup or screen may have opened while
        // waiting for the popup animation to close.
        if (activePopup != null ||
            currentScreen == null)
        {
            yield break;
        }

        LogScreenView(
            currentScreenType,
            isPopup: false
        );
    }

    private void CancelPendingScreenAnalytics()
    {
        if (pendingScreenAnalyticsCoroutine ==
            null)
        {
            return;
        }

        StopCoroutine(
            pendingScreenAnalyticsCoroutine
        );

        pendingScreenAnalyticsCoroutine = null;
    }
}