using System;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager instance;

    [SerializeField]
    private DailyRewardDatabase database;

    private const string WEEK_KEY =
        "RewardWeek";

    private const string DAY1_KEY =
        "Day1Reward";

    private const string DAY2_KEY =
       "Day2Reward";

    private const string DAY3_KEY =
        "Day3Reward";

    private const string DAY4_KEY =
       "Day4Reward";

    private const string DAY5_KEY =
        "Day5Reward";

    private const string DAY6_KEY =
       "Day6Reward";

    private const string DAY7_KEY =
        "Day7Reward";

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        CheckWeeklyRewards();

        WeeklyReward reward = GetRewardForDay(1);

        if (reward != null)
        {
            Debug.Log(reward.rewardName);
        }
    }

    void CheckWeeklyRewards()
    {
        int currentWeek =
            GetCurrentWeekNumber();

        int savedWeek =
            PlayerPrefs.GetInt(
                WEEK_KEY,
                -1
            );

        if (currentWeek == savedWeek)
            return;

        GenerateNewWeek();

        PlayerPrefs.SetInt(
            WEEK_KEY,
            currentWeek
        );

        PlayerPrefs.Save();
    }

    int GetCurrentWeekNumber()
    {
        return
            DateTime.Now.Year * 100 +
            System.Globalization
            .CultureInfo
            .CurrentCulture
            .Calendar
            .GetWeekOfYear(
                DateTime.Now,
                System.Globalization
                .CalendarWeekRule
                .FirstDay,
                DayOfWeek.Monday
            );
    }

    void GenerateNewWeek()
    {
        SelectReward(
            database.day1Rewards.Count,
            DAY1_KEY
        );

        SelectReward(
            database.day2Rewards.Count,
            DAY2_KEY
        );

        SelectReward(
            database.day3Rewards.Count,
            DAY3_KEY
        );

        SelectReward(
            database.day4Rewards.Count,
            DAY4_KEY
        );

        SelectReward(
            database.day5Rewards.Count,
            DAY5_KEY
        );

        SelectReward(
            database.day6Rewards.Count,
            DAY6_KEY
        );

        SelectReward(
            database.day7Rewards.Count,
            DAY7_KEY
        );
    }

    void SelectReward(int rewardCount, string saveKey)
    {
        if (rewardCount == 0)
        {
            PlayerPrefs.SetInt(saveKey, -1);
            return;
        }

        int lastReward = PlayerPrefs.GetInt(saveKey, -1);
        int newReward;

        do
        {
            newReward = UnityEngine.Random.Range(0, rewardCount);
        }
        while (rewardCount > 1 && newReward == lastReward);

        PlayerPrefs.SetInt(saveKey, newReward);
    }

    public WeeklyReward GetRewardForDay(int day)
    {
        switch (day)
        {
            case 1:
                int day1Index = PlayerPrefs.GetInt(DAY1_KEY, -1);
                if (day1Index >= 0 && day1Index < database.day1Rewards.Count)
                    return database.day1Rewards[day1Index];
                break;

            case 2:
                int day2Index = PlayerPrefs.GetInt(DAY2_KEY, -1);
                if (day2Index >= 0 && day2Index < database.day2Rewards.Count)
                    return database.day2Rewards[day2Index];
                break;

            case 3:
                int day3Index = PlayerPrefs.GetInt(DAY3_KEY, -1);
                if (day3Index >= 0 && day3Index < database.day3Rewards.Count)
                    return database.day3Rewards[day3Index];
                break;

            case 4:
                int day4Index = PlayerPrefs.GetInt(DAY4_KEY, -1);
                if (day4Index >= 0 && day4Index < database.day4Rewards.Count)
                    return database.day4Rewards[day4Index];
                break;

            case 5:
                int day5Index = PlayerPrefs.GetInt(DAY5_KEY, -1);
                if (day5Index >= 0 && day5Index < database.day5Rewards.Count)
                    return database.day5Rewards[day5Index];
                break;


            case 6:
                int day6Index = PlayerPrefs.GetInt(DAY6_KEY, -1);
                if (day6Index >= 0 && day6Index < database.day6Rewards.Count)
                    return database.day6Rewards[day6Index];
                break;

            case 7:
                int day7Index = PlayerPrefs.GetInt(DAY7_KEY, -1);
                if (day7Index >= 0 && day7Index < database.day7Rewards.Count)
                    return database.day7Rewards[day7Index];
                break;
        }

        return null;
    }

    public void GiveRewardForDay(int day)
    {
        WeeklyReward reward =
            GetRewardForDay(day);

        if (reward == null)
            return;

        foreach (RewardData rewardData
            in reward.rewards)
        {
            switch (rewardData.rewardType)
            {
                case RewardType.Coins:
                    CoinManager.instance.AddCoins(rewardData.amount);
                    break;

                case RewardType.Undo:
                    BoosterManager.instance.AddUndo(rewardData.amount);
                    break;

                case RewardType.Shuffle:
                    BoosterManager.instance.AddShuffle(rewardData.amount);
                    break;

                case RewardType.Magic:
                    BoosterManager.instance.AddMagic(rewardData.amount);
                    break;
            }
        }
    }

    public string GetRewardTextForDay(int day)
    {
        WeeklyReward reward =
            GetRewardForDay(day);

        if (reward == null)
            return "Reward";

        return reward.rewardName;
    }

    public bool HasRewardForDay(int day)
    {
        return GetRewardForDay(day) != null;
    }
}