using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DailyRewardDatabase",
    menuName = "Daily Rewards/Database"
)]
public class DailyRewardDatabase : ScriptableObject
{
    public List<WeeklyReward> day1Rewards;

    public List<WeeklyReward> day3Rewards;

    public List<WeeklyReward> day5Rewards;

    public List<WeeklyReward> day7Rewards;
}