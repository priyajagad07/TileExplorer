using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class DailyStreakUI
{
    public void PlayRewardSequence()
    {
        Debug.Log("PlayRewardSequence");

        // 1. SAFETY LOCK: Kill any ghost timelines if this accidentally fires twice
        DOTween.Kill("RewardSeq");

        int streak = DailyStreakManager.instance.GetStreak();

        ShowPreviousProgress(streak);

        keepGoingButton.localScale = Vector3.zero;
        birdParent.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence().SetId("RewardSeq");
        seq.AppendInterval(0.5f);

        // 2. Pop the Bird
        seq.AppendCallback(() =>
        {
            PlayBirdAnimation();
        });

        // Wait for the Bird to finish (0.5 seconds)
        seq.AppendInterval(0.5f);

        // 3. Count the streak numbers
        seq.AppendCallback(() =>
        {
            AnimateStreak(streak);
        });

        // Wait for the numbers to finish counting and punching (1 second)
        seq.AppendInterval(1f);

        // 4. Pop the Day Icon
        seq.AppendCallback(() =>
        {
            ClaimDay(streak);
        });

        seq.AppendInterval(0.25f);

        // 5. Show what they won
        seq.AppendCallback(() =>
        {
            ShowRewardPopup(DailyRewardManager.instance.GetRewardTextForDay(streak));
        });

        seq.AppendInterval(0.3f);

        // 6. Give the actual rewards in the background
        seq.AppendCallback(() =>
        {
            DailyStreakManager.instance.GiveRewardForCurrentDay();
        });
    }

    void AnimateStreak(int targetValue)
    {
        if (streakText == null) return;

        int previous = Mathf.Max(0, targetValue - 1);

        SoundManager.instance.PlaySound(SoundName.CounterStart);

        streakText.transform.DOKill(true);
        streakText.transform.localScale = Vector3.one;

        DOTween.To(() => previous, x => { streakText.text = x.ToString(); }, targetValue, 0.5f)
        .OnComplete(() =>
        {
            SoundManager.instance.PlaySound(SoundName.NumberPop);
            streakText.transform.DOPunchScale(Vector3.one * 0.3f, 0.35f, 10, 1);
        });
    }

    void PlayBirdAnimation()
    {
        birdParent.DOKill(true);
        birdParent.localScale = Vector3.zero;

        SoundManager.instance.PlaySound(SoundName.BirdPop);
        birdParent.DOScale(1f, 0.45f).SetEase(Ease.OutBack);
    }

    void ShowKeepGoing()
    {
        Debug.Log("ShowKeepGoing");
        SoundManager.instance.PlaySound(SoundName.ButtonPop);

        keepGoingButton.DOKill(true);
        keepGoingButton.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    void ClaimDay(int streak)
    {
        if (streak <= 0) return;

        Image icon = dayIcons[streak - 1];
        icon.color = completedColor;

        icon.transform.DOKill(true);
        icon.transform.localScale = Vector3.one * 0.7f;

        Sequence seq = DOTween.Sequence();

        seq.Append(icon.transform.DOScale(1.35f, 0.2f));
        seq.Append(icon.transform.DOScale(1f, 0.15f));

        seq.Join(icon.transform.DOPunchRotation(new Vector3(0, 0, 15), 0.35f, 8, 1));
    }

    void ShowPreviousProgress(
        int streak
    )
    {
        for (
            int i = 0;
            i < dayIcons.Length;
            i++
        )
        {
            dayIcons[i].color =
                i < streak - 1
                ? completedColor
                : defaultColor;
        }
    }
}