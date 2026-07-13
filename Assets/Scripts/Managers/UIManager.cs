using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<ScreenData> screens;
    public ScreenType startScreenType;
    private BaseScreen currentScreen;
    private BaseScreen activePopup;
    private ScreenType currentScreenType;
    private Stack<ScreenType> screenHistory = new Stack<ScreenType>();
    public ScrollRect scrollRect;

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

                return;
            }
        }
    }

    public void GoBack()
    {
        scrollRect.verticalNormalizedPosition = 1;

        if (screenHistory.Count > 0)
        {
            ScreenType previous = screenHistory.Pop();

            if (previous == ScreenType.GamePlay)
            {
                Time.timeScale = 1f;
            }

            Show(previous, true);
        }
    }
}