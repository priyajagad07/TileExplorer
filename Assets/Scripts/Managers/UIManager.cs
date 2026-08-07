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

    [Header("Bottom Tab Transition")]
    [SerializeField]
    private float bottomTabTransitionDuration = 0.35f;

    [SerializeField]
    private Ease bottomTabTransitionEase =
        Ease.OutCubic;

    private bool isBottomTabTransitioning;

    private Sequence bottomTabTransitionSequence;

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
    public void Show(
    ScreenType type,
    bool isBack = false)
    {
        if (isBottomTabTransitioning)
        {
            return;
        }

        CancelPendingScreenAnalytics();

        foreach (var s in screens)
        {
            if (s.screenType != type)
            {
                continue;
            }

            if (s.isPopup)
            {
                ShowPopup(type);
                return;
            }

            if (currentScreen == s.screen)
            {
                return;
            }

            if (!isBack &&
                currentScreen != null &&
                currentScreen != s.screen)
            {
                screenHistory.Push(
                    currentScreenType
                );
            }

            if (type ==
                    ScreenType.SettingsScreen &&
                currentScreenType ==
                    ScreenType.GamePlay)
            {
                Time.timeScale = 0f;
            }

            bool shouldUseTabTransition =
                currentScreen != null &&
                IsBottomTab(currentScreenType) &&
                IsBottomTab(type);

            if (shouldUseTabTransition)
            {
                StartBottomTabTransition(
                    s.screen,
                    type
                );

                return;
            }

            if (currentScreen != null)
            {
                currentScreen.Hide();
            }

            currentScreen =
                s.screen;

            currentScreenType =
                type;

            currentScreen.Show();

            FinishOpeningScreen(type);

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
        if (screenType != ScreenType.GamePlay)
        {
            return false;
        }

        if (SaveManager.instance == null ||
            SaveManager.instance.data == null)
        {
            return false;
        }

        int currentLevel =
            SaveManager.instance.data.level + 1;

        // No banner for Levels 1 - 5.
        return currentLevel >= 6;
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

    private bool IsBottomTab(
    ScreenType type)
    {
        return
            type == ScreenType.ShopScreen ||
            type == ScreenType.DailyStreakScreen ||
            type == ScreenType.HomeScreen ||
            type == ScreenType.MapScreen ||
            type == ScreenType.LeaderBoardScreen;
    }

    private int GetBottomTabIndex(
        ScreenType type)
    {
        switch (type)
        {
            case ScreenType.ShopScreen:
                return 0;

            case ScreenType.DailyStreakScreen:
                return 1;

            case ScreenType.HomeScreen:
                return 2;

            case ScreenType.MapScreen:
                return 3;

            case ScreenType.LeaderBoardScreen:
                return 4;

            default:
                return -1;
        }
    }

    private void StartBottomTabTransition(
    BaseScreen nextScreen,
    ScreenType nextScreenType)
    {
        if (isBottomTabTransitioning)
        {
            return;
        }

        BaseScreen previousScreen =
            currentScreen;

        ScreenType previousScreenType =
            currentScreenType;

        RectTransform previousRect =
            previousScreen.TransitionRect;

        RectTransform nextRect =
            nextScreen.TransitionRect;

        if (previousRect == null ||
            nextRect == null)
        {
            Debug.LogError(
                "TransitionRoot is not assigned in BaseScreen."
            );

            return;
        }

        if (previousScreen.target ==
                previousScreen.transform ||
            nextScreen.target ==
                nextScreen.transform)
        {
            Debug.LogError(
                "Assign the TransitionRoot child as BaseScreen Target."
            );

            return;
        }

        int previousIndex =
            GetBottomTabIndex(
                previousScreenType
            );

        int nextIndex =
            GetBottomTabIndex(
                nextScreenType
            );

        if (previousIndex == -1 ||
            nextIndex == -1)
        {
            return;
        }

        isBottomTabTransitioning = true;

        int direction =
            nextIndex > previousIndex
                ? 1
                : -1;

        Canvas.ForceUpdateCanvases();

        float screenWidth =
            previousRect.rect.width;

        if (screenWidth <= 0f)
        {
            screenWidth = 1080f;
        }

        Vector2 slideOffset =
            Vector2.right *
            screenWidth *
            direction;

        bottomTabTransitionSequence?.Kill();

        previousRect.DOKill();
        nextRect.DOKill();

        previousScreen
            .PrepareForTabTransition();

        nextScreen
            .PrepareForTabTransition();

        previousRect.anchoredPosition =
            previousScreen.RestingAnchoredPosition;

        nextRect.anchoredPosition =
            nextScreen.RestingAnchoredPosition +
            slideOffset;

        bottomTabTransitionSequence =
            DOTween.Sequence();

        bottomTabTransitionSequence
            .SetUpdate(true);

        // Current screen moves out.
        bottomTabTransitionSequence.Join(
            previousRect
                .DOAnchorPos(
                    previousScreen
                        .RestingAnchoredPosition -
                    slideOffset,
                    bottomTabTransitionDuration
                )
                .SetEase(
                    bottomTabTransitionEase
                )
        );

        // New screen moves in.
        bottomTabTransitionSequence.Join(
            nextRect
                .DOAnchorPos(
                    nextScreen
                        .RestingAnchoredPosition,
                    bottomTabTransitionDuration
                )
                .SetEase(
                    bottomTabTransitionEase
                )
        );

        bottomTabTransitionSequence
            .OnComplete(() =>
            {
                previousScreen
                    .CompleteTabHide();

                nextScreen
                    .CompleteTabShow();

                currentScreen =
                    nextScreen;

                currentScreenType =
                    nextScreenType;

                isBottomTabTransitioning =
                    false;

                bottomTabTransitionSequence =
                    null;

                FinishOpeningScreen(
                    nextScreenType
                );
            });
    }

    private void FinishOpeningScreen(
    ScreenType type)
    {
        RefreshBannerVisibility();

        if (activePopup == null)
        {
            LogScreenView(
                type,
                isPopup: false
            );
        }

        if (type == ScreenType.HomeScreen)
        {
            DOVirtual.DelayedCall(
                0.5f,
                () =>
                {
                    if (
                        DailyStreakManager.instance != null &&
                        DailyStreakManager.instance
                            .CanShowReward()
                    )
                    {
                        UIManager.Instance.Show(
                            ScreenType.DailyStreakScreen
                        );

                        if (DailyStreakUI.instance != null)
                        {
                            DailyStreakUI.instance
                                .OpenDailyReward();
                        }
                    }
                }
            );
        }

        ScrollReset reset =
            currentScreen
                .GetComponent<ScrollReset>();

        if (reset != null)
        {
            reset.ResetToTop();
        }
    }
}