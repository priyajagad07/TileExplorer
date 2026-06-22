using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class DailyStreakUI
{
    public void PlayRewardSequence()
    {
        Debug.Log("PlayRewardSequence");
        DOTween.Kill("RewardSeq");

        int streak = DailyStreakManager.instance.GetStreak();

        if (streakText != null)
        {
            int previous = Mathf.Max(0, streak - 1);
            streakText.text = previous.ToString();
        }

        ShowPreviousProgress(streak);

        keepGoingButton.localScale = Vector3.zero;
        birdParent.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence().SetId("RewardSeq");
        seq.AppendInterval(0.5f);

        seq.AppendCallback(() =>
        {
            PlayBirdAnimation();
        });

        seq.AppendInterval(0.5f);

        seq.AppendCallback(() =>
        {
            AnimateStreak(streak);
        });

        seq.AppendInterval(1f);

        seq.AppendCallback(() =>
        {
            ClaimDay(streak);
        });

        seq.AppendInterval(0.25f);

        seq.AppendCallback(() =>
        {
            ShowRewardPopup(DailyRewardManager.instance.GetRewardTextForDay(streak));
        });

        seq.AppendInterval(0.3f);

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

    void ShowPreviousProgress(int streak)
    {
        for (int i = 0; i < dayIcons.Length; i++)
        {
            dayIcons[i].color = i < streak - 1 ? completedColor : defaultColor;
        }
    }
}