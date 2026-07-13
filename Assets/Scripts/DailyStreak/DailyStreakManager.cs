using System;
using UnityEngine;

public class DailyStreakManager : MonoBehaviour
{
    public static DailyStreakManager instance;
    private int streak;

    public bool HasPendingReward()
    {
        return SaveManager.instance.data.dailyRewardPending == 1;
    }

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
    }

    void Start()
    {
        LoadData();
        CheckDailyLogin();
    }

    void CheckDailyLogin()
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        bool firstLaunch = SaveManager.instance.data.firstLaunchCompleted == 0;

        if (firstLaunch)
        {
            Debug.Log("First Launch");
            SaveManager.instance.data.firstLaunchCompleted = 1;
            streak = 1;

            SaveManager.instance.data.dailyStreak = streak;
            SaveManager.instance.data.dailyRewardPending = 1;
            SaveManager.instance.data.lastLoginDate = today;
            SaveManager.instance.SaveData();
            return;
        }

        string lastLogin = SaveManager.instance.data.lastLoginDate;
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

        SaveManager.instance.data.dailyStreak = streak;
        SaveManager.instance.data.lastLoginDate = today;
        SaveManager.instance.data.dailyRewardPending = 1;
        SaveManager.instance.SaveData();

        Debug.Log("New Daily Reward Available. Streak = " + streak);
    }

    void LoadData()
    {
        streak = SaveManager.instance.data.dailyStreak;
        Debug.Log("LOADED STREAK: " + streak);
    }

    public int GetStreak()
    {
        return streak;
    }

    public void GiveRewardForCurrentDay()
    {
        if (!HasPendingReward()) return;

        SaveManager.instance.data.dailyRewardPending = 0;
        SaveManager.instance.SaveData();
        DailyRewardManager.instance.GiveRewardForDay(streak);
    }

    public void ContinueFromDailyReward()
    {
        UIManager.Instance.HidePopup(ScreenType.LevelCompleted);
        UIManager.Instance.Show(ScreenType.HomeScreen);
    }

    public bool ShouldShowRewardPopup()
    {
        if (!HasPendingReward()) return false;
        return DailyRewardManager.instance.HasRewardForDay(streak);
    }

    public void ClearPendingReward()
    {
        SaveManager.instance.data.dailyRewardPending = 0;
        SaveManager.instance.SaveData();
    }

    public bool CanShowReward()
    {
        return HasPendingReward();
    }
}