using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<ScreenData> screens;
    public ScreenType startScreenType;
    private BaseScreen currentScreen;
    private BaseScreen activePopup;
    private ScreenType currentScreenType;
    private Stack<ScreenType> screenHistory = new Stack<ScreenType>();

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