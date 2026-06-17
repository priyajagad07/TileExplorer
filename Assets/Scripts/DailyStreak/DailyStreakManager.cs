using System;
using UnityEngine;

public class DailyStreakManager : MonoBehaviour
{
    public static DailyStreakManager instance;

    private int streak;

    private const string STREAK_KEY = "DailyStreak";
    private const string REWARD_PENDING_KEY = "DailyRewardPending";
    private const string FIRST_LAUNCH_KEY = "FirstLaunchCompleted";
    private const string LAST_LOGIN_DATE_KEY = "LastLoginDate";

    public bool HasPendingReward()
    {
        return PlayerPrefs.GetInt(REWARD_PENDING_KEY, 0) == 1;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LoadData();
        CheckDailyLogin();
    }

    void CheckDailyLogin()
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        bool firstLaunch = PlayerPrefs.GetInt(FIRST_LAUNCH_KEY, 0) == 0;

        if (firstLaunch)
        {
            Debug.Log("First Launch");
            PlayerPrefs.SetInt(FIRST_LAUNCH_KEY, 1);
            streak = 1;
            PlayerPrefs.SetInt(STREAK_KEY, streak);
            PlayerPrefs.SetInt(REWARD_PENDING_KEY, 1);
            PlayerPrefs.SetString(LAST_LOGIN_DATE_KEY, today);
            PlayerPrefs.Save();
            return;
        }

        string lastLogin = PlayerPrefs.GetString(LAST_LOGIN_DATE_KEY, "");

        if (lastLogin == today) return;

        DateTime previous = DateTime.Parse(lastLogin);
        int daysPassed = (DateTime.Today - previous.Date).Days;

        if (daysPassed > 1)
        {
            streak = 1;
            Debug.Log("Missed day. Reset streak.");
        }
        else
        {
            streak++;
        }

        if (streak > 7) streak = 1;

        PlayerPrefs.SetInt(STREAK_KEY, streak);
        PlayerPrefs.SetString(LAST_LOGIN_DATE_KEY, today);
        PlayerPrefs.SetInt(REWARD_PENDING_KEY, 1);
        PlayerPrefs.Save();

        Debug.Log("New Daily Reward Available. Streak = " + streak);
    }

    void LoadData()
    {
        streak = PlayerPrefs.GetInt(STREAK_KEY, 0);
        Debug.Log("LOADED STREAK: " + streak);
    }

    public int GetStreak()
    {
        return streak;
    }

    public void GiveRewardForCurrentDay()
    {
        if (!HasPendingReward()) return;

        PlayerPrefs.SetInt(REWARD_PENDING_KEY, 0);
        PlayerPrefs.Save();
        DailyRewardManager.instance.GiveRewardForDay(streak);
    }

    public void ContinueFromDailyReward()
    {
        UIManager.Instance.HidePopup(ScreenType.LevelCompleted);

        if (DailyStreakUI.instance.openedAfterReward)
        {
            DailyStreakUI.instance.openedAfterReward = false;
            UIManager.Instance.Show(ScreenType.GamePlay);
        }
        else
        {
            UIManager.Instance.Show(ScreenType.HomeScreen);
        }
    }

    public bool ShouldShowRewardPopup()
    {
        if (!HasPendingReward()) return false;
        return DailyRewardManager.instance.HasRewardForDay(streak);
    }

    public void ClearPendingReward()
    {
        PlayerPrefs.SetInt(REWARD_PENDING_KEY, 0);
        PlayerPrefs.Save();
    }

    public bool CanShowReward()
    {
        return HasPendingReward();
    }
}