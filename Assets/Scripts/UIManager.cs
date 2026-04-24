using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<ScreenData> screens;
    public ScreenType startScreenType;

    private BaseScreen currentScreen;

    public void ShowGamePlay()
    {
        Show(ScreenType.GamePlay);
    }
    public void ShowGameOver()
    {
        Show(ScreenType.GameOver);
    }
    // public void ShowPauseGame()
    // {
    //     Show(ScreenType.PauseGame);
    // }
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