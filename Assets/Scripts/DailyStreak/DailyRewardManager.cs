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

    private const string DAY3_KEY =
        "Day3Reward";

    private const string DAY5_KEY =
        "Day5Reward";

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
            database.day3Rewards.Count,
            DAY3_KEY
        );

        SelectReward(
            database.day5Rewards.Count,
            DAY5_KEY
        );

        SelectReward(
            database.day7Rewards.Count,
            DAY7_KEY
        );
    }

    void SelectReward(
        int rewardCount,
        string saveKey
    )
    {
        int lastReward =
            PlayerPrefs.GetInt(
                saveKey,
                -1
            );

        int newReward;

        do
        {
            newReward =
                UnityEngine.Random.Range(
                    0,
                    rewardCount
                );
        }
        while (
            rewardCount > 1 &&
            newReward == lastReward
        );

        PlayerPrefs.SetInt(
            saveKey,
            newReward
        );
    }

    public WeeklyReward GetRewardForDay(
        int day
    )
    {
        switch (day)
        {
            case 1:
                return database
                    .day1Rewards[
                        PlayerPrefs.GetInt(
                            DAY1_KEY,
                            0
                        )
                    ];

            case 3:
                return database
                    .day3Rewards[
                        PlayerPrefs.GetInt(
                            DAY3_KEY,
                            0
                        )
                    ];

            case 5:
                return database
                    .day5Rewards[
                        PlayerPrefs.GetInt(
                            DAY5_KEY,
                            0
                        )
                    ];

            case 7:
                return database
                    .day7Rewards[
                        PlayerPrefs.GetInt(
                            DAY7_KEY,
                            0
                        )
                    ];
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
                    CoinManager.instance
                        .AddCoins(
                            rewardData.amount
                        );
                    break;

                case RewardType.Undo:
                    BoosterManager.instance
                        .AddUndo(
                            rewardData.amount
                        );
                    break;

                case RewardType.Shuffle:
                    BoosterManager.instance
                        .AddShuffle(
                            rewardData.amount
                        );
                    break;

                case RewardType.Magic:
                    BoosterManager.instance
                        .AddMagic(
                            rewardData.amount
                        );
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