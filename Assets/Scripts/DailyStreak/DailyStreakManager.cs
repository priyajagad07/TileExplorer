using System;
using UnityEngine;

public class DailyStreakManager : MonoBehaviour
{
    public static DailyStreakManager instance;

    private int streak;

    private const string STREAK_KEY = "DailyStreak";
    private const string LAST_DATE_KEY = "LastPlayedDate";
    private const string CLAIMED_KEY = "ClaimedToday";
    private const string WEEK_COMPLETE_KEY =
    "WeekComplete";
    private const string REWARD_PENDING_KEY =
    "DailyRewardPending";

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
        CheckMissedDay();
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

    void CheckMissedDay()
    {
        string lastDateString = PlayerPrefs.GetString(LAST_DATE_KEY, "");

        if (string.IsNullOrEmpty(lastDateString))
            return;

        DateTime lastDate = DateTime.Parse(lastDateString);
        int daysPassed = (DateTime.Today - lastDate.Date).Days;

        if (daysPassed > 1)
        {
            ResetStreak();
        }

        if (daysPassed >= 1)
        {
            PlayerPrefs.SetInt(CLAIMED_KEY, 0);
        }
    }

    public void OnLevelCompleted()
    {
        if (PlayerPrefs.GetInt(CLAIMED_KEY, 0) == 1)
            return;

        bool weekComplete = PlayerPrefs.GetInt(WEEK_COMPLETE_KEY, 0) == 1;

        if (weekComplete)
        {
            streak = 1;

            PlayerPrefs.SetInt(WEEK_COMPLETE_KEY, 0);
        }
        else
        {
            streak++;
        }

        if (streak >= 7)
        {
            streak = 7;

            PlayerPrefs.SetInt(WEEK_COMPLETE_KEY, 1);
        }

        PlayerPrefs.SetInt(STREAK_KEY, streak);
        Debug.Log("STREAK SAVED: " + streak);
        PlayerPrefs.SetString(LAST_DATE_KEY, DateTime.Today.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt(CLAIMED_KEY, 1);
        PlayerPrefs.SetInt(REWARD_PENDING_KEY, 1);
        PlayerPrefs.Save();
    }

    void ResetStreak()
    {
        streak = 0;

        PlayerPrefs.SetInt(STREAK_KEY, 0);
        PlayerPrefs.Save();
    }

    public void GiveRewardForCurrentDay()
    {
        if (!HasPendingReward())
            return;

        PlayerPrefs.SetInt(
            REWARD_PENDING_KEY,
            0
        );

        PlayerPrefs.Save();

        DailyRewardManager.instance
    .GiveRewardForDay(streak);
    }

    public void ContinueFromDailyReward()
    {
        if (DailyStreakUI.instance.openedAfterReward)
        {
            LevelManager.instance.loadLevelSilently = true;
            LevelManager.instance.NextLevel(false);
        }

        UIManager.Instance.Show(
            ScreenType.HomeScreen
        );
    }

    public bool ShouldShowRewardPopup()
    {
        if (!HasPendingReward())
            return false;

        return DailyRewardManager.instance
            .HasRewardForDay(streak);
    }

    public void ClearPendingReward()
    {
        PlayerPrefs.SetInt(REWARD_PENDING_KEY, 0);
        PlayerPrefs.Save();
    }
}