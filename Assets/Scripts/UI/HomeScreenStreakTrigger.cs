using UnityEngine;

public class HomeScreenStreakTrigger : MonoBehaviour
{
    private CanvasGroup cg;
    private bool hasTriggered = false;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        hasTriggered = false; 
    }

    void Update()
    {
        if (hasTriggered) return;

        if (AutoSlider.isSliding) return;

        if (cg != null && cg.alpha < 0.5f) return;

        if (DailyStreakManager.instance != null && DailyStreakManager.instance.ShouldShowRewardPopup())
        {
            hasTriggered = true;
            
            UIManager.Instance.Show(ScreenType.DailyStreakScreen); 

            if (DailyStreakUI.instance != null)
            {
                DailyStreakUI.instance.OpenDailyReward();
            }
        }
    }
}