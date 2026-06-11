using DG.Tweening;
using UnityEngine;

public partial class DailyStreakUI
{
    void ShowRewardPopup(
        string reward
    )
    {
        SoundManager.instance.PlaySound(
            SoundName.RewardPop
        );

        rewardPopup.SetActive(true);

        rewardText.text =
            reward;

        rewardPopup.transform.localScale =
            Vector3.one * 0.7f;

        Sequence seq =
            DOTween.Sequence();

        seq.Append(
            rewardPopup.transform
                .DOScale(
                    1.08f,
                    0.35f
                )
                .SetEase(
                    Ease.OutBack
                )
        );

        seq.Append(
            rewardPopup.transform
                .DOScale(
                    1f,
                    0.1f
                )
        );

        seq.AppendInterval(1.3f);

        seq.Append(
            rewardPopup.transform
                .DOScale(
                    0f,
                    0.25f
                )
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

    public void ShowRewardPreview(
        int day,
        Transform chest
    )
    {
        rewardPreviewPopup.transform.position =
            chest.position +
            Vector3.up * 180f;

        WeeklyReward reward =
            DailyRewardManager.instance
                .GetRewardForDay(day);

        if (reward == null)
            return;

        rewardPreviewPopup.SetActive(true);

        for (
            int i = 0;
            i < rewardSlots.Length;
            i++
        )
        {
            rewardSlots[i]
                .root.SetActive(false);
        }

        for (
            int i = 0;
            i < reward.rewards.Count;
            i++
        )
        {
            rewardSlots[i]
                .root.SetActive(true);

            rewardSlots[i]
                .icon.sprite =
                reward.rewards[i]
                    .rewardIcon;

            rewardSlots[i]
                .amountText.text =
                reward.rewards[i]
                    .amount.ToString();
        }

        Transform popup =
            rewardPreviewPopup.transform;

        popup.localScale =
            Vector3.zero;

        SoundManager.instance.PlaySound(
            SoundName.RewardReveal
        );

        Sequence seq =
            DOTween.Sequence();

        seq.Append(
            popup.DOScale(
                1f,
                0.3f
            )
            .SetEase(
                Ease.OutBack
            )
        );

        seq.AppendInterval(1f);

        seq.Append(
            popup.DOScale(
                0f,
                0.2f
            )
            .SetEase(
                Ease.InBack
            )
        );

        seq.OnComplete(() =>
        {
            rewardPreviewPopup
                .SetActive(false);
        });
    }
}