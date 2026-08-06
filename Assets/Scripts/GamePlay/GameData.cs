using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // Level & Map Progression
    public int level = 0;
    public int pendingDestination = -1;

    // Player Profile & Economy
    public int coins = 300;
    public int avatarIndex = 0;
    public string playerName = "";

    // Core States
    public int firstGameplayLaunch = 0;
    public int firstLaunchCompleted = 0;

    // Boosters Inventory & Unlocks
    public int undoCount = 0;
    public int shuffleCount = 0;
    public int magicCount = 0;
    public int undoUnlocked = 0;
    public int shuffleUnlocked = 0;
    public int magicUnlocked = 0;
    public int undoAnimPlayed = 0;
    public int shuffleAnimPlayed = 0;
    public int magicAnimPlayed = 0;

    // Daily Streaks & Rewards
    public int dailyStreak = 0;
    public string lastLoginDate = "";
    public int dailyRewardPending = 0;
    public int rewardWeek = -1;
    public int[] dayRewards = new int[7] { -1, -1, -1, -1, -1, -1, -1 };

    // Tutorials
    public int tutorialCompleted = 0;
    public List<string> softTutorialsSeen = new List<string>();

    // Audio & Settings
    public int musicMuted = 0;
    public int sfxMuted = 0;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public int vibrationMuted = 0;

    // Purchases
    public int removeAdsPurchased = 0;

    // Prevent the same pending transaction from being granted twice.
    public List<string> processedIapTransactionIds =
        new List<string>();


    // Interstitial frequency tracking
    public bool interstitialPolicyInitialized;
    public int interstitialLifetimeNextClicks;

    public string interstitialDailyDate;
    public int interstitialDailyFreeLevels;
    public int interstitialDailyNextClicks;
}