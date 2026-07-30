using DG.Tweening;
using UnityEngine;
using Solo.MOST_IN_ONE;

public partial class DailyStreakUI
{
    void ShowRewardPopup(WeeklyReward reward)
    {
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);
        SoundManager.instance.PlaySound(SoundName.RewardPop);

        int day = DailyStreakManager.instance.GetStreak();

        // Hide all reward layouts
        foreach (var popup in rewardPopups)
            popup.root.SetActive(false);

        RewardPopupDayUI currentPopup = rewardPopups[day - 1];

        int count = Mathf.Min(reward.rewards.Count, currentPopup.icons.Length);

        // Fill reward data
        for (int i = 0; i < count; i++)
        {
            currentPopup.icons[i].sprite = reward.rewards[i].rewardIcon;

            currentPopup.amounts[i].text =
                reward.rewards[i].rewardType == RewardType.Coins
                ? reward.rewards[i].amount + " Coins"
                : reward.rewards[i].amount.ToString();
        }

        currentPopup.root.SetActive(true);

        // -----------------------------
        // Prepare UI
        // -----------------------------

        rewardPopup.SetActive(true);

        rewardPopup.transform.DOKill();
        bird.DOKill();
        collectButtonRect.DOKill();
        collectButton.transform.DOKill();

        rewardPopup.transform.localScale = Vector3.one * .7f;

        bird.localScale = Vector3.zero;

        Vector2 originalButtonPos = collectButtonRect.anchoredPosition;

        collectButtonRect.anchoredPosition =
            originalButtonPos + Vector2.down * 120;

        collectButtonRect.localScale = Vector3.zero;
        collectButton.interactable = false;

        // Hide reward items initially
        for (int i = 0; i < count; i++)
        {
            Transform item = currentPopup.icons[i].transform.parent;

            item.DOKill();
            item.localScale = Vector3.zero;
        }

        // -----------------------------
        // Animation
        // -----------------------------

        Sequence seq = DOTween.Sequence();

        // Popup
        seq.Append(
            rewardPopup.transform
                .DOScale(1.08f, .35f)
                .SetEase(Ease.OutBack));

        seq.Append(
            rewardPopup.transform
                .DOScale(1f, .1f));

        // Bird
        seq.Append(
            bird.DOScale(1.15f, .3f)
                .SetEase(Ease.OutBack));

        seq.Append(
            bird.DOScale(1f, .08f));



        // Rewards one by one
        for (int i = 0; i < count; i++)
        {
            Transform item = currentPopup.icons[i].transform.parent;

            seq.AppendInterval(.08f);

            seq.Append(
                item.DOScale(1.15f, .22f)
                    .SetEase(Ease.OutBack));

            seq.Join(
                item.DOPunchRotation(
                    new Vector3(0, 0, 8),
                    .22f,
                    8,
                    1));

            seq.Append(
                item.DOScale(1f, .08f));

            seq.Join(
    item.DOLocalMoveY(
        item.localPosition.y + 8f,
        .12f)
    .SetLoops(2, LoopType.Yoyo)
    .SetEase(Ease.OutQuad)
);
        }

        // Button
        seq.AppendInterval(.15f);

        seq.AppendCallback(() =>
 {
     Sequence buttonSeq = DOTween.Sequence();

     buttonSeq.Join(
         collectButtonRect
             .DOAnchorPos(originalButtonPos, .35f)
             .SetEase(Ease.OutBack)
     );

     buttonSeq.Join(
         collectButtonRect
             .DOScale(1f, .35f)
             .SetEase(Ease.OutBack)
     );

     buttonSeq.Append(
         collectButtonRect
             .DOPunchScale(Vector3.one * .1f, .2f)
     );

     collectButton.interactable = true;
 });
    }
    public void CollectReward()
    {
        collectButton.interactable = false;

        DailyStreakManager.instance.GiveRewardForCurrentDay();

        foreach (var popup in rewardPopups)
        {
            foreach (var icon in popup.icons)
            {
                icon.transform.parent.DOKill();
            }

            popup.root.SetActive(false);
        }

        bird.DOKill();
        collectButton.transform.DOKill();
        collectButtonRect.DOKill();
        rewardPopup.transform.DOKill();

        rewardPopup.transform
            .DOScale(0f, .25f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                rewardPopup.SetActive(false);
                collectButton.interactable = true;
                CloseDailyReward();
            });
    }

    void CloseDailyReward()
    {
        UIManager.Instance.Show(ScreenType.HomeScreen);
    }

    public void ShowRewardPreview(int day)
    {
        WeeklyReward reward = DailyRewardManager.instance.GetRewardForDay(day);
        if (reward == null)
            return;

        // Hide all previews
        foreach (var p in previewPopups)
            p.root.SetActive(false);

        RewardPreviewUI popup = previewPopups[day - 1];

        for (int i = 0; i < reward.rewards.Count; i++)
        {
            popup.icons[i].sprite = reward.rewards[i].rewardIcon;
            //popup.amounts[i].text = reward.rewards[i].rewardType == RewardType.Coins ? reward.rewards[i].amount + " Coins" : reward.rewards[i].amount.ToString();
            popup.amounts[i].text = reward.rewards[i].amount.ToString();
        }

        popup.root.SetActive(true);

        popup.root.transform.localScale = Vector3.zero;
        popup.root.transform
            .DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack);

        DOVirtual.DelayedCall(1.2f, () =>
        {
            popup.root.transform
                .DOScale(0f, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => popup.root.SetActive(false));
        });
    }

}