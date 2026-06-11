using UnityEngine;

public class RewardPreviewButton : MonoBehaviour
{
    [SerializeField] private int day;

    public void ShowPreview()
    {
        DailyStreakUI.instance.ShowRewardPreview(
            day,
            transform
        );
    }
}