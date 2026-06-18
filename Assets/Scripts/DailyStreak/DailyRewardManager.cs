using System;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager instance;

    [SerializeField]
    private DailyRewardDatabase database;

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
        int currentWeek = GetCurrentWeekNumber();
        int savedWeek = SaveManager.instance.data.rewardWeek;

        if (currentWeek == savedWeek) return;

        GenerateNewWeek();

        SaveManager.instance.data.rewardWeek = currentWeek;
        SaveManager.instance.SaveData();
    }

    int GetCurrentWeekNumber()
    {
        return DateTime.Now.Year * 100 +
            System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                DateTime.Now,
                System.Globalization.CalendarWeekRule.FirstDay,
                DayOfWeek.Monday
            );
    }

    void GenerateNewWeek()
    {
        SelectReward(database.day1Rewards.Count, 0); // Day 1
        SelectReward(database.day2Rewards.Count, 1); // Day 2
        SelectReward(database.day3Rewards.Count, 2); // Day 3
        SelectReward(database.day4Rewards.Count, 3); // Day 4
        SelectReward(database.day5Rewards.Count, 4); // Day 5
        SelectReward(database.day6Rewards.Count, 5); // Day 6
        SelectReward(database.day7Rewards.Count, 6); // Day 7
    }

    void SelectReward(int rewardCount, int dayIndex)
    {
        if (rewardCount == 0)
        {
            SaveManager.instance.data.dayRewards[dayIndex] = -1;
            return;
        }

        int lastReward = SaveManager.instance.data.dayRewards[dayIndex];
        int newReward;

        do
        {
            newReward = UnityEngine.Random.Range(0, rewardCount);
        }
        while (rewardCount > 1 && newReward == lastReward);

        SaveManager.instance.data.dayRewards[dayIndex] = newReward;
    }

    public WeeklyReward GetRewardForDay(int day)
    {
        int index = SaveManager.instance.data.dayRewards[day - 1];
        if (index < 0) return null;

        switch (day)
        {
            case 1: if (index < database.day1Rewards.Count) return database.day1Rewards[index]; break;
            case 2: if (index < database.day2Rewards.Count) return database.day2Rewards[index]; break;
            case 3: if (index < database.day3Rewards.Count) return database.day3Rewards[index]; break;
            case 4: if (index < database.day4Rewards.Count) return database.day4Rewards[index]; break;
            case 5: if (index < database.day5Rewards.Count) return database.day5Rewards[index]; break;
            case 6: if (index < database.day6Rewards.Count) return database.day6Rewards[index]; break;
            case 7: if (index < database.day7Rewards.Count) return database.day7Rewards[index]; break;
        }

        return null;
    }

    public void GiveRewardForDay(int day)
    {
        WeeklyReward reward = GetRewardForDay(day);

        if (reward == null) return;

        foreach (RewardData rewardData in reward.rewards)
        {
            switch (rewardData.rewardType)
            {
                case RewardType.Coins: CoinManager.instance.AddCoins(rewardData.amount); break;
                case RewardType.Undo: BoosterManager.instance.AddUndo(rewardData.amount); break;
                case RewardType.Shuffle: BoosterManager.instance.AddShuffle(rewardData.amount); break;
                case RewardType.Magic: BoosterManager.instance.AddMagic(rewardData.amount); break;
            }
        }
    }

    public string GetRewardTextForDay(int day)
    {
        WeeklyReward reward = GetRewardForDay(day);
        return reward == null ? "Reward" : reward.rewardName;
    }

    public bool HasRewardForDay(int day)
    {
        return GetRewardForDay(day) != null;
    }
}