using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<ScreenData> screens;
    public ScreenType startScreenType;

    private BaseScreen currentScreen;

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
                if (!s.isPopup && currentScreen != null)
                {
                    currentScreen.Hide();
                }
                currentScreen = s.screen;
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
}