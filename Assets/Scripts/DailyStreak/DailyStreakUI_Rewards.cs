using DG.Tweening;
using UnityEngine;
using Solo.MOST_IN_ONE;

public partial class DailyStreakUI
{
    void ShowRewardPopup(
        string reward
    )
    {
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);
        SoundManager.instance.PlaySound(SoundName.RewardPop);

        rewardPopup.SetActive(true);

        rewardText.text = reward;

        rewardPopup.transform.DOKill();
        rewardPopup.transform.localScale =
            Vector3.one * 0.7f;

        Sequence seq = DOTween.Sequence();

        seq.Append(rewardPopup.transform.DOScale(1.08f, 0.35f)
                .SetEase(
                    Ease.OutBack
                )
        );

        seq.Append(rewardPopup.transform.DOScale(1f, 0.1f));

        seq.AppendInterval(1.3f);

        seq.Append(rewardPopup.transform.DOScale(0f, 0.25f)
                .SetEase(
                    Ease.InBack
                )
        );

        seq.OnComplete(() =>
        {
            rewardPopup.SetActive(false);

            ShowKeepGoing();
        });
    }

    public void ShowRewardPreview(int day, Transform chest)
    {
        WeeklyReward reward = DailyRewardManager.instance.GetRewardForDay(day);
        if (reward == null) return;

        Transform popup = rewardPreviewPopup.transform;

        // 1. Check if the popup is ALREADY fully visible
        bool isOpen = rewardPreviewPopup.activeSelf && popup.localScale.x > 0.1f;

        // 2. NEW: Kill the OLD sequence timer so it doesn't accidentally hide the popup early!
        DOTween.Kill("RewardPreviewSeq");
        popup.DOKill();

        rewardPreviewPopup.transform.position = chest.position + Vector3.up * 180f;

        int maxSlotsToFill = Mathf.Min(reward.rewards.Count, rewardSlots.Length);

        for (int i = 0; i < rewardSlots.Length; i++)
        {
            rewardSlots[i].root.SetActive(false);
        }

        for (int i = 0; i < maxSlotsToFill; i++)
        {
            rewardSlots[i].root.SetActive(true);
            rewardSlots[i].icon.sprite = reward.rewards[i].rewardIcon;
            rewardSlots[i].amountText.text = reward.rewards[i].amount.ToString();
        }

        rewardPreviewPopup.SetActive(true);
        SoundManager.instance.PlaySound(SoundName.RewardReveal);

        // 3. NEW: Create the sequence and give it an ID so we can kill it next time!
        Sequence seq = DOTween.Sequence().SetId("RewardPreviewSeq");

        if (isOpen)
        {
            popup.localScale = Vector3.one;
            seq.Append(popup.DOPunchScale(Vector3.one * 0.15f, 0.2f, 2, 0.5f));
        }
        else
        {
            popup.localScale = Vector3.zero;
            seq.Append(popup.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        }

        // Keep it on screen for 1.2 seconds before hiding it again
        seq.AppendInterval(1.2f);
        seq.Append(popup.DOScale(0f, 0.2f).SetEase(Ease.InBack));

        seq.OnComplete(() =>
        {
            rewardPreviewPopup.SetActive(false);
        });
    }
}