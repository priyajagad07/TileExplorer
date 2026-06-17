using UnityEngine;
using DG.Tweening;

public class HomeScreenStreakTrigger : MonoBehaviour
{
    void OnEnable()
    {
        // Safe, stable DOTween timer that waits 0.6 seconds after the screen turns on
        DOVirtual.DelayedCall(0.6f, CheckDailyStreak).SetId("HomeStreakTimer");
    }

    void OnDisable()
    {
        // Safely kill the timer if the player leaves the home screen before 0.6 seconds!
        DOTween.Kill("HomeStreakTimer");
    }

    void CheckDailyStreak()
    {
        if (!gameObject.activeInHierarchy) return;

        if (DailyStreakManager.instance != null && DailyStreakManager.instance.ShouldShowRewardPopup())
        {
            UIManager.Instance.Show(ScreenType.DailyStreakScreen); 

            if (DailyStreakUI.instance != null)
            {
                DailyStreakUI.instance.OpenDailyReward();
            }
        }
    }
}