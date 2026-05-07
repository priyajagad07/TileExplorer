using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<ScreenData> screens;
    public ScreenType startScreenType;

    private BaseScreen currentScreen;
    private ScreenType currentScreenType;
    private Stack<ScreenType> screenHistory = new Stack<ScreenType>();

    public void ShowGameStartScreen()
    {
        Show(ScreenType.GameStartScreeen);
    }

    public void ShowHomeScreen()
    {
        Show(ScreenType.HomeScreen);
    }

    public void ShowMapScreen()
    {
        Show(ScreenType.MapScreen);
    }

    public void ShowShopScreen()
    {
        Show(ScreenType.ShopScreen);
    }

    public void ShowSettingsScreen()
    {
        Show(ScreenType.SettingsScreen);
    }

    public void ShowInfoScreen()
    {
        Show(ScreenType.InfoScreen);
    }

    public void ShowFreeCoinsScreen()
    {
        Show(ScreenType.FreeCoinsScreen);
    }

    public void ShowDailyStreakScreen()
    {
        Show(ScreenType.DailyStreakScreen);
    }

    public void ShowGamePlay()
    {
        Show(ScreenType.GamePlay);
    }

    public void ShowGameOver()
    {
        Show(ScreenType.GameOver);
    }

    public void ShowLevelCompleted()
    {
        Show(ScreenType.LevelCompleted);
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach (var s in screens)
        {
            s.screen.Hide();
        }

        Show(startScreenType);
    }

    public void Show(ScreenType type)
    {
        foreach (var s in screens)
        {
            if (s.screenType == type)
            {
                if (currentScreen != null && currentScreen != s.screen)
                {
                    screenHistory.Push(currentScreenType);
                }

                if(type == ScreenType.SettingsScreen && currentScreenType == ScreenType.GamePlay)
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
                s.screen.Show();
                return;
            }
        }
    }

    public void HidePopup(ScreenType type)
    {
        foreach (var s in screens)
        {
            if (s.screenType == type)
            {
                s.screen.Hide();
                return;
            }
        }
    }

    public void GoBack()
    {
        if (screenHistory.Count > 0)
        {
            ScreenType previous = screenHistory.Pop();

            if (previous == ScreenType.GamePlay)
            {
                Time.timeScale = 1f;
            }

            Show(previous);
        }
    }
}